---
title: 动效
description: 配置页面过渡、导航栏过渡、悬停动画和减少动态效果行为。
---

# 动效

Flourish 可以为页面切换、导航栏展开和支持的悬停状态提供动画。先在 [Shell 配置](shell-configuration.md)中启用 `UseMotion()`，再使用 `ConfigureMotion` 选择具体效果。

## 最小配置

```csharp
builder
    .ConfigureShell(shell => shell.UseMotion())
    .ConfigureMotion(motion =>
    {
        motion
            .EnablePageTransition()
            .EnableNavigationPanelTransition()
            .EnableHoverRevealAnimation()
            .RespectSystemReducedMotion();
    });
```

## 页面与导航栏过渡

`EnablePageTransition` 控制新页面进入内容区域的方式，`EnableNavigationPanelTransition` 控制导航栏展开和折叠时的过渡。

```csharp
motion
    .EnablePageTransition(
        FlourishPageTransition.EntranceFromBottom,
        TimeSpan.FromMilliseconds(180))
    .EnableNavigationPanelTransition(
        FlourishNavigationPanelTransition.Resize,
        TimeSpan.FromMilliseconds(180));
```

省略时长时，Flourish 使用对应动画的默认时长。显式时长必须大于零。将过渡类型设为相应枚举的 `None` 可以单独关闭该类过渡。

## 悬停动画

`EnableHoverRevealAnimation` 为支持该效果的 Shell 控件启用悬停揭示动画，也可以传入自定义时长。

```csharp
motion.EnableHoverRevealAnimation(TimeSpan.FromMilliseconds(140));
```

## 减少动态效果

`RespectSystemReducedMotion()` 让 Flourish 遵循 Windows 的减少动态效果偏好。该设置允许系统偏好在运行时抑制不必要的动画，而无需更改应用配置。

调用 `UseMotion(false)` 会禁用所有 Flourish 动效。它与将某一种过渡设为 `None` 不同：后者只关闭对应的页面或导航栏过渡。

## 相关功能

- [导航](navigation.md)使用页面和导航栏过渡。
