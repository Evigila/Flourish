using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AcksheedSys.Flourish.Abstract;

/// <summary>
/// Configures services, shell options, toolbar items, and status items before building a Flourish runtime.
/// </summary>
/// <example>
/// <code><![CDATA[
/// var flourish = FlourishBuilder
///     .CreateDefaultBuilder(args)
///     .ConfigureServices((_, services) => services.AddSingleton<App>())
///     .Build();
/// ]]></code>
/// </example>
public interface IFlourishBuilder
{
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
    ///     services.AddNavigable<HomePage>("Home", "\uE80F", isInitial: true);
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
    /// flourish.Start();
    /// ]]></code>
    /// </example>
    IFlourish Build();
}
