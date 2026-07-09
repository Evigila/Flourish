---
title: ConfigureMotion
description: 配置 Flourish 过渡和动画效果。
---

# ConfigureMotion

`ConfigureMotion` 配置动画细节。动效只有在 [`ConfigureShell`](configure-shell.md) 启用 `UseMotion()` 后才会运行。

```csharp
builder
    .ConfigureShell(shell => shell.UseMotion())
    .ConfigureMotion(motion =>
    {
        motion
            .EnablePageTransition(
                FlourishPageTransition.EntranceFromBottom,
                TimeSpan.FromMilliseconds(180))
            .EnableNavigationPanelTransition(
                FlourishNavigationPanelTransition.Resize,
                TimeSpan.FromMilliseconds(180))
            .EnableHoverRevealAnimation(TimeSpan.FromMilliseconds(140))
            .RespectSystemReducedMotion();
    });
```

## 细节

每一种过渡或动画都可以单独传入可选时长。如果不传入时长，Flourish 会使用该动画自己的默认时长。

`EnablePageTransition` 控制页面进入内容框架的方式。`EnableNavigationPanelTransition` 控制导航栏打开和关闭的方式。

`EnableHoverRevealAnimation` 为支持的控件启用轻量悬浮动画。`RespectSystemReducedMotion` 让 Flourish 遵循操作系统的减少动画偏好。

关闭动效应通过 `UseMotion(false)` 完成，而不是通过 `ConfigureMotion`。

## 相关 API

- [`ConfigureNavigation`](configure-navigation.md) 使用导航栏过渡。
- [`ConfigureDynamicToolbar`](configure-dynamic-toolbar.md) 和 [`ConfigureCustomHandler`](configure-custom-handler.md) 可以承载受 Hover Reveal 影响的控件。
