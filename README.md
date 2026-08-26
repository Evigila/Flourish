# Arkheide.Flourish

Arkheide.Flourish is a WPF application shell and control library for the Arkheide ecosystem. It combines a .NET Generic Host, dependency injection, navigation, title bar, toolbar, status, appearance, localization, projects, notifications, and background-task services behind a compact public API.

All Flourish public contracts use one namespace:

```csharp
using ArkheideSystem.Flourish.Abstract;
```

## Quick start

```csharp
return FlourishBuilder
    .CreateDefaultBuilder(args)
    .ConfigureServices((_, services) => services.AddSingleton<App>())
    .ConfigureTitleBar(titleBar =>
        titleBar.SetEnabled().SetApplicationTitle("Foobar").SetNavigationToggle())
    .ConfigureNavigation(navigation =>
        navigation
            .SetEnabled()
            .AddNavigable<HomePage>("Home", "\uE80F", isInitial: true))
    .Run<App>();
```

Startup configuration is organized by feature. `IFlourishBuilder.ConfigureAppearance`, `ConfigureFont`, `ConfigureLayout`, `ConfigureToolTips`, `ConfigureProjects`, `ConfigureTitleBar`, `ConfigureNavigation`, `ConfigureContent`, `ConfigureToolbar`, `ConfigureMotion`, `ConfigureWindow`, and `ConfigureStatusBar` each expose a focused feature builder.

After startup, resolve the matching service from dependency injection. Stateful services expose an immutable `Current` snapshot and a `Changed` event; methods such as `SetTheme`, `SetFont`, `SetEnabled`, and `SetPanelWidth` update the live application.

## Architecture

- `Abstract` contains the complete public contract surface: focused builders, runtime services, immutable state, events, and public models. Consumers only need `ArkheideSystem.Flourish.Abstract` for application APIs.
- `Controls` and `Themes` contain the reusable WPF control and resource libraries. They are the only namespaces mapped to the Flourish XAML URI.
- Feature folders such as `Navigation`, `Appearance`, `Motion`, `Profile`, and `Windowing` own their options, builders, services, persistence helpers, and other non-public implementation types.
- `Views` contains the shell and its internal pages. Views consume feature modules and adapt their state to WPF; they do not define application services.
- `Hosting` is the composition boundary. It creates the Generic Host, connects feature options and implementations, restores preferences, and owns startup and shutdown.

Startup and runtime use the same feature vocabulary while remaining lifecycle-safe. For example, `INavigationBuilder.AddNavigable` records a route before `Build`, while `INavigationService.AddNavigable` publishes one after startup. Builders become immutable when build begins; services remain the only runtime mutation surface.

## Configuration and settings

Flourish uses the standard Microsoft `IConfiguration` produced by the Generic Host. The default writable Flourish settings file is `appsettings.Flourish.json` and can be changed with `IDataBuilder.SetAppSettingsFilePath`.

- Inject `IConfiguration` to read the effective configuration from appsettings, User Secrets, environment variables, command-line arguments, and sources added through `ConfigureConfiguration`.
- Inject `IFlourishSettingsStore` to update values owned by the `Flourish:` section atomically. Its writes reload the standard configuration pipeline.

See the [English documentation](docs/en-us/articles/getting-started.md) or [简体中文文档](docs/zh-cn/articles/getting-started.md).

## License

Arkheide.Flourish is licensed under the MIT License. See [LICENSE.txt](LICENSE.txt).
