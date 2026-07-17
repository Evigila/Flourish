---
title: Getting started
description: Build and run a basic WPF application with Flourish.
---

# Getting started

A basic Flourish application registers a WPF page, enables navigation, builds an `IFlourish` runtime, and displays the shell.

Built-in Flourish text uses the English locale by default. To use Chinese, call `builder.ConfigureData(data => data.SetLocale("CN"))` before `Build()`. [Application data](configure-data.md) explains built-in and custom locales.

## Reference the controls and theme

`Run(Application)` and `IFlourish.Show(Application)` load the Flourish control and theme resources before the shell opens. Add `FlourishThemeResources` explicitly in `App.xaml` when the WPF designer, content shown before the shell, or standalone [control library](control-library.md) usage needs those resources.

When the Flourish shell is the main window, do not set `StartupUri` in `App.xaml`.

```xml
<Application
  x:Class="Foobar.App"
  xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
  xmlns:flourish="http://schemas.arkheide.system/flourish"
  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
  <Application.Resources>
    <ResourceDictionary>
      <ResourceDictionary.MergedDictionaries>
        <flourish:FlourishThemeResources />
      </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
  </Application.Resources>
</Application>
```

## Configure the application entry point

The application can own the Flourish runtime from `App.xaml.cs`. Start the Host, show the shell, and release the runtime when WPF exits.

```csharp
using System.Windows;
using ArkheideSystem.Flourish.Abstract;
using Microsoft.Extensions.DependencyInjection;

namespace Foobar;

public partial class App : Application
{
    private static IFlourish? flourish;

    public static IFlourish Flourish =>
        flourish ?? throw new InvalidOperationException("Flourish has not been built.");

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        flourish = FlourishBuilder
            .CreateDefaultBuilder(e.Args)
            .ConfigureServices((_, services) =>
            {
                services.AddSingleton(this);
                services.AddNavigable<HomePage>("Home", "\uE80F");
            })
            .ConfigureShell(shell =>
                shell.UseTitleBar().UseNavigation())
            .ConfigureTitleBar(titleBar =>
                titleBar.SetApplicationTitle("Foobar").SetNavToggle())
            .ConfigureNavigation(navigation =>
                navigation.SetGroup(null, groupId: 0, group =>
                    group.AddNavigableViewItem<HomePage>(isInitial: true)))
            .Build();

        flourish.Start();
        flourish.Show(this);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (flourish is not null)
        {
            flourish.StopAsync().GetAwaiter().GetResult();
            flourish.Dispose();
            flourish = null;
        }

        base.OnExit(e);
    }
}
```

The navigation group explicitly places `HomePage` in the panel and selects it as the initial page. Use [Navigation](navigation.md) to add more groups, fixed items, command items, or parent-child relationships.

## Alternative startup path

A custom entry point or bootstrapper can build the same configuration and use the `Run<App>()` shortcut.

```csharp
return FlourishBuilder
    .CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
    {
        services.AddSingleton<App>();
        services.AddNavigable<HomePage>("Home", "\uE80F");
    })
    .ConfigureShell(shell =>
        shell.UseTitleBar().UseNavigation())
    .ConfigureTitleBar(titleBar =>
        titleBar.SetApplicationTitle("Foobar").SetNavToggle())
    .ConfigureNavigation(navigation =>
        navigation.SetGroup(null, groupId: 0, group =>
            group.AddNavigableViewItem<HomePage>(isInitial: true)))
    .Run<App>();
```

Use either the `App.xaml.cs` lifetime path or `Run<App>()` for a launch flow.

## Create a page

Pages registered with `AddNavigable` are WPF `Page` classes. Flourish resolves them from dependency injection when navigation occurs.

```csharp
using System.Windows.Controls;

namespace Foobar;

public partial class HomePage : Page
{
    public HomePage()
    {
        InitializeComponent();
    }
}
```

## Explore features

- [Dependency injection](configure-services.md) registers application services and pages.
- [Application data](configure-data.md) selects the built-in interface language and registers custom locales.
- [Shell configuration](shell-configuration.md) enables shell features and explains their prerequisites.
- [Title bar](configure-title-bar.md) configures title bar content.
- [Projects](projects.md) adds project-aware title semantics and runtime display metadata.
- [Navigation](navigation.md) places registered pages and command items in explicit navigation groups.
- [Background tasks](background-tasks.md) runs cancellable asynchronous work.
- [Tooltips](configure-tips.md), [Typography](configure-font.md), and [Window](configure-window.md) configure supporting shell behavior.

## First run checklist

- The WPF application starts Flourish from `App.xaml.cs` or another application entry point.
- At least one page is registered with `AddNavigable`.
- Navigation is enabled, and each visible page is placed in a group or the fixed area.
- The runtime is disposed during application exit, or `Run<App>()` owns its lifetime.
