---
title: Typography
description: Configure the font family and six Flourish size tiers.
---

# Typography

Use `SetFont` inside `ConfigureFont` to set the font family and six size tiers for shell surfaces, navigated application pages, and the Profile page.

## Configure shell typography

```csharp
builder.ConfigureFont(font =>
    font.SetFont("Segoe UI", 11, 13, 14, 14, 18, 25));
```

The global families and size scale persist as one preference group by default. Pass `usePersistedPreference: false` to keep configuration authoritative. Page overrides are not persisted.

The parameters are the font family, then Small, Standard, StandardIcon, Large, ExtraLarge, and HeaderSize. Sizes must be positive and finite but need not be ordered. Defaults are `Segoe UI` with `11`, `13`, `14`, `14`, `18`, and `25` DIP.

## Size tier roles

Unspecified text uses `Standard`; choose another tier only for its defined role.

| Tier | Role |
| --- | --- |
| `Small` | Navigation group labels, OutputCard output, and other compact status or caption text owned by a control. |
| `Standard` | All ordinary body and control text, including unspecified text. |
| `StandardIcon` | Ordinary icon glyphs, including button icons. |
| `Large` | Card titles and the selected title-bar title. |
| `ExtraLarge` | The section-title family, including `Chunk.Title`. |
| `HeaderSize` | Reserved for the page title in `HeaderChunk`. |

`Document` paragraphs and `CodeSpace` use `Large` and follow its global or page override.

Large, ExtraLarge, and HeaderSize title roles use `Bold`. Choices in the title dropdown and built-in text inside the logo information surface use Standard. Application-provided content in `TitleBarApplicationInfo` retains its own WPF typography choices.

`StandardIcon` is the configurable default at 14 DIP. Card and display icons use the fixed `LargeIcon` size of 22 DIP. Toolbar, navigation, title-bar, search, status, and window controls keep their geometry-specific corrections.

Choose a font family that supports every language displayed by the application and provides `Regular` and `Bold` faces.

Pages displayed in the main content frame or Profile inherit the configured global font. A child control with an explicit local font, such as a code sample using `Consolas` or an icon using the icon font, keeps that local value.

## Override one page

Use `SetOverrideFont<TPage>` when one page needs a different initial text family or size scale. Pass `null` for any tier that should continue following its global value.

```csharp
builder.ConfigureFont(font =>
{
    font
        .SetFont("Segoe UI", 11, 13, 14, 14, 18, 25)
        .SetOverrideFont<CodeEditorPage>(
            "Cascadia Mono",
            null,
            null,
            null,
            null,
            null,
            null);

    font.SetOverrideFont<PresentationPage>(
        "Aptos Display",
        14,
        16,
        19,
        22,
        26,
        32);
});
```

Every supplied page tier must be positive and finite. Tiers are otherwise independent, including values inherited through `null`.

## Change typography at runtime

`IFontService` applies the same atomic seven-value model after startup. Overrides are matched by configured page type and are reapplied when cached or dynamically registered pages are displayed.

```csharp
fontService.SetFont("Segoe UI", 11, 13, 14, 14, 18, 25);

fontService.SetOverrideFont<CodeEditorPage>(
    "Cascadia Mono",
    null,
    null,
    null,
    null,
    null,
    null);

fontService.SetOverrideFont(
    typeof(DiagnosticsPage),
    "Segoe UI",
    11,
    14,
    16,
    19,
    22,
    28);

IReadOnlyDictionary<Type, PageFontOverride> overrides =
    fontService.Current.PageOverrides;

fontService.RemoveOverrideFont<CodeEditorPage>();
```

Clearing an override immediately returns the active page to the latest global font. A `null` override tier continues to follow later global changes.

## Related features

- [Window](configure-window.md)
- [Title bar](configure-title-bar.md), [Navigation](navigation.md), and [Status bar](status-bar.md)
- [Themes](configure-themes.md)
