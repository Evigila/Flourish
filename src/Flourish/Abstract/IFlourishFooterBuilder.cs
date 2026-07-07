namespace AckSS.Flourish.Abstract;

/// <summary>
/// Configures the Flourish shell footer status area.
/// </summary>
/// <example>
/// <code><![CDATA[
/// builder.ConfigureFooter(footer =>
/// {
///     footer.SetStatusText("Ready").ShowPowerStatus();
/// });
/// ]]></code>
/// </example>
public interface IFlourishFooterBuilder
{
    /// <summary>
    /// Sets the primary footer status text.
    /// </summary>
    /// <param name="text">The status text displayed in the shell footer.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// footer.SetStatusText("Ready");
    /// ]]></code>
    /// </example>
    IFlourishFooterBuilder SetStatusText(string text);

    /// <summary>
    /// Adds a footer status item with display text and an icon glyph.
    /// </summary>
    /// <param name="displayText">The status item display text.</param>
    /// <param name="iconGlyph">The icon glyph displayed before the text.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// footer.AddStatusItem("Online", "\uE774");
    /// ]]></code>
    /// </example>
    IFlourishFooterBuilder AddStatusItem(string displayText, string iconGlyph);

    /// <summary>
    /// Shows the built-in LAN connection status item.
    /// </summary>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// footer.ShowLANConnectionStatus();
    /// ]]></code>
    /// </example>
    IFlourishFooterBuilder ShowLANConnectionStatus();

    /// <summary>
    /// Shows the built-in power status item.
    /// </summary>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// footer.ShowPowerStatus();
    /// ]]></code>
    /// </example>
    IFlourishFooterBuilder ShowPowerStatus();
}
