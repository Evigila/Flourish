namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Enables or disables Flourish shell features at runtime.
/// </summary>
public interface IShellFeatureService
{
    /// <summary>
    /// Gets an immutable snapshot of all shell feature switches.
    /// </summary>
    FlourishShellFeatureState Current { get; }

    /// <summary>
    /// Occurs synchronously after a feature switch changes.
    /// </summary>
    event EventHandler<FlourishShellFeatureChangedEventArgs>? Changed;

    /// <summary>
    /// Enables or disables a shell feature.
    /// </summary>
    /// <param name="feature">The feature to change.</param>
    /// <param name="enabled"><see langword="true"/> to enable the feature; otherwise, <see langword="false"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="feature"/> is not defined.</exception>
    void SetEnabled(ShellFeature feature, bool enabled);
}
