---
title: Runtime APIs
description: Inspect and change Flourish configuration, shell surfaces, projects, navigation, commands, windows, notifications, and background work while the application is running.
---

# Runtime APIs

Flourish uses two complementary configuration layers:

- `IFlourishBuilder` and its `Configure...` callbacks define the initial application graph and startup state. Use them to register pages and services, select defaults, and validate the application before the shell opens.
- Runtime services change the live application after `Build()`. Resolve them through dependency injection, preferably by constructor injection into a page, view model, or application service.

All runtime services below are registered as singletons. A builder remains the right place for deterministic startup defaults; a runtime service is the right place for user preferences, plug-ins, feature switches, and state that changes during a session.

## State, events, and lifetimes

Stateful services expose an immutable `Current` snapshot (or a named snapshot such as `ActiveTasks` or `Registrations`) and a `Changed`, `StateChanged`, or domain-specific event. Read the new snapshot after an event instead of retaining mutable UI objects. Events normally run on the thread that caused the change; configuration reloads, background tasks, and notification expiry can raise events away from the WPF dispatcher, so marshal UI updates when required.

Registration and presentation APIs use disposable leases. Keep the returned object for exactly as long as the feature should exist, then call `Dispose()`. This applies to `ICommandRegistration`, `IShortcutRegistration`, `INavigationRouteRegistration`, `IShellRegionRegistration`, `IStatusBarItemHandle`, `FlourishNotificationHandle`, `IWindowCloseGuardRegistration`, and title-bar search subscriptions. `FlourishLocaleRegistration` is removed explicitly with `IFlourishLocalization.Unregister`.

## Configuration and localization

| Service | Runtime use |
| --- | --- |
| `IFlourishConfiguration` | Read `Current`, the string indexer, `Get<T>`, or `GetSection<T>` from the effective Host configuration; call `Reload()` and observe `Changed`. |
| `IAppSettingsStore` | Atomically `SetAsync`, `RemoveAsync`, `MergeAsync`, `AppendAsync`, or apply several edits in one `UpdateAsync` transaction. A changed file reloads Host configuration. |
| `IFlourishLocalization` | Read and format keys, call `SetLocale`, and register, reload, or unregister `lang_<locale>.json` files while running. |

Only `IAppSettingsStore` writes the base `appsettings.json`; ordinary runtime snapshots are in-memory unless their service explicitly documents persistence.

```csharp
public async ValueTask SaveEndpointAsync(
    IAppSettingsStore settings,
    IFlourishLocalization localization,
    string endpoint,
    CancellationToken cancellationToken)
{
    await settings.UpdateAsync(editor =>
    {
        editor.Set("Api:BaseUrl", endpoint);
        editor.Merge("FeatureFlags", new { ReportsEnabled = true });
        editor.Append("Api:RecentEndpoints", endpoint);
    }, cancellationToken);

    localization.SetLocale("CN");
}
```

## Appearance and shell features

| Service | Runtime use |
| --- | --- |
| `IShellFeatureService` | Enable or disable `TitleBar`, `Navigation`, `DynamicToolbar`, `StatusContent`, `ToolTips`, `Motion`, or `Profile` with `SetEnabled`. |
| `IThemeService` | Select and persist `System`, `Light`, or `Dark` with `SetTheme`, or cycle with `ToggleTheme`; inspect `EffectiveTheme` and `IsDark`. |
| `IFontService` | Atomically change the global family and independent positive, finite Small, Standard, Icon, Large, ExtraLarge, and HeaderSize sizes with `SetFont`; change the icon family; inspect, set, and clear page-specific overrides through `PageOverrides`, `SetOverrideFont`, and `ClearOverrideFont`. |
| `IToolTipService` | Enable tooltips and change their initial delay and spawn margin with `Configure`. |
| `IMotionService` | Enable motion, change page/navigation transitions and durations, configure hover reveal, and respect Windows reduced-motion settings. |
| `IMaterialEffectService` | Test support and apply a `MaterialEffect`, or change immersive dark mode. |

`ShellFeature.TitleBar` switches between the Flourish custom title bar and the native
Windows title bar. Disabling the feature restores the native title bar without changing
the requested material effect; enabling it again restores the Flourish title bar and
reapplies that material request to the custom frame.

