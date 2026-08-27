using ArkheideSystem.Flourish.Abstract;
using System.Windows;
using WpfRadioButton = System.Windows.Controls.RadioButton;

namespace ArkheideSystem.Flourish.Controls;

/// <summary>A Flourish-styled mutually exclusive option.</summary>
public class RadioButton : WpfRadioButton
{
    static RadioButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(RadioButton),
            new FrameworkPropertyMetadata(typeof(RadioButton))
        );
    }
}
