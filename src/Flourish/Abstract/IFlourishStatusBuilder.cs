namespace AcksheedSys.Flourish.Abstract;

/// <summary>
/// Configures the Flourish shell status area.
/// </summary>
/// <example>
/// <code><![CDATA[
/// builder.ConfigureStatus((_, status) =>
/// {
///     status.SetStatusText("Ready").ShowPowerStatus();
/// });
/// ]]></code>
/// </example>
public interface IFlourishStatusBuilder
{
    /// <summary>
    /// Sets the primary status text.
    /// </summary>
    /// <param name="text">The status text displayed in the shell status area.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// status.SetStatusText("Ready");
    /// ]]></code>
    /// </example>
    IFlourishStatusBuilder SetStatusText(string text);

    /// <summary>
    /// Adds a status item with display text and an icon glyph.
    /// </summary>
    /// <param name="displayText">The status item display text.</param>
    /// <param name="iconGlyph">The icon glyph displayed before the text.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// status.AddStatusItem("Online", "\uE774");
    /// ]]></code>
    /// </example>
    IFlourishStatusBuilder AddStatusItem(string displayText, string iconGlyph);

    /// <summary>
    /// Shows the built-in LAN connection status item.
    /// </summary>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// status.ShowLANConnectionStatus();
    /// ]]></code>
    /// </example>
    IFlourishStatusBuilder ShowLANConnectionStatus();

    /// <summary>
    /// Shows the built-in power status item.
    /// </summary>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// status.ShowPowerStatus();
    /// ]]></code>
    /// </example>
    IFlourishStatusBuilder ShowPowerStatus();
}
