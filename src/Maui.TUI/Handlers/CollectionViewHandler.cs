#nullable enable
using System.Collections;
using System.Collections.Specialized;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Platform;
using XenoAtom.Terminal.UI;
using XenoAtom.Terminal.UI.Controls;
using XenoAtom.Terminal.UI.Templating;

namespace Maui.TUI.Handlers;

/// <summary>
/// Wrapper that tracks whether a ListBox item is a group header vs a data item.
/// </summary>
public sealed class CollectionViewItem
{
	public object? Value { get; }
	public bool IsGroupHeader { get; }
	public CollectionViewItem(object? value, bool isGroupHeader = false)
	{
		Value = value;
		IsGroupHeader = isGroupHeader;
	}
	public override string ToString() => Value?.ToString() ?? string.Empty;
}

public partial class CollectionViewHandler : TuiViewHandler<CollectionView, ListBox<CollectionViewItem>>
{
	INotifyCollectionChanged? _observableSource;
	bool _updatingSelection;
	readonly HashSet<object> _selectedItems = new();

	public static IPropertyMapper<CollectionView, CollectionViewHandler> Mapper =
		new PropertyMapper<CollectionView, CollectionViewHandler>(ViewMapper)
		{
			[nameof(ItemsView.ItemsSource)] = MapItemsSource,
			[nameof(ItemsView.ItemTemplate)] = MapItemTemplate,
			[nameof(SelectableItemsView.SelectedItem)] = MapSelectedItem,
			[nameof(SelectableItemsView.SelectedItems)] = MapSelectedItems,
			[nameof(SelectableItemsView.SelectionMode)] = MapSelectionMode,
			[nameof(GroupableItemsView.IsGrouped)] = MapIsGrouped,
			[nameof(GroupableItemsView.GroupHeaderTemplate)] = MapGroupHeaderTemplate,
			[nameof(GroupableItemsView.GroupFooterTemplate)] = MapGroupFooterTemplate,
		};

	public static CommandMapper<CollectionView, CollectionViewHandler> CommandMapper = new(ViewCommandMapper);

	public CollectionViewHandler() : base(Mapper, CommandMapper) { }
	public CollectionViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
		: base(mapper ?? Mapper, commandMapper ?? CommandMapper) { }

	bool IsMultiSelect => VirtualView?.SelectionMode == SelectionMode.Multiple;

	protected override ListBox<CollectionViewItem> CreatePlatformView()
	{
		return new ListBox<CollectionViewItem>
		{
			VerticalAlignment = Align.Stretch,
			ItemTemplate = CreateDefaultTemplate(),
		};
	}

	protected override void ConnectHandler(ListBox<CollectionViewItem> platformView)
	{
		base.ConnectHandler(platformView);
		platformView.KeyDownRouted += OnKeyDown;
		platformView.PointerPressedRouted += OnPointerPressed;
	}

	protected override void DisconnectHandler(ListBox<CollectionViewItem> platformView)
	{
		platformView.KeyDownRouted -= OnKeyDown;
		platformView.PointerPressedRouted -= OnPointerPressed;
		UnsubscribeCollection();
		base.DisconnectHandler(platformView);
	}

	void OnKeyDown(object? sender, EventArgs e)
	{
		if (IsMultiSelect && e is XenoAtom.Terminal.UI.Input.KeyEventArgs keyArgs)
		{
			// Space toggles selection in multi-select mode
			if (keyArgs.Key == XenoAtom.Terminal.TerminalKey.Space)
			{
				ToggleCurrentItem();
				return;
			}
		}
		SyncSingleSelectionToMaui();
	}

	void OnPointerPressed(object? sender, EventArgs e)
	{
		if (IsMultiSelect)
		{
			// Small delay to let ListBox update SelectedIndex first
			PlatformView?.App?.Post(() => ToggleCurrentItem());
			return;
		}
		SyncSingleSelectionToMaui();
	}

	void ToggleCurrentItem()
	{
		if (_updatingSelection || VirtualView is null || PlatformView is null)
			return;

		var selectedIndex = PlatformView.SelectedIndex;
		if (selectedIndex < 0 || selectedIndex >= PlatformView.Items.Count)
			return;

		var wrapper = PlatformView.Items[selectedIndex];
		if (wrapper.IsGroupHeader || wrapper.Value is null)
			return;

		// Toggle in our tracked set
		if (!_selectedItems.Remove(wrapper.Value))
			_selectedItems.Add(wrapper.Value);

		SyncMultiSelectionToMaui();
		// Force template re-render by reassigning the template
		RefreshItemDisplay();
	}

