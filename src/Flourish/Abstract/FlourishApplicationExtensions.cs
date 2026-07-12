using ArkheideSystem.Flourish.Internal.Composition;
using Application = System.Windows.Application;
using StartupEventArgs = System.Windows.StartupEventArgs;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Provides convenience methods for building and running Flourish applications.
/// </summary>
/// <example>
/// <code><![CDATA[
/// return FlourishBuilder
///     .CreateDefaultBuilder(args)
///     .ConfigureServices((_, services) => services.AddSingleton<App>())
///     .Run<App>();
/// ]]></code>
/// </example>
public static class FlourishApplicationExtensions
{
    /// <summary>
    /// Starts Flourish, shows the shell for the registered WPF application, and runs the application dispatcher.
    /// </summary>
    /// <typeparam name="TApplication">The WPF application type registered in the service provider.</typeparam>
    /// <param name="flourish">The built Flourish runtime.</param>
    /// <returns>The application exit code returned by <see cref="Application.Run()" />.</returns>
    /// <example>
    /// <code><![CDATA[
    /// using var flourish = FlourishBuilder.CreateDefaultBuilder(args).Build();
    /// return flourish.Run<App>();
    /// ]]></code>
    /// </example>
    public static int Run<TApplication>(this IFlourish flourish)
        where TApplication : Application
    {
        ArgumentNullException.ThrowIfNull(flourish);

        return flourish.Run(flourish.GetRequiredService<TApplication>());
    }

    /// <summary>
    /// Starts Flourish, shows the shell for the specified WPF application, and runs the application dispatcher.
    /// </summary>
    /// <param name="flourish">The built Flourish runtime.</param>
    /// <param name="application">The WPF application that owns the Flourish shell.</param>
    /// <returns>The application exit code returned by <see cref="Application.Run()" />.</returns>
    /// <example>
    /// <code><![CDATA[
    /// var app = flourish.GetRequiredService<App>();
    /// return flourish.Run(app);
    /// ]]></code>
    /// </example>
    public static int Run(this IFlourish flourish, Application application)
    {
        ArgumentNullException.ThrowIfNull(flourish);
        ArgumentNullException.ThrowIfNull(application);

        if (flourish is FlourishRuntime runtime)
        {
            return runtime.Run(application);
        }

        return RunWithStartupEvent(flourish, application);
    }

    /// <summary>
    /// Builds a Flourish runtime, starts it, shows the shell for the registered WPF application,
    /// and disposes the runtime when the application exits.
    /// </summary>
    /// <typeparam name="TApplication">The WPF application type registered in the service provider.</typeparam>
    /// <param name="builder">The configured Flourish builder.</param>
    /// <returns>The application exit code returned by <see cref="Application.Run()" />.</returns>
    /// <example>
    /// <code><![CDATA[
    /// return FlourishBuilder
    ///     .CreateDefaultBuilder(args)
    ///     .ConfigureServices((_, services) => services.AddSingleton<App>())
    ///     .Run<App>();
    /// ]]></code>
    /// </example>
    public static int Run<TApplication>(this IFlourishBuilder builder)
        where TApplication : Application
    {
        ArgumentNullException.ThrowIfNull(builder);

        using var flourish = builder.Build();
        return flourish.Run<TApplication>();
    }

    private static int RunWithStartupEvent(IFlourish flourish, Application application)
    {
        flourish.Start();

        try
        {
            application.Startup += ShowShell;
            return application.Run();
        }
        finally
        {
            application.Startup -= ShowShell;
            flourish.StopAsync().GetAwaiter().GetResult();
        }

        void ShowShell(object? sender, StartupEventArgs e)
        {
            application.Startup -= ShowShell;
            flourish.Show(application);
        }
    }
}
