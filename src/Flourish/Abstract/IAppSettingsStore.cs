namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Provides transactional, atomic updates to the host appsettings.json file.
/// </summary>
public interface IAppSettingsStore
{
    /// <summary>
    /// Gets the absolute path of the managed appsettings.json file.
    /// </summary>
    string FilePath { get; }

    /// <summary>
    /// Applies multiple edits in one atomic write and reloads host configuration.
    /// </summary>
    /// <remarks>
    /// The editor runs on Flourish's serialized settings worker. It must not access
    /// dispatcher-affine UI objects.
    /// </remarks>
    ValueTask<AppSettingsUpdateResult> UpdateAsync(
        Action<IAppSettingsEditor> update,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Replaces one value and reloads host configuration.
    /// </summary>
    ValueTask<AppSettingsUpdateResult> SetAsync<T>(
        string path,
        T value,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Removes one value and reloads host configuration when it existed.
    /// </summary>
    ValueTask<AppSettingsUpdateResult> RemoveAsync(
        string path,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Recursively merges an object and reloads host configuration.
    /// </summary>
    ValueTask<AppSettingsUpdateResult> MergeAsync<T>(
        string path,
        T value,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Appends one array value and reloads host configuration.
    /// </summary>
    ValueTask<AppSettingsUpdateResult> AppendAsync<T>(
        string path,
        T value,
        CancellationToken cancellationToken = default
    );
}
