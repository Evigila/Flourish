namespace ArkheideSystem.Flourish.Abstract.Runtime;

/// <summary>
/// Controls application-wide Flourish scrolling behavior at runtime.
/// </summary>
public interface IScrollService
{
    /// <summary>
    /// Gets an immutable snapshot of the active scrolling settings.
    /// </summary>
    FlourishScrollSettings GetCurrent();

    /// <summary>
    /// Occurs synchronously after the application-wide scrolling settings change.
    /// </summary>
    event EventHandler<FlourishScrollChangedEventArgs>? Changed;

    /// <summary>
    /// Enables or disables smooth mouse-wheel scrolling for Flourish scroll viewers.
    /// </summary>
    /// <param name="enabled">
    /// <see langword="true"/> to enable smooth scrolling; otherwise,
    /// <see langword="false"/> to use native scrolling.
    /// </param>
    /// <remarks>
    /// A locally assigned
    /// <see cref="ArkheideSystem.Flourish.Controls.ScrollViewer.IsSmoothScrollingEnabled"/>
    /// value takes precedence over this application-wide setting.
    /// </remarks>
    void SetSmoothScrollingEnabled(bool enabled);
}

/// <summary>
/// Describes the current application-wide Flourish scrolling settings.
/// </summary>
public sealed class FlourishScrollSettings
{
    internal FlourishScrollSettings(bool isSmoothScrollingEnabled, long version)
    {
        IsSmoothScrollingEnabled = isSmoothScrollingEnabled;
        Version = version;
    }

    /// <summary>
    /// Gets a value indicating whether smooth mouse-wheel scrolling is enabled.
    /// </summary>
    public bool IsSmoothScrollingEnabled { get; }

    /// <summary>
    /// Gets the monotonic settings version.
    /// </summary>
    public long Version { get; }
}

/// <summary>
/// Provides the previous and current application-wide scrolling settings.
/// </summary>
public sealed class FlourishScrollChangedEventArgs(
    FlourishScrollSettings previous,
    FlourishScrollSettings current
) : EventArgs
{
    /// <summary>
    /// Gets the settings before the change.
    /// </summary>
    public FlourishScrollSettings Previous { get; } = previous;

    /// <summary>
    /// Gets the settings after the change.
    /// </summary>
    public FlourishScrollSettings Current { get; } = current;
}
