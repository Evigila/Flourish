namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Provides read access to the effective host configuration and reports reloads.
/// </summary>
public interface IFlourishConfiguration
{
    /// <summary>
    /// Gets an immutable snapshot of all effective configuration values.
    /// </summary>
    FlourishConfigurationSnapshot Current { get; }

    /// <summary>
    /// Gets an effective configuration value by its colon-delimited path.
    /// </summary>
    string? this[string key] { get; }

    /// <summary>
    /// Raised after the effective configuration has been reloaded.
    /// </summary>
    /// <remarks>
    /// The event may be raised on a non-UI thread.
    /// </remarks>
    event EventHandler<FlourishConfigurationChangedEventArgs>? Changed;

    /// <summary>
    /// Gets and converts an effective configuration value.
    /// </summary>
    T? Get<T>(string key);

    /// <summary>
    /// Binds a configuration section to an object.
    /// </summary>
    T? GetSection<T>(string path);

    /// <summary>
    /// Reloads all reloadable configuration providers.
    /// </summary>
    void Reload();
}
