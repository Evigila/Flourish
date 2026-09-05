using System;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Controls the active application theme at runtime.
/// </summary>
public interface IThemeService
{
    /// <summary>
    /// Gets an immutable snapshot of the requested and resolved theme.
    /// </summary>
    ThemeState Current { get; }

    /// <summary>
    /// Raised after either the requested or effective theme changes.
    /// </summary>
    event EventHandler<StateChangedEventArgs<ThemeState>>? Changed;

    /// <summary>
    /// Selects the next System, Light, or Dark theme mode.
    /// </summary>
    void ToggleTheme();

    /// <summary>
    /// Selects a theme mode immediately and schedules its persistence.
    /// </summary>
    void SetTheme(ApplicationTheme theme);
}

/// <summary>Represents the requested and resolved runtime theme.</summary>
public sealed record ThemeState(
    ApplicationTheme RequestedTheme,
    ApplicationTheme EffectiveTheme
)
{
    /// <summary>Gets whether the resolved theme is dark.</summary>
    public bool IsDark => EffectiveTheme == ApplicationTheme.Dark;
}
