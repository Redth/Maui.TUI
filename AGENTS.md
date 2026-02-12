# AGENTS.md

Guidance for AI agents working on this repository.

## Project Overview

Maui.TUI is a terminal UI backend for .NET MAUI. It uses the standard MAUI handler architecture (`ViewHandler<TVirtualView, TPlatformView>`) to render MAUI controls as terminal UI elements via the [XenoAtom.Terminal.UI](https://xenoatom.github.io/terminal/) library.

## Build & Run

```bash
# Restore and build everything
dotnet build Maui.TUI.slnx

# Run the sample app
cd samples/Maui.TUI.Sample && dotnet run

# Run with diagnostics
cd samples/Maui.TUI.Sample && dotnet run -- --dump   # visual tree dump
cd samples/Maui.TUI.Sample && dotnet run -- --svg    # SVG render
```

- Target framework: `net10.0`
- Requires the MAUI workload: `dotnet workload install maui`
- MAUI version is pinned via `$(MauiVersion)` in each `.csproj` (currently `10.0.31`)

## Repository Layout

```
src/Maui.TUI/                  # Core library (Maui.TUI.csproj)
  Handlers/                    # One handler per MAUI control type
  Hosting/                     # MauiAppBuilder extensions, bootstrap
  Platform/                    # Application lifecycle, context, dispatcher, alerts

samples/Maui.TUI.Sample/      # Sample app exercising all supported controls
docs/                          # Design proposals for MAUI extensibility
```

## Architecture & Conventions

### Handler Pattern

Each handler lives in `src/Maui.TUI/Handlers/` and follows this pattern:

- Inherit from `TuiViewHandler<TVirtualView, TPlatformView>` (defined in `TuiViewHandler.cs`)
- `TVirtualView` is the MAUI interface (e.g. `ILabel`, `IButton`)
- `TPlatformView` is the XenoAtom.Terminal.UI type (e.g. `TextBlock`, `Button`)
- Map MAUI properties to terminal control properties via a `PropertyMapper`
- Register the handler in `AppHostBuilderExtensions.AddMauiControlsHandlers()`

### Platform Infrastructure

- `MauiTuiApplication` — abstract base class for TUI apps; subclass and implement `CreateMauiApp()`
- `TuiMauiContext` — implements `IMauiContext` for the TUI platform
- `TuiDispatcherProvider` — dispatches work on the terminal UI thread
- `TuiAlertManager` — hooks into MAUI's internal `AlertManager` via reflection/`DispatchProxy` to handle `DisplayAlert`, `DisplayActionSheet`, and `DisplayPromptAsync`

### Key Design Decisions

- **No MAUI fork** — everything works against the public (and some internal) MAUI APIs
- **Reflection for alerts** — MAUI's `IAlertManagerSubscription` is internal, so we use `DispatchProxy` to register a DI-based implementation (see `TuiAlertManager.cs` and the proposal in `docs/maui-extensibility-proposal.md`)
- **Cell-based layout** — terminal coordinates are integer cells, not floating-point pixels; `TuiViewHandler` converts between the two in `PlatformArrange` and `GetDesiredSize`
- **XenoAtom.Terminal.UI visuals** — all platform views are XenoAtom `Visual` subclasses (`TextBlock`, `Panel`, `Button`, etc.)

## Code Style

- C# with `LangVersion` preview, nullable enabled, implicit usings enabled (set in `Directory.Build.props`)
- Tabs for indentation
- Namespace matches folder structure (`Maui.TUI.Handlers`, `Maui.TUI.Hosting`, `Maui.TUI.Platform`)
- Minimal XML doc comments — add them on public APIs and non-obvious internal methods
- No `#region` blocks

## Adding a New Handler

1. Create `src/Maui.TUI/Handlers/{ControlName}Handler.cs`
2. Inherit from `TuiViewHandler<I{Control}, {TuiVisualType}>`
3. Define a static `PropertyMapper` mapping MAUI properties to TUI updates
4. Override `CreatePlatformView()` to return the XenoAtom control
5. Add a `handlersCollection.AddHandler<>()` line in `AppHostBuilderExtensions.AddMauiControlsHandlers()`

## Testing

There are no automated tests yet. To verify changes:

1. `dotnet build Maui.TUI.slnx` — ensure it compiles
2. `cd samples/Maui.TUI.Sample && dotnet run -- --dump` — verify the visual tree
3. `cd samples/Maui.TUI.Sample && dotnet run -- --svg` — render to SVG and inspect
4. `cd samples/Maui.TUI.Sample && dotnet run` — run interactively and verify behavior (Ctrl+Q to exit)

## Important Notes

- The `run-sample.sh` script opens a new macOS Terminal window to run the sample (required because TUI apps need a full terminal)
- Alert handling uses reflection against MAUI internals — changes to `Microsoft.Maui.Controls.Platform.AlertManager` in new MAUI versions may require updates to `TuiAlertManager.cs`
- The project targets .NET 10 preview and MAUI 10 — ensure you have the correct SDK installed
