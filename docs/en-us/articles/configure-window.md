---
title: Window
description: Set the Flourish shell window size, position, rendering, and WPF window behavior.
---

# Window

Every Flourish shell has a WPF window. Use `ConfigureWindow` to choose its initial dimensions, placement, state, rendering behavior, taskbar visibility, topmost behavior, and close-to-tray behavior.

## Configure the window

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

Window configuration does not depend on the feature switches in [Shell configuration](shell-configuration.md).

## Sizing and placement

Initial and minimum dimensions must be finite positive values. Maximum dimensions accept positive values or `double.PositiveInfinity`, and the minimum cannot exceed the maximum. `SetManualWindowPosition` switches the startup location to `Manual` and stores the requested coordinates.

```csharp
window.SetManualWindowPosition(left: 120, top: 80);
```

Use either a `WindowStartupLocation` or manual coordinates to make startup placement explicit.

## Window behavior

`SetWindowResizeMode` controls whether the custom title bar maximize command is available. `ShowInTaskbar` and `UseTopmost` map to normal WPF window behavior.

## Text rendering and pixel alignment

The rendering methods set the corresponding inherited WPF properties on the shell window:

```csharp
window
    .UseTextStrategy(TextFormattingMode.Display, TextRenderingMode.ClearType)
    .SnapsToDevicePixels()
    .UseLayoutRounding();
```

Calling `UseTextStrategy()` without arguments selects `Display` text formatting and `ClearType` text rendering. `SnapsToDevicePixels()` and `UseLayoutRounding()` enable their WPF behaviors by default; pass `false` to either method to disable that behavior.

Descendants inherit these settings unless they provide their own local value or style. If a method is not called, Flourish does not set the corresponding value on the window. The visible result can still vary with the font, display scale, rendering surface, and descendant overrides.

## Close to the notification area

`SetTrayExit(true)` changes the close command into a minimize-to-tray action. Clicking the title bar close button hides the window in the Windows notification area immediately and does not open the close confirmation dialog. The tray menu can restore the window or exit the application.

```csharp
builder.ConfigureWindow(window => window.SetTrayExit());
```

When tray exit is disabled, the title bar close button uses the normal close-confirmation flow. Passing `false` is useful when a shared configuration enables tray behavior conditionally.

The close confirmation and tray menu use the locale selected through [Application data](configure-data.md).

## Related features

- [Getting started](getting-started.md) shows window startup from `App.xaml.cs`.
- [Title bar](configure-title-bar.md) controls the chrome displayed inside the window.
- [Material effects](configure-material-effect.md) change the window background material.
