---
title: ConfigureMotion
description: Configure Flourish transitions and animations.
---

# ConfigureMotion

`ConfigureMotion` configures animation details. Motion runs only when [`ConfigureShell`](configure-shell.md) enables `UseMotion()`.

```csharp
builder
    .ConfigureShell(shell => shell.UseMotion())
    .ConfigureMotion(motion =>
    {
        motion
            .EnablePageTransition(
                FlourishPageTransition.EntranceFromBottom,
                TimeSpan.FromMilliseconds(180))
            .EnableNavigationPanelTransition(
                FlourishNavigationPanelTransition.Resize,
                TimeSpan.FromMilliseconds(180))
            .EnableHoverRevealAnimation(TimeSpan.FromMilliseconds(140))
            .RespectSystemReducedMotion();
    });
```

## Details

Each transition or animation accepts its own optional duration. If no duration is supplied, Flourish uses that animation's default timing.

`EnablePageTransition` controls how pages enter the content frame. `EnableNavigationPanelTransition` controls how the navigation panel opens and closes.

`EnableHoverRevealAnimation` enables subtle hover animation on supported controls. `RespectSystemReducedMotion` lets Flourish follow the operating system reduced-motion preference.

Disabling motion is done through `UseMotion(false)`, not through `ConfigureMotion`.

## Related APIs

- [`ConfigureNavigation`](configure-navigation.md) uses navigation panel transitions.
- [`ConfigureDynamicToolbar`](configure-dynamic-toolbar.md) and [`ConfigureCustomHandler`](configure-custom-handler.md) can host controls affected by hover reveal.
