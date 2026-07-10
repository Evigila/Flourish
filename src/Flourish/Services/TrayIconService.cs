using System.Drawing;
using System.Windows;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Configuration;
using Application = System.Windows.Application;
using Forms = System.Windows.Forms;

namespace ArkheideSystem.Flourish.Services;

internal sealed class TrayIconService(
    FlourishShellOptions options,
    FlourishLocalizationService localizationService
) : IDisposable
{
    private const string DefaultIconUri = "pack://application:,,,/Flourish;component/Assets/favicon.ico";

    private Forms.NotifyIcon? notifyIcon;
    private Icon? icon;
    private Window? owner;
    private bool isDisposed;

    public bool IsEnabled => options.IsTrayExitEnabled;

    public bool IsExitRequested { get; private set; }

    public void Initialize(Window owner, string tooltipText)
    {
        if (!IsEnabled || isDisposed)
        {
            return;
        }

        this.owner = owner;
        EnsureNotifyIcon(tooltipText);
    }

    public bool MinimizeToTray()
    {
        if (!IsEnabled || isDisposed || owner is null)
        {
            return false;
        }

        EnsureNotifyIcon(owner.Title);
        if (notifyIcon is null)
        {
            return false;
        }

        notifyIcon.Visible = true;
        owner.Hide();
        return true;
    }

    public void RestoreFromTray()
    {
        if (isDisposed || owner is null)
        {
            return;
        }

        owner.Show();
        if (owner.WindowState == WindowState.Minimized)
        {
            owner.WindowState = WindowState.Normal;
        }

        owner.Activate();
        if (notifyIcon is not null)
        {
            notifyIcon.Visible = false;
        }
    }

    public void ExitFromTray()
    {
        if (isDisposed)
        {
            return;
        }

        IsExitRequested = true;
        if (notifyIcon is not null)
        {
            notifyIcon.Visible = false;
        }

        owner?.Dispatcher.Invoke(() =>
        {
            if (owner is not null)
            {
                owner.Close();
                return;
            }

            Application.Current?.Shutdown();
        });
    }

    public void Dispose()
    {
        if (isDisposed)
        {
            return;
        }

        isDisposed = true;
        if (notifyIcon is not null)
        {
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
        }

        icon?.Dispose();
    }

    private void EnsureNotifyIcon(string tooltipText)
    {
        if (notifyIcon is not null)
        {
            notifyIcon.Text = CreateTooltipText(tooltipText);
            return;
        }

        icon = LoadDefaultIcon();
        notifyIcon = new Forms.NotifyIcon
        {
            Icon = icon,
            Text = CreateTooltipText(tooltipText),
            Visible = false,
            ContextMenuStrip = CreateContextMenu(),
        };
        notifyIcon.DoubleClick += (_, _) => owner?.Dispatcher.Invoke(RestoreFromTray);
    }

    private Forms.ContextMenuStrip CreateContextMenu()
    {
        var contextMenu = new Forms.ContextMenuStrip();
        contextMenu.Items.Add(
            localizationService.Get(FlourishLocaleKeys.TrayShow),
            null,
            (_, _) => owner?.Dispatcher.Invoke(RestoreFromTray)
        );
        contextMenu.Items.Add(
            localizationService.Get(FlourishLocaleKeys.TrayExit),
            null,
            (_, _) => ExitFromTray()
        );
        return contextMenu;
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
        return string.IsNullOrWhiteSpace(tooltipText)
            ? "Flourish"
            : tooltipText.Length > 63
                ? tooltipText[..63]
                : tooltipText;
    }
}
