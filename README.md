# Maui.TUI

A terminal UI backend for [.NET MAUI](https://github.com/dotnet/maui). Write your app with the familiar MAUI API — `ContentPage`, `Button`, `Label`, `Grid`, etc. — and render it in your terminal.

Built on [XenoAtom.Terminal.UI](https://xenoatom.github.io/terminal/) for high-performance terminal rendering.

https://github.com/user-attachments/assets/44014b7f-0c05-4907-893a-5ae9bf7dc151

## Features

- **Full MAUI handler pipeline** — uses the standard `ViewHandler<TVirtualView, TPlatformView>` architecture, no fork required
- **25+ control handlers** — Label, Button, Entry, Editor, CheckBox, Switch, Slider, ProgressBar, Picker, DatePicker, TimePicker, Stepper, RadioButton, CollectionView, ActivityIndicator, ScrollView, Border, Frame, and more
- **Layout support** — VerticalStackLayout, HorizontalStackLayout, Grid, AbsoluteLayout, FlexLayout via MAUI's cross-platform layout engine
- **Navigation** — NavigationPage (push/pop), TabbedPage, FlyoutPage, modal pages
- **Alerts & dialogs** — `DisplayAlert`, `DisplayActionSheet`, `DisplayPromptAsync` rendered as TUI modal dialogs
- **SVG rendering** — render your UI to SVG for testing and documentation (`--svg`)
- **Visual tree dump** — inspect the rendered control tree for debugging (`--dump`)

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (preview)
- MAUI workload: `dotnet workload install maui`

## Quick Start

### 1. Create a new console project

```bash
dotnet new console -n MyTuiApp
cd MyTuiApp
```

### 2. Add the project reference (or NuGet package when available)

```xml
<ProjectReference Include="path/to/src/Maui.TUI/Maui.TUI.csproj" />
```

### 3. Set up your MAUI app

```csharp
// MauiProgram.cs
using Maui.TUI.Hosting;
using Microsoft.Maui.Hosting;

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
```

```csharp
// App.cs
using Microsoft.Maui.Controls;

class App : Application
{
    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new ContentPage
        {
            Title = "Hello TUI",
            Content = new VerticalStackLayout
            {
                Children =
                {
                    new Label { Text = "Hello, .NET MAUI TUI!" },
                    new Button { Text = "Click me" },
                }
            }
        });
    }
}
```

### 4. Create your application entry point and run

```csharp
// Program.cs
using Maui.TUI;

public class MyApp : MauiTuiApplication
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}

var app = new MyApp();
app.Run();
```

Press **Ctrl+Q** to exit the terminal app.

## Running the Sample

```bash
./run-sample.sh
```

Or directly:

```bash
cd samples/Maui.TUI.Sample
dotnet run
```

The sample app demonstrates tabbed pages with controls, collection views, navigation, modals, and a form — all running in the terminal.

### Diagnostics modes

```bash
# Dump the visual tree to stderr
dotnet run -- --dump

# Render the UI to SVG
dotnet run -- --svg
```

## Project Structure

```
src/Maui.TUI/              # Core library
  Handlers/                 # MAUI ViewHandler implementations (one per control)
  Hosting/                  # AppHostBuilder extensions (UseMauiAppTUI)
  Platform/                 # TUI platform infrastructure
    MauiTuiApplication.cs   #   Application lifecycle & terminal loop
    TuiMauiContext.cs        #   IMauiContext implementation
    TuiDispatcherProvider.cs #   Dispatcher for terminal thread
    TuiAlertManager.cs       #   Alert/dialog/prompt handling

samples/Maui.TUI.Sample/   # Sample app with multiple demo pages
docs/                       # Design documents & proposals
```

## How It Works

Maui.TUI implements the MAUI handler architecture for terminal rendering:

1. **Handlers** map each MAUI virtual view (`ILabel`, `IButton`, etc.) to a [XenoAtom.Terminal.UI](https://xenoatom.github.io/terminal/) visual (e.g. `TextBlock`, `Button`)
2. **Layout** delegates to MAUI's `CrossPlatformMeasure`/`CrossPlatformArrange` — the cross-platform layout engine works out of the box
3. **Bootstrap** wires up the MAUI DI pipeline, handler factory, and dispatcher, then runs the `TerminalApp` loop
4. **Navigation** manages a stack of page containers, swapping content for push/pop operations
5. **Alerts** intercept MAUI's internal `AlertManager` via DI and render modal `Dialog` controls

## License

[MIT](LICENSE) — Copyright (c) 2026 Jonathan Dick
