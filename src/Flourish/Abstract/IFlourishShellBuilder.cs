namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Configures high-level Flourish shell features.
/// </summary>
/// <example>
/// <code><![CDATA[
/// builder.ConfigureShell(shell =>
/// {
///     shell.UseTitleBar()
///          .UseNavigation()
///          .UseDynamicToolbar()
///          .UseTips(200)
///          .UseMotion()
///          .UseMaterialEffect(MaterialEffect.Mica)
///          .UseGlobalFont("Segoe UI", 14)
///          .UseStatusBar();
/// });
/// ]]></code>
/// </example>
public interface IFlourishShellBuilder
{
    /// <summary>
    /// Enables or disables the shell title bar.
    /// </summary>
    /// <param name="enabled">A value indicating whether the title bar should be enabled.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishShellBuilder UseTitleBar(bool enabled = true);

    /// <summary>
    /// Enables or disables the shell navigation panel.
    /// </summary>
    /// <param name="enabled">A value indicating whether navigation should be enabled.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishShellBuilder UseNavigation(bool enabled = true);

    /// <summary>
    /// Enables or disables the dynamic toolbar surface.
    /// </summary>
    /// <param name="enabled">A value indicating whether the dynamic toolbar should be enabled.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishShellBuilder UseDynamicToolbar(bool enabled = true);

    /// <summary>
    /// Configures and enables Flourish tooltips.
    /// </summary>
    /// <param name="delay">The initial tooltip delay in milliseconds.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishShellBuilder UseTips(int delay = 200);

    /// <summary>
    /// Enables or disables Flourish motion.
    /// </summary>
    /// <param name="enabled">A value indicating whether motion should be enabled.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishShellBuilder UseMotion(bool enabled = true);

    /// <summary>
    /// Configures and enables the shell material effect.
    /// </summary>
    /// <param name="effect">The material effect applied to the shell window.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishShellBuilder UseMaterialEffect(
        MaterialEffect effect = MaterialEffect.Mica
    );

    /// <summary>
    /// Configures the global font used by Flourish shell UI.
    /// </summary>
    /// <param name="fontFamily">The font family name.</param>
    /// <param name="fontSize">The base font size.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishShellBuilder UseGlobalFont(string fontFamily, double fontSize = 14);

    /// <summary>
    /// Enables or disables the shell status bar.
    /// </summary>
    /// <param name="enabled">A value indicating whether the status bar should be enabled.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishShellBuilder UseStatusBar(bool enabled = true);
}
