using System.Collections;
using System.Globalization;
using ArkheideSystem.Flourish.WPF.Abstract;

namespace ArkheideSystem.Flourish.WPF.Controls;

/// <summary>Culture-aware, stable operations shared by record and read-only presentation.</summary>
public static class TableData
{
    public static object? Value(object item, TableColumn column) => column.Value is null ? Read(item, column.BindingPath) : column.Value(item);

    public static object? Read(object? item, string path)
    {
        foreach (var segment in path.Split('.'))
        {
            if (item is null) return null;
            item = item is IDictionary dictionary ? dictionary[segment] : item.GetType().GetProperty(segment)?.GetValue(item);
        }
        return item;
    }

    public static string Display(object item, TableColumn column, CultureInfo culture)
    {
        return Value(item, column) switch
        {
            null => string.Empty,
            DateTime value when column.Format is null => value.ToString("d", culture),
            DateOnly value when column.Format is null => value.ToString("d", culture),
            DateTimeOffset value when column.Format is null => value.ToString("g", culture),
            IFormattable value => value.ToString(column.Format, culture) ?? string.Empty,
            var value => value.ToString() ?? string.Empty
        };
    }

    public static IReadOnlyList<object> Filter(IReadOnlyList<object> items, IReadOnlyList<TableColumn> columns,
        string? query, string? columnKey, CultureInfo culture)
    {
        if (string.IsNullOrWhiteSpace(query)) return items;
        var searchable = columns.Where(column => column.Searchable && (string.IsNullOrEmpty(columnKey) || column.Key == columnKey)).ToArray();
        return items.Where(item => searchable.Any(column => culture.CompareInfo.IndexOf(
            column.SearchValue?.Invoke(item) ?? Display(item, column, culture), query.Trim(),
            CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0)).ToArray();
    }

    public static IReadOnlyList<object> Sort(IReadOnlyList<object> items, TableColumn? column,
        TableSortDirection direction, CultureInfo culture)
    {
        if (column is null || !column.Sortable || direction == TableSortDirection.Default) return items;
        var rows = items.Select((item, index) => (Item: item, Index: index, Value: column.SortValue is null ? Value(item, column) : column.SortValue(item))).ToArray();
        Array.Sort(rows, (left, right) =>
        {
            var order = left.Value is null ? right.Value is null ? 0 : 1 : right.Value is null ? -1
                : Math.Sign(Compare(left.Value, right.Value, culture)) * (direction == TableSortDirection.Descending ? -1 : 1);
            return order == 0 ? left.Index.CompareTo(right.Index) : order;
        });
        return rows.Select(row => row.Item).ToArray();
    }

    public static int ClampPage(int page, int count, int pageSize)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        return Math.Clamp(page, 1, Math.Max(1, (int)Math.Ceiling(count / (double)pageSize)));
    }

    public static void ValidateColumns(IReadOnlyList<TableColumn> columns)
    {
        var keys = new HashSet<string>(StringComparer.Ordinal);
        foreach (var column in columns)
            if (column is null || string.IsNullOrWhiteSpace(column.Key) || !keys.Add(column.Key)
                || string.IsNullOrWhiteSpace(column.Label) || (column.Value is null && string.IsNullOrWhiteSpace(column.BindingPath))
                || !double.IsFinite(column.Width) || column.Width < 72)
                throw new ArgumentException("Columns require unique stable keys, labels, a binding or getter and finite widths of at least 72.", nameof(columns));
    }

    private static int Compare(object left, object right, CultureInfo culture)
    {
        if (IsNumber(left) && IsNumber(right))
            return left is double or float || right is double or float
                ? Convert.ToDouble(left, CultureInfo.InvariantCulture).CompareTo(Convert.ToDouble(right, CultureInfo.InvariantCulture))
                : Convert.ToDecimal(left, CultureInfo.InvariantCulture).CompareTo(Convert.ToDecimal(right, CultureInfo.InvariantCulture));
        if (left is string a && right is string b)
            return culture.CompareInfo.Compare(a, b, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace);
        if (left.GetType() == right.GetType() && left is IComparable comparable) return comparable.CompareTo(right);
        return culture.CompareInfo.Compare(Convert.ToString(left, culture), Convert.ToString(right, culture), CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace);
    }

    private static bool IsNumber(object value) => value is byte or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal;
}
