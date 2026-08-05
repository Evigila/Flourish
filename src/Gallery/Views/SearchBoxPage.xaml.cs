using System.Windows.Controls;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class SearchBoxPage : Page
{
    public SearchBoxPage()
    {
        InitializeComponent();
        MemberGrid.ItemsSource = new ControlMemberRow[]
        {
            new(
                "Placeholder",
                GalleryLocaleKeys.ControlsDisplaysAnInControlHintWhileTheQueryIsEmptyAndUnfocused_E3A4B8DC
            ),
            new("Text", GalleryLocaleKeys.ControlsGetsOrSetsTheCurrentSearchQuery_8E559920),
            new(
                "IsReadOnly",
                GalleryLocaleKeys.ControlsPreventsQueryEditsWhilePreservingSelection_3D0FAB32
            ),
            new("MaxLength", GalleryLocaleKeys.ControlsLimitsTheAcceptedQueryLength_715D6B09),
            new("TextChanged", GalleryLocaleKeys.ControlsReportsEachQueryUpdate_4FBCC3DE),
            new(
                "CommandBindings",
                GalleryLocaleKeys.ControlsConnectsKeyboardGesturesSuchAsEnterToApplicationSearch_537F6EA1
            ),
        };
    }

    public string UsageCode { get; } =
        """
            <flourish:FlourishSearchBox
              x:Name="ControlSearch"
              Placeholder="Search controls"
              Text="{Binding Query, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
              KeyDown="ControlSearch_KeyDown" />

            private void ControlSearch_KeyDown(object sender, KeyEventArgs e)
            {
                if (e.Key == Key.Enter)
                    SearchCommand.Execute(ControlSearch.Text);
            }
            """;
}
