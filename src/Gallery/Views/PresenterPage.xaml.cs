using System.Windows.Controls;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class PresenterPage : Page
{
    public IReadOnlyList<ControlMemberRow> Properties { get; } =
    [
        new("Title", "Required explicit heading for the presentation."),
        new("Description", "Required explicit supporting copy below the heading."),
        new("Body", "Explicitly hosts controls left-aligned with the copy."),
        new("Presentation", "Default XAML content centered in the rounded Split presentation surface."),
        new("PresenterMode", "Required explicit Split or Overlay composition."),
        new("PresenterPosition", "Required explicit placement of Split presentation content on the left or right."),
    ];

    public PresenterPage()
    {
        InitializeComponent();
    }
}
