---
title: Motion
description: Configure page, navigation, and hover animations while respecting reduced-motion preferences.
---

# Motion

Use `ConfigureMotion` to set page, navigation-panel, and hover animation.

## Configure motion

```csharp
builder
    .ConfigureMotion(motion =>
    {
        motion
            .SetEnabled()
            .SetPageTransition(
                transition: PageTransition.EntranceFromBottom,
                duration: TimeSpan.FromMilliseconds(180))
            .SetNavigationPanelTransition(
                transition: NavigationPanelTransition.Resize,
                duration: TimeSpan.FromMilliseconds(180))
            .SetHoverReveal(duration: TimeSpan.FromMilliseconds(140))
            .SetRespectSystemReducedMotion();
    });
```

Motion settings persist independently by default. Pass `usePersistedPreference: false` to keep configuration authoritative.

```csharp
builder
    .ConfigureMotion(motion => motion
        .SetEnabled()
        .SetPageTransition(transition: PageTransition.Fade)
        .SetNavigationPanelTransition(
            transition: NavigationPanelTransition.Resize)
        .SetHoverReveal()
        .SetRespectSystemReducedMotion());
```

## Transitions and durations

Each transition or animation accepts its own optional duration. If no duration is supplied, Flourish uses that animation's default timing.

Explicit durations must be greater than zero. Set the page or navigation transition enum to `None` to disable only that category.

`SetPageTransition` controls how pages enter the content frame. `SetNavigationPanelTransition` controls how the navigation panel opens and closes.

`SetHoverReveal` enables hover animation on supported controls, including the Button family, CheckBox layouts, selection-item containers, the closed ComboBox selector, and the parent-owned interaction layer of BunchedListBox.

## Navigation panel behavior during transitions

`Resize` animates the navigation panel and Shell content bounds, then commits the final width. Centered content remains centered, respects its maximum width, and is not horizontally scaled.

## Page behavior during transitions

`Fade` fades the page in; `EntranceFromBottom` also moves it upward. Transitions do not change final layout, and cancellation restores the live page immediately.

## Reduced motion

`SetRespectSystemReducedMotion` lets Flourish follow the operating system reduced-motion preference. Use it when animations are enabled so the shell can adapt to the user's accessibility setting.

At runtime, `IMotionService.SetEnabled(false)` disables all configured motion. `IMotionService.Current` exposes the active settings, and `Changed` reports updates.

## Related features

- [Control library](control-library.md)
- [Navigation](navigation.md)
