using System.Windows;

namespace ArkheideSystem.Flourish.Controls;

/// <summary>A themed button that presents an icon with optional content.</summary>
public class IconButton : Button
{
    /// <summary>Identifies the <see cref="Icon" /> dependency property.</summary>
    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon),
        typeof(object),
        typeof(IconButton),
        new FrameworkPropertyMetadata(null)
    );

    static IconButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(IconButton),
            new FrameworkPropertyMetadata(typeof(IconButton))
        );
    }

    /// <summary>Gets or sets the icon content displayed before the button content.</summary>
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
}
