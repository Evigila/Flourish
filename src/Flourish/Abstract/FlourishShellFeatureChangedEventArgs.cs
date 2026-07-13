namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Provides a shell feature snapshot after it changes.
/// </summary>
/// <param name="feature">The feature that was requested to change.</param>
/// <param name="state">The complete state after the change.</param>
public sealed class FlourishShellFeatureChangedEventArgs(
    ShellFeature feature,
    FlourishShellFeatureState state
) : EventArgs
{
    /// <summary>
    /// Gets the feature that was requested to change.
    /// </summary>
    public ShellFeature Feature { get; } = feature;

    /// <summary>
    /// Gets the complete shell feature state after the change.
    /// </summary>
    public FlourishShellFeatureState State { get; } = state;
}
