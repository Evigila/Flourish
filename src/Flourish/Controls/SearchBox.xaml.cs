using System.Windows;

namespace ArkheideSystem.Flourish.Controls;

/// <summary>
/// A compact search text box with a search glyph and an in-control placeholder.
/// </summary>
public class FlourishSearchBox : FlourishTextBox
{
    /// <summary>Identifies the <see cref="Placeholder" /> dependency property.</summary>
    public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.Register(
        nameof(Placeholder),
        typeof(string),
        typeof(FlourishSearchBox),
        new FrameworkPropertyMetadata(string.Empty)
    );

    static FlourishSearchBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(FlourishSearchBox),
            new FrameworkPropertyMetadata(typeof(FlourishSearchBox))
        );
    }

    /// <summary>Gets or sets the hint displayed while the search box is empty and unfocused.</summary>
    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }
}