	void SyncSingleSelectionToMaui()
	{
		if (_updatingSelection || VirtualView is null || PlatformView is null)
			return;

		if (VirtualView.SelectionMode == SelectionMode.None)
			return;

		var selectedIndex = PlatformView.SelectedIndex;
		if (selectedIndex < 0 || selectedIndex >= PlatformView.Items.Count)
		{
			if (VirtualView.SelectedItem is not null)
			{
				_updatingSelection = true;
				VirtualView.SelectedItem = null;
				_updatingSelection = false;
			}
			return;
		}

		var wrapper = PlatformView.Items[selectedIndex];
		if (wrapper.IsGroupHeader)
			return;

		var selectedItem = wrapper.Value;
		if (VirtualView.SelectedItem != selectedItem)
		{
			_updatingSelection = true;
			VirtualView.SelectedItem = selectedItem;
			_updatingSelection = false;
		}
	}

	void SyncMultiSelectionToMaui()
	{
		if (_updatingSelection || VirtualView is null)
			return;

		_updatingSelection = true;
		try
		{
			VirtualView.SelectedItems.Clear();
			foreach (var item in _selectedItems)
				VirtualView.SelectedItems.Add(item);
		}
		finally
		{
			_updatingSelection = false;
		}
	}

	void RefreshItemDisplay()
	{
		// Force ListBox to re-render by reloading items
		if (PlatformView is null || VirtualView is null)
			return;

		var currentIndex = PlatformView.SelectedIndex;
		var items = PlatformView.Items.ToList();
		PlatformView.Items.Clear();
		foreach (var item in items)
			PlatformView.Items.Add(item);
		if (currentIndex >= 0 && currentIndex < PlatformView.Items.Count)
			PlatformView.SelectedIndex = currentIndex;
	}

	void UnsubscribeCollection()
	{
		if (_observableSource is not null)
		{
			_observableSource.CollectionChanged -= OnCollectionChanged;
			_observableSource = null;
		}
	}

