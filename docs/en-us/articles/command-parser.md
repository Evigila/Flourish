---
title: Command parser
description: Handle command keys raised by Flourish UI surfaces.
---

# Command parser

Commands assigned to Flourish UI surfaces are dispatched through `ICommandDispatcher`. Implement `ICommandParser` for a fixed set of synchronous handlers registered during service configuration. Runtime handlers registered through `ICommandRegistry` are evaluated first; when dispatch reaches the parsers, they run in registration order.

## Register a parser

Register parser implementations through [Dependency injection](configure-services.md).

```csharp
builder.ConfigureServices((_, services) =>
{
    services.AddSingleton<ICommandParser, AppCommandParser>();
});
```

Multiple parsers can be registered. Each parser returns `true` when it handles the command and `false` when the key is unknown to that parser.

## Implement TryParse

```csharp
internal sealed class AppCommandParser(IMessageService messages) : ICommandParser
{
    public bool TryParse(string commandKey)
    {
        return commandKey switch
        {
            "reports.refresh" => RefreshReports(),
            "reports.export" => ExportReports(),
            "help.open" => ShowHelp(),
            _ => false
        };
    }

    private bool ShowHelp()
    {
        messages.Show(
            "Help is available from the support site.",
            "Foobar",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
        return true;
    }

    private static bool RefreshReports()
    {
        return true;
    }

    private static bool ExportReports()
    {
        return true;
    }
}
```

Return `false` for unknown command keys. Route by stable keys rather than localized display text.

> [!CAUTION]
> `TryParse` runs synchronously and must not block the calling thread. Delegate long-running work to an application service or an asynchronous workflow.

## Connect toolbar items

```csharp
toolbar.CreateToolbarItems<ReportsPage>(
    new FlourishToolbarItem("Export", "\uE898", "reports.export"));
```

The third constructor argument is the command key. It is optional, but toolbar actions that should do work should provide one. [Dynamic toolbar](dynamic-toolbar.md) explains page-specific item registration.

## Connect navigation command items

Navigation command items use the same dispatch path. Add them with `AddNavigableItem` inside a group, or with `AddFixedNavigableItem` in the fixed bottom section described in [Navigation](navigation.md).

```csharp
builder.ConfigureNavigation(navigation =>
{
    navigation.SetGroup("Commands", groupId: 1, group =>
    {
        group.AddNavigableItem("Refresh", "\uE72C", "reports.refresh");
    });

    navigation.AddFixedNavigableItem("Help", "\uE946", "help.open");
});
```

If a command item is a parent node, clicking it expands or collapses children and does not execute the command key.

## Use services inside a parser

Because parsers are resolved from DI, they can depend on application services. Flourish also registers `IMessageService`, which shows Flourish-styled modal messages with the same button, icon, and result enums used by WPF `MessageBox`. It also supports custom options; see [Message service](message-service.md). Title bar and status bar commands described in [Custom shell content](configure-custom-handler.md) use the same dispatch path.

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
