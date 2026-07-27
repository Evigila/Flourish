namespace ArkheideSystem.Flourish.Abstract.Runtime;

/// <summary>
/// Controls Flourish notification-area behavior at runtime.
/// </summary>
public interface ITrayService
{
    /// <summary>
    /// Gets an immutable snapshot of notification-area and shell visibility state.
    /// </summary>
    FlourishTrayState Current { get; }

    /// <summary>
    /// Occurs synchronously after notification-area state changes.
    /// </summary>
    event EventHandler<FlourishTrayStateChangedEventArgs>? StateChanged;

    /// <summary>
    /// Enables or disables notification-area behavior at runtime.
    /// </summary>
    /// <param name="enabled"><see langword="true"/> to enable it; otherwise, <see langword="false"/>.</param>
    void SetEnabled(bool enabled);

    /// <summary>
    /// Changes the notification-area icon tooltip.
    /// </summary>
    /// <param name="text">The tooltip text. Empty text uses the default product name; text longer than 63 characters is truncated.</param>
    void SetToolTip(string text);

    /// <summary>
    /// Hides the shell window and shows its notification-area icon.
    /// </summary>
    /// <returns><see langword="true"/> when the window was hidden; otherwise, <see langword="false"/>.</returns>
    bool MinimizeToTray();

    /// <summary>
    /// Restores and activates the shell window and hides its notification-area icon.
    /// </summary>
    void Restore();

    /// <summary>
    /// Requests application exit from the notification area.
    /// </summary>
    void Exit();
}
