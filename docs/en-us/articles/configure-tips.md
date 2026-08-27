---
title: Tooltips
description: Configure the Flourish presentation for tooltips owned by Flourish controls and Shell surfaces.
---

# Tooltips

`ConfigureToolTips` applies Flourish presentation and timing to hints owned by Flourish controls and Shell surfaces.

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

Native WPF and third-party tooltips keep their own appearance and behavior.

When the Flourish presentation is active, WPF `ToolTipService` still owns opening, delay, popup placement, and closure.

Background-task status details are not controlled by tooltip presentation. They remain available through [Background tasks](background-tasks.md).

## Related features

- [Shell configuration](shell-configuration.md)
- [Title bar](configure-title-bar.md), [Navigation](navigation.md), and [Status bar](status-bar.md)
- [Background tasks](background-tasks.md)
