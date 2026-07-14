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

## Own startup registrations

A DI-owned service can keep related registrations together. Resolve it once after `Build()` so its constructor registers the commands; the Flourish service provider disposes it with the runtime.

```csharp
internal sealed class ReportCommands : IDisposable
{
    private readonly ICommandRegistration refresh;
    private readonly ICommandRegistration export;

    public ReportCommands(ICommandRegistry commands, ReportService reports)
    {
        refresh = commands.Register(
            "reports.refresh",
            async (_, token) =>
            {
                await reports.RefreshAsync(token);
                return CommandResult.Handled;
            });

        export = commands.Register(
            "reports.export",
            async (context, token) =>
            {
                await reports.ExportAsync(context.Parameter, token);
                return CommandResult.Handled;
            });
    }

    public void Dispose()
    {
        export.Dispose();
        refresh.Dispose();
    }
}
```

Register the owner and its dependencies during service configuration, then activate it from the built runtime:

```csharp
builder.ConfigureServices((_, services) =>
{
    services.AddSingleton<ReportService>();
    services.AddSingleton<ReportCommands>();
});

using var flourish = builder.Build();
_ = flourish.GetRequiredService<ReportCommands>();
```

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
