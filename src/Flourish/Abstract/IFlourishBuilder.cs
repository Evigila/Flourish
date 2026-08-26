using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using Application = System.Windows.Application;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Configures a Flourish application before building its runtime.
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
    /// Configures localization.
    /// </summary>
    /// <param name="configureData">A callback that receives the data builder.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// builder.ConfigureData(data =>
    /// {
    ///     data.SetLocale("en-US");
    /// });
    /// ]]></code>
    /// </example>
    IFlourishBuilder ConfigureData(Action<IDataBuilder> configureData);

    /// <summary>
    /// Registers application-owned configuration sources in the .NET Host pipeline.
    /// </summary>
    /// <param name="configure">
    /// A callback that receives the Host context and the standard .NET configuration builder.
    /// </param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <remarks>
    /// Sources are inserted after Host appsettings and User Secrets, and before environment
    /// variables and command-line arguments. Flourish retains control of provider ordering while
    /// the final configuration remains the standard Microsoft <c>IConfiguration</c>.
    /// </remarks>
    /// <example>
    /// <code><![CDATA[
    /// builder.ConfigureConfiguration((_, configuration) =>
    ///     configuration.AddJsonFile(
    ///         "appsettings.User.json",
    ///         optional: true,
    ///         reloadOnChange: true));
    /// ]]></code>
    /// </example>
    IFlourishBuilder ConfigureConfiguration(
        Action<HostBuilderContext, IConfigurationBuilder> configure
    );

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

    /// <summary>Configures application appearance defaults.</summary>
    IFlourishBuilder ConfigureAppearance(Action<IAppearanceBuilder> configureAppearance);

    /// <summary>Configures global and page-specific fonts.</summary>
    IFlourishBuilder ConfigureFont(Action<IFontBuilder> configureFont);

    /// <summary>Configures shell content layout and scrolling.</summary>
    IFlourishBuilder ConfigureLayout(Action<ILayoutBuilder> configureLayout);

    /// <summary>Configures Flourish-owned tooltip presentation.</summary>
    IFlourishBuilder ConfigureToolTips(Action<IToolTipBuilder> configureToolTips);

    /// <summary>Configures optional project-aware shell behavior.</summary>
    IFlourishBuilder ConfigureProjects(Action<IProjectBuilder> configureProjects);

    /// <summary>
    /// Configures the title bar.
    /// </summary>
    /// <param name="configureTitleBar">A callback that receives the title bar builder.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// builder.ConfigureTitleBar(titleBar =>
    /// {
    ///     titleBar.SetApplicationTitle("Foobar");
    /// });
    /// ]]></code>
    /// </example>
    IFlourishBuilder ConfigureTitleBar(Action<ITitleBarBuilder> configureTitleBar);

    /// <summary>
    /// Configures the visible navigation model.
    /// </summary>
    /// <param name="configureNavigation">A callback that receives the navigation builder.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// builder.ConfigureNavigation(navigation =>
    /// {
    ///     navigation.AddGroup("Navigation", groupId: 0, group =>
    ///     {
    ///         group.AddNavigableViewItem<HomePage>(isInitial: true);
    ///     });
    /// });
    /// ]]></code>
    /// </example>
    IFlourishBuilder ConfigureNavigation(Action<INavigationBuilder> configureNavigation);

    /// <summary>
    /// Configures custom WPF elements displayed in predefined Flourish regions.
    /// </summary>
    /// <param name="configureCustomHandler">A callback that receives the custom handler builder.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// builder.ConfigureContent(custom =>
    /// {
    ///     custom.AddRegionContent(
    ///         FlourishRegion.TitleBarEnd,
    ///         services => new Button { Content = "Account" });
    /// });
    /// ]]></code>
    /// </example>
    IFlourishBuilder ConfigureContent(
        Action<ICustomContentBuilder> configureCustomHandler
    );

    /// <summary>
    /// Configures page-specific dynamic toolbar items.
    /// </summary>
    /// <param name="configureToolbar">A callback that receives the dynamic toolbar builder.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// builder.ConfigureToolbar(toolbar =>
    /// {
    ///     toolbar.Set<ReportsPage>(
    ///         new FlourishToolbarItem("Export", "\uE898", "cmd_reports_export"));
    /// });
    /// ]]></code>
    /// </example>
    IFlourishBuilder ConfigureToolbar(
        Action<IToolbarBuilder> configureToolbar
    );

    /// <summary>
    /// Configures motion behavior.
    /// </summary>
    /// <param name="configureMotion">A callback that receives the motion builder.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishBuilder ConfigureMotion(Action<IMotionBuilder> configureMotion);

    /// <summary>
    /// Configures shell window properties.
    /// </summary>
    /// <param name="configureWindow">A callback that receives the window property builder.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishBuilder ConfigureWindow(Action<IWindowBuilder> configureWindow);

    /// <summary>
    /// Configures the shell status bar.
    /// </summary>
    /// <param name="configureStatusBar">A callback that receives the status bar builder.</param>
    /// <returns>The current builder for chained configuration.</returns>
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
    IFlourishBuilder ConfigureStatusBar(
        Action<IStatusBarBuilder> configureStatusBar
    );

    /// <summary>
    /// Builds the Flourish runtime.
    /// </summary>
    /// <returns>An <see cref="IFlourish" /> runtime that can be started and disposed.</returns>
    /// <remarks>
    /// Building consumes this builder. A second build, later <c>Configure...</c> call, or mutation
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

    /// <summary>
    /// Builds a Flourish runtime, runs the registered WPF application, and disposes the runtime on exit.
    /// </summary>
    /// <typeparam name="TApplication">The WPF application type registered in the service provider.</typeparam>
    /// <returns>The application exit code.</returns>
    int Run<TApplication>()
        where TApplication : Application
    {
        using var flourish = Build();
        return flourish.Run<TApplication>();
    }
}
