using System;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>Configures framework-owned tooltip presentation.</summary>
public interface IToolTipBuilder
{
    /// <summary>Enables or disables tooltip presentation.</summary>
    IToolTipBuilder SetEnabled(bool enabled = true);

    /// <summary>Sets tooltip timing and placement margin.</summary>
    IToolTipBuilder SetSettings(
        int initialShowDelayMilliseconds = 200,
        double spawnableMargin = 5
    );
}

/// <summary>Represents runtime tooltip settings.</summary>
public sealed record ToolTipSettings(
    bool IsEnabled,
    int InitialShowDelayMilliseconds,
    double SpawnableMargin
);

/// <summary>Controls tooltip behavior at runtime.</summary>
public interface IToolTipService
{
    /// <summary>Gets the current tooltip settings.</summary>
    ToolTipSettings Current { get; }

    /// <summary>Occurs after tooltip settings change.</summary>
    event EventHandler<StateTransitionEventArgs<ToolTipSettings>>? Changed;

    /// <summary>Enables or disables framework tooltip presentation.</summary>
    void SetEnabled(bool enabled);

    /// <summary>Changes delay and placement margin and enables presentation.</summary>
    void SetSettings(int initialShowDelayMilliseconds, double spawnableMargin = 5);
}
