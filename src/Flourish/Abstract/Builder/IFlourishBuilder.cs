using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ArkheideSystem.Flourish.Abstract.Builder;

/// <summary>
/// Configures a Flourish application before building its runtime.
/// </summary>
/// <example>
/// <code><![CDATA[
/// return FlourishBuilder
///     .CreateDefaultBuilder(args)
///     .ConfigServices((_, services) => services.AddSingleton<App>())
///     .Run<App>();
/// ]]></code>
/// </example>
public interface IFlourishBuilder
{
    /// <summary>
    /// Configures localization.
    /// </summary>
    /// <param name="configureData">A callback that receives the data builder.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// builder.ConfigData(data =>
    /// {
    ///     data.InitLocale("en-US");
    /// });
    /// ]]></code>
    /// </example>
    IFlourishBuilder ConfigData(Action<IFlourishDataBuilder> configureData);

    /// <summary>
    /// Registers application-owned configuration sources in the .NET Host pipeline.
    /// </summary>
    /// <param name="configure">
    /// A callback that receives the Host context and Flourish configuration source builder.
    /// </param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <remarks>
    /// Sources are inserted after Host appsettings and User Secrets, and before environment
    /// variables and command-line arguments. Flourish retains control of provider ordering while
    /// the final configuration remains the standard Microsoft <c>IConfiguration</c>.
    /// </remarks>
    /// <example>
    /// <code><![CDATA[
    /// builder.ConfigConfiguration((_, configuration) =>
    ///     configuration.UseConfigurationFile(
    ///         "appsettings.User.json",
    ///         optional: true,
    ///         reloadOnChange: true));
    /// ]]></code>
    /// </example>
    IFlourishBuilder ConfigConfiguration(
        Action<HostBuilderContext, IFlourishConfigurationBuilder> configure
    );

    /// <summary>
    /// Adds service registrations to the underlying .NET host builder.
    /// </summary>
    /// <param name="configureServices">A callback that receives the host context and service collection.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// builder.ConfigServices((_, services) =>
    /// {
    ///     services.AddSingleton<App>();
    ///     services.AddNavigable<HomePage>("Home", "\uE80F");
    /// });
    /// ]]></code>
    /// </example>
    IFlourishBuilder ConfigServices(Action<HostBuilderContext, IServiceCollection> configureServices);

    /// <summary>
    /// Configures Flourish shell features and shared options.
    /// </summary>
    /// <param name="configureShell">A callback that receives the shell builder.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// builder.ConfigShell(shell =>
    /// {
    ///     shell.UseTitleBar().UseNavigation().UseDynamicToolbar();
    /// });
    /// ]]></code>
    /// </example>
    IFlourishBuilder ConfigShell(Action<IFlourishShellBuilder> configureShell);

    /// <summary>
    /// Configures the page hosted by the profile flyout.
    /// </summary>
    /// <param name="configureProfile">A callback that receives the profile builder.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishBuilder ConfigProfile(Action<IFlourishProfileBuilder> configureProfile);

    /// <summary>
    /// Configures the title bar displayed when it is enabled through <see cref="ConfigShell" />.
    /// </summary>
    /// <param name="configureTitleBar">A callback that receives the title bar builder.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// builder.ConfigTitleBar(titleBar =>
    /// {
    ///     titleBar.InitApplicationTitle("Foobar");
    /// });
    /// ]]></code>
    /// </example>
    IFlourishBuilder ConfigTitleBar(Action<IFlourishTitlebarBuilder> configureTitleBar);

    /// <summary>
    /// Configures the visible navigation model.
    /// </summary>
    /// <param name="configureNavigation">A callback that receives the navigation builder.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <remarks>
    /// The navigation surface is displayed only when <see cref="IFlourishShellBuilder.UseNavigation" />
    /// enables it through <see cref="ConfigShell" />.
    /// </remarks>
    /// <example>
    /// <code><![CDATA[
    /// builder.ConfigNavigation(navigation =>
    /// {
    ///     navigation.AddGroup("Navigation", groupId: 0, group =>
    ///     {
    ///         group.AddNavigableViewItem<HomePage>(isInitial: true);
    ///     });
    /// });
    /// ]]></code>
    /// </example>
    IFlourishBuilder ConfigNavigation(Action<IFlourishNavigationBuilder> configureNavigation);

    /// <summary>
    /// Configures custom WPF elements displayed in predefined Flourish regions.
    /// </summary>
    /// <param name="configureCustomHandler">A callback that receives the custom handler builder.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// builder.ConfigCustomHandler(custom =>
    /// {
    ///     custom.Add(
    ///         FlourishRegion.TitlebarEnd,
    ///         services => new Button { Content = "Account" });
    /// });
    /// ]]></code>
    /// </example>
    IFlourishBuilder ConfigCustomHandler(
        Action<IFlourishCustomHandlerBuilder> configureCustomHandler
    );

    /// <summary>
    /// Configures page-specific dynamic toolbar items.
    /// </summary>
    /// <param name="configureToolbar">A callback that receives the dynamic toolbar builder.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// builder.ConfigDynamicToolbar(toolbar =>
    /// {
    ///     toolbar.InitToolbarItems<ReportsPage>(
    ///         new FlourishToolbarItem("Export", "\uE898", "reports.export"));
    /// });
    /// ]]></code>
    /// </example>
    IFlourishBuilder ConfigDynamicToolbar(
        Action<IFlourishDynamicToolbarBuilder> configureToolbar
    );

    /// <summary>
    /// Configures motion behavior used when motion is enabled through <see cref="ConfigShell" />.
    /// </summary>
    /// <param name="configureMotion">A callback that receives the motion builder.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishBuilder ConfigMotion(Action<IFlourishMotionBuilder> configureMotion);

    /// <summary>
    /// Configures shell window properties.
    /// </summary>
    /// <param name="configureWindow">A callback that receives the window property builder.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishBuilder ConfigWindow(Action<IFlourishWindowPropertyBuilder> configureWindow);

    /// <summary>
    /// Configures the shell status bar.
    /// </summary>
    /// <param name="configureStatusBar">A callback that receives the status bar builder.</param>
    /// <returns>The current builder for chained configuration.</returns>
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
    IFlourishBuilder ConfigStatusBar(
        Action<IFlourishStatusBarBuilder> configureStatusBar
    );

    /// <summary>
    /// Builds the Flourish runtime.
    /// </summary>
    /// <returns>An <see cref="IFlourish" /> runtime that can be started and disposed.</returns>
    /// <remarks>
    /// Building consumes this builder. A second build, later <c>Config...</c> call, or mutation
    /// through a nested builder captured from a completed callback throws
    /// <see cref="InvalidOperationException" />.
    /// </remarks>
    /// <example>
    /// <code><![CDATA[
    /// using var flourish = builder.Build();
    /// return flourish.Run<App>();
    /// ]]></code>
    /// </example>
    IFlourish Build();
}
