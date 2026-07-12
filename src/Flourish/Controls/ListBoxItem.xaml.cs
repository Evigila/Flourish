using System.Windows;
using WpfListBoxItem = System.Windows.Controls.ListBoxItem;

namespace ArkheideSystem.Flourish.Controls;

/// <summary>A Flourish-styled list-box item container.</summary>
public class FlourishListBoxItem : WpfListBoxItem
{
    /// <summary>Identifies the navigation visibility state.</summary>
    public static readonly DependencyProperty IsItemVisibleProperty = DependencyProperty.Register(
        nameof(IsItemVisible),
        typeof(bool),
        typeof(FlourishListBoxItem),
        new FrameworkPropertyMetadata(true)
    );

    /// <summary>Identifies whether this item represents a navigation group heading.</summary>
    public static readonly DependencyProperty IsGroupHeaderProperty = DependencyProperty.Register(
        nameof(IsGroupHeader),
        typeof(bool),
        typeof(FlourishListBoxItem),
        new FrameworkPropertyMetadata(false)
    );

    /// <summary>Identifies whether this item dispatches a command.</summary>
    public static readonly DependencyProperty IsCommandItemProperty = DependencyProperty.Register(
        nameof(IsCommandItem),
        typeof(bool),
        typeof(FlourishListBoxItem),
        new FrameworkPropertyMetadata(false)
    );

    static FlourishListBoxItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(FlourishListBoxItem),
            new FrameworkPropertyMetadata(typeof(FlourishListBoxItem))
        );
    }

    /// <summary>Gets or sets whether a navigation item is visible.</summary>
    public bool IsItemVisible
    {
        get => (bool)GetValue(IsItemVisibleProperty);
        set => SetValue(IsItemVisibleProperty, value);
    }

    /// <summary>Gets or sets whether this is a navigation group heading.</summary>
    public bool IsGroupHeader
    {
        get => (bool)GetValue(IsGroupHeaderProperty);
        set => SetValue(IsGroupHeaderProperty, value);
    }

    /// <summary>Gets or sets whether this navigation item dispatches a command.</summary>
    public bool IsCommandItem
    {
        get => (bool)GetValue(IsCommandItemProperty);
        set => SetValue(IsCommandItemProperty, value);
    }
}
