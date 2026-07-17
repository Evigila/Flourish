---
title: IFlourishBuilder
description: 理解 builder、Hosting 集成、服务注册和页面注册。
---

# IFlourishBuilder

`IFlourishBuilder` 是 Flourish 应用的组合入口。配置阶段不会立即创建窗口或页面；它会收集服务注册和 Shell 选项，最后在 `Build()` 时创建一个由 .NET Generic Host 支撑的 `IFlourish` 运行时。

## Hosting 模型

`FlourishBuilder.CreateDefaultBuilder(args)` 创建采用 .NET Generic Host 默认配置和生命周期模型的运行时。因此应用可以使用标准 Hosting 能力：

- 可以在 `ConfigureServices` 中通过 `HostBuilderContext` 访问配置和环境信息
- Flourish 设置使用同一 Host 配置中的标准 appsettings 与 User Secrets 来源
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
    })
    .Build();

return flourish.Run<App>();
```

## Builder 阶段

公开 builder 分别组织 Hosting、应用服务、功能开关和各功能配置。

| 功能 | Builder 方法 | 作用 |
| --- | --- | --- |
| [应用数据](configure-data.md) | `ConfigureData` | 配置本地化，并说明共享的 Host 配置。 |
| [依赖注入](configure-services.md) | `ConfigureServices` | 注册应用服务、页面和可替换的 Flourish 服务。 |
| [Shell 配置](shell-configuration.md) | `ConfigureShell` | 配置 Shell 区域、提示浮层、排版和材质特效。 |
| [用户资料（Profile）](configure-profile.md) | `ConfigureProfile` | 配置 Profile 承载页面；入口与名称顺序由标题栏配置。 |
| [标题栏](configure-title-bar.md) | `ConfigureTitleBar` | 配置标题栏内容和行为。 |
| [项目](projects.md) | `ConfigureShell`、`IProjectService` | 启用项目感知的标题显示，并在运行时管理内存中的项目元数据。 |
| [导航](navigation.md) | `ConfigureNavigation` | 配置导航栏展示、页面位置、命令项、分组和固定项。 |
| [自定义 Shell 内容](configure-custom-handler.md) | `ConfigureCustomHandler` | 将自定义 WPF 元素或命令插入预定义 Shell 区域。 |
| [动态工具栏](dynamic-toolbar.md) | `ConfigureDynamicToolbar` | 注册按页面变化的工具栏项。 |
| [后台任务](background-tasks.md) | `IBackgroundTaskService` | 提交有并发上限、可取消的异步工作。 |
| [提示浮层](configure-tips.md) | `ConfigureShell` | 使用 `UseTips` 配置并启用提示浮层。 |
| [动效](configure-motion.md) | `ConfigureMotion` | 配置页面过渡、导航栏过渡和悬停揭示动画。 |
| [窗口](configure-window.md) | `ConfigureWindow` | 配置 Shell 窗口属性与行为。 |
| [排版](configure-font.md) | `ConfigureShell` | 使用 `UseGlobalFont` 配置 Shell 排版。 |
| [材质特效](configure-material-effect.md) | `ConfigureShell` | 使用 `UseMaterialEffect` 应用窗口材质。 |
| [主题](configure-themes.md) | `ConfigureShell`、`ConfigureTitleBar` | 配置应用颜色和圆角，并使用 `SetThemeToggle` 启用主题选择。 |
| [状态栏](status-bar.md) | `ConfigureStatusBar` | 配置自定义状态项和合并的系统状态入口。 |

Builder 入口可以调用多次。同一入口的重复回调会在 `Build()` 时按注册顺序应用；重复设置同一选项时使用最后一次配置的值。

## 注册服务

所有属于依赖注入的内容都放在[依赖注入](configure-services.md)配置中。

```csharp
builder.ConfigureServices((_, services) =>
{
    services.AddSingleton<App>();
    services.AddSingleton<ReportService>();
    services.AddTransient<EditorViewModel>();
});
```

调用 `Build()` 后，应用可以从 `IFlourish.Services` 解析公开服务。

其中包括 `IBackgroundTaskService` 与 `IProjectService`。应用通过依赖注入解析它们，以提交异步工作或同步项目显示元数据；完整用法参见[后台任务](background-tasks.md)与[项目](projects.md)。

## 注册导航页面

`AddNavigable<TPage>` 会在依赖注入中注册 WPF `Page`。Flourish 从页面类名生成区分大小写的导航键，并移除一个末尾 `Page` 后缀。

```csharp
services.AddNavigable<HomePage>(
    displayName: "首页",
    iconGlyph: "\uE80F",
    cacheMode: FlourishPageCacheMode.Enabled);
```

注册页面会使其可供导航使用，但不会添加可见项。使用 `ConfigureNavigation` 显式放置页面：

```csharp
builder.ConfigureNavigation(navigation =>
    navigation.SetGroup(null, groupId: 0, group =>
        group.AddNavigableViewItem<HomePage>(isInitial: true)));
```

[导航](navigation.md)说明生成键、页面元数据、缓存行为、分组、固定项、校验和运行时字符串导航。

## 构建运行时

`Build()` 会创建 Host 并返回 `IFlourish`。调用之后，配置阶段就结束了。对于常见 WPF 应用，调用 `Run<App>()` 即可；它会启动 Host、创建并显示 Flourish Shell、进入 WPF dispatcher，并在应用退出后停止 Host。

```csharp
using var flourish = builder.Build();
return flourish.Run<App>();
```

只要 WPF 应用还在运行，就应保持这个运行时实例存活。
