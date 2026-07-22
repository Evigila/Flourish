---
title: 提示浮层
description: 配置 Flourish 控件与 Shell 区域自有 Tooltip 的 Flourish 呈现方式。
---

# 提示浮层

Tooltip 为紧凑或仅含图标的 Flourish 控件与 Shell 区域提供标签。`UseTips` 将这些提示从原生 WPF Tooltip 呈现切换为 Flourish 呈现，并设置首次显示延迟。

## 配置提示

```csharp
builder.ConfigureShell(shell => shell.UseTips(delay: 200));
```

`delay` 参数表示指针悬停后到 Flourish Tooltip 显示前的时间，单位为毫秒。默认值为 `200`，且不能为负数。Flourish 呈现使用临时 [Overlay](../controls/overlay.md)，指针离开提示上下文后会自行关闭。

Flourish 会把这种呈现保持在 Shell 边界内。省略 `UseTips` 时，同一份 Tooltip 内容会使用原生 WPF 的外观、显示时间、定位和打开行为。在运行时禁用 `ToolTips` 功能会执行相同的回退；重新启用或配置该功能会为 Flourish 标题栏、导航栏、工具栏与状态栏控件恢复 Flourish 呈现。

## 原生与第三方控件

`UseTips` 不会配置附加到原生 WPF 控件的 Tooltip，也不会配置第三方控件自有的 Tooltip。Flourish 不会在应用级为这些 Tooltip 应用统一模板，因此它们始终保留各自的外观、显示时间、定位和打开行为。

启用 Flourish 呈现后，打开、延迟、Popup 定位与关闭仍由 WPF `ToolTipService` 负责。

后台任务状态详情不受 `UseTips` 控制，仍可通过[后台任务](background-tasks.md)界面查看。

## 相关功能

- [Shell 配置](shell-configuration.md)配置 Flourish 自有 Tooltip 的呈现方式。
- [标题栏](configure-title-bar.md)、[导航](navigation.md)和[状态栏](status-bar.md)包含受此设置控制的控件。
- [后台任务](background-tasks.md)提供任务状态和队列详情。
