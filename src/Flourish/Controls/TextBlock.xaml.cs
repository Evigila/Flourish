using System;

using ArkheideSystem.Flourish.Abstract;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using WpfTextBlock = System.Windows.Controls.TextBlock;

namespace ArkheideSystem.Flourish.Controls;

/// <summary>
/// Describes the semantic typography role of a <see cref="TextBlock" />.
/// </summary>
public enum TextRole
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

/// <summary>Describes how a <see cref="TextBlock" /> participates in layout.</summary>
public enum TextLayoutMode
{
    /// <summary>Preserves the role-specific spacing used by flowing text.</summary>
    Flow,

    /// <summary>Uses a spacing-free line box for text centered inside a control.</summary>
    Control,
}

/// <summary>
/// A Flourish-styled text element with a semantic typography role.
/// </summary>
public class TextBlock : WpfTextBlock
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
        typeof(TextBlock),
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
        typeof(TextRole),
        typeof(TextBlock),
        new FrameworkPropertyMetadata(TextRole.Body),
        IsRoleValid
    );

    /// <summary>
    /// Identifies the <see cref="LayoutMode" /> dependency property.
    /// </summary>
    public static readonly DependencyProperty LayoutModeProperty = DependencyProperty.Register(
        nameof(LayoutMode),
        typeof(TextLayoutMode),
        typeof(TextBlock),
        new FrameworkPropertyMetadata(TextLayoutMode.Flow),
        IsLayoutModeValid
    );

    static TextBlock()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(TextBlock),
            new FrameworkPropertyMetadata(typeof(TextBlock))
        );
        MaxHeightProperty.OverrideMetadata(
            typeof(TextBlock),
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
    public TextRole Role
    {
        get => (TextRole)GetValue(RoleProperty);
        set => SetValue(RoleProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the text uses flowing typography spacing or a control-centered line box.
    /// </summary>
    public TextLayoutMode LayoutMode
    {
        get => (TextLayoutMode)GetValue(LayoutModeProperty);
        set => SetValue(LayoutModeProperty, value);
    }

    /// <summary>Initializes a new instance of the <see cref="TextBlock" /> class.</summary>
    public TextBlock()
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
        var textBlock = (TextBlock)dependencyObject;
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
        return value is TextRole role && Enum.IsDefined(role);
    }

    private static bool IsLayoutModeValid(object value)
    {
        return value is TextLayoutMode mode && Enum.IsDefined(mode);
    }
}
