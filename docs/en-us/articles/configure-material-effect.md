---
title: Material effects
description: Select the Windows material used by the Flourish shell window.
---

# Material effects

`SetEffect` selects the background material for the shell window.

```csharp
builder.ConfigureAppearance(appearance =>
    appearance.SetEffect());
```

## Select a material

| Value | Behavior |
| --- | --- |
| `MaterialEffect.Auto` | Uses Mica on Windows 11, Acrylic on Windows 10, and no material on unsupported systems. |
| `MaterialEffect.None` | Uses an opaque shell background without a system material. |
| `MaterialEffect.Mica` | Uses the long-lived-window Mica backdrop on Windows 11. |
| `MaterialEffect.Acrylic` | Uses Desktop Acrylic through the Windows 11 system backdrop or the Windows 10 compatibility backend. |
| `MaterialEffect.MicaAlt` | Uses the stronger Mica Alt backdrop on Windows 11 build 22621 or later. |

Materials are enabled with `Auto` by default: Mica on Windows 11 build 22000 or later, Acrylic on Windows 10 build 17134 or later, and `None` elsewhere.

Pass `enabled: false` or select `MaterialEffect.None` to use the opaque Shell background. `IsSupported` reports whether a concrete material is available. `SetEffect` throws `PlatformNotSupportedException` before changing state when a concrete unsupported material is requested; `Auto` never throws and safely resolves to `None` when necessary.

Material applies to the Shell window while the page host stays transparent. `IMaterialEffectService.Current` reports requested and effective values; `Changed` publishes updates.

Material persists by default. Pass `usePersistedPreference: false` to keep configuration authoritative. Invalid saved data leaves the fallback unchanged; an unsupported saved material becomes `Auto` instead of failing startup.

> [!NOTE]
> Windows can replace Acrylic with a solid fallback when transparency is disabled, battery saver is active, or system rendering policy requires it. A successful native call therefore does not guarantee visible blur in every system state.

## Related features

- [Window](configure-window.md)
- [Themes](configure-themes.md)
- [DWM system backdrop types](https://learn.microsoft.com/windows/win32/api/dwmapi/ne-dwmapi-dwm_systembackdrop_type)
