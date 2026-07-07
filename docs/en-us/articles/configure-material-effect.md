---
title: ConfigureMaterialEffect
description: Configure the system material used by the Flourish shell window.
---

# ConfigureMaterialEffect

`ConfigureMaterialEffect` selects the material effect used when [`ConfigureShell`](configure-shell.md) enables `UseMaterialEffect()`.

```csharp
builder
    .ConfigureShell(shell => shell.UseMaterialEffect())
    .ConfigureMaterialEffect(MaterialEffect.Mica);
```

## Details

`MaterialEffect.Mica` applies the Windows Mica material when the platform supports it. `MaterialEffect.None` keeps the shell fully opaque and avoids platform-specific composition behavior.

The feature switch and material choice are intentionally separate. `UseMaterialEffect(false)` disables material even if `ConfigureMaterialEffect(MaterialEffect.Mica)` appears later in the builder chain.

Material behavior belongs to the shell window, not to page content. Pages can still define their own WPF backgrounds inside the content frame.

## Related APIs

- [`ConfigureWindow`](configure-window.md) configures the window that receives the material.
- [`ConfigureThemes`](configure-themes.md) controls light and dark resources used with the material.
