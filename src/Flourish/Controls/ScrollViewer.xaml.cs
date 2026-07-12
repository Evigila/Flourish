using System.Windows;
using WpfScrollViewer = System.Windows.Controls.ScrollViewer;

namespace ArkheideSystem.Flourish.Controls;

/// <summary>A Flourish-styled scrolling content host.</summary>
public class FlourishScrollViewer : WpfScrollViewer
{
    /// <summary>Identifies the <see cref="IsCompact" /> dependency property.</summary>
    public static readonly DependencyProperty IsCompactProperty = DependencyProperty.Register(
        nameof(IsCompact),
        typeof(bool),
        typeof(FlourishScrollViewer),
        new FrameworkPropertyMetadata(false)
    );

    static FlourishScrollViewer()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(FlourishScrollViewer),
            new FrameworkPropertyMetadata(typeof(FlourishScrollViewer))
        );
    }

    /// <summary>Gets or sets whether the viewer uses compact scroll bars.</summary>
    public bool IsCompact
    {
        get => (bool)GetValue(IsCompactProperty);
        set => SetValue(IsCompactProperty, value);
    }
}
