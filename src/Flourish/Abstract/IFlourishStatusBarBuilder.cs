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
    /// Adds the built-in LAN connection status item.
    /// </summary>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishStatusBarBuilder ShowLANConnectionStatus();

    /// <summary>
    /// Adds the built-in power status item.
    /// </summary>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishStatusBarBuilder ShowPowerStatus();
}
