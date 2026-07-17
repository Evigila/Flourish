---
title: Typography
description: Set the global font used by Flourish shell surfaces and application pages, with optional page-specific overrides.
---

# Typography

Use `UseGlobalFont` inside `ConfigureShell` to set the font family, five text sizes, and the dedicated icon size for shell surfaces, navigated application pages, and the Profile page.

## Configure shell typography

```csharp
builder.ConfigureShell(shell =>
    shell.UseGlobalFont("Segoe UI", 12, 14, 16, 16, 24, 32));
```

The seven parameters are the font family followed by Small, Standard, Icon, Large, ExtraLarge, and HeaderSize. Each size must be positive and finite. The tiers are independent and may use equal values; Flourish does not impose a relative size order. When `UseGlobalFont` is not called, Flourish uses `Segoe UI` with `12`, `14`, `16`, `16`, `24`, and `32` DIP.

Built-in controls use Standard for ordinary text, Small for navigation group labels and status/output text, Icon for glyphs, Large with `Bold` for card titles and the selected title-bar title, ExtraLarge with `Bold` for Chunk titles, and HeaderSize with `Bold` for ChunkHero titles. Choices in the title dropdown and built-in text inside the logo information surface use Standard. Application-provided content in `TitlebarApplicationInfo` retains its own WPF typography choices.

Small and Standard have compact line spacing and bottom space; Large, ExtraLarge, and HeaderSize progressively add more, while Icon adds none.

Icon is the default glyph size. Flourish applies fixed visual corrections where the Segoe MDL2 glyph bounds differ substantially: navigation `18`, title-bar commands `16`, window captions `12`, search `14`, status-bar items `14`, status-bar background tasks `12`, background-task detail rows `16`, and system-status detail rows `16` DIP. These contextual corrections do not change the configured default Icon value.

Choose a font family that supports every language displayed by the application and provides `Regular` and `Bold` faces.

Pages displayed in the main content frame or Profile inherit the configured global font. A child control with an explicit local font, such as a code sample using `Consolas` or an icon using the icon font, keeps that local value.

## Override one page

Use `SetOverrideFont<TPage>` when one page needs a different text family or size scale. Pass `null` for any tier that should continue following its global value.

```csharp
builder.ConfigureShell(shell =>
    shell
        .UseGlobalFont("Segoe UI", 12, 14, 16, 16, 24, 32)
        .SetOverrideFont<CodeEditorPage>(
            "Cascadia Mono",
            null,
            null,
            null,
            null,
            null,
            null));

shell.SetOverrideFont<PresentationPage>(
    "Aptos Display",
    14,
    16,
    19,
    22,
    26,
    32);
```

Every supplied page tier must be positive and finite. Tiers are otherwise independent, including values inherited through `null`.

## Change typography at runtime

`IFontService` applies the same atomic seven-value model after startup. Overrides are matched by configured page type and are reapplied when cached or dynamically registered pages are displayed.

```csharp
fontService.SetFont("Segoe UI", 12, 14, 16, 16, 24, 32);

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

IReadOnlyDictionary<Type, FlourishPageFontOverride> overrides =
    fontService.PageOverrides;

fontService.ClearOverrideFont<CodeEditorPage>();
```

Clearing an override immediately returns the active page to the latest global font. A `null` override tier continues to follow later global changes.

## Related features

- [Window](configure-window.md) controls the available space for shell text.
- [Title bar](configure-title-bar.md), [Navigation](navigation.md), and [Status bar](status-bar.md) display text affected by the configured font.
- [Themes](configure-themes.md) provide resources for text and background colors.
