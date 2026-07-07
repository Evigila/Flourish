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
    typeof(GalleryPage),
    new FlourishToolbarItem("Import", "\uE898", "gallery.import"));
```

## Control icon visibility

The overload with `icon: false` lets you keep text-only toolbar items.

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
| `IconGlyph` | Optional glyph shown with the item. |
| `CommandKey` | Optional command key sent to `ICommandParser`. |

Prefer stable, namespaced command keys such as `gallery.import` or `editor.preview`. They are easier to route than display text and do not change when UI language changes.

## Handle commands

Register one or more `ICommandParser` implementations in `ConfigureServices`.

```csharp
services.AddSingleton<ICommandParser, AppCommandParser>();
```

```csharp
internal sealed class AppCommandParser : ICommandParser
{
    public bool TryParse(string commandKey)
    {
        return commandKey switch
        {
            "home.open" => OpenHome(),
            "home.save" => SaveHome(),
            "gallery.import" => ImportGallery(),
            _ => false
        };
    }
}
```

Return `true` when the command was recognized and handled. Return `false` to allow other parsers to try the same key.

## Complete example

```csharp
var flourish = FlourishBuilder
    .CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
    {
        services.AddSingleton<App>();
        services.AddSingleton<ICommandParser, AppCommandParser>();
        services.AddNavigable<HomePage>("Home", "\uE80F");
        services.AddNavigable<GalleryPage>("Gallery", "\uE91B");
    })
    .ConfigureShell(shell =>
    {
        shell
            .UseNavigation()
            .UseDynamicToolbar();
    })
    .ConfigureNavigation(navigation =>
    {
        navigation
            .SetInitiallyOpen()
            .SetGroup("Navigation", groupId: 0, group =>
            {
                group.AddNavigableViewItem<HomePage>(isInitial: true);
                group.AddNavigableViewItem<GalleryPage>();
            });
    })
    .ConfigureDynamicToolbar(toolbar =>
    {
        toolbar.CreateToolbarItems<HomePage>(
            new FlourishToolbarItem("Open", "\uE8E5", "home.open"),
            new FlourishToolbarItem("Save", "\uE74E", "home.save"));

        toolbar.CreateToolbarItems<GalleryPage>(
            new FlourishToolbarItem("Open", "\uE8E5", "gallery.open"),
            new FlourishToolbarItem("Save", "\uE74E", "gallery.save"),
            new FlourishToolbarItem("Import", "\uE898", "gallery.import"));
    })
    .Build();
```
