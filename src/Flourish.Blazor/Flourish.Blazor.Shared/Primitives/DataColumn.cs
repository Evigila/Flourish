
namespace ArkheideSystem.Flourish.Blazor.Components.Primitives;

public enum CardField
{
    Detail,
    Image,
    Title,
    Identifier,
    Metric
}

public enum TablePurpose
{
    Registry,
    Pool,
    Worklist
}

public enum SortDirection
{
    Ascending,
    Descending
}

public sealed class DataColumn<TItem>
{
    public DataColumn(
        string key,
        string label,
        Func<TItem, string?> value,
        bool defaultVisible = true,
        bool canHide = true,
        CardField cardField = CardField.Detail,
        bool? canReorder = null,
        Func<TItem, IComparable?>? sortValue = null,
        bool canSort = true,
        Func<TItem, string?>? searchValue = null,
        bool canSearch = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(label);
        Key = key;
        Label = label;
        Value = value ?? throw new ArgumentNullException(nameof(value));
        DefaultVisible = defaultVisible;
        CanHide = canHide;
        CardField = cardField;
        CanReorder = canReorder ?? canHide;
        SortValue = sortValue ?? (item => Value(item));
        CanSort = canSort;
        SearchValue = searchValue ?? Value;
        CanSearch = canSearch && cardField != CardField.Image;
    }

    public string Key { get; }
    public string Label { get; }
    public Func<TItem, string?> Value { get; }
    public bool DefaultVisible { get; }
    public bool CanHide { get; }
    public CardField CardField { get; }
    public bool CanReorder { get; }
    public Func<TItem, IComparable?> SortValue { get; }
    public bool CanSort { get; }
    public Func<TItem, string?> SearchValue { get; }
    public bool CanSearch { get; }
    public bool UsesTemplate { get; init; }
}

public sealed record DataCellContext<TItem>(TItem Item, DataColumn<TItem> Column);
