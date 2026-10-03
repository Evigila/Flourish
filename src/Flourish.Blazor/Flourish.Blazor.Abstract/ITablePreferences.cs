namespace ArkheideSystem.Flourish.Blazor.Abstract;

/// <summary>UI preferences scoped to the current user/circuit, independent of business storage.</summary>
public interface ITablePreferences
{
    bool TryGetSort(string listKey, out TableSortPreference preference);
    void SetSort(string listKey, TableSortPreference preference);
    void RemoveSort(string listKey);
}

public sealed record TableSortPreference(string SortKey, bool Descending);