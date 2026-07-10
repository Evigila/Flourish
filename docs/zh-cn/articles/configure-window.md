---
title: 窗口
description: 配置 Flourish Shell 窗口的尺寸、位置和标准 WPF 窗口行为。
---

# 窗口

每个 Flourish Shell 都具有一个 WPF 窗口。使用 `ConfigureWindow` 可以设置初始尺寸、尺寸约束、启动位置、窗口状态、调整大小方式、置顶行为和任务栏可见性。这些设置不需要额外的 Shell 功能开关。

## 最小配置

```csharp
builder.ConfigureWindow(window =>
{
    window
        .SetWindowSize(1280, 720)
        .SetWindowMinSize(960, 540)
        .SetWindowPosition(WindowStartupLocation.CenterScreen);
});
```

## 尺寸与位置

`SetWindowSize` 设置初始尺寸，`SetWindowMinSize` 和 `SetWindowMaxSize` 限制用户可调整的范围。初始尺寸和最小尺寸必须是有限正数；最大尺寸可以是正数或 `double.PositiveInfinity`，且不能小于最小尺寸。

```csharp
window
    .SetWindowSize(1280, 720)
    .SetWindowMinSize(960, 540)
    .SetWindowMaxSize(1920, 1080);
```

`SetWindowPosition` 接收标准 WPF `WindowStartupLocation`。需要指定坐标时，使用 `SetManualWindowPosition(left, top)`；该方法会保存坐标并自动将启动位置设为 `Manual`。

```csharp
window.SetManualWindowPosition(left: 40, top: 40);
```

## 窗口行为

```csharp
window
    .SetWindowState(WindowState.Normal)
    .SetWindowResizeMode(ResizeMode.CanResize)
    .UseTopmost(false)
    .ShowInTaskbar(true);
```

`SetWindowResizeMode` 也会影响自定义标题栏中的最大化命令是否可用。`UseTopmost` 和 `ShowInTaskbar` 对应标准 WPF 窗口行为。

## 相关功能

- [标题栏](configure-title-bar.md)配置窗口中的标题与窗口命令。
- [材质特效](configure-material-effect.md)配置窗口背景材质。
- [Shell 配置](shell-configuration.md)控制窗口中显示的功能区域。
