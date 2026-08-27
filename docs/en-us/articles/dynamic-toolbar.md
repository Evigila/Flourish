---
title: Dynamic toolbar
description: Configure page-specific toolbar items and connect them to command dispatch.
---

# Dynamic toolbar

Use `ConfigureToolbar` to enable a Shell toolbar whose commands follow the active page.

## Enable the surface

```csharp
builder.ConfigureToolbar(toolbar => toolbar.SetEnabled());
```

`SetEnabled(false)` keeps the surface disabled even if items are registered.

> [!NOTE]
> Enabling the dynamic toolbar only creates the shell surface. A page shows toolbar buttons after matching items are registered with `ConfigureToolbar`.

## Register items for a page

Use `IToolbarBuilder.Set<TPage>` to associate toolbar items with a WPF page type.

```csharp
builder.ConfigureToolbar(toolbar =>
{
    toolbar
        .SetEnabled()
        .Set<ReportsPage>(
        new ToolbarItem("Refresh", "\uE72C", "cmd_reports_refresh"),
        new ToolbarItem("Export", "\uE898", "cmd_reports_export"));
});
```

## Control icon visibility

The overload with `iconOnly: false` keeps text-only toolbar items.

```csharp
toolbar.Set<EditorPage>(
    iconOnly: false,
    new ToolbarItem("Preview", "\uE8A7", "cmd_editor_preview"));
```

## Toolbar item fields

`ToolbarItem` contains:

| Value | Purpose |
| --- | --- |
| `DisplayName` | Text shown in the toolbar. |
| `IconGlyph` | Glyph shown when icon display is enabled. |
| `CommandKey` | Optional command key dispatched through `ICommandDispatcher`. |

Use stable `cmd_` command keys with underscore-separated segments, such as `cmd_reports_export` or `cmd_editor_preview`. Localizing display text does not change the command key.

## Handle commands

Resolve `ICommandRegistry` from the built runtime and register the command key used by the toolbar item.

```csharp
IRegistration exportCommand = commands.Register(
    "cmd_reports_export",
    async (_, token) =>
    {
        await exporter.ExportAsync(token);
        return CommandResult.Handled;
    });
```

See [Command dispatch](commands.md) and [Custom shell content](configure-custom-handler.md).
