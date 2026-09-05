using System;
using System.Threading;
using System.Threading.Tasks;

using ArkheideSystem.Flourish.Abstract;
using Microsoft.Extensions.Hosting;

namespace ArkheideSystem.Flourish.Windowing;

internal sealed class WindowCloseOptionSynchronizer(
    WindowOptions options,
    WindowCloseService windowCloseService
) : IHostedService, IDisposable
{
    private int isSubscribed;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (Interlocked.Exchange(ref isSubscribed, 1) == 0)
        {
            windowCloseService.Changed += WindowCloseService_Changed;
        }

        Apply(windowCloseService.Current);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Dispose();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref isSubscribed, 0) != 0)
        {
            windowCloseService.Changed -= WindowCloseService_Changed;
        }
    }

    private void WindowCloseService_Changed(
        object? sender,
        StateChangedEventArgs<WindowCloseState> eventArgs
    ) => Apply(eventArgs.Current);

    private void Apply(WindowCloseState state)
    {
        options.IsTrayExitEnabled = state.Behavior == WindowCloseBehavior.MinimizeToTray;
    }
}
