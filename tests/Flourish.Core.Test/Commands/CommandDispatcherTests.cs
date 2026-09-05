using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Commands;

using Xunit;

namespace ArkheideSystem.Flourish.Core.Test.Commands;

public sealed class CommandDispatcherTests
{
    [Fact]
    public async Task ExecuteAsync_ReceivesContextAndReturnsValue()
    {
        var dispatcher = new CommandDispatcher();
        CommandContext? received = null;
        dispatcher.Register(
            "cmd_save",
            (context, _) =>
            {
                received = context;
                return ValueTask.FromResult(CommandResult.HandledWith("saved"));
            }
        );

        var result = await dispatcher.ExecuteAsync(
            "cmd_save",
            42,
            CommandSource.Toolbar
        );

        Assert.True(result.IsHandled);
        Assert.Equal("saved", result.Value);
        Assert.NotNull(received);
        Assert.Equal("cmd_save", received.CommandKey);
        Assert.Equal(42, received.Parameter);
        Assert.Equal(CommandSource.Toolbar, received.Source);
    }

    [Fact]
    public async Task Append_UsesPriorityThenRegistrationOrder()
    {
        var dispatcher = new CommandDispatcher();
        var calls = new List<string>();
        dispatcher.Register(
            "cmd_save",
            (_, _) =>
            {
                calls.Add("low");
                return ValueTask.FromResult(CommandResult.Handled);
            }
        );
        dispatcher.Register(
            "cmd_save",
            (_, _) =>
            {
                calls.Add("high-first");
                return ValueTask.FromResult(CommandResult.NotHandled);
            },
            options: new CommandRegistrationOptions
            {
                DuplicatePolicy = CommandDuplicatePolicy.Append,
                Priority = 10,
            }
        );
        dispatcher.Register(
            "cmd_save",
            (_, _) =>
            {
                calls.Add("high-second");
                return ValueTask.FromResult(CommandResult.NotHandled);
            },
            options: new CommandRegistrationOptions
            {
                DuplicatePolicy = CommandDuplicatePolicy.Append,
                Priority = 10,
            }
        );

        var result = await dispatcher.ExecuteAsync("cmd_save");

        Assert.True(result.IsHandled);
        Assert.Equal(["high-first", "high-second", "low"], calls);
    }

    [Fact]
    public async Task Replace_InvalidatesOldLease()
    {
        var dispatcher = new CommandDispatcher();
        var original = dispatcher.Register(
            "cmd_save",
            (_, _) => ValueTask.FromResult(CommandResult.NotHandled)
        );
        var replacement = dispatcher.Register(
            "cmd_save",
            (_, _) => ValueTask.FromResult(CommandResult.Handled),
            options: new CommandRegistrationOptions
            {
                DuplicatePolicy = CommandDuplicatePolicy.Replace,
            }
        );

        original.Dispose();

        Assert.False(original.IsRegistered);
        Assert.True(replacement.IsRegistered);
        Assert.True((await dispatcher.ExecuteAsync("cmd_save")).IsHandled);
    }

    [Fact]
    public async Task DisabledAndCanceled_AreDistinctResults()
    {
        var dispatcher = new CommandDispatcher();
        dispatcher.Register(
            "cmd_save",
            (_, _) => ValueTask.FromResult(CommandResult.Handled),
            _ => false
        );
        using var cancellationSource = new CancellationTokenSource();
        cancellationSource.Cancel();

        var disabled = await dispatcher.ExecuteAsync("cmd_save");
        var canceled = await dispatcher.ExecuteAsync(
            "cmd_save",
            cancellationToken: cancellationSource.Token
        );

        Assert.Equal(CommandExecutionStatus.Disabled, disabled.Status);
        Assert.Equal(CommandExecutionStatus.Canceled, canceled.Status);
    }

    [Fact]
    public async Task HandlerFailure_IsCaptured()
    {
        var dispatcher = new CommandDispatcher();
        var failure = new InvalidOperationException("failed");
        dispatcher.Register("cmd_save", (_, _) => throw failure);

        var result = await dispatcher.ExecuteAsync("cmd_save");

        Assert.Equal(CommandExecutionStatus.Failed, result.Status);
        Assert.Same(failure, result.Exception);
    }

    [Fact]
    public void RegistrationLease_IsIdempotentAndPublishesImmutableSnapshot()
    {
        var dispatcher = new CommandDispatcher();
        var versions = new List<long>();
        dispatcher.Changed += (_, change) => versions.Add(change.Version);
        var lease = dispatcher.Register(
            "cmd_save",
            (_, _) => ValueTask.FromResult(CommandResult.Handled)
        );
        var registeredSnapshot = dispatcher.Current;

        lease.Dispose();
        lease.Dispose();

        Assert.False(lease.IsRegistered);
        Assert.Single(registeredSnapshot);
        Assert.Empty(dispatcher.Current);
        Assert.Equal([1, 2], versions);
    }
}
