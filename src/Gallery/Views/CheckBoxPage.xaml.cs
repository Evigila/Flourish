using System.Windows.Controls;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class CheckBoxPage : Page
{
    public CheckBoxPage()
    {
        InitializeComponent();
        MemberGrid.ItemsSource = new ControlMemberRow[]
        {
            new("IsChecked", "Gets or sets true, false, or null when three-state behavior is enabled."),
            new("IsThreeState", "Allows the control to enter the indeterminate state."),
            new("Content", "Supplies the visible option label."),
            new("Checked", "Reports a transition to the checked state."),
            new("Unchecked", "Reports a transition to the unchecked state."),
            new("Indeterminate", "Reports a transition to the null state."),
        };
    }

    public string UsageCode { get; } =
        """
        <flourish:FlourishCheckBox
          Content="Enable notifications"
          IsChecked="{Binding NotificationsEnabled, Mode=TwoWay}"
          Checked="Notifications_Changed"
          Unchecked="Notifications_Changed" />

        private void Notifications_Changed(object sender, RoutedEventArgs e)
        {
            SavePreferences();
        }
        """;
}
