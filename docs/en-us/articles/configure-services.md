---
title: Dependency injection
description: Register application services, navigable pages, and replaceable Flourish services.
---

# Dependency injection

Use `ConfigureServices` to register application services, WPF pages, and replaceable Flourish services with the Generic Host.

## Register services

```csharp
builder.ConfigureServices((context, services) =>
{
    services.AddSingleton<App>();
    services.AddSingleton<ReportExporter>();
    services.AddCommandParser<ReportCommands>();

    services.AddNavigable<HomePage>("Home", "\uE80F");
    services.AddNavigable<ReportsPage>("Reports", "\uE9D2");
});
```

The callback receives `HostBuilderContext`, including the active environment and Host configuration.

`IBackgroundTaskService` is already registered and can be received through constructor injection. See [Background tasks](background-tasks.md).

## Register navigable pages

`AddNavigable<TPage>` registers a `System.Windows.Controls.Page` and its navigation metadata. Flourish generates the case-sensitive navigation key from the class name by removing one trailing `Page` suffix: `SettingsPage` becomes `Settings`, while `Page1` remains `Page1`.

Page registration does not create a visible navigation item. Use `ConfigureNavigation` to place each page in a group or the fixed area. [Navigation](navigation.md) explains visible positions and initial-page selection.

View models can navigate with the generated key, for example `navigation.Navigate("Settings")`, without referencing the WPF page type. `Build()` rejects duplicate generated keys.

## Supply command dependencies

Add host-lifetime `ICommandParser` mappings with `AddCommandParser<TParser>`. Use `ICommandRegistry` for shorter or dynamic lifetimes; see [Command dispatch](commands.md).

## Replace profile services

Register `IProfileAuthService` to provide application authentication while retaining the built-in profile state and remembered-login behavior. Register `IProfileService` when the application owns the complete profile workflow. Flourish provides default implementations only when the application has not registered these interfaces.

## Replace project behavior

Register one singleton `IProjectBehavior` when the application owns project dialogs or file lifecycle. Flourish provides its `.txt` placeholder-file behavior only when the application has not registered this interface.

```csharp
builder.ConfigureServices((_, services) =>
    services.AddSingleton<IProjectBehavior, WorkspaceProjectBehavior>());
```

In multi-project mode, the Shell calls `CreateProjectAsync`, `SaveActiveProjectAsync`, `ActivateProjectAsync`, `DeleteProjectAsync`, and `CanCloseAsync`. Return `false` to cancel the operation. These entry points are inactive outside project mode.

Replacing `IProjectBehavior` changes dialogs and files, not the catalog. Publish metadata and selection through `IProjectService`; Flourish writes valid existing-file mappings atomically to `SetProjectCatalogFilePath`. See [Projects](projects.md).

## Related features

- [Navigation](navigation.md)
- [Dynamic toolbar](dynamic-toolbar.md)
- [Profile](configure-profile.md)
- [Projects](projects.md)
- [Background tasks](background-tasks.md)
- [Command dispatch](commands.md)
