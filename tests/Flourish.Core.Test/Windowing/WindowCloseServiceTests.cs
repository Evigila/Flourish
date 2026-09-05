using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Windowing;

using Xunit;

namespace ArkheideSystem.Flourish.Core.Test.Windowing;

public sealed class WindowCloseServiceTests
{
    [Fact]
    public async Task Guards_RunByOrderThenOrdinalIdAndStopOnCancel()
    {
        var services = new ServiceProviderStub();
        var sut = new WindowCloseService(services);
        var calls = new List<string>();
        using var later = sut.RegisterGuard("later", Guard("later"), order: 10);
        using var beta = sut.RegisterGuard("beta", Guard("beta"), order: 0);
        using var alpha = sut.RegisterGuard(
            "alpha",
            Guard("alpha", WindowCloseDecision.Cancel),
            order: 0
        );
        using var never = sut.RegisterGuard("never", Guard("never"), order: 20);

        Assert.False(await sut.CanCloseAsync(WindowCloseRequestReason.Application));
        Assert.Equal(["alpha"], calls);

        alpha.Dispose();
        calls.Clear();
        Assert.True(await sut.CanCloseAsync(WindowCloseRequestReason.Window));
        Assert.Equal(["beta", "later", "never"], calls);

        Func<WindowCloseContext, CancellationToken, ValueTask<WindowCloseDecision>> Guard(
            string id,
            WindowCloseDecision decision = WindowCloseDecision.Allow
        ) =>
            (context, token) =>
            {
                Assert.Same(services, context.Services);
                Assert.False(token.IsCancellationRequested);
                calls.Add(id);
                return ValueTask.FromResult(decision);
            };
    }

    [Fact]
    public async Task RequestClose_RunsGuardsBeforeAttachedPlatformAndHonorsDetach()
    {
        var sut = CreateService();
        var calls = new List<string>();
        using var guard = sut.RegisterGuard(
            "guard",
            (context, token) =>
            {
                Assert.Equal(WindowCloseRequestReason.Tray, context.Reason);
                Assert.False(token.IsCancellationRequested);
                calls.Add("guard");
                return ValueTask.FromResult(WindowCloseDecision.Allow);
            }
        );
        sut.Attach(
            (reason, token) =>
            {
                Assert.Equal(WindowCloseRequestReason.Tray, reason);
                Assert.False(token.IsCancellationRequested);
                calls.Add("platform");
                return ValueTask.FromResult(true);
            }
        );

        Assert.True(await sut.RequestCloseAsync(WindowCloseRequestReason.Tray));
        Assert.Equal(["guard", "platform"], calls);

        sut.Detach();
        calls.Clear();
        Assert.False(await sut.RequestCloseAsync(WindowCloseRequestReason.Tray));
        Assert.Equal(["guard"], calls);
    }

    [Fact]
    public async Task RequestClose_DoesNotInvokePlatformAfterGuardVeto()
    {
        var sut = CreateService();
        var platformCalls = 0;
        sut.Attach((_, _) =>
        {
            platformCalls++;
            return ValueTask.FromResult(true);
        });
        using var guard = sut.RegisterGuard(
            "veto",
            (_, _) => ValueTask.FromResult(WindowCloseDecision.Cancel)
        );

        Assert.False(await sut.RequestCloseAsync(WindowCloseRequestReason.TitleBar));
        Assert.Equal(0, platformCalls);
    }

