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

## 导航栏过渡期间的行为

`Resize` 会为导航栏和 Shell 内容区的视觉边界生成动画，并在过渡完成时提交最终列宽。如果启用了内容居中，页面以及与其对齐的 Shell 区域会在整个过渡期间保持居中且不超过已配置的最大宽度。始终达到最大宽度的内容只会平移而不会被横向缩放，因此文本与内部间距会保持自然度量；可用宽度跨越限制时，居中界面会在该限制范围内调整宽度。

## 页面过渡期间的行为

`Fade` 让导航到的页面淡入，`EntranceFromBottom` 在淡入的同时加入一小段向上位移。两种过渡激活时，Flourish 都会临时缓存透明页面表面，让动画合成缓存位图，而不是在每一帧重绘复杂页面，并保留分数像素移动以获得平滑位移。过渡完成或取消后会立即恢复实时页面并移除缓存和渲染变换，两种过渡都不会改变页面的最终布局。

## 减少动态效果

`RespectSystemReducedMotion` 让 Flourish 遵循操作系统的减少动态效果偏好。启用动画时使用该设置，可让 Shell 根据用户的辅助功能首选项进行调整。

`UseMotion(false)` 会禁用所有已配置的动效。

## 相关功能

- [控件库](control-library.md)说明标准控件模板与公共 HoverReveal 附加行为。
- [导航](navigation.md)使用导航栏过渡。
