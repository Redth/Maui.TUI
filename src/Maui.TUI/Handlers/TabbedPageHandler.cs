#nullable enable
using Microsoft.Maui.Controls;
using Microsoft.Maui.Platform;
using XenoAtom.Terminal.UI;
using XenoAtom.Terminal.UI.Controls;

namespace Maui.TUI.Handlers;

public partial class TabbedPageHandler : TuiViewHandler<TabbedPage, TabControl>
{
	public static IPropertyMapper<TabbedPage, TabbedPageHandler> Mapper =
		new PropertyMapper<TabbedPage, TabbedPageHandler>(ViewMapper);

	public static CommandMapper<TabbedPage, TabbedPageHandler> CommandMapper = new(ViewCommandMapper);

	public TabbedPageHandler() : base(Mapper, CommandMapper) { }
	public TabbedPageHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
		: base(mapper ?? Mapper, commandMapper ?? CommandMapper) { }

	protected override TabControl CreatePlatformView() => new TabControl();

	public override void SetVirtualView(IView view)
	{
		base.SetVirtualView(view);
		BuildTabs();
	}

	protected override void ConnectHandler(TabControl platformView)
	{
		base.ConnectHandler(platformView);

		if (VirtualView is TabbedPage tabbedPage)
			tabbedPage.PagesChanged += OnPagesChanged;
	}

	protected override void DisconnectHandler(TabControl platformView)
	{
		if (VirtualView is TabbedPage tabbedPage)
			tabbedPage.PagesChanged -= OnPagesChanged;

		base.DisconnectHandler(platformView);
	}

	void OnPagesChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
	{
		BuildTabs();
	}

	void BuildTabs()
	{
		if (PlatformView is null || VirtualView is not TabbedPage tabbedPage || MauiContext is null)
			return;

		// TabControl doesn't have a Clear/Remove, so we rebuild by creating a new one
		// For now, only build tabs on initial load
		foreach (var page in tabbedPage.Children)
		{
			var header = new TextBlock(page.Title ?? "Tab");
			var content = ((IView)page).ToPlatform(MauiContext);

			if (content is Visual visual)
				PlatformView.AddTab(header, visual);
		}
	}
}
