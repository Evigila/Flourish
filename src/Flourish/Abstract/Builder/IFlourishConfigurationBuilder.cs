using Microsoft.Extensions.Configuration;

namespace ArkheideSystem.Flourish.Abstract.Builder;

/// <summary>
/// Registers application-owned configuration sources in the Flourish Host pipeline.
/// </summary>
/// <remarks>
/// Registered sources are applied after the Host appsettings and User Secrets sources, and
/// before environment variables and command-line arguments. Later registrations override
/// earlier registrations without changing the priority of environment or command-line values.
/// </remarks>
public interface IFlourishConfigurationBuilder
{
    /// <summary>
    /// Registers a JSON configuration file.
    /// </summary>
    /// <param name="path">
    /// The file path. Relative paths are resolved against <see cref="AppContext.BaseDirectory" />.
    /// </param>
    /// <param name="optional">Whether a missing file is allowed.</param>
    /// <param name="reloadOnChange">Whether changes reload the effective configuration.</param>
    /// <returns>The current configuration builder for chained registration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// configuration.UseConfigurationFile(
    ///     "appsettings.User.json",
    ///     optional: true,
    ///     reloadOnChange: true);
    /// ]]></code>
    /// </example>
    IFlourishConfigurationBuilder UseConfigurationFile(
        string path,
        bool optional = true,
        bool reloadOnChange = true
    );

    /// <summary>
    /// Registers a standard Microsoft configuration source.
    /// </summary>
    /// <param name="source">The configuration source to register.</param>
    /// <returns>The current configuration builder for chained registration.</returns>
    /// <remarks>
    /// Flourish controls where the source is inserted. The underlying
    /// <see cref="IConfigurationBuilder" /> is not exposed.
    /// </remarks>
    IFlourishConfigurationBuilder AddConfigurationSource(IConfigurationSource source);
}
