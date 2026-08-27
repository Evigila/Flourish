---
title: 窗口
description: 配置 Flourish Shell 窗口的尺寸、位置和 WPF 窗口行为。
---

# 窗口

使用 `ConfigureWindow` 设置 Shell 窗口的初始尺寸、约束、位置、状态、置顶、任务栏可见性和托盘关闭流程。

## 配置窗口

```csharp
builder.ConfigureWindow(window =>
{
    window
        .SetSize(1280, 720)
        .SetMinimumSize(960, 540)
        .SetMaximumSize(1920, 1080)
        .SetStartupLocation(WindowStartupLocation.CenterScreen)
        .SetState(WindowState.Normal)
        .SetResizeMode(ResizeMode.CanResize)
        .SetTopmost(false)
        .SetShownInTaskbar(true)
        .SetTrayExit();
});
```

窗口配置不依赖 [Shell 配置](shell-configuration.md)中的功能开关。

## 尺寸与位置

初始尺寸和最小尺寸必须是有限正数。最大尺寸可以是正数或 `double.PositiveInfinity`，且不能小于最小尺寸。`SetManualPosition` 会将启动位置设为 `Manual` 并保存指定坐标。

```csharp
window.SetManualPosition(left: 120, top: 80);
```

使用 `WindowStartupLocation` 或手动坐标可明确指定启动位置。

尺寸、启动/手动位置、状态、置顶与托盘退出偏好默认会恢复并更新。某个方法的代码值必须始终优先时，为它传入 `usePersistedPreference: false`。尺寸和位置按完整组合恢复，Flourish 永远不会恢复 `Minimized`，并会把完全移出屏幕的已保存位置移入可触达的虚拟桌面区域。

```csharp
window
    .SetSize(1280, 720)
    .SetManualPosition(120, 80)
    .SetState(WindowState.Normal)
    .SetTopmost(false)
    .SetTrayExit();
```

## 窗口行为

`SetResizeMode` 控制自定义标题栏中的最大化命令是否可用。`SetTopmost` 和 `SetShownInTaskbar` 对应标准 WPF 窗口行为。

自定义窗口最大化时，标题栏按钮会延伸至屏幕边缘，因此可以从右上角直接执行关闭命令。还原窗口后，可缩放边缘也会恢复。

## 文本与像素默认值

Shell 根窗口默认启用设备像素对齐和布局取整，并保持 WPF 默认的文本格式化、呈现与 hinting 模式。辅助文本使用 `Regular` 字形，卡片、分区、页面、标题栏和对话框标题使用 `Bold` 字形。

## 项目关闭守卫

多项目模式下，关闭请求通过守卫调用 `IProjectBehavior.CanCloseAsync`。默认行为在活动项目 `StoragePath == null` 时提供“保存”“不保存”和“取消”：保存成功或不保存可关闭，取消则保持打开。未启用多项目模式时不运行项目守卫，也不显示保存提示。

该守卫适用于标题栏关闭命令、直接关闭窗口、应用关闭请求以及通知区域菜单中的“退出”。应用提供的 `IProjectBehavior` 可以替换该决定与保存流程。参见[项目](projects.md)。

## 托盘关闭行为

`SetTrayExit(true)` 会将关闭命令改为最小化到托盘操作。点击标题栏关闭按钮会立即在 Windows 通知区域中隐藏窗口；由于应用并未关闭，因此不会打开关闭确认或项目保存对话框。双击托盘图标或选择“显示”会恢复窗口；选择“退出”会启动实际关闭流程，包括项目关闭守卫。

```csharp
builder.ConfigureWindow(window => window.SetTrayExit());
```

托盘关闭行为禁用时，标题栏关闭按钮会使用正常的关闭确认与项目守卫流程。共享配置需要按条件启用托盘行为时，可以传入 `false`。

关闭确认和托盘菜单使用[应用数据](configure-data.md)中选择的语言。

## 相关功能

- [快速开始](getting-started.md)
- [标题栏](configure-title-bar.md)
- [项目](projects.md)
- [材质特效](configure-material-effect.md)
