---
title: Status bar
description: Configure the Flourish shell status bar text, custom items, and built-in items.
---

# Status bar

The status bar presents low-priority state such as readiness, connection state, power state, or short contextual messages. Enable it through [Shell configuration](shell-configuration.md), then use `ConfigureStatusBar` to define its content.

```csharp
builder
    .ConfigureShell(shell => shell.UseStatusBar())
    .ConfigureStatusBar(statusBar =>
    {
        statusBar
            .SetStatusText("Ready")
            .AddStatusItem("Online", "\uE774")
            .ShowLANConnectionStatus()
            .ShowPowerStatus();
    });
```

## Primary status text

`SetStatusText` sets the main text in the status bar.

```csharp
statusBar.SetStatusText("Ready");
```

Use it for stable state, not for long logs or notifications. Keep the text short so it remains readable in smaller windows.

## Custom status items

`AddStatusItem` adds a compact item with display text and a glyph.

```csharp
statusBar.AddStatusItem("Online", "\uE774");
statusBar.AddStatusItem("Synced", "\uE73E");
```

Use custom status items for application-specific state such as account state, workspace name, sync state, or current mode.

Items are displayed in the order in which they are added.

## Built-in status items

`ShowLANConnectionStatus` adds an item that reflects LAN availability when configuration is applied. It does not update automatically. `ShowPowerStatus` adds a static power item; it does not read the current battery or power-source state.

These built-in labels follow the locale selected through [Application data](configure-data.md). Text passed to `SetStatusText` or `AddStatusItem` is application content and is not translated automatically.

```csharp
statusBar.ShowLANConnectionStatus();
statusBar.ShowPowerStatus();
```

Use these helpers when their snapshot or label semantics fit the application. Use application-provided status content for live monitoring.

## Add custom content

`ConfigureStatusBar` provides status text and status items. Use [Custom shell content](configure-custom-handler.md) for application-provided controls and command buttons.

```csharp
var flourish = FlourishBuilder
    .CreateDefaultBuilder(args)
    .ConfigureShell(shell => shell.UseStatusBar())
    .ConfigureStatusBar(statusBar =>
    {
        statusBar.SetStatusText("Ready").ShowLANConnectionStatus().ShowPowerStatus();
    })
    .ConfigureCustomHandler(custom =>
    {
        custom.AddFooterCommand("Sync", "\uE895", "sync.run");
    })
    .Build();
```
