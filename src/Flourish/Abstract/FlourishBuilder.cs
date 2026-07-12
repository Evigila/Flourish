using ArkheideSystem.Flourish.Internal.Composition;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Provides factory methods for creating Flourish application builders.
/// </summary>
/// <example>
/// <code><![CDATA[
/// var builder = FlourishBuilder.CreateDefaultBuilder(args);
/// ]]></code>
/// </example>
public static class FlourishBuilder
{
    /// <summary>
    /// Creates a default Flourish builder configured with the standard .NET host defaults.
    /// </summary>
    /// <param name="args">The command-line arguments passed to the application.</param>
    /// <returns>An <see cref="IFlourishBuilder" /> that configures and builds the Flourish runtime.</returns>
    /// <example>
    /// <code><![CDATA[
    /// var flourish = FlourishBuilder
    ///     .CreateDefaultBuilder(args)
    ///     .ConfigureServices((_, services) => { })
    ///     .Build();
    /// ]]></code>
    /// </example>
    public static IFlourishBuilder CreateDefaultBuilder(string[] args)
    {
        return new DefaultFlourishBuilder(args);
    }
}