	void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) =>
		ReloadItems();

	void ReloadItems()
	{
		if (PlatformView is null || VirtualView is null)
			return;

		PlatformView.Items.Clear();
		_selectedItems.Clear();

		if (VirtualView.ItemsSource is not IEnumerable items)
			return;

		if (VirtualView.IsGrouped)
		{
			foreach (var group in items)
			{
				// Add group header
				PlatformView.Items.Add(new CollectionViewItem(group, isGroupHeader: true));

				// Add group items
				if (group is IEnumerable groupItems)
				{
					foreach (var item in groupItems)
						PlatformView.Items.Add(new CollectionViewItem(item));
				}
			}
		}
		else
		{
			foreach (var item in items)
				PlatformView.Items.Add(new CollectionViewItem(item));
		}
	}

	DataTemplate<CollectionViewItem> CreateDefaultTemplate()
	{
		return new DataTemplate<CollectionViewItem>
		{
			Display = (DataTemplateValue<CollectionViewItem> value, in DataTemplateContext context) =>
			{
				var wrapper = value.GetValue();
				if (wrapper is null)
					return new TextBlock(string.Empty);

				if (wrapper.IsGroupHeader)
					return new TextBlock($"── {wrapper.Value} ──");

				var text = wrapper.Value?.ToString() ?? string.Empty;
				if (IsMultiSelect)
				{
					var check = wrapper.Value is not null && _selectedItems.Contains(wrapper.Value) ? "☑" : "☐";
					text = $"{check} {text}";
				}
				return new TextBlock(text);
			}
		};
	}

	DataTemplate<CollectionViewItem> BuildItemTemplate()
	{
		var mauiItemTemplate = VirtualView?.ItemTemplate;
		var mauiHeaderTemplate = VirtualView?.GroupHeaderTemplate;

		return new DataTemplate<CollectionViewItem>
		{
			Display = (DataTemplateValue<CollectionViewItem> value, in DataTemplateContext context) =>
			{
				var wrapper = value.GetValue();
				if (wrapper is null)
					return new TextBlock(string.Empty);

				if (wrapper.IsGroupHeader)
					return RenderGroupHeader(wrapper.Value, mauiHeaderTemplate);

				var itemVisual = RenderItem(wrapper.Value, mauiItemTemplate);

				// Prepend checkbox in multi-select mode
				if (IsMultiSelect)
				{
					var check = wrapper.Value is not null && _selectedItems.Contains(wrapper.Value) ? "☑" : "☐";
					return new HStack(new TextBlock($"{check} "), itemVisual);
				}

				return itemVisual;
			}
		};
	}

	Visual RenderGroupHeader(object? group, Microsoft.Maui.Controls.DataTemplate? headerTemplate)
	{
		if (headerTemplate is not null && group is not null)
		{
			var content = headerTemplate.CreateContent();
			if (content is BindableObject bindable)
				bindable.BindingContext = group;

			// Try to extract text from the created content
			if (content is Microsoft.Maui.Controls.Label label)
				return new TextBlock($"── {label.Text} ──");
			if (content is Microsoft.Maui.Controls.View view)
			{
				try
				{
					var platformView = view.ToPlatform(MauiContext!);
					if (platformView is Visual visual)
						return visual;
				}
				catch { }
			}
		}

		// Default group header rendering
		var text = group?.ToString() ?? "Group";
		return new TextBlock($"── {text} ──");
	}

	Visual RenderItem(object? item, Microsoft.Maui.Controls.DataTemplate? itemTemplate)
	{
		if (itemTemplate is not null && item is not null)
		{
			var content = itemTemplate.CreateContent();
			if (content is BindableObject bindable)
				bindable.BindingContext = item;

			// If the template produces a MAUI View, convert it to platform
			if (content is Microsoft.Maui.Controls.View view)
			{
				try
				{
					var platformView = view.ToPlatform(MauiContext!);
					if (platformView is Visual visual)
						return visual;
				}
				catch { }
			}

			// If it's a Label, extract text
			if (content is Microsoft.Maui.Controls.Label label)
				return new TextBlock(label.Text ?? item.ToString() ?? string.Empty);
		}

		return new TextBlock(item?.ToString() ?? string.Empty);
	}

	void RebuildTemplate()
	{
		if (PlatformView is null)
			return;

		PlatformView.ItemTemplate = BuildItemTemplate();
		ReloadItems();
	}

	public static void MapItemsSource(CollectionViewHandler handler, CollectionView view)
	{
		handler.UnsubscribeCollection();
		handler.ReloadItems();

		if (view.ItemsSource is INotifyCollectionChanged observable)
		{
			handler._observableSource = observable;
			observable.CollectionChanged += handler.OnCollectionChanged;
		}
	}

	public static void MapItemTemplate(CollectionViewHandler handler, CollectionView view)
	{
		handler.RebuildTemplate();
	}

	public static void MapSelectedItem(CollectionViewHandler handler, CollectionView view)
	{
		if (handler._updatingSelection || handler.PlatformView is null)
			return;

		if (view.SelectedItem is null)
		{
			handler.PlatformView.SelectedIndex = -1;
			return;
		}

		// Find the item in the flat list
		for (int i = 0; i < handler.PlatformView.Items.Count; i++)
		{
			var wrapper = handler.PlatformView.Items[i];
			if (!wrapper.IsGroupHeader && wrapper.Value == view.SelectedItem)
			{
				handler.PlatformView.SelectedIndex = i;
				return;
			}
		}
	}

	public static void MapIsGrouped(CollectionViewHandler handler, CollectionView view)
	{
		handler.RebuildTemplate();
	}

	public static void MapGroupHeaderTemplate(CollectionViewHandler handler, CollectionView view)
	{
		handler.RebuildTemplate();
	}

	public static void MapGroupFooterTemplate(CollectionViewHandler handler, CollectionView view)
	{
		handler.RebuildTemplate();
	}

	public static void MapSelectedItems(CollectionViewHandler handler, CollectionView view)
	{
		if (handler._updatingSelection || handler.PlatformView is null)
			return;

		handler._selectedItems.Clear();
		if (view.SelectedItems is not null)
		{
			foreach (var item in view.SelectedItems)
			{
				if (item is not null)
					handler._selectedItems.Add(item);
			}
		}
		handler.RefreshItemDisplay();
	}

	public static void MapSelectionMode(CollectionViewHandler handler, CollectionView view)
	{
		// Clear multi-select state when switching modes
		handler._selectedItems.Clear();
		handler.RebuildTemplate();
	}
}
