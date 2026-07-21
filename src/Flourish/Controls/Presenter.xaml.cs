using System.Collections;
using System.Windows;
using System.Windows.Markup;
using WpfControl = System.Windows.Controls.Control;

namespace ArkheideSystem.Flourish.Controls;

/// <summary>
/// A full-width presentation layout with copy and supporting body content beside or over a
/// flexible presentation region.
/// </summary>
[ContentProperty(nameof(Body))]
public class Presenter : WpfControl
{
    /// <summary>Identifies the <see cref="Title" /> dependency property.</summary>
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title),
        typeof(string),
        typeof(Presenter),
        new FrameworkPropertyMetadata(string.Empty)
    );

    /// <summary>Identifies the <see cref="Description" /> dependency property.</summary>
    public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
        nameof(Description),
        typeof(string),
        typeof(Presenter),
        new FrameworkPropertyMetadata(null)
    );

    /// <summary>Identifies the <see cref="Body" /> dependency property.</summary>
    public static readonly DependencyProperty BodyProperty = DependencyProperty.Register(
        nameof(Body),
        typeof(object),
        typeof(Presenter),
        new FrameworkPropertyMetadata(null, OnLogicalContentChanged)
    );

    /// <summary>Identifies the <see cref="Presentation" /> dependency property.</summary>
    public static readonly DependencyProperty PresentationProperty = DependencyProperty.Register(
        nameof(Presentation),
        typeof(object),
        typeof(Presenter),
        new FrameworkPropertyMetadata(null, OnLogicalContentChanged)
    );

    /// <summary>Identifies the <see cref="PresenterMode" /> dependency property.</summary>
    public static readonly DependencyProperty PresenterModeProperty =
        DependencyProperty.Register(
            nameof(PresenterMode),
            typeof(PresenterMode),
            typeof(Presenter),
            new FrameworkPropertyMetadata(
                global::ArkheideSystem.Flourish.Controls.PresenterMode.Split
            ),
            IsPresenterModeValid
        );

    /// <summary>Identifies the <see cref="PresenterPosition" /> dependency property.</summary>
    public static readonly DependencyProperty PresenterPositionProperty =
        DependencyProperty.Register(
            nameof(PresenterPosition),
            typeof(PresenterPosition),
            typeof(Presenter),
            new FrameworkPropertyMetadata(
                global::ArkheideSystem.Flourish.Controls.PresenterPosition.Right
            ),
            IsPresenterPositionValid
        );

    static Presenter()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(Presenter),
            new FrameworkPropertyMetadata(typeof(Presenter))
        );
    }

    /// <summary>Gets or sets the optional presentation heading.</summary>
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Gets or sets optional supporting copy below the heading.</summary>
    public string? Description
    {
        get => (string?)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    /// <summary>Gets or sets controls or supporting content arranged with the copy.</summary>
    public object? Body
    {
        get => GetValue(BodyProperty);
        set => SetValue(BodyProperty, value);
    }

    /// <summary>
    /// Gets or sets the image, icon group, illustration, or other content being presented.
    /// </summary>
    public object? Presentation
    {
        get => GetValue(PresentationProperty);
        set => SetValue(PresentationProperty, value);
    }

    /// <summary>Gets or sets whether the presentation is split from or behind the copy.</summary>
    public PresenterMode PresenterMode
    {
        get => (PresenterMode)GetValue(PresenterModeProperty);
        set => SetValue(PresenterModeProperty, value);
    }

    /// <summary>Gets or sets the presentation position in split mode.</summary>
    public PresenterPosition PresenterPosition
    {
        get => (PresenterPosition)GetValue(PresenterPositionProperty);
        set => SetValue(PresenterPositionProperty, value);
    }

    /// <inheritdoc />
    protected override IEnumerator LogicalChildren => EnumerateLogicalChildren();

    private static bool IsPresenterModeValid(object value)
    {
        return value is PresenterMode mode && Enum.IsDefined(mode);
    }

    private static bool IsPresenterPositionValid(object value)
    {
        return value is PresenterPosition position && Enum.IsDefined(position);
    }

    private static void OnLogicalContentChanged(
        DependencyObject dependencyObject,
        DependencyPropertyChangedEventArgs eventArgs
    )
    {
        var presenter = (Presenter)dependencyObject;
        if (eventArgs.OldValue is not null)
        {
            presenter.RemoveLogicalChild(eventArgs.OldValue);
        }

        if (eventArgs.NewValue is not null)
        {
            presenter.AddLogicalChild(eventArgs.NewValue);
        }
    }

    private IEnumerator EnumerateLogicalChildren()
    {
        if (Body is not null)
        {
            yield return Body;
        }

        if (Presentation is not null)
        {
            yield return Presentation;
        }
    }
}
