---
title: Footer status
description: Configure the Flourish shell footer text, custom items, and built-in items.
---

# Footer status

The footer status area presents low-priority state such as readiness, connection state, power state, or short contextual messages. Enable the footer through [Shell configuration](shell-configuration.md), then use `ConfigureFooter` to define its content.

```csharp
builder
    .ConfigureShell(shell => shell.UseFooter())
    .ConfigureFooter(footer =>
    {
        footer
            .SetStatusText("Ready")
            .AddStatusItem("Online", "\uE774")
            .ShowLANConnectionStatus()
            .ShowPowerStatus();
    });
```

## Primary status text

`SetStatusText` sets the main text in the footer status area.

```csharp
footer.SetStatusText("Ready");
```

Use it for stable state, not for long logs or notifications. Keep the text short so it remains readable in smaller windows.

## Custom status items

`AddStatusItem` adds a compact item with display text and a glyph.

```csharp
footer.AddStatusItem("Online", "\uE774");
footer.AddStatusItem("Synced", "\uE73E");
```

Use custom status items for application-specific state such as account state, workspace name, sync state, or current mode.

Items are displayed in the order in which they are added.

## Built-in status items

`ShowLANConnectionStatus` adds an item that reflects LAN availability when configuration is applied. It does not update automatically. `ShowPowerStatus` adds a static power item; it does not read the current battery or power-source state.

```csharp
footer.ShowLANConnectionStatus();
footer.ShowPowerStatus();
```

Use these helpers when their snapshot or label semantics fit the application. Use application-provided status content for live monitoring.

## Add custom content

`ConfigureFooter` provides status text and status items. Use [Custom shell content](configure-custom-handler.md) for application-provided controls and command buttons.

```csharp
var flourish = FlourishBuilder
    .CreateDefaultBuilder(args)
    .ConfigureShell(shell => shell.UseFooter())
    .ConfigureFooter(footer =>
    {
        footer.SetStatusText("Ready").ShowLANConnectionStatus().ShowPowerStatus();
    })
    .ConfigureCustomHandler(custom =>
    {
        custom.AddFooterCommand("Sync", "\uE895", "sync.run");
    })
    .Build();
```
