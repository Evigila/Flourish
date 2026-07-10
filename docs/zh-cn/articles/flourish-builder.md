---
title: IFlourishBuilder
description: 理解 builder、Hosting 集成、服务注册和页面注册。
---

# IFlourishBuilder

`IFlourishBuilder` 是 Flourish 应用的组合入口。配置阶段不会立即创建窗口或页面；它会收集服务注册和 Shell 选项，最后在 `Build()` 时创建一个由 .NET Generic Host 支撑的 `IFlourish` 运行时。

## Hosting 模型

`FlourishBuilder.CreateDefaultBuilder(args)` 创建采用 .NET Generic Host 默认配置和生命周期模型的运行时。因此应用可以使用标准 Hosting 能力：

- 可以在 `ConfigureServices` 中通过 `HostBuilderContext` 访问配置和环境信息
- 使用 `IServiceCollection` 注册服务
- 最终服务提供器可通过 `IFlourish.Services` 获取
- 应用对象可以用 `flourish.GetRequiredService<T>()` 解析
- `flourish.Run<App>()` 会启动 Host、显示 Shell、运行 WPF dispatcher，并在应用退出时停止 Host
- `flourish.Start()` 和 `flourish.StopAsync()` 对应 Host 生命周期方法

```csharp
using var flourish = FlourishBuilder
    .CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<App>();
        services.AddSingleton<ICommandParser, AppCommandParser>();
    })
    .Build();

return flourish.Run<App>();
```

## Builder 阶段

公开 builder 将高层功能开关和详细配置拆分开。

| 功能 | Builder 方法 | 作用 |
| --- | --- | --- |
| [应用数据](configure-data.md) | `ConfigureData` | 配置应用标识和偏好存储。 |
| [依赖注入](configure-services.md) | `ConfigureServices` | 注册应用服务、页面、命令解析器、ViewModel 和基础设施。 |
| [Shell 配置](shell-configuration.md) | `ConfigureShell` | 启用或禁用 Shell 高层功能。 |
| [用户资料（Profile）](configure-profile.md) | `ConfigureProfile` | 配置默认资料、名称顺序和承载页面。 |
| [标题栏](configure-title-bar.md) | `ConfigureTitleBar` | 配置标题栏内容和行为。 |
| [导航](navigation.md) | `ConfigureNavigation` | 配置导航栏展示、页面位置、命令项、分组和固定项。 |
| [自定义 Shell 内容](configure-custom-handler.md) | `ConfigureCustomHandler` | 将自定义 WPF 元素或命令插入预定义 Shell 区域。 |
| [动态工具栏](dynamic-toolbar.md) | `ConfigureDynamicToolbar` | 注册按页面变化的工具栏项。 |
| [提示浮层](configure-tips.md) | `ConfigureTips` | 配置提示显示延迟和 Shell 边缘间距。 |
| [动效](configure-motion.md) | `ConfigureMotion` | 配置页面过渡、导航栏过渡和悬停揭示动画。 |
| [窗口](configure-window.md) | `ConfigureWindow` | 配置 Shell 窗口尺寸、位置、状态、调整大小模式、任务栏显示和置顶行为。 |
| [排版](configure-font.md) | `ConfigureFont` | 配置 Shell 字体和基础字号。 |
| [材质特效](configure-material-effect.md) | `ConfigureMaterialEffect` | 选择 Shell 窗口材质。 |
| [主题](configure-themes.md) | `ConfigureThemes` | 选择未保存偏好时使用的主题。 |
| [状态栏（Footer）](status-bar.md) | `ConfigureFooter` | 配置状态文本和状态项。 |

Builder 入口可以调用多次。同一入口的重复回调会在 `Build()` 时按注册顺序应用；字体、材质和主题等直接值设置使用最后一次配置的值。

## 注册服务

所有属于依赖注入的内容都放在[依赖注入](configure-services.md)配置中。

```csharp
builder.ConfigureServices((_, services) =>
{
    services.AddSingleton<App>();
    services.AddSingleton<ICommandParser, AppCommandParser>();
    services.AddSingleton<ReportService>();
    services.AddTransient<EditorViewModel>();
});
```

Flourish 会在构建期间注册内置服务。应用可以从 `IFlourish.Services` 解析公开服务，无需直接创建 Shell 基础设施。

## 注册导航页面

`AddNavigable` 会在依赖注入中注册 WPF `Page`，并记录显示元数据、缓存模式和可选导航键。

```csharp
services.AddNavigable<HomePage>(
    displayName: "首页",
    iconGlyph: "\uE80F",
    cacheMode: FlourishPageCacheMode.Enabled,
    navigationKey: NavigationRoutes.Home);
```

页面注册不会决定其显示位置。[导航](navigation.md)是页面元数据、缓存行为、可见分组、固定项和运行时导航的唯一完整指南。

## 构建运行时

`Build()` 会创建 Host 并返回 `IFlourish`。调用之后，配置阶段就结束了。对于常见 WPF 应用，调用 `Run<App>()` 即可；它会启动 Host、创建并显示 Flourish Shell、进入 WPF dispatcher，并在应用退出后停止 Host。

```csharp
using var flourish = builder.Build();
return flourish.Run<App>();
```

只要 WPF 应用还在运行，就应保持这个运行时实例存活。
