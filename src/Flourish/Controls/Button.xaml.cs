using System.Windows;
using WpfButton = System.Windows.Controls.Button;

namespace ArkheideSystem.Flourish.Controls;

/// <summary>
/// Describes the semantic appearance of a <see cref="FlourishButton" />.
/// </summary>
public enum FlourishButtonAppearance
{
    /// <summary>A neutral action button.</summary>
    Standard,

    /// <summary>A visually prominent primary action.</summary>
    Primary,

    /// <summary>A low-emphasis action that blends into its surrounding surface.</summary>
    Subtle,

    /// <summary>An interactive card surface.</summary>
    Card,

    /// <summary>A destructive action with warning feedback.</summary>
    Danger,
}

/// <summary>
/// Describes the layout role of a <see cref="FlourishButton" />.
/// </summary>
public enum FlourishButtonVariant
{
    /// <summary>A regular content button.</summary>
    Standard,

    /// <summary>A compact, square icon button.</summary>
    Icon,

    /// <summary>A compact command hosted by a toolbar.</summary>
    Toolbar,

    /// <summary>A dialog or form action button.</summary>
    Action,

    /// <summary>A native-sized window caption command.</summary>
    WindowCaption,

    /// <summary>A small status-bar icon command.</summary>
    StatusIcon,

    /// <summary>An action hosted by a Flourish message box.</summary>
    MessageBox,
}

/// <summary>
/// A Flourish-styled button with a semantic appearance contract.
/// </summary>
public class FlourishButton : WpfButton
{
    /// <summary>
    /// Identifies the <see cref="Appearance" /> dependency property.
    /// </summary>
    public static readonly DependencyProperty AppearanceProperty = DependencyProperty.Register(
        nameof(Appearance),
        typeof(FlourishButtonAppearance),
        typeof(FlourishButton),
        new FrameworkPropertyMetadata(FlourishButtonAppearance.Standard),
        IsAppearanceValid
    );

    /// <summary>
    /// Identifies the <see cref="Variant" /> dependency property.
    /// </summary>
    public static readonly DependencyProperty VariantProperty = DependencyProperty.Register(
        nameof(Variant),
        typeof(FlourishButtonVariant),
        typeof(FlourishButton),
        new FrameworkPropertyMetadata(FlourishButtonVariant.Standard),
        IsVariantValid
    );

    static FlourishButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(FlourishButton),
            new FrameworkPropertyMetadata(typeof(FlourishButton))
        );
    }

    /// <summary>
    /// Gets or sets the semantic visual appearance of the button.
    /// </summary>
    public FlourishButtonAppearance Appearance
    {
        get => (FlourishButtonAppearance)GetValue(AppearanceProperty);
        set => SetValue(AppearanceProperty, value);
    }

    /// <summary>
    /// Gets or sets the layout role used by the button template.
    /// </summary>
    public FlourishButtonVariant Variant
    {
        get => (FlourishButtonVariant)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        HoverReveal.NotifyTemplateApplied(this);
    }

    private static bool IsAppearanceValid(object value)
    {
        return value is FlourishButtonAppearance appearance && Enum.IsDefined(appearance);
    }

    private static bool IsVariantValid(object value)
    {
        return value is FlourishButtonVariant variant && Enum.IsDefined(variant);
    }
}
