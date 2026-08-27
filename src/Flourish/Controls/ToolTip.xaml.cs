using ArkheideSystem.Flourish.Abstract;
using System.Windows;
using WpfToolTip = System.Windows.Controls.ToolTip;

namespace ArkheideSystem.Flourish.Controls;

/// <summary>A Flourish-styled tooltip with shell-region-aware placement.</summary>
public class ToolTip : WpfToolTip
{
    static ToolTip()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ToolTip),
            new FrameworkPropertyMetadata(typeof(ToolTip))
        );
    }
}
