using System.Globalization;

namespace ArkheideSystem.Flourish.Blazor.Components.Primitives;

public static class DataSorter
{
    private static readonly CompareInfo PortugueseComparison = CultureInfo.GetCultureInfo("pt-BR").CompareInfo;

    public static IReadOnlyList<TItem> Sort<TItem>(
        IReadOnlyList<TItem> items,
        DataColumn<TItem> column,
        SortDirection direction)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(column);
        if (!column.CanSort || items.Count < 2)
        {
            return items;
        }

        var comparer = Comparer<TItem>.Create((left, right) => CompareValues(
            column.SortValue(left),
            column.SortValue(right),
            direction));
        return items.OrderBy(item => item, comparer).ToArray();
    }

    private static int CompareValues(
        IComparable? left,
        IComparable? right,
        SortDirection direction)
    {
        if (ReferenceEquals(left, right)) return 0;
        if (left is null) return 1;
        if (right is null) return -1;

        int comparison;
        if (left is string leftText && right is string rightText)
        {
            comparison = PortugueseComparison.Compare(
                leftText,
                rightText,
                CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace);
        }
        else if (left.GetType() == right.GetType())
        {
            comparison = left.CompareTo(right);
        }
        else
        {
            comparison = PortugueseComparison.Compare(
                Convert.ToString(left, CultureInfo.InvariantCulture),
                Convert.ToString(right, CultureInfo.InvariantCulture),
                CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace);
        }

        return direction == SortDirection.Ascending ? comparison : -comparison;
    }
}
