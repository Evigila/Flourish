using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Services;

namespace ArkheideSystem.Flourish.Test.Services;

public sealed class CommandParserHostedServiceTests
{
    [Fact]
    public async Task StartAndStop_ManageParserMappingsAndDisposalOrder()
    {
        var dispatcher = new CommandDispatcher();
        var changes = new List<string>();
        var actionExecutions = 0;
        dispatcher.Changed += (_, change) =>
            changes.Add($"{change.ChangeKind}:{change.CommandKey}");
        var parsers = new ICommandParser[]
        {
            new DelegateParser(commands =>
                commands.Register("app.first", () => actionExecutions++)
            ),
            new DelegateParser(commands =>
                commands.Register(
                    "app.second",
                    static (_, _) => ValueTask.FromResult(CommandResult.Handled)
                )
            ),
        };
        using var sut = new CommandParserHostedService(dispatcher, parsers);

        await sut.StartAsync(CancellationToken.None);

        Assert.True(dispatcher.Contains("app.first"));
        Assert.True(dispatcher.Contains("app.second"));
        var result = await dispatcher.ExecuteAsync("app.first");
        Assert.Equal(CommandExecutionStatus.Handled, result.Status);
        Assert.Equal(1, actionExecutions);

        await sut.StopAsync(CancellationToken.None);

        Assert.False(dispatcher.Contains("app.first"));
        Assert.False(dispatcher.Contains("app.second"));
        Assert.Equal(
            [
                "Registered:app.first",
                "Registered:app.second",
                "Unregistered:app.second",
                "Unregistered:app.first",
            ],
            changes
        );
    }

    [Fact]
    public async Task Start_IsIdempotentAndCanRegisterAgainAfterStop()
    {
        var dispatcher = new CommandDispatcher();
        var parserCalls = 0;
        var parser = new DelegateParser(commands =>
        {
            parserCalls++;
            commands.Register("app.restart", static () => { });
        });
        using var sut = new CommandParserHostedService(dispatcher, [parser]);

        await sut.StartAsync(CancellationToken.None);
        await sut.StartAsync(CancellationToken.None);

        Assert.Equal(1, parserCalls);
        await sut.StopAsync(CancellationToken.None);

        await sut.StartAsync(CancellationToken.None);

        Assert.Equal(2, parserCalls);
        Assert.True(dispatcher.Contains("app.restart"));
    }

    [Fact]
    public void Start_WhenParserFails_RollsBackEveryRegistration()
    {
        var dispatcher = new CommandDispatcher();
        var failure = new InvalidOperationException("Parser failed.");
        var parsers = new ICommandParser[]
        {
            new DelegateParser(commands =>
                commands.Register("app.first", static () => { })
            ),
            new DelegateParser(commands =>
            {
                commands.Register("app.second", static () => { });
                throw failure;
            }),
        };
        using var sut = new CommandParserHostedService(dispatcher, parsers);

        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            _ = sut.StartAsync(CancellationToken.None);
        });

        Assert.Same(failure, exception);
        Assert.False(dispatcher.Contains("app.first"));
        Assert.False(dispatcher.Contains("app.second"));
    }

    [Fact]
    public async Task Start_ClosesRegistrarAfterParserReturns()
    {
        var dispatcher = new CommandDispatcher();
        ICommandRegistrar? captured = null;
        var parser = new DelegateParser(commands => captured = commands);
        using var sut = new CommandParserHostedService(dispatcher, [parser]);

        await sut.StartAsync(CancellationToken.None);

        Assert.NotNull(captured);
        Assert.Throws<ObjectDisposedException>(() =>
            captured.Register("app.late", static () => { })
        );
        Assert.False(dispatcher.Contains("app.late"));
    }

    [Fact]
    public void Start_WithCanceledToken_DoesNotInvokeParsers()
    {
        var dispatcher = new CommandDispatcher();
        var parserCalls = 0;
        var parser = new DelegateParser(_ => parserCalls++);
        using var cancellationSource = new CancellationTokenSource();
        cancellationSource.Cancel();
        using var sut = new CommandParserHostedService(dispatcher, [parser]);

        Assert.Throws<OperationCanceledException>(() =>
        {
            _ = sut.StartAsync(cancellationSource.Token);
        });

        Assert.Equal(0, parserCalls);
        Assert.Empty(dispatcher.Registrations);
    }

    private sealed class DelegateParser(Action<ICommandRegistrar> registerCommands)
        : ICommandParser
    {
        public void RegisterCommands(ICommandRegistrar commands)
        {
            registerCommands(commands);
        }
    }
}
