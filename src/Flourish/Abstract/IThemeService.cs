using System;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Controls the active Flourish theme at runtime.
/// </summary>
public interface IThemeService
{
    /// <summary>
    /// Gets an immutable snapshot of the requested and resolved theme.
    /// </summary>
    FlourishThemeState Current { get; }

    /// <summary>
    /// Raised after either the requested or effective theme changes.
    /// </summary>
    /// <remarks>
    /// After WPF is initialized, the event is raised on the application dispatcher.
    /// </remarks>
    event EventHandler<FlourishStateChangedEventArgs<FlourishThemeState>>? Changed;

    /// <summary>
    /// Selects the next System, Light, or Dark theme mode.
    /// </summary>
    void ToggleTheme();

    /// <summary>
    /// Selects a theme mode immediately and schedules its persistence.
    /// </summary>
    void SetTheme(FlourishTheme theme);
}

/// <summary>Represents the requested and resolved runtime theme.</summary>
public sealed record FlourishThemeState(
    FlourishTheme RequestedTheme,
    FlourishTheme EffectiveTheme
)
{
    /// <summary>Gets whether the resolved theme is dark.</summary>
    public bool IsDark => EffectiveTheme == FlourishTheme.Dark;
}
