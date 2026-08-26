using System;

using CKey = Arkheide.Essential.Culture.Key;
using Localizer = Arkheide.Essential.Culture.Localizer;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class ComboBoxPage : Page
{
    public ComboBoxPage()
    {
        RefreshDensityOptions();
        InitializeComponent();
        MemberGrid.ItemsSource = new ControlMemberRow[]
        {
            new("ItemsSource", CKey.Controls_SuppliesApplicationOwnedOptionData_13C18B8E),
            new("Items", CKey.Controls_ContainsOptionsDeclaredDirectlyInXAMLOrCode_6FAC0DAA),
            new("SelectedItem", CKey.Controls_GetsOrSetsTheSelectedDataItem_56379BE7),
            new("SelectedIndex", CKey.Controls_GetsOrSetsTheSelectedZeroBasedIndex_8AED634D),
            new(
                "DisplayMemberPath",
                CKey.Controls_SelectsThePropertyDisplayedForEachDataItem_0F59DE20
            ),
            new("SelectionChanged", CKey.Controls_ReportsAddedAndRemovedSelections_CBA4EF2F),
            new(
                "HoverReveal.IsEnabled",
                CKey.Controls_ControlsPointerRevealFeedbackOnTheClosedSelector_CF5469E0
            ),
        };
        Loaded += Page_Loaded;
        Unloaded += Page_Unloaded;
    }

    public ObservableCollection<string> DensityOptions { get; } = [];

    private void Page_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        Localizer.Current.Changed -= Localization_Changed;
        Localizer.Current.Changed += Localization_Changed;
        RefreshDensityOptions();
    }

    private void Page_Unloaded(object sender, System.Windows.RoutedEventArgs e)
    {
        Localizer.Current.Changed -= Localization_Changed;
    }

    private void Localization_Changed(object? sender, EventArgs e)
    {
        RefreshDensityOptions();
    }

    private void RefreshDensityOptions()
    {
        DensityOptions.Clear();
        DensityOptions.Add(Localizer.Parse(CKey.Controls_Comfortable_459A23A5));
        DensityOptions.Add(Localizer.Parse(CKey.Controls_Compact_99452646));
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
