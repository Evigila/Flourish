---
title: Window
description: Set the Flourish shell window size, position, and WPF window behavior.
---

# Window

Every Flourish shell has a WPF window. Use `ConfigureWindow` to choose its initial dimensions, placement, state, taskbar visibility, topmost behavior, and close-to-tray behavior.

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

When the custom window is maximized, its caption buttons extend to the screen edges so the close command remains available from the upper-right corner. Restoring the window also restores its resizable edge.

## Text and pixel defaults

The shell root enables device-pixel snapping and layout rounding. Flourish does not override WPF text formatting, rendering, or hinting modes. Supporting text uses the `Regular` face, while card, section, page, title-bar, and dialog headings use `Bold`.

## Project close guard

When multi-project mode is enabled, an actual close request runs `IProjectBehavior.CanCloseAsync` through the window close-guard pipeline. With the default behavior, an active project whose `StoragePath` is `null` offers **Save**, **Don't save**, and **Cancel**. Saving must complete before closing can continue; choosing **Don't save** closes without creating a project file, while **Cancel** or canceling the Save dialog keeps the application open. When multi-project mode is disabled, Flourish does not run the project close guard or display a project-save prompt.

This guard applies to the title-bar close command, a direct window close, application close requests, and **Exit** from the notification-area menu. An application-provided `IProjectBehavior` can replace the decision and save workflow. See [Projects](projects.md).

## Close to the notification area

`SetTrayExit(true)` changes the close command into a minimize-to-tray action. Clicking the title bar close button hides the window in the Windows notification area immediately and does not open the close confirmation or project-save dialogs because the application is not closing. Double-clicking the tray icon or selecting Show restores the window; selecting Exit starts the actual close flow, including the project close guard.

```csharp
builder.ConfigureWindow(window => window.SetTrayExit());
```

When tray exit is disabled, the title bar close button uses the normal close-confirmation and project-guard flow. Passing `false` is useful when a shared configuration enables tray behavior conditionally.

The close confirmation and tray menu use the locale selected through [Application data](configure-data.md).

## Related features

- [Getting started](getting-started.md) shows window startup from `App.xaml.cs`.
- [Title bar](configure-title-bar.md) controls the chrome displayed inside the window.
- [Projects](projects.md) explains multi-project save, don't-save, and cancel handling before an actual close.
- [Material effects](configure-material-effect.md) change the window background material.
