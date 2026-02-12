#nullable enable
using XenoAtom.Terminal.UI.Input;
using TuiSwitch = XenoAtom.Terminal.UI.Controls.Switch;

namespace Maui.TUI.Handlers;

public partial class SwitchHandler : TuiViewHandler<ISwitch, TuiSwitch>
{
	public static IPropertyMapper<ISwitch, SwitchHandler> Mapper =
		new PropertyMapper<ISwitch, SwitchHandler>(ViewMapper)
		{
			[nameof(ISwitch.IsOn)] = MapIsOn,
		};

	public static CommandMapper<ISwitch, SwitchHandler> CommandMapper = new(ViewCommandMapper);

	public SwitchHandler() : base(Mapper, CommandMapper) { }
	public SwitchHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
		: base(mapper ?? Mapper, commandMapper ?? CommandMapper) { }

	protected override TuiSwitch CreatePlatformView() => new TuiSwitch();

	protected override void ConnectHandler(TuiSwitch platformView)
	{
		base.ConnectHandler(platformView);
		platformView.ToggledRouted += OnToggled;
	}

	protected override void DisconnectHandler(TuiSwitch platformView)
	{
		platformView.ToggledRouted -= OnToggled;
		base.DisconnectHandler(platformView);
	}

	void OnToggled(object? sender, ToggleChangedEventArgs e)
	{
		if (VirtualView is not null)
			VirtualView.IsOn = PlatformView.IsOn;
	}

	public static void MapIsOn(SwitchHandler handler, ISwitch @switch) =>
		handler.PlatformView.IsOn = @switch.IsOn;
}
