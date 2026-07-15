using System.Collections;
using System.Windows;
using System.Windows.Markup;
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
/// A themed content surface with built-in title, supporting text, and a flexible body.
/// </summary>
[ContentProperty(nameof(Body))]
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

    /// <summary>Identifies the <see cref="Text" /> dependency property.</summary>
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        nameof(Text),
        typeof(string),
        typeof(Card),
        new FrameworkPropertyMetadata(string.Empty)
    );

    /// <summary>Identifies the <see cref="Body" /> dependency property.</summary>
    public static readonly DependencyProperty BodyProperty = DependencyProperty.Register(
        nameof(Body),
        typeof(object),
        typeof(Card),
        new FrameworkPropertyMetadata(null, OnBodyChanged)
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

    /// <summary>Gets or sets the card heading.</summary>
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Gets or sets the card's supporting text.</summary>
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>
    /// Gets or sets arbitrary content displayed with the built-in title and text. This is the
    /// card's implicit XAML content and can be <see langword="null" />.
    /// </summary>
    public object? Body
    {
        get => GetValue(BodyProperty);
        set => SetValue(BodyProperty, value);
    }

    /// <summary>Gets or sets the horizontal alignment of the copy-and-body group.</summary>
    public WpfHorizontalAlignment ContentHorizontalAlignment
    {
        get => (WpfHorizontalAlignment)GetValue(ContentHorizontalAlignmentProperty);
        set => SetValue(ContentHorizontalAlignmentProperty, value);
    }

    /// <summary>
    /// Gets or sets the vertical alignment of the copy-and-body group. When set to
    /// <see cref="WpfVerticalAlignment.Bottom" />, the body is placed above the copy.
    /// </summary>
    public WpfVerticalAlignment ContentVerticalAlignment
    {
        get => (WpfVerticalAlignment)GetValue(ContentVerticalAlignmentProperty);
        set => SetValue(ContentVerticalAlignmentProperty, value);
    }

    /// <inheritdoc />
    protected override IEnumerator LogicalChildren => EnumerateLogicalChildren();

    private static void OnBodyChanged(
        DependencyObject dependencyObject,
        DependencyPropertyChangedEventArgs eventArgs
    )
    {
        var card = (Card)dependencyObject;
        if (eventArgs.OldValue is not null)
        {
            card.RemoveLogicalChild(eventArgs.OldValue);
        }

        if (eventArgs.NewValue is not null)
        {
            card.AddLogicalChild(eventArgs.NewValue);
        }
    }

    private static bool IsVariantValid(object value)
    {
        return value is Variant variant && Enum.IsDefined(variant);
    }

    private static bool IsHorizontalAlignmentValid(object value)
    {
        return value is WpfHorizontalAlignment alignment && Enum.IsDefined(alignment);
    }

    private static bool IsVerticalAlignmentValid(object value)
    {
        return value is WpfVerticalAlignment alignment && Enum.IsDefined(alignment);
    }

    private IEnumerator EnumerateLogicalChildren()
    {
        if (Body is not null)
        {
            yield return Body;
        }
    }
}
