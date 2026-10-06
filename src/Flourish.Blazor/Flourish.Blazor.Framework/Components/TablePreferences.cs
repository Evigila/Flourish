using ArkheideSystem.Flourish.Blazor.Abstract;

namespace ArkheideSystem.Flourish.Blazor.Components;

/// <summary>Presentation preferences for one scope; never shared between user circuits.</summary>
public sealed class TablePreferences : ITablePreferences
{
    private readonly Dictionary<string, TableSortPreference> SortPreferences = new(StringComparer.Ordinal);

    public bool TryGetSort(string listKey, out TableSortPreference preference)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(listKey);
        return SortPreferences.TryGetValue(listKey, out preference!);
    }

    public void SetSort(string listKey, TableSortPreference preference)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(listKey);
        ArgumentNullException.ThrowIfNull(preference);
        SortPreferences[listKey] = preference;
    }

    public void RemoveSort(string listKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(listKey);
        SortPreferences.Remove(listKey);
    }
}
