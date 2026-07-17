---
title: 快速开始
description: 使用 Flourish 构建并运行一个基本 WPF 应用。
---

# 快速开始

基本的 Flourish 应用需要注册一个 WPF 页面、启用导航、构建 `IFlourish` 运行时并显示 Shell。

Flourish 内置文案默认使用英文。如需中文，请在 `Build()` 前调用 `builder.ConfigureData(data => data.SetLocale("CN"))`。[应用数据](configure-data.md)说明内置语言和自定义语言。

## 引用控件与主题资源

`Run(Application)` 和 `IFlourish.Show(Application)` 会在 Shell 打开前加载 Flourish 控件与主题资源。当 WPF 设计器、Shell 显示前的内容或独立使用[控件库](control-library.md)需要这些资源时，请在 `App.xaml` 中显式添加 `FlourishThemeResources`。

Flourish Shell 作为主窗口时，不要在 `App.xaml` 中设置 `StartupUri`。

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

## 配置应用起始点

应用可以在 `App.xaml.cs` 中持有 Flourish 运行时。启动 Host、显示 Shell，并在 WPF 退出时释放运行时。

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
                services.AddNavigable<HomePage>("首页", "\uE80F");
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

导航分组会显式地把 `HomePage` 放入面板，并将它设为初始页面。需要更多分组、固定项、命令项或父子结构时，请参阅[导航](navigation.md)。

## 其他起始方式

自定义入口或 bootstrapper 可以构建相同配置，并使用 `Run<App>()` 快捷入口。

```csharp
return FlourishBuilder
    .CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
    {
        services.AddSingleton<App>();
        services.AddNavigable<HomePage>("首页", "\uE80F");
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

同一启动流程应选择 `App.xaml.cs` 生命周期方式或 `Run<App>()`。

## 创建页面

通过 `AddNavigable` 注册的页面是 WPF `Page` 类。导航发生时，Flourish 会从依赖注入容器解析页面。

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

## 继续配置

- [依赖注入](configure-services.md)注册应用服务和页面。
- [应用数据](configure-data.md)选择内置界面语言并注册自定义语言。
- [Shell 配置](shell-configuration.md)启用 Shell 功能并说明其前置条件。
- [标题栏](configure-title-bar.md)配置标题栏内容。
- [项目](projects.md)添加项目感知的标题语义与运行时显示元数据。
- [导航](navigation.md)把已注册页面和命令项放入显式导航分组。
- [后台任务](background-tasks.md)运行可取消异步工作。
- [提示浮层](configure-tips.md)、[排版](configure-font.md)和[窗口](configure-window.md)配置其他 Shell 行为。

## 首次运行检查清单

- WPF 应用从 `App.xaml.cs` 或其他应用起始点启动 Flourish。
- 至少一个页面已通过 `AddNavigable` 注册。
- 已启用导航，且每个可见页面都放入分组或固定区域。
- 应用退出时释放运行时，或由 `Run<App>()` 管理其生命周期。
