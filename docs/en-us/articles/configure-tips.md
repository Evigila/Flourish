---
title: Tooltips
description: Configure the Flourish presentation for tooltips owned by Flourish controls and Shell surfaces.
---

# Tooltips

Tooltips provide labels for compact or icon-only Flourish controls and Shell surfaces. `UseTips` switches these hints from the native WPF tooltip presentation to the Flourish presentation and sets its initial delay.

## Configure tooltips

```csharp
builder.ConfigureShell(shell => shell.UseTips(delay: 200));
```

The `delay` argument is the time in milliseconds between pointer hover and the Flourish tooltip appearing. It defaults to `200` and must be non-negative. The Flourish presentation uses a temporary [Overlay](../controls/overlay.md) and closes when the pointer leaves the tooltip context.

Flourish keeps this presentation within the shell boundary. Omit `UseTips` to present the same tooltip content with the native WPF appearance, timing, placement, and opening behavior. Disabling the `ToolTips` feature at runtime performs the same fallback; enabling or configuring it restores the Flourish presentation across Flourish title-bar, navigation, toolbar, and status controls.

## Native and third-party controls

`UseTips` does not configure tooltips attached to native WPF controls or tooltips owned by third-party controls. Flourish does not apply an application-wide template to those tooltips, so they always retain their own appearance, timing, placement, and opening behavior.

When the Flourish presentation is active, WPF `ToolTipService` still owns opening, delay, popup placement, and closure.

Background-task status details are not controlled by `UseTips`. They remain available through [Background tasks](background-tasks.md).

## Related features

- [Shell configuration](shell-configuration.md) configures the presentation of Flourish-owned tooltips.
- [Title bar](configure-title-bar.md), [Navigation](navigation.md), and [Status bar](status-bar.md) contain controls governed by this setting.
- [Background tasks](background-tasks.md) provides task status and queue details.
