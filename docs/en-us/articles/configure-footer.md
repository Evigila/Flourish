---
title: ConfigureFooter
description: Configure the Flourish footer status area.
---

# ConfigureFooter

`ConfigureFooter` configures the built-in footer status area. The footer is displayed only when [`ConfigureShell`](configure-shell.md) enables `UseFooter()`.

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

## Details

`SetStatusText` provides the primary footer text. It should stay short because the footer is a persistent, low-priority area.

`AddStatusItem` adds compact icon-and-text status items. Built-in helpers add LAN connection and power status indicators.

Custom footer controls belong to [`ConfigureCustomHandler`](configure-custom-handler.md), which can add WPF content or command buttons to `FooterStart` and `FooterEnd`.

## Related APIs

- [`ConfigureShell`](configure-shell.md) owns the `UseFooter` switch.
- [`ConfigureCustomHandler`](configure-custom-handler.md) adds custom footer content and commands.
- [`Footer status`](status-bar.md) gives a workflow-oriented footer overview.
