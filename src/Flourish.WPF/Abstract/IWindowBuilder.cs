using System.Windows;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Configures the Flourish shell window.
/// </summary>
/// <example>
/// <code><![CDATA[
/// builder.ConfigureWindow(window =>
/// {
///     window.SetSize(1280, 720);
/// });
/// ]]></code>
/// </example>
public interface IWindowBuilder
{
    /// <summary>
    /// Sets the initial shell window size.
    /// </summary>
    /// <param name="width">The initial window width.</param>
    /// <param name="height">The initial window height.</param>
    /// <param name="usePersistedPreference">Whether the persisted user preference is restored and updated.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// window.SetSize(1536, 864);
    /// ]]></code>
    /// </example>
    IWindowBuilder SetSize(
        double width = 1536,
        double height = 864,
        bool usePersistedPreference = true
    );

    /// <summary>
    /// Sets the minimum shell window size.
    /// </summary>
    /// <param name="minWidth">The minimum window width.</param>
    /// <param name="minHeight">The minimum window height.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// window.SetMinimumSize(1280, 720);
    /// ]]></code>
    /// </example>
    IWindowBuilder SetMinimumSize(double minWidth = 1280, double minHeight = 720);

    /// <summary>
    /// Sets the maximum shell window size.
    /// </summary>
    /// <param name="maxWidth">The maximum window width.</param>
    /// <param name="maxHeight">The maximum window height.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// window.SetMaximumSize(1920, 1080);
    /// ]]></code>
    /// </example>
    IWindowBuilder SetMaximumSize(
        double maxWidth = double.PositiveInfinity,
        double maxHeight = double.PositiveInfinity
    );

    /// <summary>
    /// Sets the shell window startup position.
    /// </summary>
    /// <param name="startupLocation">The WPF startup location used by the shell window.</param>
    /// <param name="usePersistedPreference">Whether the persisted user preference is restored and updated.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// window.SetStartupLocation(WindowStartupLocation.CenterScreen);
    /// ]]></code>
    /// </example>
    IWindowBuilder SetStartupLocation(
        WindowStartupLocation startupLocation = WindowStartupLocation.CenterScreen,
        bool usePersistedPreference = true
    );

    /// <summary>
    /// Sets a manual shell window position.
    /// </summary>
    /// <param name="left">The left coordinate of the shell window.</param>
    /// <param name="top">The top coordinate of the shell window.</param>
    /// <param name="usePersistedPreference">Whether the persisted user preference is restored and updated.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// window.SetManualPosition(left: 40, top: 40);
    /// ]]></code>
    /// </example>
    IWindowBuilder SetManualPosition(
        double left = 0,
        double top = 0,
        bool usePersistedPreference = true
    );

    /// <summary>
    /// Sets the initial shell window state.
    /// </summary>
    /// <param name="windowState">The initial WPF window state.</param>
    /// <param name="usePersistedPreference">Whether the persisted user preference is restored and updated.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// window.SetState(WindowState.Maximized);
    /// ]]></code>
    /// </example>
    IWindowBuilder SetState(
        WindowState windowState = WindowState.Normal,
        bool usePersistedPreference = true
    );

    /// <summary>
    /// Sets the shell window resize mode.
    /// </summary>
    /// <param name="resizeMode">The WPF resize mode used by the shell window.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// window.SetResizeMode(ResizeMode.CanResize);
    /// ]]></code>
    /// </example>
    IWindowBuilder SetResizeMode(ResizeMode resizeMode = ResizeMode.CanResize);

    /// <summary>
    /// Sets whether the shell window should stay above other windows.
    /// </summary>
    /// <param name="enabled">A value indicating whether topmost behavior should be enabled.</param>
    /// <param name="usePersistedPreference">Whether the persisted user preference is restored and updated.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// window.SetTopmost(false);
    /// ]]></code>
    /// </example>
    IWindowBuilder SetTopmost(
        bool enabled = true,
        bool usePersistedPreference = true
    );

    /// <summary>
    /// Sets whether the shell window is shown in the Windows taskbar.
    /// </summary>
    /// <param name="enabled">A value indicating whether the window should be shown in the taskbar.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// window.SetShownInTaskbar(true);
    /// ]]></code>
    /// </example>
    IWindowBuilder SetShownInTaskbar(bool enabled = true);

    /// <summary>
    /// Sets whether the title bar close button hides the shell window in the notification area.
    /// </summary>
    /// <param name="enabled">A value indicating whether closing to the notification area should be enabled.</param>
    /// <param name="usePersistedPreference">Whether the persisted user preference is restored and updated.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <remarks>
    /// When enabled, the close button hides the window without showing the exit confirmation.
    /// Use the notification-area menu to restore the window or exit the application. When
    /// disabled, the close button uses the normal exit confirmation flow.
    /// </remarks>
    IWindowBuilder SetTrayExit(
        bool enabled = true,
        bool usePersistedPreference = true
    );
}