    [Fact]
    public async Task RegistrationAndEnumValidation_RejectInvalidInput()
    {
        var sut = CreateService();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            sut.SetBehavior((WindowCloseBehavior)int.MaxValue)
        );
        Assert.Throws<ArgumentException>(() =>
            sut.RegisterGuard(" ", (_, _) => ValueTask.FromResult(WindowCloseDecision.Allow))
        );
        Assert.Throws<ArgumentNullException>(() => sut.RegisterGuard("guard", null!));
        using var registered = sut.RegisterGuard(
            " guard ",
            (_, _) => ValueTask.FromResult(WindowCloseDecision.Allow)
        );
        Assert.Throws<InvalidOperationException>(() =>
            sut.RegisterGuard(
                "guard",
                (_, _) => ValueTask.FromResult(WindowCloseDecision.Allow)
            )
        );
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            await sut.CanCloseAsync((WindowCloseRequestReason)int.MaxValue)
        );
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            await sut.RequestCloseAsync((WindowCloseRequestReason)int.MaxValue)
        );

        registered.Dispose();
        using var invalidDecision = sut.RegisterGuard(
            "invalid",
            (_, _) => ValueTask.FromResult((WindowCloseDecision)int.MaxValue)
        );
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await sut.CanCloseAsync(WindowCloseRequestReason.Application)
        );
    }

    [Fact]
    public void BehaviorState_IsIdempotentAndReentrantChangesRemainOrdered()
    {
        var sut = new WindowCloseService(
            new ServiceProviderStub(),
            WindowCloseBehavior.Close
        );
        var observed = new List<WindowCloseBehavior>();
        sut.Changed += (_, args) =>
        {
            observed.Add(args.Current.Behavior);
            if (args.Current.Behavior == WindowCloseBehavior.MinimizeToTray)
            {
                sut.SetBehavior(WindowCloseBehavior.Prompt);
            }
        };

        Assert.Equal(WindowCloseBehavior.Close, sut.Current.Behavior);
        sut.SetBehavior(WindowCloseBehavior.MinimizeToTray);
        sut.SetBehavior(WindowCloseBehavior.Prompt);

        Assert.Equal(WindowCloseBehavior.Prompt, sut.Current.Behavior);
        Assert.Equal(
            [WindowCloseBehavior.MinimizeToTray, WindowCloseBehavior.Prompt],
            observed
        );
    }

    [Fact]
    public async Task CancellationDuringGuard_IsObservedBeforeEvaluationCompletes()
    {
        var sut = CreateService();
        var entered = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        var release = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        using var guard = sut.RegisterGuard(
            "async",
            async (_, _) =>
            {
                entered.SetResult();
                await release.Task;
                return WindowCloseDecision.Allow;
            }
        );
        using var cancellation = new CancellationTokenSource();

        var evaluation = sut.CanCloseAsync(
            WindowCloseRequestReason.Application,
            cancellation.Token
        ).AsTask();
        await entered.Task.WaitAsync(TimeSpan.FromSeconds(2));
        cancellation.Cancel();
        release.SetResult();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => evaluation);
    }

    [Fact]
    public async Task Evaluation_UsesStableGuardSnapshot()
    {
        var sut = CreateService();
        var entered = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        var release = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        var calls = new List<string>();
        using var first = sut.RegisterGuard(
            "first",
            async (_, _) =>
            {
                calls.Add("first");
                entered.SetResult();
                await release.Task;
                return WindowCloseDecision.Allow;
            }
        );

        var evaluation = sut.CanCloseAsync(WindowCloseRequestReason.Application).AsTask();
        await entered.Task.WaitAsync(TimeSpan.FromSeconds(2));
        first.Dispose();
        using var later = sut.RegisterGuard(
            "later",
            (_, _) =>
            {
                calls.Add("later");
                return ValueTask.FromResult(WindowCloseDecision.Allow);
            }
        );
        release.SetResult();

        Assert.True(await evaluation);
        Assert.Equal(["first"], calls);

        calls.Clear();
        Assert.True(await sut.CanCloseAsync(WindowCloseRequestReason.Application));
        Assert.Equal(["later"], calls);
    }

    [Fact]
    public async Task ConcurrentRegistrationAndDisposal_LeavesConsistentGuardSet()
    {
        var sut = CreateService();
        var calls = new ConcurrentDictionary<string, int>(StringComparer.Ordinal);
        var registrations = await Task.WhenAll(
            Enumerable.Range(0, 64)
                .Select(index =>
                    Task.Run(() =>
                        sut.RegisterGuard(
                            $"guard-{index:D2}",
                            (_, _) =>
                            {
                                calls.AddOrUpdate(
                                    $"guard-{index:D2}",
                                    1,
                                    (_, count) => count + 1
                                );
                                return ValueTask.FromResult(WindowCloseDecision.Allow);
                            },
                            order: index % 4
                        )
                    )
                )
        );

        Assert.True(await sut.CanCloseAsync(WindowCloseRequestReason.Application));
        Assert.Equal(64, calls.Count);
        Assert.All(calls.Values, count => Assert.Equal(1, count));

        await Task.WhenAll(
            registrations.Select(registration =>
                Task.Run(registration.Dispose)
            )
        );
        Assert.All(registrations, registration => Assert.False(registration.IsRegistered));
        calls.Clear();
        Assert.True(await sut.CanCloseAsync(WindowCloseRequestReason.Application));
        Assert.Empty(calls);
    }

    [Fact]
    public async Task PreCanceledRequests_DoNotEvaluateGuardsOrPlatform()
    {
        var sut = CreateService();
        var calls = 0;
        using var guard = sut.RegisterGuard(
            "guard",
            (_, _) =>
            {
                calls++;
                return ValueTask.FromResult(WindowCloseDecision.Allow);
            }
        );
        sut.Attach((_, _) =>
        {
            calls++;
            return ValueTask.FromResult(true);
        });
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await sut.RequestCloseAsync(
                WindowCloseRequestReason.Application,
                cancellation.Token
            )
        );
        Assert.Equal(0, calls);
    }

    private static WindowCloseService CreateService() =>
        new(new ServiceProviderStub());

    private sealed class ServiceProviderStub : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }
}
