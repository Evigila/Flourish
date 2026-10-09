using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using ArkheideSystem.Flourish.WPF.Abstract;
using Xunit;
using F = ArkheideSystem.Flourish.WPF.Controls;

namespace ArkheideSystem.Tests.Flourish.WPF;

public class DataTests
{
    private static readonly TableColumn[] Columns =
    [new("name", "Name", nameof(Record.Name)), new("amount", "Amount", nameof(Record.Amount), Format:"N2")];

    [Fact]
    public void Shared_table_data_supports_culture_search_numeric_sort_and_stable_default_order()
    {
        object[] items = [new Record("Évora", 20m), new Record("Alpha", 3m), new Record("Beta", 3m)];
        var culture = CultureInfo.GetCultureInfo("pt-BR");
        Assert.Equal("20,00", F.TableData.Display(items[0], Columns[1], culture));
        Assert.Equal(items[0], Assert.Single(F.TableData.Filter(items, Columns, "evora", "name", culture)));
        Assert.Equal(new[] { items[1], items[2], items[0] }, F.TableData.Sort(items, Columns[1], TableSortDirection.Ascending, culture));
        Assert.Same(items, F.TableData.Sort(items, Columns[1], TableSortDirection.Default, culture));
        Assert.Equal(3, F.TableData.ClampPage(999, 25, 10));
        Assert.Throws<ArgumentException>(() => F.TableData.ValidateColumns([Columns[0], Columns[0]]));
    }

    [Fact]
    public void Native_table_local_and_remote_search_and_paging_keep_data_owned_by_consumer() => NativeTest.Run(() =>
    {
        var source = new ObservableCollection<Record>([new("Alpha", 1), new("Beta", 2), new("Alpha second", 3)]);
        var table = new F.DataTable { Columns = Columns, ItemsSource = source, PageSize = 1 };
        var stage = NativeTest.Stage(table);
        Assert.Equal(source[0], Assert.Single(table.VisibleItems));
        table.CurrentPage = 2;
        Assert.Equal(source[1], Assert.Single(table.VisibleItems));
        table.SearchColumnKey = "name"; table.SearchQuery = "Alpha"; table.CurrentPage = 1;
        Assert.Equal(source[0], Assert.Single(table.VisibleItems));
        table.CurrentPage = 2;
        Assert.Equal(source[2], Assert.Single(table.VisibleItems));
        table.SearchMode = TableSearchMode.Remote; table.SearchQuery = "server expression";
        Assert.Equal(source[1], Assert.Single(table.VisibleItems));
        table.PageSize = 10;
        Assert.Equal(3, table.VisibleItems.Count);
        source.Add(new("Gamma", 4));
        Assert.Equal(4, table.VisibleItems.Count);
        Assert.Equal(4, source.Count);
        TableSearchRequest? request = null;
        table.SearchChanged += (_, value) => request = value;
        var search = NativeTest.Descendants<F.DataSearch>(stage).Single();
        var input = NativeTest.Descendants<F.TextBox>(search).Single();
        input.Text = "new remote query";
        Assert.Equal(new TableSearchRequest("name", "new remote query"), request);
        Assert.Equal(4, table.VisibleItems.Count);
    });

    [Fact]
    public void Native_header_owns_sort_cycle_and_pool_rejects_sorting() => NativeTest.Run(() =>
    {
        var first = new Record("A", 10); var second = new Record("B", 2);
        var table = new F.DataTable { Columns = Columns, ItemsSource = new[] { first, second } };
        var stage = NativeTest.Stage(table);
        var grid = NativeTest.Descendants<DataGrid>(stage).Single();
        Sort(grid, grid.Columns[1]);
        Assert.Equal(TableSortDirection.Descending, table.SortDirection);
        Assert.Equal(new object[] { first, second }, table.VisibleItems);
        Sort(grid, grid.Columns[1]);
        Assert.Equal(TableSortDirection.Ascending, table.SortDirection);
        Assert.Equal(new object[] { second, first }, table.VisibleItems);
        Sort(grid, grid.Columns[1]);
        Assert.Equal(TableSortDirection.Default, table.SortDirection);
        Assert.Equal(new object[] { first, second }, table.VisibleItems);
        table.Purpose = TablePurpose.Pool;
        Sort(grid, grid.Columns[1]);
        Assert.Equal(TableSortDirection.Default, table.SortDirection);
        Assert.All(grid.Columns, column => Assert.False(column.CanUserSort));
    });

