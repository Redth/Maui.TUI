#nullable enable
using XenoAtom.Terminal.UI.Input;
using TuiSlider = XenoAtom.Terminal.UI.Controls.Slider<double>;

namespace Maui.TUI.Handlers;

public partial class SliderHandler : TuiViewHandler<ISlider, TuiSlider>
{
	public static IPropertyMapper<ISlider, SliderHandler> Mapper =
		new PropertyMapper<ISlider, SliderHandler>(ViewMapper)
		{
			[nameof(IRange.Minimum)] = MapMinimum,
			[nameof(IRange.Maximum)] = MapMaximum,
			[nameof(IRange.Value)] = MapValue,
		};

	public static CommandMapper<ISlider, SliderHandler> CommandMapper = new(ViewCommandMapper);

	public SliderHandler() : base(Mapper, CommandMapper) { }
	public SliderHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
		: base(mapper ?? Mapper, commandMapper ?? CommandMapper) { }

	protected override TuiSlider CreatePlatformView() => new TuiSlider();

	protected override void ConnectHandler(TuiSlider platformView)
	{
		base.ConnectHandler(platformView);
		platformView.ValueChangedRouted += OnValueChanged;
	}

	protected override void DisconnectHandler(TuiSlider platformView)
	{
		platformView.ValueChangedRouted -= OnValueChanged;
		base.DisconnectHandler(platformView);
	}

	void OnValueChanged(object? sender, ValueChangedEventArgs<double> e)
	{
		if (VirtualView is not null)
			VirtualView.Value = PlatformView.Value;
	}

	public static void MapMinimum(SliderHandler handler, ISlider slider) =>
		handler.PlatformView.Minimum = slider.Minimum;

	public static void MapMaximum(SliderHandler handler, ISlider slider) =>
		handler.PlatformView.Maximum = slider.Maximum;

	public static void MapValue(SliderHandler handler, ISlider slider) =>
		handler.PlatformView.Value = slider.Value;
}
