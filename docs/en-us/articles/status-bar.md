---
title: Status bar
description: Configure custom status items, background-task indicators, and consolidated system status.
---

# Status bar

The status bar is the compact Shell surface for active background work, application-defined status items, and built-in system details. Enable its persistent surface through [Shell configuration](shell-configuration.md), then use `ConfigureStatusBar` for optional custom and system items.

```csharp
builder
    .ConfigureShell(shell => shell.UseStatusBar())
    .ConfigureStatusBar(statusBar =>
    {
        statusBar
            .AddStatusItem("Online", "\uE774")
            .ShowLANConnectionStatus()
            .ShowPowerStatus();
    });
```

Use `AddStatusItem` for non-interactive text-and-icon state. Use a [custom footer region](configure-custom-handler.md) for interactive controls or application-defined WPF content.

## Background-task indicators

`IBackgroundTaskService` is integrated automatically with the left side of the status bar:

- each running or cancelling task receives its own icon
- hovering a running icon shows the name, optional description, state, and progress
- clicking a running icon opens the background-task flyout
- queued tasks share one plain numeric count without an icon or badge
- hovering or clicking the queue count opens the waiting list and its cancellation controls

The Shell takes task names, descriptions, and glyphs from `FlourishBackgroundTaskMetadata`. A task that has not supplied an icon uses the built-in task glyph.

Active work temporarily shows the status bar even when `UseStatusBar()` was omitted. When no active tasks remain, the bar returns to its configured visibility. See [Background tasks](background-tasks.md) for submission, bounded concurrency, cancellation, progress, and results.

## Custom status items

`AddStatusItem` adds a compact, non-interactive item with Small display text and an icon glyph. Items appear in registration order before the system-status icon.

```csharp
statusBar.AddStatusItem("Online", "\uE774");
statusBar.AddStatusItem("Synced", "\uE73E");
```

Use custom items for application-specific state such as account state, workspace name, synchronization state, or current mode. The supplied text is application content and is not translated automatically.

## Consolidated network and power status

`ShowLANConnectionStatus` and `ShowPowerStatus` enable rows in one consolidated system-status icon on the right side of the status bar. Configuring either helper displays that single icon; configuring both does not create two separate icons.

```csharp
statusBar
    .ShowLANConnectionStatus()
    .ShowPowerStatus();
```

Hover or click the icon to open its temporary [Overlay](../controls/overlay.md). It closes after the pointer leaves both the icon and surface. The network row reads current network availability when the overlay opens. The power row reports AC, battery, or unknown power source and includes the battery percentage when Windows supplies a valid value. These are current snapshots taken when the surface opens, not a continuous connectivity or battery monitor.

Built-in labels follow the locale selected through [Application data](configure-data.md).

## Add custom footer content

Use [Custom shell content](configure-custom-handler.md) for application-provided controls and command buttons. `FooterStart` is placed after built-in background-task indicators; `FooterEnd` is placed after the custom and system status area.

```csharp
builder.ConfigureCustomHandler(custom =>
{
    custom.AddFooterCommand(
        FlourishRegion.FooterEnd,
        "Sync",
        "\uE895",
        "sync.run");
});
```

Custom content does not enable the persistent status bar by itself, so call `UseStatusBar()` when it should remain visible without active background work.
