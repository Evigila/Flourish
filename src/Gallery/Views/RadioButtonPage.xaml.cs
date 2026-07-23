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
            new("GroupName", "Associates mutually exclusive options across a logical container."),
            new("IsChecked", "Gets or sets whether this option is selected."),
            new("Content", "Supplies the visible option label."),
            new("Checked", "Reports selection of this option."),
            new("Command", "Invokes application-owned behavior when selected."),
            new("CommandParameter", "Supplies the selected option value to a command."),
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
