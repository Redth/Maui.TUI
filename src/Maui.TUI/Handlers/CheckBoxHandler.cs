#nullable enable
using TuiCheckBox = XenoAtom.Terminal.UI.Controls.CheckBox;

namespace Maui.TUI.Handlers;

public partial class CheckBoxHandler : TuiViewHandler<ICheckBox, TuiCheckBox>
{
	public static IPropertyMapper<ICheckBox, CheckBoxHandler> Mapper =
		new PropertyMapper<ICheckBox, CheckBoxHandler>(ViewMapper)
		{
			[nameof(ICheckBox.IsChecked)] = MapIsChecked,
		};

	public static CommandMapper<ICheckBox, CheckBoxHandler> CommandMapper = new(ViewCommandMapper);

	public CheckBoxHandler() : base(Mapper, CommandMapper) { }
	public CheckBoxHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
		: base(mapper ?? Mapper, commandMapper ?? CommandMapper) { }

	protected override TuiCheckBox CreatePlatformView() => new TuiCheckBox();

	protected override void ConnectHandler(TuiCheckBox platformView)
	{
		base.ConnectHandler(platformView);
		platformView.KeyDownRouted += OnToggled;
		platformView.PointerPressedRouted += OnToggled;
	}

	protected override void DisconnectHandler(TuiCheckBox platformView)
	{
		platformView.KeyDownRouted -= OnToggled;
		platformView.PointerPressedRouted -= OnToggled;
		base.DisconnectHandler(platformView);
	}

	void OnToggled(object? sender, EventArgs e)
	{
		if (VirtualView is not null && PlatformView is not null)
			VirtualView.IsChecked = PlatformView.IsChecked;
	}

	public static void MapIsChecked(CheckBoxHandler handler, ICheckBox checkBox) =>
		handler.PlatformView.IsChecked = checkBox.IsChecked;
}
