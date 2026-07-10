---
title: ConfigureTitleBar
description: 在 ConfigureShell 启用标题栏后配置 Flourish 标题栏。
---

# ConfigureTitleBar

`ConfigureTitleBar` 配置内置标题栏内容和行为。标题栏只有在 [`ConfigureShell`](configure-shell.md) 启用 `UseTitleBar()` 后才会显示。

```csharp
builder
    .ConfigureShell(shell => shell.UseTitleBar().UseProfile())
    .ConfigureTitleBar(titleBar =>
    {
        titleBar
            .ShowLogo()
            .ShowTitle()
            .ShowSubTitle()
            .ShowSearch()
            .ShowBreadcrumb()
            .ShowNavToggle()
            .ShowProfile()
            .ShowThemeToggle()
            .SetTitle("Gallery")
            .SetSubtitle("Flourish 示例")
            .SetSearchPlaceholder("搜索图片");
    });
```

## 细节

`Show...` 方法控制内置标题栏区域。它们不会创建自定义内容；自定义 WPF 元素应通过 [`ConfigureCustomHandler`](configure-custom-handler.md) 插入。

`SetSearchHandler` 接收搜索文本变化。带 `IServiceProvider` 的重载适合解析消息服务、ViewModel 或应用搜索协调器。

`ShowThemeToggle` 依赖 [`ConfigureShell`](configure-shell.md) 通过 `UseThemes()` 启用主题。默认主题由 [`ConfigureThemes`](configure-themes.md) 选择。

`ShowProfile` 控制标题栏入口，[`ConfigureProfile`](configure-profile.md) 则配置用户、登录行为和承载页面。该入口还要求 `UseProfile()` 已启用。

## 相关 API

- [`ConfigureCustomHandler`](configure-custom-handler.md) 可替换用户区域或添加标题栏动作。
- [`ConfigureProfile`](configure-profile.md) 配置内置 Profile 弹层与服务。
- [`ConfigureNavigation`](configure-navigation.md) 与 `ShowNavToggle` 配合。
- [`ConfigureThemes`](configure-themes.md) 与 `ShowThemeToggle` 配合。
