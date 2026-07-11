---
title: Typography
description: Set the global font used by Flourish shell surfaces and application pages, with optional page-specific overrides.
---

# Typography

Use `UseGlobalFont` inside `ConfigureShell` to set the font family and base size for the complete Flourish visual tree, including navigated application pages and the Profile page.

## Configure shell typography

```csharp
builder.ConfigureShell(shell =>
    shell.UseGlobalFont("Segoe UI", 14));
```

When only the font family is supplied, `UseGlobalFont` uses a base size of `14`.

Choose a font family that supports every language displayed by the application. The base size must be positive and finite.

Flourish derives several text sizes from the base value, so changing it affects multiple shell regions and page controls. Verify the result at the window sizes the application supports.

WPF `Frame` navigation normally breaks inherited font properties. Flourish explicitly bridges that boundary whenever a main content page or Profile page is displayed. Plain page text inherits the global values, while Flourish controls resolve the same values through page-aware dynamic resources. A child control with an explicit local font, such as a code sample using `Consolas` or an icon using the icon font, keeps that local value.

## Override one page

Use `SetOverrideFont<TPage>` when one page needs a different text family. Omit the optional size to keep following the global base size.

```csharp
builder.ConfigureShell(shell =>
    shell
        .UseGlobalFont("Segoe UI", 14)
        .SetOverrideFont<CodeEditorPage>("Cascadia Mono"));
```

Pass a size when the page should also have an independent typography scale.

```csharp
shell.SetOverrideFont<PresentationPage>("Aptos Display", 16);
```

## Change overrides at runtime

`IFontService` applies the same model after startup. Overrides are matched by configured page type and are reapplied when cached or dynamically registered pages are displayed.

```csharp
fontService.SetOverrideFont<CodeEditorPage>("Cascadia Mono");
fontService.SetOverrideFont(typeof(DiagnosticsPage), "Segoe UI", 15);

IReadOnlyDictionary<Type, FlourishPageFontOverride> overrides =
    fontService.PageOverrides;

fontService.ClearOverrideFont<CodeEditorPage>();
```

Clearing an override immediately returns the active page to the latest global font. A `null` override size continues to follow later global size changes.

## Related features

- [Window](configure-window.md) controls the available space for shell text.
- [Title bar](configure-title-bar.md), [Navigation](navigation.md), and [Status bar](status-bar.md) display text affected by the configured font.
