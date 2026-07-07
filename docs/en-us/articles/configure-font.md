---
title: ConfigureFont
description: Configure the Flourish shell font family and base size.
---

# ConfigureFont

`ConfigureFont` sets the font family and base font size used by the Flourish shell UI.

```csharp
builder.ConfigureFont("Microsoft YaHei", 14);
```

## Details

The font family should support every language the application displays. Native WPF applications with mixed CJK and Latin text commonly choose a Windows system font that covers both scripts.

The font size must be positive and finite. Flourish derives several shell text sizes from this base value, so the value should be chosen for repeated desktop use rather than marketing-scale display text.

Application pages can still use their own WPF styles. `ConfigureFont` targets the Flourish shell frame: title bar, navigation, toolbar, footer, and shell dialogs.

## Related APIs

- [`ConfigureWindow`](configure-window.md) controls the shell size that the font must fit.
- [`ConfigureTitleBar`](configure-title-bar.md), [`ConfigureNavigation`](configure-navigation.md), and [`ConfigureFooter`](configure-footer.md) display shell text affected by the configured font.
