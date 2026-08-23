namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Represents an immutable snapshot of the Flourish shell window.
/// </summary>
/// <param name="Bounds">The window position and size in device-independent pixels.</param>
/// <param name="MinimumSize">The minimum allowed size.</param>
/// <param name="MaximumSize">The maximum allowed size.</param>
/// <param name="WindowState">The native minimized, normal, or maximized state.</param>
/// <param name="ResizeMode">The native resizing behavior.</param>
/// <param name="IsTopmost">Whether the window remains above non-topmost windows.</param>
/// <param name="IsShownInTaskbar">Whether the window has a taskbar button.</param>
/// <param name="IsVisible">Whether the attached window is visible.</param>
/// <param name="IsActive">Whether the attached window is currently active.</param>
public sealed record FlourishWindowState(
    System.Windows.Rect Bounds,
    System.Windows.Size MinimumSize,
    System.Windows.Size MaximumSize,
    System.Windows.WindowState WindowState,
    System.Windows.ResizeMode ResizeMode,
    bool IsTopmost,
    bool IsShownInTaskbar,
    bool IsVisible,
    bool IsActive
);
