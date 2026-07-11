using System.Windows;
using System.Windows.Controls;
using System.Windows.Shell;

namespace ArkheideSystem.Flourish.Windows;

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

    private void ApplyCustomFrame()
    {
        window.WindowStyle = WindowStyle.None;
        if (!ReferenceEquals(WindowChrome.GetWindowChrome(window), Chrome))
        {
            WindowChrome.SetWindowChrome(window, Chrome);
        }

        shellBorder.BorderThickness = new Thickness(1);
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
}
