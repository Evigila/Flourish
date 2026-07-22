using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using WpfBinding = System.Windows.Data.Binding;
using WpfListBox = System.Windows.Controls.ListBox;

namespace ArkheideSystem.Flourish.Controls;

/// <summary>Describes the semantic presentation of a <see cref="FlourishListBox" />.</summary>
public enum FlourishListBoxAppearance
{
    /// <summary>A bordered general-purpose list.</summary>
    Standard,

    /// <summary>A borderless shell navigation list.</summary>
    Navigation,
}

/// <summary>A Flourish-styled list selector that generates Flourish item containers.</summary>
public class FlourishListBox : WpfListBox
{
    private static readonly DependencyProperty IsNavigationPreparedProperty =
        DependencyProperty.RegisterAttached(
            "IsNavigationPrepared",
            typeof(bool),
            typeof(FlourishListBox),
            new FrameworkPropertyMetadata(false)
        );

    /// <summary>Identifies the <see cref="Appearance" /> dependency property.</summary>
    public static readonly DependencyProperty AppearanceProperty = DependencyProperty.Register(
        nameof(Appearance),
        typeof(FlourishListBoxAppearance),
        typeof(FlourishListBox),
        new FrameworkPropertyMetadata(
            FlourishListBoxAppearance.Standard,
            static (dependencyObject, _) =>
                ((FlourishListBox)dependencyObject).RefreshContainerPresentation()
        ),
        value => value is FlourishListBoxAppearance appearance && Enum.IsDefined(appearance)
    );

    /// <summary>Identifies the <see cref="IsCompact" /> dependency property.</summary>
    public static readonly DependencyProperty IsCompactProperty = DependencyProperty.Register(
        nameof(IsCompact),
        typeof(bool),
        typeof(FlourishListBox),
        new FrameworkPropertyMetadata(false)
    );

    static FlourishListBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(FlourishListBox),
            new FrameworkPropertyMetadata(typeof(FlourishListBox))
        );
    }

    /// <summary>Gets or sets the semantic presentation of the list.</summary>
    public FlourishListBoxAppearance Appearance
    {
        get => (FlourishListBoxAppearance)GetValue(AppearanceProperty);
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
            ClearNavigationPresentation(container);
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
        ClearNavigationPresentation(container);
        if (Appearance != FlourishListBoxAppearance.Navigation)
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
        container.SetValue(IsNavigationPreparedProperty, true);
    }

    private static void ClearNavigationPresentation(FlourishListBoxItem container)
    {
        if (!(bool)container.GetValue(IsNavigationPreparedProperty))
        {
            return;
        }

        BindingOperations.ClearBinding(container, FlourishListBoxItem.IsItemVisibleProperty);
        BindingOperations.ClearBinding(container, FlourishListBoxItem.IsGroupHeaderProperty);
        BindingOperations.ClearBinding(container, FlourishListBoxItem.IsCommandItemProperty);
        BindingOperations.ClearBinding(container, IsEnabledProperty);
        container.ClearValue(ToolTipProperty);
        container.ClearValue(IsNavigationPreparedProperty);
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
