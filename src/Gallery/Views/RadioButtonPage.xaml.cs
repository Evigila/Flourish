using System.Windows.Controls;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class RadioButtonPage : Page
{
    public RadioButtonPage()
    {
        InitializeComponent();
        MemberGrid.ItemsSource = new ControlMemberRow[]
        {
            new(
                "GroupName",
                GalleryLocaleKeys.ControlsAssociatesMutuallyExclusiveOptionsAcrossALogicalContainer_21819671
            ),
            new(
                "IsChecked",
                GalleryLocaleKeys.ControlsGetsOrSetsWhetherThisOptionIsSelected_2A781960
            ),
            new("Content", GalleryLocaleKeys.ControlsSuppliesTheVisibleOptionLabel_2FF959F7),
            new("Checked", GalleryLocaleKeys.ControlsReportsSelectionOfThisOption_6027BFAA),
            new(
                "Command",
                GalleryLocaleKeys.ControlsInvokesApplicationOwnedBehaviorWhenSelected_DE302977
            ),
            new(
                "CommandParameter",
                GalleryLocaleKeys.ControlsSuppliesTheSelectedOptionValueToACommand_F7AB29C1
            ),
        };
    }

    public string UsageCode { get; } =
        """
            <StackPanel>
              <flourish:FlourishRadioButton
                Content="Light"
                GroupName="Theme"
                IsChecked="{Binding UseLightTheme}" />
              <flourish:FlourishRadioButton
                Content="Dark"
                GroupName="Theme"
                IsChecked="{Binding UseDarkTheme}" />
            </StackPanel>

            // GroupName keeps the options mutually exclusive at runtime.
            """;
}
