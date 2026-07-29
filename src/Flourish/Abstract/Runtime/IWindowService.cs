using System.Windows;

namespace ArkheideSystem.Flourish.Abstract.Runtime;

/// <summary>
/// Controls the Flourish shell window while the application is running.
/// </summary>
public interface IWindowService
{
    /// <summary>
    /// Gets an immutable snapshot of the shell window or, before attachment, its configured initial state.
    /// </summary>
    FlourishWindowState Current { get; }

    /// <summary>
    /// Occurs synchronously after a requested window change or an observed native window state change.
    /// </summary>
    event EventHandler<FlourishWindowStateChangedEventArgs>? Changed;

    /// <summary>
    /// Sets the shell window position and size in device-independent pixels.
    /// </summary>
    /// <param name="bounds">Finite coordinates and strictly positive dimensions.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="bounds"/> contains invalid coordinates or dimensions.</exception>
    void SetBounds(Rect bounds);

    /// <summary>
    /// Sets the shell window size in device-independent pixels.
    /// </summary>
    /// <param name="width">A finite width greater than zero.</param>
    /// <param name="height">A finite height greater than zero.</param>
    /// <exception cref="ArgumentOutOfRangeException">A dimension is not finite and greater than zero.</exception>
    void SetSize(double width, double height);

    /// <summary>
    /// Sets the minimum shell window size.
    /// </summary>
    /// <param name="width">A finite, non-negative minimum width.</param>
    /// <param name="height">A finite, non-negative minimum height.</param>
    /// <exception cref="ArgumentOutOfRangeException">A dimension is invalid or exceeds the corresponding maximum.</exception>
    void SetMinimumSize(double width, double height);

    /// <summary>
    /// Sets the maximum shell window size.
    /// </summary>
    /// <param name="width">A positive finite width or <see cref="double.PositiveInfinity"/>.</param>
    /// <param name="height">A positive finite height or <see cref="double.PositiveInfinity"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException">A dimension is invalid or is below the corresponding minimum.</exception>
    void SetMaximumSize(double width, double height);

    /// <summary>
    /// Sets the native window resize behavior.
    /// </summary>
    /// <param name="resizeMode">The WPF resize mode.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="resizeMode"/> is not defined.</exception>
    void SetResizeMode(ResizeMode resizeMode);

    /// <summary>
    /// Sets whether the shell remains above non-topmost windows.
    /// </summary>
    /// <param name="topmost"><see langword="true"/> to make the shell topmost; otherwise, <see langword="false"/>.</param>
    void SetTopmost(bool topmost);

    /// <summary>
    /// Sets whether the shell window appears in the Windows taskbar.
    /// </summary>
    /// <param name="shown"><see langword="true"/> to show the taskbar button; otherwise, <see langword="false"/>.</param>
    void SetShownInTaskbar(bool shown);

    /// <summary>
    /// Centers the shell window in the primary display work area.
    /// </summary>
    void CenterOnScreen();

    /// <summary>
    /// Shows the shell window.
    /// </summary>
    void Show();

    /// <summary>
    /// Hides the shell window without closing it.
    /// </summary>
    void Hide();

    /// <summary>
    /// Attempts to activate the shell window.
    /// </summary>
    void Activate();

    /// <summary>
    /// Minimizes the shell window.
    /// </summary>
    void Minimize();

    /// <summary>
    /// Maximizes the shell window.
    /// </summary>
    void Maximize();

    /// <summary>
    /// Restores the shell window to its normal state.
    /// </summary>
    void Restore();
}
