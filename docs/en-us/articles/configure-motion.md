---
title: Motion
description: Configure page, navigation, and hover animations while respecting reduced-motion preferences.
---

# Motion

Motion can communicate page changes, navigation panel state, and hover affordances. Enable motion through [Shell configuration](shell-configuration.md), then use `ConfigureMotion` to select transitions and durations.

## Configure motion

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

## Transitions and durations

Each transition or animation accepts its own optional duration. If no duration is supplied, Flourish uses that animation's default timing.

Explicit durations must be greater than zero. Set the page or navigation transition enum to `None` to disable only that category.

`EnablePageTransition` controls how pages enter the content frame. `EnableNavigationPanelTransition` controls how the navigation panel opens and closes.

`EnableHoverRevealAnimation` enables hover animation on supported controls.

## Navigation panel behavior during transitions

`Resize` animates the navigation panel and the visual bounds of the Shell content area, then commits the final column width when the transition completes. If centered content is enabled, the page and aligned Shell regions remain centered and within the configured maximum width throughout the transition. Content that remains at the maximum width translates without horizontal scaling, so text and internal spacing retain their natural metrics. When the available width crosses the limit, the centered surface resizes within that limit.

## Page behavior during transitions

`Fade` fades the navigated page into view. `EntranceFromBottom` combines the fade with a short upward translation. Flourish temporarily caches the transparent page surface while either transition is active, so the animation is composed from a bitmap instead of redrawing a complex page on every frame. Fractional movement remains available for a smooth translation. The live page, cache, and render transform are restored as soon as the transition completes or is cancelled, and neither transition changes the page's final layout.

## Reduced motion

`RespectSystemReducedMotion` lets Flourish follow the operating system reduced-motion preference. Use it when animations are enabled so the shell can adapt to the user's accessibility setting.

`UseMotion(false)` disables all configured motion.

## Related features

- [Control library](control-library.md) describes the standard templates and the public HoverReveal attached behavior.
- [Navigation](navigation.md) uses navigation panel transitions.
