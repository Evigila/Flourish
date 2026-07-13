---
title: Shell 配置
description: 选择 Flourish Shell 的功能区域，并直接配置常用外观与交互选项。
---

# Shell 配置

`ConfigureShell` 负责启用 Shell 的高层功能。导航、动态工具栏和状态栏等复杂区域由对应文章中的 builder 配置内容；提示、材质和全局字体等单一选项则在 `Use...` 调用中直接完成配置。

```csharp
builder.ConfigureShell(shell =>
{
    shell
        .UseTitleBar()
        .UseNavigation()
        .UseDynamicToolbar()
        .UseTips(delay: 200)
        .UseMotion()
        .UseMaterialEffect(MaterialEffect.Mica)
        .UseGlobalFont("Microsoft YaHei UI", 14)
        .UseStatusBar();
});
```

## 功能入口与详细配置

| Shell 方法 | 功能文章 | 行为 |
| --- | --- | --- |
| `UseTitleBar` | [标题栏](configure-title-bar.md) | 启用 Flourish 自定义标题栏；禁用后使用 Windows 原生标题栏，并保留请求的材质效果。`ConfigureTitleBar` 配置并显示自定义标题栏中的元素。 |
| `UseNavigation` | [导航](navigation.md) | 启用导航区域；`ConfigureNavigation` 配置方向、分组、固定项和导航树。 |
| `UseDynamicToolbar` | [动态工具栏](dynamic-toolbar.md) | 启用工具栏区域；`ConfigureDynamicToolbar` 配置随页面变化的命令。 |
| `UseTips` | [提示浮层](configure-tips.md) | 设置显示延迟并启用提示浮层。 |
| `UseMotion` | [动效](configure-motion.md) | 启用动效；`ConfigureMotion` 配置页面、导航栏和悬停动画。 |
| `UseMaterialEffect` | [材质特效](configure-material-effect.md) | 选择并启用窗口材质。 |
| `UseGlobalFont` | [排版](configure-font.md) | 设置 Shell 与导航页面使用的全局字体系列和基础字号。 |
| `UseStatusBar` | [状态栏](status-bar.md) | 启用状态栏；`ConfigureStatusBar` 配置自定义状态项和系统状态入口。 |

[窗口](configure-window.md)不需要 Shell 功能开关，可直接通过 `ConfigureWindow` 设置。

## 标题栏元素

标题栏采用“配置即显示”的方式。调用 `SetSearch`、`SetBreadcrumbButton`、`SetNavToggle`、`SetLogo`、`SetTitle`、`SetSubTitle`、`SetProfile` 或 `SetThemeToggle` 时，对应元素会自动显示；未配置的元素保持隐藏。

```csharp
builder
    .ConfigureShell(shell => shell.UseTitleBar().UseNavigation())
    .ConfigureTitleBar(titleBar =>
    {
        titleBar
            .SetTitle("Foobar")
            .SetNavToggle()
            .SetProfile()
            .SetThemeToggle(FlourishTheme.System);
    });
```

`SetProfile` 会同时启用默认 Profile，`SetThemeToggle` 会同时启用主题功能，因此不需要额外的 Shell 开关。

## 前置关系

- `SetNavToggle()` 需要同时启用 `UseNavigation()`，否则导航切换按钮不会显示。
- 自定义内容不会自动启用所属区域。向标题栏、动态工具栏或状态栏添加内容前，需要先启用相应区域；自定义 Profile 内容还需要调用 `SetProfile()`。
- `SetThemeToggle()` 的主题偏好使用[应用数据](configure-data.md)中说明的 Host 配置。

后台任务活动是常驻状态栏开关的例外：即使没有调用 `UseStatusBar()`，存在活动任务时也会临时显示任务指示器；活动列表清空后，状态栏会恢复为配置决定的可见性。参见[后台任务](background-tasks.md)。

## 内容主体对齐

Breadcrumb、动态工具栏、内容页面和内容区域宿主统一使用 `FlourishContentBodyMargin` 动态资源。默认值为 `32,0,32,0`，因此所有内容表面具有相同的左右边界。

需要不同留白或全宽内容的应用，可以在加入 `FlourishThemeResources` 后覆盖该资源：

```xml
<Thickness x:Key="FlourishContentBodyMargin">24,0,24,0</Thickness>
```

## Shell 边缘对齐

内置标题栏内容、导航项和状态栏在窗口左右边缘使用相同的水平留白。导航栏位于右侧时，Flourish 会镜像其外侧留白，使导航内容与标题栏和状态栏保持对齐。折叠导航栏的宽度同时容纳这段留白、命令按钮区域和紧凑滚动条。最小化、最大化或还原以及关闭按钮仍贴紧窗口右上边缘，以保留窗口命令的命中区域。

## 禁用与省略

`UseTitleBar`、`UseNavigation`、`UseDynamicToolbar`、`UseMotion` 和 `UseStatusBar` 接收可选的 `enabled` 参数。传入 `false` 会禁用对应区域，即使已经提供详细配置。

```csharp
builder.ConfigureShell(shell =>
{
    shell
        .UseTitleBar()
        .UseNavigation(false)
        .UseMotion(false)
        .UseMaterialEffect(MaterialEffect.None);
});
```

`UseMaterialEffect(MaterialEffect.None)` 会禁用系统材质。提示浮层和全局字体没有独立禁用开关：不调用对应方法时，Shell 使用默认行为。

后台任务由 Host 管理的运行时服务提供，不需要新的 Shell 开关；完整模型参见[后台任务](background-tasks.md)。
