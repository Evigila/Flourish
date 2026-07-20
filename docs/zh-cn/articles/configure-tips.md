---
title: 提示浮层
description: 配置 Shell 边界内的提示显示时间和位置。
---

# 提示浮层

Flourish 提示为紧凑或仅含图标的 Shell 控件提供标签。`UseTips` 设置初始延迟并启用这些提示。

## 配置提示

```csharp
builder.ConfigureShell(shell => shell.UseTips(delay: 200));
```

`delay` 参数表示指针悬停后到提示显示前的时间，单位为毫秒。默认值为 `200`，且不能为负数。Tooltip 使用临时 [Overlay](../controls/overlay.md)，指针离开提示上下文后会自行关闭。

Flourish 会把这些提示保持在 Shell 边界内。不需要提示时，请省略 `UseTips`。运行时更改 `ToolTips` 功能会统一应用于标题栏、导航栏、工具栏与状态栏控件。

后台任务状态详情不受 `UseTips` 控制，仍可通过[后台任务](background-tasks.md)界面查看。

## 相关功能

- [Shell 配置](shell-configuration.md)配置并启用提示。
- [标题栏](configure-title-bar.md)、[导航](navigation.md)和[状态栏](status-bar.md)包含受此设置控制的控件。
- [后台任务](background-tasks.md)提供任务状态和队列详情。
