---
title: Material effects
description: Select the Windows material used by the Flourish shell window.
---

# Material effects

Material effects integrate the shell background with supported Windows desktop composition. `UseMaterialEffect` selects and applies the shell material in one step.

## Configure the material

```csharp
builder.ConfigureShell(shell =>
    shell.UseMaterialEffect(MaterialEffect.Mica));
```

## Platform behavior

`MaterialEffect.Mica` applies the Windows Mica material when the platform supports it. `MaterialEffect.None` keeps the shell fully opaque and avoids platform-specific composition behavior.

`MaterialEffect.Mica` is the default argument. Pass `MaterialEffect.None` to disable material composition explicitly, or omit `UseMaterialEffect` when no material should be configured.

Material behavior belongs to the shell window, not to page content. Pages can still define their own WPF backgrounds inside the content frame.

## Related features

- [Window](configure-window.md) configures the window that receives the material.
- [Themes](configure-themes.md) control light and dark resources used with the material.
