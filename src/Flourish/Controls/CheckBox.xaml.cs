using System.Windows;
using WpfCheckBox = System.Windows.Controls.CheckBox;

namespace ArkheideSystem.Flourish.Controls;

/// <summary>Describes the fixed layout used by a <see cref="CheckBox" />.</summary>
public enum CheckBoxVariant
{
    /// <summary>Places the state indicator before the content in a compact row.</summary>
    Horizontal,

    /// <summary>Places an optional icon above the content with the state indicator at the upper right.</summary>
    Vertical,
}

/// <summary>A themed two-state or three-state selection control.</summary>
public class CheckBox : WpfCheckBox
{
    /// <summary>Identifies the <see cref="Icon" /> dependency property.</summary>
    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon),
        typeof(object),
        typeof(CheckBox),
        new FrameworkPropertyMetadata(null)
    );

    /// <summary>Identifies the <see cref="Variant" /> dependency property.</summary>
    public static readonly DependencyProperty VariantProperty = DependencyProperty.Register(
        nameof(Variant),
        typeof(CheckBoxVariant),
        typeof(CheckBox),
        new FrameworkPropertyMetadata(CheckBoxVariant.Horizontal),
        IsVariantValid
    );

    static CheckBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(CheckBox),
            new FrameworkPropertyMetadata(typeof(CheckBox))
        );
    }

    /// <summary>
    /// Gets or sets the optional icon displayed by the <see cref="CheckBoxVariant.Vertical" />
    /// layout.
    /// </summary>
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>Gets or sets the fixed check box layout.</summary>
    public CheckBoxVariant Variant
    {
        get => (CheckBoxVariant)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    private static bool IsVariantValid(object value)
    {
        return value is CheckBoxVariant variant && Enum.IsDefined(variant);
    }
}
