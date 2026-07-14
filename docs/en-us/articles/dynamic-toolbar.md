---
title: Dynamic toolbar
description: Configure page-specific toolbar items and connect them to command dispatch.
---

# Dynamic toolbar

The dynamic toolbar is a shell surface whose items change with the active page. Use it for page-scoped commands such as open, save, import, or refresh.

There are two steps:

1. Enable the toolbar surface in [Shell configuration](shell-configuration.md).
2. Register page-specific toolbar items with `ConfigureDynamicToolbar`.

## Enable the surface

```csharp
builder.ConfigureShell(shell =>
{
    shell.UseDynamicToolbar();
});
```

`UseDynamicToolbar(false)` keeps the surface disabled even if items are registered.

> [!NOTE]
> Enabling the dynamic toolbar only creates the shell surface. A page shows toolbar buttons after matching items are registered with `ConfigureDynamicToolbar`.

## Register items for a page

Use `IFlourishDynamicToolbarBuilder.CreateToolbarItems<TPage>` to associate toolbar items with a WPF page type.

```csharp
builder.ConfigureDynamicToolbar(toolbar =>
{
    toolbar.CreateToolbarItems<ReportsPage>(
        new FlourishToolbarItem("Refresh", "\uE72C", "reports.refresh"),
        new FlourishToolbarItem("Export", "\uE898", "reports.export"));
});
```

## Control icon visibility

The overload with `icon: false` keeps text-only toolbar items.

```csharp
toolbar.CreateToolbarItems<EditorPage>(
    icon: false,
    new FlourishToolbarItem("Preview", "\uE8A7", "editor.preview"));
```

## Toolbar item fields

`FlourishToolbarItem` contains three values:

| Value | Purpose |
| --- | --- |
| `DisplayName` | Text shown in the toolbar. |
| `IconGlyph` | Glyph shown when icon display is enabled. |
| `CommandKey` | Optional command key dispatched through `ICommandDispatcher`. |

Use stable, namespaced command keys such as `reports.export` or `editor.preview`. Localizing display text does not change the command key.

## Handle commands

Resolve `ICommandRegistry` from the built runtime and register the command key used by the toolbar item.

```csharp
ICommandRegistration exportCommand = commands.Register(
    "reports.export",
    async (_, token) =>
    {
        await exporter.ExportAsync(token);
        return CommandResult.Handled;
    });
```

[Command dispatch](commands.md) explains registration ownership, availability, duplicate policies, and results. [Custom shell content](configure-custom-handler.md) can use the same command keys for title bar and status bar commands.
