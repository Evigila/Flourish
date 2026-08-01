using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace ArkheideSystem.Flourish.Controls;

/// <summary>
/// Describes the semantic typography role of a <see cref="FlourishTextBlock" />.
/// </summary>
public enum FlourishTextRole
{
    /// <summary>Regular body copy.</summary>
    Body,

    /// <summary>
    /// A single wrapped block of body copy. Use <see cref="Document" /> to arrange multiple
    /// <see cref="Paragraph" /> elements with standard indentation and spacing.
    /// </summary>
    Paragraph,

    /// <summary>Compact supporting copy.</summary>
    Caption,

    /// <summary>De-emphasized supporting copy.</summary>
    Muted,

    /// <summary>A label associated with an input field.</summary>
    FieldLabel,

    /// <summary>A subtitle below a larger heading.</summary>
    Subtitle,

    /// <summary>Compact supporting copy below a heading.</summary>
    Description,

    /// <summary>A heading used inside a card, presenter, or compact content surface.</summary>
    CardTitle,

    /// <summary>The large heading used by a <see cref="Chunk" /> content section.</summary>
    SectionTitle,

    /// <summary>The primary page heading reserved for <see cref="HeaderChunk" />.</summary>
    PageTitle,

    /// <summary>Status or feedback text.</summary>
    Status,

    /// <summary>An icon glyph rendered with the configured icon typeface.</summary>
    Icon,
}

/// <summary>
/// A Flourish-styled text element with a semantic typography role.
/// </summary>
public class FlourishTextBlock : TextBlock
{
    private static readonly DependencyPropertyDescriptor[] LineMetricDescriptors =
    {
        DependencyPropertyDescriptor.FromProperty(LineHeightProperty, typeof(TextBlock)),
        DependencyPropertyDescriptor.FromProperty(FontFamilyProperty, typeof(TextBlock)),
        DependencyPropertyDescriptor.FromProperty(FontSizeProperty, typeof(TextBlock)),
        DependencyPropertyDescriptor.FromProperty(PaddingProperty, typeof(TextBlock)),
    };

    private bool _isObservingLineMetrics;

    /// <summary>
    /// Identifies the <see cref="MaxLines" /> dependency property.
    /// </summary>
    public static readonly DependencyProperty MaxLinesProperty = DependencyProperty.Register(
        nameof(MaxLines),
        typeof(int),
        typeof(FlourishTextBlock),
        new FrameworkPropertyMetadata(
            0,
            FrameworkPropertyMetadataOptions.AffectsMeasure,
            OnMaxLinesChanged
        ),
        value => value is int lines && lines >= 0
    );

    /// <summary>
    /// Identifies the <see cref="Role" /> dependency property.
    /// </summary>
    public static readonly DependencyProperty RoleProperty = DependencyProperty.Register(
        nameof(Role),
        typeof(FlourishTextRole),
        typeof(FlourishTextBlock),
        new FrameworkPropertyMetadata(FlourishTextRole.Body),
        IsRoleValid
    );

    static FlourishTextBlock()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(FlourishTextBlock),
            new FrameworkPropertyMetadata(typeof(FlourishTextBlock))
        );
        MaxHeightProperty.OverrideMetadata(
            typeof(FlourishTextBlock),
            new FrameworkPropertyMetadata(
                double.PositiveInfinity,
                null,
                CoerceMaximumHeight
            )
        );
    }

    /// <summary>
    /// Gets or sets the semantic typography role of the text.
    /// </summary>
    public FlourishTextRole Role
    {
        get => (FlourishTextRole)GetValue(RoleProperty);
        set => SetValue(RoleProperty, value);
    }

    /// <summary>Initializes a new instance of the <see cref="FlourishTextBlock" /> class.</summary>
    public FlourishTextBlock()
    {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    /// <summary>
    /// Gets or sets the maximum number of rendered lines. A value of zero allows unlimited lines.
    /// </summary>
    public int MaxLines
    {
        get => (int)GetValue(MaxLinesProperty);
        set => SetValue(MaxLinesProperty, value);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (!_isObservingLineMetrics)
        {
            foreach (var descriptor in LineMetricDescriptors)
            {
                descriptor.AddValueChanged(this, OnLineMetricChanged);
            }

            _isObservingLineMetrics = true;
        }

        CoerceValue(MaxHeightProperty);
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        if (!_isObservingLineMetrics)
        {
            return;
        }

        foreach (var descriptor in LineMetricDescriptors)
        {
            descriptor.RemoveValueChanged(this, OnLineMetricChanged);
        }

        _isObservingLineMetrics = false;
    }

    private void OnLineMetricChanged(object? sender, EventArgs e)
    {
        CoerceValue(MaxHeightProperty);
    }

    private static void OnMaxLinesChanged(
        DependencyObject dependencyObject,
        DependencyPropertyChangedEventArgs _
    )
    {
        dependencyObject.CoerceValue(MaxHeightProperty);
    }

    private static object CoerceMaximumHeight(
        DependencyObject dependencyObject,
        object baseValue
    )
    {
        var textBlock = (FlourishTextBlock)dependencyObject;
        var maximumHeight = (double)baseValue;
        if (textBlock.MaxLines == 0)
        {
            return maximumHeight;
        }

        var effectiveLineHeight = double.IsNaN(textBlock.LineHeight)
            ? textBlock.FontFamily.LineSpacing * textBlock.FontSize
            : textBlock.LineHeight;
        var lineLimit =
            (effectiveLineHeight * textBlock.MaxLines)
            + textBlock.Padding.Top
            + textBlock.Padding.Bottom;
        return Math.Min(maximumHeight, lineLimit);
    }

    private static bool IsRoleValid(object value)
    {
        return value is FlourishTextRole role && Enum.IsDefined(role);
    }
}
