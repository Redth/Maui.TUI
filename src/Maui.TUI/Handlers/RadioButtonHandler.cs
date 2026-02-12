#nullable enable
using TuiRadioButton = XenoAtom.Terminal.UI.Controls.RadioButton;
using TuiTextBlock = XenoAtom.Terminal.UI.Controls.TextBlock;

namespace Maui.TUI.Handlers;

public partial class RadioButtonHandler : TuiViewHandler<IRadioButton, TuiRadioButton>
{
	public static IPropertyMapper<IRadioButton, RadioButtonHandler> Mapper =
		new PropertyMapper<IRadioButton, RadioButtonHandler>(ViewMapper)
		{
			[nameof(IRadioButton.IsChecked)] = MapIsChecked,
		};

	public static CommandMapper<IRadioButton, RadioButtonHandler> CommandMapper = new(ViewCommandMapper);

	public RadioButtonHandler() : base(Mapper, CommandMapper) { }
	public RadioButtonHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
		: base(mapper ?? Mapper, commandMapper ?? CommandMapper) { }

	protected override TuiRadioButton CreatePlatformView() => new TuiRadioButton();

	protected override void ConnectHandler(TuiRadioButton platformView)
	{
		base.ConnectHandler(platformView);
		platformView.KeyDownRouted += OnToggled;
		platformView.PointerPressedRouted += OnToggled;
	}

	protected override void DisconnectHandler(TuiRadioButton platformView)
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

	public static void MapIsChecked(RadioButtonHandler handler, IRadioButton radioButton) =>
		handler.PlatformView.IsChecked = radioButton.IsChecked;
}
