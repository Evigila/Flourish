using Microsoft.Extensions.Hosting;

namespace AckSS.Flourish.Abstract;

/// <summary>
/// Configures the high-level Flourish shell.
/// </summary>
/// <example>
/// <code><![CDATA[
/// builder.ConfigureShell((_, shell) =>
/// {
///     shell.UseTitlebar((_, titlebar) => titlebar.SetTitle("Gallery"));
/// });
/// ]]></code>
/// </example>
public interface IFlourishShellBuilder
{
    /// <summary>
    /// Enables and configures the shell title bar.
    /// </summary>
    /// <param name="configureTitlebar">A callback that receives the host context and title bar builder.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// shell.UseTitlebar((_, titlebar) =>
    /// {
    ///     titlebar.ShowSearch().SetTitle("Gallery");
    /// });
    /// ]]></code>
    /// </example>
    IFlourishShellBuilder UseTitlebar(
        Action<HostBuilderContext, IFlourishTitlebarBuilder> configureTitlebar
    );

    /// <summary>
    /// Enables and configures the shell navigation panel.
    /// </summary>
    /// <param name="configureNavigationPanel">A callback that receives the host context and navigation panel builder.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <remarks>
    /// Register pages with <c>AddNavigable</c> during service configuration, then use the navigation
    /// panel builder to decide where registered page items and command items are displayed.
    /// </remarks>
    /// <example>
    /// <code><![CDATA[
    /// shell.UseNavigationPanel((_, nav) =>
    /// {
    ///     nav.SetInitiallyOpen()
    ///        .SetGroup("Navigation", groupId: 0, group =>
    ///        {
    ///            group.AddNavigableViewItem<HomePage>(isInitial: true);
    ///            group.AddNavigableViewItem<GalleryPage>();
    ///        })
    ///        .AddFixedNavigableViewItem<SettingsPage>();
    /// });
    /// ]]></code>
    /// </example>
    IFlourishShellBuilder UseNavigationPanel(
        Action<HostBuilderContext, IFlourishNavigationPanelBuilder> configureNavigationPanel
    );

    /// <summary>
    /// Configures custom WPF content displayed in predefined Flourish shell regions.
    /// </summary>
    /// <param name="configureRegions">A callback that receives the host context and region builder.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// shell.UseRegions((_, regions) =>
    /// {
    ///     regions.Add(
    ///         FlourishRegion.TitlebarEnd,
    ///         services => new Button { Content = "Account" });
    /// });
    /// ]]></code>
    /// </example>
    IFlourishShellBuilder UseRegions(
        Action<HostBuilderContext, IFlourishRegionBuilder> configureRegions
    );

    /// <summary>
    /// Configures shell window properties.
    /// </summary>
    /// <param name="configureWindow">A callback that receives the host context and window property builder.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// shell.SetWindowProperty((_, window) =>
    /// {
    ///     window.SetWindowSize(1280, 720).SetWindowMinSize(960, 540);
    /// });
    /// ]]></code>
    /// </example>
    IFlourishShellBuilder SetWindowProperty(
        Action<HostBuilderContext, IFlourishWindowPropertyBuilder> configureWindow
    );

    /// <summary>
    /// Sets the global font used by Flourish shell UI.
    /// </summary>
    /// <param name="fontFamily">The font family name.</param>
    /// <param name="fontSize">The base font size.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// shell.SetGlobalFont("Microsoft YaHei", 14);
    /// ]]></code>
    /// </example>
    IFlourishShellBuilder SetGlobalFont(string fontFamily, double fontSize = 14);

    /// <summary>
    /// Applies a system material effect to the shell window.
    /// </summary>
    /// <param name="effect">The material effect to apply.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// shell.UseMaterialEffect(MaterialEffect.Mica);
    /// ]]></code>
    /// </example>
    IFlourishShellBuilder UseMaterialEffect(MaterialEffect effect = MaterialEffect.Mica);

    /// <summary>
    /// Enables Flourish theme support.
    /// </summary>
    /// <param name="defaultTheme">The theme used when no user preference has been saved.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// shell.UseThemes(FlourishTheme.System);
    /// ]]></code>
    /// </example>
    IFlourishShellBuilder UseThemes(FlourishTheme defaultTheme = FlourishTheme.System);

    /// <summary>
    /// Enables or disables the dynamic toolbar surface.
    /// </summary>
    /// <param name="enabled">A value indicating whether the dynamic toolbar should be enabled.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// shell.UseDynamicToolbar();
    /// ]]></code>
    /// </example>
    IFlourishShellBuilder UseDynamicToolbar(bool enabled = true);

    /// <summary>
    /// Enables or disables Flourish motion using default motion settings.
    /// </summary>
    /// <param name="enabled">A value indicating whether motion should be enabled.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// shell.UseMotion(enabled: true);
    /// ]]></code>
    /// </example>
    IFlourishShellBuilder UseMotion(bool enabled = true);

    /// <summary>
    /// Enables and configures Flourish motion.
    /// </summary>
    /// <param name="configureMotion">A callback that receives the host context and motion builder.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// shell.UseMotion((_, motion) =>
    /// {
    ///     motion.SetDuration(TimeSpan.FromMilliseconds(180));
    /// });
    /// ]]></code>
    /// </example>
    IFlourishShellBuilder UseMotion(
        Action<HostBuilderContext, IFlourishMotionBuilder> configureMotion
    );

    /// <summary>
    /// Configures Flourish tooltips.
    /// </summary>
    /// <param name="configureTips">A callback that receives the host context and tooltip builder.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// shell.UseTips((_, tips) =>
    /// {
    ///     tips.SetDelay(600).SetSpawnableMargin(5);
    /// });
    /// ]]></code>
    /// </example>
    IFlourishShellBuilder UseTips(
        Action<HostBuilderContext, IFlourishTipsBuilder> configureTips
    );
}
