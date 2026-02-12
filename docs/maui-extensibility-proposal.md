# Proposal: Public Extensibility APIs for Custom .NET MAUI Backends

## Summary

While building [Maui.TUI](https://github.com/nicwise/Maui.TUI) — a standalone .NET MAUI backend that renders to a terminal UI — we identified several internal APIs that custom backend implementations must work around using reflection. This document proposes minimal, non-breaking additions to the MAUI public API surface that would make it feasible to build third-party MAUI backends without reflection hacks.

This is informed by real implementation experience: Maui.TUI currently has working handlers for 20+ controls, navigation, tabbed pages, alerts/dialogs, and a dispatcher — all without forking MAUI. The pain points below are the remaining friction.

---

## Problem 1: Alert/Dialog Subscription Requires Reflection

### Current Behavior

`AlertManager` and its nested `IAlertManagerSubscription` interface are internal types in `Microsoft.Maui.Controls.Platform`. Custom backends cannot implement alert handling (`DisplayAlert`, `DisplayActionSheet`, `DisplayPromptAsync`) without reflection.

The `AlertManager.Subscribe()` method already has a DI-based extensibility point:

```csharp
// Inside AlertManager.Subscribe() (internal):
_subscription =
    context.Services.GetService<IAlertManagerSubscription>() ??  // ← DI lookup!
    CreateSubscription(context);                                  // ← platform fallback
```

This is the correct design pattern — it just needs the interface to be public so custom backends can register an implementation.

### What We Have to Do Today

```csharp
// 1. Use reflection to find the internal interface type
var amType = typeof(Window).Assembly
    .GetType("Microsoft.Maui.Controls.Platform.AlertManager");
var iamsType = amType?.GetNestedType("IAlertManagerSubscription",
    BindingFlags.Public | BindingFlags.NonPublic);

// 2. Create a DispatchProxy to implement the internal interface at runtime
var proxyType = typeof(AlertSubscriptionProxy<>).MakeGenericType(iamsType);
var createMethod = typeof(DispatchProxy)
    .GetMethods(BindingFlags.Public | BindingFlags.Static)
    .First(m => m.Name == "Create" && m.GetGenericArguments().Length == 2)
    .MakeGenericMethod(iamsType, proxyType);
var proxy = createMethod.Invoke(null, null);

// 3. Register via DI so AlertManager.Subscribe() discovers it
services.AddSingleton(iamsType, proxy);
```

And then the proxy implementation must use string-based method dispatch:

```csharp
public class AlertSubscriptionProxy<T> : DispatchProxy
{
    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        switch (targetMethod.Name)  // ← Fragile string matching
        {
            case "OnAlertRequested":
                // Must access AlertArguments via reflection too
                var title = args[1].GetType().GetProperty("Title")?.GetValue(args[1]);
                break;
        }
        return null;
    }
}
```

This approach:
- Uses 4+ separate reflection calls
- Creates a `DispatchProxy` with fragile string-based method dispatch
- Must access `AlertArguments` properties via reflection because args arrive as `object`
- Is incompatible with NativeAOT trimming
- Breaks silently at runtime if internal APIs change

### Proposed Fix

**Make `IAlertManagerSubscription` a public, non-nested interface:**

```csharp
namespace Microsoft.Maui.Controls.Platform;

/// <summary>
/// Implement this interface and register it in DI to handle
/// DisplayAlert, DisplayActionSheet, and DisplayPromptAsync on custom platforms.
/// </summary>
public interface IAlertManagerSubscription
{
    void OnAlertRequested(Page sender, AlertArguments arguments);
    void OnActionSheetRequested(Page sender, ActionSheetArguments arguments);
    void OnPromptRequested(Page sender, PromptArguments arguments);
}
```

**What our code would become:**

```csharp
// Clean, strongly-typed, no reflection:
public class TuiAlertSubscription : IAlertManagerSubscription
{
    public void OnAlertRequested(Page sender, AlertArguments args)
    {
        var title = args.Title;        // Direct property access
        var message = args.Message;
        // Show TUI dialog, then:
        args.Result.TrySetResult(true);
    }

    public void OnActionSheetRequested(Page sender, ActionSheetArguments args) { ... }
    public void OnPromptRequested(Page sender, PromptArguments args) { ... }
}

// Registration:
services.AddSingleton<IAlertManagerSubscription, TuiAlertSubscription>();
```

**Why this is safe:**
- `AlertManager.Subscribe()` already resolves this interface via DI — the extensibility point already exists, it just needs the interface to be public
- `AlertArguments`, `ActionSheetArguments`, and `PromptArguments` are already public types in `Microsoft.Maui.Controls.Internals`
- No existing platform code needs to change — the existing `AlertRequestHelper` partial class continues to work as the platform-specific fallback
- The `OnPageBusy` method is already `[Obsolete]` and could be excluded from the public interface

**Impact:** Zero breaking changes. One interface extracted from a nested internal type to a public top-level type.

### Estimated Diff

```diff
 // AlertManager.cs
 namespace Microsoft.Maui.Controls.Platform;

-internal partial class AlertManager
+internal partial class AlertManager  // AlertManager itself stays internal
 {
-    internal interface IAlertManagerSubscription
-    {
-        void OnActionSheetRequested(Page sender, ActionSheetArguments arguments);
-        void OnAlertRequested(Page sender, AlertArguments arguments);
-        void OnPromptRequested(Page sender, PromptArguments arguments);
-        [Obsolete] void OnPageBusy(Page sender, bool enabled);
-    }
+    // Moved to IAlertManagerSubscription.cs as public type
 }

+// New file: IAlertManagerSubscription.cs
+namespace Microsoft.Maui.Controls.Platform;
+
+public interface IAlertManagerSubscription
+{
+    void OnAlertRequested(Page sender, AlertArguments arguments);
+    void OnActionSheetRequested(Page sender, ActionSheetArguments arguments);
+    void OnPromptRequested(Page sender, PromptArguments arguments);
+}
```

Internal references (`AlertRequestHelper`, `AlertManager._subscription`) would change from `AlertManager.IAlertManagerSubscription` to just `IAlertManagerSubscription` — a find-and-replace.

---

## Problem 2: Alert Argument Types Live in `Internals` Namespace

### Current Behavior

`AlertArguments`, `ActionSheetArguments`, and `PromptArguments` are public types, but live in `Microsoft.Maui.Controls.Internals`. This namespace is documented as subject to breaking changes.

### Proposed Fix

Move these types to `Microsoft.Maui.Controls.Platform` (or `Microsoft.Maui.Controls`) and add `[TypeForwardedFrom]` attributes for binary compatibility. These types are already used in the public `Page.DisplayAlertAsync` API surface (they're the arguments that flow through the system), so promoting them to a stable namespace is appropriate.

If moving is too disruptive, at minimum **document that these types are stable** and won't be removed from the `Internals` namespace, since custom backends depend on them.

---

## Problem 3: No Public Bootstrap API for Custom Platforms

### Current Behavior

MAUI's application lifecycle is driven by platform-specific entry points (`Activity.OnCreate` on Android, `AppDelegate.FinishedLaunching` on iOS, etc.). There is no public API or documented pattern to bootstrap MAUI from a generic .NET application (console app, custom UI toolkit, etc.).

### What We Have to Do Today

Our `MauiTuiApplication` must manually replicate what each platform does internally:

```csharp
public Panel Initialize()
{
    // 1. Set global platform reference
    IPlatformApplication.Current = this;

    // 2. Build MAUI pipeline
    var mauiApp = CreateMauiApp();

    // 3. Manually create scoped contexts (MAUI does this internally on each platform)
    var rootContext = new TuiMauiContext(mauiApp.Services);
    var applicationContext = rootContext.MakeApplicationScope(this);

    // 4. Manually wire up Application handler
    var appHandler = new ApplicationHandler();
    appHandler.SetMauiContext(applicationContext);
    appHandler.SetVirtualView(application);

    // 5. Manually create Window + handler + wire to content
    var windowHandler = new WindowHandler();
    windowHandler.SetMauiContext(windowContext);
    windowHandler.SetVirtualView(window);
}
```

This initialization sequence was reverse-engineered from the Android/iOS platform code and the [maui.wpf](https://github.com/pureween/maui.wpf) prototype. It's fragile because:
- There's no documentation of which steps are required and in what order
- If MAUI adds new initialization steps in future versions, custom backends break silently
- Each custom backend author must independently reverse-engineer this

### Proposed Fix (Option A — Documentation)

At minimum, add a **"Custom Backend Guide"** to the MAUI documentation covering:

1. Required initialization sequence (context creation → handler wiring → window creation)
2. Which `IMauiContext` scoping steps are required (application scope vs window scope)
3. How to properly trigger lifecycle events (`IWindow.Created()`, `IWindow.Activated()`, etc.)
4. How to register platform-specific services vs using MAUI defaults

### Proposed Fix (Option B — Helper API)

Add a public helper class that encapsulates the standard initialization:

```csharp
namespace Microsoft.Maui.Hosting;

public static class MauiAppExtensions
{
    /// <summary>
    /// Creates the MAUI application pipeline for a custom platform.
    /// Returns the IApplication, scoped MauiContext, and Window.
    /// </summary>
    public static (IApplication App, IMauiContext Context, IWindow Window)
        CreateForCustomPlatform(this MauiApp mauiApp, IPlatformApplication platform);
}
```

This would replace ~30 lines of reverse-engineered initialization code in every custom backend with a single method call.

---

## Summary of Proposed Changes

| # | Change | Type | Breaking? | Effort | Impact |
|---|--------|------|-----------|--------|--------|
| 1 | Make `IAlertManagerSubscription` public | Visibility change | No | Very low | **High** — eliminates all reflection in alert handling |
| 2 | Stabilize alert argument types | Namespace/docs | Minor | Low | Medium — removes `Internals` dependency |
| 3a | Document custom backend bootstrap | Documentation | No | Low | **High** — unblocks all future backend authors |
| 3b | Add `CreateForCustomPlatform` helper | New API | No | Medium | High — reduces boilerplate significantly |

### Recommended Priority

1. **`IAlertManagerSubscription` → public** — Highest impact-to-effort ratio. The DI extensibility point already exists; the interface just needs to be visible. This is a ~20-line diff.

2. **Custom backend bootstrap documentation** — High impact, low effort. A single markdown guide covering the initialization sequence would save every future backend author days of reverse-engineering.

3. **`CreateForCustomPlatform` helper** — Nice-to-have that builds on the documentation. Formalizes the init sequence into a testable API.

4. **Alert argument namespace cleanup** — Lower priority, nice cleanup.

---

## Context & Prior Art

This proposal comes from building **Maui.TUI**, a terminal-based MAUI backend using [XenoAtom.Terminal.UI](https://xenoatom.github.io/terminal/). The project demonstrates that MAUI's handler architecture is remarkably well-suited for custom backends — the cross-platform layout engine, handler pipeline, and virtual view system all work cleanly out of the box. The issues above are the small number of remaining friction points.

Related prior art:
- [pureween/maui.wpf](https://github.com/pureween/maui.wpf) — WPF backend prototype
- [nicwise/Maui.TUI](.) — This project (terminal backend)
- The existing Android, iOS, Windows, macCatalyst, and Tizen backends in `dotnet/maui`

### What Already Works Well

For balance, here's what's great about MAUI's current extensibility:

- **Handler architecture** — `ViewHandler<TVirtualView, TPlatformView>` is clean and easy to implement for custom controls
- **Cross-platform layout** — `CrossPlatformMeasure`/`CrossPlatformArrange` delegation means custom backends get correct layout for free
- **DI-based services** — Dispatcher, handlers factory, and most services are properly injectable
- **`ToPlatform()` extensions** — Clean way to convert virtual views to platform views recursively
