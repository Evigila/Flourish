---
title: ConfigureServices
description: Register WPF application services, pages, and command parsers.
---

# ConfigureServices

`ConfigureServices` registers application services in the underlying .NET Generic Host. It is the only `Configure...` callback that receives `HostBuilderContext`, because service registration often depends on environment or configuration.

```csharp
builder.ConfigureServices((context, services) =>
{
    services.AddSingleton<App>();
    services.AddSingleton<ICommandParser, AppCommandParser>();

    services.AddNavigable<HomePage>("Home", "\uE80F", navigationKey: NavigationRoutes.Home);
    services.AddNavigable<SettingsPage>("Settings", "\uE713", navigationKey: NavigationRoutes.Settings);
});
```

## Details

Pages shown by the navigation panel are registered with `AddNavigable`. The page must derive from `System.Windows.Controls.Page`; Flourish resolves it from dependency injection during navigation. Use `navigationKey` for runtime navigation from view models so they do not reference page classes.

Command parsers implement `ICommandParser` and are registered here. Navigation items, dynamic toolbar buttons, title bar actions, and footer commands can all send command keys into this parser chain.

View models, repositories, and application services also belong in `ConfigureServices`. Detailed shell configuration should stay in the dedicated APIs such as [`ConfigureNavigation`](configure-navigation.md), [`ConfigureTitleBar`](configure-title-bar.md), and [`ConfigureWindow`](configure-window.md).

Profile authentication is replaceable here as well. Register `IProfileAuthService` to keep the built-in state and encrypted persistence, or register `IProfileService` to replace the complete profile workflow. Flourish uses its defaults only when these interfaces have not already been registered.

## Related APIs

- [`ConfigureNavigation`](configure-navigation.md) places registered pages in the visible navigation model.
- [`ConfigureDynamicToolbar`](configure-dynamic-toolbar.md) attaches page commands to registered page types.
- [`ConfigureProfile`](configure-profile.md) describes profile pages, authentication, and persistence.
- [`Command parser`](command-parser.md) describes command-key routing.
