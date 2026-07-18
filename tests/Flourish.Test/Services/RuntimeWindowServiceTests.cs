using System.Windows;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;
using ArkheideSystem.Flourish.Services;
using Moq;

namespace ArkheideSystem.Flourish.Test.Services;

public sealed class RuntimeWindowServiceTests
{
    [Fact]
    public void WindowService_UpdatesUnattachedStateAndRaisesSnapshots()
    {
        var options = new FlourishShellOptions();
        IWindowService sut = new WindowService(options);
        var states = new List<FlourishWindowState>();
        sut.StateChanged += (_, args) => states.Add(args.State);

        sut.SetBounds(new Rect(40, 60, 1000, 700));
        sut.SetMinimumSize(400, 300);
        sut.SetMaximumSize(1600, 1200);
        sut.SetResizeMode(ResizeMode.CanMinimize);
        sut.SetTopmost(true);
        sut.SetShownInTaskbar(false);
        sut.Maximize();

        Assert.Equal(new Rect(40, 60, 1000, 700), sut.Current.Bounds);
        Assert.Equal(new Size(400, 300), sut.Current.MinimumSize);
        Assert.Equal(new Size(1600, 1200), sut.Current.MaximumSize);
        Assert.Equal(WindowState.Maximized, sut.Current.WindowState);
        Assert.Equal(ResizeMode.CanMinimize, sut.Current.ResizeMode);
        Assert.True(sut.Current.IsTopmost);
        Assert.False(sut.Current.IsShownInTaskbar);
        Assert.Equal(7, states.Count);
    }

    [Fact]
    public void WindowService_DoesNotPublishIdenticalUnattachedSnapshots()
    {
        var sut = new WindowService(new FlourishShellOptions());
        var changes = 0;
        sut.StateChanged += (_, _) => changes++;

        sut.SetTopmost(true);
        sut.SetTopmost(true);
        sut.SetSize(900, 600);
        sut.SetSize(900, 600);

        Assert.Equal(2, changes);
    }

    [Fact]
    public void WindowService_RejectsInvalidGeometryAndReportsCorrectDimension()
    {
        var options = new FlourishShellOptions
        {
            WindowMaxWidth = 1000,
            WindowMaxHeight = 700,
            WindowMinWidth = 400,
            WindowMinHeight = 300,
        };
        var sut = new WindowService(options);

        Assert.Throws<ArgumentOutOfRangeException>(() => sut.SetSize(double.NaN, 100));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            sut.SetBounds(new Rect(0, 0, 0, 100))
        );
        var minimumHeightError = Assert.Throws<ArgumentOutOfRangeException>(() =>
            sut.SetMinimumSize(500, 701)
        );
        var maximumHeightError = Assert.Throws<ArgumentOutOfRangeException>(() =>
            sut.SetMaximumSize(800, 299)
        );

