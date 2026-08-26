---
title: Tooltips
description: Configure the Flourish presentation for tooltips owned by Flourish controls and Shell surfaces.
---

# Tooltips

Tooltips provide labels for compact or icon-only Flourish controls and Shell surfaces. `ConfigureToolTips` switches these hints from the native WPF tooltip presentation to the Flourish presentation and configures its timing.

## Configure tooltips

```csharp
builder.ConfigureToolTips(toolTips =>
    toolTips
        .SetEnabled()
        .SetSettings(initialShowDelayMilliseconds: 200));
```

The `initialShowDelayMilliseconds` argument is the time in milliseconds between pointer hover and the Flourish tooltip appearing. It defaults to `200` and must be non-negative. The Flourish presentation uses a temporary [Overlay](../controls/overlay.md) and closes when the pointer leaves the tooltip context.

Flourish keeps this presentation within the shell boundary. Pass `false` to `SetEnabled`, or omit `ConfigureToolTips`, to present the same tooltip content with the native WPF appearance, timing, placement, and opening behavior. `IToolTipService.SetEnabled` provides the same runtime switch; `Current` exposes the active settings and `Changed` reports updates.

## Native and third-party controls

`ConfigureToolTips` does not configure tooltips attached to native WPF controls or tooltips owned by third-party controls. Flourish does not apply an application-wide template to those tooltips, so they always retain their own appearance, timing, placement, and opening behavior.

When the Flourish presentation is active, WPF `ToolTipService` still owns opening, delay, popup placement, and closure.

Background-task status details are not controlled by tooltip presentation. They remain available through [Background tasks](background-tasks.md).

## Related features

- [Shell configuration](shell-configuration.md) explains the feature-specific Builder and Service model.
- [Title bar](configure-title-bar.md), [Navigation](navigation.md), and [Status bar](status-bar.md) contain controls governed by this setting.
- [Background tasks](background-tasks.md) provides task status and queue details.
