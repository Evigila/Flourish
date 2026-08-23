using ArkheideSystem.Flourish.Services;

namespace ArkheideSystem.Flourish.Test.Services;

public sealed class CommandDispatcherTests
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CanExecute_WithMissingCommandKey_ReturnsFalse(string? commandKey)
    {
        var sut = new CommandDispatcher();

        Assert.False(sut.CanExecute(commandKey!));
    }

    [Fact]
    public async Task ExecuteAsync_WithNoRegistration_ReturnsNotHandled()
    {
        var sut = new CommandDispatcher();

        var result = await sut.ExecuteAsync("cmd_gallery_open");

        Assert.Equal(CommandExecutionStatus.NotHandled, result.Status);
        Assert.False(sut.CanExecute("cmd_gallery_open"));
    }

    [Fact]
    public async Task ExecuteAsync_RuntimeHandlerReceivesInvocationContextAndValue()
    {
        var sut = new CommandDispatcher();
        CommandContext? receivedContext = null;
        CancellationToken receivedToken = default;
        var parameter = new object();
        sut.Register(
            "cmd_editor_save",
            (context, cancellationToken) =>
            {
                receivedContext = context;
                receivedToken = cancellationToken;
                return ValueTask.FromResult(CommandResult.HandledWith("saved"));
            }
        );
        using var cancellationSource = new CancellationTokenSource();

        var result = await sut.ExecuteAsync(
            "cmd_editor_save",
            parameter,
            CommandSource.Toolbar,
            cancellationSource.Token
        );

        Assert.Equal(CommandExecutionStatus.Handled, result.Status);
        Assert.Equal("saved", result.Value);
        Assert.NotNull(receivedContext);
        Assert.Equal("cmd_editor_save", receivedContext.CommandKey);
        Assert.Same(parameter, receivedContext.Parameter);
        Assert.Equal(CommandSource.Toolbar, receivedContext.Source);
        Assert.Equal(cancellationSource.Token, receivedToken);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPredicateIsFalse_ReturnsDisabledWithoutInvokingHandler()
    {
        var invoked = false;
        var sut = new CommandDispatcher();
        sut.Register(
            "cmd_editor_save",
            (_, _) =>
            {
                invoked = true;
                return ValueTask.FromResult(CommandResult.Handled);
            },
            _ => false
        );

        var result = await sut.ExecuteAsync("cmd_editor_save");

        Assert.Equal(CommandExecutionStatus.Disabled, result.Status);
        Assert.False(result.IsHandled);
        Assert.False(invoked);
        Assert.False(sut.CanExecute("cmd_editor_save"));
    }

    [Fact]
    public async Task ExecuteAsync_RuntimeNotHandled_ReturnsNotHandled()
    {
        var sut = new CommandDispatcher();
        sut.Register(
            "cmd_reports_export",
            (_, _) => ValueTask.FromResult(CommandResult.NotHandled)
        );

        var result = await sut.ExecuteAsync("cmd_reports_export");

        Assert.Equal(CommandExecutionStatus.NotHandled, result.Status);
    }

    [Fact]
    public async Task ExecuteAsync_HandlerException_IsCaptured()
    {
        var sut = new CommandDispatcher();
        var failure = new InvalidOperationException("save failed");
        sut.Register("cmd_editor_save", (_, _) => throw failure);

        var result = await sut.ExecuteAsync("cmd_editor_save");

        Assert.Equal(CommandExecutionStatus.Failed, result.Status);
        Assert.Same(failure, result.Exception);
    }

    [Fact]
    public async Task ExecuteAsync_WithCanceledToken_ReturnsCanceledWithoutCallingHandlers()
    {
        var invoked = false;
        var sut = new CommandDispatcher();
        sut.Register(
            "cmd_editor_save",
            (_, _) =>
            {
                invoked = true;
                return ValueTask.FromResult(CommandResult.Handled);
            }
        );
        using var cancellationSource = new CancellationTokenSource();
        cancellationSource.Cancel();

        var result = await sut.ExecuteAsync(
            "cmd_editor_save",
            cancellationToken: cancellationSource.Token
        );

        Assert.Equal(CommandExecutionStatus.Canceled, result.Status);
        Assert.False(invoked);
    }

    [Fact]
    public void Register_WithDuplicateKeyAndRejectPolicy_Throws()
    {
        var sut = new CommandDispatcher();
        sut.Register("cmd_editor_save", (_, _) => ValueTask.FromResult(CommandResult.Handled));

        var exception = Assert.Throws<InvalidOperationException>(() =>
            sut.Register("cmd_editor_save", (_, _) => ValueTask.FromResult(CommandResult.Handled))
        );

        Assert.Contains("cmd_editor_save", exception.Message);
        Assert.Single(sut.Registrations);
    }

    [Fact]
    public async Task Register_WithReplacePolicy_InvalidatesOldLeaseAndUsesReplacement()
    {
        var calls = new List<string>();
        var sut = new CommandDispatcher();
        var original = sut.Register(
            "cmd_editor_save",
            (_, _) =>
            {
                calls.Add("original");
                return ValueTask.FromResult(CommandResult.Handled);
            }
        );
        var replacement = sut.Register(
            "cmd_editor_save",
            (_, _) =>
            {
                calls.Add("replacement");
                return ValueTask.FromResult(CommandResult.Handled);
            },
            options: new CommandRegistrationOptions
            {
                DuplicatePolicy = CommandDuplicatePolicy.Replace,
            }
        );

        Assert.False(original.IsRegistered);
        Assert.True(replacement.IsRegistered);
        original.Dispose();
        var result = await sut.ExecuteAsync("cmd_editor_save");

        Assert.True(result.IsHandled);
        Assert.Equal(["replacement"], calls);
    }

    [Fact]
    public async Task Register_WithAppendPolicy_UsesPriorityThenSequenceAndNotHandledChain()
    {
        var calls = new List<string>();
        var sut = new CommandDispatcher();
        sut.Register(
            "cmd_editor_save",
            (_, _) =>
            {
                calls.Add("low");
                return ValueTask.FromResult(CommandResult.Handled);
            },
            options: new CommandRegistrationOptions { Priority = -1 }
        );
        sut.Register(
            "cmd_editor_save",
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
        sut.Register(
            "cmd_editor_save",
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

        var result = await sut.ExecuteAsync("cmd_editor_save");

        Assert.True(result.IsHandled);
        Assert.Equal(["high-first", "high-second", "low"], calls);
    }

    [Fact]
    public async Task ExecuteAsync_UnregisteredEntryCapturedByCurrentDispatchIsSkipped()
    {
        var sut = new CommandDispatcher();
        var firstStarted = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        var releaseFirst = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        var laterInvoked = false;
        sut.Register(
            "cmd_editor_save",
            async (_, _) =>
            {
                firstStarted.TrySetResult();
                await releaseFirst.Task;
                return CommandResult.NotHandled;
            },
            options: new CommandRegistrationOptions { Priority = 10 }
        );
        var later = sut.Register(
            "cmd_editor_save",
            (_, _) =>
            {
                laterInvoked = true;
                return ValueTask.FromResult(CommandResult.Handled);
            },
            options: new CommandRegistrationOptions
            {
                DuplicatePolicy = CommandDuplicatePolicy.Append,
            }
        );
        var execution = sut.ExecuteAsync("cmd_editor_save").AsTask();
        await firstStarted.Task.WaitAsync(Timeout);

        later.Dispose();
        releaseFirst.TrySetResult();
        var result = await execution.WaitAsync(Timeout);

        Assert.Equal(CommandExecutionStatus.NotHandled, result.Status);
        Assert.False(laterInvoked);
    }

    [Fact]
    public async Task ExecuteAsync_ReplaceInvalidatesLaterCapturedEntriesUntilNextDispatch()
    {
        var sut = new CommandDispatcher();
        var firstStarted = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        var releaseFirst = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        var replacedInvoked = false;
        var replacementInvocations = 0;
        sut.Register(
            "cmd_editor_save",
            async (_, _) =>
            {
                firstStarted.TrySetResult();
                await releaseFirst.Task;
                return CommandResult.NotHandled;
            },
            options: new CommandRegistrationOptions { Priority = 10 }
        );
        sut.Register(
            "cmd_editor_save",
            (_, _) =>
            {
                replacedInvoked = true;
                return ValueTask.FromResult(CommandResult.Handled);
            },
            options: new CommandRegistrationOptions
            {
                DuplicatePolicy = CommandDuplicatePolicy.Append,
            }
        );
        var execution = sut.ExecuteAsync("cmd_editor_save").AsTask();
        await firstStarted.Task.WaitAsync(Timeout);
        sut.Register(
            "cmd_editor_save",
            (_, _) =>
            {
                replacementInvocations++;
                return ValueTask.FromResult(CommandResult.Handled);
            },
            options: new CommandRegistrationOptions
            {
                DuplicatePolicy = CommandDuplicatePolicy.Replace,
            }
        );

        releaseFirst.TrySetResult();
        var firstResult = await execution.WaitAsync(Timeout);
        var secondResult = await sut.ExecuteAsync("cmd_editor_save");

        Assert.Equal(CommandExecutionStatus.NotHandled, firstResult.Status);
        Assert.False(replacedInvoked);
        Assert.True(secondResult.IsHandled);
        Assert.Equal(1, replacementInvocations);
    }

    [Fact]
    public async Task ExecuteAsync_ReentrantRegistrationIsVisibleOnlyToNextDispatch()
    {
        var sut = new CommandDispatcher();
        var calls = new List<string>();
        ICommandRegistration? added = null;
        sut.Register(
            "cmd_editor_save",
            (_, _) =>
            {
                calls.Add("original");
                added ??= sut.Register(
                    "cmd_editor_save",
                    (_, _) =>
                    {
                        calls.Add("added");
                        return ValueTask.FromResult(CommandResult.Handled);
                    },
                    options: new CommandRegistrationOptions
                    {
                        DuplicatePolicy = CommandDuplicatePolicy.Append,
                        Priority = 10,
                    }
                );
                return ValueTask.FromResult(CommandResult.NotHandled);
            }
        );

        var firstResult = await sut.ExecuteAsync("cmd_editor_save");
        var secondResult = await sut.ExecuteAsync("cmd_editor_save");

        Assert.Equal(CommandExecutionStatus.NotHandled, firstResult.Status);
        Assert.True(secondResult.IsHandled);
        Assert.Equal(["original", "added"], calls);
    }

    [Fact]
    public void Changed_ReentrantRegistrationPublishesCacheBeforeCallbackReturns()
    {
        var sut = new CommandDispatcher();
        var changes = new List<CommandRegistryChangedEventArgs>();
        ICommandRegistration? added = null;
        var reentrantRegistrationIsVisible = false;
        sut.Changed += (_, args) =>
        {
            changes.Add(args);
            if (args.Version != 1)
            {
                return;
            }

            added = sut.Register(
                "cmd_editor_save",
                (_, _) => ValueTask.FromResult(CommandResult.Handled),
                _ => true,
                new CommandRegistrationOptions
                {
                    DuplicatePolicy = CommandDuplicatePolicy.Append,
                    Priority = 10,
                }
            );
            reentrantRegistrationIsVisible = sut.CanExecute("cmd_editor_save");
        };

        var original = sut.Register(
            "cmd_editor_save",
            (_, _) => ValueTask.FromResult(CommandResult.NotHandled),
            _ => false
        );

        Assert.True(reentrantRegistrationIsVisible);
        Assert.NotNull(added);
        Assert.Equal([1, 2], changes.Select(change => change.Version));
        Assert.Equal([original.Id], changes[0].Registrations.Select(entry => entry.Id));
        Assert.Equal([original.Id, added.Id], changes[1].Registrations.Select(entry => entry.Id));
    }

    [Fact]
    public async Task Dispose_UnregistersOnlyOwnedHandlerAndRaisesChangedSnapshot()
    {
        var sut = new CommandDispatcher();
        var changes = new List<CommandRegistryChangedEventArgs>();
        sut.Changed += (_, args) => changes.Add(args);
        var first = sut.Register(
            "cmd_editor_save",
            (_, _) => ValueTask.FromResult(CommandResult.NotHandled)
        );
        var second = sut.Register(
            "cmd_editor_save",
            (_, _) => ValueTask.FromResult(CommandResult.Handled),
            options: new CommandRegistrationOptions
            {
                DuplicatePolicy = CommandDuplicatePolicy.Append,
            }
        );

        first.Dispose();
        first.Dispose();

        Assert.False(first.IsRegistered);
        Assert.True(second.IsRegistered);
        Assert.Single(sut.Registrations);
        Assert.Equal(3, changes.Count);
        Assert.Equal(CommandRegistryChangeKind.Unregistered, changes[^1].ChangeKind);
        Assert.Single(changes[^1].Registrations);
        Assert.True((await sut.ExecuteAsync("cmd_editor_save")).IsHandled);
    }

    [Fact]
    public void NotifyCanExecuteChanged_FromLeaseAndRegistry_IdentifiesAffectedCommands()
    {
        var sut = new CommandDispatcher();
        var affectedKeys = new List<string?>();
        sut.CanExecuteChanged += (_, args) => affectedKeys.Add(args.CommandKey);
        var registration = sut.Register(
            "cmd_editor_save",
            (_, _) => ValueTask.FromResult(CommandResult.Handled)
        );

        registration.NotifyCanExecuteChanged();
        sut.NotifyCanExecuteChanged();
        registration.Dispose();
        registration.NotifyCanExecuteChanged();

        Assert.Equal(["cmd_editor_save", null], affectedKeys);
    }
}