        Assert.Equal("height", minimumHeightError.ParamName);
        Assert.Equal("height", maximumHeightError.ParamName);
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            sut.SetResizeMode((ResizeMode)int.MaxValue)
        );
    }

    [Fact]
    public async Task WindowCloseService_OrdersGuardsStopsOnCancelAndHonorsLease()
    {
        var sut = CreateCloseService();
        var calls = new List<string>();
        using var late = sut.RegisterGuard("late", Guard("late", WindowCloseDecision.Allow), 10);
        using var cancel = sut.RegisterGuard("cancel", Guard("cancel", WindowCloseDecision.Cancel), 0);
        using var never = sut.RegisterGuard("never", Guard("never", WindowCloseDecision.Allow), 20);

        Assert.False(await sut.CanCloseAsync(WindowCloseRequestReason.Application));
        Assert.Equal(["cancel"], calls);

        cancel.Dispose();
        calls.Clear();
        Assert.True(await sut.CanCloseAsync(WindowCloseRequestReason.Window));
        Assert.Equal(["late", "never"], calls);

        Func<WindowCloseContext, CancellationToken, ValueTask<WindowCloseDecision>> Guard(
            string id,
            WindowCloseDecision decision
        )
        {
            return (_, _) =>
            {
                calls.Add(id);
                return ValueTask.FromResult(decision);
            };
        }
    }

    [Fact]
    public async Task WindowCloseService_RequestRunsGuardsBeforeAttachedDelegate()
    {
        var sut = CreateCloseService();
        var delegateCalls = 0;
        sut.Attach((reason, _) =>
        {
            delegateCalls++;
            Assert.Equal(WindowCloseRequestReason.Tray, reason);
            return ValueTask.FromResult(true);
        });
        using var registration = sut.RegisterGuard(
            "veto",
            (_, _) => ValueTask.FromResult(WindowCloseDecision.Cancel)
        );

        Assert.False(await sut.RequestCloseAsync(WindowCloseRequestReason.Tray));
        Assert.Equal(0, delegateCalls);

        registration.Dispose();
        Assert.True(await sut.RequestCloseAsync(WindowCloseRequestReason.Tray));
        Assert.Equal(1, delegateCalls);
    }

    [Fact]
    public async Task WindowCloseService_RequestCloseRunsAllowedGuardsInOrderBeforeShell()
    {
        var services = new Mock<IServiceProvider>().Object;
        var sut = new WindowCloseService(new FlourishShellOptions(), services);
        var calls = new List<string>();
        using var cancellation = new CancellationTokenSource();
        using var later = sut.RegisterGuard(
            "later",
            (context, token) =>
            {
                Assert.Same(services, context.Services);
                Assert.Equal(WindowCloseRequestReason.Tray, context.Reason);
                Assert.Equal(cancellation.Token, token);
                calls.Add("later");
                return ValueTask.FromResult(WindowCloseDecision.Allow);
            },
            order: 10
        );
        using var earlier = sut.RegisterGuard(
            "earlier",
            (context, token) =>
            {
                Assert.Same(services, context.Services);
                Assert.Equal(WindowCloseRequestReason.Tray, context.Reason);
                Assert.Equal(cancellation.Token, token);
                calls.Add("earlier");
                return ValueTask.FromResult(WindowCloseDecision.Allow);
            },
            order: -10
        );
        sut.Attach((reason, token) =>
        {
            Assert.Equal(WindowCloseRequestReason.Tray, reason);
            Assert.Equal(cancellation.Token, token);
            calls.Add("shell");
            return ValueTask.FromResult(true);
        });

        var handled = await sut.RequestCloseAsync(
            WindowCloseRequestReason.Tray,
            cancellation.Token
        );

        Assert.True(handled);
        Assert.Equal(["earlier", "later", "shell"], calls);
    }

    [Fact]
    public async Task WindowCloseService_ValidatesCancellationDecisionsAndRegistrationIds()
    {
        var sut = CreateCloseService();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await sut.CanCloseAsync(
                WindowCloseRequestReason.Application,
                cancellation.Token
            )
        );
        Assert.Throws<ArgumentException>(() =>
            sut.RegisterGuard(" ", (_, _) => ValueTask.FromResult(WindowCloseDecision.Allow))
        );
        using var invalid = sut.RegisterGuard(
            "invalid",
            (_, _) => ValueTask.FromResult((WindowCloseDecision)int.MaxValue)
        );
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await sut.CanCloseAsync(WindowCloseRequestReason.Application)
        );
    }

    [Fact]
    public void WindowCloseService_UpdatesBehaviorAndTrayOption()
    {
        var options = new FlourishShellOptions();
        var sut = new WindowCloseService(options, new Mock<IServiceProvider>().Object);

        sut.SetBehavior(WindowCloseBehavior.MinimizeToTray);
        Assert.Equal(WindowCloseBehavior.MinimizeToTray, sut.Behavior);
        Assert.True(options.IsTrayExitEnabled);
        sut.SetBehavior(WindowCloseBehavior.Close);
        Assert.False(options.IsTrayExitEnabled);
    }

    private static WindowCloseService CreateCloseService() =>
        new(new FlourishShellOptions(), new Mock<IServiceProvider>().Object);
}
