using System.Windows;
using WpfToolTip = System.Windows.Controls.ToolTip;

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
        ToolTipProperty.OverrideMetadata(
            typeof(IconButton),
            new FrameworkPropertyMetadata(null, OnToolTipChanged)
        );
    }

    /// <summary>Gets or sets the icon content displayed before the button content.</summary>
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    private static void OnToolTipChanged(
        DependencyObject element,
        DependencyPropertyChangedEventArgs e
    )
    {
        if (
            element is not IconButton iconButton
            || e.NewValue is null
            || e.NewValue is WpfToolTip
        )
        {
            return;
        }

        iconButton.SetCurrentValue(
            ToolTipProperty,
            new FlourishToolTip { Content = e.NewValue }
        );
    }
}
