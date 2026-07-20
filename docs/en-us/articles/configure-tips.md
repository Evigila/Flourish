---
title: Tooltips
description: Configure tooltip timing and placement inside the shell boundary.
---

# Tooltips

Flourish tooltips provide labels for compact or icon-only shell controls. `UseTips` sets the initial delay and enables them.

## Configure tooltips

```csharp
builder.ConfigureShell(shell => shell.UseTips(delay: 200));
```

The `delay` argument is the time in milliseconds between pointer hover and display. It defaults to `200` and must be non-negative. Tooltips use a temporary [Overlay](../controls/overlay.md) and close when the pointer leaves the tooltip context.

Flourish keeps these tooltips within the shell boundary. Omit `UseTips` when they should remain disabled. Changing the `ToolTips` feature at runtime applies consistently to title-bar, navigation, toolbar, and status controls.

Background-task status details are not controlled by `UseTips`. They remain available through [Background tasks](background-tasks.md).

## Related features

- [Shell configuration](shell-configuration.md) configures and enables tooltips.
- [Title bar](configure-title-bar.md), [Navigation](navigation.md), and [Status bar](status-bar.md) contain controls governed by this setting.
- [Background tasks](background-tasks.md) provides task status and queue details.
