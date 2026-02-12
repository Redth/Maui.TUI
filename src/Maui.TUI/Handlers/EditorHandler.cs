#nullable enable
using XenoAtom.Terminal.UI.Controls;

namespace Maui.TUI.Handlers;

public partial class EditorHandler : TuiViewHandler<IEditor, TextArea>
{
	public static IPropertyMapper<IEditor, EditorHandler> Mapper =
		new PropertyMapper<IEditor, EditorHandler>(ViewMapper)
		{
			[nameof(ITextInput.Text)] = MapText,
			[nameof(ITextInput.IsReadOnly)] = MapIsReadOnly,
		};

	public static CommandMapper<IEditor, EditorHandler> CommandMapper = new(ViewCommandMapper);

	public EditorHandler() : base(Mapper, CommandMapper) { }
	public EditorHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
		: base(mapper ?? Mapper, commandMapper ?? CommandMapper) { }

	protected override TextArea CreatePlatformView() => new TextArea();

	public static void MapText(EditorHandler handler, IEditor editor) =>
		handler.PlatformView.Text = editor.Text ?? string.Empty;

	public static void MapIsReadOnly(EditorHandler handler, IEditor editor) =>
		handler.PlatformView.IsEnabled = !editor.IsReadOnly;
}
