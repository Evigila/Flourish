namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Configures the Flourish shell status bar.
/// </summary>
/// <example>
/// <code><![CDATA[
/// builder.ConfigureStatusBar(statusBar =>
/// {
///     statusBar
///         .AddStatusItem("Ready", "\uE73E")
///         .SetLanStatusEnabled()
///         .SetPowerStatusEnabled();
/// });
/// ]]></code>
/// </example>
public interface IStatusBarBuilder
{
    /// <summary>Enables or disables the persistent status bar.</summary>
    IStatusBarBuilder SetEnabled(bool enabled = true);

    /// <summary>
    /// Adds a status item with display text and an icon glyph.
    /// </summary>
    /// <param name="displayText">The status item display text.</param>
    /// <param name="iconGlyph">The icon glyph displayed before the text.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IStatusBarBuilder AddStatusItem(string displayText = "OK", string iconGlyph = "\uE930");

    /// <summary>
    /// Enables the built-in LAN connection status in the consolidated system status surface.
    /// </summary>
    /// <returns>The current builder for chained configuration.</returns>
    IStatusBarBuilder SetLanStatusEnabled(bool enabled = true);

    /// <summary>
    /// Enables the built-in power status in the consolidated system status surface.
    /// </summary>
    /// <returns>The current builder for chained configuration.</returns>
    IStatusBarBuilder SetPowerStatusEnabled(bool enabled = true);
}
