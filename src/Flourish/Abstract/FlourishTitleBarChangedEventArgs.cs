namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Provides the latest title bar snapshot after a runtime change.
/// </summary>
/// <param name="state">The state after the change.</param>
/// <param name="version">The state version represented by the snapshot.</param>
public sealed class FlourishTitleBarChangedEventArgs(FlourishTitleBarState state, long version = 0)
    : EventArgs
{
    /// <summary>
    /// Gets the state after the change.
    /// </summary>
    public FlourishTitleBarState State { get; } = state;

    /// <summary>Gets the state version represented by this snapshot.</summary>
    public long Version { get; } = version;
}
