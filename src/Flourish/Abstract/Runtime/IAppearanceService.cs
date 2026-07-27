namespace ArkheideSystem.Flourish.Abstract.Runtime;

/// <summary>Controls shared Flourish appearance overrides at runtime.</summary>
public interface IAppearanceService
{
    /// <summary>Gets an immutable snapshot of the active appearance overrides.</summary>
    FlourishAppearanceSettings Current { get; }

    /// <summary>Occurs after an appearance override changes.</summary>
    event EventHandler<FlourishAppearanceChangedEventArgs>? Changed;

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

/// <summary>Provides data for <see cref="IAppearanceService.Changed" />.</summary>
public sealed class FlourishAppearanceChangedEventArgs(
    FlourishAppearanceSettings previous,
    FlourishAppearanceSettings current
) : EventArgs
{
    /// <summary>Gets the state before the change.</summary>
    public FlourishAppearanceSettings Previous { get; } = previous;

    /// <summary>Gets the state after the change.</summary>
    public FlourishAppearanceSettings Current { get; } = current;
}
