namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Controls Flourish tooltip behavior at runtime.
/// </summary>
public interface IToolTipService
{
    /// <summary>
    /// Gets the current tooltip settings.
    /// </summary>
    FlourishToolTipSettings Current { get; }

    /// <summary>
    /// Raised after the tooltip settings change.
    /// </summary>
    /// <remarks>
    /// When application resources are attached, the event is raised on their dispatcher.
    /// </remarks>
    event EventHandler<FlourishToolTipChangedEventArgs>? Changed;

    /// <summary>
    /// Enables or disables Flourish tooltips.
    /// </summary>
    void SetEnabled(bool enabled);

    /// <summary>
    /// Changes the tooltip delay and placement margin and enables tooltips.
    /// </summary>
    void Configure(int initialShowDelayMilliseconds, double spawnableMargin = 5);
}
