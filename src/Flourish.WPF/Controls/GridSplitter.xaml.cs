using System;

using ArkheideSystem.Flourish.Abstract;
using System.Windows;
using WpfGridSplitter = System.Windows.Controls.GridSplitter;

namespace ArkheideSystem.Flourish.Controls;

/// <summary>Describes the layout role of a <see cref="GridSplitter" />.</summary>
public enum GridSplitterVariant
{
    /// <summary>A standard splitter.</summary>
    Standard,

    /// <summary>The resize affordance at the edge of the navigation pane.</summary>
    NavigationPane,
}

/// <summary>
/// A Flourish-styled grid splitter that resizes its target live behind a thin indicator.
/// </summary>
public class GridSplitter : WpfGridSplitter
{
    /// <summary>Identifies the <see cref="Variant" /> dependency property.</summary>
    public static readonly DependencyProperty VariantProperty = DependencyProperty.Register(
        nameof(Variant),
        typeof(GridSplitterVariant),
        typeof(GridSplitter),
        new FrameworkPropertyMetadata(GridSplitterVariant.Standard),
        value => value is GridSplitterVariant variant && Enum.IsDefined(variant)
    );

    static GridSplitter()
    {
        ShowsPreviewProperty.OverrideMetadata(
            typeof(GridSplitter),
            new FrameworkPropertyMetadata(
                false,
                null,
                static (_, _) => false
            )
        );
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(GridSplitter),
            new FrameworkPropertyMetadata(typeof(GridSplitter))
        );
    }

    /// <summary>Gets or sets the splitter's layout role.</summary>
    public GridSplitterVariant Variant
    {
        get => (GridSplitterVariant)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }
}
