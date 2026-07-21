using System.Windows.Controls;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class PresenterPage : Page
{
    public IReadOnlyList<ControlMemberRow> Properties { get; } =
    [
        new("Title", "Sets the optional presentation heading."),
        new("Description", "Adds optional supporting copy."),
        new("Body", "Hosts controls arranged in the same region as the copy."),
        new("Presentation", "Hosts an image, illustration, icon group, or other presented content."),
        new("PresenterMode", "Chooses Split or Overlay composition."),
        new("PresenterPosition", "Places Split presentation content on the left or right."),
    ];

    public PresenterPage()
    {
        InitializeComponent();
    }
}
