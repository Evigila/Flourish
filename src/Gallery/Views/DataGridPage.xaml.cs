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
            new("ItemsSource", "Supplies rows through the native WPF items contract."),
            new("Columns", "Contains native DataGridColumn definitions."),
            new("AutoGenerateColumns", "Generates columns from item properties when enabled."),
            new("RowCount", "Reports data rows without the new-item placeholder."),
            new("ColumnCount", "Reports declared and generated columns."),
            new("FirstColumnForeground", "Sets the first displayed column color."),
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
            new("Foobar", localization.Get("Ready"), localization.Get("Application")),
            new(
                localization.Get("Reports"),
                localization.Get("Running"),
                localization.Get("Workspace")
            ),
            new(
                localization.Get("Archive"),
                localization.Get("Paused"),
                localization.Get("System")
            ),
        };
    }
}

public sealed record DataGridExampleRow(string Name, string Status, string Owner);
