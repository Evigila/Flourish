using ArkheideSystem.Flourish.Hosting;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Provides factory methods for creating Flourish application builders.
/// </summary>
/// <example>
/// <code><![CDATA[
/// var builder = ApplicationBuilder.CreateDefaultBuilder(args);
/// ]]></code>
/// </example>
public static class ApplicationBuilder
{
    /// <summary>
    /// Creates a default Flourish builder configured with the standard .NET host defaults.
    /// </summary>
    /// <param name="args">The command-line arguments passed to the application.</param>
    /// <returns>An <see cref="IApplicationBuilder" /> that configures and builds the Flourish runtime.</returns>
    /// <example>
    /// <code><![CDATA[
    /// var flourish = ApplicationBuilder
    ///     .CreateDefaultBuilder(args)
    ///     .ConfigureServices((_, services) => { })
    ///     .Build();
    /// ]]></code>
    /// </example>
    public static IApplicationBuilder CreateDefaultBuilder(string[] args)
    {
        return new DefaultApplicationBuilder(args);
    }
}
