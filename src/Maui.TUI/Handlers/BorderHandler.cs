#nullable enable
using XenoAtom.Terminal.UI;
using Microsoft.Maui.Platform;
using TuiBorder = XenoAtom.Terminal.UI.Controls.Border;

namespace Maui.TUI.Handlers;

public partial class BorderHandler : TuiViewHandler<IBorderView, TuiBorder>
{
	public static IPropertyMapper<IBorderView, BorderHandler> Mapper =
		new PropertyMapper<IBorderView, BorderHandler>(ViewMapper)
		{
			[nameof(IContentView.Content)] = MapContent,
		};

	public static CommandMapper<IBorderView, BorderHandler> CommandMapper = new(ViewCommandMapper);

	public BorderHandler() : base(Mapper, CommandMapper) { }
	public BorderHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
		: base(mapper ?? Mapper, commandMapper ?? CommandMapper) { }

	protected override TuiBorder CreatePlatformView() => new TuiBorder();

	public static void MapContent(BorderHandler handler, IBorderView border)
	{
		_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set.");

		if (border.PresentedContent is IView view)
		{
			var platformView = view.ToPlatform(handler.MauiContext);
			if (platformView is Visual visual)
				handler.PlatformView.Content = visual;
		}
		else
		{
			handler.PlatformView.Content = null;
		}
	}
}
