---
title: 提示浮层
description: 配置 Flourish 控件与 Shell 区域自有 Tooltip 的 Flourish 呈现方式。
---

# 提示浮层

`ConfigureToolTips` 为紧凑或仅含图标的控件与 Shell 区域启用 Flourish Tooltip，并配置显示时序。

## 配置提示

```csharp
builder.ConfigureToolTips(toolTips =>
    toolTips
        .SetEnabled()
        .SetSettings(initialShowDelayMilliseconds: 200));
```

`initialShowDelayMilliseconds` 参数表示指针悬停后到 Flourish Tooltip 显示前的时间，单位为毫秒。默认值为 `200`，且不能为负数。Flourish 呈现使用临时 [Overlay](../controls/overlay.md)，指针离开提示上下文后会自行关闭。

Flourish Tooltip 保持在 Shell 边界内。`SetEnabled(false)` 或省略 `ConfigureToolTips` 时，同一内容使用原生 WPF 外观与行为。运行时通过 `IToolTipService.SetEnabled` 切换，`Current` 读取设置，`Changed` 监听更新。

## 原生与第三方控件

`ConfigureToolTips` 不会配置附加到原生 WPF 控件的 Tooltip，也不会配置第三方控件自有的 Tooltip。Flourish 不会在应用级为这些 Tooltip 应用统一模板，因此它们始终保留各自的外观、显示时间、定位和打开行为。

启用 Flourish 呈现后，打开、延迟、Popup 定位与关闭仍由 WPF `ToolTipService` 负责。

后台任务状态详情不受 Tooltip 呈现控制，仍可通过[后台任务](background-tasks.md)界面查看。

## 相关功能

- [Shell 配置](shell-configuration.md)
- [标题栏](configure-title-bar.md)、[导航](navigation.md)和[状态栏](status-bar.md)
- [后台任务](background-tasks.md)
