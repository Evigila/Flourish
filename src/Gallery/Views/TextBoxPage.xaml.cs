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
            new("Text", GalleryLocaleKeys.ControlsGetsOrSetsTheEditableTextValue_607E5735),
            new(
                "IsReadOnly",
                GalleryLocaleKeys.ControlsPreventsEditsWhilePreservingSelectionAndCopying_AB44FBCC
            ),
            new(
                "AcceptsReturn",
                GalleryLocaleKeys.ControlsAllowsTheEnterKeyToInsertANewLine_578EB817
            ),
            new(
                "TextWrapping",
                GalleryLocaleKeys.ControlsWrapsLongTextWithinTheAvailableWidth_1FC91E99
            ),
            new(
                "MaxLength",
                GalleryLocaleKeys.ControlsLimitsTheNumberOfAcceptedCharacters_F99BE746
            ),
            new(
                "TextChanged",
                GalleryLocaleKeys.ControlsReportsEditsMadeByTheUserOrApplication_91DF28A1
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