    [Theory]
    [InlineData(TablePurpose.Pool)]
    [InlineData(TablePurpose.Worklist)]
    public void Pool_and_worklist_keep_native_cell_editors_and_unsaved_drafts_when_records_refresh_or_move(TablePurpose purpose) => NativeTest.Run(() =>
    {
        var first = new DraftRecord("Alpha"); var second = new DraftRecord("Beta");
        var rows = new ObservableCollection<DraftRecord>([first, second]);
        var cell = new FrameworkElementFactory(typeof(StackPanel));
        var display = new FrameworkElementFactory(typeof(TextBlock));
        display.SetBinding(TextBlock.TextProperty, new Binding(nameof(TableCellContext.Display)));
        var editor = new FrameworkElementFactory(typeof(F.TextBox));
        cell.AppendChild(display); cell.AppendChild(editor);
        var table = new F.DataTable { Columns = new[] { new TableColumn("name", "Name", nameof(DraftRecord.Name)) }, ItemsSource = rows,
            Purpose = purpose, CellTemplate = new DataTemplate { VisualTree = cell } };
        var stage = NativeTest.Stage(table);
        F.TextBox EditorFor(DraftRecord record) => NativeTest.Descendants<F.TextBox>(stage)
            .Single(input => input.DataContext is TableCellContext context && ReferenceEquals(context.Item, record));
        var original = EditorFor(first);
        original.Text = "unsaved host draft";
        first.Name = "Alpha updated";
        table.Refresh(); NativeTest.Layout(stage, 1000, 700);
        Assert.Same(original, EditorFor(first));
        Assert.Equal("Alpha updated", Assert.IsType<TableCellContext>(original.DataContext).Display);
        rows.Move(0, 1); NativeTest.Layout(stage, 1000, 700);
        Assert.Same(original, EditorFor(first));
        Assert.Equal("unsaved host draft", original.Text);
        Assert.Equal(new object[] { second, first }, table.VisibleItems);
    });

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Explicit_refresh_updates_mutable_record_or_delegate_display_without_replacing_native_editors(bool delegatedValue) => NativeTest.Run(() =>
    {
        var record = new MutableRecord { Name = "Original" };
        var prefix = "Before: ";
        var layout = new FrameworkElementFactory(typeof(StackPanel));
        var display = new FrameworkElementFactory(typeof(TextBlock));
        display.SetValue(FrameworkElement.TagProperty, "DisplayValue");
        display.SetBinding(TextBlock.TextProperty, new Binding(nameof(TableCellContext.Display)));
        layout.AppendChild(display); layout.AppendChild(new FrameworkElementFactory(typeof(F.TextBox)));
        var column = new TableColumn("name", "Name", nameof(MutableRecord.Name), Value: delegatedValue ? item => prefix + ((MutableRecord)item).Name : null);
        var table = new F.DataTable { Columns = new[] { column }, ItemsSource = new[] { record }, Purpose = TablePurpose.Pool,
            CellTemplate = new DataTemplate { VisualTree = layout } };
        var stage = NativeTest.Stage(table);
        var editor = NativeTest.Descendants<F.TextBox>(stage).Single(input => input.DataContext is TableCellContext);
        editor.Text = "host-owned draft";
        var originalDisplay = NativeTest.Descendants<TextBlock>(stage).Single(text => Equals(text.Tag, "DisplayValue"));
        Assert.Equal(delegatedValue ? "Before: Original" : "Original", originalDisplay.Text);
        record.Name = "Updated"; prefix = "After: ";
        table.Refresh(); NativeTest.Layout(stage, 1000, 700);
        Assert.Same(editor, NativeTest.Descendants<F.TextBox>(stage).Single(input => input.DataContext is TableCellContext));
        Assert.Same(originalDisplay, NativeTest.Descendants<TextBlock>(stage).Single(text => Equals(text.Tag, "DisplayValue")));
        Assert.Equal(delegatedValue ? "After: Updated" : "Updated", originalDisplay.Text);
        Assert.Equal("host-owned draft", editor.Text);
    });

    [Fact]
    public void Shared_search_renderer_emits_typed_requests_and_pager_clamps_before_events() => NativeTest.Run(() =>
    {
        var search = new F.DataSearch { Columns = Columns };
        TableSearchRequest? observed = null;
        search.Changed += (_, value) => observed = value;
        var stage = NativeTest.Stage(search, 600, 100);
        Assert.Equal("name", search.ColumnKey);
        NativeTest.Descendants<F.TextBox>(stage).Single().Text = "Ada";
        Assert.Equal(new TableSearchRequest("name", "Ada"), observed);
        var pager = new F.DataPager { TotalCount = 25, PageSize = 10 };
        var pages = new List<int>(); pager.PageChanged += (_, page) => pages.Add(page);
        pager.GoToPage(999); pager.GoToPage(999); pager.GoToPage(-1);
        Assert.Equal(new[] { 3, 1 }, pages);
        pager.IsEnabled = false; pager.GoToPage(2);
        Assert.Equal(1, pager.CurrentPage);
    });

    [Fact]
    public void Table_display_reuses_general_selection_and_retains_manual_width_across_cards_and_hidden_columns() => NativeTest.Run(() =>
    {
        var table = new F.DataTable { Columns = Columns, ItemsSource = new[] { new Record("Ada", 2m) } };
        var stage = NativeTest.Stage(table);
        var display = NativeTest.Descendants<F.MultiSelectBox>(stage).Single();
        table.SetColumnWidth("amount", 333);
        Assert.True(display.SetSelected("amount", false));
        table.View = TableView.Cards;
        Assert.Equal(333, table.ColumnWidths["amount"]);
        Assert.True(display.SetSelected("amount", true));
        table.View = TableView.Table;
        NativeTest.Layout(stage, 1000, 700);
        var grid = NativeTest.Descendants<DataGrid>(stage).Single();
        Assert.Equal(333, grid.Columns.Single(column => column.SortMemberPath == "amount").Width.Value);
        table.ResetColumnWidth("amount");
        Assert.DoesNotContain("amount", table.ColumnWidths.Keys);
        Assert.Equal(Columns[1].Width, grid.Columns.Single(column => column.SortMemberPath == "amount").Width.Value);
    });

