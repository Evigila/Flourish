using System;

using CKey = Arkheide.Essential.Culture.Key;
using Localizer = Arkheide.Essential.Culture.Localizer;
using System.Windows.Controls;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class DataGridPage : Page
{
    public DataGridPage()
    {
        InitializeComponent();
        MemberGrid.ItemsSource = new ControlMemberRow[]
        {
            new(
                "ItemsSource",
                CKey.Controls_SuppliesRowsThroughTheNativeWPFItemsContract_EFF3C048
            ),
            new("Columns", CKey.Controls_ContainsNativeDataGridColumnDefinitions_14066905),
            new(
                "AutoGenerateColumns",
                CKey.Controls_GeneratesColumnsFromItemPropertiesWhenEnabled_AD6A6621
            ),
            new("RowCount", CKey.Controls_ReportsDataRowsWithoutTheNewItemPlaceholder_072240D4),
            new("ColumnCount", CKey.Controls_ReportsDeclaredAndGeneratedColumns_580BD2E1),
            new(
                "FirstColumnForeground",
                CKey.Controls_SetsTheFirstDisplayedColumnColor_59F9C6AD
            ),
        };
        RefreshExampleRows();
        Loaded += Page_Loaded;
        Unloaded += Page_Unloaded;
    }

    private void Page_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        Localizer.Current.Changed -= Localization_Changed;
        Localizer.Current.Changed += Localization_Changed;
        RefreshExampleRows();
    }

    private void Page_Unloaded(object sender, System.Windows.RoutedEventArgs e)
    {
        Localizer.Current.Changed -= Localization_Changed;
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
                Localizer.Parse(CKey.Runtime_Ready_5FA7AAC5),
                Localizer.Parse(CKey.Runtime_Application_E7AD522E)
            ),
            new(
                Localizer.Parse(CKey.Controls_Reports_DACCA3CB),
                Localizer.Parse(CKey.Runtime_Running_F4CCAE29),
                Localizer.Parse(CKey.Controls_Workspace_87BB59BA)
            ),
            new(
                Localizer.Parse(CKey.Runtime_Archive_66F4804E),
                Localizer.Parse(CKey.Runtime_Paused_E159B061),
                Localizer.Parse(CKey.Runtime_System_6725E7BB)
            ),
        };
    }
}

public sealed record DataGridExampleRow(string Name, string Status, string Owner);
