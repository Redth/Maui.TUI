#nullable enable
using Maui.TUI.Platform;
using Microsoft.Maui.Platform;
using XenoAtom.Terminal.UI;
using XenoAtom.Terminal.UI.Controls;

namespace Maui.TUI.Handlers;

/// <summary>
/// NavigationPage handler that uses a simple content-swap approach.
/// Pushes replace the visible content; pops restore the previous page.
/// </summary>
public partial class NavigationPageHandler : TuiViewHandler<IStackNavigationView, TuiNavigationContainer>
{
	public static IPropertyMapper<IStackNavigationView, NavigationPageHandler> Mapper =
		new PropertyMapper<IStackNavigationView, NavigationPageHandler>(ViewMapper);

	public static CommandMapper<IStackNavigationView, NavigationPageHandler> CommandMapper =
		new(ViewCommandMapper)
		{
			[nameof(IStackNavigation.RequestNavigation)] = MapRequestNavigation,
		};

	public NavigationPageHandler() : base(Mapper, CommandMapper) { }
	public NavigationPageHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
		: base(mapper ?? Mapper, commandMapper ?? CommandMapper) { }

	protected override TuiNavigationContainer CreatePlatformView() => new TuiNavigationContainer();

	static void MapRequestNavigation(NavigationPageHandler handler, IStackNavigationView view, object? args)
	{
		if (args is not NavigationRequest request)
			return;

		handler.HandleNavigation(request);
	}

	void HandleNavigation(NavigationRequest request)
	{
		if (MauiContext is null || VirtualView is null)
			return;

		PlatformView.Children.Clear();

		// Show the top of the navigation stack
		var newStack = request.NavigationStack;
		if (newStack.Count > 0)
		{
			var topPage = newStack[newStack.Count - 1];
			var platformView = topPage.ToPlatform(MauiContext);
			if (platformView is Visual visual)
				PlatformView.Children.Add(visual);
		}

		// Tell MAUI navigation is complete
		VirtualView.NavigationFinished(newStack);
	}
}
