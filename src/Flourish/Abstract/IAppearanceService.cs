using System;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>Controls shared Flourish appearance overrides at runtime.</summary>
public interface IAppearanceService
{
    /// <summary>Gets an immutable snapshot of the active appearance overrides.</summary>
    AppearanceSettings Current { get; }

    /// <summary>Occurs after an appearance override changes.</summary>
    event EventHandler<StateTransitionEventArgs<AppearanceSettings>>? Changed;

    /// <summary>Sets the application palette override, or restores the standard palette.</summary>
    void SetThemeColors(ThemeColors? colors);

    /// <summary>Sets the shared corner radius, or restores the standard radius hierarchy.</summary>
    void SetCornerRadius(double? radius);

    /// <summary>Changes the palette and corner-radius overrides atomically.</summary>
    void SetAppearance(ThemeColors? colors, double? cornerRadius);
}

/// <summary>Represents the active appearance overrides.</summary>
public sealed record AppearanceSettings(
    ThemeColors? ThemeColors,
    double? CornerRadius,
    long Version
);
