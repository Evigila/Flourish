---
title: Typography
description: Set the font family and base size used by Flourish shell surfaces.
---

# Typography

Use `ConfigureFont` to set the font family and base size for Flourish shell surfaces.

## Configure shell typography

```csharp
builder.ConfigureFont("Segoe UI", 14);
```

When only the font family is supplied, `ConfigureFont` uses a base size of `14`.

Choose a font family that supports every language displayed by the application. The base size must be positive and finite.

Flourish derives several shell text sizes from the base value, so changing it affects multiple shell regions. Verify the result at the window sizes the application supports.

Application pages can continue to use their own WPF styles. `ConfigureFont` applies to the shell frame, including the title bar, navigation, toolbar, footer, and shell dialogs.

## Related features

- [Window](configure-window.md) controls the available space for shell text.
- [Title bar](configure-title-bar.md), [Navigation](navigation.md), and [Footer status](status-bar.md) display text affected by the configured font.
