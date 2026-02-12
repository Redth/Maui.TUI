#nullable enable
using Microsoft.Maui.Platform;
using XenoAtom.Terminal.UI;
using XenoAtom.Terminal.UI.Controls;

namespace Maui.TUI.Handlers;

/// <summary>
/// TimePicker → TextBox with formatted time display.
/// </summary>
public partial class TimePickerHandler : TuiViewHandler<ITimePicker, TextBox>
{
	bool _updating;

	public static IPropertyMapper<ITimePicker, TimePickerHandler> Mapper =
		new PropertyMapper<ITimePicker, TimePickerHandler>(ViewMapper)
		{
			[nameof(ITimePicker.Time)] = MapTime,
			[nameof(ITimePicker.Format)] = MapTime,
		};

	public static CommandMapper<ITimePicker, TimePickerHandler> CommandMapper = new(ViewCommandMapper);

	public TimePickerHandler() : base(Mapper, CommandMapper) { }
	public TimePickerHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
		: base(mapper ?? Mapper, commandMapper ?? CommandMapper) { }

	protected override TextBox CreatePlatformView()
	{
		return new TextBox();
	}

	protected override void ConnectHandler(TextBox platformView)
	{
		base.ConnectHandler(platformView);
		platformView.KeyDownRouted += OnKeyDown;
	}

	protected override void DisconnectHandler(TextBox platformView)
	{
		platformView.KeyDownRouted -= OnKeyDown;
		base.DisconnectHandler(platformView);
	}

	void OnKeyDown(object? sender, EventArgs e)
	{
		if (_updating || VirtualView is null || PlatformView is null)
			return;

		PlatformView.App?.Post(() =>
		{
			if (TimeSpan.TryParse(PlatformView.Text, out var time))
			{
				_updating = true;
				VirtualView.Time = time;
				_updating = false;
			}
		});
	}

	public static void MapTime(TimePickerHandler handler, ITimePicker timePicker)
	{
		if (handler._updating || handler.PlatformView is null)
			return;

		var format = timePicker.Format ?? "t";
		handler._updating = true;
		var time = timePicker.Time ?? TimeSpan.Zero;
		var dt = DateTime.Today.Add(time);
		handler.PlatformView.Text = dt.ToString(format, System.Globalization.CultureInfo.CurrentCulture);
		handler._updating = false;
	}
}
