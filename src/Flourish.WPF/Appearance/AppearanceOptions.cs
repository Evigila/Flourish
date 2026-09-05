using System;
using System.Collections.Generic;

using ArkheideSystem.Flourish.Abstract;

namespace ArkheideSystem.Flourish.Appearance;

internal sealed class AppearanceOptions
{
    public string FontFamily { get; set; } = "Segoe UI";
    public string IconFontFamily { get; set; } = "Segoe MDL2 Assets";
    public double FontSizeSmall { get; set; } = 11;
    public double FontSizeStandard { get; set; } = 13;
    public double FontSizeIcon { get; set; } = 14;
    public double FontSizeLarge { get; set; } = 14;
    public double FontSizeExtraLarge { get; set; } = 18;
    public double FontSizeHeaderSize { get; set; } = 25;
    public Dictionary<Type, PageFontOverride> PageFontOverridesByPageType { get; } = [];
    public ThemeColors? ThemeColors { get; set; }
    public double? CornerRadius { get; set; }
    public MaterialEffect MaterialEffect { get; set; } = MaterialEffect.Auto;
    public bool IsMaterialEffectEnabled { get; set; } = true;
    public bool IsThemeEnabled { get; set; }
    public ApplicationTheme DefaultTheme { get; set; } = ApplicationTheme.System;
    public bool UsePersistedTheme { get; set; } = true;
    public bool UsePersistedFont { get; set; } = true;
    public bool UsePersistedMaterialEffect { get; set; } = true;
    public bool UsePersistedThemeColors { get; set; } = true;
    public bool UsePersistedCornerRadius { get; set; } = true;
}
