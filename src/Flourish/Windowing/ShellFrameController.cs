using System;

using ArkheideSystem.Flourish.Abstract;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shell;

namespace ArkheideSystem.Flourish.Windowing;

internal enum ShellFrameMode
{
    Custom,
    Native,
}

internal sealed class ShellFrameController(Window window, Border shellBorder)
{
    public WindowChrome Chrome { get; } = new()
    {
        CaptionHeight = 0,
        CornerRadius = new CornerRadius(),
        GlassFrameThickness = new Thickness(),
        ResizeBorderThickness = new Thickness(6),
        UseAeroCaptionButtons = false,
    };

    public ShellFrameMode CurrentMode { get; private set; }

    public void Apply(ShellFrameMode mode)
    {
        switch (mode)
        {
            case ShellFrameMode.Custom:
                ApplyCustomFrame();
                break;
            case ShellFrameMode.Native:
                ApplyNativeFrame();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unknown frame mode.");
        }

        CurrentMode = mode;
    }

    public void UpdateWindowState()
    {
        if (CurrentMode == ShellFrameMode.Custom)
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
