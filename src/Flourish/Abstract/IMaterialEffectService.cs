using System;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Controls the system backdrop and immersive dark mode of the Flourish shell.
/// </summary>
public interface IMaterialEffectService
{
    /// <summary>
    /// Gets an immutable snapshot of the requested and effective material state.
    /// </summary>
    FlourishMaterialEffectState Current { get; }

    /// <summary>
    /// Raised after material or immersive dark mode state changes.
    /// </summary>
    /// <remarks>
    /// When a shell window is attached, the event is raised on its dispatcher.
    /// </remarks>
    event EventHandler<FlourishStateChangedEventArgs<FlourishMaterialEffectState>>? Changed;

    /// <summary>
    /// Gets whether an effect is supported on the current operating system.
    /// </summary>
    bool IsSupported(MaterialEffect effect);

    /// <summary>
    /// Applies or removes a material effect at runtime.
    /// </summary>
    /// <exception cref="PlatformNotSupportedException">
    /// The requested concrete effect is unavailable on the current operating system.
    /// </exception>
    void SetEffect(MaterialEffect effect);

    /// <summary>
    /// Changes the immersive dark mode of the attached shell window.
    /// </summary>
    void SetDarkMode(bool isDarkMode);
}

/// <summary>Represents the active runtime material and immersive dark-mode state.</summary>
public sealed record FlourishMaterialEffectState(
    MaterialEffect RequestedEffect,
    MaterialEffect EffectiveEffect,
    bool IsSupported,
    bool IsApplied,
    bool IsDarkMode
);
