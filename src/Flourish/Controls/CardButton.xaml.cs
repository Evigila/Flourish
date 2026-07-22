using System.Windows;
using System.Windows.Controls;

namespace ArkheideSystem.Flourish.Controls;

/// <summary>An interactive card with an icon, title, and descriptive content.</summary>
public class CardButton : Button
{
    /// <summary>Identifies the <see cref="IconPosition" /> dependency property.</summary>
    public static readonly DependencyProperty IconPositionProperty = DependencyProperty.Register(
        nameof(IconPosition),
        typeof(Dock),
        typeof(CardButton),
        new FrameworkPropertyMetadata(Dock.Top),
        value => value is Dock position && Enum.IsDefined(position)
    );

    /// <summary>Identifies the <see cref="Title" /> dependency property.</summary>
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title),
        typeof(string),
        typeof(CardButton),
        new FrameworkPropertyMetadata(string.Empty)
    );

    static CardButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(CardButton),
            new FrameworkPropertyMetadata(typeof(CardButton))
        );
        VariantProperty.OverrideMetadata(
            typeof(CardButton),
            new FrameworkPropertyMetadata(ButtonVariant.Standard)
        );
    }

    /// <summary>Gets or sets where the icon appears relative to the card text.</summary>
    public Dock IconPosition
    {
        get => (Dock)GetValue(IconPositionProperty);
        set => SetValue(IconPositionProperty, value);
    }

    /// <summary>Gets or sets the card heading.</summary>
    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }
}
