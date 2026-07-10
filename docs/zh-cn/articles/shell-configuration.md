---
title: Shell 配置
description: 选择 Flourish Shell 提供的功能区域，并了解各项功能的启用关系。
---

# Shell 配置

`ConfigureShell` 决定 Shell 中启用哪些高层功能。每个 `Use...` 方法都接收一个 `enabled` 参数；省略该参数时表示启用功能。

```csharp
builder
    .ConfigureData(data =>
        data.SetAppCompany("示例公司").SetAppName("Foobar"))
    .ConfigureShell(shell =>
    {
        shell
            .UseTitleBar()
            .UseProfile()
            .UseNavigation()
            .UseDynamicToolbar()
            .UseTips()
            .UseMotion()
            .UseMaterialEffect()
            .UseThemes()
            .UseFooter();
    });
```

## 功能开关与详细配置

Shell 开关决定功能是否可用，其他配置方法决定功能的内容和行为。例如，`ConfigureNavigation` 可以定义分组和导航项，但只有启用 `UseNavigation()` 后，导航栏才会显示。

| Shell 开关 | 功能文章 | 详细配置 |
| --- | --- | --- |
| `UseTitleBar` | [标题栏](configure-title-bar.md) | `ConfigureTitleBar` 配置标题、搜索、面包屑和标题栏入口。 |
| `UseProfile` | [用户资料（Profile）](configure-profile.md) | `ConfigureProfile` 配置默认资料、名称顺序和自定义页面。 |
| `UseNavigation` | [导航](navigation.md) | `ConfigureNavigation` 配置方向、分组、固定项和导航树。 |
| `UseDynamicToolbar` | [动态工具栏](dynamic-toolbar.md) | `ConfigureDynamicToolbar` 配置随页面变化的命令。 |
| `UseTips` | [提示浮层](configure-tips.md) | `ConfigureTips` 配置显示延迟和窗口边距。 |
| `UseMotion` | [动效](configure-motion.md) | `ConfigureMotion` 配置页面、导航栏和悬停动画。 |
| `UseMaterialEffect` | [材质特效](configure-material-effect.md) | `ConfigureMaterialEffect` 选择窗口材质。 |
| `UseThemes` | [主题](configure-themes.md) | `ConfigureThemes` 选择未保存偏好时使用的主题。 |
| `UseFooter` | [状态栏（Footer）](status-bar.md) | `ConfigureFooter` 配置状态文本和状态项。 |

[窗口](configure-window.md)和[排版](configure-font.md)不需要单独的 Shell 开关，因为每个 Shell 都具有窗口和基础排版设置。

## 前置关系

部分功能依赖另一个 Shell 区域：

- Profile 入口位于标题栏中，因此需要同时启用 `UseTitleBar()` 和 `UseProfile()`。标题栏配置中的 `ShowProfile(false)` 仍可隐藏该入口。
- 标题栏主题按钮需要启用 `UseTitleBar()` 和 `UseThemes()`，并通过 `ShowThemeToggle()` 显示。
- 标题栏导航按钮需要启用 `UseTitleBar()` 和 `UseNavigation()`，并通过 `ShowNavToggle()` 显示。
- 自定义内容不会自动启用所属区域。向标题栏、动态工具栏或状态栏添加内容前，需要先启用相应功能。
- 主题偏好存储需要[应用数据](configure-data.md)中说明的应用标识或显式目录。

## 禁用功能

向 `Use...` 方法传入 `false` 会禁用对应功能，即使已经提供了详细配置。

```csharp
builder.ConfigureShell(shell =>
{
    shell
        .UseTitleBar()
        .UseNavigation(false)
        .UseMotion(false);
});
```

这类配置适用于单页 Shell、自定义导航或不需要动画的界面。功能处于禁用状态时，对应的详细配置不会显示或运行。
