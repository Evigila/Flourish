---
title: IFlourishBuilder
description: Understand the builder, hosting integration, services, and page registration.
---

# IFlourishBuilder

`IFlourishBuilder` is the composition entry point for a Flourish application. It does not directly create windows or pages during configuration. Instead, it collects service registrations and shell options, then builds an `IFlourish` runtime backed by a .NET Generic Host.

## Hosting model

`FlourishBuilder.CreateDefaultBuilder(args)` follows the .NET Generic Host default configuration and lifetime model:

- configuration and environment are available through `HostBuilderContext` in `ConfigureServices`
- services are registered in `IServiceCollection`
- the final service provider is available from `IFlourish.Services`
- application objects can be resolved with `flourish.GetRequiredService<T>()`
- `flourish.Run<App>()` starts the host, shows the shell, runs the WPF dispatcher, and stops the host when the application exits
- `flourish.Start()` and `flourish.StopAsync()` map to host lifecycle methods

```csharp
using var flourish = FlourishBuilder
    .CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<App>();
        services.AddSingleton<ICommandParser, AppCommandParser>();
    })
    .Build();

return flourish.Run<App>();
```

## Configuration areas

The public builder separates hosting, application services, feature switches, and feature-specific configuration.

| Feature | Builder method | Purpose |
| --- | --- | --- |
| [Application data](configure-data.md) | `ConfigureData` | Identifies application preference storage. |
| [Dependency injection](configure-services.md) | `ConfigureServices` | Registers application and replaceable Flourish services. |
| [Shell configuration](shell-configuration.md) | `ConfigureShell` | Enables or disables shell features. |
| [Profile](configure-profile.md) | `ConfigureProfile` | Configures the default identity, name order, and hosted profile page. |
| [Title bar](configure-title-bar.md) | `ConfigureTitleBar` | Configures title bar content and behavior. |
| [Navigation](navigation.md) | `ConfigureNavigation` | Configures the navigation panel and visible model. |
| [Custom shell content](configure-custom-handler.md) | `ConfigureCustomHandler` | Inserts custom WPF elements into shell regions. |
| [Dynamic toolbar](dynamic-toolbar.md) | `ConfigureDynamicToolbar` | Registers page-specific toolbar items. |
| [Tooltips](configure-tips.md) | `ConfigureTips` | Configures tooltip timing and placement. |
| [Motion](configure-motion.md) | `ConfigureMotion` | Configures transitions and hover animation. |
| [Window](configure-window.md) | `ConfigureWindow` | Configures shell window properties. |
| [Typography](configure-font.md) | `ConfigureFont` | Configures shell typography. |
| [Material effects](configure-material-effect.md) | `ConfigureMaterialEffect` | Selects the window material effect. |
| [Themes](configure-themes.md) | `ConfigureThemes` | Selects the default theme. |
| [Footer status](status-bar.md) | `ConfigureFooter` | Configures footer status content. |

Builder entry points can be called multiple times. Repeated callbacks for the same entry point are applied in registration order during `Build()`; direct value settings such as font, material, and theme use the last configured value.

## Register services

Use [Dependency injection](configure-services.md) for application services and replaceable Flourish services.

```csharp
builder.ConfigureServices((_, services) =>
{
    services.AddSingleton<App>();
    services.AddSingleton<ICommandParser, AppCommandParser>();
    services.AddSingleton<ImageLibrary>();
    services.AddTransient<EditorViewModel>();
});
```

Flourish registers its built-in services during build. Applications can resolve the public services from `IFlourish.Services` without constructing shell infrastructure directly.

## Register navigation pages

`AddNavigable` registers a WPF `Page` in dependency injection and records its display metadata, cache mode, and optional navigation key.

```csharp
services.AddNavigable<HomePage>(
    displayName: "Home",
    iconGlyph: "\uE80F",
    cacheMode: FlourishPageCacheMode.Enabled,
    navigationKey: NavigationRoutes.Home);
```

Registration does not determine where a page appears. [Navigation](navigation.md) is the canonical guide to page metadata, cache behavior, visible groups, fixed items, and runtime navigation.

## Build the runtime

`Build()` creates the host and returns `IFlourish`. After that, configuration is complete. For the common WPF application path, call `Run<App>()`; it starts the host, creates and shows the Flourish shell, enters the WPF dispatcher, and stops the host when the application exits.

```csharp
using var flourish = builder.Build();
return flourish.Run<App>();
```

Keep the runtime alive for as long as the WPF application is running.
