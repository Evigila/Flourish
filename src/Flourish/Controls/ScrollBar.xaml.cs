using ArkheideSystem.Flourish.Abstract;
using System.Windows;
using WpfScrollBar = System.Windows.Controls.Primitives.ScrollBar;

namespace ArkheideSystem.Flourish.Controls;

/// <summary>A Flourish-styled scroll bar.</summary>
public class ScrollBar : WpfScrollBar
{
    static ScrollBar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ScrollBar),
            new FrameworkPropertyMetadata(typeof(ScrollBar))
        );
    }
}
