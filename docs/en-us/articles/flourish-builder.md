---
title: IApplicationBuilder
description: Configure and build a Flourish application with focused feature builders.
---

# IApplicationBuilder

`IApplicationBuilder` records startup configuration, then `Build()` creates a Generic Host-backed `IApplicationRuntime`. Public contracts are in `ArkheideSystem.Flourish.Abstract`.

```csharp
using ArkheideSystem.Flourish.Abstract;

using var flourish = ApplicationBuilder
    .CreateDefaultBuilder(args)
    .ConfigureServices((context, services) => services.AddSingleton<App>())
    .ConfigureTitleBar(titleBar =>
        titleBar.SetEnabled().SetApplicationTitle("Foobar"))
    .Build();

return flourish.Run<App>();
```

## Feature builders

| Entry point | Builder | Startup responsibility |
| --- | --- | --- |
| `ConfigureData` | `IDataBuilder` | Locale, culture files, and writable settings/project paths. |
| `ConfigureConfiguration` | standard `IConfigurationBuilder` | Application-owned Microsoft configuration sources. |
| `ConfigureServices` | standard `IServiceCollection` | Application services and replaceable Flourish services. |
| `ConfigureAppearance` | `IAppearanceBuilder` | Material, palette, and corner-radius defaults. |
| `ConfigureFont` | `IFontBuilder` | Global and page-specific fonts. |
| `ConfigureLayout` | `ILayoutBuilder` | Centered content and smooth scrolling. |
| `ConfigureToolTips` | `IToolTipBuilder` | Flourish tooltip presentation and timing. |
| `ConfigureProjects` | `IProjectBuilder` | Project-aware shell mode. |
| `ConfigureTitleBar` | `ITitleBarBuilder` | Title bar state and content. |
| `ConfigureNavigation` | `INavigationBuilder` | Navigation surface, routes, pages, and visible items. |
| `ConfigureContent` | `ICustomContentBuilder` | Custom WPF shell-region content. |
| `ConfigureToolbar` | `IToolbarBuilder` | Page-specific toolbar definitions. |
| `ConfigureMotion` | `IMotionBuilder` | Motion state and transition defaults. |
| `ConfigureWindow` | `IWindowBuilder` | Initial window bounds and behavior. |
| `ConfigureStatusBar` | `IStatusBarBuilder` | Status surface and built-in indicators. |

Builder callbacks may be registered more than once before `Build()` and run in registration order. `Build()` consumes the builder; later configuration, a second build, or use of a captured nested builder throws `InvalidOperationException`.

## Host configuration

`CreateDefaultBuilder` retains standard .NET Host configuration. `ConfigureConfiguration` receives Microsoft `IConfigurationBuilder`, while `ConfigureServices` receives `HostBuilderContext` and `IServiceCollection`. Read effective configuration by injecting `IConfiguration`.

`appsettings.Flourish.json` remains the default writable Flourish settings file. `ISettingsStore` owns atomic writes under `Flourish:` and reloads the same standard `IConfiguration`; it does not replace Microsoft configuration.

## Runtime services

After `Build()`, resolve the corresponding runtime service through dependency injection. Stateful services expose `Current` and `Changed`.
