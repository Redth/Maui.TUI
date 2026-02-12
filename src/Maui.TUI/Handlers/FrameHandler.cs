#nullable enable
using Maui.TUI.Platform;
using XenoAtom.Terminal.UI;
using Microsoft.Maui.Platform;
using TuiBorder = XenoAtom.Terminal.UI.Controls.Border;

namespace Maui.TUI.Handlers;

public partial class FrameHandler : TuiViewHandler<IContentView, TuiBorder>
{
	public static IPropertyMapper<IContentView, FrameHandler> Mapper =
		new PropertyMapper<IContentView, FrameHandler>(ViewMapper)
		{
			[nameof(IContentView.Content)] = MapContent,
		};

	public static CommandMapper<IContentView, FrameHandler> CommandMapper = new(ViewCommandMapper);

	public FrameHandler() : base(Mapper, CommandMapper) { }
	public FrameHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
		: base(mapper ?? Mapper, commandMapper ?? CommandMapper) { }

	protected override TuiBorder CreatePlatformView() => new TuiBorder();

	public static void MapContent(FrameHandler handler, IContentView frame)
	{
		_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set.");

		if (frame.PresentedContent is IView view)
		{
			var platformView = view.ToPlatform(handler.MauiContext);
			if (platformView is Visual visual)
			{
				var panel = new TuiContentPanel();
				panel.CrossPlatformMeasure = frame.CrossPlatformMeasure;
				panel.CrossPlatformArrange = frame.CrossPlatformArrange;
				panel.Children.Add(visual);
				handler.PlatformView.Content = panel;
			}
		}
		else
		{
			handler.PlatformView.Content = null;
		}
	}
}
