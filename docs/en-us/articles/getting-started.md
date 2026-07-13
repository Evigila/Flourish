---
title: Getting started
description: Build and run a basic WPF application with Flourish.
---

# Getting started

A basic Flourish application lets the shell host a WPF `Application`: add the theme resources, build an `IFlourish` runtime from `App.xaml.cs` or another application entry point, register pages with `AddNavigable`, place them in the [navigation model](navigation.md), then show the shell.

Built-in Flourish text uses the English locale by default, including when `ConfigureData` is omitted. To use Chinese, add `builder.ConfigureData(data => data.SetLocale("CN"))` before `Build()`. [Application data](configure-data.md) explains built-in and custom locale files.

## Reference the controls and theme

The `Run(Application)` helper and `IFlourish.Show(Application)` automatically load the complete Flourish control and theme resource graph before the shell is opened. `FlourishThemeResources` can also be added explicitly in `App.xaml`; this is useful for the WPF designer, resources used before the shell is shown, and applications that use the [control library](control-library.md) independently from the shell.

> [!NOTE]
> Add `FlourishThemeResources` in `App.xaml` when design-time or early application resources need Flourish controls before the shell is shown. The former `FlourishStyles` and `FlourishControlResources` types are obsolete compatibility shims and should not be used by new applications.

When the Flourish shell owns the main window, `App.xaml` should not set `StartupUri`. The shell becomes `Application.MainWindow` when `IFlourish.Show(Application)` runs.

```xml
<Application
  x:Class="MyApp.App"
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

Most WPF applications can build and own the Flourish runtime from `App.xaml.cs`. The application starts the host, shows the shell, and releases the runtime when WPF exits.

```csharp
using System.Windows;
using ArkheideSystem.Flourish.Abstract;
using Microsoft.Extensions.DependencyInjection;

namespace MyApp;

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
                services.AddSingleton<ICommandParser, AppCommandParser>();

                services.AddNavigable<HomePage>("Home", "\uE80F");
                services.AddNavigable<SettingsPage>("Settings", "\uE713");
            })
            .ConfigureShell(shell =>
            {
                shell
                    .UseTitleBar()
                    .UseNavigation()
                    .UseTips(200)
                    .UseMaterialEffect(MaterialEffect.Mica)
                    .UseGlobalFont("Segoe UI");
            })
            .ConfigureTitleBar(titleBar =>
            {
                titleBar
                    .SetTitle("Foobar")
                    .SetSubTitle("Desktop workspace")
                    .SetSearch("Search", (_, searchText) => { })
                    .SetBreadcrumbButton()
                    .SetNavToggle();
            })
            .ConfigureNavigation(navigation =>
            {
                navigation
                    .SetInitiallyOpen()
                    .SetPanelWidth(openWidth: 260, closedWidth: 56, maxWidth: 480, minWidth: 180)
                    .SetGroup("Navigation", groupId: 0, group =>
                    {
                        group.AddNavigableViewItem<HomePage>(isInitial: true);
                    });

                navigation.AddFixedNavigableViewItem<SettingsPage>();
            })
            .ConfigureWindow(window => window.SetWindowSize(1280, 720).SetWindowMinSize(960, 540))
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

`CreateDefaultBuilder(e.Args)` creates a standard .NET Generic Host. [Dependency injection](configure-services.md) uses a normal `IServiceCollection`, so application services, pages, and command parsers follow familiar registration patterns.

## Alternative startup paths

Some applications use a custom generated entry point or a dedicated bootstrapper. Those entry points can build the same configured builder and call the `Run<App>()` shortcut.

```csharp
return FlourishBuilder
    .CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
    {
        services.AddSingleton<App>();
        services.AddNavigable<HomePage>("Home", "\uE80F");
    })
    .ConfigureShell(shell => shell.UseTitleBar().UseNavigation())
    .ConfigureNavigation(navigation =>
    {
        navigation.SetGroup("Navigation", groupId: 0, group =>
        {
            group.AddNavigableViewItem<HomePage>(isInitial: true);
        });
    })
    .Run<App>();
```

An application should use either the `App.xaml.cs` lifetime path or the `Run<App>()` shortcut for a given launch flow. Using both would show the shell twice.

## Explore features

The startup example keeps each feature responsibility separate:

- [Dependency injection](configure-services.md) registers services, pages, and command parsers.
- [Application data](configure-data.md) selects the built-in interface language and registers custom locale files.
- [Shell configuration](shell-configuration.md) enables shell features and explains their prerequisites.
- [Title bar](configure-title-bar.md) configures application identity and title bar controls.
- [Navigation](navigation.md) configures panel display and visible items.
- [Background tasks](background-tasks.md) runs cancellable asynchronous work and integrates its state with the status bar.
- [Tooltips](configure-tips.md), [Typography](configure-font.md), and [Window](configure-window.md) tune supporting shell behavior.

## Create pages

Pages registered with `AddNavigable` are regular WPF `Page` classes. Flourish resolves them from dependency injection when navigation occurs. The page display name, icon, and cache mode come from `AddNavigable`; [Navigation](navigation.md) configures the visible position and initial page.

```csharp
using System.Windows.Controls;

namespace MyApp;

public partial class HomePage : Page
{
    public HomePage()
    {
        InitializeComponent();
    }
}
```

## First run checklist

- The WPF application starts Flourish from `App.xaml.cs` or another application entry point.
- At least one page is registered with `AddNavigable`.
- At least one visible page item is added with `AddNavigableViewItem`, preferably with `isInitial: true`.
- The shell surface needed by that page is enabled through [Shell configuration](shell-configuration.md).
- The runtime is disposed during application exit, or the builder shortcut `.Run<App>()` owns disposal.
