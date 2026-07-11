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

    services.AddNavigable<HomePage>("Home", "\uE80F");
    services.AddNavigable<ReportsPage>("Reports", "\uE9D2");
});
```

The callback receives `HostBuilderContext`, so registrations can use the active environment and the same Host configuration that supplies Flourish settings. `CreateDefaultBuilder` loads the standard appsettings providers, while an application `<UserSecretsId>` identifies the User Secrets document used for protected remembered Profile credentials. View models, repositories, and other application services use the same `IServiceCollection` patterns as any Generic Host application.

## Register navigable pages

`AddNavigable<TPage>` registers a `System.Windows.Controls.Page` and its navigation metadata. Flourish generates the case-sensitive navigation key from the class name by removing one trailing `Page` suffix: `SettingsPage` becomes `Settings`, while `Page1` remains `Page1`. Registration makes the page resolvable; [Navigation](navigation.md) determines where it appears and which page is shown first.

View models navigate with that generated string, for example `navigation.Navigate("Settings")`, without referencing the WPF page. Flourish rejects duplicate generated keys when `Build()` applies the registrations.

## Replace Flourish services

Register `IProfileAuthService` to supply application authentication while keeping Flourish profile state and persistence. Register `IProfileService` when the application owns the complete profile workflow. Flourish supplies its defaults only when the application has not registered these interfaces.

## Related features

- [Navigation](navigation.md) places registered pages in the visible navigation model.
- [Dynamic toolbar](dynamic-toolbar.md) attaches commands to registered page types.
- [Profile](configure-profile.md) explains authentication and profile service replacement.
- [Command parser](command-parser.md) explains command-key routing.
