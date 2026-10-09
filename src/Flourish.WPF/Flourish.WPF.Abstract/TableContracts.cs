namespace ArkheideSystem.Flourish.WPF.Abstract;

public enum TableView { Table, Cards }
public enum TablePurpose { Registry, Pool, Worklist }
public enum TableSearchMode { Local, Remote }
public enum TableSortDirection { Default, Descending, Ascending }
public enum TableCardField { Detail, Image, Title, Identifier, Metric }
public sealed record TableSearchRequest(string? ColumnKey, string Value);
/// <summary>The single record-list contract; BindingPath enables native binding and editing.</summary>
public sealed record TableColumn(string Key, string Label, string BindingPath, bool Sortable = true,
    bool Searchable = true, bool DefaultVisible = true, double Width = 180,
    TableCardField CardField = TableCardField.Detail, Func<object, object?>? Value = null,
    string? Format = null, Func<object, object?>? SortValue = null,
    Func<object, string?>? SearchValue = null, bool CanHide = true, bool CanReorder = true);
public sealed record TableCellContext(object Item, TableColumn Column, string Display);
public sealed record RowAction(string Text, Func<object, Task> OnClick,
    Func<object, bool>? Disabled = null, bool Destructive = false, Func<object, bool>? IsAvailable = null);
public sealed record TableSortPreference(string SortKey, bool Descending);
public interface ITablePreferences
{
    TableSortPreference? Get(string key);
    void Set(string key, TableSortPreference? preference);
}
public enum GridEditorKind { Text, Decimal, Date, Multiline, Select, Masked, Link, Template }
public sealed record GridColumn(string Key, string Label, GridEditorKind Editor = GridEditorKind.Text,
    bool Required = false, bool ReadOnly = false, double Width = 180,
    IReadOnlyList<SelectOption>? Options = null, string? BindingPath = null,
    int MaximumLength = 0, string? Mask = null, Func<object?, string?>? Validate = null,
    Func<object, Task>? LinkAction = null);
public sealed record GridCellChange(object Item, string ColumnKey, object? Value);
public sealed record GridEditError(object Item, string ColumnKey, string Message);
public sealed record ChartPointLabel(string Key, string Text, string? Tooltip = null);
public sealed record ChartSeries(string Key, string Label, IReadOnlyList<double> Values,
    bool Visible = true, double? Maximum = null, Func<double, string>? Format = null);
