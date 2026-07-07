---
title: ConfigureMotion
description: 配置 Flourish 动画时长和过渡效果。
---

# ConfigureMotion

`ConfigureMotion` 配置动画细节。动效只有在 [`ConfigureShell`](configure-shell.md) 启用 `UseMotion()` 后才会运行。

```csharp
builder
    .ConfigureShell(shell => shell.UseMotion())
    .ConfigureMotion(motion =>
    {
        motion
            .SetDuration(TimeSpan.FromMilliseconds(180))
            .SetPageTransition(FlourishPageTransition.EntranceFromBottom)
            .SetNavigationPanelTransition(FlourishNavigationPanelTransition.Resize)
            .SetHoverReveal()
            .RespectSystemReducedMotion();
    });
```

## 细节

`SetDuration` 设置共享动画时长。无参重载使用 Flourish 默认的平衡时长。

`SetPageTransition` 控制页面进入内容框架的方式。`SetNavigationPanelTransition` 控制导航栏打开和关闭的方式。

`SetHoverReveal` 为支持的控件启用轻量悬浮动画。`RespectSystemReducedMotion` 让 Flourish 遵循操作系统的减少动画偏好。

关闭动效应通过 `UseMotion(false)` 完成，而不是通过 `ConfigureMotion`。

## 相关 API

- [`ConfigureNavigation`](configure-navigation.md) 使用导航栏过渡。
- [`ConfigureDynamicToolbar`](configure-dynamic-toolbar.md) 和 [`ConfigureCustomHandler`](configure-custom-handler.md) 可以承载受 Hover Reveal 影响的控件。
