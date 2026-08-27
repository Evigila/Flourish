using ArkheideSystem.Flourish.Abstract;
using System.Windows;
using WpfLabel = System.Windows.Controls.Label;

namespace ArkheideSystem.Flourish.Controls;

/// <summary>A Flourish-styled content label with native access-key support.</summary>
public class Label : WpfLabel
{
    static Label()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(Label),
            new FrameworkPropertyMetadata(typeof(Label))
        );
    }
}
