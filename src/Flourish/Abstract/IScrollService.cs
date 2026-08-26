using System;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Controls application-wide Flourish scrolling behavior at runtime.
/// </summary>
public interface IScrollService
{
    /// <summary>
    /// Gets an immutable snapshot of the active scrolling settings.
    /// </summary>
    FlourishScrollSettings Current { get; }

    /// <summary>
    /// Occurs synchronously after the application-wide scrolling settings change.
    /// </summary>
    event EventHandler<FlourishStateTransitionEventArgs<FlourishScrollSettings>>? Changed;

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
