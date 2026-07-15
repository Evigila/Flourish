using System.Windows.Controls;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Configures Flourish shell features and shared options.
/// </summary>
/// <example>
/// <code><![CDATA[
/// builder.ConfigureShell(shell =>
/// {
///     shell.UseTitleBar()
///          .UseNavigation()
///          .UseCenterContent(enabled: true, contentWidth: 1200)
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
    /// Enables or disables a maximum width for navigated page content. Content stretches across
    /// narrower viewports and is centered after the viewport exceeds <paramref name="contentWidth" />.
    /// The page scrolling surface remains full width, so its vertical scrollbar stays at the edge
    /// of the Shell content area, including while the window is maximized.
    /// </summary>
    /// <param name="enabled">
    /// A value indicating whether the content width limit should be enabled.
    /// </param>
    /// <param name="contentWidth">
    /// The finite, positive maximum width of navigated page content in device-independent pixels.
    /// </param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishShellBuilder UseCenterContent(
        bool enabled = true,
        double contentWidth = 1200
    );

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
    /// Configures the shell material effect. <see cref="MaterialEffect.None"/> disables the effect.
    /// </summary>
    /// <param name="effect">The material effect applied to the shell window.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishShellBuilder UseMaterialEffect(
        MaterialEffect effect = MaterialEffect.Mica
    );

    /// <summary>
    /// Configures the primary, secondary, and accent colors used by Flourish theme resources.
    /// </summary>
    /// <param name="colors">The application theme colors.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishShellBuilder UseThemeColors(FlourishThemeColors colors);

    /// <summary>
    /// Configures the shared corner radius used by Flourish controls and surfaces.
    /// </summary>
    /// <param name="radius">A finite, non-negative radius in device-independent pixels.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishShellBuilder UseCornerRadius(double radius);

    /// <summary>
    /// Configures the global font used by Flourish shell UI and application pages.
    /// </summary>
    /// <param name="fontFamily">The font family name.</param>
    /// <param name="fontSize">The base font size.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishShellBuilder UseGlobalFont(string fontFamily, double fontSize = 14);

    /// <summary>
    /// Overrides the font used by one page type while leaving all other pages on the global font.
    /// </summary>
    /// <typeparam name="TPage">The WPF page type that receives the override.</typeparam>
    /// <param name="fontFamily">The page-specific font family name.</param>
    /// <param name="fontSize">
    /// The page-specific base font size, or <see langword="null"/> to continue following the global size.
    /// </param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishShellBuilder SetOverrideFont<TPage>(
        string fontFamily,
        double? fontSize = null
    )
        where TPage : Page;

    /// <summary>
    /// Enables or disables the shell status bar.
    /// </summary>
    /// <param name="enabled">A value indicating whether the status bar should be enabled.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishShellBuilder UseStatusBar(bool enabled = true);
}
