using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using System.Windows.Media;
using ArkheideSystem.Flourish.Internal.Interaction;
using WpfControl = System.Windows.Controls.Control;
using WpfHorizontalAlignment = System.Windows.HorizontalAlignment;
using WpfOrientation = System.Windows.Controls.Orientation;
using WpfPanel = System.Windows.Controls.Panel;
using WpfSize = System.Windows.Size;
using WpfVerticalAlignment = System.Windows.VerticalAlignment;

namespace ArkheideSystem.Flourish.Controls;

/// <summary>
/// A presentation layout with required copy and an explicitly selected composition.
/// In the standard split composition, copy and body content occupy one side while the presented
/// visual occupies the other. The presentation surface fills its region and centers presentation
/// content, while body content aligns with the copy on the transparent side.
/// </summary>
/// <remarks>
/// Split and Overlay compositions occupy a complete row. TopDown is the only composition that
/// may be arranged in columns with peer Presenter controls.
/// </remarks>
[ContentProperty(nameof(Presentation))]
[TemplatePart(Name = PartPresenterSurface, Type = typeof(Border))]
[TemplatePart(Name = PartClipHost, Type = typeof(FrameworkElement))]
public class Presenter : WpfControl
{
    private const string PartPresenterSurface = "PresenterSurface";
    private const string PartClipHost = "PART_ClipHost";

    private readonly RoundedClipCoordinator roundedClip = new();

