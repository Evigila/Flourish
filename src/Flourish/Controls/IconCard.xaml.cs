using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace ArkheideSystem.Flourish.Controls;

/// <summary>
/// A card that adds one icon to the optional title and body text presented by <see cref="Card" />.
/// </summary>
public class IconCard : Card
{
    /// <summary>Identifies the <see cref="Icon" /> dependency property.</summary>
    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon),
        typeof(string),
        typeof(IconCard),
        new FrameworkPropertyMetadata(null),
        IsIconValid
    );

    /// <summary>Identifies the <see cref="IconPosition" /> dependency property.</summary>
    public static readonly DependencyProperty IconPositionProperty = DependencyProperty.Register(
        nameof(IconPosition),
        typeof(Dock),
        typeof(IconCard),
        new FrameworkPropertyMetadata(Dock.Left),
        value => value is Dock position && Enum.IsDefined(position)
    );

    static IconCard()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(IconCard),
            new FrameworkPropertyMetadata(typeof(IconCard))
        );
    }

    /// <summary>
    /// Gets or sets the single icon-font glyph presented by the card. Empty content collapses.
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

    private static bool IsIconValid(object? value)
    {
        return value is null
            || value is string icon
                && (icon.Length == 0 || new StringInfo(icon).LengthInTextElements == 1);
    }
}
