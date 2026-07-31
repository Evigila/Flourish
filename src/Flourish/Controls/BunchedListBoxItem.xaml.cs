using System.Windows;

namespace ArkheideSystem.Flourish.Controls;

/// <summary>
/// A list-box item whose background interaction states are coordinated by a
/// <see cref="BunchedListBox" />.
/// </summary>
public class BunchedListBoxItem : FlourishListBoxItem
{
    static BunchedListBoxItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(BunchedListBoxItem),
            new FrameworkPropertyMetadata(typeof(BunchedListBoxItem))
        );
    }
}
