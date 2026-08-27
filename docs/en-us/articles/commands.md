---
title: Command dispatch
description: Register asynchronous command handlers and dispatch command keys from Flourish UI surfaces.
---

# Command dispatch

Use `ICommandRegistry` to register handlers and `ICommandDispatcher` to send stable command keys after `Build()`.

## Register handlers

`ICommandRegistry.Register` returns an `IRegistration`. Dispose it to unregister the asynchronous handler.

```csharp
ICommandRegistry commands = flourish.GetRequiredService<ICommandRegistry>();

IRegistration exportCommand = commands.Register(
    "cmd_reports_export",
    async (context, cancellationToken) =>
    {
        await exporter.ExportAsync(context.Parameter, cancellationToken);
        return CommandResult.Handled;
    });
```

Handlers receive a `CommandContext` containing the command key, optional parameter, and originating `CommandSource`. Return `CommandResult.Handled`, `HandledWith(value)`, `NotHandled`, `Canceled`, or `Failed(exception)` to describe the outcome.

## Define startup mappings

A command parser owns host-lifetime mappings. Flourish invokes parsers at startup and removes their mappings in reverse order at shutdown.

```csharp
internal sealed class ReportCommands(ReportService reports)
    : ICommandParser
{
    public void RegisterCommands(ICommandRegistrar commands)
    {
        commands.Register(
            "cmd_reports_refresh",
            async (_, token) =>
            {
                await reports.RefreshAsync(token);
                return CommandResult.Handled;
            });

        commands.Register(
            "cmd_reports_export",
            async (context, token) =>
            {
                await reports.ExportAsync(context.Parameter, token);
                return CommandResult.Handled;
            });
    }
}
```

Register the parser and its dependencies during service configuration:

```csharp
builder.ConfigureServices((_, services) =>
{
    services.AddSingleton<ReportService>();
    services.AddCommandParser<ReportCommands>();
});
```

`ICommandRegistrar.Register` returns no lease. Define mappings synchronously inside `RegisterCommands`, do not retain the registrar, and use `ICommandRegistry` for dynamic lifetimes.

## Control availability

Pass a predicate to `Register` when availability depends on application state. Flourish UI can query it through `ICommandDispatcher.CanExecute`.

```csharp
var saveCommand = commands.Register(
    "cmd_editor_save",
    async (_, token) =>
    {
        await editor.SaveAsync(token);
        return CommandResult.Handled;
    },
    _ => editor.HasChanges);
```

Call `commands.NotifyCanExecuteChanged("cmd_editor_save")` when the state used by the predicate changes. Omit the key to notify listeners that any command may have changed.

## Duplicate command keys

The default duplicate policy is `Reject`. Set `CommandRegistrationOptions.DuplicatePolicy` to `Replace` when a new handler should supersede the current registration, or to `Append` when several handlers should be evaluated. Appended handlers run by descending priority and then registration order; dispatch stops when a handler returns a result other than `NotHandled`.

Startup parsers should normally keep `Reject`. `Replace` deactivates existing handlers immediately, so a later startup failure can remove the replacement but cannot reconstruct handlers that it replaced.

## Dispatch directly

Use `ICommandDispatcher` when application code needs to invoke the same path as a Flourish control.

```csharp
CommandResult result = await dispatcher.ExecuteAsync(
    "cmd_reports_export",
    selectedReport,
    CommandSource.Application,
    cancellationToken);

if (result.Status == CommandExecutionStatus.Failed)
{
    logger.LogError(result.Exception, "Export failed");
}
```

The dispatcher captures handler exceptions in `CommandResult` and reports cancellation through `CommandExecutionStatus.Canceled`.

## Connect Flourish surfaces

Toolbar, navigation, title-bar, status-bar, notification, and shortcut APIs accept the same command keys. For example:

```csharp
toolbar.Set<ReportsPage>(
    new ToolbarItem("Export", "\uE898", "cmd_reports_export"));
```

Command keys keep display text localizable and handlers replaceable without rebuilding the UI model.

## Command key conventions

- Use lowercase `cmd_` names with underscore-separated segments, such as `cmd_reports_export`.
- Prefix keys by feature or page.
- Keep keys stable when display text is localized.
- Dispose registrations when their owning feature is removed.
