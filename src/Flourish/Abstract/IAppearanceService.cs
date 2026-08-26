using System;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>Controls shared Flourish appearance overrides at runtime.</summary>
public interface IAppearanceService
{
    /// <summary>Gets an immutable snapshot of the active appearance overrides.</summary>
    FlourishAppearanceSettings Current { get; }

    /// <summary>Occurs after an appearance override changes.</summary>
    event EventHandler<FlourishStateTransitionEventArgs<FlourishAppearanceSettings>>? Changed;

    /// <summary>Sets the application palette override, or restores the standard palette.</summary>
    void SetThemeColors(FlourishThemeColors? colors);

    /// <summary>Sets the shared corner radius, or restores the standard radius hierarchy.</summary>
    void SetCornerRadius(double? radius);

    /// <summary>Changes the palette and corner-radius overrides atomically.</summary>
    void SetAppearance(FlourishThemeColors? colors, double? cornerRadius);
}

/// <summary>Represents the active appearance overrides.</summary>
public sealed record FlourishAppearanceSettings(
    FlourishThemeColors? ThemeColors,
    double? CornerRadius,
    long Version
);
