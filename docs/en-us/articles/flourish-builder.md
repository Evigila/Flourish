---
title: IFlourishBuilder
description: Understand the builder, hosting integration, services, and page registration.
---

# IFlourishBuilder

`IFlourishBuilder` is the composition entry point for a Flourish application. It does not directly create windows or pages during configuration. Instead, it collects service registrations and shell options, then builds an `IFlourish` runtime backed by a .NET Generic Host.

## Hosting model

`FlourishBuilder.CreateDefaultBuilder(args)` follows the .NET Generic Host default configuration and lifetime model:

- configuration and environment are available through `HostBuilderContext` in `ConfigureServices`
- Flourish settings use the standard appsettings and User Secrets sources from that Host configuration
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
    })
    .Build();

return flourish.Run<App>();
```

## Configuration areas

The public builder separates hosting, application services, feature switches, and feature-specific configuration.

| Feature | Builder method | Purpose |
| --- | --- | --- |
| [Application data](configure-data.md) | `ConfigureData` | Configures localization and explains the shared Host configuration. |
| [Dependency injection](configure-services.md) | `ConfigureServices` | Registers application and replaceable Flourish services. |
| [Shell configuration](shell-configuration.md) | `ConfigureShell` | Configures shell surfaces, tooltips, typography, and material effects. |
| [Profile](configure-profile.md) | `ConfigureProfile` | Selects a custom page for the profile enabled by the title bar. |
| [Title bar](configure-title-bar.md) | `ConfigureTitleBar` | Configures title bar content and behavior. |
| [Projects](projects.md) | `ConfigureShell`, `IProjectService`, `IProjectBehavior` | Enables project-aware title display, persists its metadata catalog, and provides a replaceable lifecycle. |
| [Navigation](navigation.md) | `ConfigureNavigation` | Configures the navigation panel and visible model. |
| [Custom shell content](configure-custom-handler.md) | `ConfigureCustomHandler` | Inserts custom WPF elements into shell regions. |
| [Dynamic toolbar](dynamic-toolbar.md) | `ConfigureDynamicToolbar` | Registers page-specific toolbar items. |
| [Background tasks](background-tasks.md) | `IBackgroundTaskService` | Submits bounded, cancellable asynchronous work. |
| [Tooltips](configure-tips.md) | `ConfigureShell` | Selects and configures the Flourish presentation for Flourish-owned tooltips with `UseTips`. |
| [Motion](configure-motion.md) | `ConfigureMotion` | Configures transitions and hover animation. |
| [Window](configure-window.md) | `ConfigureWindow` | Configures shell window properties and behavior. |
| [Typography](configure-font.md) | `ConfigureShell` | Configures shell typography with `UseGlobalFont`. |
| [Material effects](configure-material-effect.md) | `ConfigureShell` | Applies the window material with `UseMaterialEffect`. |
| [Themes](configure-themes.md) | `ConfigureShell`, `ConfigureTitleBar` | Configures application colors and corner radius, and enables theme selection with `SetThemeToggle`. |
| [Status bar](status-bar.md) | `ConfigureStatusBar` | Configures custom status items and the consolidated system-status entry. |

Builder entry points can be called multiple times. Repeated callbacks for the same entry point are applied in registration order during `Build()`; repeated setting methods use the last configured value.

## Register services

Use [Dependency injection](configure-services.md) for application services and replaceable Flourish services.

```csharp
builder.ConfigureServices((_, services) =>
{
    services.AddSingleton<App>();
    services.AddSingleton<ImageLibrary>();
    services.AddTransient<EditorViewModel>();
});
```

After `Build()`, applications can resolve public services from `IFlourish.Services`.

This includes `IBackgroundTaskService`, `IProjectService`, and `IProjectBehavior`. Resolve them through dependency injection to submit asynchronous work, mutate the persistent project catalog, or invoke project lifecycle behavior. Applications can register their own singleton `IProjectBehavior` before `Build()` to replace the default dialog and `.txt` file workflow. See [Background tasks](background-tasks.md) and [Projects](projects.md).

## Register navigation pages

`AddNavigable<TPage>` registers a WPF `Page` in dependency injection. Flourish derives its case-sensitive navigation key from the page class name by removing one trailing `Page` suffix.

```csharp
services.AddNavigable<HomePage>(
    displayName: "Home",
    iconGlyph: "\uE80F",
    cacheMode: FlourishPageCacheMode.Enabled);
```

Registering a page makes it available to navigation but does not add a visible item. Place it explicitly with `ConfigureNavigation`:

```csharp
builder.ConfigureNavigation(navigation =>
    navigation.SetGroup(null, groupId: 0, group =>
        group.AddNavigableViewItem<HomePage>(isInitial: true)));
```

[Navigation](navigation.md) explains generated keys, page metadata, cache behavior, groups, fixed items, validation, and runtime string navigation.

## Build the runtime

`Build()` creates the host and returns `IFlourish`. After that, configuration is complete. For the common WPF application path, call `Run<App>()`; it starts the host, creates and shows the Flourish shell, enters the WPF dispatcher, and stops the host when the application exits.

```csharp
using var flourish = builder.Build();
return flourish.Run<App>();
```

Keep the runtime alive for as long as the WPF application is running.