    /// <summary>Identifies the <see cref="Title" /> dependency property.</summary>
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title),
        typeof(string),
        typeof(Presenter),
        new FrameworkPropertyMetadata(string.Empty)
    );

    /// <summary>Identifies the <see cref="Content" /> dependency property.</summary>
    public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(
        nameof(Content),
        typeof(string),
        typeof(Presenter),
        new FrameworkPropertyMetadata(null)
    );

    /// <summary>Identifies the <see cref="Body" /> dependency property.</summary>
    public static readonly DependencyProperty BodyProperty = DependencyProperty.Register(
        nameof(Body),
        typeof(object),
        typeof(Presenter),
        new FrameworkPropertyMetadata(null, OnLogicalContentChanged)
    );

    /// <summary>Identifies the <see cref="Presentation" /> dependency property.</summary>
    public static readonly DependencyProperty PresentationProperty = DependencyProperty.Register(
        nameof(Presentation),
        typeof(object),
        typeof(Presenter),
        new FrameworkPropertyMetadata(null, OnLogicalContentChanged)
    );

    /// <summary>
    /// Identifies the <see cref="PresentationMinHeight" /> dependency property.
    /// </summary>
    public static readonly DependencyProperty PresentationMinHeightProperty =
        DependencyProperty.Register(
            nameof(PresentationMinHeight),
            typeof(double),
            typeof(Presenter),
            new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsMeasure),
            IsPresentationMinHeightValid
        );

    /// <summary>Identifies the <see cref="PresenterMode" /> dependency property.</summary>
    public static readonly DependencyProperty PresenterModeProperty =
        DependencyProperty.Register(
            nameof(PresenterMode),
            typeof(PresenterMode),
            typeof(Presenter),
            new FrameworkPropertyMetadata(
                global::ArkheideSystem.Flourish.Controls.PresenterMode.Split
            ),
            IsPresenterModeValid
        );

    /// <summary>Identifies the <see cref="PresenterPosition" /> dependency property.</summary>
    public static readonly DependencyProperty PresenterPositionProperty =
        DependencyProperty.Register(
            nameof(PresenterPosition),
            typeof(PresenterPosition),
            typeof(Presenter),
            new FrameworkPropertyMetadata(
                global::ArkheideSystem.Flourish.Controls.PresenterPosition.Left
            ),
            IsPresenterPositionValid
        );

    static Presenter()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(Presenter),
            new FrameworkPropertyMetadata(typeof(Presenter))
        );
    }

    /// <summary>Gets or sets the required presentation heading.</summary>
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Gets or sets the required supporting copy below the heading.</summary>
    public string? Content
    {
        get => (string?)GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    /// <summary>
    /// Gets or sets optional controls or supporting content arranged on the same side as the
    /// copy. This property must be assigned explicitly in XAML.
    /// </summary>
    public object? Body
    {
        get => GetValue(BodyProperty);
        set => SetValue(BodyProperty, value);
    }

    /// <summary>
    /// Gets or sets the image, icon group, illustration, or other content being presented. This
    /// is the default XAML content property. Auto-sized surfaces fill the presentation region,
    /// fixed content remains centered, and natural-size text or grouping panels remain centered
    /// on their grouping axes. A vertical StackPanel fills the horizontal cross-axis by default.
    /// </summary>
    public object? Presentation
    {
        get => GetValue(PresentationProperty);
        set => SetValue(PresentationProperty, value);
    }

    /// <summary>
    /// Gets or sets the minimum height of the built-in presentation surface.
    /// </summary>
    public double PresentationMinHeight
    {
        get => (double)GetValue(PresentationMinHeightProperty);
        set => SetValue(PresentationMinHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the presentation is split beside, stacked above, or placed behind
    /// the copy. Authors should assign this property explicitly; its runtime fallback is
    /// <see cref="ArkheideSystem.Flourish.Controls.PresenterMode.Split" />.
    /// </summary>
    public PresenterMode PresenterMode
    {
        get => (PresenterMode)GetValue(PresenterModeProperty);
        set => SetValue(PresenterModeProperty, value);
    }

    /// <summary>
    /// Gets or sets the presentation position in split mode. Authors should assign this property
    /// explicitly; its runtime fallback places the presentation on the left.
    /// </summary>
    public PresenterPosition PresenterPosition
    {
        get => (PresenterPosition)GetValue(PresenterPositionProperty);
        set => SetValue(PresenterPositionProperty, value);
    }

    /// <inheritdoc />
    protected override IEnumerator LogicalChildren => EnumerateLogicalChildren();

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
        roundedClip.Detach();
        base.OnApplyTemplate();

        roundedClip.Attach(
            GetTemplateChild(PartClipHost) as FrameworkElement,
            GetTemplateChild(PartPresenterSurface) as Border
        );
    }

    private static bool IsPresenterModeValid(object value)
    {
        return value is PresenterMode mode && Enum.IsDefined(mode);
    }

    private static bool IsPresenterPositionValid(object value)
    {
        return value is PresenterPosition position && Enum.IsDefined(position);
    }

    private static bool IsPresentationMinHeightValid(object value)
    {
        return value is double height
            && double.IsFinite(height)
            && height >= 0;
    }

    private static void OnLogicalContentChanged(
        DependencyObject dependencyObject,
        DependencyPropertyChangedEventArgs eventArgs
    )
    {
        var presenter = (Presenter)dependencyObject;
        if (eventArgs.OldValue is not null)
        {
            presenter.RemoveLogicalChild(eventArgs.OldValue);
        }

        if (eventArgs.NewValue is not null)
        {
            presenter.AddLogicalChild(eventArgs.NewValue);
        }
    }

    private IEnumerator EnumerateLogicalChildren()
    {
        if (Body is not null)
        {
            yield return Body;
        }

        if (Presentation is not null)
        {
            yield return Presentation;
        }
    }
}

/// <summary>
/// Hosts presentation content without changing properties on the consumer-owned content tree.
/// The host always fills its surface. Auto-sized ordinary content fills an axis, fixed content is
/// centered, and natural-size grouping/text containers remain centered on their grouping axes.
/// </summary>
internal sealed class PresenterPresentationHost : FrameworkElement
{
    private readonly ContentPresenter contentPresenter;

    public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(
        nameof(Content),
        typeof(object),
        typeof(PresenterPresentationHost),
        new FrameworkPropertyMetadata(
            null,
            FrameworkPropertyMetadataOptions.AffectsMeasure
                | FrameworkPropertyMetadataOptions.AffectsArrange,
            OnContentChanged
        )
    );

    public PresenterPresentationHost()
    {
        contentPresenter = new ContentPresenter
        {
            HorizontalAlignment = WpfHorizontalAlignment.Stretch,
            VerticalAlignment = WpfVerticalAlignment.Stretch,
        };
        AddVisualChild(contentPresenter);
    }

    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    protected override int VisualChildrenCount => 1;

    /// <inheritdoc />
    protected override Visual GetVisualChild(int index)
    {
        if (index != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        return contentPresenter;
    }

    /// <inheritdoc />
    protected override WpfSize MeasureOverride(WpfSize availableSize)
    {
        contentPresenter.SnapsToDevicePixels = SnapsToDevicePixels;
        contentPresenter.UseLayoutRounding = UseLayoutRounding;
        contentPresenter.Measure(availableSize);
        return contentPresenter.DesiredSize;
    }

    /// <inheritdoc />
    protected override WpfSize ArrangeOverride(WpfSize finalSize)
    {
        var centerHorizontally = ShouldCenter(Content, horizontal: true);
        var centerVertically = ShouldCenter(Content, horizontal: false);
        var contentWidth = centerHorizontally
            ? Math.Min(contentPresenter.DesiredSize.Width, finalSize.Width)
            : finalSize.Width;
        var contentHeight = centerVertically
            ? Math.Min(contentPresenter.DesiredSize.Height, finalSize.Height)
            : finalSize.Height;
        var contentX = centerHorizontally ? (finalSize.Width - contentWidth) / 2 : 0;
        var contentY = centerVertically ? (finalSize.Height - contentHeight) / 2 : 0;

        contentPresenter.Arrange(
            new Rect(contentX, contentY, contentWidth, contentHeight)
        );
        return finalSize;
    }

    private static void OnContentChanged(
        DependencyObject dependencyObject,
        DependencyPropertyChangedEventArgs eventArgs
    )
    {
        ((PresenterPresentationHost)dependencyObject).contentPresenter.Content =
            eventArgs.NewValue;
    }

    private static bool ShouldCenter(object? content, bool horizontal)
    {
        if (content is not FrameworkElement element)
        {
            return content is not null;
        }

        var requestedSize = horizontal ? element.Width : element.Height;
        if (!double.IsNaN(requestedSize))
        {
            return true;
        }

        var alignmentProperty = horizontal
            ? HorizontalAlignmentProperty
            : VerticalAlignmentProperty;
        var localAlignment = element.ReadLocalValue(alignmentProperty);
        if (localAlignment != DependencyProperty.UnsetValue)
        {
            return localAlignment switch
            {
                WpfHorizontalAlignment localHorizontalAlignment =>
                    localHorizontalAlignment != WpfHorizontalAlignment.Stretch,
                WpfVerticalAlignment localVerticalAlignment =>
                    localVerticalAlignment != WpfVerticalAlignment.Stretch,
                _ => false,
            };
        }

        var effectiveAlignment = horizontal
            ? (object)element.HorizontalAlignment
            : element.VerticalAlignment;
        if (
            effectiveAlignment is WpfHorizontalAlignment effectiveHorizontalAlignment
                && effectiveHorizontalAlignment != WpfHorizontalAlignment.Stretch
            || effectiveAlignment is WpfVerticalAlignment effectiveVerticalAlignment
                && effectiveVerticalAlignment != WpfVerticalAlignment.Stretch
        )
        {
            return true;
        }

        return UsesNaturalSizeOnAxis(element, horizontal);
    }

    private static bool UsesNaturalSizeOnAxis(FrameworkElement element, bool horizontal)
    {
        if (element is TextBlock or WrapPanel or UniformGrid)
        {
            return true;
        }

        if (element is StackPanel stackPanel)
        {
            // A vertical group uses the complete cross-axis so controls such as lists can
            // stretch, while the group remains centered on its stacking axis. Horizontal
            // icon/text groups retain natural bounds on both axes to keep their glyphs centered.
            return !horizontal || stackPanel.Orientation == WpfOrientation.Horizontal;
        }

        return element is WpfPanel { Background: null } panel
            && panel.Children.Count > 1;
    }
}
