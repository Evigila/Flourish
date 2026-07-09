---
title: Getting started
description: Apply Flourish to a WPF application with the shortest useful path.
---

# Getting started

The fastest way to use Flourish is to let the shell host a WPF `Application`: add the theme resources, build an `IFlourish` runtime from `App.xaml.cs` or another application entry point, register pages with `AddNavigable`, place them in the navigation model with [`ConfigureNavigation`](configure-navigation.md), then show the shell.

## Reference the theme

The `Run(Application)` helper and `IFlourish.Show(Application)` automatically merge `/Flourish;component/Themes/Generic.xaml` into the application resources before the shell is opened. The dictionary can still be added explicitly in `App.xaml`; this is useful for the WPF designer and for resources used before the shell is shown.

> [!NOTE]
> Add the theme dictionary in `App.xaml` when design-time resources or early application resources need Flourish styles before the shell is shown.

When the Flourish shell owns the main window, `App.xaml` should not set `StartupUri`. The shell becomes `Application.MainWindow` when `IFlourish.Show(Application)` runs.

```xml
<Application
  x:Class="MyApp.App"
  xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
  <Application.Resources>
    <ResourceDictionary>
      <ResourceDictionary.MergedDictionaries>
        <ResourceDictionary Source="/Flourish;component/Themes/Generic.xaml" />
      </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
  </Application.Resources>
</Application>
```

## Configure the application entry point

Most WPF applications can build and own the Flourish runtime from `App.xaml.cs`. The application starts the host, shows the shell, and releases the runtime when WPF exits.

```csharp
using System.Windows;
using AckSS.Flourish.Abstract;
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
                    .UseDynamicToolbar()
                    .UseTips()
                    .UseMotion()
                    .UseMaterialEffect()
                    .UseThemes();
            })
            .ConfigureTitleBar(titleBar =>
            {
                titleBar
                    .ShowLogo()
                    .ShowTitle()
                    .ShowSearch()
                    .ShowBreadcrumb()
                    .ShowNavToggle()
                    .SetTitle("My App")
                    .SetSubtitle("Flourish shell");
            })
            .ConfigureNavigation(navigation =>
            {
                navigation
                    .SetInitiallyOpen()
                    .SetPanelWidth(openWidth: 260, closedWidth: 48, maxWidth: 480, minWidth: 180)
                    .SetGroup("Navigation", groupId: 0, group =>
                    {
                        group.AddNavigableViewItem<HomePage>(isInitial: true);
                    });

                navigation.AddFixedNavigableViewItem<SettingsPage>();
            })
            .ConfigureTips(tips => tips.SetDelay(200).SetSpawnableMargin(5))
            .ConfigureFont("Microsoft YaHei")
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

`CreateDefaultBuilder(e.Args)` creates a standard .NET Generic Host. That means [`ConfigureServices`](configure-services.md) receives a normal `IServiceCollection`, so application services, pages, and command parsers use familiar dependency injection patterns.

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

## Configuration API pages

The startup example intentionally keeps each configuration responsibility separate:

- [`ConfigureServices`](configure-services.md) registers services, pages, and command parsers.
- [`ConfigureShell`](configure-shell.md) enables high-level shell features.
- [`ConfigureTitleBar`](configure-title-bar.md) configures the title bar.
- [`ConfigureNavigation`](configure-navigation.md) configures navigation panel display and visible items.
- [`ConfigureTips`](configure-tips.md), [`ConfigureFont`](configure-font.md), and [`ConfigureWindow`](configure-window.md) tune supporting shell behavior.

## Create pages

Pages registered with `AddNavigable` are regular WPF `Page` classes. Flourish resolves them from dependency injection when navigation occurs. The page display name, icon, and cache mode come from `AddNavigable`; the visible navigation position and initial page are configured in [`ConfigureNavigation`](configure-navigation.md).

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
- The shell surface needed by that page is enabled through [`ConfigureShell`](configure-shell.md).
- The runtime is disposed during application exit, or the builder shortcut `.Run<App>()` owns disposal.
