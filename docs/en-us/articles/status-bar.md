---
title: Footer status
description: Configure the Flourish shell footer status area.
---

# Footer status

Enable the footer with `ConfigureShell`, then configure the status area through `ConfigureFooter`. It is intended for low-priority state such as readiness, connection state, power state, or short contextual messages.

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

## Built-in status items

`ShowLANConnectionStatus` adds the built-in LAN connection indicator. `ShowPowerStatus` adds the built-in power indicator.

```csharp
footer.ShowLANConnectionStatus();
footer.ShowPowerStatus();
```

These are useful for desktop tools where network or battery state affects the user workflow.

## Where to configure it

Footer status configuration belongs beside the rest of application composition. Custom footer controls belong in `ConfigureCustomHandler`.

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

The footer is part of the shell, so configure it once at startup.
