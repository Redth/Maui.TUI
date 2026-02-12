using Maui.TUI.Hosting;
using Maui.TUI.Platform;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
using XenoAtom.Terminal.UI;
using XenoAtom.Terminal.UI.Controls;
using XenoAtom.Terminal.UI.Layout;
using XenoAtom.Terminal.UI.Rendering;

namespace Maui.TUI;

/// <summary>
/// Base class for .NET MAUI TUI applications. 
/// Subclass and implement CreateMauiApp() to configure handlers and services.
/// Call Run() to start the fullscreen terminal application.
/// </summary>
public abstract class MauiTuiApplication : IPlatformApplication
{
	TerminalApp? _terminalApp;
	Panel _rootPanel = new VStack();
	TuiWindowRootContainer? _windowRoot;

	protected abstract MauiApp CreateMauiApp();

	public IServiceProvider Services { get; protected set; } = null!;
	public IApplication Application { get; protected set; } = null!;

	internal void SetWindowRoot(TuiWindowRootContainer windowRoot)
	{
		_windowRoot = windowRoot;
		_rootPanel.Children.Add(windowRoot);
	}

	/// <summary>
	/// Initializes the MAUI app and builds the visual tree without running the terminal loop.
	/// Useful for testing and diagnostics.
	/// </summary>
	public Panel Initialize()
	{
		TuiDispatcher.SetUIThread();
		IPlatformApplication.Current = this;

		var mauiApp = CreateMauiApp();
		var rootContext = new TuiMauiContext(mauiApp.Services);
		var applicationContext = rootContext.MakeApplicationScope(this);

		Services = applicationContext.Services;
		Application = Services.GetRequiredService<IApplication>();

		this.SetApplicationHandler(Application, applicationContext);

		// Create the platform window (triggers MAUI to create Window + Page + content)
		ApplicationExtensions.CreatePlatformWindow(this, Application);

		_rootPanel.VerticalAlignment = Align.Stretch;
		_rootPanel.HorizontalAlignment = Align.Stretch;

		return _rootPanel;
	}

	public void Run()
	{
		var rootPanel = Initialize();

		// Ensure Ctrl+C is passed as input rather than handled as SIGINT
		var terminal = XenoAtom.Terminal.Terminal.Instance;
		terminal.Options.TreatControlCAsInput = true;

		// Create the terminal app with the fully-built visual tree
		_terminalApp = new TerminalApp(rootPanel, terminal,
			new TerminalAppOptions
			{
				HostKind = XenoAtom.Terminal.UI.Hosting.TerminalHostKind.Fullscreen,
				// Use Ctrl+Q as exit gesture (Ctrl+C is unreliable across terminals)
				ExitGesture = new XenoAtom.Terminal.UI.Input.KeyGesture(
					'q', XenoAtom.Terminal.TerminalModifiers.Ctrl),
			});

		// Also try to handle Ctrl+C via Console.CancelKeyPress as fallback
		Console.CancelKeyPress += (s, e) =>
		{
			e.Cancel = true;
			_terminalApp?.Stop();
		};

		// Wire up dispatcher to use TerminalApp.Post() for background thread dispatch
		TuiDispatcher.SetTerminalApp(_terminalApp);

		// Run the terminal app loop (blocks until exit)
		_terminalApp.Run();
	}

	/// <summary>
	/// Renders the current visual tree to SVG without starting the terminal loop.
	/// Useful for testing and diagnostics.
	/// </summary>
	public string RenderSvg(int width = 80, int height = 24)
	{
		var rootPanel = Initialize();
		return TerminalAppSnapshotRenderer.RenderSvg(rootPanel, width: width, height: height);
	}

	/// <summary>
	/// Dumps the visual tree to the console for debugging.
	/// </summary>
	public static void DumpVisualTree(Visual visual, int depth = 0)
	{
		var indent = new string(' ', depth * 2);
		var name = visual.GetType().Name;
		var extra = visual is TextBlock tb ? $" Text=\"{tb.Text}\"" : "";
		Console.Error.WriteLine($"{indent}{name}{extra}");

		if (visual is Panel panel)
		{
			foreach (var child in panel.Children)
				DumpVisualTree(child, depth + 1);
		}
		else if (visual is ContentVisual cv && cv.Content is not null)
		{
			DumpVisualTree(cv.Content, depth + 1);
		}
		else if (visual is ScrollViewer sv && sv.Content is not null)
		{
			DumpVisualTree(sv.Content, depth + 1);
		}
		else if (visual is TabControl tc)
		{
			foreach (var tab in tc.Tabs)
			{
				Console.Error.WriteLine($"{indent}  TabPage");
				if (tab.Header is not null)
					DumpVisualTree(tab.Header, depth + 2);
				if (tab.Content is not null)
					DumpVisualTree(tab.Content, depth + 2);
			}
		}
	}
}
