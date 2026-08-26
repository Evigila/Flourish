---
title: Runtime APIs
description: Use focused Flourish services after the application is built.
---

# Runtime APIs

Startup builders and runtime services describe the same domains at different times:

- a feature Builder records the initial draft before `Build()`;
- a feature Service changes the built application and reports its live state.

All contracts use `ArkheideSystem.Flourish.Abstract` and all services are available through dependency injection.

## State and events

Stateful UI services expose an immutable `Current` snapshot and a `Changed` event. Read related values from one snapshot so they represent one atomic state.

```csharp
var themeState = theme.Current;
var materialState = material.Current;
var fontState = fonts.Current;

theme.Changed += (_, change) => ApplyTheme(change.Current);
```

`IThemeService.Current` contains `RequestedTheme`, `EffectiveTheme`, and `IsDark`. `IMaterialEffectService.Current` contains requested/effective effects, support, application, and dark-mode state. `IFontService.Current` contains both font families, all size tiers, and `PageOverrides`. Font changes keep the specialized `FlourishFontChangedEventArgs` because `ChangeKind` and `AffectedPageType` identify the changed domain scope; the resulting font state is still `args.Current`.

Most simple state changes use `FlourishStateChangedEventArgs<TState>` or `FlourishStateTransitionEventArgs<TState>`. Domain events retain specialized arguments only when they carry additional change metadata.

Collection services use the same `Current`/`Changed` shape. `IBackgroundTaskService.Current` is the active task list, and its generic change event publishes that same cached list through `args.Current`. `INotificationService.Current` is a `FlourishNotificationState` that keeps `Notifications` and `Version` atomic. `ICommandRegistry` and `IShortcutService` expose active registrations; their specialized event arguments use `Current` for the resulting snapshot and `FlourishRuntimeChangeKind` for `Added`, `Updated`, or `Removed` metadata.

`IFlourishLocalization.Current` contains `Locale` and `AvailableLocales`; call `SetLocale` and observe `Changed` for locale or source updates. `IProfileService.Current` is a `FlourishProfileState` containing the profile, login state, and name order, and its `Changed` event reports the replacement snapshot.

## Configuration and writable settings

Inject standard `Microsoft.Extensions.Configuration.IConfiguration` to read the effective Host configuration, including appsettings, User Secrets, environment variables, command-line arguments, and sources added with `ConfigureConfiguration`.

`appsettings.Flourish.json` remains the default writable settings file. `IDataBuilder.SetAppSettingsFilePath` can select another path before build. Inject `IFlourishSettingsStore` to atomically update values under `Flourish:`; each successful write reloads the same standard `IConfiguration` pipeline.

```csharp
public async ValueTask SaveEndpointAsync(
    IConfiguration configuration,
    IFlourishSettingsStore settings,
    string endpoint,
    CancellationToken cancellationToken)
{
    string? previous = configuration["Flourish:Extensions:Api:BaseUrl"];
    await settings.SetAsync(
        "Flourish:Extensions:Api:BaseUrl",
        endpoint,
        cancellationToken);
}
```

## Runtime service map

| Domain | Runtime service |
| --- | --- |
| Theme and appearance | `IThemeService`, `IAppearanceService`, `IMaterialEffectService`, `IFontService` |
| Layout and interaction | `IContentLayoutService`, `IScrollService`, `IToolTipService`, `IMotionService` |
| Title bar and profile | `ITitleBarService`, `IProfileFlyoutService`, `IProfileService` |
| Navigation | `INavigationService` |
| Toolbar, status, regions | `IToolbarService`, `IStatusBarService`, `IShellRegionService` |
| Window and tray | `IWindowService`, `IWindowCloseService`, `ITrayService` |
| Projects | `IProjectService`, `IProjectBehavior` |
| Commands | `ICommandRegistry`, `ICommandDispatcher`, `IShortcutService` |
| Messaging and work | `IMessageService`, `INotificationService`, `IBackgroundTaskService` |
| Localization and settings | `IFlourishLocalization`, `IFlourishSettingsStore`, standard `IConfiguration` |

`INavigationService` is the complete navigation facade. Its `Current` snapshot combines active navigation, route, menu, panel, and cache state; it also owns route registration, menu transactions, panel changes, navigation history, and page-cache operations.

`ITitleBarService` similarly owns title content, element visibility, search state, and search subscriptions. Search registration returns `IRegistration` through `SubscribeSearch`.

## Registration lifetimes

Command handlers, shortcuts, runtime routes, shell-region entries, close guards, and title-bar search handlers return the common `IRegistration`. Keep the lease while the feature is active, then call `Dispose()`.

`FlourishCultureRegistration` also implements `IRegistration`. It adds `Reload()` for refreshing its culture file and unregisters when disposed. `IStatusBarItemHandle` inherits `IRegistration`; it and `FlourishNotificationHandle` retain specialized APIs because they can update a live presentation as well as dispose it.

```csharp
public sealed class RefreshBindings : IDisposable
{
    private readonly IRegistration command;
    private readonly IRegistration shortcut;

    public RefreshBindings(ICommandRegistry commands, IShortcutService shortcuts)
    {
        command = commands.Register("refresh", (_, _) =>
            ValueTask.FromResult(CommandResult.Handled));
        shortcut = shortcuts.Register(new KeyGesture(Key.F5), "refresh");
    }

    public void Dispose()
    {
        shortcut.Dispose();
        command.Dispose();
    }
}
```
