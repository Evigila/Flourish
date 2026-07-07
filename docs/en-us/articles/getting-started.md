---
title: Getting started
description: Apply Flourish to a WPF application with the shortest useful path.
---

# Getting started

The fastest way to use Flourish is to let the shell host your WPF `Application`: add the theme resources, build an `IFlourish` runtime in `Program.Main`, register pages with `AddNavigable`, place them in the navigation model with `ConfigureNavigation`, then run the application with `flourish.Run<App>()`.

## Reference the theme

The `Run(Application)` helper and `IFlourish.Show(Application)` automatically merge `/Flourish;component/Themes/Generic.xaml` into the application resources before the shell is opened. You can still add it explicitly in `App.xaml`; this is useful for the WPF designer and for resources used before the shell is shown.

> [!NOTE]
> Add the theme dictionary in `App.xaml` when design-time resources or early application resources need Flourish styles before the shell is shown.

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

## Create a Program entry point

Store the built runtime in a static property so `App` and pages can reach services consistently.

```csharp
using AckSS.Flourish.Abstract;
using Microsoft.Extensions.DependencyInjection;

namespace MyApp;

internal static class Program
{
    private static IFlourish? flourish;

    public static IFlourish Flourish =>
        flourish ?? throw new InvalidOperationException("Flourish has not been built.");

    [STAThread]
    public static int Main(string[] args)
    {
        flourish = FlourishBuilder
            .CreateDefaultBuilder(args)
            .ConfigureServices((_, services) =>
            {
                services.AddSingleton<App>();
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
            .ConfigureTips(tips => tips.SetDelay(600).SetSpawnableMargin(5))
            .ConfigureFont("Microsoft YaHei")
            .ConfigureWindow(window => window.SetWindowSize(1280, 720).SetWindowMinSize(960, 540))
            .Build();

        try
        {
            return flourish.Run<App>();
        }
        finally
        {
            flourish.Dispose();
            flourish = null;
        }
    }
}
```

`CreateDefaultBuilder(args)` creates a standard .NET Generic Host. That means `ConfigureServices` receives a normal `IServiceCollection`, so your application services, pages, and command parsers all use familiar dependency injection patterns.

## Keep App simple

`Program.Main` now owns the Flourish startup flow, so `App.xaml.cs` only needs to initialize the XAML resources.

> [!WARNING]
> When `flourish.Run<App>()` is used, do not also call `Program.Flourish.Show(this)` from `App.OnStartup`. Both paths show the shell, so using both creates duplicate startup behavior.

```csharp
using System.Windows;

namespace MyApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }
}
```

## Create pages

Pages registered with `AddNavigable` are regular WPF `Page` classes. Flourish resolves them from dependency injection when navigation occurs. The page display name, icon, and cache mode come from `AddNavigable`; the visible navigation position and initial page are configured in `ConfigureNavigation`.

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

- `App` is registered with `services.AddSingleton<App>()`.
- At least one page is registered with `AddNavigable`.
- At least one visible page item is added with `AddNavigableViewItem`, preferably with `isInitial: true`.
- `Program.Main` calls `flourish.Run<App>()`.
- `Dispose()` is called in `finally`, or you use the builder shortcut `.Run<App>()`.
