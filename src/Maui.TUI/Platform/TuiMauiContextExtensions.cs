using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Dispatching;

namespace Maui.TUI.Platform;

internal static class TuiMauiContextExtensions
{
	public static IMauiContext MakeApplicationScope(this IMauiContext mauiContext, object platformApplication)
	{
		var scopedContext = new TuiMauiContext(mauiContext.Services);
		scopedContext.AddSpecific(platformApplication);
		scopedContext.InitializeScopedServices();
		return scopedContext;
	}

	public static IMauiContext MakeWindowScope(this IMauiContext mauiContext, TuiWindowRootContainer platformWindow, out IServiceScope scope)
	{
		scope = mauiContext.Services.CreateScope();
		var scopedContext = new TuiMauiContext(scope.ServiceProvider);
		scopedContext.AddWeakSpecific(platformWindow);
		return scopedContext;
	}

	public static void InitializeScopedServices(this IMauiContext scopedContext)
	{
		var scopedServices = scopedContext.Services.GetServices<IMauiInitializeScopedService>();
		foreach (var service in scopedServices)
			service.Initialize(scopedContext.Services);
	}

	public static IDispatcher GetDispatcher(this IMauiContext mauiContext) =>
		mauiContext.Services.GetRequiredService<IDispatcher>();
}
