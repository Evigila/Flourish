---
title: Dependency injection
description: Register application services, navigable pages, and replaceable Flourish services.
---

# Dependency injection

Flourish uses the `IServiceCollection` from its .NET Generic Host. Register application services, WPF pages, and replaceable Flourish services through `ConfigureServices`.

## Register services

```csharp
builder.ConfigureServices((context, services) =>
{
    services.AddSingleton<App>();
    services.AddSingleton<ReportExporter>();

    services.AddNavigable<HomePage>("Home", "\uE80F");
    services.AddNavigable<ReportsPage>("Reports", "\uE9D2");
});
```

The callback receives `HostBuilderContext`, so registrations can use the active environment and the same Host configuration that supplies Flourish settings. View models, repositories, and other application services use the standard .NET dependency injection patterns.

`IBackgroundTaskService` is already registered and can be received through constructor injection. See [Background tasks](background-tasks.md).

## Register navigable pages

`AddNavigable<TPage>` registers a `System.Windows.Controls.Page` and its navigation metadata. Flourish generates the case-sensitive navigation key from the class name by removing one trailing `Page` suffix: `SettingsPage` becomes `Settings`, while `Page1` remains `Page1`.

Page registration does not create a visible navigation item. Use `ConfigureNavigation` to place each page in a group or the fixed area. [Navigation](navigation.md) explains visible positions and initial-page selection.

View models can navigate with the generated key, for example `navigation.Navigate("Settings")`, without referencing the WPF page type. `Build()` rejects duplicate generated keys.

## Supply command dependencies

Register the application services used by command handlers through `ConfigureServices`. After the runtime is built, resolve `ICommandRegistry` and call `Register` for each command key. [Command dispatch](commands.md) explains handler registration, availability, results, and disposal.

## Replace profile services

Register `IProfileAuthService` to provide application authentication while retaining the built-in profile state and remembered-login behavior. Register `IProfileService` when the application owns the complete profile workflow. Flourish provides default implementations only when the application has not registered these interfaces.

## Related features

- [Navigation](navigation.md) explains explicit navigation groups and fixed items.
- [Dynamic toolbar](dynamic-toolbar.md) attaches commands to registered page types.
- [Profile](configure-profile.md) explains authentication and profile service replacement.
- [Background tasks](background-tasks.md) explains asynchronous work, cancellation, progress, and results.
- [Command dispatch](commands.md) explains command-key routing.
