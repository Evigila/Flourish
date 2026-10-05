using System.Globalization;

namespace ArkheideSystem.Flourish.Blazor.Components.Primitives;

public static class DataFilter
{
    private static readonly CompareInfo Comparison = CultureInfo.GetCultureInfo("pt-BR").CompareInfo;

    public static IReadOnlyList<TItem> Filter<TItem>(IReadOnlyList<TItem> items,
        IReadOnlyList<DataColumn<TItem>> columns, string? columnKey, string? term)
    {
        if (string.IsNullOrWhiteSpace(term)) return items;
        var column = columns.FirstOrDefault(column => column.CanSearch && column.Key == columnKey)
            ?? columns.FirstOrDefault(column => column.CanSearch);
        if (column is null) return [];
        var query = term.Trim();
        return items.Where(item => column.SearchValue(item) is { } value &&
            Comparison.IndexOf(value, query, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0).ToArray();
    }
}
