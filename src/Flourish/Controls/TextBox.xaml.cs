using ArkheideSystem.Flourish.Abstract;
using System.Windows;
using WpfTextBox = System.Windows.Controls.TextBox;

namespace ArkheideSystem.Flourish.Controls;

/// <summary>A Flourish-styled editable text field.</summary>
public class TextBox : WpfTextBox
{
    static TextBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(TextBox),
            new FrameworkPropertyMetadata(typeof(TextBox))
        );
    }
}
