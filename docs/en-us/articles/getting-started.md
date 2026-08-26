---
title: Getting started
description: Build and run a basic WPF application with Flourish.
---

# Getting started

Install or reference `Arkheide.Flourish`, then import the single public namespace:

```csharp
using ArkheideSystem.Flourish.Abstract;
```

Do not set `StartupUri` when the Flourish shell is the main window. Register the WPF application, configure the title bar and navigation, then run the host:

```csharp
return FlourishBuilder
    .CreateDefaultBuilder(args)
    .ConfigureServices((_, services) => services.AddSingleton<App>())
    .ConfigureTitleBar(titleBar =>
        titleBar
            .SetEnabled()
            .SetApplicationTitle("Foobar")
            .SetNavigationToggle())
    .ConfigureNavigation(navigation =>
        navigation
            .SetEnabled()
            .AddNavigable<HomePage>(
                displayName: "Home",
                iconGlyph: "\uE80F",
                isInitial: true))
    .Run<App>();
```

`AddNavigable<TPage>` registers the page, creates its route, and adds its visible navigation item. Use `AddGroup` or the fixed-item methods when the application needs an explicit hierarchy.

Flourish text defaults to `en-US`. Select another built-in or custom locale before build:

```csharp
builder.ConfigureData(data =>
    data.SetLocale("zh-CN")
        .AddCultureFile("Locales/FlourishCulture.Json"));
```

After startup, resolve runtime services through dependency injection. Builders define initial state; services update live state. For example, read `theme.Current`, observe `theme.Changed`, and call `theme.SetTheme(...)`.

Flourish uses standard Microsoft `IConfiguration`. Inject it to read effective values. Inject `IFlourishSettingsStore` when the application must atomically update the `Flourish:` section in `appsettings.Flourish.json`.

Continue with [IFlourishBuilder](flourish-builder.md), [feature configuration](shell-configuration.md), and [runtime APIs](runtime-apis.md).
