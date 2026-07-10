---
title: 快速开始
description: 使用 Flourish 构建并运行一个基本 WPF 应用。
---

# 快速开始

基本的 Flourish 应用由 Shell 托管 WPF `Application`：引用主题资源，在 `App.xaml.cs` 或其他应用起始点构建 `IFlourish` 运行时，用 `AddNavigable` 注册页面，在[导航](navigation.md)配置中放置导航项，然后显示 Shell。

## 引用主题资源

`Run(Application)` 快捷方法和 `IFlourish.Show(Application)` 会在打开 Shell 前自动把 `/Flourish;component/Themes/Generic.xaml` 合并进应用资源。你仍然可以在 `App.xaml` 中显式引用它；这样 WPF 设计器和 Shell 显示前就需要使用的资源也能正常工作。

> [!NOTE]
> 当设计器资源或早期应用资源需要在 Shell 显示前使用 Flourish 样式时，建议在 `App.xaml` 中显式加入主题字典。

当 Flourish Shell 承担主窗口职责时，`App.xaml` 不应设置 `StartupUri`。`IFlourish.Show(Application)` 运行后，Shell 会成为 `Application.MainWindow`。

```xml
<Application
  x:Class="Foobar.App"
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

## 配置应用起始点

大多数 WPF 应用可以在 `App.xaml.cs` 中构建并持有 Flourish 运行时。应用启动 Host、显示 Shell，并在 WPF 退出时释放运行时。

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
                services.AddSingleton<ICommandParser, AppCommandParser>();

                services.AddNavigable<HomePage>("首页", "\uE80F");
                services.AddNavigable<SettingsPage>("设置", "\uE713");
            })
            .ConfigureShell(shell =>
            {
                shell
                    .UseTitleBar()
                    .UseNavigation()
                    .UseTips()
                    .UseMaterialEffect();
            })
            .ConfigureTitleBar(titleBar =>
            {
                titleBar
                    .ShowLogo()
                    .ShowTitle()
                    .ShowSubTitle()
                    .ShowSearch()
                    .ShowBreadcrumb()
                    .ShowNavToggle()
                    .SetTitle("Foobar")
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

`CreateDefaultBuilder(e.Args)` 会创建标准的 .NET Generic Host。因此[依赖注入](configure-services.md)配置拿到的是普通 `IServiceCollection`，应用服务、页面和命令解析器都可以按标准依赖注入方式注册。

## 其他起始点

部分应用会使用自定义生成入口或专门的 bootstrapper。这类起始点可以构建同一个 builder，并调用 `Run<App>()` 快捷入口。

```csharp
return FlourishBuilder
    .CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
    {
        services.AddSingleton<App>();
        services.AddNavigable<HomePage>("首页", "\uE80F");
    })
    .ConfigureShell(shell => shell.UseTitleBar().UseNavigation())
    .ConfigureNavigation(navigation =>
    {
        navigation.SetGroup("导航", groupId: 0, group =>
        {
            group.AddNavigableViewItem<HomePage>(isInitial: true);
        });
    })
    .Run<App>();
```

同一次启动流程应选择 `App.xaml.cs` 生命周期路径或 `Run<App>()` 快捷入口之一。两者同时使用会导致 Shell 显示两次。

## 按功能继续配置

启动示例将不同配置职责分开：

- [依赖注入](configure-services.md)注册服务、页面和命令解析器。
- [Shell 配置](shell-configuration.md)启用高层 Shell 功能。
- [标题栏](configure-title-bar.md)配置标题栏内容和入口。
- [导航](navigation.md)配置导航栏展示和可见导航项。
- [提示浮层](configure-tips.md)、[排版](configure-font.md)和[窗口](configure-window.md)调整 Shell 的交互和外观。

## 创建页面

通过 `AddNavigable` 注册的页面就是普通 WPF `Page`。导航发生时，Flourish 会从依赖注入容器解析页面实例。页面显示名称、图标和缓存模式来自 `AddNavigable`；可见导航位置和初始页则在[导航](navigation.md)配置中设置。

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

## 首次运行检查清单

- WPF 应用从 `App.xaml.cs` 或其他应用起始点启动 Flourish。
- 至少一个页面已通过 `AddNavigable` 注册。
- 至少一个可见页面项已通过 `AddNavigableViewItem` 添加，最好指定 `isInitial: true`。
- 页面所需的 Shell 区域已通过 [Shell 配置](shell-configuration.md)启用。
- 运行时会在应用退出时释放，或由 builder 快捷入口 `.Run<App>()` 负责释放。
