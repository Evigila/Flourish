using System.Windows;

namespace ArkheideSystem.Flourish.Controls;

/// <summary>A native-sized window caption command button.</summary>
public class WindowCaptionButton : Button
{
    static WindowCaptionButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(WindowCaptionButton),
            new FrameworkPropertyMetadata(typeof(WindowCaptionButton))
        );
    }
}
