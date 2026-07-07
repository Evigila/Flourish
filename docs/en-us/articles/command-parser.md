---
title: Command parser
description: Handle command keys raised by Flourish UI surfaces.
---

# Command parser

`ICommandParser` is the public extension point for command keys raised by Flourish UI surfaces, especially dynamic toolbar items and command navigation items. A UI item stores a command key; when the user invokes that item, Flourish asks registered parsers whether they can handle the key.

## Register a parser

Register parser implementations in [`ConfigureServices`](configure-services.md).

```csharp
builder.ConfigureServices((_, services) =>
{
    services.AddSingleton<ICommandParser, AppCommandParser>();
});
```

Multiple parsers can be registered. Each parser returns `true` when it handled the command and `false` when the key is unknown to that parser.

## Implement TryParse

```csharp
internal sealed class AppCommandParser(IMessageService messages) : ICommandParser
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

    private bool OpenHome()
    {
        messages.Show(
            "Open from Home",
            "Home",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
        return true;
    }

    private static bool SaveHome()
    {
        return true;
    }

    private static bool ImportGallery()
    {
        return true;
    }
}
```

`TryParse` should be fast and explicit. Avoid routing by display text; use stable command keys.

> [!CAUTION]
> `TryParse` runs on the UI path that triggered the command. Long-running work should be delegated to an application service or an asynchronous workflow instead of blocking the parser.

## Connect toolbar items

```csharp
toolbar.CreateToolbarItems<GalleryPage>(
    new FlourishToolbarItem("Import", "\uE898", "gallery.import"));
```

The third constructor argument is the command key. It is optional, but toolbar actions that should do work should provide one. Toolbar items are usually registered through [`ConfigureDynamicToolbar`](configure-dynamic-toolbar.md).

## Connect navigation command items

Navigation command items use the same parser path. Add them with `AddNavigableItem` inside a group, or with `AddFixedNavigableItem` in the fixed bottom section through [`ConfigureNavigation`](configure-navigation.md).

```csharp
builder.ConfigureNavigation(navigation =>
{
    navigation.SetGroup("Commands", groupId: 1, group =>
    {
        group.AddNavigableItem("Refresh", "gallery.refresh", iconGlyph: "\uE72C");
    });

    navigation.AddFixedNavigableItem("About", "app.about", iconGlyph: "\uE946");
});
```

If a command item is a parent node, clicking it expands or collapses children and does not execute the command key.

## Use services inside a parser

Because parsers are resolved from DI, they can depend on application services. Flourish also registers `IMessageService`, which shows Flourish-styled modal messages with the same button, icon, and result enums used by WPF `MessageBox`. It also supports custom options; see [Message service](message-service.md). Custom title bar and footer commands registered through [`ConfigureCustomHandler`](configure-custom-handler.md) use the same parser path.

```csharp
internal sealed class GalleryCommandParser(ImageLibrary library) : ICommandParser
{
    public bool TryParse(string commandKey)
    {
        if (commandKey != "gallery.import")
        {
            return false;
        }

        library.Import();
        return true;
    }
}
```

Register the dependency as usual:

```csharp
services.AddSingleton<ImageLibrary>();
services.AddSingleton<ICommandParser, GalleryCommandParser>();
```

## Command key conventions

- Use lowercase dotted names such as `gallery.import`.
- Prefix keys by feature or page.
- Keep keys stable even when display text is localized.
- Return `false` for unknown keys instead of throwing.
