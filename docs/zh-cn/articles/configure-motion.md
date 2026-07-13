---
title: 动效
description: 配置页面过渡、导航栏过渡、悬停动画和减少动态效果行为。
---

# 动效

Flourish 可以为页面切换、导航栏展开和支持的悬停状态提供动画。先在 [Shell 配置](shell-configuration.md)中启用 `UseMotion()`，再使用 `ConfigureMotion` 选择具体效果。

## 配置动效

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

## 过渡与时长

每一种过渡或动画都可以设置独立的可选时长。省略时长时，Flourish 使用对应动画的默认时长。

显式时长必须大于零。将页面或导航栏过渡枚举设为 `None`，可以单独禁用对应类别。

`EnablePageTransition` 控制页面进入内容框架时的呈现方式，`EnableNavigationPanelTransition` 控制导航栏展开和折叠时的过渡。

`EnableHoverRevealAnimation` 为支持该效果的控件启用悬停动画。

## 页面过渡期间的渲染

页面过渡只动画内容框架上方由 Shell 管理、不可交互且不承载文本的遮罩。导航到的页面本身保持静止和完全不透明，因此文本始终与设备像素对齐，页面内容也不会在每个动画帧中重新测量或排列。

`Fade` 通过淡出遮罩揭示页面；`EntranceFromBottom` 从底部揭示保持静止的页面。两种效果都会在过渡结束时将遮罩恢复为隐藏状态，并且不会改变页面的最终布局。

## 减少动态效果

`RespectSystemReducedMotion` 让 Flourish 遵循操作系统的减少动态效果偏好。启用动画时使用该设置，可让 Shell 根据用户的辅助功能首选项进行调整。

`UseMotion(false)` 会禁用所有已配置的动效。

## 相关功能

- [控件库](control-library.md)说明标准控件模板与公共 HoverReveal 附加行为。
- [导航](navigation.md)使用导航栏过渡。
