---
title: Material effects
description: Select the Windows material used by the Flourish shell window.
---

# Material effects

`UseMaterialEffect` selects the background material for the shell window.

```csharp
builder.ConfigureShell(shell =>
    shell.UseMaterialEffect(MaterialEffect.Mica));
```

## Select a material

| Value | Behavior |
| --- | --- |
| `MaterialEffect.Mica` | Uses Windows Mica when the platform supports it. |
| `MaterialEffect.None` | Uses an opaque shell background without a system material. |

`MaterialEffect.Mica` is the default argument. Pass `MaterialEffect.None`, or omit `UseMaterialEffect`, when the shell should not use a system material.

On a platform that does not support the selected material, the shell remains usable without that effect. Application state and content distinctions should not depend on material availability.

Material is applied to the shell window. The built-in page host remains transparent so Mica continues through the content area. Pages can still add local backgrounds when their design requires one.

## Related features

- [Window](configure-window.md) configures the window that receives the material.
- [Themes](configure-themes.md) control light and dark resources used with the material.
