using System.Windows;
using System.Windows.Controls;
using System.Windows.Shell;

namespace ArkheideSystem.Flourish.Views.Windows;

internal enum FlourishShellWindowFrameMode
{
    Custom,
    Native,
}

internal sealed class FlourishShellWindowFrame(Window window, Border shellBorder)
{
    public WindowChrome Chrome { get; } = new()
    {
        CaptionHeight = 0,
        CornerRadius = new CornerRadius(),
        GlassFrameThickness = new Thickness(),
        ResizeBorderThickness = new Thickness(6),
        UseAeroCaptionButtons = false,
    };

    public FlourishShellWindowFrameMode CurrentMode { get; private set; }

    public void Apply(FlourishShellWindowFrameMode mode)
    {
        switch (mode)
        {
            case FlourishShellWindowFrameMode.Custom:
                ApplyCustomFrame();
                break;
            case FlourishShellWindowFrameMode.Native:
                ApplyNativeFrame();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unknown frame mode.");
        }

        CurrentMode = mode;
    }

    public void UpdateWindowState()
    {
        if (CurrentMode == FlourishShellWindowFrameMode.Custom)
        {
            UpdateCustomFrameMetrics();
        }
    }

    private void ApplyCustomFrame()
    {
        window.WindowStyle = WindowStyle.None;
        if (!ReferenceEquals(WindowChrome.GetWindowChrome(window), Chrome))
        {
            WindowChrome.SetWindowChrome(window, Chrome);
        }

        UpdateCustomFrameMetrics();
    }

    private void ApplyNativeFrame()
    {
        if (WindowChrome.GetWindowChrome(window) is not null)
        {
            WindowChrome.SetWindowChrome(window, null);
        }

        window.WindowStyle = WindowStyle.SingleBorderWindow;
        shellBorder.BorderThickness = new Thickness();
    }

    private void UpdateCustomFrameMetrics()
    {
        var isMaximized = window.WindowState == WindowState.Maximized;
        shellBorder.BorderThickness = isMaximized ? new Thickness() : new Thickness(1);
        Chrome.ResizeBorderThickness = isMaximized ? new Thickness() : new Thickness(6);
    }
}
