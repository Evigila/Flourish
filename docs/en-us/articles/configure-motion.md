---
title: ConfigureMotion
description: Configure Flourish animation duration and transitions.
---

# ConfigureMotion

`ConfigureMotion` configures animation details. Motion runs only when [`ConfigureShell`](configure-shell.md) enables `UseMotion()`.

```csharp
builder
    .ConfigureShell(shell => shell.UseMotion())
    .ConfigureMotion(motion =>
    {
        motion
            .SetDuration(TimeSpan.FromMilliseconds(180))
            .SetPageTransition(FlourishPageTransition.EntranceFromBottom)
            .SetNavigationPanelTransition(FlourishNavigationPanelTransition.Resize)
            .SetHoverReveal()
            .RespectSystemReducedMotion();
    });
```

## Details

`SetDuration` sets the shared animation duration. The default overload uses a balanced Flourish duration.

`SetPageTransition` controls how pages enter the content frame. `SetNavigationPanelTransition` controls how the navigation panel opens and closes.

`SetHoverReveal` enables subtle hover animation on supported controls. `RespectSystemReducedMotion` lets Flourish follow the operating system reduced-motion preference.

Disabling motion is done through `UseMotion(false)`, not through `ConfigureMotion`.

## Related APIs

- [`ConfigureNavigation`](configure-navigation.md) uses navigation panel transitions.
- [`ConfigureDynamicToolbar`](configure-dynamic-toolbar.md) and [`ConfigureCustomHandler`](configure-custom-handler.md) can host controls affected by hover reveal.
