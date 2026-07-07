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

    services.AddNavigable<HomePage>("Home", "\uE80F");
    services.AddNavigable<SettingsPage>("Settings", "\uE713");
});
```

## Details

Pages shown by the navigation panel are registered with `AddNavigable`. The page must derive from `System.Windows.Controls.Page`; Flourish resolves it from dependency injection during navigation.

Command parsers implement `ICommandParser` and are registered here. Navigation items, dynamic toolbar buttons, title bar actions, and footer commands can all send command keys into this parser chain.

View models, repositories, and application services also belong in `ConfigureServices`. Detailed shell configuration should stay in the dedicated APIs such as [`ConfigureNavigation`](configure-navigation.md), [`ConfigureTitleBar`](configure-title-bar.md), and [`ConfigureWindow`](configure-window.md).

## Related APIs

- [`ConfigureNavigation`](configure-navigation.md) places registered pages in the visible navigation model.
- [`ConfigureDynamicToolbar`](configure-dynamic-toolbar.md) attaches page commands to registered page types.
- [`Command parser`](command-parser.md) describes command-key routing.
