namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Configures the Flourish shell status bar.
/// </summary>
/// <example>
/// <code><![CDATA[
/// builder.ConfigureStatusBar(statusBar =>
/// {
///     statusBar.SetStatusText("Ready").ShowPowerStatus();
/// });
/// ]]></code>
/// </example>
public interface IFlourishStatusBarBuilder
{
    /// <summary>
    /// Sets the primary status text.
    /// </summary>
    /// <param name="text">The text displayed in the shell status bar.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishStatusBarBuilder SetStatusText(string text);

    /// <summary>
    /// Adds a status item with display text and an icon glyph.
    /// </summary>
    /// <param name="displayText">The status item display text.</param>
    /// <param name="iconGlyph">The icon glyph displayed before the text.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishStatusBarBuilder AddStatusItem(string displayText, string iconGlyph);

    /// <summary>
    /// Captures LAN availability during configuration and adds the built-in status item.
    /// The displayed status does not update automatically.
    /// </summary>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishStatusBarBuilder ShowLANConnectionStatus();

    /// <summary>
    /// Adds the built-in static power status item.
    /// The item does not report live power or battery state.
    /// </summary>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishStatusBarBuilder ShowPowerStatus();
}
