using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;
using Application = System.Windows.Application;
using Window = System.Windows.Window;

namespace ArkheideSystem.Flourish.Services;

internal sealed class FlourishToolTipService(FlourishShellOptions options) : IToolTipService
{
    private readonly object gate = new();
    private Window? owner;

    public FlourishToolTipSettings Current
    {
        get
        {
            lock (gate)
            {
                return CaptureSettings();
            }
        }
    }

    public event EventHandler<FlourishToolTipChangedEventArgs>? Changed;

    public void SetEnabled(bool enabled)
    {
        FlourishToolTipSettings previous;
        FlourishToolTipSettings current;
        lock (gate)
        {
            previous = CaptureSettings();
            if (previous.IsEnabled == enabled)
            {
                return;
            }

            options.IsTipsEnabled = enabled;
            current = CaptureSettings();
        }

        ApplyAndNotify(previous, current);
    }

    public void Configure(int initialShowDelayMilliseconds, double spawnableMargin = 5)
    {
        if (initialShowDelayMilliseconds < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(initialShowDelayMilliseconds),
                initialShowDelayMilliseconds,
                "Tooltip delay cannot be negative."
            );
        }

        if (
            double.IsNaN(spawnableMargin)
            || double.IsInfinity(spawnableMargin)
            || spawnableMargin < 0
        )
        {
            throw new ArgumentOutOfRangeException(
                nameof(spawnableMargin),
                spawnableMargin,
                "Tooltip margin must be a non-negative finite number."
            );
        }

        FlourishToolTipSettings previous;
        FlourishToolTipSettings current;
        lock (gate)
        {
            previous = CaptureSettings();
            options.IsTipsEnabled = true;
            options.Tips.InitialShowDelayMilliseconds = initialShowDelayMilliseconds;
            options.Tips.SpawnableMargin = spawnableMargin;
            current = CaptureSettings();
            if (previous == current)
            {
                return;
            }
        }

        ApplyAndNotify(previous, current);
    }

    internal void Attach(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);
        owner = window;
        ApplyToWindow(window, Current);
    }

    private void ApplyAndNotify(
        FlourishToolTipSettings previous,
        FlourishToolTipSettings current
    )
    {
        var attachedOwner = owner;
        if (attachedOwner is not null)
        {
            if (attachedOwner.Dispatcher.CheckAccess())
            {
                var effective = Current;
                ApplyToWindow(attachedOwner, effective);
                Changed?.Invoke(
                    this,
                    new FlourishToolTipChangedEventArgs(previous, effective)
                );
            }
            else
            {
                attachedOwner.Dispatcher.Invoke(() =>
                {
                    var effective = Current;
                    ApplyToWindow(attachedOwner, effective);
                    Changed?.Invoke(
                        this,
                        new FlourishToolTipChangedEventArgs(previous, effective)
                    );
                });
            }

            return;
        }

        Changed?.Invoke(this, new FlourishToolTipChangedEventArgs(previous, current));
    }

    private FlourishToolTipSettings CaptureSettings()
    {
        return new FlourishToolTipSettings(
            options.IsTipsEnabled,
            options.Tips.InitialShowDelayMilliseconds,
            options.Tips.SpawnableMargin
        );
    }

    private static void ApplyToWindow(Window window, FlourishToolTipSettings settings)
    {
        var delay = settings.IsEnabled
            ? settings.InitialShowDelayMilliseconds
            : int.MaxValue;
        var margin = settings.IsEnabled ? settings.SpawnableMargin : 0d;
        window.Resources["FlourishToolTipInitialShowDelay"] = delay;
        window.Resources["FlourishToolTipSpawnableMargin"] = margin;

        var application = Application.Current;
        if (application is null)
        {
            return;
        }

        application.Resources["FlourishToolTipInitialShowDelay"] = delay;
        application.Resources["FlourishToolTipSpawnableMargin"] = margin;
    }
}
