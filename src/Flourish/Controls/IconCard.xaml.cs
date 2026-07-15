using System.Collections;
using System.Windows;

namespace ArkheideSystem.Flourish.Controls;

/// <summary>
/// A card that presents an icon or image beside, or behind, its textual content.
/// </summary>
public class IconCard : Card
{
    /// <summary>Identifies the <see cref="Presenter" /> dependency property.</summary>
    public static readonly DependencyProperty PresenterProperty = DependencyProperty.Register(
        nameof(Presenter),
        typeof(object),
        typeof(IconCard),
        new FrameworkPropertyMetadata(null, OnPresenterChanged)
    );

    /// <summary>Identifies the <see cref="PresenterMode" /> dependency property.</summary>
    public static readonly DependencyProperty PresenterModeProperty =
        DependencyProperty.Register(
            nameof(PresenterMode),
            typeof(PresenterMode),
            typeof(IconCard),
            new FrameworkPropertyMetadata(PresenterMode.Split),
            IsPresenterModeValid
        );

    /// <summary>Identifies the <see cref="PresenterPosition" /> dependency property.</summary>
    public static readonly DependencyProperty PresenterPositionProperty =
        DependencyProperty.Register(
            nameof(PresenterPosition),
            typeof(PresenterPosition),
            typeof(IconCard),
            new FrameworkPropertyMetadata(PresenterPosition.Left),
            IsPresenterPositionValid
        );

    static IconCard()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(IconCard),
            new FrameworkPropertyMetadata(typeof(IconCard))
        );
    }

    /// <summary>Gets or sets the icon, image, or custom visual presented by the card.</summary>
    public object? Presenter
    {
        get => GetValue(PresenterProperty);
        set => SetValue(PresenterProperty, value);
    }

    /// <summary>Gets or sets how the presenter is arranged with the textual content.</summary>
    public PresenterMode PresenterMode
    {
        get => (PresenterMode)GetValue(PresenterModeProperty);
        set => SetValue(PresenterModeProperty, value);
    }

    /// <summary>
    /// Gets or sets the presenter's position in split mode. Overlay mode ignores this value.
    /// </summary>
    public PresenterPosition PresenterPosition
    {
        get => (PresenterPosition)GetValue(PresenterPositionProperty);
        set => SetValue(PresenterPositionProperty, value);
    }

    /// <inheritdoc />
    protected override IEnumerator LogicalChildren => EnumerateLogicalChildren();

    private static void OnPresenterChanged(
        DependencyObject dependencyObject,
        DependencyPropertyChangedEventArgs eventArgs
    )
    {
        var card = (IconCard)dependencyObject;
        if (eventArgs.OldValue is not null)
        {
            card.RemoveLogicalChild(eventArgs.OldValue);
        }

        if (eventArgs.NewValue is not null)
        {
            card.AddLogicalChild(eventArgs.NewValue);
        }
    }

    private static bool IsPresenterModeValid(object value)
    {
        return value is PresenterMode mode && Enum.IsDefined(mode);
    }

    private static bool IsPresenterPositionValid(object value)
    {
        return value is PresenterPosition position && Enum.IsDefined(position);
    }

    private IEnumerator EnumerateLogicalChildren()
    {
        var baseChildren = base.LogicalChildren;
        while (baseChildren.MoveNext())
        {
            yield return baseChildren.Current;
        }

        if (Presenter is not null)
        {
            yield return Presenter;
        }
    }
}
