using System.Collections;
using System.Windows;
using System.Windows.Markup;
using WpfControl = System.Windows.Controls.Control;

namespace ArkheideSystem.Flourish.Controls;

/// <summary>
/// Defines a full-width page section with a required title and body plus optional supporting content.
/// </summary>
[ContentProperty(nameof(Body))]
public class Chunk : WpfControl
{
    private static readonly Thickness DefaultChunkMargin = new(0, 32, 0, 0);
    private static readonly Thickness DefaultChunkSpacing = new(0, 12, 0, 0);

    /// <summary>Identifies the <see cref="Title" /> dependency property.</summary>
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title),
        typeof(string),
        typeof(Chunk),
        new FrameworkPropertyMetadata(string.Empty)
    );

    /// <summary>Identifies the <see cref="Content" /> dependency property.</summary>
    public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(
        nameof(Content),
        typeof(string),
        typeof(Chunk),
        new FrameworkPropertyMetadata(null)
    );

    /// <summary>Identifies the <see cref="ChunkMargin" /> dependency property.</summary>
    public static readonly DependencyProperty ChunkMarginProperty = DependencyProperty.Register(
        nameof(ChunkMargin),
        typeof(Thickness),
        typeof(Chunk),
        new FrameworkPropertyMetadata(DefaultChunkMargin)
    );

    /// <summary>Identifies the <see cref="ChunkSpacing" /> dependency property.</summary>
    public static readonly DependencyProperty ChunkSpacingProperty =
        DependencyProperty.Register(
            nameof(ChunkSpacing),
            typeof(Thickness),
            typeof(Chunk),
            new FrameworkPropertyMetadata(DefaultChunkSpacing)
        );

    /// <summary>Identifies the <see cref="Body" /> dependency property.</summary>
    public static readonly DependencyProperty BodyProperty = DependencyProperty.Register(
        nameof(Body),
        typeof(object),
        typeof(Chunk),
        new FrameworkPropertyMetadata(null, OnBodyChanged)
    );

    static Chunk()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(Chunk),
            new FrameworkPropertyMetadata(typeof(Chunk))
        );
    }

    /// <summary>Gets or sets the required section heading.</summary>
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Gets or sets optional supporting information not covered by the title.</summary>
    public string? Content
    {
        get => (string?)GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    /// <summary>Gets or sets the space before this section.</summary>
    public Thickness ChunkMargin
    {
        get => (Thickness)GetValue(ChunkMarginProperty);
        set => SetValue(ChunkMarginProperty, value);
    }

    /// <summary>Gets or sets the spacing between the section header regions and body.</summary>
    public Thickness ChunkSpacing
    {
        get => (Thickness)GetValue(ChunkSpacingProperty);
        set => SetValue(ChunkSpacingProperty, value);
    }

    /// <summary>Gets or sets the required content presented by the section.</summary>
    public object? Body
    {
        get => GetValue(BodyProperty);
        set => SetValue(BodyProperty, value);
    }

    /// <inheritdoc />
    protected override IEnumerator LogicalChildren => EnumerateLogicalChildren();

    private static void OnBodyChanged(
        DependencyObject dependencyObject,
        DependencyPropertyChangedEventArgs eventArgs
    )
    {
        var chunk = (Chunk)dependencyObject;
        if (eventArgs.OldValue is not null)
        {
            chunk.RemoveLogicalChild(eventArgs.OldValue);
        }

        if (eventArgs.NewValue is not null)
        {
            chunk.AddLogicalChild(eventArgs.NewValue);
        }
    }

    private IEnumerator EnumerateLogicalChildren()
    {
        if (Body is not null)
        {
            yield return Body;
        }
    }
}
