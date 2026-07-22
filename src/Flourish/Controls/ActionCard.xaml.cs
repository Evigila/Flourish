using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Markup;
using WpfControl = System.Windows.Controls.Control;

namespace ArkheideSystem.Flourish.Controls;

/// <summary>Describes the fixed layout used by an <see cref="ActionCard" />.</summary>
public enum ActionCardVariant
{
    /// <summary>Places the icon and copy before a right-side action.</summary>
    Horizontal,

    /// <summary>Stacks the icon, copy, and action from top to bottom.</summary>
    Vertical,
}

/// <summary>
/// A fixed action surface with optional icon and copy regions plus one interactive body.
/// </summary>
[ContentProperty(nameof(Body))]
public class ActionCard : WpfControl
{
    /// <summary>Identifies the <see cref="Variant" /> dependency property.</summary>
    public static readonly DependencyProperty VariantProperty = DependencyProperty.Register(
        nameof(Variant),
        typeof(ActionCardVariant),
        typeof(ActionCard),
        new FrameworkPropertyMetadata(ActionCardVariant.Horizontal),
        IsVariantValid
    );

    /// <summary>Identifies the <see cref="Title" /> dependency property.</summary>
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title),
        typeof(string),
        typeof(ActionCard),
        new FrameworkPropertyMetadata(string.Empty)
    );

    /// <summary>Identifies the <see cref="Content" /> dependency property.</summary>
    public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(
        nameof(Content),
        typeof(string),
        typeof(ActionCard),
        new FrameworkPropertyMetadata(string.Empty)
    );

    /// <summary>Identifies the <see cref="Icon" /> dependency property.</summary>
    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon),
        typeof(string),
        typeof(ActionCard),
        new FrameworkPropertyMetadata(null),
        IsIconValid
    );

    /// <summary>Identifies the <see cref="Body" /> dependency property.</summary>
    public static readonly DependencyProperty BodyProperty = DependencyProperty.Register(
        nameof(Body),
        typeof(object),
        typeof(ActionCard),
        new FrameworkPropertyMetadata(null, OnBodyChanged)
    );

    static ActionCard()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ActionCard),
            new FrameworkPropertyMetadata(typeof(ActionCard))
        );
    }

    /// <summary>Gets or sets the fixed card layout.</summary>
    public ActionCardVariant Variant
    {
        get => (ActionCardVariant)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    /// <summary>Gets or sets the optional action heading.</summary>
    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Gets or sets the optional single block of supporting copy.</summary>
    public string? Content
    {
        get => (string?)GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    /// <summary>Gets or sets the optional single icon-font glyph.</summary>
    public string? Icon
    {
        get => (string?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>Gets or sets the single interactive control presented by the card.</summary>
    public object? Body
    {
        get => GetValue(BodyProperty);
        set => SetValue(BodyProperty, value);
    }

    /// <inheritdoc />
    protected override IEnumerator LogicalChildren => EnumerateLogicalChildren();

    private static bool IsVariantValid(object value)
    {
        return value is ActionCardVariant variant && Enum.IsDefined(variant);
    }

    private static bool IsIconValid(object? value)
    {
        return value is null
            || value is string icon
                && (icon.Length == 0 || new StringInfo(icon).LengthInTextElements == 1);
    }

    private static void OnBodyChanged(
        DependencyObject dependencyObject,
        DependencyPropertyChangedEventArgs eventArgs
    )
    {
        var card = (ActionCard)dependencyObject;
        if (eventArgs.OldValue is not null)
        {
            card.RemoveLogicalChild(eventArgs.OldValue);
        }

        if (eventArgs.NewValue is not null)
        {
            card.AddLogicalChild(eventArgs.NewValue);
        }
    }

    private IEnumerator EnumerateLogicalChildren()
    {
        if (Body is not null)
        {
            yield return Body;
        }
    }
}
