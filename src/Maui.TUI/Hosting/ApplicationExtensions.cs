using Maui.TUI.Platform;
using Microsoft.Maui.Handlers;
using XenoAtom.Terminal.UI;

namespace Maui.TUI.Hosting;

public static class ApplicationExtensions
{
	public static void CreatePlatformWindow(MauiTuiApplication tuiApp, IApplication application)
	{
		if (application.Handler?.MauiContext is not IMauiContext applicationContext)
			return;

		var windowRoot = new TuiWindowRootContainer();
		var mauiContext = applicationContext.MakeWindowScope(windowRoot, out _);

		var activationState = new ActivationState(mauiContext);
		var window = application.CreateWindow(activationState);

		var windowHandler = new Maui.TUI.Handlers.WindowHandler();
		windowHandler.SetMauiContext(mauiContext);
		windowHandler.SetVirtualView(window);

		// Get the platform view created by the handler and pass it to the TUI app
		if (windowHandler.PlatformView is TuiWindowRootContainer container)
			tuiApp.SetWindowRoot(container);
	}

	internal static void SetApplicationHandler(this MauiTuiApplication tuiApp, IApplication application, IMauiContext applicationContext)
	{
		var appHandler = new Maui.TUI.Handlers.ApplicationHandler();
		appHandler.SetMauiContext(applicationContext);
		appHandler.SetVirtualView(application);
	}
}
