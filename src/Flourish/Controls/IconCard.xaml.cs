using System.Collections;
using System.Windows;
using System.Windows.Media;
using Point = System.Windows.Point;
using Size = System.Windows.Size;
using WpfBorder = System.Windows.Controls.Border;

namespace ArkheideSystem.Flourish.Controls;

/// <summary>
/// A card that presents an icon, image, or custom visual beside or behind its copy and body.
/// </summary>
[TemplatePart(Name = PartSurfaceChrome, Type = typeof(WpfBorder))]
[TemplatePart(Name = PartClipHost, Type = typeof(FrameworkElement))]
public class IconCard : Card
{
    private const string PartSurfaceChrome = "PART_SurfaceChrome";
    private const string PartClipHost = "PART_ClipHost";

    private WpfBorder? surfaceChrome;
    private FrameworkElement? clipHost;

    /// <summary>Identifies the <see cref="Presenter" /> dependency property.</summary>
    public static readonly DependencyProperty PresenterProperty = DependencyProperty.Register(
        nameof(Presenter),
        typeof(object),
        typeof(IconCard),
        new FrameworkPropertyMetadata(null, OnPresenterChanged)
    );

    /// <summary>Identifies the <see cref="PresenterMode" /> dependency property.</summary>
    public static readonly DependencyProperty PresenterModeProperty =
        DependencyProperty.Register(
            nameof(PresenterMode),
            typeof(PresenterMode),
            typeof(IconCard),
            new FrameworkPropertyMetadata(PresenterMode.Split),
            IsPresenterModeValid
        );

    /// <summary>Identifies the <see cref="PresenterPosition" /> dependency property.</summary>
    public static readonly DependencyProperty PresenterPositionProperty =
        DependencyProperty.Register(
            nameof(PresenterPosition),
            typeof(PresenterPosition),
            typeof(IconCard),
            new FrameworkPropertyMetadata(PresenterPosition.Left),
            IsPresenterPositionValid
        );

    static IconCard()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(IconCard),
            new FrameworkPropertyMetadata(typeof(IconCard))
        );
    }

    /// <summary>Gets or sets the icon, image, or custom visual presented by the card.</summary>
    public object? Presenter
    {
        get => GetValue(PresenterProperty);
        set => SetValue(PresenterProperty, value);
    }

    /// <summary>Gets or sets how the presenter is arranged with the copy-and-body group.</summary>
    public PresenterMode PresenterMode
    {
        get => (PresenterMode)GetValue(PresenterModeProperty);
        set => SetValue(PresenterModeProperty, value);
    }

    /// <summary>
    /// Gets or sets the presenter's position in split mode. Overlay mode ignores this value.
    /// </summary>
    public PresenterPosition PresenterPosition
    {
        get => (PresenterPosition)GetValue(PresenterPositionProperty);
        set => SetValue(PresenterPositionProperty, value);
    }

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
        if (clipHost is not null)
        {
            clipHost.SizeChanged -= ClipHost_SizeChanged;
        }

        base.OnApplyTemplate();

        surfaceChrome = GetTemplateChild(PartSurfaceChrome) as WpfBorder;
        clipHost = GetTemplateChild(PartClipHost) as FrameworkElement;
        if (clipHost is not null)
        {
            clipHost.SizeChanged += ClipHost_SizeChanged;
        }

        UpdateRoundedClip();
    }

    /// <inheritdoc />
    protected override IEnumerator LogicalChildren => EnumerateLogicalChildren();

    private static void OnPresenterChanged(
        DependencyObject dependencyObject,
        DependencyPropertyChangedEventArgs eventArgs
    )
    {
        var card = (IconCard)dependencyObject;
        if (eventArgs.OldValue is not null)
        {
            card.RemoveLogicalChild(eventArgs.OldValue);
        }

        if (eventArgs.NewValue is not null)
        {
            card.AddLogicalChild(eventArgs.NewValue);
        }
    }

    private static bool IsPresenterModeValid(object value)
    {
        return value is PresenterMode mode && Enum.IsDefined(mode);
    }

    private static bool IsPresenterPositionValid(object value)
    {
        return value is PresenterPosition position && Enum.IsDefined(position);
    }

    private static Geometry CreateRoundedRectangleClip(Size size, CornerRadius cornerRadius)
    {
        var width = Math.Max(0, size.Width);
        var height = Math.Max(0, size.Height);
        var topLeft = Math.Max(0, cornerRadius.TopLeft);
        var topRight = Math.Max(0, cornerRadius.TopRight);
        var bottomRight = Math.Max(0, cornerRadius.BottomRight);
        var bottomLeft = Math.Max(0, cornerRadius.BottomLeft);
        var scale = 1d;

        scale = LimitCornerScale(scale, width, topLeft + topRight);
        scale = LimitCornerScale(scale, width, bottomLeft + bottomRight);
        scale = LimitCornerScale(scale, height, topLeft + bottomLeft);
        scale = LimitCornerScale(scale, height, topRight + bottomRight);

        topLeft *= scale;
        topRight *= scale;
        bottomRight *= scale;
        bottomLeft *= scale;

        var geometry = new StreamGeometry();
        using (var context = geometry.Open())
        {
            context.BeginFigure(new Point(topLeft, 0), isFilled: true, isClosed: true);
            context.LineTo(new Point(width - topRight, 0), true, false);
            AppendCorner(context, new Point(width, topRight), topRight);
            context.LineTo(new Point(width, height - bottomRight), true, false);
            AppendCorner(context, new Point(width - bottomRight, height), bottomRight);
            context.LineTo(new Point(bottomLeft, height), true, false);
            AppendCorner(context, new Point(0, height - bottomLeft), bottomLeft);
            context.LineTo(new Point(0, topLeft), true, false);
            AppendCorner(context, new Point(topLeft, 0), topLeft);
        }

        geometry.Freeze();
        return geometry;
    }

    private static void AppendCorner(
        StreamGeometryContext context,
        Point endPoint,
        double radius
    )
    {
        if (radius <= 0)
        {
            context.LineTo(endPoint, true, false);
            return;
        }

        context.ArcTo(
            endPoint,
            new Size(radius, radius),
            0,
            false,
            SweepDirection.Clockwise,
            true,
            false
        );
    }

    private static double LimitCornerScale(
        double currentScale,
        double availableLength,
        double requestedLength
    )
    {
        return requestedLength > availableLength && requestedLength > 0
            ? Math.Min(currentScale, availableLength / requestedLength)
            : currentScale;
    }

    private IEnumerator EnumerateLogicalChildren()
    {
        var baseChildren = base.LogicalChildren;
        while (baseChildren.MoveNext())
        {
            yield return baseChildren.Current;
        }

        if (Presenter is not null)
        {
            yield return Presenter;
        }
    }

    private void ClipHost_SizeChanged(object sender, SizeChangedEventArgs eventArgs)
    {
        UpdateRoundedClip();
    }

    private void UpdateRoundedClip()
    {
        if (clipHost is null || surfaceChrome is null)
        {
            return;
        }

        clipHost.Clip = CreateRoundedRectangleClip(
            clipHost.RenderSize,
            surfaceChrome.CornerRadius
        );
    }
}
