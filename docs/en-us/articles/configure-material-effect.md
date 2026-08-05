---
title: Material effects
description: Select the Windows material used by the Flourish shell window.
---

# Material effects

`UseMaterialEffect` selects the background material for the shell window.

```csharp
builder.ConfigShell(shell =>
    shell.UseMaterialEffect());
```

## Select a material

| Value | Behavior |
| --- | --- |
| `MaterialEffect.Auto` | Uses Mica on Windows 11, Acrylic on Windows 10, and no material on unsupported systems. |
| `MaterialEffect.None` | Uses an opaque shell background without a system material. |
| `MaterialEffect.Mica` | Uses the long-lived-window Mica backdrop on Windows 11. |
| `MaterialEffect.Acrylic` | Uses Desktop Acrylic through the Windows 11 system backdrop or the Windows 10 compatibility backend. |
| `MaterialEffect.MicaAlt` | Uses the stronger Mica Alt backdrop on Windows 11 build 22621 or later. |

`MaterialEffect.Auto` is the default request, and material effects are enabled by default. Its effective value is Mica on Windows 11 build 22000 or later, Acrylic on Windows 10 build 17134 or later, and `None` elsewhere. Windows 11 build 22621 or later uses the documented DWM system-backdrop API. Initial Windows 11 builds use the legacy Mica attribute, while Windows 10 Acrylic uses the isolated AccentPolicy compatibility backend.

Pass `enabled: false` or select `MaterialEffect.None` to use the opaque Shell background. `IsSupported` reports whether a concrete material is available. `SetEffect` throws `PlatformNotSupportedException` before changing state when a concrete unsupported material is requested; `Auto` never throws and safely resolves to `None` when necessary.

Material is applied to the shell window. The built-in page host remains transparent so the backdrop continues through the content area. Pages can still add local backgrounds when their design requires one. `CurrentEffect` returns the requested value and `EffectiveEffect` returns the concrete platform result.

The runtime material selection is restored from and written to the selected Flourish settings file by default. Pass `usePersistedPreference: false` to keep the configured material authoritative. A missing, invalid, or incomplete saved group leaves the configured arguments intact. When a concrete saved material is moved to a platform that does not support it, Flourish changes that saved request to `Auto` and uses the new platform default instead of failing Shell startup.

> [!NOTE]
> Windows can replace Acrylic with a solid fallback when transparency is disabled, battery saver is active, or system rendering policy requires it. A successful native call therefore does not guarantee visible blur in every system state.

## Related features

- [Window](configure-window.md) configures the window that receives the material.
- [Themes](configure-themes.md) control light and dark resources used with the material.
- [DWM system backdrop types](https://learn.microsoft.com/windows/win32/api/dwmapi/ne-dwmapi-dwm_systembackdrop_type) define the Windows 11 mappings.
