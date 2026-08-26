using System;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>Controls the shared page-content layout at runtime.</summary>
public interface IContentLayoutService
{
    /// <summary>Gets an immutable snapshot of the current content layout.</summary>
    FlourishContentLayoutSettings Current { get; }

    /// <summary>Occurs after the content layout changes.</summary>
    event EventHandler<FlourishStateTransitionEventArgs<FlourishContentLayoutSettings>>? Changed;

    /// <summary>Enables or disables centered content and sets its maximum width.</summary>
    void SetCenterContent(bool enabled, double contentWidth = 1200);
}

/// <summary>Represents the shared page-content layout.</summary>
public sealed record FlourishContentLayoutSettings(
    bool IsCenterContentEnabled,
    double ContentWidth,
    long Version
);
