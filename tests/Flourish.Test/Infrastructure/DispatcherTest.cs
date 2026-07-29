using System.Windows.Threading;

namespace ArkheideSystem.Flourish.Test.Infrastructure;

internal static class DispatcherTest
{
    public static void DrainApplicationIdle()
    {
        Dispatcher.CurrentDispatcher.Invoke(
            DispatcherPriority.ApplicationIdle,
            static () => { }
        );
    }

    public static void Wait(Dispatcher dispatcher, Task task)
    {
        var frame = new DispatcherFrame();
        _ = task.ContinueWith(
            _ =>
                dispatcher.BeginInvoke(
                    DispatcherPriority.Send,
                    new Action(() => frame.Continue = false)
                ),
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default
        );
        Dispatcher.PushFrame(frame);
        task.GetAwaiter().GetResult();
    }
}
