namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Represents runtime tooltip settings.
/// </summary>
public sealed record ToolTipSettings(
    bool IsEnabled,
    int InitialShowDelayMilliseconds,
    double SpawnableMargin
);
