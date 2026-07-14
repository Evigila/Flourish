using System.Windows;
using WpfButton = System.Windows.Controls.Button;

namespace ArkheideSystem.Flourish.Controls;

/// <summary>
/// Describes the semantic appearance of a <see cref="Button" />.
/// </summary>
public enum ButtonAppearance
{
    /// <summary>A neutral action button.</summary>
    Standard,

    /// <summary>A primary action button.</summary>
    Primary,

    /// <summary>A low-emphasis action that blends into its surrounding surface.</summary>
    Subtle,

    /// <summary>A destructive action with warning feedback.</summary>
    Danger,
}

/// <summary>
/// A themed content button with a semantic appearance contract.
/// </summary>
public class Button : WpfButton
{
    /// <summary>
    /// Identifies the <see cref="Appearance" /> dependency property.
    /// </summary>
    public static readonly DependencyProperty AppearanceProperty = DependencyProperty.Register(
        nameof(Appearance),
        typeof(ButtonAppearance),
        typeof(Button),
        new FrameworkPropertyMetadata(ButtonAppearance.Standard),
        IsAppearanceValid
    );

    static Button()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(Button),
            new FrameworkPropertyMetadata(typeof(Button))
        );
    }

    /// <summary>
    /// Gets or sets the semantic visual appearance of the button.
    /// </summary>
    public ButtonAppearance Appearance
    {
        get => (ButtonAppearance)GetValue(AppearanceProperty);
        set => SetValue(AppearanceProperty, value);
    }

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        HoverReveal.NotifyTemplateApplied(this);
    }

    private static bool IsAppearanceValid(object value)
    {
        return value is ButtonAppearance appearance && Enum.IsDefined(appearance);
    }
}
