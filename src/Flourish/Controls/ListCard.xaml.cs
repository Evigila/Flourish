using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Markup;
using WpfHorizontalAlignment = System.Windows.HorizontalAlignment;
using WpfVerticalAlignment = System.Windows.VerticalAlignment;

namespace ArkheideSystem.Flourish.Controls;

/// <summary>
/// A compact configuration row with a left icon, vertically stacked copy, and a right action.
/// </summary>
[ContentProperty(nameof(ActionBody))]
public class ListCard : Card
{
    /// <summary>Identifies the <see cref="Icon" /> dependency property.</summary>
    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon),
        typeof(string),
        typeof(ListCard),
        new FrameworkPropertyMetadata(null),
        IsIconValid
    );

    /// <summary>Identifies the <see cref="ActionBody" /> dependency property.</summary>
    public static readonly DependencyProperty ActionBodyProperty = DependencyProperty.Register(
        nameof(ActionBody),
        typeof(object),
        typeof(ListCard),
        new FrameworkPropertyMetadata(null, OnLogicalContentChanged)
    );

    static ListCard()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ListCard),
            new FrameworkPropertyMetadata(typeof(ListCard))
        );
        VariantProperty.OverrideMetadata(
            typeof(ListCard),
            new FrameworkPropertyMetadata(Variant.Standard, null, CoerceVariant)
        );
        ContentHorizontalAlignmentProperty.OverrideMetadata(
            typeof(ListCard),
            new FrameworkPropertyMetadata(
                WpfHorizontalAlignment.Stretch,
                null,
                CoerceHorizontalAlignment
            )
        );
        ContentVerticalAlignmentProperty.OverrideMetadata(
            typeof(ListCard),
            new FrameworkPropertyMetadata(
                WpfVerticalAlignment.Center,
                null,
                CoerceVerticalAlignment
            )
        );
    }

    /// <summary>
    /// Gets or sets the single icon-font glyph displayed in the fixed left region.
    /// </summary>
    public string? Icon
    {
        get => (string?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>Gets or sets the interactive control displayed in the right action region.</summary>
    public object? ActionBody
    {
        get => GetValue(ActionBodyProperty);
        set => SetValue(ActionBodyProperty, value);
    }

    /// <inheritdoc />
    protected override IEnumerator LogicalChildren => EnumerateLogicalChildren();

    private static object CoerceVariant(DependencyObject dependencyObject, object value)
    {
        return Variant.Standard;
    }

    private static object CoerceHorizontalAlignment(
        DependencyObject dependencyObject,
        object value
    )
    {
        return WpfHorizontalAlignment.Stretch;
    }

    private static object CoerceVerticalAlignment(
        DependencyObject dependencyObject,
        object value
    )
    {
        return WpfVerticalAlignment.Center;
    }

    private static bool IsIconValid(object? value)
    {
        return value is null
            || value is string icon
                && (icon.Length == 0 || new StringInfo(icon).LengthInTextElements == 1);
    }

    private static void OnLogicalContentChanged(
        DependencyObject dependencyObject,
        DependencyPropertyChangedEventArgs eventArgs
    )
    {
        var card = (ListCard)dependencyObject;
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
        if (ActionBody is not null)
        {
            yield return ActionBody;
        }
    }
}
