---
title: ConfigureWindow
description: 配置 Flourish Shell 窗口尺寸、位置和窗口行为。
---

# ConfigureWindow

`ConfigureWindow` 配置 Shell 窗口属性。它独立于 [`ConfigureShell`](configure-shell.md) 功能开关，因为每个 Shell 都需要窗口。

```csharp
builder.ConfigureWindow(window =>
{
    window
        .SetWindowSize(1280, 720)
        .SetWindowMinSize(960, 540)
        .SetWindowMaxSize(1920, 1080)
        .SetWindowPosition(WindowStartupLocation.CenterScreen)
        .SetWindowState(WindowState.Normal)
        .SetWindowResizeMode(ResizeMode.CanResize)
        .UseTopmost(false)
        .ShowInTaskbar(true);
});
```

## 细节

尺寸方法会校验有限正数，并保证最小值和最大值关系正确。`SetManualWindowPosition` 会把启动位置切换为 `Manual` 并保存指定坐标。

`SetWindowResizeMode` 会影响自定义标题栏中的最大化命令是否可用。`ShowInTaskbar` 和 `UseTopmost` 对应普通 WPF 窗口行为。

窗口配置应靠近 Shell 组合配置；WPF 资源和应用生命周期仍属于 WPF 应用自身。

## 相关 API

- [`快速开始`](getting-started.md) 展示从 `App.xaml.cs` 或其他应用起始点启动。
- [`ConfigureTitleBar`](configure-title-bar.md) 控制已配置窗口中的标题栏按钮。
- [`ConfigureMaterialEffect`](configure-material-effect.md) 改变窗口背景材质。
