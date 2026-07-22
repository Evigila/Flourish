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

自定义窗口最大化时，标题栏按钮会延伸至屏幕边缘，因此可以从右上角直接执行关闭命令。还原窗口后，可缩放边缘也会恢复。

## 文本与像素默认值

Shell 根窗口默认启用设备像素对齐和布局取整，并保持 WPF 默认的文本格式化、呈现与 hinting 模式。辅助文本使用 `Regular` 字形，卡片、分区、页面、标题栏和对话框标题使用 `Bold` 字形。

## 项目关闭守卫

启用多项目模式后，实际关闭请求会通过窗口关闭守卫管线调用 `IProjectBehavior.CanCloseAsync`。使用默认行为时，如果活动项目的 `StoragePath` 为 `null`，界面会提供“保存”“不保存”和“取消”。保存完成后才能继续关闭；选择“不保存”会在不创建项目文件的情况下关闭；选择“取消”或取消保存对话框会让应用保持打开。未启用多项目模式时，Flourish 不运行项目关闭守卫，也不会显示项目保存提示。

该守卫适用于标题栏关闭命令、直接关闭窗口、应用关闭请求以及通知区域菜单中的“退出”。应用提供的 `IProjectBehavior` 可以替换该决定与保存流程。参见[项目](projects.md)。

## 托盘关闭行为

`SetTrayExit(true)` 会将关闭命令改为最小化到托盘操作。点击标题栏关闭按钮会立即在 Windows 通知区域中隐藏窗口；由于应用并未关闭，因此不会打开关闭确认或项目保存对话框。双击托盘图标或选择“显示”会恢复窗口；选择“退出”会启动实际关闭流程，包括项目关闭守卫。

```csharp
builder.ConfigureWindow(window => window.SetTrayExit());
```

托盘关闭行为禁用时，标题栏关闭按钮会使用正常的关闭确认与项目守卫流程。共享配置需要按条件启用托盘行为时，可以传入 `false`。

关闭确认和托盘菜单使用[应用数据](configure-data.md)中选择的语言。

## 相关功能

- [快速开始](getting-started.md)演示如何从 `App.xaml.cs` 启动窗口。
- [标题栏](configure-title-bar.md)控制窗口内显示的标题栏界面。
- [项目](projects.md)说明多项目模式下实际关闭前的保存、不保存与取消处理。
- [材质特效](configure-material-effect.md)更改窗口背景材质。
