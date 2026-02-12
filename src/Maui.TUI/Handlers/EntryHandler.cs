#nullable enable
using XenoAtom.Terminal.UI.Controls;

namespace Maui.TUI.Handlers;

public partial class EntryHandler : TuiViewHandler<IEntry, TextBox>
{
	public static IPropertyMapper<IEntry, EntryHandler> Mapper =
		new PropertyMapper<IEntry, EntryHandler>(ViewMapper)
		{
			[nameof(ITextInput.Text)] = MapText,
			[nameof(ITextInput.IsReadOnly)] = MapIsReadOnly,
			[nameof(IEntry.IsPassword)] = MapIsPassword,
		};

	public static CommandMapper<IEntry, EntryHandler> CommandMapper = new(ViewCommandMapper);

	public EntryHandler() : base(Mapper, CommandMapper) { }
	public EntryHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
		: base(mapper ?? Mapper, commandMapper ?? CommandMapper) { }

	protected override TextBox CreatePlatformView() => new TextBox();

	protected override void ConnectHandler(TextBox platformView)
	{
		base.ConnectHandler(platformView);
		// TODO: subscribe to text changed when TUI TextBox exposes a change event
	}

	protected override void DisconnectHandler(TextBox platformView)
	{
		base.DisconnectHandler(platformView);
	}

	public static void MapText(EntryHandler handler, IEntry entry) =>
		handler.PlatformView.Text = entry.Text ?? string.Empty;

	public static void MapIsReadOnly(EntryHandler handler, IEntry entry) =>
		handler.PlatformView.IsEnabled = !entry.IsReadOnly;

	public static void MapIsPassword(EntryHandler handler, IEntry entry) =>
		handler.PlatformView.IsPassword = entry.IsPassword;
}
