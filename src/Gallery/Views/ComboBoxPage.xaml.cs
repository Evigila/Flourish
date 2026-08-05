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
            new(
                "ItemsSource",
                GalleryLocaleKeys.ControlsSuppliesApplicationOwnedOptionData_13C18B8E
            ),
            new(
                "Items",
                GalleryLocaleKeys.ControlsContainsOptionsDeclaredDirectlyInXAMLOrCode_6FAC0DAA
            ),
            new("SelectedItem", GalleryLocaleKeys.ControlsGetsOrSetsTheSelectedDataItem_56379BE7),
            new(
                "SelectedIndex",
                GalleryLocaleKeys.ControlsGetsOrSetsTheSelectedZeroBasedIndex_8AED634D
            ),
            new(
                "DisplayMemberPath",
                GalleryLocaleKeys.ControlsSelectsThePropertyDisplayedForEachDataItem_0F59DE20
            ),
            new(
                "SelectionChanged",
                GalleryLocaleKeys.ControlsReportsAddedAndRemovedSelections_CBA4EF2F
            ),
            new(
                "HoverReveal.IsEnabled",
                GalleryLocaleKeys.ControlsControlsPointerRevealFeedbackOnTheClosedSelector_CF5469E0
            ),
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
        DensityOptions.Add(localization.Get(GalleryLocaleKeys.ControlsComfortable_459A23A5));
        DensityOptions.Add(localization.Get(GalleryLocaleKeys.ControlsCompact_99452646));
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
