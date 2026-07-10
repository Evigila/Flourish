---
title: Material effects
description: Select the Windows material used by the Flourish shell window.
---

# Material effects

Material effects integrate the shell background with supported Windows desktop composition. Enable the feature through [Shell configuration](shell-configuration.md), then select the effect with `ConfigureMaterialEffect`.

## Configure the material

```csharp
builder
    .ConfigureShell(shell => shell.UseMaterialEffect())
    .ConfigureMaterialEffect(MaterialEffect.Mica);
```

## Platform behavior

`MaterialEffect.Mica` applies the Windows Mica material when the platform supports it. `MaterialEffect.None` keeps the shell fully opaque and avoids platform-specific composition behavior.

`UseMaterialEffect(false)` disables the material even when an effect is configured.

Material behavior belongs to the shell window, not to page content. Pages can still define their own WPF backgrounds inside the content frame.

## Related features

- [Window](configure-window.md) configures the window that receives the material.
- [Themes](configure-themes.md) control light and dark resources used with the material.
