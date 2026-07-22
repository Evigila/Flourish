using System.Windows.Controls;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class PresenterPage : Page
{
    public IReadOnlyList<ControlMemberRow> Properties { get; } =
    [
        new("Title", "Required explicit heading for the presentation."),
        new("Content", "Required explicit supporting copy below the heading."),
        new("Body", "Explicitly hosts controls left-aligned with the copy."),
        new("Presentation", "Default XAML content centered in the rounded Split presentation surface."),
        new("PresenterMode", "Required explicit Split, Overlay, or TopDown composition."),
        new("PresenterPosition", "Places Split presentation content on the left or right; other modes ignore it."),
    ];

    public PresenterPage()
    {
        InitializeComponent();
    }
}
