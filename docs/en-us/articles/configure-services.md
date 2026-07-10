---
title: Dependency injection
description: Register application services, navigable pages, command parsers, and profile services.
---

# Dependency injection

Flourish uses the service collection from its .NET Generic Host. Register application services, WPF pages, command parsers, and replaceable Flourish services through `ConfigureServices`.

## Register services

```csharp
builder.ConfigureServices((context, services) =>
{
    services.AddSingleton<App>();
    services.AddSingleton<ICommandParser, AppCommandParser>();

    services.AddNavigable<HomePage>("Home", "\uE80F", navigationKey: NavigationRoutes.Home);
    services.AddNavigable<ReportsPage>("Reports", "\uE9D2", navigationKey: NavigationRoutes.Reports);
});
```

The callback receives `HostBuilderContext`, so registrations can use the active environment and host configuration. View models, repositories, and other application services use the same `IServiceCollection` patterns as any Generic Host application.

## Register navigable pages

`AddNavigable` registers a `System.Windows.Controls.Page` and its navigation metadata. Registration makes the page resolvable; [Navigation](navigation.md) determines where it appears and which page is shown first.

Use a stable `navigationKey` when view models navigate through `INavigationService` without referencing page types.

## Replace Flourish services

Register `IProfileAuthService` to supply application authentication while keeping Flourish profile state and persistence. Register `IProfileService` when the application owns the complete profile workflow. Flourish supplies its defaults only when the application has not registered these interfaces.

## Related features

- [Navigation](navigation.md) places registered pages in the visible navigation model.
- [Dynamic toolbar](dynamic-toolbar.md) attaches commands to registered page types.
- [Profile](configure-profile.md) explains authentication and profile service replacement.
- [Command parser](command-parser.md) explains command-key routing.
