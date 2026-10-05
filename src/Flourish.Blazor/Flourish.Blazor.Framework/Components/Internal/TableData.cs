using System.Globalization;

namespace ArkheideSystem.Flourish.Blazor.Components;

/// <summary>Pure local-data operations shared by both visual modes.</summary>
internal static class TableData<TItem>
{
    internal static IReadOnlyList<TItem> Filter(
        IReadOnlyList<TItem> items,
        IReadOnlyList<TableColumn<TItem>> columns,
        string? query,
        string? columnKey,
        CultureInfo culture)
    {
        if (string.IsNullOrWhiteSpace(query)) return items;
        var searchable = columns.Where(column => column.Searchable
            && (string.IsNullOrEmpty(columnKey) || column.Key == columnKey)).ToArray();
        var text = query.Trim();
        return items.Where(item => searchable.Any(column =>
            culture.CompareInfo.IndexOf(Display(item, column, culture), text,
                CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0)).ToArray();
    }

    internal static IReadOnlyList<TItem> Sort(
        IReadOnlyList<TItem> items,
        TableColumn<TItem>? column,
        TableSortDirection direction,
        CultureInfo culture)
    {
        if (column is null || !column.Sortable || direction == TableSortDirection.Default) return items;
        var rows = items.Select((item, index) => (Item: item, Index: index, Value: column.Value(item))).ToArray();
        Array.Sort(rows, (left, right) =>
        {
            // Null is always last; reversing a single composite comparer would break that guarantee.
            var order = left.Value is null ? (right.Value is null ? 0 : 1)
                : right.Value is null ? -1
                : Math.Sign(CompareNonNull(left.Value, right.Value, culture)) * (direction == TableSortDirection.Descending ? -1 : 1);
            return order == 0 ? left.Index.CompareTo(right.Index) : order;
        });
        return rows.Select(row => row.Item).ToArray();
    }

    internal static string Display(TItem item, TableColumn<TItem> column, CultureInfo culture)
    {
        if (column.Format is not null) return column.Format(item) ?? string.Empty;
        return column.Value(item) switch
        {
            null => string.Empty,
            DateTime date => date.ToString("d", culture),
            DateTimeOffset date => date.ToString("g", culture),
            DateOnly date => date.ToString("d", culture),
            IFormattable value => value.ToString(null, culture) ?? string.Empty,
            var value => value.ToString() ?? string.Empty
        };
    }

    internal static int ClampPage(int page, int itemCount, int pageSize)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
        return Math.Clamp(page, 1, Math.Max(1, (int)Math.Ceiling(itemCount / (double)pageSize)));
    }

    private static int CompareNonNull(object left, object right, CultureInfo culture)
    {
        if (IsNumber(left) && IsNumber(right))
        {
            if (left is float or double || right is float or double)
                return Convert.ToDouble(left, CultureInfo.InvariantCulture).CompareTo(Convert.ToDouble(right, CultureInfo.InvariantCulture));
            return Convert.ToDecimal(left, CultureInfo.InvariantCulture).CompareTo(Convert.ToDecimal(right, CultureInfo.InvariantCulture));
        }
        if (left is string leftText && right is string rightText)
            return culture.CompareInfo.Compare(leftText, rightText, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace);
        if (left is DateTimeOffset leftOffset && right is DateTimeOffset rightOffset) return leftOffset.CompareTo(rightOffset);
        if (left is DateTime leftDate && right is DateTime rightDate) return leftDate.CompareTo(rightDate);
        if (left is DateOnly leftDay && right is DateOnly rightDay) return leftDay.CompareTo(rightDay);
        if (left is TimeOnly leftTime && right is TimeOnly rightTime) return leftTime.CompareTo(rightTime);
        if (left.GetType() == right.GetType() && left is IComparable comparable)
            return Math.Sign(comparable.CompareTo(right));
        return culture.CompareInfo.Compare(Convert.ToString(left, culture), Convert.ToString(right, culture),
            CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace);
    }

    private static bool IsNumber(object value) => value is byte or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal;
}
