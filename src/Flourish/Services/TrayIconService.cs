using System.Windows;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;
using Microsoft.Extensions.Logging;
using Application = System.Windows.Application;
using Forms = System.Windows.Forms;

namespace ArkheideSystem.Flourish.Services;

internal sealed class TrayIconService(
    FlourishShellOptions options,
    FlourishLocalizationService localizationService,
    WindowCloseService windowCloseService,
    ILogger<TrayIconService> logger
) : ITrayService, IDisposable
{
    private const string DefaultIconUri =
        "pack://application:,,,/Flourish;component/Assets/favicon.ico";

    private readonly object gate = new();
    private Forms.NotifyIcon? notifyIcon;
    private Icon? icon;
    private Window? owner;
    private bool isDisposed;
    private bool isExitRequested;
    private bool isIconVisible;
    private bool isWindowHidden;
    private bool isLocalizationSubscribed;
    private string toolTipText = "Flourish";

    public event EventHandler<FlourishTrayStateChangedEventArgs>? StateChanged;

    public bool IsEnabled
    {
        get
        {
            lock (gate)
            {
                return options.IsTrayExitEnabled;
            }
        }
    }

    public bool IsExitRequested
    {
        get
        {
            lock (gate)
            {
                return isExitRequested;
            }
        }
    }

    public FlourishTrayState Current
    {
        get
        {
            lock (gate)
            {
                return new FlourishTrayState(
                    options.IsTrayExitEnabled,
                    isIconVisible,
                    isWindowHidden,
                    isExitRequested,
                    toolTipText
                );
            }
        }
    }

    public void Initialize(Window owner, string tooltipText)
    {
        ArgumentNullException.ThrowIfNull(owner);
        if (!owner.Dispatcher.CheckAccess())
        {
            owner.Dispatcher.Invoke(() => Initialize(owner, tooltipText));
            return;
        }

        lock (gate)
        {
            if (isDisposed)
            {
                return;
            }

            if (!ReferenceEquals(this.owner, owner))
            {
                if (this.owner is not null)
                {
                    this.owner.IsVisibleChanged -= Owner_IsVisibleChanged;
                }

                this.owner = owner;
                this.owner.IsVisibleChanged += Owner_IsVisibleChanged;
            }

            isWindowHidden = !owner.IsVisible;
            toolTipText = CreateTooltipText(tooltipText);
            if (!isLocalizationSubscribed)
            {
                localizationService.Changed += LocalizationService_Changed;
                isLocalizationSubscribed = true;
            }

            if (options.IsTrayExitEnabled)
            {
                EnsureNotifyIcon(toolTipText);
            }
        }

        RaiseChanged();
    }

    public void SetEnabled(bool enabled)
    {
        InvokeOnOwner(() =>
        {
            var shouldRestore = false;
            var shouldResetCloseBehavior = false;
            lock (gate)
            {
                if (isDisposed || options.IsTrayExitEnabled == enabled)
                {
                    return;
                }

                options.IsTrayExitEnabled = enabled;
                if (enabled && owner is not null)
                {
                    EnsureNotifyIcon(toolTipText);
                }
                else if (!enabled)
                {
                    SetIconVisibleLocked(false);
                    shouldRestore = isWindowHidden;
                    shouldResetCloseBehavior =
                        windowCloseService.Behavior == WindowCloseBehavior.MinimizeToTray;
                }
            }

            if (shouldResetCloseBehavior)
            {
                windowCloseService.SetBehavior(WindowCloseBehavior.Prompt);
            }

            if (shouldRestore)
            {
                RestoreFromTrayCore();
            }

            RaiseChanged();
        });
    }

    public void SetToolTip(string text)
    {
        var normalized = CreateTooltipText(text);
        InvokeOnOwner(() =>
        {
            lock (gate)
            {
                if (isDisposed || string.Equals(toolTipText, normalized, StringComparison.Ordinal))
                {
                    return;
                }

                toolTipText = normalized;
                if (notifyIcon is not null)
                {
                    notifyIcon.Text = toolTipText;
                }
            }

            RaiseChanged();
        });
    }

    public bool MinimizeToTray()
    {
        return InvokeOnOwner(MinimizeToTrayCore);
    }

    private bool MinimizeToTrayCore()
    {
        Window? current;
        lock (gate)
        {
            if (!options.IsTrayExitEnabled || isDisposed || owner is null)
            {
                return false;
            }

            EnsureNotifyIcon(toolTipText);
            if (notifyIcon is null)
            {
                return false;
            }

            SetIconVisibleLocked(true);
            current = owner;
        }

        current.Hide();
        RaiseChanged();
        return true;
    }

    public void RestoreFromTray()
    {
        InvokeOnOwner(() =>
        {
            if (RestoreFromTrayCore())
            {
                RaiseChanged();
            }
        });
    }

    private bool RestoreFromTrayCore()
    {
        Window? current;
        lock (gate)
        {
            if (isDisposed || owner is null)
            {
                return false;
            }

            current = owner;
            SetIconVisibleLocked(false);
        }

        current.Show();
        if (current.WindowState == WindowState.Minimized)
        {
            current.WindowState = WindowState.Normal;
        }

        current.Activate();
        return true;
    }

    public void ExitFromTray()
    {
        InvokeOnOwner(() =>
        {
            lock (gate)
            {
                if (isDisposed || isExitRequested)
                {
                    return;
                }

                isExitRequested = true;
                SetIconVisibleLocked(false);
            }

            RaiseChanged();

            _ = RequestExitAsync();
        });
    }

    private async Task RequestExitAsync()
    {
        try
        {
            if (
                await windowCloseService.RequestCloseAsync(WindowCloseRequestReason.Tray)
            )
            {
                return;
            }

            ResetExitRequested();
        }
        catch (Exception error)
        {
            ResetExitRequested();
            logger.LogError(error, "The notification-area exit request failed.");
        }
    }

    private void ResetExitRequested()
    {
        InvokeOnOwner(() =>
        {
            lock (gate)
            {
                if (isDisposed || !isExitRequested)
                {
                    return;
                }

                isExitRequested = false;
            }

            RaiseChanged();
        });
    }

    public void Dispose()
    {
        InvokeOnOwner(() =>
        {
            Forms.NotifyIcon? currentNotifyIcon;
            Icon? currentIcon;
            Forms.ContextMenuStrip? contextMenu;
            lock (gate)
            {
                if (isDisposed)
                {
                    return;
                }

                isDisposed = true;
                if (owner is not null)
                {
                    owner.IsVisibleChanged -= Owner_IsVisibleChanged;
                }

                if (isLocalizationSubscribed)
                {
                    localizationService.Changed -= LocalizationService_Changed;
                    isLocalizationSubscribed = false;
                }

                SetIconVisibleLocked(false);
                currentNotifyIcon = notifyIcon;
                currentIcon = icon;
                contextMenu = notifyIcon?.ContextMenuStrip;
                notifyIcon = null;
                icon = null;
                owner = null;
                isWindowHidden = false;
            }

            currentNotifyIcon?.Dispose();
            contextMenu?.Dispose();
            currentIcon?.Dispose();
            RaiseChanged();
        });
    }

    void ITrayService.Restore() => RestoreFromTray();

    void ITrayService.Exit() => ExitFromTray();

    private void EnsureNotifyIcon(string tooltipText)
    {
        if (notifyIcon is not null)
        {
            toolTipText = CreateTooltipText(tooltipText);
            notifyIcon.Text = toolTipText;
            return;
        }

        toolTipText = CreateTooltipText(tooltipText);

        icon = LoadDefaultIcon();
        notifyIcon = new Forms.NotifyIcon
        {
            Icon = icon,
            Text = toolTipText,
            Visible = false,
            ContextMenuStrip = CreateContextMenu(),
        };
        isIconVisible = false;
        notifyIcon.DoubleClick += (_, _) => RestoreFromTray();
    }

    private Forms.ContextMenuStrip CreateContextMenu()
    {
        var contextMenu = new Forms.ContextMenuStrip();
        contextMenu.Items.Add(
            localizationService.Get(FlourishLocaleKeys.TrayShow),
            null,
            (_, _) => RestoreFromTray()
        );
        contextMenu.Items.Add(
            localizationService.Get(FlourishLocaleKeys.TrayExit),
            null,
            (_, _) => ExitFromTray()
        );
        return contextMenu;
    }

    private void Owner_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        lock (gate)
        {
            if (isDisposed || !ReferenceEquals(sender, owner) || owner is null)
            {
                return;
            }

            isWindowHidden = !owner.IsVisible;
        }

        RaiseChanged();
    }

    private void LocalizationService_Changed(
        object? sender,
        FlourishLocalizationChangedEventArgs e
    )
    {
        InvokeOnOwner(() =>
        {
            Forms.ContextMenuStrip? previousMenu;
            lock (gate)
            {
                if (isDisposed || notifyIcon is null)
                {
                    return;
                }

                previousMenu = notifyIcon.ContextMenuStrip;
                notifyIcon.ContextMenuStrip = CreateContextMenu();
            }

            previousMenu?.Dispose();
        });
    }

    private void SetIconVisibleLocked(bool visible)
    {
        isIconVisible = visible && notifyIcon is not null;
        if (notifyIcon is not null)
        {
            notifyIcon.Visible = isIconVisible;
        }
    }

    private void InvokeOnOwner(Action action)
    {
        Window? current;
        lock (gate)
        {
            current = owner;
        }

        if (current is null || current.Dispatcher.CheckAccess())
        {
            action();
            return;
        }

        current.Dispatcher.Invoke(action);
    }

    private T InvokeOnOwner<T>(Func<T> action)
    {
        Window? current;
        lock (gate)
        {
            current = owner;
        }

        return current is null || current.Dispatcher.CheckAccess()
            ? action()
            : current.Dispatcher.Invoke(action);
    }

    private static Icon LoadDefaultIcon()
    {
        var resourceInfo = Application.GetResourceStream(new Uri(DefaultIconUri, UriKind.Absolute));
        if (resourceInfo?.Stream is null)
        {
            return (Icon)SystemIcons.Application.Clone();
        }

        return new Icon(resourceInfo.Stream);
    }

    private static string CreateTooltipText(string tooltipText)
    {
        return string.IsNullOrWhiteSpace(tooltipText) ? "Flourish"
            : tooltipText.Length > 63 ? tooltipText[..63]
            : tooltipText;
    }

    private void RaiseChanged()
    {
        StateChanged?.Invoke(this, new FlourishTrayStateChangedEventArgs(Current));
    }
}
