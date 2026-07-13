---
title: 窗口
description: 配置 Flourish Shell 窗口的尺寸、位置、渲染和 WPF 窗口行为。
---

# 窗口

每个 Flourish Shell 都具有一个 WPF 窗口。使用 `ConfigureWindow` 可以设置初始尺寸、尺寸约束、启动位置、窗口状态、渲染行为、置顶行为、任务栏可见性和托盘关闭流程。这些设置不需要额外的 Shell 功能开关。

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
        .UseTextStrategy()
        .SnapsToDevicePixels()
        .UseLayoutRounding()
        .UseTopmost(false)
        .ShowInTaskbar(true)
        .SetTrayExit();
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

## 文本呈现与像素对齐

以下方法会在 Shell 窗口上设置对应的可继承 WPF 属性：

```csharp
window
    .UseTextStrategy(TextFormattingMode.Display, TextRenderingMode.ClearType)
    .SnapsToDevicePixels()
    .UseLayoutRounding();
```

无参调用 `UseTextStrategy()` 会选择 `Display` 文本格式化模式和 `ClearType` 文本呈现模式。`SnapsToDevicePixels()` 和 `UseLayoutRounding()` 默认启用各自对应的 WPF 行为；向任一方法传入 `false` 可以将其禁用。

后代元素会继承这些设置，但自身的本地值或样式可以覆盖继承值。未调用某个方法时，Flourish 不会在窗口上设置对应值。最终显示效果仍可能受到字体、显示缩放、渲染表面和后代元素覆盖值的影响。

## 托盘关闭行为

`SetTrayExit()` 启用通知区域流程。启用后，点击标题栏关闭按钮会直接隐藏窗口并显示托盘图标，不会打开退出确认对话框；双击托盘图标或选择显示操作可以恢复窗口，选择退出操作才会退出应用。

```csharp
builder.ConfigureWindow(window => window.SetTrayExit());
```

调用 `SetTrayExit(false)` 会禁用该流程。此时标题栏关闭按钮会显示退出确认对话框，确认后关闭窗口。

关闭确认和托盘菜单使用[应用数据](configure-data.md)中选择的语言。

## 相关功能

- [快速入门](getting-started.md)演示如何从 `App.xaml.cs` 启动窗口。
- [标题栏](configure-title-bar.md)控制窗口内显示的标题栏界面。
- [材质特效](configure-material-effect.md)更改窗口背景材质。
