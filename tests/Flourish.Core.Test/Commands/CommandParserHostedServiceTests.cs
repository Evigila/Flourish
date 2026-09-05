using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Commands;


namespace ArkheideSystem.Flourish.Core.Test.Commands;

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
                commands.Register("cmd_app_first", () => actionExecutions++)
            ),
            new DelegateParser(commands =>
                commands.Register(
                    "cmd_app_second",
                    static (_, _) => ValueTask.FromResult(CommandResult.Handled)
                )
            ),
        };
        using var sut = new CommandParserHostedService(dispatcher, parsers);

        await sut.StartAsync(CancellationToken.None);

        Assert.True(dispatcher.Contains("cmd_app_first"));
        Assert.True(dispatcher.Contains("cmd_app_second"));
        var result = await dispatcher.ExecuteAsync("cmd_app_first");
        Assert.Equal(CommandExecutionStatus.Handled, result.Status);
        Assert.Equal(1, actionExecutions);

        await sut.StopAsync(CancellationToken.None);

        Assert.False(dispatcher.Contains("cmd_app_first"));
        Assert.False(dispatcher.Contains("cmd_app_second"));
        Assert.Equal(
            [
                "Added:cmd_app_first",
                "Added:cmd_app_second",
                "Removed:cmd_app_second",
                "Removed:cmd_app_first",
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
            commands.Register("cmd_app_restart", static () => { });
        });
        using var sut = new CommandParserHostedService(dispatcher, [parser]);

        await sut.StartAsync(CancellationToken.None);
        await sut.StartAsync(CancellationToken.None);

        Assert.Equal(1, parserCalls);
        await sut.StopAsync(CancellationToken.None);

        await sut.StartAsync(CancellationToken.None);

        Assert.Equal(2, parserCalls);
        Assert.True(dispatcher.Contains("cmd_app_restart"));
    }

    [Fact]
    public void Start_WhenParserFails_RollsBackEveryRegistration()
    {
        var dispatcher = new CommandDispatcher();
        var failure = new InvalidOperationException("Parser failed.");
        var parsers = new ICommandParser[]
        {
            new DelegateParser(commands => commands.Register("cmd_app_first", static () => { })),
            new DelegateParser(commands =>
            {
                commands.Register("cmd_app_second", static () => { });
                throw failure;
            }),
        };
        using var sut = new CommandParserHostedService(dispatcher, parsers);

        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            _ = sut.StartAsync(CancellationToken.None);
        });

        Assert.Same(failure, exception);
        Assert.False(dispatcher.Contains("cmd_app_first"));
        Assert.False(dispatcher.Contains("cmd_app_second"));
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
            captured.Register("cmd_app_late", static () => { })
        );
        Assert.False(dispatcher.Contains("cmd_app_late"));
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
        Assert.Empty(dispatcher.Current);
    }

    private sealed class DelegateParser(Action<ICommandRegistrar> registerCommands) : ICommandParser
    {
        public void RegisterCommands(ICommandRegistrar commands)
        {
            registerCommands(commands);
        }
    }
}
