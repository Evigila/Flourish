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

## Page rendering during transitions

Page transitions animate a Shell-owned, non-interactive mask above the content frame. The navigated page itself remains stationary and fully opaque, which keeps its text pixel-aligned and avoids measuring or arranging page content on every animation frame.

`Fade` fades the mask away to reveal the page. `EntranceFromBottom` reveals the stationary page from the bottom edge. Both effects return the mask to its hidden state when the transition completes without changing the page's final layout.

## Reduced motion

`RespectSystemReducedMotion` lets Flourish follow the operating system reduced-motion preference. Use it when animations are enabled so the shell can adapt to the user's accessibility setting.

`UseMotion(false)` disables all configured motion.

## Related features

- [Control library](control-library.md) describes the standard templates and the public HoverReveal attached behavior.
- [Navigation](navigation.md) uses navigation panel transitions.
