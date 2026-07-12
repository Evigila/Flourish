using System.Windows;
using WpfLabel = System.Windows.Controls.Label;

namespace ArkheideSystem.Flourish.Controls;

/// <summary>A Flourish-styled content label with native access-key support.</summary>
public class FlourishLabel : WpfLabel
{
    static FlourishLabel()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(FlourishLabel),
            new FrameworkPropertyMetadata(typeof(FlourishLabel))
        );
    }
}
