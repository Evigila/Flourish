---
title: ConfigureDynamicToolbar
description: Configure page-specific dynamic toolbar commands.
---

# ConfigureDynamicToolbar

`ConfigureDynamicToolbar` registers toolbar items that change with the active page. The toolbar surface is displayed only when [`ConfigureShell`](configure-shell.md) enables `UseDynamicToolbar()`.

```csharp
builder
    .ConfigureShell(shell => shell.UseDynamicToolbar())
    .ConfigureDynamicToolbar(toolbar =>
    {
        toolbar.CreateToolbarItems<HomePage>(
            new FlourishToolbarItem("Open", "\uE8E5", "home.open"),
            new FlourishToolbarItem("Save", "\uE74E", "home.save"));
    });
```

## Details

Toolbar items are associated with page types. When navigation displays that page, Flourish replaces the active toolbar content with the matching items.

Each item can include display text, an icon glyph, and a command key. Command keys are routed to `ICommandParser`, so toolbar actions share the same command infrastructure as navigation items and custom region commands.

The `icon: false` overload keeps text-only toolbar items when icons are not appropriate for the command set.

## Related APIs

- [`ConfigureNavigation`](configure-navigation.md) determines which registered pages can become active.
- [`ConfigureCustomHandler`](configure-custom-handler.md) can add custom content at the start or end of the toolbar surface.
- [`Dynamic toolbar`](dynamic-toolbar.md) provides a longer workflow example.
