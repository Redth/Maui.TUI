#nullable enable
using XenoAtom.Terminal.UI.Controls;
using XenoAtom.Terminal.UI.Input;
using XenoAtom.Terminal.UI.Templating;

namespace Maui.TUI.Handlers;

public partial class PickerHandler : TuiViewHandler<IPicker, Select<string>>
{
	public static IPropertyMapper<IPicker, PickerHandler> Mapper =
		new PropertyMapper<IPicker, PickerHandler>(ViewMapper)
		{
			[nameof(IPicker.Items)] = MapItems,
			[nameof(IPicker.SelectedIndex)] = MapSelectedIndex,
			[nameof(IPicker.Title)] = MapTitle,
		};

	public static CommandMapper<IPicker, PickerHandler> CommandMapper = new(ViewCommandMapper);

	public PickerHandler() : base(Mapper, CommandMapper) { }
	public PickerHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
		: base(mapper ?? Mapper, commandMapper ?? CommandMapper) { }

	protected override Select<string> CreatePlatformView()
	{
		var select = new Select<string>
		{
			ItemTemplate = new DataTemplate<string>
			{
				Display = (DataTemplateValue<string> value, in DataTemplateContext context) =>
					new TextBlock(value.GetValue())
			}
		};
		return select;
	}

	protected override void ConnectHandler(Select<string> platformView)
	{
		base.ConnectHandler(platformView);
		platformView.SelectionChangedRouted += OnSelectionChanged;
	}

	protected override void DisconnectHandler(Select<string> platformView)
	{
		platformView.SelectionChangedRouted -= OnSelectionChanged;
		base.DisconnectHandler(platformView);
	}

	void OnSelectionChanged(object? sender, SelectSelectionChangedEventArgs e)
	{
		if (VirtualView is not null)
			VirtualView.SelectedIndex = PlatformView.SelectedIndex;
	}

	public static void MapItems(PickerHandler handler, IPicker picker)
	{
		handler.PlatformView.Items.Clear();
		if (picker.Items is not null)
		{
			foreach (var item in picker.Items)
				handler.PlatformView.Items.Add(item);
		}
	}

	public static void MapSelectedIndex(PickerHandler handler, IPicker picker)
	{
		// Select<T> doesn't support -1; its default is 0
		if (picker.SelectedIndex >= 0)
			handler.PlatformView.SelectedIndex = picker.SelectedIndex;
	}

	public static void MapTitle(PickerHandler handler, IPicker picker)
	{
		// Title could be shown as placeholder; not directly supported in Select<T>
	}
}
