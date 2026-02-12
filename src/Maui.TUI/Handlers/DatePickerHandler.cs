#nullable enable
using Microsoft.Maui.Platform;
using XenoAtom.Terminal.UI;
using XenoAtom.Terminal.UI.Controls;

namespace Maui.TUI.Handlers;

/// <summary>
/// DatePicker → TextBox with formatted date display.
/// Clicking opens inline editing; Tab/Enter confirms.
/// </summary>
public partial class DatePickerHandler : TuiViewHandler<IDatePicker, TextBox>
{
	bool _updating;

	public static IPropertyMapper<IDatePicker, DatePickerHandler> Mapper =
		new PropertyMapper<IDatePicker, DatePickerHandler>(ViewMapper)
		{
			[nameof(IDatePicker.Date)] = MapDate,
			[nameof(IDatePicker.Format)] = MapDate,
			[nameof(IDatePicker.MinimumDate)] = MapDate,
			[nameof(IDatePicker.MaximumDate)] = MapDate,
		};

	public static CommandMapper<IDatePicker, DatePickerHandler> CommandMapper = new(ViewCommandMapper);

	public DatePickerHandler() : base(Mapper, CommandMapper) { }
	public DatePickerHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
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

		// Post to process after the key has been handled
		PlatformView.App?.Post(() =>
		{
			if (DateTime.TryParse(PlatformView.Text, out var date))
			{
				var min = VirtualView.MinimumDate ?? DateTime.MinValue;
				var max = VirtualView.MaximumDate ?? DateTime.MaxValue;
				if (date < min) date = min;
				if (date > max) date = max;

				_updating = true;
				VirtualView.Date = date;
				_updating = false;
			}
		});
	}

	public static void MapDate(DatePickerHandler handler, IDatePicker datePicker)
	{
		if (handler._updating || handler.PlatformView is null)
			return;

		var format = datePicker.Format ?? "d";
		handler._updating = true;
		var date = datePicker.Date ?? DateTime.Today;
		handler.PlatformView.Text = date.ToString(format, System.Globalization.CultureInfo.CurrentCulture);
		handler._updating = false;
	}
}
