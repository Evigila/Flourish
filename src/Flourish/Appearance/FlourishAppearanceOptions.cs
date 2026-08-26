using System;
using System.Collections.Generic;

using ArkheideSystem.Flourish.Abstract;

namespace ArkheideSystem.Flourish.Appearance;

internal sealed class FlourishAppearanceOptions
{
    public string FontFamily { get; set; } = "Segoe UI";
    public string IconFontFamily { get; set; } = "Segoe MDL2 Assets";
    public double FontSizeSmall { get; set; } = 12;
    public double FontSizeStandard { get; set; } = 14;
    public double FontSizeIcon { get; set; } = 22;
    public double FontSizeLarge { get; set; } = 16;
    public double FontSizeExtraLarge { get; set; } = 24;
    public double FontSizeHeaderSize { get; set; } = 32;
    public Dictionary<Type, FlourishPageFontOverride> PageFontOverridesByPageType { get; } = [];
    public FlourishThemeColors? ThemeColors { get; set; }
    public double? CornerRadius { get; set; }
    public MaterialEffect MaterialEffect { get; set; } = MaterialEffect.Auto;
    public bool IsMaterialEffectEnabled { get; set; } = true;
    public bool IsThemeEnabled { get; set; }
    public FlourishTheme DefaultTheme { get; set; } = FlourishTheme.System;
    public bool UsePersistedTheme { get; set; } = true;
    public bool UsePersistedFont { get; set; } = true;
    public bool UsePersistedMaterialEffect { get; set; } = true;
    public bool UsePersistedThemeColors { get; set; } = true;
    public bool UsePersistedCornerRadius { get; set; } = true;
}
