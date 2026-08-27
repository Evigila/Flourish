---
title: IApplicationBuilder
description: 使用按功能划分的 Builder 配置并构建 Flourish 应用。
---

# IApplicationBuilder

`IApplicationBuilder` 记录启动默认值和服务注册，`Build()` 最终创建基于 Generic Host 的 `IApplicationRuntime`。公开契约位于 `ArkheideSystem.Flourish.Abstract`。

```csharp
using ArkheideSystem.Flourish.Abstract;

using var flourish = ApplicationBuilder
    .CreateDefaultBuilder(args)
    .ConfigureServices((context, services) => services.AddSingleton<App>())
    .ConfigureTitleBar(titleBar =>
        titleBar.SetEnabled().SetApplicationTitle("Foobar"))
    .Build();

return flourish.Run<App>();
```

## 功能 Builder

| 入口 | Builder | 启动职责 |
| --- | --- | --- |
| `ConfigureData` | `IDataBuilder` | 语言、文化文件以及可写设置/项目路径。 |
| `ConfigureConfiguration` | 标准 `IConfigurationBuilder` | 应用拥有的 Microsoft 配置源。 |
| `ConfigureServices` | 标准 `IServiceCollection` | 应用服务和可替换的 Flourish 服务。 |
| `ConfigureAppearance` | `IAppearanceBuilder` | 材质、色板和圆角默认值。 |
| `ConfigureFont` | `IFontBuilder` | 全局与页面字体。 |
| `ConfigureLayout` | `ILayoutBuilder` | 居中内容和平滑滚动。 |
| `ConfigureToolTips` | `IToolTipBuilder` | Flourish 提示外观与时序。 |
| `ConfigureProjects` | `IProjectBuilder` | 项目感知 Shell 模式。 |
| `ConfigureTitleBar` | `ITitleBarBuilder` | 标题栏状态和内容。 |
| `ConfigureNavigation` | `INavigationBuilder` | 导航区域、路由、页面和可见项。 |
| `ConfigureContent` | `ICustomContentBuilder` | 自定义 WPF Shell 区域内容。 |
| `ConfigureToolbar` | `IToolbarBuilder` | 按页面变化的工具栏定义。 |
| `ConfigureMotion` | `IMotionBuilder` | 动效状态和过渡默认值。 |
| `ConfigureWindow` | `IWindowBuilder` | 初始窗口边界与行为。 |
| `ConfigureStatusBar` | `IStatusBarBuilder` | 状态区域和内置指示器。 |

嵌套 Builder 使用 `SetEnabled`、`SetFont`、`SetSize`、`SetPanelWidth` 等领域动词；可选行为和持久化偏好选择也统一使用 `Set...` 约定。

同一入口可在 `Build()` 前注册多次，并按注册顺序执行。`Build()` 会消费 Builder；之后继续配置、再次构建或调用已结束回调中捕获的嵌套 Builder，都会抛出 `InvalidOperationException`。

## Host 配置

`CreateDefaultBuilder` 保留标准 .NET Host 配置。`ConfigureConfiguration` 接收 `IConfigurationBuilder`，`ConfigureServices` 接收 `HostBuilderContext` 和 `IServiceCollection`；注入 `IConfiguration` 可读取最终配置。

`appsettings.Flourish.json` 仍是默认的可写 Flourish 设置文件。`ISettingsStore` 负责原子写入 `Flourish:` 节并重新加载同一份标准 `IConfiguration`，它不会取代 Microsoft 配置系统。

## 运行时 Service

Builder 定义启动草稿；`Build()` 后通过依赖注入解析对应 Service。例如 `ITitleBarBuilder` 定义标题栏初始状态，`ITitleBarService` 修改已构建的标题栏。具有状态的 Service 统一公开 `Current` 与 `Changed`。
