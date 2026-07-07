using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AckSS.Flourish.Abstract;

/// <summary>
/// Configures services, shell options, navigation items, toolbar items, custom content, and footer status items before building a Flourish runtime.
/// </summary>
/// <example>
/// <code><![CDATA[
/// return FlourishBuilder
///     .CreateDefaultBuilder(args)
///     .ConfigureServices((_, services) => services.AddSingleton<App>())
///     .Run<App>();
/// ]]></code>
/// </example>
public interface IFlourishBuilder
{
    /// <summary>
    /// Configures application-level data and preference storage.
    /// </summary>
    /// <param name="configureData">A callback that receives the data builder.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// builder.ConfigureData(data =>
    /// {
    ///     data.SetAppCompany("Acksheed System Team").SetAppName("Gallery");
    /// });
    /// ]]></code>
    /// </example>
    IFlourishBuilder ConfigureData(Action<IFlourishDataBuilder> configureData);

    /// <summary>
    /// Adds service registrations to the underlying .NET host builder.
    /// </summary>
    /// <param name="configureServices">A callback that receives the host context and service collection.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// builder.ConfigureServices((_, services) =>
    /// {
    ///     services.AddSingleton<App>();
    ///     services.AddNavigable<HomePage>("Home", "\uE80F");
    /// });
    /// ]]></code>
    /// </example>
    IFlourishBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureServices);

    /// <summary>
    /// Configures the Flourish shell feature switches.
    /// </summary>
    /// <param name="configureShell">A callback that receives the shell builder.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// builder.ConfigureShell(shell =>
    /// {
    ///     shell.UseTitleBar().UseNavigation().UseDynamicToolbar();
    /// });
    /// ]]></code>
    /// </example>
    IFlourishBuilder ConfigureShell(Action<IFlourishShellBuilder> configureShell);

    /// <summary>
    /// Configures the title bar displayed when it is enabled through <see cref="ConfigureShell" />.
    /// </summary>
    /// <param name="configureTitleBar">A callback that receives the title bar builder.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// builder.ConfigureTitleBar(titleBar =>
    /// {
    ///     titleBar.ShowSearch().SetTitle("Gallery");
    /// });
    /// ]]></code>
    /// </example>
    IFlourishBuilder ConfigureTitleBar(Action<IFlourishTitlebarBuilder> configureTitleBar);

    /// <summary>
    /// Configures the visible navigation model.
    /// </summary>
    /// <param name="configureNavigation">A callback that receives the navigation builder.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <remarks>
    /// The navigation surface is displayed only when <see cref="IFlourishShellBuilder.UseNavigation" />
    /// enables it through <see cref="ConfigureShell" />.
    /// </remarks>
    /// <example>
    /// <code><![CDATA[
    /// builder.ConfigureNavigation(navigation =>
    /// {
    ///     navigation.SetGroup("Navigation", groupId: 0, group =>
    ///     {
    ///         group.AddNavigableViewItem<HomePage>(isInitial: true);
    ///     });
    /// });
    /// ]]></code>
    /// </example>
    IFlourishBuilder ConfigureNavigation(Action<IFlourishNavigationBuilder> configureNavigation);

    /// <summary>
    /// Configures custom WPF elements displayed in predefined Flourish regions.
    /// </summary>
    /// <param name="configureCustomHandler">A callback that receives the custom handler builder.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// builder.ConfigureCustomHandler(custom =>
    /// {
    ///     custom.Add(
    ///         FlourishRegion.TitlebarEnd,
    ///         services => new Button { Content = "Account" });
    /// });
    /// ]]></code>
    /// </example>
    IFlourishBuilder ConfigureCustomHandler(
        Action<IFlourishCustomHandlerBuilder> configureCustomHandler
    );

    /// <summary>
    /// Configures page-specific dynamic toolbar items.
    /// </summary>
    /// <param name="configureToolbar">A callback that receives the dynamic toolbar builder.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// builder.ConfigureDynamicToolbar(toolbar =>
    /// {
    ///     toolbar.CreateToolbarItems<HomePage>(new FlourishToolbarItem("Open", "\uE8E5", "home.open"));
    /// });
    /// ]]></code>
    /// </example>
    IFlourishBuilder ConfigureDynamicToolbar(
        Action<IFlourishDynamicToolbarBuilder> configureToolbar
    );

    /// <summary>
    /// Configures tooltip behavior displayed when tips are enabled through <see cref="ConfigureShell" />.
    /// </summary>
    /// <param name="configureTips">A callback that receives the tips builder.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishBuilder ConfigureTips(Action<IFlourishTipsBuilder> configureTips);

    /// <summary>
    /// Configures motion behavior used when motion is enabled through <see cref="ConfigureShell" />.
    /// </summary>
    /// <param name="configureMotion">A callback that receives the motion builder.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishBuilder ConfigureMotion(Action<IFlourishMotionBuilder> configureMotion);

    /// <summary>
    /// Configures shell window properties.
    /// </summary>
    /// <param name="configureWindow">A callback that receives the window property builder.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishBuilder ConfigureWindow(Action<IFlourishWindowPropertyBuilder> configureWindow);

    /// <summary>
    /// Configures the global font used by Flourish shell UI.
    /// </summary>
    /// <param name="fontFamily">The font family name.</param>
    /// <param name="fontSize">The base font size.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishBuilder ConfigureFont(string fontFamily, double fontSize = 14);

    /// <summary>
    /// Configures the material effect used when material effects are enabled through <see cref="ConfigureShell" />.
    /// </summary>
    /// <param name="effect">The material effect to apply.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishBuilder ConfigureMaterialEffect(MaterialEffect effect = MaterialEffect.Mica);

    /// <summary>
    /// Configures the default theme used when themes are enabled through <see cref="ConfigureShell" />.
    /// </summary>
    /// <param name="defaultTheme">The theme used when no user preference has been saved.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishBuilder ConfigureThemes(FlourishTheme defaultTheme = FlourishTheme.System);

    /// <summary>
    /// Configures the shell footer status area.
    /// </summary>
    /// <param name="configureFooter">A callback that receives the footer builder.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// builder.ConfigureFooter(footer =>
    /// {
    ///     footer.SetStatusText("Ready").ShowPowerStatus();
    /// });
    /// ]]></code>
    /// </example>
    IFlourishBuilder ConfigureFooter(Action<IFlourishFooterBuilder> configureFooter);

    /// <summary>
    /// Builds the Flourish runtime.
    /// </summary>
    /// <returns>An <see cref="IFlourish" /> runtime that can be started and disposed.</returns>
    /// <example>
    /// <code><![CDATA[
    /// using var flourish = builder.Build();
    /// return flourish.Run<App>();
    /// ]]></code>
    /// </example>
    IFlourish Build();
}
