using CKey = ArkheideSystem.Essential.Culture.Key;
using System.Windows.Controls;
using ArkheideSystem.Gallery.Flourish.WPF.Models;

namespace ArkheideSystem.Gallery.Flourish.WPF.Views;

public partial class TextBoxPage : Page
{
    public TextBoxPage()
    {
        InitializeComponent();
        MemberGrid.ItemsSource = new MemberRow[]
        {
            new("Text", CKey.Controls_GetsOrSetsTheEditableTextValue_607E5735),
            new(
                "IsReadOnly",
                CKey.Controls_PreventsEditsWhilePreservingSelectionAndCopying_AB44FBCC
            ),
            new(
                "AcceptsReturn",
                CKey.Controls_AllowsTheEnterKeyToInsertANewLine_578EB817
            ),
            new(
                "TextWrapping",
                CKey.Controls_WrapsLongTextWithinTheAvailableWidth_1FC91E99
            ),
            new(
                "MaxLength",
                CKey.Controls_LimitsTheNumberOfAcceptedCharacters_F99BE746
            ),
            new(
                "TextChanged",
                CKey.Controls_ReportsEditsMadeByTheUserOrApplication_91DF28A1
            ),
        };
    }

    public string UsageCode { get; } =
        """
            <flourish:TextBox
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
