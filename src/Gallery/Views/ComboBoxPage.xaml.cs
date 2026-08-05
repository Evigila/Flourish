using System.Collections.ObjectModel;
using System.Windows.Controls;
using ArkheideSystem.Gallery.Localization;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class ComboBoxPage : Page
{
    private readonly IGalleryLocalization localization;

    public ComboBoxPage(IGalleryLocalization localization)
    {
        this.localization = localization;
        RefreshDensityOptions();
        InitializeComponent();
        MemberGrid.ItemsSource = new ControlMemberRow[]
        {
            new("ItemsSource", "Supplies application-owned option data."),
            new("Items", "Contains options declared directly in XAML or code."),
            new("SelectedItem", "Gets or sets the selected data item."),
            new("SelectedIndex", "Gets or sets the selected zero-based index."),
            new("DisplayMemberPath", "Selects the property displayed for each data item."),
            new("SelectionChanged", "Reports added and removed selections."),
            new("HoverReveal.IsEnabled", "Controls pointer-reveal feedback on the closed selector."),
        };
        Loaded += Page_Loaded;
        Unloaded += Page_Unloaded;
    }

    public ObservableCollection<string> DensityOptions { get; } = [];

    private void Page_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        localization.Changed -= Localization_Changed;
        localization.Changed += Localization_Changed;
        RefreshDensityOptions();
    }

    private void Page_Unloaded(object sender, System.Windows.RoutedEventArgs e)
    {
        localization.Changed -= Localization_Changed;
    }

    private void Localization_Changed(object? sender, EventArgs e)
    {
        RefreshDensityOptions();
    }

    private void RefreshDensityOptions()
    {
        DensityOptions.Clear();
        DensityOptions.Add(localization.Get("Comfortable"));
        DensityOptions.Add(localization.Get("Compact"));
    }

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
