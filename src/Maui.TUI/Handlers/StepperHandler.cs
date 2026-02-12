#nullable enable
using XenoAtom.Terminal.UI;
using XenoAtom.Terminal.UI.Controls;
using TuiButton = XenoAtom.Terminal.UI.Controls.Button;

namespace Maui.TUI.Handlers;

/// <summary>
/// Stepper → HStack with [-] value [+] buttons.
/// </summary>
public partial class StepperHandler : TuiViewHandler<IStepper, HStack>
{
	TextBlock? _valueLabel;
	bool _updating;

	public static IPropertyMapper<IStepper, StepperHandler> Mapper =
		new PropertyMapper<IStepper, StepperHandler>(ViewMapper)
		{
			[nameof(IStepper.Value)] = MapValue,
			[nameof(IStepper.Minimum)] = MapValue,
			[nameof(IStepper.Maximum)] = MapValue,
			[nameof(IStepper.Interval)] = MapValue,
		};

	public static CommandMapper<IStepper, StepperHandler> CommandMapper = new(ViewCommandMapper);

	public StepperHandler() : base(Mapper, CommandMapper) { }
	public StepperHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
		: base(mapper ?? Mapper, commandMapper ?? CommandMapper) { }

	protected override HStack CreatePlatformView()
	{
		_valueLabel = new TextBlock("0");

		var minusBtn = new TuiButton("-");
		minusBtn.ClickRouted += (s, e) => Step(-1);

		var plusBtn = new TuiButton("+");
		plusBtn.ClickRouted += (s, e) => Step(1);

		return new HStack(minusBtn, _valueLabel, plusBtn) { Spacing = 1 };
	}

	void Step(int direction)
	{
		if (_updating || VirtualView is null)
			return;

		var interval = VirtualView.Interval > 0 ? VirtualView.Interval : 1;
		var newValue = VirtualView.Value + (direction * interval);
		newValue = Math.Clamp(newValue, VirtualView.Minimum, VirtualView.Maximum);

		_updating = true;
		VirtualView.Value = newValue;
		_updating = false;

		UpdateLabel();
	}

	void UpdateLabel()
	{
		if (_valueLabel is not null && VirtualView is not null)
			_valueLabel.Text = VirtualView.Value.ToString("G");
	}

	public static void MapValue(StepperHandler handler, IStepper stepper)
	{
		if (handler._updating)
			return;
		handler.UpdateLabel();
	}
}
