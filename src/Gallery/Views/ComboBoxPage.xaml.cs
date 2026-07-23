using System.Windows.Controls;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class ComboBoxPage : Page
{
    public ComboBoxPage()
    {
        InitializeComponent();
        MemberGrid.ItemsSource = new ControlMemberRow[]
        {
            new("ItemsSource", "Supplies application-owned option data."),
            new("Items", "Contains options declared directly in XAML or code."),
            new("SelectedItem", "Gets or sets the selected data item."),
            new("SelectedIndex", "Gets or sets the selected zero-based index."),
            new("DisplayMemberPath", "Selects the property displayed for each data item."),
            new("SelectionChanged", "Reports added and removed selections."),
        };
    }

    public string[] DensityOptions { get; } = ["Comfortable", "Compact"];

    public string UsageCode { get; } =
        """
        <flourish:FlourishComboBox
          ItemsSource="{Binding ThemeOptions}"
          SelectedItem="{Binding Theme, Mode=TwoWay}"
          DisplayMemberPath="DisplayName"
          SelectionChanged="Theme_SelectionChanged" />

        private void Theme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SavePreferences();
        }
        """;
}
