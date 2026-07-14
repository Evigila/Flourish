using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Services;

namespace ArkheideSystem.Flourish.Test.Services;

public sealed class CommandDispatcherTests
{
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

        var result = await sut.ExecuteAsync("gallery.open");

        Assert.Equal(CommandExecutionStatus.NotHandled, result.Status);
        Assert.False(sut.CanExecute("gallery.open"));
    }

    [Fact]
    public async Task ExecuteAsync_RuntimeHandlerReceivesInvocationContextAndValue()
    {
        var sut = new CommandDispatcher();
        CommandContext? receivedContext = null;
        CancellationToken receivedToken = default;
        var parameter = new object();
        sut.Register(
            "editor.save",
            (context, cancellationToken) =>
            {
                receivedContext = context;
                receivedToken = cancellationToken;
                return ValueTask.FromResult(CommandResult.HandledWith("saved"));
            }
        );
        using var cancellationSource = new CancellationTokenSource();

        var result = await sut.ExecuteAsync(
            "editor.save",
            parameter,
            CommandSource.Toolbar,
            cancellationSource.Token
        );

        Assert.Equal(CommandExecutionStatus.Handled, result.Status);
        Assert.Equal("saved", result.Value);
        Assert.NotNull(receivedContext);
        Assert.Equal("editor.save", receivedContext.CommandKey);
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
            "editor.save",
            (_, _) =>
            {
                invoked = true;
                return ValueTask.FromResult(CommandResult.Handled);
            },
            _ => false
        );

        var result = await sut.ExecuteAsync("editor.save");

        Assert.Equal(CommandExecutionStatus.Disabled, result.Status);
        Assert.False(result.IsHandled);
        Assert.False(invoked);
        Assert.False(sut.CanExecute("editor.save"));
    }

    [Fact]
    public async Task ExecuteAsync_RuntimeNotHandled_ReturnsNotHandled()
    {
        var sut = new CommandDispatcher();
        sut.Register(
            "reports.export",
            (_, _) => ValueTask.FromResult(CommandResult.NotHandled)
        );

        var result = await sut.ExecuteAsync("reports.export");

        Assert.Equal(CommandExecutionStatus.NotHandled, result.Status);
    }

    [Fact]
    public async Task ExecuteAsync_HandlerException_IsCaptured()
    {
        var sut = new CommandDispatcher();
        var failure = new InvalidOperationException("save failed");
        sut.Register("editor.save", (_, _) => throw failure);

        var result = await sut.ExecuteAsync("editor.save");

        Assert.Equal(CommandExecutionStatus.Failed, result.Status);
        Assert.Same(failure, result.Exception);
    }

    [Fact]
    public async Task ExecuteAsync_WithCanceledToken_ReturnsCanceledWithoutCallingHandlers()
    {
        var invoked = false;
        var sut = new CommandDispatcher();
        sut.Register(
            "editor.save",
            (_, _) =>
            {
                invoked = true;
                return ValueTask.FromResult(CommandResult.Handled);
            }
        );
        using var cancellationSource = new CancellationTokenSource();
        cancellationSource.Cancel();

        var result = await sut.ExecuteAsync(
            "editor.save",
            cancellationToken: cancellationSource.Token
        );

        Assert.Equal(CommandExecutionStatus.Canceled, result.Status);
        Assert.False(invoked);
    }

    [Fact]
    public void Register_WithDuplicateKeyAndRejectPolicy_Throws()
    {
        var sut = new CommandDispatcher();
        sut.Register(
            "editor.save",
            (_, _) => ValueTask.FromResult(CommandResult.Handled)
        );

        var exception = Assert.Throws<InvalidOperationException>(() =>
            sut.Register(
                "editor.save",
                (_, _) => ValueTask.FromResult(CommandResult.Handled)
            )
        );

        Assert.Contains("editor.save", exception.Message);
        Assert.Single(sut.Registrations);
    }

    [Fact]
    public async Task Register_WithReplacePolicy_InvalidatesOldLeaseAndUsesReplacement()
    {
        var calls = new List<string>();
        var sut = new CommandDispatcher();
        var original = sut.Register(
            "editor.save",
            (_, _) =>
            {
                calls.Add("original");
                return ValueTask.FromResult(CommandResult.Handled);
            }
        );
        var replacement = sut.Register(
            "editor.save",
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
        var result = await sut.ExecuteAsync("editor.save");

        Assert.True(result.IsHandled);
        Assert.Equal(["replacement"], calls);
    }

    [Fact]
    public async Task Register_WithAppendPolicy_UsesPriorityThenRegistrationOrder()
    {
        var calls = new List<string>();
        var sut = new CommandDispatcher();
        sut.Register(
            "editor.save",
            (_, _) =>
            {
                calls.Add("low");
                return ValueTask.FromResult(CommandResult.Handled);
            },
            options: new CommandRegistrationOptions { Priority = -1 }
        );
        sut.Register(
            "editor.save",
            (_, _) =>
            {
                calls.Add("high");
                return ValueTask.FromResult(CommandResult.Handled);
            },
            options: new CommandRegistrationOptions
            {
                DuplicatePolicy = CommandDuplicatePolicy.Append,
                Priority = 10,
            }
        );

        var result = await sut.ExecuteAsync("editor.save");

        Assert.True(result.IsHandled);
        Assert.Equal(["high"], calls);
    }

    [Fact]
    public async Task Dispose_UnregistersOnlyOwnedHandlerAndRaisesChangedSnapshot()
    {
        var sut = new CommandDispatcher();
        var changes = new List<CommandRegistryChangedEventArgs>();
        sut.Changed += (_, args) => changes.Add(args);
        var first = sut.Register(
            "editor.save",
            (_, _) => ValueTask.FromResult(CommandResult.NotHandled)
        );
        var second = sut.Register(
            "editor.save",
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
        Assert.True((await sut.ExecuteAsync("editor.save")).IsHandled);
    }

    [Fact]
    public void NotifyCanExecuteChanged_FromLeaseAndRegistry_IdentifiesAffectedCommands()
    {
        var sut = new CommandDispatcher();
        var affectedKeys = new List<string?>();
        sut.CanExecuteChanged += (_, args) => affectedKeys.Add(args.CommandKey);
        var registration = sut.Register(
            "editor.save",
            (_, _) => ValueTask.FromResult(CommandResult.Handled)
        );

        registration.NotifyCanExecuteChanged();
        sut.NotifyCanExecuteChanged();
        registration.Dispose();
        registration.NotifyCanExecuteChanged();

        Assert.Equal(["editor.save", null], affectedKeys);
    }
}
