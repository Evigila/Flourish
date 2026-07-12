using System.Windows;
using WpfScrollBar = System.Windows.Controls.Primitives.ScrollBar;

namespace ArkheideSystem.Flourish.Controls;

/// <summary>A Flourish-styled scroll bar.</summary>
public class FlourishScrollBar : WpfScrollBar
{
    static FlourishScrollBar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(FlourishScrollBar),
            new FrameworkPropertyMetadata(typeof(FlourishScrollBar))
        );
    }
}
