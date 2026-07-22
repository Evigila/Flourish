using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using WpfControl = System.Windows.Controls.Control;
using WpfHorizontalAlignment = System.Windows.HorizontalAlignment;
using WpfVerticalAlignment = System.Windows.VerticalAlignment;

namespace ArkheideSystem.Flourish.Controls;

/// <summary>
/// Describes the visual variant of a <see cref="Card" />.
/// </summary>
public enum Variant
{
    /// <summary>A content surface separated from its background by elevation.</summary>
    Elevated,

    /// <summary>The default content surface.</summary>
    Standard,

    /// <summary>A quiet content surface filled with a neutral tone.</summary>
    Tonal,

    /// <summary>A high-emphasis content surface filled with the primary color.</summary>
    Filled,
}

/// <summary>
/// A themed, non-interactive surface that presents an optional title and one block of text.
/// </summary>
public class Card : WpfControl
{
    /// <summary>Identifies the <see cref="Variant" /> dependency property.</summary>
    public static readonly DependencyProperty VariantProperty = DependencyProperty.Register(
        nameof(Variant),
        typeof(Variant),
        typeof(Card),
        new FrameworkPropertyMetadata(Variant.Standard),
        IsVariantValid
    );

    /// <summary>Identifies the <see cref="Title" /> dependency property.</summary>
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title),
        typeof(string),
        typeof(Card),
        new FrameworkPropertyMetadata(string.Empty)
    );

    /// <summary>Identifies the <see cref="Content" /> dependency property.</summary>
    public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(
        nameof(Content),
        typeof(string),
        typeof(Card),
        new FrameworkPropertyMetadata(string.Empty)
    );

    /// <summary>Identifies the <see cref="Icon" /> dependency property.</summary>
    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon),
        typeof(string),
        typeof(Card),
        new FrameworkPropertyMetadata(null),
        IsIconValid
    );

    /// <summary>Identifies the <see cref="IconPosition" /> dependency property.</summary>
    public static readonly DependencyProperty IconPositionProperty = DependencyProperty.Register(
        nameof(IconPosition),
        typeof(Dock),
        typeof(Card),
        new FrameworkPropertyMetadata(Dock.Left),
        value => value is Dock position && Enum.IsDefined(position)
    );

    /// <summary>
    /// Identifies the <see cref="ContentHorizontalAlignment" /> dependency property.
    /// </summary>
    public static readonly DependencyProperty ContentHorizontalAlignmentProperty =
        DependencyProperty.Register(
            nameof(ContentHorizontalAlignment),
            typeof(WpfHorizontalAlignment),
            typeof(Card),
            new FrameworkPropertyMetadata(WpfHorizontalAlignment.Stretch),
            IsHorizontalAlignmentValid
        );

    /// <summary>
    /// Identifies the <see cref="ContentVerticalAlignment" /> dependency property.
    /// </summary>
    public static readonly DependencyProperty ContentVerticalAlignmentProperty =
        DependencyProperty.Register(
            nameof(ContentVerticalAlignment),
            typeof(WpfVerticalAlignment),
            typeof(Card),
            new FrameworkPropertyMetadata(WpfVerticalAlignment.Stretch),
            IsVerticalAlignmentValid
        );

    static Card()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(Card),
            new FrameworkPropertyMetadata(typeof(Card))
        );
    }

    /// <summary>Gets or sets the visual variant of the card.</summary>
    public Variant Variant
    {
        get => (Variant)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    /// <summary>Gets or sets the optional card heading.</summary>
    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Gets or sets the optional body text presented by the card.</summary>
    public string? Content
    {
        get => (string?)GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    /// <summary>
    /// Gets or sets the optional single icon-font glyph presented by the card.
    /// </summary>
    public string? Icon
    {
        get => (string?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>Gets or sets where the icon appears relative to the card copy.</summary>
    public Dock IconPosition
    {
        get => (Dock)GetValue(IconPositionProperty);
        set => SetValue(IconPositionProperty, value);
    }

    /// <summary>Gets or sets the horizontal alignment of the card copy.</summary>
    public WpfHorizontalAlignment ContentHorizontalAlignment
    {
        get => (WpfHorizontalAlignment)GetValue(ContentHorizontalAlignmentProperty);
        set => SetValue(ContentHorizontalAlignmentProperty, value);
    }

    /// <summary>Gets or sets the vertical alignment of the card copy.</summary>
    public WpfVerticalAlignment ContentVerticalAlignment
    {
        get => (WpfVerticalAlignment)GetValue(ContentVerticalAlignmentProperty);
        set => SetValue(ContentVerticalAlignmentProperty, value);
    }

    private static bool IsVariantValid(object value)
    {
        return value is Variant variant && Enum.IsDefined(variant);
    }

    private static bool IsIconValid(object? value)
    {
        return value is null
            || value is string icon
                && (icon.Length == 0 || new StringInfo(icon).LengthInTextElements == 1);
    }

    private static bool IsHorizontalAlignmentValid(object value)
    {
        return value is WpfHorizontalAlignment alignment && Enum.IsDefined(alignment);
    }

    private static bool IsVerticalAlignmentValid(object value)
    {
        return value is WpfVerticalAlignment alignment && Enum.IsDefined(alignment);
    }
}
