---
title: ConfigureWindow
description: Configure Flourish shell window size, position, and window behavior.
---

# ConfigureWindow

`ConfigureWindow` configures shell window properties. It is independent from [`ConfigureShell`](configure-shell.md) feature switches because every shell needs a window.

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

## Details

Size methods validate finite positive values and enforce min/max consistency. `SetManualWindowPosition` switches the startup location to `Manual` and stores the requested coordinates.

`SetWindowResizeMode` controls whether the custom title bar maximize command is available. `ShowInTaskbar` and `UseTopmost` map to normal WPF window behavior.

Window configuration belongs near shell composition, while WPF resources and application lifetime still belong to the WPF application.

## Related APIs

- [`Getting started`](getting-started.md) shows startup from `App.xaml.cs` or another application entry point.
- [`ConfigureTitleBar`](configure-title-bar.md) controls title bar buttons shown inside the configured window.
- [`ConfigureMaterialEffect`](configure-material-effect.md) changes the window background material.
