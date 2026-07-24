using System.Windows;
using WpfGridSplitter = System.Windows.Controls.GridSplitter;

namespace ArkheideSystem.Flourish.Controls;

/// <summary>Describes the layout role of a <see cref="FlourishGridSplitter" />.</summary>
public enum FlourishGridSplitterVariant
{
    /// <summary>A standard splitter.</summary>
    Standard,

    /// <summary>The resize affordance at the edge of the navigation pane.</summary>
    NavigationPane,
}

/// <summary>
/// A Flourish-styled grid splitter that resizes its target live behind a thin indicator.
/// </summary>
public class FlourishGridSplitter : WpfGridSplitter
{
    /// <summary>Identifies the <see cref="Variant" /> dependency property.</summary>
    public static readonly DependencyProperty VariantProperty = DependencyProperty.Register(
        nameof(Variant),
        typeof(FlourishGridSplitterVariant),
        typeof(FlourishGridSplitter),
        new FrameworkPropertyMetadata(FlourishGridSplitterVariant.Standard),
        value => value is FlourishGridSplitterVariant variant && Enum.IsDefined(variant)
    );

    static FlourishGridSplitter()
    {
        ShowsPreviewProperty.OverrideMetadata(
            typeof(FlourishGridSplitter),
            new FrameworkPropertyMetadata(
                false,
                null,
                static (_, _) => false
            )
        );
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(FlourishGridSplitter),
            new FrameworkPropertyMetadata(typeof(FlourishGridSplitter))
        );
    }

    /// <summary>Gets or sets the splitter's layout role.</summary>
    public FlourishGridSplitterVariant Variant
    {
        get => (FlourishGridSplitterVariant)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }
}
