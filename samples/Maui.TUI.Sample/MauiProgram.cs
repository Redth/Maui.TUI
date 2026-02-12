using Maui.TUI;
using Maui.TUI.Hosting;
using Microsoft.Maui.Hosting;

namespace Maui.TUI.Sample;

public class MauiTuiSampleApp : MauiTuiApplication
{
	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp
			.CreateBuilder()
			.UseMauiAppTUI<App>();

		return builder.Build();
	}
}
