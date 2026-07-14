---
title: Command dispatch
description: Register asynchronous command handlers and dispatch command keys from Flourish UI surfaces.
---

# Command dispatch

Flourish UI surfaces send stable command keys through `ICommandDispatcher`. Register handlers with `ICommandRegistry`; both interfaces are available from the Flourish service provider after `Build()`.

## Register handlers

`ICommandRegistry.Register` associates a command key with an asynchronous handler and returns an `ICommandRegistration`. Keep the registration for as long as the handler should remain active, then dispose it to unregister the command.

```csharp
ICommandRegistry commands = flourish.GetRequiredService<ICommandRegistry>();

ICommandRegistration exportCommand = commands.Register(
    "reports.export",
    async (context, cancellationToken) =>
    {
        await exporter.ExportAsync(context.Parameter, cancellationToken);
        return CommandResult.Handled;
    });
```

Handlers receive a `CommandContext` containing the command key, optional parameter, and originating `CommandSource`. Return `CommandResult.Handled`, `HandledWith(value)`, `NotHandled`, `Canceled`, or `Failed(exception)` to describe the outcome.

## Define startup mappings

A command parser defines host-lifetime mappings between command keys and handlers without exposing their registration leases. Flourish invokes each parser when the host starts and removes its mappings in reverse order when the host stops.

```csharp
internal sealed class ReportCommands(ReportService reports)
    : ICommandParser
{
    public void RegisterCommands(ICommandRegistrar commands)
    {
        commands.Register(
            "reports.refresh",
            async (_, token) =>
            {
                await reports.RefreshAsync(token);
                return CommandResult.Handled;
            });

        commands.Register(
            "reports.export",
            async (context, token) =>
            {
                await reports.ExportAsync(context.Parameter, token);
                return CommandResult.Handled;
            });
    }
}
```

Register the parser and its dependencies during service configuration. The application does not need to resolve the parser or implement `IDisposable`:

```csharp
builder.ConfigureServices((_, services) =>
{
    services.AddSingleton<ReportService>();
    services.AddCommandParser<ReportCommands>();
});
```

`ICommandRegistrar.Register` returns no lease because the host owns it. Parsers must define their mappings synchronously inside `RegisterCommands` and must not retain the registrar. Use `ICommandRegistry` directly when a handler must be added or removed while the host remains running.

## Control availability

Pass a predicate to `Register` when availability depends on application state. Flourish UI can query it through `ICommandDispatcher.CanExecute`.

```csharp
var saveCommand = commands.Register(
    "editor.save",
    async (_, token) =>
    {
        await editor.SaveAsync(token);
        return CommandResult.Handled;
    },
    _ => editor.HasChanges);
```

Call `commands.NotifyCanExecuteChanged("editor.save")` when the state used by the predicate changes. Omit the key to notify listeners that any command may have changed.

## Duplicate command keys

The default duplicate policy is `Reject`. Set `CommandRegistrationOptions.DuplicatePolicy` to `Replace` when a new handler should supersede the current registration, or to `Append` when several handlers should be evaluated. Appended handlers run by descending priority and then registration order; dispatch stops when a handler returns a result other than `NotHandled`.

Startup parsers should normally keep `Reject`. `Replace` deactivates existing handlers immediately, so a later startup failure can remove the replacement but cannot reconstruct handlers that it replaced.

## Dispatch directly

Use `ICommandDispatcher` when application code needs to invoke the same path as a Flourish control.

```csharp
CommandResult result = await dispatcher.ExecuteAsync(
    "reports.export",
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
toolbar.CreateToolbarItems<ReportsPage>(
    new FlourishToolbarItem("Export", "\uE898", "reports.export"));
```

Command items do not need to know which service handles the key. This keeps display text localizable and lets registrations change without rebuilding the UI model.

## Command key conventions

- Use lowercase dotted names such as `reports.export`.
- Prefix keys by feature or page.
- Keep keys stable when display text is localized.
- Dispose registrations when their owning feature is removed.
