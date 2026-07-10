---
title: Command parser
description: Handle command keys raised by Flourish UI surfaces.
---

# Command parser

`ICommandParser` is the public extension point for command keys raised by Flourish UI surfaces, especially dynamic toolbar items and command navigation items. A UI item stores a command key; when the user invokes that item, Flourish asks registered parsers whether they can handle the key.

## Register a parser

Register parser implementations through [Dependency injection](configure-services.md).

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
            "reports.export" => ExportReports(),
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

    private static bool ExportReports()
    {
        return true;
    }
}
```

`TryParse` should be fast and explicit. Avoid routing by display text; use stable command keys.

> [!CAUTION]
> `TryParse` is called synchronously on the UI thread that triggered the command. Delegate long-running work to an application service or an asynchronous workflow instead of blocking the parser.

## Connect toolbar items

```csharp
toolbar.CreateToolbarItems<ReportsPage>(
    new FlourishToolbarItem("Export", "\uE898", "reports.export"));
```

The third constructor argument is the command key. It is optional, but toolbar actions that should do work should provide one. [Dynamic toolbar](dynamic-toolbar.md) explains page-specific item registration.

## Connect navigation command items

Navigation command items use the same parser path. Add them with `AddNavigableItem` inside a group, or with `AddFixedNavigableItem` in the fixed bottom section described in [Navigation](navigation.md).

```csharp
builder.ConfigureNavigation(navigation =>
{
    navigation.SetGroup("Commands", groupId: 1, group =>
    {
        group.AddNavigableItem("Refresh", "reports.refresh", iconGlyph: "\uE72C");
    });

    navigation.AddFixedNavigableItem("About", "app.about", iconGlyph: "\uE946");
});
```

If a command item is a parent node, clicking it expands or collapses children and does not execute the command key.

## Use services inside a parser

Because parsers are resolved from DI, they can depend on application services. Flourish also registers `IMessageService`, which shows Flourish-styled modal messages with the same button, icon, and result enums used by WPF `MessageBox`. It also supports custom options; see [Message service](message-service.md). Title bar and footer commands described in [Custom shell content](configure-custom-handler.md) use the same parser path.

```csharp
internal sealed class ReportsCommandParser(ReportExporter exporter) : ICommandParser
{
    public bool TryParse(string commandKey)
    {
        if (commandKey != "reports.export")
        {
            return false;
        }

        exporter.Export();
        return true;
    }
}
```

Register the dependency as usual:

```csharp
services.AddSingleton<ReportExporter>();
services.AddSingleton<ICommandParser, ReportsCommandParser>();
```

## Command key conventions

- Use lowercase dotted names such as `reports.export`.
- Prefix keys by feature or page.
- Keep keys stable even when display text is localized.
- Return `false` for unknown keys instead of throwing.
