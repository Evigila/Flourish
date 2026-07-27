namespace ArkheideSystem.Flourish.Abstract.Runtime;

/// <summary>
/// Controls the system backdrop and immersive dark mode of the Flourish shell.
/// </summary>
public interface IMaterialEffectService
{
    /// <summary>
    /// Gets the requested material effect.
    /// </summary>
    MaterialEffect CurrentEffect { get; }

    /// <summary>
    /// Gets whether the requested material effect is currently applied.
    /// </summary>
    bool IsApplied { get; }

    /// <summary>
    /// Gets whether immersive dark mode is requested for the shell window.
    /// </summary>
    bool IsDarkMode { get; }

    /// <summary>
    /// Raised after material or immersive dark mode state changes.
    /// </summary>
    /// <remarks>
    /// When a shell window is attached, the event is raised on its dispatcher.
    /// </remarks>
    event EventHandler<FlourishMaterialEffectChangedEventArgs>? Changed;

    /// <summary>
    /// Gets whether an effect is supported on the current operating system.
    /// </summary>
    bool IsSupported(MaterialEffect effect);

    /// <summary>
    /// Applies or removes a material effect at runtime.
    /// </summary>
    void SetEffect(MaterialEffect effect);

    /// <summary>
    /// Changes the immersive dark mode of the attached shell window.
    /// </summary>
    void SetDarkMode(bool isDarkMode);
}
