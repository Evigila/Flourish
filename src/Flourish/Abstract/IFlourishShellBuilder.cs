namespace AckSS.Flourish.Abstract;

/// <summary>
/// Configures high-level Flourish shell feature switches.
/// </summary>
/// <example>
/// <code><![CDATA[
/// builder.ConfigureShell(shell =>
/// {
///     shell.UseTitleBar()
///          .UseNavigation()
///          .UseDynamicToolbar()
///          .UseTips()
///          .UseMotion()
///          .UseMaterialEffect()
///          .UseThemes();
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
    /// Enables or disables Flourish tooltips.
    /// </summary>
    /// <param name="enabled">A value indicating whether tooltips should be enabled.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishShellBuilder UseTips(bool enabled = true);

    /// <summary>
    /// Enables or disables Flourish motion.
    /// </summary>
    /// <param name="enabled">A value indicating whether motion should be enabled.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishShellBuilder UseMotion(bool enabled = true);

    /// <summary>
    /// Enables or disables shell material effects.
    /// </summary>
    /// <param name="enabled">A value indicating whether material effects should be enabled.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishShellBuilder UseMaterialEffect(bool enabled = true);

    /// <summary>
    /// Enables or disables Flourish theme support.
    /// </summary>
    /// <param name="enabled">A value indicating whether themes should be enabled.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishShellBuilder UseThemes(bool enabled = true);

    /// <summary>
    /// Enables or disables the shell footer.
    /// </summary>
    /// <param name="enabled">A value indicating whether the footer should be enabled.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishShellBuilder UseFooter(bool enabled = true);
}
