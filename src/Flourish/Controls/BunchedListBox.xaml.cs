using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using ArkheideSystem.Flourish.Internal.Interaction;

namespace ArkheideSystem.Flourish.Controls;

/// <summary>
/// A list selector that coordinates hover, pressed, and selection backgrounds in one
/// parent-owned indicator layer.
/// </summary>
[TemplatePart(Name = InteractionViewportPartName, Type = typeof(FrameworkElement))]
[TemplatePart(Name = IndicatorLayerPartName, Type = typeof(Canvas))]
[TemplatePart(Name = SelectionChromePartName, Type = typeof(Border))]
[TemplatePart(Name = HoverChromePartName, Type = typeof(Border))]
[TemplatePart(Name = PressedChromePartName, Type = typeof(Border))]
[TemplatePart(Name = ScrollViewerPartName, Type = typeof(ScrollViewer))]
public class BunchedListBox : ListBox
{
    internal const string InteractionViewportPartName = "PART_InteractionViewport";
    internal const string IndicatorLayerPartName = "PART_IndicatorLayer";
    internal const string SelectionChromePartName = "PART_SelectionChrome";
    internal const string HoverChromePartName = "PART_HoverChrome";
    internal const string PressedChromePartName = "PART_PressedChrome";
    internal const string ScrollViewerPartName = "PART_ScrollViewer";

    private readonly BunchedListBoxInteractionController interactionController;

    static BunchedListBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(BunchedListBox),
            new FrameworkPropertyMetadata(typeof(BunchedListBox))
        );
    }

    /// <summary>Initializes a new instance of the <see cref="BunchedListBox" /> class.</summary>
    public BunchedListBox()
    {
        interactionController = new BunchedListBoxInteractionController(this);
    }

    internal BunchedListBoxInteractionController InteractionController => interactionController;

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
        interactionController.ClearTemplate();
        base.OnApplyTemplate();
        interactionController.ApplyTemplate(
            GetTemplateChild(InteractionViewportPartName) as FrameworkElement,
            GetTemplateChild(IndicatorLayerPartName) as Canvas,
            GetTemplateChild(SelectionChromePartName) as Border,
            GetTemplateChild(HoverChromePartName) as Border,
            GetTemplateChild(PressedChromePartName) as Border,
            GetTemplateChild(ScrollViewerPartName) as ScrollViewer
        );
    }

    /// <inheritdoc />
    protected override DependencyObject GetContainerForItemOverride()
    {
        return new BunchedListBoxItem();
    }

    /// <inheritdoc />
    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is BunchedListBoxItem;
    }

    /// <inheritdoc />
    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);
        interactionController.ContainerPrepared(element);
    }

    /// <inheritdoc />
    protected override void ClearContainerForItemOverride(DependencyObject element, object item)
    {
        interactionController.ContainerClearing(element);
        base.ClearContainerForItemOverride(element, item);
    }

    /// <inheritdoc />
    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        base.OnSelectionChanged(e);
        interactionController.SelectionChanged();
    }

    /// <inheritdoc />
    protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
    {
        base.OnItemsChanged(e);
        interactionController.ItemsChanged();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (
            e.Property == HoverReveal.IsEnabledProperty
            || e.Property == HoverReveal.IsMotionEnabledProperty
            || e.Property == HoverReveal.AnimationDurationProperty
            || e.Property == SelectionModeProperty
            || e.Property == System.Windows.Controls.ScrollViewer.CanContentScrollProperty
        )
        {
            interactionController.PolicyChanged();
        }
    }
}
