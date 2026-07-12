using System.Windows;
using WpfComboBox = System.Windows.Controls.ComboBox;

namespace ArkheideSystem.Flourish.Controls;

/// <summary>A Flourish-styled selector that generates Flourish item containers.</summary>
public class FlourishComboBox : WpfComboBox
{
    static FlourishComboBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(FlourishComboBox),
            new FrameworkPropertyMetadata(typeof(FlourishComboBox))
        );
    }

    /// <inheritdoc />
    protected override DependencyObject GetContainerForItemOverride()
    {
        return new FlourishComboBoxItem();
    }

    /// <inheritdoc />
    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is FlourishComboBoxItem;
    }
}
