using System;

using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Configuration;

namespace ArkheideSystem.Flourish.Appearance;

internal sealed class AppearanceBuilder(AppearanceOptions options)
    : BuilderMutationGuard,
        IAppearanceBuilder
{
    public IAppearanceBuilder SetEffect(
        bool enabled = true,
        MaterialEffect effect = MaterialEffect.Auto,
        bool usePersistedPreference = true
    )
    {
        ThrowIfFrozen();
        ValueValidation.Enum(effect, nameof(effect));
        options.MaterialEffect = effect;
        options.IsMaterialEffectEnabled = enabled && effect != MaterialEffect.None;
        options.UsePersistedMaterialEffect = usePersistedPreference;
        return this;
    }

    public IAppearanceBuilder SetThemeColors(
        bool enabled,
        ThemeColors colors,
        bool usePersistedPreference = true
    )
    {
        ThrowIfFrozen();
        ArgumentNullException.ThrowIfNull(colors);
        options.ThemeColors = enabled ? colors : null;
        options.UsePersistedThemeColors = usePersistedPreference;
        return this;
    }

    public IAppearanceBuilder SetCornerRadius(
        bool enabled,
        double radius = 6,
        bool usePersistedPreference = true
    )
    {
        ThrowIfFrozen();
        ValueValidation.NonNegativeFinite(radius, nameof(radius));
        options.CornerRadius = enabled ? radius : null;
        options.UsePersistedCornerRadius = usePersistedPreference;
        return this;
    }
}
