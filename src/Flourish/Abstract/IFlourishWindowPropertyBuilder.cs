using System.Windows;
using System.Windows.Media;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Configures the Flourish shell window.
/// </summary>
/// <example>
/// <code><![CDATA[
/// builder.ConfigureWindow(window =>
/// {
///     window.SetWindowSize(1280, 720);
/// });
/// ]]></code>
/// </example>
public interface IFlourishWindowPropertyBuilder
{
    /// <summary>
    /// Sets the initial shell window size.
    /// </summary>
    /// <param name="width">The initial window width.</param>
    /// <param name="height">The initial window height.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// window.SetWindowSize(1536, 864);
    /// ]]></code>
    /// </example>
    IFlourishWindowPropertyBuilder SetWindowSize(double width = 1536, double height = 864);

    /// <summary>
    /// Sets the minimum shell window size.
    /// </summary>
    /// <param name="minWidth">The minimum window width.</param>
    /// <param name="minHeight">The minimum window height.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// window.SetWindowMinSize(1280, 720);
    /// ]]></code>
    /// </example>
    IFlourishWindowPropertyBuilder SetWindowMinSize(double minWidth = 1280, double minHeight = 720);

    /// <summary>
    /// Sets the maximum shell window size.
    /// </summary>
    /// <param name="maxWidth">The maximum window width.</param>
    /// <param name="maxHeight">The maximum window height.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// window.SetWindowMaxSize(1920, 1080);
    /// ]]></code>
    /// </example>
    IFlourishWindowPropertyBuilder SetWindowMaxSize(
        double maxWidth = double.PositiveInfinity,
        double maxHeight = double.PositiveInfinity
    );

    /// <summary>
    /// Sets the shell window startup position.
    /// </summary>
    /// <param name="startupLocation">The WPF startup location used by the shell window.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// window.SetWindowPosition(WindowStartupLocation.CenterScreen);
    /// ]]></code>
    /// </example>
    IFlourishWindowPropertyBuilder SetWindowPosition(
        WindowStartupLocation startupLocation = WindowStartupLocation.CenterScreen
    );

    /// <summary>
    /// Sets a manual shell window position.
    /// </summary>
    /// <param name="left">The left coordinate of the shell window.</param>
    /// <param name="top">The top coordinate of the shell window.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// window.SetManualWindowPosition(left: 40, top: 40);
    /// ]]></code>
    /// </example>
    IFlourishWindowPropertyBuilder SetManualWindowPosition(double left = 0, double top = 0);

    /// <summary>
    /// Sets the initial shell window state.
    /// </summary>
    /// <param name="windowState">The initial WPF window state.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// window.SetWindowState(WindowState.Maximized);
    /// ]]></code>
    /// </example>
    IFlourishWindowPropertyBuilder SetWindowState(WindowState windowState = WindowState.Normal);

    /// <summary>
    /// Sets the shell window resize mode.
    /// </summary>
    /// <param name="resizeMode">The WPF resize mode used by the shell window.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// window.SetWindowResizeMode(ResizeMode.CanResize);
    /// ]]></code>
    /// </example>
    IFlourishWindowPropertyBuilder SetWindowResizeMode(ResizeMode resizeMode = ResizeMode.CanResize);

    /// <summary>
    /// Sets the WPF text formatting and rendering modes inherited by shell content.
    /// </summary>
    /// <param name="textFormattingMode">The WPF text formatting mode applied to the shell window.</param>
    /// <param name="textRenderingMode">The WPF text rendering mode applied to the shell window.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <remarks>
    /// Descendants can override either inherited value through their own WPF text options.
    /// </remarks>
    /// <example>
    /// <code><![CDATA[
    /// window.UseTextStrategy(TextFormattingMode.Display, TextRenderingMode.ClearType);
    /// ]]></code>
    /// </example>
    IFlourishWindowPropertyBuilder UseTextStrategy(
        TextFormattingMode textFormattingMode = TextFormattingMode.Display,
        TextRenderingMode textRenderingMode = TextRenderingMode.ClearType
    ) =>
        throw new NotSupportedException(
            "This IFlourishWindowPropertyBuilder implementation does not support text rendering configuration."
        );

    /// <summary>
    /// Sets whether the shell window snaps rendering to device pixels.
    /// </summary>
    /// <param name="enabled">A value indicating whether device-pixel snapping should be enabled.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// window.SnapsToDevicePixels();
    /// ]]></code>
    /// </example>
    IFlourishWindowPropertyBuilder SnapsToDevicePixels(bool enabled = true) =>
        throw new NotSupportedException(
            "This IFlourishWindowPropertyBuilder implementation does not support device-pixel snapping configuration."
        );

    /// <summary>
    /// Sets whether layout rounding is applied to the shell window during layout.
    /// </summary>
    /// <param name="enabled">A value indicating whether layout rounding should be enabled.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// window.UseLayoutRounding();
    /// ]]></code>
    /// </example>
    IFlourishWindowPropertyBuilder UseLayoutRounding(bool enabled = true) =>
        throw new NotSupportedException(
            "This IFlourishWindowPropertyBuilder implementation does not support layout-rounding configuration."
        );

    /// <summary>
    /// Sets whether the shell window should stay above other windows.
    /// </summary>
    /// <param name="enabled">A value indicating whether topmost behavior should be enabled.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// window.UseTopmost(false);
    /// ]]></code>
    /// </example>
    IFlourishWindowPropertyBuilder UseTopmost(bool enabled = true);

    /// <summary>
    /// Sets whether the shell window is shown in the Windows taskbar.
    /// </summary>
    /// <param name="enabled">A value indicating whether the window should be shown in the taskbar.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// window.ShowInTaskbar(true);
    /// ]]></code>
    /// </example>
    IFlourishWindowPropertyBuilder ShowInTaskbar(bool enabled = true);

    /// <summary>
    /// Sets whether the title bar close button hides the shell window in the notification area.
    /// </summary>
    /// <param name="enabled">A value indicating whether closing to the notification area should be enabled.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <remarks>
    /// When enabled, the close button hides the window without showing the exit confirmation.
    /// Use the notification-area menu to restore the window or exit the application. When
    /// disabled, the close button uses the normal exit confirmation flow.
    /// </remarks>
    IFlourishWindowPropertyBuilder SetTrayExit(bool enabled = true);
}
