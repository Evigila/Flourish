namespace ArkheideSystem.Flourish.Abstract.Builder;

/// <summary>
/// Configures the Flourish shell status bar.
/// </summary>
/// <example>
/// <code><![CDATA[
/// builder.ConfigStatusBar(statusBar =>
/// {
///     statusBar
///         .InitStatusItem("Ready", "\uE73E")
///         .UseLanConnectionStatus()
///         .UsePowerStatus();
/// });
/// ]]></code>
/// </example>
public interface IFlourishStatusBarBuilder
{
    /// <summary>
    /// Adds a status item with display text and an icon glyph.
    /// </summary>
    /// <param name="displayText">The status item display text.</param>
    /// <param name="iconGlyph">The icon glyph displayed before the text.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishStatusBarBuilder InitStatusItem(string displayText = "OK", string iconGlyph = "\uE930");

    /// <summary>
    /// Enables the built-in LAN connection status in the consolidated system status surface.
    /// </summary>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishStatusBarBuilder UseLanConnectionStatus(bool enabled = true);

    /// <summary>
    /// Enables the built-in power status in the consolidated system status surface.
    /// </summary>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishStatusBarBuilder UsePowerStatus(bool enabled = true);
}
