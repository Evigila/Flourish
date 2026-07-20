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
            new("ItemsSource", "Supplies rows through the native WPF items contract."),
            new("Columns", "Contains native DataGridColumn definitions."),
            new("AutoGenerateColumns", "Generates columns from item properties when enabled."),
            new("RowCount", "Reports data rows without the new-item placeholder."),
            new("ColumnCount", "Reports declared and generated columns."),
            new("FirstColumnForeground", "Sets the first displayed column color."),
        };
        ExampleGrid.ItemsSource = new DataGridExampleRow[]
        {
            new("Foobar", "Ready", "Application"),
            new("Reports", "Running", "Workspace"),
            new("Archive", "Paused", "System"),
        };
    }
}

public sealed record DataGridExampleRow(string Name, string Status, string Owner);
