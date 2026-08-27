using System;
using System.Threading;
using System.Threading.Tasks;

using Application = System.Windows.Application;
using StartupEventArgs = System.Windows.StartupEventArgs;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Represents a built Flourish application runtime.
/// </summary>
/// <example>
/// <code><![CDATA[
/// using var flourish = ApplicationBuilder.CreateDefaultBuilder(args).Build();
/// return flourish.Run<App>();
/// ]]></code>
/// </example>
public interface IApplicationRuntime : IDisposable
{
    /// <summary>
    /// Gets the application service provider created by the underlying .NET host.
    /// </summary>
    /// <example>
    /// <code><![CDATA[
    /// var app = (App)flourish.Services.GetRequiredService(typeof(App));
    /// ]]></code>
    /// </example>
    IServiceProvider Services { get; }

    /// <summary>
    /// Gets a required service from the Flourish service provider.
    /// </summary>
    /// <typeparam name="T">The service type to resolve.</typeparam>
    /// <returns>The resolved service instance.</returns>
    /// <example>
    /// <code><![CDATA[
    /// var app = flourish.GetRequiredService<App>();
    /// ]]></code>
    /// </example>
    T GetRequiredService<T>()
        where T : notnull;

    /// <summary>
    /// Starts the underlying application host.
    /// </summary>
    /// <example>
    /// <code><![CDATA[
    /// flourish.Start();
    /// ]]></code>
    /// </example>
    void Start();

    /// <summary>
    /// Stops the underlying application host asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the stop operation.</param>
    /// <returns>A task that completes when the host has stopped.</returns>
    /// <example>
    /// <code><![CDATA[
    /// await flourish.StopAsync(cancellationToken);
    /// ]]></code>
    /// </example>
    Task StopAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Shows the Flourish shell for the specified WPF application.
    /// </summary>
    /// <param name="application">The WPF application that owns the Flourish shell.</param>
    /// <example>
    /// <code><![CDATA[
    /// var app = flourish.GetRequiredService<App>();
    /// flourish.Show(app);
    /// ]]></code>
    /// </example>
    void Show(Application application);

    /// <summary>
    /// Starts Flourish, shows the shell for the registered WPF application, and runs its dispatcher.
    /// </summary>
    /// <typeparam name="TApplication">The WPF application type registered in the service provider.</typeparam>
    /// <returns>The application exit code.</returns>
    int Run<TApplication>()
        where TApplication : Application => Run(GetRequiredService<TApplication>());

    /// <summary>
    /// Starts Flourish, shows the shell for the specified WPF application, and runs its dispatcher.
    /// </summary>
    /// <param name="application">The WPF application that owns the Flourish shell.</param>
    /// <returns>The application exit code.</returns>
    int Run(Application application)
    {
        ArgumentNullException.ThrowIfNull(application);

        Start();

        try
        {
            application.Startup += ShowShell;
            return application.Run();
        }
        finally
        {
            application.Startup -= ShowShell;
            StopAsync().GetAwaiter().GetResult();
        }

        void ShowShell(object? sender, StartupEventArgs e)
        {
            application.Startup -= ShowShell;
            Show(application);
        }
    }
}