    [Fact]
    public void Bulk_selection_contains_all_loaded_rows_across_pages_and_busy_blocks_changes() => NativeTest.Run(() =>
    {
        var source = new[] { new Record("A", 1), new Record("B", 2), new Record("C", 3) };
        var table = new F.DataTable { Columns = Columns, ItemsSource = source, PageSize = 1, BulkEditing = true };
        IReadOnlyList<object>? selected = null;
        table.EditSelectionChanged += (_, selection) => selected = selection;
        table.SelectAllLoadedForEditing(true);
        Assert.Equal(source.Cast<object>(), selected);
        table.CurrentPage = 2;
        Assert.Equal(source.Cast<object>(), table.EditSelection!.Cast<object>());
        table.EditingBusy = true;
        table.SelectAllLoadedForEditing(false);
        Assert.Equal(3, table.EditSelection!.Cast<object>().Count());
    });

    [Fact]
    public void Chart_single_point_negative_values_and_display_snapshots_use_native_geometry() => NativeTest.Run(() =>
    {
        var chart = new F.LineChart { Title = "Balances", IndependentScales = true };
        chart.SetData([new ChartPointLabel("day", "Day one")], [new ChartSeries("a", "Account A", [-5], Maximum:10), new ChartSeries("b", "Account B", [2], Maximum:20)]);
        var stage = NativeTest.Stage(chart, 800, 450);
        var display = NativeTest.Descendants<F.MultiSelectBox>(stage).Single();
        Assert.True(display.Move("b", -1));
        Assert.Equal(new[] { "b", "a" }, chart.VisibleSeries.Select(series => series.Key));
        Assert.True(display.SetSelected("a", false));
        Assert.Equal("b", Assert.Single(chart.VisibleSeries).Key);
        NativeTest.Export(stage, "chart-negative-single-point", 800, 450);
        Assert.Throws<ArgumentException>(() => new F.LineChart().SetData([new ChartPointLabel("day", "Day")], [new ChartSeries("bad", "Invalid", [double.NaN])]));
        Assert.Throws<ArgumentException>(() => new F.LineChart().SetData([new ChartPointLabel("day", "Day")], [new ChartSeries("bad", "Invalid", [3], Maximum:2)]));
    });

    [Fact]
    public void Spreadsheet_native_commit_proposes_typed_change_and_invalid_commit_reuses_validation_renderer() => NativeTest.Run(() =>
    {
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("pt-BR");
        var record = new Record("Ada", 1.5m);
        var editing = new F.EditingGrid { Columns = new[] { new GridColumn("amount", "Amount", GridEditorKind.Decimal, BindingPath:nameof(Record.Amount)) }, ItemsSource = new[] { record } };
        GridCellChange? proposed = null; GridEditError? rejected = null;
        editing.CellChanged += (_, change) => proposed = change; editing.EditError += (_, error) => rejected = error;
        var stage = NativeTest.Stage(editing);
        var grid = NativeTest.Descendants<DataGrid>(stage).Single();
        var row = Assert.IsType<DataGridRow>(grid.ItemContainerGenerator.ContainerFromIndex(0));
        var valid = new DataGridCellEditEndingEventArgs(grid.Columns[0], row, new F.TextBox { Text = "2,75" }, DataGridEditAction.Commit);
        Commit(grid, valid);
        Assert.False(valid.Cancel);
        Assert.NotNull(proposed);
        Assert.Equal("amount", proposed.ColumnKey);
        Assert.IsType<decimal>(proposed.Value);
        Assert.Equal(2.75m, proposed.Value);
        Assert.Equal(1.5m, record.Amount); // Only the host applies proposed edits.
        var invalid = new DataGridCellEditEndingEventArgs(grid.Columns[0], row, new F.TextBox { Text = "not a number" }, DataGridEditAction.Commit);
        Commit(grid, invalid);
        Assert.True(invalid.Cancel);
        Assert.NotNull(rejected);
        Assert.NotEmpty(NativeTest.Descendants<F.ValidationMessages>(stage).Single().Items);
        editing.EditDisabled = true;
        Assert.True(grid.IsReadOnly);
    });

    private static void Sort(DataGrid grid, DataGridColumn column) => typeof(DataGrid).GetMethod("OnSorting", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(grid, [new DataGridSortingEventArgs(column)]);
    private static void Commit(DataGrid grid, DataGridCellEditEndingEventArgs args) => typeof(DataGrid).GetMethod("OnCellEditEnding", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(grid, [args]);
    public sealed record Record(string Name, decimal Amount);
    private sealed class MutableRecord { public string Name { get; set; } = ""; }
    private sealed class DraftRecord(string name) : INotifyPropertyChanged
    {
        public string Name { get => name; set { name = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name))); } }
        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
