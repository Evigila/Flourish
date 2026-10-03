namespace ArkheideSystem.Flourish.Blazor.Components.Primitives;

/// <summary>Pagination of an already loaded result set; never a remote record count.</summary>
public sealed class DataPagination
{
    public const int DefaultPageSize = 20;

    public DataPagination(int pageSize = DefaultPageSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);
        PageSize = pageSize;
    }

    public int PageSize { get; }
    public int TotalCount { get; private set; }
    public int CurrentPage { get; private set; } = 1;
    public int PageCount => Math.Max(1, (int)(((long)TotalCount + PageSize - 1) / PageSize));
    public int Offset => (CurrentPage - 1) * PageSize;
    public int FirstItem => TotalCount == 0 ? 0 : Offset + 1;
    public int LastItem => (int)Math.Min(TotalCount, (long)CurrentPage * PageSize);
    public bool HasPrevious => CurrentPage > 1;
    public bool HasNext => CurrentPage < PageCount;

    public void UpdateTotal(int totalCount, bool reset = false)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(totalCount);
        TotalCount = totalCount;
        GoToPage(reset ? 1 : CurrentPage);
    }

    public void GoToPage(int page) => CurrentPage = Math.Clamp(page, 1, PageCount);
}
