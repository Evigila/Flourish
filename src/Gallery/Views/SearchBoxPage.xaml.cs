using CKey = ArkheideSystem.Essential.Culture.Key;
using InputKey = System.Windows.Input.Key;
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
                CKey.Controls_DisplaysAnInControlHintWhileTheQueryIsEmptyAndUnfocused_E3A4B8DC
            ),
            new("Text", CKey.Controls_GetsOrSetsTheCurrentSearchQuery_8E559920),
            new(
                "IsReadOnly",
                CKey.Controls_PreventsQueryEditsWhilePreservingSelection_3D0FAB32
            ),
            new("MaxLength", CKey.Controls_LimitsTheAcceptedQueryLength_715D6B09),
            new("TextChanged", CKey.Controls_ReportsEachQueryUpdate_4FBCC3DE),
            new(
                "CommandBindings",
                CKey.Controls_ConnectsKeyboardGesturesSuchAsEnterToApplicationSearch_537F6EA1
            ),
        };
    }

    public string UsageCode { get; } =
        """
            <flourish:SearchBox
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
