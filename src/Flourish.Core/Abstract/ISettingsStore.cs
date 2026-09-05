using System;
using System.Threading;
using System.Threading.Tasks;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Provides transactional, atomic updates to the owned section of the JSON file selected by
/// <see cref="IDataBuilder.SetAppSettingsFilePath(string)" />.
/// </summary>
/// <remarks>
/// Every settings path must start with <c>Flourish:</c> and identify a child value.
/// Root matching is case-insensitive; a newly created root uses the canonical <c>Flourish</c>
/// spelling.
/// Other top-level sections are outside the store's ownership and are preserved unchanged.
/// </remarks>
public interface ISettingsStore
{
    /// <summary>
    /// Gets the absolute path of the managed JSON settings file.
    /// </summary>
    string FilePath { get; }

    /// <summary>
    /// Applies multiple edits in one atomic write and reloads host configuration.
    /// </summary>
    /// <remarks>
    /// The update callback may run on a non-UI thread and must not access dispatcher-affine
    /// UI objects.
    /// </remarks>
    ValueTask<SettingsUpdateResult> UpdateAsync(
        Action<ISettingsEditor> update,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Replaces one value under <c>Flourish</c> and reloads host configuration.
    /// </summary>
    ValueTask<SettingsUpdateResult> SetAsync<T>(
        string path,
        T value,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Removes one value under <c>Flourish</c> and reloads host configuration when it existed.
    /// </summary>
    ValueTask<SettingsUpdateResult> RemoveAsync(
        string path,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Recursively merges an object under <c>Flourish</c> and reloads host configuration.
    /// </summary>
    ValueTask<SettingsUpdateResult> MergeAsync<T>(
        string path,
        T value,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Appends one array value under <c>Flourish</c> and reloads host configuration.
    /// </summary>
    ValueTask<SettingsUpdateResult> AppendAsync<T>(
        string path,
        T value,
        CancellationToken cancellationToken = default
    );
}
