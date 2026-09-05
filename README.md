# Flourish

Flourish is a Windows application shell and control library. The repository separates platform-neutral state and services into Core while retaining the complete WPF implementation. The WinUI 3 projects are intentionally kept as standard blank Windows App SDK baselines for future, feature-by-feature implementation.

Packages are split by responsibility:

- `Arkheide.Flourish.Core` contains platform-neutral contracts and services.
- `Arkheide.Flourish.WPF` contains the production WPF shell and controls.

The future WinUI 3 package is not currently implemented or ready for consumption.

Shared Flourish contracts use one namespace:

```csharp
using ArkheideSystem.Flourish.Abstract;
```

## WPF quick start

```csharp
return ApplicationBuilder
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

Startup configuration is organized by feature. `IApplicationBuilder.ConfigureAppearance`, `ConfigureFont`, `ConfigureLayout`, `ConfigureToolTips`, `ConfigureProjects`, `ConfigureTitleBar`, `ConfigureNavigation`, `ConfigureContent`, `ConfigureToolbar`, `ConfigureMotion`, `ConfigureWindow`, and `ConfigureStatusBar` each expose a focused feature builder.

After startup, resolve the matching service from dependency injection. Stateful services expose an immutable `Current` snapshot and a `Changed` event; methods such as `SetTheme`, `SetFont`, `SetEnabled`, and `SetPanelWidth` update the live application.

## Architecture

- `src/Flourish.Core` contains UI-independent contracts, immutable state, configuration, commands, background tasks, localization, projects, Profile state, notifications, layout and Shell state.
- `src/Flourish.WPF` is the WPF platform project and produces `Flourish.WPF.dll`.
- `src/Flourish.WinUI3` and `src/Gallery.WinUI3` are standard blank WinUI 3 baselines. They intentionally contain no Flourish Shell implementation yet.
- `src/Gallery.WPF` is the validation application for the current implementation.
- WPF depends on Core; Core does not reference either UI framework. WinUI 3 will consume Core only as each planned feature is deliberately implemented.

Startup and runtime use the same feature vocabulary while remaining lifecycle-safe. For example, `INavigationBuilder.AddNavigable` records a route before `Build`, while `INavigationService.AddNavigable` publishes one after startup. Builders become immutable when build begins; services remain the only runtime mutation surface.

## Configuration and settings

Flourish uses the standard Microsoft `IConfiguration` produced by the Generic Host. The default writable Flourish settings file is `appsettings.Flourish.json` and can be changed with `IDataBuilder.SetAppSettingsFilePath`.

- Inject `IConfiguration` to read the effective configuration from appsettings, User Secrets, environment variables, command-line arguments, and sources added through `ConfigureConfiguration`.
- Inject `ISettingsStore` to update values owned by the `Flourish:` section atomically. Its writes reload the standard configuration pipeline.

See the WPF guides in [English](docs/en-us/articles/getting-started.md) or [简体中文](docs/zh-cn/articles/getting-started.md). The future WinUI 3 implementation is tracked as fine-grained modules in the [migration roadmap](docs/roadmap.md).

## License

Flourish is licensed under the MIT License. See [LICENSE.txt](LICENSE.txt).
