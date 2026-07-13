---
title: Shell 配置
description: 启用 Flourish Shell 功能并配置共享外观选项。
---

# Shell 配置

`ConfigureShell` 用于启用主要 Shell 区域，并应用这些区域共用的选项。标题栏、导航、工具栏、动效与状态栏的内容和行为由对应功能 builder 配置。

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

## 功能开关与共享选项

| Shell 方法 | 行为 | 功能文章 |
| --- | --- | --- |
| `UseTitleBar` | 启用 Flourish 标题栏；禁用后使用 Windows 原生标题栏。 | [标题栏](configure-title-bar.md) |
| `UseNavigation` | 启用导航栏。 | [导航](navigation.md) |
| `UseDynamicToolbar` | 启用页面专属工具栏内容。 | [动态工具栏](dynamic-toolbar.md) |
| `UseTips` | 设置首次显示延迟并启用 Flourish 提示浮层。 | [提示浮层](configure-tips.md) |
| `UseMotion` | 启用已配置的过渡和动画。 | [动效](configure-motion.md) |
| `UseMaterialEffect` | 选择并启用窗口材质；`None` 会禁用材质。 | [材质特效](configure-material-effect.md) |
| `UseThemeColors` | 设置主要色、辅助色和强调色。 | [主题](configure-themes.md) |
| `UseCornerRadius` | 设置控件与表面共用的圆角。 | [主题](configure-themes.md) |
| `UseGlobalFont` | 设置全局文字字体与基础字号。 | [排版](configure-font.md) |
| `UseStatusBar` | 启用常驻状态栏。 | [状态栏](status-bar.md) |

[窗口](configure-window.md)不需要 Shell 功能开关，通过 `ConfigureWindow` 直接配置。

## 前置条件与优先级

布尔功能开关的优先级高于详细配置。例如，启用 `UseDynamicToolbar(false)` 时不会显示已注册的工具栏项，启用 `UseStatusBar(false)` 时不会显示已配置的状态项。

标题栏元素需要 `UseTitleBar()`。导航切换按钮还需要 `UseNavigation()`，因为它控制该面板。向预定义 Shell 区域加入应用内容时，也需要启用对应的标题栏、导航、工具栏或状态栏区域。

后台任务是常驻状态栏可见性的例外。即使省略 `UseStatusBar()`，活动任务也会临时显示任务指示器；没有活动任务后，状态栏会恢复到配置决定的可见性。参见[后台任务](background-tasks.md)。

## 自定义内容对齐

面包屑、动态工具栏、内容页面与内容区域宿主使用 `FlourishContentBodyMargin` 动态资源。应用可以在加入 `FlourishThemeResources` 后覆盖该资源：

```xml
<Thickness x:Key="FlourishContentBodyMargin">24,0,24,0</Thickness>
```

## 禁用功能

`UseTitleBar`、`UseNavigation`、`UseDynamicToolbar`、`UseMotion` 和 `UseStatusBar` 接受可选的 `enabled` 值。共用 builder 设置需要禁用某项功能时，传入 `false`。

```csharp
builder.ConfigureShell(shell =>
{
    shell
        .UseNavigation(showNavigation)
        .UseMotion(!useStaticInterface)
        .UseStatusBar(showStatusBar);
});
```

省略 `UseTips` 或 `UseGlobalFont` 时保留其默认行为。共用配置需要显式禁用材质时，使用 `MaterialEffect.None`。

## 相关功能

- [窗口](configure-window.md)配置尺寸、位置、渲染和关闭行为。
- [应用数据](configure-data.md)配置本地化与 Host 设置。
- [依赖注入](configure-services.md)注册应用服务与可替换的 Flourish 服务。
- [自定义 Shell 内容](configure-custom-handler.md)向已启用的 Shell 区域插入应用元素。
- [后台任务](background-tasks.md)运行可取消工作并显示活动状态。
