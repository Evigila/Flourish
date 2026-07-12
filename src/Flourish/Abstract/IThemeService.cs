namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Controls the active Flourish theme at runtime.
/// </summary>
public interface IThemeService
{
    /// <summary>
    /// Gets the requested theme mode.
    /// </summary>
    FlourishTheme CurrentTheme { get; }

    /// <summary>
    /// Gets the resolved light or dark theme.
    /// </summary>
    FlourishTheme EffectiveTheme { get; }

    /// <summary>
    /// Gets a value indicating whether the effective theme is dark.
    /// </summary>
    bool IsDark { get; }

    /// <summary>
    /// Raised after either the requested or effective theme changes.
    /// </summary>
    /// <remarks>
    /// After WPF is initialized, the event is raised on the application dispatcher.
    /// </remarks>
    event EventHandler<FlourishThemeChangedEventArgs>? ThemeChanged;

    /// <summary>
    /// Selects the next System, Light, or Dark theme mode.
    /// </summary>
    void ToggleTheme();

    /// <summary>
    /// Selects a theme mode immediately and schedules its persistence.
    /// </summary>
    void SetTheme(FlourishTheme theme);
}
