#nullable enable
using XenoAtom.Terminal.UI.Controls;

namespace Maui.TUI.Handlers;

public partial class LabelHandler : TuiViewHandler<ILabel, TextBlock>
{
	public static IPropertyMapper<ILabel, LabelHandler> Mapper =
		new PropertyMapper<ILabel, LabelHandler>(ViewMapper)
		{
			[nameof(ILabel.Text)] = MapText,
		};

	public static CommandMapper<ILabel, LabelHandler> CommandMapper =
		new(ViewCommandMapper);

	public LabelHandler() : base(Mapper)
	{
	}

	public LabelHandler(IPropertyMapper? mapper)
		: base(mapper ?? Mapper, CommandMapper)
	{
	}

	public LabelHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
		: base(mapper ?? Mapper, commandMapper ?? CommandMapper)
	{
	}

	protected override TextBlock CreatePlatformView() => new TextBlock();

	public static void MapText(LabelHandler handler, ILabel label) =>
		handler.PlatformView.Text = label.Text ?? string.Empty;
}
