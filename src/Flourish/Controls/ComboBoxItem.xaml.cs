using System.Windows;
using WpfComboBoxItem = System.Windows.Controls.ComboBoxItem;

namespace ArkheideSystem.Flourish.Controls;

/// <summary>A Flourish-styled combo-box item container.</summary>
public class FlourishComboBoxItem : WpfComboBoxItem
{
    static FlourishComboBoxItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(FlourishComboBoxItem),
            new FrameworkPropertyMetadata(typeof(FlourishComboBoxItem))
        );
    }
}
