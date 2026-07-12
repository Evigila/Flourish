using System.Windows;
using WpfTextBox = System.Windows.Controls.TextBox;

namespace ArkheideSystem.Flourish.Controls;

/// <summary>A Flourish-styled editable text field.</summary>
public class FlourishTextBox : WpfTextBox
{
    static FlourishTextBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(FlourishTextBox),
            new FrameworkPropertyMetadata(typeof(FlourishTextBox))
        );
    }
}
