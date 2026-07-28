---
title: IFlourishBuilder
description: Understand the builder, hosting integration, services, and page registration.
---

# IFlourishBuilder

`IFlourishBuilder` is the composition entry point for a Flourish application. It does not directly create windows or pages during configuration. Instead, it collects service registrations and shell options, then builds an `IFlourish` runtime backed by a .NET Generic Host.

Builder APIs are declared in `ArkheideSystem.Flourish.Abstract.Builder`. Shared models remain
in `ArkheideSystem.Flourish.Abstract`.

## Hosting model

`FlourishBuilder.CreateDefaultBuilder(args)` follows the .NET Generic Host default configuration and lifetime model:

- configuration and environment are available through `HostBuilderContext` in `ConfigServices`
- Flourish settings use the standard appsettings and User Secrets sources from that Host configuration
- services are registered in `IServiceCollection`
- the final service provider is available from `IFlourish.Services`
- application objects can be resolved with `flourish.GetRequiredService<T>()`
- `flourish.Run<App>()` starts the host, shows the shell, runs the WPF dispatcher, and stops the host when the application exits
- `flourish.Start()` and `flourish.StopAsync()` map to host lifecycle methods

```csharp
using var flourish = FlourishBuilder
    .CreateDefaultBuilder(args)
    .ConfigServices((context, services) =>
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
| [Application data](configure-data.md) | `ConfigData` | Configures localization and the writable settings and project-catalog paths. |
| [Application configuration](configure-data.md) | `ConfigAppConfiguration` | Adds JSON files or other Microsoft configuration providers. |
| [Dependency injection](configure-services.md) | `ConfigServices` | Registers application and replaceable Flourish services. |
| [Shell configuration](shell-configuration.md) | `ConfigShell` | Configures shell surfaces, tooltips, typography, and material effects. |
| [Profile](configure-profile.md) | `ConfigProfile` | Selects a custom page for the profile enabled by the title bar. |
| [Title bar](configure-title-bar.md) | `ConfigTitleBar` | Configures title bar content and behavior. |
| [Projects](projects.md) | `ConfigShell`, `IProjectService`, `IProjectBehavior` | Enables project-aware title display, persists its metadata catalog, and provides a replaceable lifecycle. |
| [Navigation](navigation.md) | `ConfigNavigation` | Configures the navigation panel and visible model. |
| [Custom shell content](configure-custom-handler.md) | `ConfigCustomHandler` | Inserts custom WPF elements into shell regions. |
| [Dynamic toolbar](dynamic-toolbar.md) | `ConfigDynamicToolbar` | Registers page-specific toolbar items. |
| [Background tasks](background-tasks.md) | `IBackgroundTaskService` | Submits bounded, cancellable asynchronous work. |
| [Tooltips](configure-tips.md) | `ConfigShell` | Selects and configures the Flourish presentation for Flourish-owned tooltips with `UseTips`. |
| [Motion](configure-motion.md) | `ConfigMotion` | Configures transitions and hover animation. |
| [Window](configure-window.md) | `ConfigWindow` | Configures shell window properties and behavior. |
| [Typography](configure-font.md) | `ConfigShell` | Configures shell typography with `InitGlobalFont`. |
| [Material effects](configure-material-effect.md) | `ConfigShell` | Applies the window material with `UseMaterialEffect`. |
| [Themes](configure-themes.md) | `ConfigShell`, `ConfigTitleBar` | Configures application colors and corner radius, and enables theme selection with `UseThemeToggle`. |
| [Status bar](status-bar.md) | `ConfigStatusBar` | Configures custom status items and the consolidated system-status entry. |

Builder entry points can be called multiple times before `Build()`. Repeated callbacks for the same entry point are applied in registration order; repeated setting methods use the last configured value.

`ConfigData` callbacks run before the Host is built so the selected settings file participates in the final `IConfiguration` that is available to `ConfigServices`. `ConfigAppConfiguration` callbacks then add application-owned sources through the standard Microsoft configuration builder.

`Build()` consumes and freezes the builder. Calling `Build()` again, adding another `Config...`
callback, or invoking a captured nested builder after its callback has ended throws
`InvalidOperationException`. This keeps dependency injection, creation-time window settings,
and the initial object graph fixed after composition.

## Register services

Use [Dependency injection](configure-services.md) for application services and replaceable Flourish services.

```csharp
builder.ConfigServices((_, services) =>
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

Registering a page makes it available to navigation but does not add a visible item. Place it explicitly with `ConfigNavigation`:

```csharp
builder.ConfigNavigation(navigation =>
    navigation.InitGroup(null, groupId: 0, group =>
        group.InitNavigableViewItem<HomePage>(isInitial: true)));
```

[Navigation](navigation.md) explains generated keys, page metadata, cache behavior, groups, fixed items, validation, and runtime string navigation.

## Build the runtime

`Build()` creates the host and returns `IFlourish`. After that, the one-shot configuration is frozen. For the common WPF application path, call `Run<App>()`; it starts the host, creates and shows the Flourish shell, enters the WPF dispatcher, and stops the host when the application exits.

```csharp
using var flourish = builder.Build();
return flourish.Run<App>();
```

Keep the runtime alive for as long as the WPF application is running.
