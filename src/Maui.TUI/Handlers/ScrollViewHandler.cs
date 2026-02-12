#nullable enable
using Maui.TUI.Platform;
using XenoAtom.Terminal.UI;
using XenoAtom.Terminal.UI.Controls;
using Microsoft.Maui.Platform;

namespace Maui.TUI.Handlers;

public partial class ScrollViewHandler : TuiViewHandler<IScrollView, ScrollViewer>
{
	TuiContentPanel? _contentPanel;

	public static IPropertyMapper<IScrollView, ScrollViewHandler> Mapper =
		new PropertyMapper<IScrollView, ScrollViewHandler>(ViewMapper)
		{
			[nameof(IContentView.Content)] = MapContent,
		};

	public static CommandMapper<IScrollView, ScrollViewHandler> CommandMapper = new(ViewCommandMapper);

	public ScrollViewHandler() : base(Mapper, CommandMapper) { }
	public ScrollViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
		: base(mapper ?? Mapper, commandMapper ?? CommandMapper) { }

	protected override ScrollViewer CreatePlatformView()
	{
		var scrollViewer = new ScrollViewer();
		_contentPanel = new TuiContentPanel();
		scrollViewer.Content = _contentPanel;
		return scrollViewer;
	}

	public override void SetVirtualView(IView view)
	{
		base.SetVirtualView(view);

		_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set.");

		if (_contentPanel is not null)
		{
			_contentPanel.CrossPlatformMeasure = VirtualView.CrossPlatformMeasure;
			_contentPanel.CrossPlatformArrange = VirtualView.CrossPlatformArrange;
		}

		UpdateContent();
	}

	void UpdateContent()
	{
		if (_contentPanel is null || MauiContext is null || VirtualView is null)
			return;

		_contentPanel.Children.Clear();

		if (VirtualView.PresentedContent is IView view)
		{
			var platformView = view.ToPlatform(MauiContext);
			if (platformView is Visual visual)
				_contentPanel.Children.Add(visual);
		}
	}

	public static void MapContent(ScrollViewHandler handler, IScrollView scrollView)
	{
		handler.UpdateContent();
	}
}
