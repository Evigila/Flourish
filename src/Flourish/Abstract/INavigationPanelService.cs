namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Provides runtime control over the Flourish navigation panel.
/// </summary>
public interface INavigationPanelService
{
    /// <summary>Occurs after the navigation panel state changes.</summary>
    event EventHandler<FlourishNavigationPanelChangedEventArgs>? Changed;

    /// <summary>Gets an immutable snapshot of the current panel state.</summary>
    FlourishNavigationPanelState Current { get; }

    /// <summary>Enables or disables the navigation panel.</summary>
    void SetEnabled(bool enabled);

    /// <summary>Moves the navigation panel to the selected side of the window.</summary>
    void SetDirection(NavigationPanelDirection direction);

    /// <summary>Updates the open, closed, minimum, and maximum panel widths.</summary>
    /// <param name="openWidth">The width of the open navigation panel.</param>
    /// <param name="closedWidth">
    /// The collapsed width. Use 0 to hide the collapsed panel completely; any visible
    /// collapsed panel must be at least 56 device-independent pixels wide.
    /// </param>
    /// <param name="maxWidth">The maximum open width available to the splitter.</param>
    /// <param name="minWidth">The minimum open width available to the splitter.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// A dimension is non-finite or outside its supported range; <paramref name="closedWidth" />
    /// is non-zero and less than 56; the closed width exceeds the open width; or the open
    /// width is outside the supplied minimum and maximum range.
    /// </exception>
    void SetPanelWidth(double openWidth, double closedWidth, double maxWidth, double minWidth);

    /// <summary>Opens the panel.</summary>
    void Open(bool animate = true);

    /// <summary>Closes the panel.</summary>
    void Close(bool animate = true);

    /// <summary>Toggles the panel between its open and closed states.</summary>
    void Toggle(bool animate = true);
}

/// <summary>Represents the current navigation panel state.</summary>
public sealed record FlourishNavigationPanelState(
    bool IsEnabled,
    bool IsOpen,
    NavigationPanelDirection Direction,
    double OpenWidth,
    double ClosedWidth,
    double MinWidth,
    double MaxWidth,
    long Version
);

/// <summary>Provides data for <see cref="INavigationPanelService.Changed" />.</summary>
public sealed class FlourishNavigationPanelChangedEventArgs(
    FlourishNavigationPanelState previous,
    FlourishNavigationPanelState current,
    bool animate
) : EventArgs
{
    /// <summary>Gets the state before the mutation.</summary>
    public FlourishNavigationPanelState Previous { get; } = previous;

    /// <summary>Gets the state after the mutation.</summary>
    public FlourishNavigationPanelState Current { get; } = current;

    /// <summary>Gets whether the shell should animate this transition.</summary>
    public bool Animate { get; } = animate;
}
