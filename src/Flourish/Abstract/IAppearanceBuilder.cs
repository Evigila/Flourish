namespace ArkheideSystem.Flourish.Abstract;

/// <summary>Configures application-wide appearance defaults before startup.</summary>
public interface IAppearanceBuilder
{
    /// <summary>Enables or disables the window material effect.</summary>
    IAppearanceBuilder SetEffect(
        bool enabled = true,
        MaterialEffect effect = MaterialEffect.Auto,
        bool usePersistedPreference = true
    );

    /// <summary>Sets or clears the shared theme color override.</summary>
    IAppearanceBuilder SetThemeColors(
        bool enabled,
        FlourishThemeColors colors,
        bool usePersistedPreference = true
    );

    /// <summary>Sets or clears the shared corner radius override.</summary>
    IAppearanceBuilder SetCornerRadius(
        bool enabled,
        double radius = 6,
        bool usePersistedPreference = true
    );
}
