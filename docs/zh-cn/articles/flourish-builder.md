---
title: IFlourishBuilder
description: 理解 builder、Hosting 集成、服务注册和页面注册。
---

# IFlourishBuilder

`IFlourishBuilder` 是 Flourish 应用的组合入口。配置阶段不会立即创建窗口或页面；它会收集服务注册和 Shell 选项，最后在 `Build()` 时创建一个由 .NET Generic Host 支撑的 `IFlourish` 运行时。

## Hosting 模型

`FlourishBuilder.CreateDefaultBuilder(args)` 内部使用 `Host.CreateDefaultBuilder(args)`。因此构建出的运行时拥有现代 .NET 应用中熟悉的 Hosting 行为：

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

| 方法 | 作用 |
| --- | --- |
| `ConfigureServices` | 注册应用服务、页面、命令解析器、ViewModel 以及你希望放入 DI 的基础设施。 |
| `ConfigureShell` | 启用或禁用标题栏、导航、动态工具栏、Tips、动效、材质特效、主题和 Footer 等 Shell 功能。 |
| `ConfigureTitleBar` | 在标题栏启用时配置标题栏内容和行为。 |
| `ConfigureNavigation` | 配置导航栏展示参数、已注册页面位置、命令项、分组和固定项。 |
| `ConfigureCustomHandler` | 将自定义 WPF 元素插入预定义 Shell 区域。 |
| `ConfigureDynamicToolbar` | 注册按页面变化的工具栏项。 |
| `ConfigureTips` | 配置提示浮层延迟和 Shell 边缘间距。 |
| `ConfigureMotion` | 配置动画时长、页面过渡、导航栏过渡和 Hover Reveal。 |
| `ConfigureWindow` | 配置 Shell 窗口尺寸、位置、状态、缩放模式、任务栏显示和置顶行为。 |
| `ConfigureFont` | 配置 Shell 字体和基础字号。 |
| `ConfigureMaterialEffect` | 配置材质特效启用时使用的材质类型。 |
| `ConfigureThemes` | 配置主题启用时使用的默认主题。 |
| `ConfigureFooter` | 配置 Shell Footer 中的状态区域。 |

这些方法都可以调用多次。Flourish 会保存回调，并在 `Build()` 时统一应用。

## 注册服务

所有属于依赖注入的内容都放在 `ConfigureServices` 中。

```csharp
builder.ConfigureServices((_, services) =>
{
    services.AddSingleton<App>();
    services.AddSingleton<ICommandParser, AppCommandParser>();
    services.AddSingleton<ImageLibrary>();
    services.AddTransient<EditorViewModel>();
});
```

Flourish 会在构建阶段注册自己的内部服务，包括导航、工具栏、Footer 状态、Message、Tips、材质特效、动效、页面缓存和 Shell 窗口服务。你不需要直接构造这些内部服务。

## 使用 AddNavigable 注册页面

`AddNavigable` 是让 WPF `Page` 进入 Flourish 导航系统的推荐注册方式。它会把页面注册到 DI，并记录页面导航项使用的显示元数据。它不会决定页面显示在导航栏的哪个位置。

```csharp
services.AddNavigable<HomePage>(
    displayName: "首页",
    iconGlyph: "\uE80F",
    cacheMode: FlourishPageCacheMode.Enabled);
```

泛型重载适合页面类型在编译期已知的场景。

```csharp
services.AddNavigable<SettingsPage>("设置", "\uE713");
```

`Type` 重载适合页面注册来自配置或插件的场景。

```csharp
services.AddNavigable(
    typeof(ReportPage),
    displayName: "报表",
    iconGlyph: "\uE9D2",
    cacheMode: FlourishPageCacheMode.Disabled);
```

`displayName` 会被 `AddNavigableViewItem` 显示出来。`iconGlyph` 通常是 Segoe Fluent Icons 字形，例如 `"\uE80F"`。`cacheMode` 控制页面实例是否复用。

已注册页面需要通过 `ConfigureNavigation` 放入可见导航模型。导航栏方向、宽度和初始展开状态等展示设置也在这里配置。

```csharp
builder.ConfigureNavigation(navigation =>
{
    navigation
        .SetInitiallyOpen()
        .SetGroup("导航", groupId: 0, group =>
        {
            group.AddNavigableViewItem<HomePage>(isInitial: true);
            group.AddNavigableViewItem<SettingsPage>();
        });

    navigation.AddFixedNavigableViewItem<ReportPage>();
});
```

`isInitial` 属于可见的 ViewItem。这样页面只需要注册一次，之后可以放入可滚动分组区域或底部固定区域，不会把注册元数据和布局决策混在一起。

## 页面缓存模式

如果页面需要保留局部 UI 状态、滚动位置或已加载数据，使用 `FlourishPageCacheMode.Enabled`。如果页面每次进入都应重新创建，例如总是要加载新状态的向导页或编辑页，使用 `Disabled`。

```csharp
services.AddNavigable<DashboardPage>(
    "仪表盘",
    "\uE9D2",
    cacheMode: FlourishPageCacheMode.Enabled);

services.AddNavigable<ImportWizardPage>(
    "导入",
    "\uE8B5",
    cacheMode: FlourishPageCacheMode.Disabled);
```

缓存模式绑定到已注册页面类型，而不是某一个导航项。一个页面只能显示在一个导航位置，但它的缓存行为仍然在注册阶段定义。

## 构建运行时

`Build()` 会创建 Host 并返回 `IFlourish`。调用之后，配置阶段就结束了。对于常见 WPF 应用，调用 `Run<App>()` 即可；它会启动 Host、创建并显示 Flourish Shell、进入 WPF dispatcher，并在应用退出后停止 Host。

```csharp
using var flourish = builder.Build();
return flourish.Run<App>();
```

只要 WPF 应用还在运行，就应保持这个运行时实例存活。
