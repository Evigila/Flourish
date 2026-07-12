using System.Windows;
using WpfRadioButton = System.Windows.Controls.RadioButton;

namespace ArkheideSystem.Flourish.Controls;

/// <summary>A Flourish-styled mutually exclusive option.</summary>
public class FlourishRadioButton : WpfRadioButton
{
    static FlourishRadioButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(FlourishRadioButton),
            new FrameworkPropertyMetadata(typeof(FlourishRadioButton))
        );
    }
}
