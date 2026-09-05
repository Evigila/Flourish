namespace ArkheideSystem.Flourish.Abstract;

/// <summary>Represents the current navigation panel state.</summary>
public sealed record NavigationPanelState(
    bool IsEnabled,
    bool IsOpen,
    NavigationPanelDirection Direction,
    double OpenWidth,
    double ClosedWidth,
    double MinWidth,
    double MaxWidth,
    long Version
);
