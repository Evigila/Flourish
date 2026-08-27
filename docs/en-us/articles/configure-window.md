---
title: Window
description: Set the Flourish shell window size, position, and WPF window behavior.
---

# Window

Use `ConfigureWindow` to set the Shell window's size, placement, state, taskbar visibility, topmost state, and close behavior.

## Configure the window

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

Window configuration does not depend on the feature switches in [Shell configuration](shell-configuration.md).

## Sizing and placement

Initial and minimum dimensions must be finite positive values. Maximum dimensions accept positive values or `double.PositiveInfinity`, and the minimum cannot exceed the maximum. `SetManualPosition` switches the startup location to `Manual` and stores the requested coordinates.

```csharp
window.SetManualPosition(left: 120, top: 80);
```

Use either a `WindowStartupLocation` or manual coordinates to make startup placement explicit.

Size, startup/manual position, state, topmost, and tray-exit preferences are restored and updated by default. Pass `usePersistedPreference: false` on the corresponding method when its configured value must always win. Size and position are restored as complete pairs, `Minimized` is never restored, and an off-screen saved position is moved into the reachable virtual desktop.

```csharp
window
    .SetSize(1280, 720)
    .SetManualPosition(120, 80)
    .SetState(WindowState.Normal)
    .SetTopmost(false)
    .SetTrayExit();
```

## Window behavior

`SetResizeMode` controls whether the custom title bar maximize command is available. `SetShownInTaskbar` and `SetTopmost` map to normal WPF window behavior.

When maximized, caption buttons extend to the screen edges; restoring the window restores its resizable edge.

## Text and pixel defaults

The shell root enables device-pixel snapping and layout rounding. Flourish does not override WPF text formatting, rendering, or hinting modes. Supporting text uses the `Regular` face, while card, section, page, title-bar, and dialog headings use `Bold`.

## Project close guard

In multi-project mode, actual close requests run `IProjectBehavior.CanCloseAsync`. The default behavior offers **Save**, **Don't save**, and **Cancel** for an active project with `StoragePath == null`; outside project mode no project prompt appears.

This guard applies to the title-bar close command, a direct window close, application close requests, and **Exit** from the notification-area menu. An application-provided `IProjectBehavior` can replace the decision and save workflow. See [Projects](projects.md).

## Close to the notification area

`SetTrayExit(true)` makes the close command hide the window without running close guards. Double-click the tray icon or select Show to restore it; Exit runs the actual close flow.

```csharp
builder.ConfigureWindow(window => window.SetTrayExit());
```

When tray exit is disabled, the title bar close button uses the normal close-confirmation and project-guard flow. Passing `false` is useful when a shared configuration enables tray behavior conditionally.

The close confirmation and tray menu use the locale selected through [Application data](configure-data.md).

## Related features

- [Getting started](getting-started.md)
- [Title bar](configure-title-bar.md)
- [Projects](projects.md)
- [Material effects](configure-material-effect.md)
