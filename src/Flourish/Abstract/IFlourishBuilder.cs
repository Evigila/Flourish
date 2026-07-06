using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AcksheedSys.Flourish.Abstract;

/// <summary>
/// Configures services, shell options, toolbar items, and status items before building a Flourish runtime.
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
    /// <param name="configureData">A callback that receives the host context and data builder.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// builder.ConfigureData((_, data) =>
    /// {
    ///     data.SetAppCompany("Acksheed").SetAppName("Gallery");
    /// });
    /// ]]></code>
    /// </example>
    IFlourishBuilder ConfigureData(Action<HostBuilderContext, IFlourishDataBuilder> configureData);

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
    /// Configures the Flourish shell.
    /// </summary>
    /// <param name="configureShell">A callback that receives the host context and shell builder.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// builder.ConfigureShell((_, shell) =>
    /// {
    ///     shell.UseTitlebar((_, titlebar) => titlebar.SetTitle("Gallery"));
    /// });
    /// ]]></code>
    /// </example>
    IFlourishBuilder ConfigureShell(Action<HostBuilderContext, IFlourishShellBuilder> configureShell);

    /// <summary>
    /// Configures page-specific dynamic toolbar items.
    /// </summary>
    /// <param name="configureToolbar">A callback that receives the host context and dynamic toolbar builder.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// builder.ConfigureDynamicToolbar((_, toolbar) =>
    /// {
    ///     toolbar.CreateToolbarItems<HomePage>(new FlourishToolbarItem("Open", "\uE8E5", "home.open"));
    /// });
    /// ]]></code>
    /// </example>
    IFlourishBuilder ConfigureDynamicToolbar(Action<HostBuilderContext, IFlourishDynamicToolbarBuilder> configureToolbar);

    /// <summary>
    /// Configures the shell status area.
    /// </summary>
    /// <param name="configureStatus">A callback that receives the host context and status builder.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// builder.ConfigureStatus((_, status) =>
    /// {
    ///     status.SetStatusText("Ready").ShowPowerStatus();
    /// });
    /// ]]></code>
    /// </example>
    IFlourishBuilder ConfigureStatus(Action<HostBuilderContext, IFlourishStatusBuilder> configureStatus);

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
