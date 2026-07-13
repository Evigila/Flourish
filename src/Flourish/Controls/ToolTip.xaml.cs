using System.Windows;
using WpfToolTip = System.Windows.Controls.ToolTip;

namespace ArkheideSystem.Flourish.Controls;

/// <summary>A Flourish-styled tooltip with shell-region-aware placement.</summary>
public class FlourishToolTip : WpfToolTip
{
    static FlourishToolTip()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(FlourishToolTip),
            new FrameworkPropertyMetadata(typeof(FlourishToolTip))
        );
    }
}
