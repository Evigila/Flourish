---
title: Runtime APIs
description: Use focused Flourish services after the application is built.
---

# Runtime APIs

Builders define startup state; runtime services change and report the built application:

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

Theme, material, and font services expose their complete state through `Current`. `FontChangedEventArgs` adds `ChangeKind` and `AffectedPageType`; the resulting state remains `args.Current`.

Most simple state changes use `StateChangedEventArgs<TState>` or `StateTransitionEventArgs<TState>`. Domain events retain specialized arguments only when they carry additional change metadata.

Collection services also use `Current` and `Changed`. Specialized events publish the resulting snapshot and `CollectionChangeKind` when needed.

`ILocalizationService.Current` contains `Locale` and `AvailableLocales`; call `SetLocale` and observe `Changed` for locale or source updates. `IProfileService.Current` is a `ProfileState` containing the profile, login state, and name order, and its `Changed` event reports the replacement snapshot.

## Configuration and writable settings

Inject standard `Microsoft.Extensions.Configuration.IConfiguration` to read the effective Host configuration, including appsettings, User Secrets, environment variables, command-line arguments, and sources added with `ConfigureConfiguration`.

`appsettings.Flourish.json` remains the default writable settings file. `IDataBuilder.SetAppSettingsFilePath` can select another path before build. Inject `ISettingsStore` to atomically update values under `Flourish:`; each successful write reloads the same standard `IConfiguration` pipeline.

```csharp
public async ValueTask SaveEndpointAsync(
    IConfiguration configuration,
    ISettingsStore settings,
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
| Localization and settings | `ILocalizationService`, `ISettingsStore`, standard `IConfiguration` |

`INavigationService.Current` combines route, menu, panel, history, and cache state.

`ITitleBarService` owns title content, visibility, and search; `SubscribeSearch` returns `IRegistration`.

## Registration lifetimes

Command handlers, shortcuts, runtime routes, shell-region entries, close guards, and title-bar search handlers return the common `IRegistration`. Keep the lease while the feature is active, then call `Dispose()`.

`CultureRegistration` also implements `IRegistration`. It adds `Reload()` for refreshing its culture file and unregisters when disposed. `IStatusBarItemHandle` inherits `IRegistration`; it and `NotificationHandle` retain specialized APIs because they can update a live presentation as well as dispose it.

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
