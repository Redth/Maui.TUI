#nullable enable
using Maui.TUI.Platform;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Platform;
using XenoAtom.Terminal.UI;
using XenoAtom.Terminal.UI.Controls;
using XenoAtom.Terminal.UI.Layout;
using TuiButton = XenoAtom.Terminal.UI.Controls.Button;

namespace Maui.TUI.Handlers;

/// <summary>
/// Page handler that wraps content in a DockLayout with an optional toolbar at the top.
/// The toolbar renders ToolbarItems as buttons in an HStack.
/// </summary>
public partial class PageHandler : TuiViewHandler<IContentView, DockLayout>
{
	TuiContentPanel? _contentPanel;
	HStack? _toolbarPanel;

	public static IPropertyMapper<IContentView, PageHandler> Mapper =
		new PropertyMapper<IContentView, PageHandler>(ViewMapper)
		{
			[nameof(IContentView.Content)] = MapContent,
			[nameof(ITitledElement.Title)] = MapTitle,
		};

	public static CommandMapper<IContentView, PageHandler> CommandMapper =
		new(ViewCommandMapper);

	public PageHandler() : base(Mapper, CommandMapper) { }
	public PageHandler(IPropertyMapper? mapper) : base(mapper ?? Mapper, CommandMapper) { }
	public PageHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
		: base(mapper ?? Mapper, commandMapper ?? CommandMapper) { }

	protected override DockLayout CreatePlatformView()
	{
		_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} must be set.");

		_contentPanel = new TuiContentPanel
		{
			CrossPlatformMeasure = VirtualView.CrossPlatformMeasure,
			CrossPlatformArrange = VirtualView.CrossPlatformArrange
		};

		_toolbarPanel = new HStack { Spacing = 1, IsVisible = false };

		return new DockLayout
		{
			Top = _toolbarPanel,
			Content = _contentPanel,
		};
	}

	public override void SetVirtualView(IView view)
	{
		base.SetVirtualView(view);

		if (_contentPanel is not null && VirtualView is not null)
		{
			_contentPanel.CrossPlatformMeasure = VirtualView.CrossPlatformMeasure;
			_contentPanel.CrossPlatformArrange = VirtualView.CrossPlatformArrange;
		}
	}

	protected override void ConnectHandler(DockLayout platformView)
	{
		base.ConnectHandler(platformView);

		if (VirtualView is ContentPage page)
			UpdateToolbar(page);
	}

	protected override void DisconnectHandler(DockLayout platformView)
	{
		base.DisconnectHandler(platformView);
	}

	void UpdateToolbar(ContentPage page)
	{
		if (_toolbarPanel is null)
			return;

		_toolbarPanel.Children.Clear();

		foreach (var item in page.ToolbarItems)
		{
			var btn = new TuiButton(item.Text ?? string.Empty);
			var captured = item;
			btn.ClickRouted += (s, e) =>
			{
				if (captured.Command?.CanExecute(captured.CommandParameter) == true)
					captured.Command.Execute(captured.CommandParameter);
				else
					((IMenuItemController)captured).Activate();
			};
			_toolbarPanel.Children.Add(btn);
		}

		_toolbarPanel.IsVisible = _toolbarPanel.Children.Count > 0;
	}

	public static void MapContent(PageHandler handler, IContentView page)
	{
		if (handler._contentPanel is null || handler.MauiContext is null || handler.VirtualView is null)
			return;

		handler._contentPanel.Children.Clear();

		if (handler.VirtualView.PresentedContent is IView view)
		{
			var platformView = view.ToPlatform(handler.MauiContext);
			if (platformView is Visual visual)
				handler._contentPanel.Children.Add(visual);
		}

		// Rebuild toolbar when content is mapped (toolbar items may have changed)
		if (handler.VirtualView is ContentPage cp)
			handler.UpdateToolbar(cp);
	}

	public static void MapTitle(PageHandler handler, IContentView page)
	{
		// Title handled by NavigationPage/TabbedPage
	}
}
