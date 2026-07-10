---
title: Dynamic toolbar
description: Configure page-specific toolbar items and connect them to command parsing.
---

# Dynamic toolbar

The dynamic toolbar is a shell surface whose items change with the active page. It is useful for commands such as open, save, import, refresh, or page-specific actions.

There are two steps:

1. Enable the toolbar surface in shell configuration.
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

Use `IFlourishDynamicToolbarBuilder.CreateToolbarItems<TPage>` when the page type is known at compile time.

```csharp
builder.ConfigureDynamicToolbar(toolbar =>
{
    toolbar.CreateToolbarItems<HomePage>(
        new FlourishToolbarItem("Open", "\uE8E5", "home.open"),
        new FlourishToolbarItem("Save", "\uE74E", "home.save"));
});
```

Use the `Type` overload when page types are discovered dynamically.

```csharp
toolbar.CreateToolbarItems(
    typeof(ReportsPage),
    new FlourishToolbarItem("Export", "\uE898", "reports.export"));
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
| `CommandKey` | Optional command key sent to `ICommandParser`. |

Prefer stable, namespaced command keys such as `reports.export` or `editor.preview`. They are easier to route than display text and do not change when UI language changes.

## Handle commands

Register one or more `ICommandParser` implementations through [Dependency injection](configure-services.md).

```csharp
services.AddSingleton<ICommandParser, AppCommandParser>();
```

[Command parser](command-parser.md) explains command matching and the behavior of multiple parsers. [Custom shell content](configure-custom-handler.md) can use the same command keys for title bar and footer commands.
