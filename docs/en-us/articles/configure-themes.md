---
title: ConfigureThemes
description: Configure the default Flourish theme and theme preference behavior.
---

# ConfigureThemes

`ConfigureThemes` chooses the default theme used when [`ConfigureShell`](configure-shell.md) enables `UseThemes()` and no saved preference exists.

```csharp
builder
    .ConfigureShell(shell => shell.UseThemes())
    .ConfigureThemes(FlourishTheme.System);
```

## Details

`FlourishTheme.System` follows the Windows app theme. `Light` and `Dark` force a specific default until the user changes the theme.

When themes are enabled, Flourish stores the selected theme in application preferences. That storage depends on application identity from [`ConfigureData`](configure-data.md).

The title bar theme toggle is controlled by [`ConfigureTitleBar`](configure-title-bar.md). It appears only when both the toggle is shown and themes are enabled.

## Related APIs

- [`ConfigureData`](configure-data.md) provides the preference storage identity.
- [`ConfigureTitleBar`](configure-title-bar.md) can show the title bar theme toggle.
- [`ConfigureMaterialEffect`](configure-material-effect.md) works with effective light and dark resources.
