---
title: Tooltips
description: Configure tooltip timing and keep tooltips inside the shell boundary.
---

# Tooltips

Tooltips make compact or icon-only shell controls discoverable. `UseTips` sets the initial delay and enables Flourish tooltips in one step.

## Configure tooltips

```csharp
builder.ConfigureShell(shell => shell.UseTips(delay: 200));
```

## Timing and placement

The `delay` argument controls the initial hover delay in milliseconds. A shorter value shows help sooner; a longer value reduces unintended tooltips as the pointer moves across controls. It defaults to `200` and must be non-negative.

Flourish uses its built-in boundary margin to keep tooltips away from shell edges, including controls near the collapsed navigation panel, status bar, or toolbar boundary.

Omit `UseTips` when Flourish tooltips should remain disabled.

## Related features

- [Shell configuration](shell-configuration.md) configures and enables tooltips.
- [Title bar](configure-title-bar.md), [Navigation](navigation.md), and [Status bar](status-bar.md) contain built-in controls that can show tooltips.
