using System.Windows;

namespace ArkheideSystem.Flourish.Controls;

/// <summary>
/// A normalized Large-size text paragraph intended for use as a direct child of a
/// <see cref="Document" />.
/// </summary>
public class Paragraph : FlourishTextBlock
{
    static Paragraph()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(Paragraph),
            new FrameworkPropertyMetadata(typeof(Paragraph))
        );
    }
}
