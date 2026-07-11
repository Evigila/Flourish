using System.Windows;
using System.Windows.Controls;
using System.Windows.Shell;

namespace ArkheideSystem.Flourish.Windows;

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

    public void Apply()
    {
        window.WindowStyle = WindowStyle.None;
        if (!ReferenceEquals(WindowChrome.GetWindowChrome(window), Chrome))
        {
            WindowChrome.SetWindowChrome(window, Chrome);
        }

        shellBorder.BorderThickness = new Thickness(1);
    }
}
