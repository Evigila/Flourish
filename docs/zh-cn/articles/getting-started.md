---
title: 快速开始
description: 用最短路径把 Flourish 接入 WPF 应用。
---

# 快速开始

使用 Flourish 最快的方式，是让 Flourish Shell 托管你的 WPF `Application`：引用主题资源，在 `Program.Main` 中构建 `IFlourish` 运行时，用 `AddNavigable` 注册页面，在 `ConfigureNavigation` 中放置导航项，然后通过 `flourish.Run<App>()` 运行应用。

## 引用主题资源

`Run(Application)` 快捷方法和 `IFlourish.Show(Application)` 会在打开 Shell 前自动把 `/Flourish;component/Themes/Generic.xaml` 合并进应用资源。你仍然可以在 `App.xaml` 中显式引用它；这样 WPF 设计器和 Shell 显示前就需要使用的资源也能正常工作。

> [!NOTE]
> 当设计器资源或早期应用资源需要在 Shell 显示前使用 Flourish 样式时，建议在 `App.xaml` 中显式加入主题字典。

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

## 创建 Program 入口

把构建好的运行时保存在静态属性中，这样 `App` 和页面都可以用同一个入口访问服务。

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

                services.AddNavigable<HomePage>("首页", "\uE80F");
                services.AddNavigable<SettingsPage>("设置", "\uE713");
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
                    .SetSubtitle("Flourish Shell");
            })
            .ConfigureNavigation(navigation =>
            {
                navigation
                    .SetInitiallyOpen()
                    .SetPanelWidth(openWidth: 260, closedWidth: 48, maxWidth: 480, minWidth: 180)
                    .SetGroup("导航", groupId: 0, group =>
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

`CreateDefaultBuilder(args)` 会创建标准的 .NET Generic Host。因此 `ConfigureServices` 拿到的是普通 `IServiceCollection`，你的应用服务、页面、命令解析器都可以按熟悉的依赖注入方式注册。

## 保持 App 简洁

现在 Flourish 的启动流程由 `Program.Main` 承接，`App.xaml.cs` 只需要初始化 XAML 资源。

> [!WARNING]
> 使用 `flourish.Run<App>()` 后，不要再在 `App.OnStartup` 中调用 `Program.Flourish.Show(this)`。两条路径都会显示 Shell，同时使用会造成重复启动行为。

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

## 创建页面

通过 `AddNavigable` 注册的页面就是普通 WPF `Page`。导航发生时，Flourish 会从依赖注入容器解析页面实例。页面显示名称、图标和缓存模式来自 `AddNavigable`；可见导航位置和初始页则在 `ConfigureNavigation` 中配置。

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

## 首次运行检查清单

- `App` 已通过 `services.AddSingleton<App>()` 注册。
- 至少一个页面已通过 `AddNavigable` 注册。
- 至少一个可见页面项已通过 `AddNavigableViewItem` 添加，最好指定 `isInitial: true`。
- `Program.Main` 调用了 `flourish.Run<App>()`。
- `finally` 中调用了 `Dispose()`，或者直接使用 builder 快捷入口 `.Run<App>()`。
