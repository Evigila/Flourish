namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Represents an immutable notification-area state snapshot.
/// </summary>
/// <param name="IsEnabled">Whether notification-area behavior is enabled.</param>
/// <param name="IsIconVisible">Whether the notification-area icon is currently visible.</param>
/// <param name="IsWindowHidden">Whether the attached shell window is hidden.</param>
/// <param name="IsExitRequested">Whether exit has been requested from the notification area.</param>
/// <param name="ToolTipText">The normalized notification-area icon tooltip.</param>
public sealed record TrayState(
    bool IsEnabled,
    bool IsIconVisible,
    bool IsWindowHidden,
    bool IsExitRequested,
    string ToolTipText
);
