using System.Windows.Controls;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class TextBoxPage : Page
{
    public TextBoxPage()
    {
        InitializeComponent();
        MemberGrid.ItemsSource = new ControlMemberRow[]
        {
            new("Text", Key.Controls_GetsOrSetsTheEditableTextValue_607E5735),
            new(
                "IsReadOnly",
                Key.Controls_PreventsEditsWhilePreservingSelectionAndCopying_AB44FBCC
            ),
            new(
                "AcceptsReturn",
                Key.Controls_AllowsTheEnterKeyToInsertANewLine_578EB817
            ),
            new(
                "TextWrapping",
                Key.Controls_WrapsLongTextWithinTheAvailableWidth_1FC91E99
            ),
            new(
                "MaxLength",
                Key.Controls_LimitsTheNumberOfAcceptedCharacters_F99BE746
            ),
            new(
                "TextChanged",
                Key.Controls_ReportsEditsMadeByTheUserOrApplication_91DF28A1
            ),
        };
    }

    public string UsageCode { get; } =
        """
            <flourish:FlourishTextBox
              Text="{Binding DisplayName, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
              MaxLength="80" />

            private void SetName(string value)
            {
                NameBox.Text = value;
                NameBox.SelectAll();
                NameBox.Focus();
            }
            """;
}
