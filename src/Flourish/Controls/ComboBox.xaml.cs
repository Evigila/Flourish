using ArkheideSystem.Flourish.Abstract;
using System.Windows;
using WpfComboBox = System.Windows.Controls.ComboBox;

namespace ArkheideSystem.Flourish.Controls;

/// <summary>A Flourish-styled selector that generates Flourish item containers.</summary>
public class ComboBox : WpfComboBox
{
    static ComboBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ComboBox),
            new FrameworkPropertyMetadata(typeof(ComboBox))
        );
    }

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        HoverReveal.NotifyTemplateApplied(this);
    }

    /// <inheritdoc />
    protected override DependencyObject GetContainerForItemOverride()
    {
        return new ComboBoxItem();
    }

    /// <inheritdoc />
    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is ComboBoxItem;
    }
}
