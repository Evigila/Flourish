---
title: 窗口
description: 配置 Flourish Shell 窗口的尺寸、位置和 WPF 窗口行为。
---

# 窗口

每个 Flourish Shell 都具有一个 WPF 窗口。使用 `ConfigureWindow` 可以设置初始尺寸、尺寸约束、启动位置、窗口状态、置顶行为、任务栏可见性和托盘关闭流程。

## 配置窗口

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
        .ShowInTaskbar(true)
        .SetTrayExit();
});
```

窗口配置不依赖 [Shell 配置](shell-configuration.md)中的功能开关。

## 尺寸与位置

初始尺寸和最小尺寸必须是有限正数。最大尺寸可以是正数或 `double.PositiveInfinity`，且不能小于最小尺寸。`SetManualWindowPosition` 会将启动位置设为 `Manual` 并保存指定坐标。

```csharp
window.SetManualWindowPosition(left: 120, top: 80);
```

使用 `WindowStartupLocation` 或手动坐标可明确指定启动位置。

## 窗口行为

`SetWindowResizeMode` 控制自定义标题栏中的最大化命令是否可用。`UseTopmost` 和 `ShowInTaskbar` 对应标准 WPF 窗口行为。

## 文本与像素默认值

Shell 根窗口默认启用设备像素对齐和布局取整，并保持 WPF 默认的文本格式化、呈现与 hinting 模式。辅助文本使用 `Regular` 字形，卡片、分区、页面、标题栏和对话框标题使用 `Bold` 字形。

## 托盘关闭行为

`SetTrayExit(true)` 会将关闭命令改为最小化到托盘操作。点击标题栏关闭按钮会立即在 Windows 通知区域中隐藏窗口，不会打开关闭确认对话框；双击托盘图标或选择“显示”会恢复窗口，选择“退出”会关闭应用。

```csharp
builder.ConfigureWindow(window => window.SetTrayExit());
```

托盘关闭行为禁用时，标题栏关闭按钮会使用正常的关闭确认流程。共享配置需要按条件启用托盘行为时，可以传入 `false`。

关闭确认和托盘菜单使用[应用数据](configure-data.md)中选择的语言。

## 相关功能

- [快速开始](getting-started.md)演示如何从 `App.xaml.cs` 启动窗口。
- [标题栏](configure-title-bar.md)控制窗口内显示的标题栏界面。
- [材质特效](configure-material-effect.md)更改窗口背景材质。
