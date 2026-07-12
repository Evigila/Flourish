using System.Windows;
using WpfCheckBox = System.Windows.Controls.CheckBox;

namespace ArkheideSystem.Flourish.Controls;

/// <summary>A Flourish-styled three-state check box.</summary>
public class FlourishCheckBox : WpfCheckBox
{
    static FlourishCheckBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(FlourishCheckBox),
            new FrameworkPropertyMetadata(typeof(FlourishCheckBox))
        );
    }
}
