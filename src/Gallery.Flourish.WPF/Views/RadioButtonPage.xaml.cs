using CKey = ArkheideSystem.Essential.Culture.Key;
using System.Windows.Controls;
using ArkheideSystem.Gallery.Flourish.WPF.Models;

namespace ArkheideSystem.Gallery.Flourish.WPF.Views;

public partial class RadioButtonPage : Page
{
    public RadioButtonPage()
    {
        InitializeComponent();
        MemberGrid.ItemsSource = new MemberRow[]
        {
            new(
                "GroupName",
                CKey.Controls_AssociatesMutuallyExclusiveOptionsAcrossALogicalContainer_21819671
            ),
            new(
                "IsChecked",
                CKey.Controls_GetsOrSetsWhetherThisOptionIsSelected_2A781960
            ),
            new("Content", CKey.Controls_SuppliesTheVisibleOptionLabel_2FF959F7),
            new("Checked", CKey.Controls_ReportsSelectionOfThisOption_6027BFAA),
            new(
                "Command",
                CKey.Controls_InvokesApplicationOwnedBehaviorWhenSelected_DE302977
            ),
            new(
                "CommandParameter",
                CKey.Controls_SuppliesTheSelectedOptionValueToACommand_F7AB29C1
            ),
        };
    }

    public string UsageCode { get; } =
        """
            <StackPanel>
              <flourish:RadioButton
                Content="Light"
                GroupName="Theme"
                IsChecked="{Binding UseLightTheme}" />
              <flourish:RadioButton
                Content="Dark"
                GroupName="Theme"
                IsChecked="{Binding UseDarkTheme}" />
            </StackPanel>

            // GroupName keeps the options mutually exclusive at runtime.
            """;
}
