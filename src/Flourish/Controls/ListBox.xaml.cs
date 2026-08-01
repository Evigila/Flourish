using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using WpfBinding = System.Windows.Data.Binding;
using WpfListBox = System.Windows.Controls.ListBox;

namespace ArkheideSystem.Flourish.Controls;

/// <summary>Describes the semantic presentation of a <see cref="ListBox" />.</summary>
public enum ListBoxAppearance
{
    /// <summary>A bordered general-purpose list.</summary>
    Standard,

    /// <summary>A borderless shell navigation list.</summary>
    Borderless,
}

/// <summary>A Flourish-styled list selector that generates Flourish item containers.</summary>
public class ListBox : WpfListBox
{
    private static readonly DependencyProperty IsBorderlessPreparedProperty =
        DependencyProperty.RegisterAttached(
            "IsBorderlessPrepared",
            typeof(bool),
            typeof(ListBox),
            new FrameworkPropertyMetadata(false)
        );

    /// <summary>Identifies the <see cref="Appearance" /> dependency property.</summary>
    public static readonly DependencyProperty AppearanceProperty = DependencyProperty.Register(
        nameof(Appearance),
        typeof(ListBoxAppearance),
        typeof(ListBox),
        new FrameworkPropertyMetadata(
            ListBoxAppearance.Standard,
            static (dependencyObject, _) =>
                ((ListBox)dependencyObject).RefreshContainerPresentation()
        ),
        value => value is ListBoxAppearance appearance && Enum.IsDefined(appearance)
    );

    /// <summary>Identifies the <see cref="IsCompact" /> dependency property.</summary>
    public static readonly DependencyProperty IsCompactProperty = DependencyProperty.Register(
        nameof(IsCompact),
        typeof(bool),
        typeof(ListBox),
        new FrameworkPropertyMetadata(false)
    );

    static ListBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ListBox),
            new FrameworkPropertyMetadata(typeof(ListBox))
        );
    }

    /// <summary>Gets or sets the semantic presentation of the list.</summary>
    public ListBoxAppearance Appearance
    {
        get => (ListBoxAppearance)GetValue(AppearanceProperty);
        set => SetValue(AppearanceProperty, value);
    }

    /// <summary>Gets or sets whether navigation items use their collapsed geometry.</summary>
    public bool IsCompact
    {
        get => (bool)GetValue(IsCompactProperty);
        set => SetValue(IsCompactProperty, value);
    }

    /// <inheritdoc />
    protected override DependencyObject GetContainerForItemOverride()
    {
        return new FlourishListBoxItem();
    }

    /// <inheritdoc />
    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is FlourishListBoxItem;
    }

    /// <inheritdoc />
    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);
        if (element is FlourishListBoxItem container)
        {
            ConfigureContainerPresentation(container, item);
        }
    }

    /// <inheritdoc />
    protected override void ClearContainerForItemOverride(DependencyObject element, object item)
    {
        if (element is FlourishListBoxItem container)
        {
            ClearBorderlessPresentation(container);
        }

        base.ClearContainerForItemOverride(element, item);
    }

    private void RefreshContainerPresentation()
    {
        for (var index = 0; index < Items.Count; index++)
        {
            if (
                ItemContainerGenerator.ContainerFromIndex(index)
                is FlourishListBoxItem container
            )
            {
                ConfigureContainerPresentation(container, Items[index]);
            }
        }
    }

    private void ConfigureContainerPresentation(FlourishListBoxItem container, object item)
    {
        ClearBorderlessPresentation(container);
        if (Appearance != ListBoxAppearance.Borderless)
        {
            return;
        }

        // A caller may provide a FlourishListBoxItem directly instead of a data item.
        // Its local values and bindings are already the presentation contract and must not
        // be replaced with bindings whose source would be the container itself.
        if (ReferenceEquals(container, item))
        {
            return;
        }

        Bind(container, FlourishListBoxItem.IsItemVisibleProperty, item, "IsVisible");
        Bind(container, FlourishListBoxItem.IsGroupHeaderProperty, item, "IsGroupHeader");
        Bind(container, FlourishListBoxItem.IsCommandItemProperty, item, "IsCommandItem");
        Bind(container, IsEnabledProperty, item, "IsEnabled");

        Bind(container, ToolTipProperty, item, "Label");
        container.SetValue(IsBorderlessPreparedProperty, true);
    }

    private static void ClearBorderlessPresentation(FlourishListBoxItem container)
    {
        if (!(bool)container.GetValue(IsBorderlessPreparedProperty))
        {
            return;
        }

        BindingOperations.ClearBinding(container, FlourishListBoxItem.IsItemVisibleProperty);
        BindingOperations.ClearBinding(container, FlourishListBoxItem.IsGroupHeaderProperty);
        BindingOperations.ClearBinding(container, FlourishListBoxItem.IsCommandItemProperty);
        BindingOperations.ClearBinding(container, IsEnabledProperty);
        container.ClearValue(ToolTipProperty);
        container.ClearValue(IsBorderlessPreparedProperty);
    }

    private static void Bind(
        DependencyObject target,
        DependencyProperty property,
        object source,
        string path
    )
    {
        BindingOperations.SetBinding(
            target,
            property,
            new WpfBinding(path) { Source = source }
        );
    }
}
