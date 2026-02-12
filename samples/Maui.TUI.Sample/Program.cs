using Maui.TUI;
using Maui.TUI.Sample;

var app = new MauiTuiSampleApp();

if (args.Contains("--dump"))
{
	var rootPanel = app.Initialize();
	MauiTuiApplication.DumpVisualTree(rootPanel);
}
else if (args.Contains("--svg"))
{
	var svg = app.RenderSvg(80, 24);
	Console.WriteLine(svg);
}
else
{
	app.Run();
}
