---
title: Window
description: Set the Flourish shell window size, position, state, and WPF window behavior.
---

# Window

Every Flourish shell has a WPF window. Use `ConfigureWindow` to choose its initial dimensions, placement, state, resize behavior, taskbar visibility, topmost behavior, and close-to-tray behavior.

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
