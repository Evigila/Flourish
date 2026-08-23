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
                Key.Controls_DisplaysAnInControlHintWhileTheQueryIsEmptyAndUnfocused_E3A4B8DC
            ),
            new("Text", Key.Controls_GetsOrSetsTheCurrentSearchQuery_8E559920),
            new(
                "IsReadOnly",
                Key.Controls_PreventsQueryEditsWhilePreservingSelection_3D0FAB32
            ),
            new("MaxLength", Key.Controls_LimitsTheAcceptedQueryLength_715D6B09),
            new("TextChanged", Key.Controls_ReportsEachQueryUpdate_4FBCC3DE),
            new(
                "CommandBindings",
                Key.Controls_ConnectsKeyboardGesturesSuchAsEnterToApplicationSearch_537F6EA1
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
                if (e.Key == InputKey.Enter)
                    SearchCommand.Execute(ControlSearch.Text);
            }
            """;
}
