---
title: Tooltips
description: Configure tooltip timing and keep tooltips inside the shell boundary.
---

# Tooltips

Tooltips make compact or icon-only shell controls discoverable. Enable them through [Shell configuration](shell-configuration.md), then use `ConfigureTips` to tune timing and placement.

## Configure tooltips

```csharp
builder
    .ConfigureShell(shell => shell.UseTips())
    .ConfigureTips(tips =>
    {
        tips.SetDelay(200).SetSpawnableMargin(5);
    });
```

## Timing and placement

`SetDelay` controls the initial hover delay in milliseconds. A shorter value shows help sooner; a longer value reduces unintended tooltips as the pointer moves across controls.

`SetSpawnableMargin` keeps tooltips away from shell edges, including controls near the collapsed navigation panel, footer, or toolbar boundary.

The delay must be non-negative. The margin must be a finite, non-negative value.

`UseTips(false)` disables tooltips even when timing and margin values have been configured.

## Related features

- [Shell configuration](shell-configuration.md) owns the `UseTips` switch.
- [Title bar](configure-title-bar.md), [Navigation](navigation.md), and [Footer status](status-bar.md) contain built-in controls that can show tooltips.