## Title bar, projects, and search

| Service | Runtime use |
| --- | --- |
| `ITitleBarService` | Change the application title/subtitle, unnamed-project placeholder, logo and information-field visibility, search placeholder, breadcrumb mode, and each `TitleBarElement`. |
| `IProjectService` | Add, update, query, activate, and remove in-memory `FlourishProject` metadata; change project mode; observe snapshots; and handle title-menu creation or activation requests. |
| `ITitleBarSearchService` | Control search text, visibility, placeholder, clearing and focus; observe `QueryChanged`; and add ordered asynchronous handlers through `Subscribe`. |

The title bar displays `IProjectService.Current.ActiveProject` while project mode is enabled and otherwise displays the application title. Project storage paths are descriptive only; project operations do not access the file system. Handle `NewProjectRequested` and `ProjectActivationRequested`, complete application work, then call `AddProject` or `SetActiveProject` to update the shell. See [Projects](projects.md).

```csharp
public sealed class SearchModule(
    ITitleBarSearchService search,
    ISearchIndex searchIndex) : IDisposable
{
    private readonly IDisposable subscription = search.Subscribe(async (query, token) =>
    {
        await searchIndex.UpdateResultsAsync(query.Text, token);
    });

    public void Open()
    {
        search.SetVisible(true);
        search.SetPlaceholder("Search reports");
        search.Focus();
    }

    public void Dispose() => subscription.Dispose();
}
```

## Navigation, routes, and page cache

| Service | Runtime use |
| --- | --- |
| `INavigationService` | Navigate by case-sensitive route key or page type, navigate asynchronously, inspect the current route/parameter, and control back/forward history. |
| `INavigationPanelService` | Enable, move, size, open, close, or toggle the navigation panel. |
| `INavigationMenuService` | Atomically edit groups, fixed items, page/command items, order, labels, visibility, enabled state, and tree expansion through `Update`. |
| `INavigationRouteRegistry` | `Register` or `Upsert` a route, remove it, and change its `FlourishPageCacheMode`. |
| `IPageCacheService` | Change cache mode by page type, inspect cached page types, evict one page, or clear all cached instances. |

Register the route before exposing a menu item that targets it. Remove the menu item before disposing the route lease.

```csharp
public sealed class DiagnosticsModule : IDisposable
{
    private readonly INavigationRouteRegistration route;
    private readonly INavigationMenuService menu;

    public DiagnosticsModule(
        INavigationRouteRegistry routes,
        INavigationMenuService menu,
        INavigationService navigation)
    {
        this.menu = menu;
        route = routes.Register(new FlourishNavigationRoute(
            "runtime.diagnostics",
            typeof(DiagnosticsPage),
            FlourishPageCacheMode.Enabled));

        menu.Update(editor =>
        {
            editor.AddGroup("runtime", "Runtime");
            editor.AddItem("runtime", FlourishNavigationMenuItem.Page(
                "runtime.diagnostics.item", "runtime.diagnostics", "Diagnostics", "\uE9D2"));
        });

        navigation.Navigate("runtime.diagnostics");
    }

    public void Dispose()
    {
        menu.Update(editor =>
        {
            editor.RemoveItem("runtime.diagnostics.item");
            editor.RemoveGroup("runtime");
        });
        route.Dispose();
    }
}
```

## Toolbar, status bar, and shell regions

| Service | Runtime use |
| --- | --- |
| `IToolbarService` | Enable the surface; replace default or page-specific definitions; add, upsert, move, show, enable, remove, or clear `FlourishToolbarItem` values; change page `IconOnly` mode. |
| `IStatusBarService` | Enable custom content and built-in LAN/power indicators; add, update, move, show, hide, remove, or clear `FlourishStatusItem` values. `Show` can create a timed item and returns a disposable handle. |
| `IShellRegionService` | Add or upsert WPF content factories in a `FlourishRegion`, then enable, reorder, remove, or clear registrations. |

Toolbar and navigation command items are dispatched through `ICommandDispatcher`.

## Commands and keyboard shortcuts

