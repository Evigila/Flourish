using ArkheideSystem.Flourish.Abstract;
using System.Windows;
using WpfComboBoxItem = System.Windows.Controls.ComboBoxItem;

namespace ArkheideSystem.Flourish.Controls;

/// <summary>A Flourish-styled combo-box item container.</summary>
public class ComboBoxItem : WpfComboBoxItem
{
    static ComboBoxItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ComboBoxItem),
            new FrameworkPropertyMetadata(typeof(ComboBoxItem))
        );
    }

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        HoverReveal.NotifyTemplateApplied(this);
    }
}
