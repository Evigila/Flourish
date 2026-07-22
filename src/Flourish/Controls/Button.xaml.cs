using System.Windows;
using WpfButton = System.Windows.Controls.Button;

namespace ArkheideSystem.Flourish.Controls;

/// <summary>
/// Describes the visual variant of a <see cref="Button" />.
/// </summary>
public enum ButtonVariant
{
    /// <summary>A raised button on a neutral surface.</summary>
    Elevated,

    /// <summary>A high-emphasis button filled with the primary color.</summary>
    Filled,

    /// <summary>A medium-emphasis button filled with a secondary tonal color.</summary>
    Tonal,

    /// <summary>A button with a transparent background and visible outline.</summary>
    Outlined,

    /// <summary>A low-emphasis button without a fill or outline.</summary>
    Text,

    /// <summary>A destructive action with warning feedback.</summary>
    Danger,

    /// <summary>A neutral card surface used by card-shaped buttons.</summary>
    Standard,
}

/// <summary>
/// A themed content button with a visual variant contract.
/// </summary>
public class Button : WpfButton
{
    /// <summary>Identifies the <see cref="Icon" /> dependency property.</summary>
    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon),
        typeof(object),
        typeof(Button),
        new FrameworkPropertyMetadata(null)
    );

    /// <summary>
    /// Identifies the <see cref="Variant" /> dependency property.
    /// </summary>
    public static readonly DependencyProperty VariantProperty = DependencyProperty.Register(
        nameof(Variant),
        typeof(ButtonVariant),
        typeof(Button),
        new FrameworkPropertyMetadata(ButtonVariant.Outlined),
        IsVariantValid
    );

    static Button()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(Button),
            new FrameworkPropertyMetadata(typeof(Button))
        );
    }

    /// <summary>
    /// Gets or sets the visual variant of the button.
    /// </summary>
    public ButtonVariant Variant
    {
        get => (ButtonVariant)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    /// <summary>Gets or sets the optional icon displayed with the button content.</summary>
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        HoverReveal.NotifyTemplateApplied(this);
    }

    private static bool IsVariantValid(object value)
    {
        return value is ButtonVariant variant && Enum.IsDefined(variant);
    }
}