`ICommandRegistry.Register` adds an asynchronous handler with an optional availability predicate, duplicate policy, and priority. `ICommandDispatcher.CanExecute` queries whether a command is available. `ExecuteAsync` dispatches the command and returns a captured `CommandResult`. `IShortcutService.Register` maps a WPF `KeyGesture` to a command with application, window, or page scope and configurable conflict handling.

Shortcuts are ignored while a text input control has keyboard focus by default, preserving typing, clipboard, editing, AltGr, and IME behavior. Set `ShortcutRegistrationOptions.AllowWhenTextInputFocused` to `true` only for shortcuts that must remain active while the user is editing text.

Command handlers are registered through `ICommandRegistry` and invoked through `ICommandDispatcher`. Keep each `ICommandRegistration` for the lifetime of its owning feature and dispose it to remove the handler. For mappings that share the complete Host lifetime, implement `ICommandParser` so Flourish owns their leases automatically.

```csharp
public sealed class RefreshBindings : IDisposable
{
    private readonly ICommandRegistration command;
    private readonly IShortcutRegistration shortcut;

    public RefreshBindings(
        ICommandRegistry commands,
        IShortcutService shortcuts,
        IDataRefresher refresher)
    {
        command = commands.Register("data.refresh", async (_, token) =>
        {
            await refresher.RefreshAsync(token);
            return CommandResult.Handled;
        });

        shortcut = shortcuts.Register(
            new KeyGesture(Key.F5, ModifierKeys.Control),
            "data.refresh");
    }

    public void Dispose()
    {
        shortcut.Dispose();
        command.Dispose();
    }
}
```

Call `NotifyCanExecuteChanged(commandKey)` when external state changes an availability predicate.

## Window, tray, close, profile, messages, and notifications

| Service | Runtime use |
| --- | --- |
| `IWindowService` | Change bounds and size constraints, resize mode, topmost/taskbar state; center, show, hide, activate, minimize, maximize, or restore the shell. |
| `ITrayService` | Enable notification-area behavior, change its tooltip, minimize to tray, restore, or request exit. |
| `IWindowCloseService` | Select `Prompt`, `Close`, or `MinimizeToTray`; register ordered asynchronous close guards; evaluate or request a close. |
| `IProfileFlyoutService` | Enable, show, hide, or toggle the profile flyout and replace its WPF `Page` content. |
| `IProfileService` | Inspect profile/login state, initialize remembered login, sign in, change remember-login behavior, or sign out. |
| `IMessageService` | Show standard or custom-choice modal messages synchronously or with `ShowAsync`; async cancellation only applies before the dialog opens. |
| `INotificationService` | `Show` or `Upsert` non-modal notifications, inspect active notifications, dismiss one/all, and optionally dispatch a command when activated. |

Close guards and notifications return disposable leases. A notification handle can update its notification before dismissal.
`IProfileAuthService` remains a startup-registered authentication provider; `IProfileService` is the runtime facade that invokes it.

## Background tasks

`IBackgroundTaskService.AddTask` queues bounded asynchronous work at runtime. Each task receives cooperative cancellation and progress reporting through `FlourishBackgroundTaskContext`; the returned handle exposes `Cancel`, `Snapshot`, and a `Completion` task whose result captures success, cancellation, or failure.

```csharp
public FlourishBackgroundTaskHandle StartExport(
    IBackgroundTaskService tasks,
    IReportExporter exporter)
{
    return tasks.AddTask(
        new FlourishBackgroundTaskMetadata("Export report", "Writing files", "\uE74E"),
        async context =>
        {
            for (var step = 1; step <= 10; step++)
            {
                await exporter.WritePartAsync(step, context.CancellationToken);
                context.ReportProgress(step / 10d);
            }
        });
}
```

Task delegates do not run on the WPF UI thread. Observe `TasksChanged` for live shell or application UI, and dispatch control updates to the UI thread.

## Related guides

- [IFlourishBuilder](flourish-builder.md) and [Dependency injection](configure-services.md)
- [Application data](configure-data.md), [Projects](projects.md), [Navigation](navigation.md), and [Command dispatch](commands.md)
- [Dynamic toolbar](dynamic-toolbar.md), [Status bar](status-bar.md), and [Background tasks](background-tasks.md)
- [Window](configure-window.md), [Profile](configure-profile.md), and [Message service](message-service.md)
