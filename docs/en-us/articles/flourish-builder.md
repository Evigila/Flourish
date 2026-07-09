---
title: IFlourishBuilder
description: Understand the builder, hosting integration, services, and page registration.
---

# IFlourishBuilder

`IFlourishBuilder` is the composition entry point for a Flourish application. It does not directly create windows or pages during configuration. Instead, it collects service registrations and shell options, then builds an `IFlourish` runtime backed by a .NET Generic Host.

## Hosting model

`FlourishBuilder.CreateDefaultBuilder(args)` uses `Host.CreateDefaultBuilder(args)` internally. The resulting runtime therefore follows the familiar hosting behavior of modern .NET applications:

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

## Builder stages

The public builder separates high-level feature switches from detailed configuration.

| Method | Purpose |
| --- | --- |
| [`ConfigureData`](configure-data.md) | Configures application identity and preference storage. |
| [`ConfigureServices`](configure-services.md) | Registers application services, pages, command parsers, view models, and infrastructure in DI. |
| [`ConfigureShell`](configure-shell.md) | Enables or disables shell features such as the title bar, navigation, dynamic toolbar, tips, motion, material effects, themes, and footer. |
| [`ConfigureTitleBar`](configure-title-bar.md) | Configures title bar content and behavior when the title bar is enabled. |
| [`ConfigureNavigation`](configure-navigation.md) | Configures navigation panel display, registered page positions, command items, groups, and fixed items. |
| [`ConfigureCustomHandler`](configure-custom-handler.md) | Inserts custom WPF elements into predefined shell regions. |
| [`ConfigureDynamicToolbar`](configure-dynamic-toolbar.md) | Registers page-specific toolbar items. |
| [`ConfigureTips`](configure-tips.md) | Configures tooltip delay and shell-edge spacing. |
| [`ConfigureMotion`](configure-motion.md) | Configures page transitions, navigation panel transitions, and hover reveal animations. |
| [`ConfigureWindow`](configure-window.md) | Configures shell window size, position, state, resize mode, taskbar visibility, and topmost behavior. |
| [`ConfigureFont`](configure-font.md) | Configures the shell font family and base size. |
| [`ConfigureMaterialEffect`](configure-material-effect.md) | Configures the material effect used when material effects are enabled. |
| [`ConfigureThemes`](configure-themes.md) | Configures the default theme used when themes are enabled. |
| [`ConfigureFooter`](configure-footer.md) | Configures the status area in the shell footer. |

Each method can be called multiple times. Flourish stores the callbacks and applies them during `Build()`.

## Register services

Use [`ConfigureServices`](configure-services.md) for anything that belongs to dependency injection.

```csharp
builder.ConfigureServices((_, services) =>
{
    services.AddSingleton<App>();
    services.AddSingleton<ICommandParser, AppCommandParser>();
    services.AddSingleton<ImageLibrary>();
    services.AddTransient<EditorViewModel>();
});
```

Flourish also registers its own internal services during build, including navigation, toolbar, footer status, message, tooltip, material effect, motion, page cache, and shell window services. Applications do not construct those services directly.

## Register pages with AddNavigable

`AddNavigable` is the recommended way to register a WPF `Page` for Flourish navigation. It registers the page in DI and records the display metadata used by page navigation items. It does not decide where the page appears in the navigation panel.

```csharp
services.AddNavigable<HomePage>(
    displayName: "Home",
    iconGlyph: "\uE80F",
    cacheMode: FlourishPageCacheMode.Enabled);
```

The generic overload is best when the page type is known at compile time.

```csharp
services.AddNavigable<SettingsPage>("Settings", "\uE713");
```

The `Type` overload is useful when page registrations come from configuration or plugins.

```csharp
services.AddNavigable(
    typeof(ReportPage),
    displayName: "Reports",
    iconGlyph: "\uE9D2",
    cacheMode: FlourishPageCacheMode.Disabled);
```

`displayName` is shown by `AddNavigableViewItem`. `iconGlyph` is typically a Segoe Fluent Icons glyph such as `"\uE80F"`. `cacheMode` controls whether the same page instance is reused.

Place registered pages in the visible navigation model with [`ConfigureNavigation`](configure-navigation.md). Navigation panel display settings such as direction, width, and initial open state are also configured there.

```csharp
builder.ConfigureNavigation(navigation =>
{
    navigation
        .SetInitiallyOpen()
        .SetGroup("Navigation", groupId: 0, group =>
        {
            group.AddNavigableViewItem<HomePage>(isInitial: true);
            group.AddNavigableViewItem<SettingsPage>();
        });

    navigation.AddFixedNavigableViewItem<ReportPage>();
});
```

`isInitial` belongs to the visible view item. This lets a page be registered once and later placed in the scrollable group area or fixed bottom area without mixing registration metadata with layout decisions.

## Page cache mode

Use `FlourishPageCacheMode.Enabled` for pages that should keep local UI state, scroll position, or loaded data while the user navigates away and back. Use `Disabled` for pages that should be recreated every time, such as editors that must always load fresh state.

```csharp
services.AddNavigable<DashboardPage>(
    "Dashboard",
    "\uE9D2",
    cacheMode: FlourishPageCacheMode.Enabled);

services.AddNavigable<ImportWizardPage>(
    "Import",
    "\uE8B5",
    cacheMode: FlourishPageCacheMode.Disabled);
```

The cache mode is attached to the registered page type, not to a specific navigation item. A page can only be displayed in one navigation position, but its cache behavior is still defined at registration time.

## Build the runtime

`Build()` creates the host and returns `IFlourish`. After that, configuration is complete. For the common WPF application path, call `Run<App>()`; it starts the host, creates and shows the Flourish shell, enters the WPF dispatcher, and stops the host when the application exits.

```csharp
using var flourish = builder.Build();
return flourish.Run<App>();
```

Keep the runtime alive for as long as the WPF application is running.
