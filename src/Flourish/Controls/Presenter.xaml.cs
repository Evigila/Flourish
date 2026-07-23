using System.Collections;
using System.Windows;
using System.Windows.Markup;
using WpfControl = System.Windows.Controls.Control;

namespace ArkheideSystem.Flourish.Controls;

/// <summary>
/// A presentation layout with required copy and an explicitly selected composition.
/// In the standard split composition, copy and body content occupy one side while the presented
/// visual occupies the other. The presentation surface fills its region and centers presentation
/// content, while body content aligns with the copy on the transparent side.
/// </summary>
/// <remarks>
/// Split and Overlay compositions occupy a complete row. TopDown is the only composition that
/// may be arranged in columns with peer Presenter controls.
/// </remarks>
[ContentProperty(nameof(Presentation))]
public class Presenter : WpfControl
{
    /// <summary>Identifies the <see cref="Title" /> dependency property.</summary>
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title),
        typeof(string),
        typeof(Presenter),
        new FrameworkPropertyMetadata(string.Empty)
    );

    /// <summary>Identifies the <see cref="Content" /> dependency property.</summary>
    public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(
        nameof(Content),
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
                global::ArkheideSystem.Flourish.Controls.PresenterPosition.Left
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

    /// <summary>Gets or sets the required presentation heading.</summary>
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Gets or sets the required supporting copy below the heading.</summary>
    public string? Content
    {
        get => (string?)GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    /// <summary>
    /// Gets or sets optional controls or supporting content arranged on the same side as the
    /// copy. This property must be assigned explicitly in XAML.
    /// </summary>
    public object? Body
    {
        get => GetValue(BodyProperty);
        set => SetValue(BodyProperty, value);
    }

    /// <summary>
    /// Gets or sets the image, icon group, illustration, or other content being presented. This
    /// is the default XAML content property.
    /// </summary>
    public object? Presentation
    {
        get => GetValue(PresentationProperty);
        set => SetValue(PresentationProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the presentation is split beside, stacked above, or placed behind
    /// the copy. Authors should assign this property explicitly; its runtime fallback is
    /// <see cref="ArkheideSystem.Flourish.Controls.PresenterMode.Split" />.
    /// </summary>
    public PresenterMode PresenterMode
    {
        get => (PresenterMode)GetValue(PresenterModeProperty);
        set => SetValue(PresenterModeProperty, value);
    }

    /// <summary>
    /// Gets or sets the presentation position in split mode. Authors should assign this property
    /// explicitly; its runtime fallback places the presentation on the left.
    /// </summary>
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
