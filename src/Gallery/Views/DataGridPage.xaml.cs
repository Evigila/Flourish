using System.Windows.Controls;
using ArkheideSystem.Gallery.Localization;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class DataGridPage : Page
{
    private readonly IGalleryLocalization localization;

    public DataGridPage(IGalleryLocalization localization)
    {
        this.localization = localization;
        InitializeComponent();
        MemberGrid.ItemsSource = new ControlMemberRow[]
        {
            new(
                "ItemsSource",
                GalleryLocaleKeys.ControlsSuppliesRowsThroughTheNativeWPFItemsContract_EFF3C048
            ),
            new(
                "Columns",
                GalleryLocaleKeys.ControlsContainsNativeDataGridColumnDefinitions_14066905
            ),
            new(
                "AutoGenerateColumns",
                GalleryLocaleKeys.ControlsGeneratesColumnsFromItemPropertiesWhenEnabled_AD6A6621
            ),
            new(
                "RowCount",
                GalleryLocaleKeys.ControlsReportsDataRowsWithoutTheNewItemPlaceholder_072240D4
            ),
            new(
                "ColumnCount",
                GalleryLocaleKeys.ControlsReportsDeclaredAndGeneratedColumns_580BD2E1
            ),
            new(
                "FirstColumnForeground",
                GalleryLocaleKeys.ControlsSetsTheFirstDisplayedColumnColor_59F9C6AD
            ),
        };
        RefreshExampleRows();
        Loaded += Page_Loaded;
        Unloaded += Page_Unloaded;
    }

    private void Page_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        localization.Changed -= Localization_Changed;
        localization.Changed += Localization_Changed;
        RefreshExampleRows();
    }

    private void Page_Unloaded(object sender, System.Windows.RoutedEventArgs e)
    {
        localization.Changed -= Localization_Changed;
    }

    private void Localization_Changed(object? sender, EventArgs e)
    {
        RefreshExampleRows();
    }

    private void RefreshExampleRows()
    {
        ExampleGrid.ItemsSource = new DataGridExampleRow[]
        {
            new(
                "Foobar",
                localization.Get(GalleryLocaleKeys.RuntimeReady_5FA7AAC5),
                localization.Get(GalleryLocaleKeys.RuntimeApplication_E7AD522E)
            ),
            new(
                localization.Get(GalleryLocaleKeys.ControlsReports_DACCA3CB),
                localization.Get(GalleryLocaleKeys.RuntimeRunning_F4CCAE29),
                localization.Get(GalleryLocaleKeys.ControlsWorkspace_87BB59BA)
            ),
            new(
                localization.Get(GalleryLocaleKeys.RuntimeArchive_66F4804E),
                localization.Get(GalleryLocaleKeys.RuntimePaused_E159B061),
                localization.Get(GalleryLocaleKeys.RuntimeSystem_6725E7BB)
            ),
        };
    }
}

public sealed record DataGridExampleRow(string Name, string Status, string Owner);
