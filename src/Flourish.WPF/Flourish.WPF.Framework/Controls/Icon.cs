using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;

namespace ArkheideSystem.Flourish.WPF.Controls;

/// <summary>A decorative native vector icon sharing Blazor's official outlined and filled artwork.</summary>
[RuntimeNameProperty(nameof(ElementName))]
public class Icon : FrameworkElement
{
    static Icon()
    {
        FocusableProperty.OverrideMetadata(typeof(Icon), new FrameworkPropertyMetadata(false));
        IsHitTestVisibleProperty.OverrideMetadata(typeof(Icon), new FrameworkPropertyMetadata(false));
        ClipToBoundsProperty.OverrideMetadata(typeof(Icon), new FrameworkPropertyMetadata(true));
    }

    // Artwork identity is independent from native x:Name registration, including numeric official names.
    public new static readonly DependencyProperty NameProperty = DependencyProperty.Register(nameof(Name), typeof(string), typeof(Icon),
        new FrameworkPropertyMetadata("description", FrameworkPropertyMetadataOptions.AffectsRender));
    public static readonly DependencyProperty SizeProperty = DependencyProperty.Register(nameof(Size), typeof(double), typeof(Icon),
        new FrameworkPropertyMetadata(26d, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender),
        value => value is double size && double.IsFinite(size) && size > 0);
    public static readonly DependencyProperty FilledProperty = DependencyProperty.Register(nameof(Filled), typeof(bool), typeof(Icon),
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));
    public static readonly DependencyProperty ForegroundProperty = TextElement.ForegroundProperty.AddOwner(typeof(Icon),
        new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsRender));

    public new string Name { get => (string)GetValue(NameProperty); set => SetValue(NameProperty, value); }
    public double Size { get => (double)GetValue(SizeProperty); set => SetValue(SizeProperty, value); }
    public bool Filled { get => (bool)GetValue(FilledProperty); set => SetValue(FilledProperty, value); }
    public Brush Foreground { get => (Brush)GetValue(ForegroundProperty); set => SetValue(ForegroundProperty, value); }
    /// <summary>The native namescope identity used by x:Name, separate from the artwork Name.</summary>
    public string ElementName { get => base.Name; set => base.Name = value; }

    protected override Size MeasureOverride(Size availableSize) => new(Size, Size);

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);
        var size = Size;
        // Material Symbols has a 1 em design square; CSS line-height:1 puts its baseline at the square's bottom.
        drawingContext.PushTransform(new MatrixTransform(size, 0, 0, size,
            (RenderSize.Width - size) / 2, (RenderSize.Height - size) / 2 + size));
        drawingContext.DrawGeometry(Foreground, null, IconCatalog.Outline(Name, Filled));
        drawingContext.Pop();
    }

    protected override AutomationPeer OnCreateAutomationPeer() => new DecorativePeer(this);
    private sealed class DecorativePeer(Icon owner) : FrameworkElementAutomationPeer(owner)
    {
        protected override string GetClassNameCore() => nameof(Icon);
        protected override string GetNameCore() => "";
        protected override bool IsControlElementCore() => false;
        protected override bool IsContentElementCore() => false;
    }
}
