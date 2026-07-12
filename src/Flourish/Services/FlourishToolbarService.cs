using System.Windows.Controls;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;

namespace ArkheideSystem.Flourish.Services;

internal sealed class FlourishToolbarService(FlourishShellOptions options) : IToolbarService
{
    private readonly object gate = new();
    private readonly FlourishShellOptions options = options ?? throw new ArgumentNullException(nameof(options));
    private long version;

    public event EventHandler<FlourishToolbarChangedEventArgs>? Changed;

    public FlourishToolbarSnapshot Current
    {
        get
        {
            lock (gate)
            {
                return CreateSnapshot();
            }
        }
    }

    public void SetEnabled(bool enabled)
    {
        Mutate(
            () =>
            {
                if (options.IsDynamicToolbarEnabled == enabled)
                {
                    return false;
                }

                options.IsDynamicToolbarEnabled = enabled;
                return true;
            },
            FlourishRuntimeChangeKind.Updated,
            pageType: null,
            itemId: null
        );
    }

    public void ReplaceDefault(IEnumerable<FlourishToolbarItem> items)
    {
        var replacement = ValidateItems(items);
        Mutate(
            () =>
            {
                options.ToolbarItems.Clear();
                options.ToolbarItems.AddRange(replacement);
                return true;
            },
            FlourishRuntimeChangeKind.Reset,
            pageType: null,
            itemId: null
        );
    }

    public void Replace(
        Type pageType,
        IEnumerable<FlourishToolbarItem> items,
        bool iconOnly = true
    )
    {
        ValidatePageType(pageType);
        var replacement = ValidateItems(items);
        Mutate(
            () =>
            {
                options.DynamicToolbarItems[pageType] = replacement;
                options.DynamicToolbarIconModes[pageType] = iconOnly;
                return true;
            },
            FlourishRuntimeChangeKind.Reset,
            pageType,
            itemId: null
        );
    }

    public void Replace<TPage>(
        IEnumerable<FlourishToolbarItem> items,
        bool iconOnly = true
    ) where TPage : Page
    {
        Replace(typeof(TPage), items, iconOnly);
    }

    public void Add(FlourishToolbarItem item, Type? pageType = null, int? index = null)
    {
        ValidatePageTypeIfPresent(pageType);
        ValidateItem(item);
        Mutate(
            () =>
            {
                var items = GetMutableItems(pageType);
                if (FindIndex(items, item.Id) >= 0)
                {
                    throw new InvalidOperationException(
                        $"Toolbar item ID '{item.Id}' is already registered."
                    );
                }

                Insert(items, item, index);
                StoreItems(pageType, items);
                return true;
            },
            FlourishRuntimeChangeKind.Added,
            pageType,
            item.Id
        );
    }

    public void Upsert(FlourishToolbarItem item, Type? pageType = null, int? index = null)
    {
        ValidatePageTypeIfPresent(pageType);
        ValidateItem(item);
        Mutate(
            () =>
            {
                var items = GetMutableItems(pageType);
                var existingIndex = FindIndex(items, item.Id);
                if (existingIndex >= 0)
                {
                    items.RemoveAt(existingIndex);
                    Insert(items, item, index ?? existingIndex);
                }
                else
                {
                    Insert(items, item, index);
                }

                StoreItems(pageType, items);
                return true;
            },
            FlourishRuntimeChangeKind.Updated,
            pageType,
            item.Id
        );
    }

    public bool Remove(string id, Type? pageType = null)
    {
        id = ValidateId(id, nameof(id));
        ValidatePageTypeIfPresent(pageType);
        var removed = false;
        Mutate(
            () =>
            {
                var items = GetMutableItems(pageType);
                var index = FindIndex(items, id);
                if (index < 0)
                {
                    return false;
                }

                items.RemoveAt(index);
                StoreItems(pageType, items);
                removed = true;
                return true;
            },
            FlourishRuntimeChangeKind.Removed,
            pageType,
            id
        );
        return removed;
    }

    public void Clear(Type? pageType = null)
    {
        ValidatePageTypeIfPresent(pageType);
        Mutate(
            () =>
            {
                var items = GetMutableItems(pageType);
                if (items.Count == 0)
                {
                    return false;
                }

                items.Clear();
                StoreItems(pageType, items);
                return true;
            },
            FlourishRuntimeChangeKind.Reset,
            pageType,
            itemId: null
        );
    }

    public void Move(string id, int newIndex, Type? pageType = null)
    {
        id = ValidateId(id, nameof(id));
        ValidatePageTypeIfPresent(pageType);
        Mutate(
            () =>
            {
                var items = GetMutableItems(pageType);
                var oldIndex = FindIndex(items, id);
                if (oldIndex < 0)
                {
                    throw new KeyNotFoundException($"Toolbar item ID '{id}' was not found.");
                }

                ValidateMoveIndex(newIndex, items.Count);
                if (oldIndex == newIndex)
                {
                    return false;
                }

                var item = items[oldIndex];
                items.RemoveAt(oldIndex);
                items.Insert(newIndex, item);
                StoreItems(pageType, items);
                return true;
            },
            FlourishRuntimeChangeKind.Moved,
            pageType,
            id
        );
    }

    public void SetItemEnabled(string id, bool enabled, Type? pageType = null)
    {
        UpdateItem(id, pageType, item => item with { IsEnabled = enabled });
    }

    public void SetItemVisible(string id, bool visible, Type? pageType = null)
    {
        UpdateItem(id, pageType, item => item with { IsVisible = visible });
    }

    public void SetIconOnly(Type pageType, bool iconOnly)
    {
        ValidatePageType(pageType);
        Mutate(
            () =>
            {
                var current = options.DynamicToolbarIconModes.GetValueOrDefault(pageType, true);
                if (current == iconOnly)
                {
                    return false;
                }

                options.DynamicToolbarIconModes[pageType] = iconOnly;
                return true;
            },
            FlourishRuntimeChangeKind.Updated,
            pageType,
            itemId: null
        );
    }

    internal IReadOnlyList<FlourishToolbarItem> GetToolbarItems(Type? pageType = null)
    {
        lock (gate)
        {
            if (
                options.IsDynamicToolbarEnabled
                && pageType is not null
                && options.DynamicToolbarItems.TryGetValue(pageType, out var dynamicItems)
            )
            {
                return dynamicItems;
            }

            return options.ToolbarItems;
        }
    }

    internal bool ShouldShowIconOnly(Type pageType)
    {
        lock (gate)
        {
            return options.DynamicToolbarIconModes.GetValueOrDefault(pageType, true);
        }
    }

    private void UpdateItem(
        string id,
        Type? pageType,
        Func<FlourishToolbarItem, FlourishToolbarItem> update
    )
    {
        id = ValidateId(id, nameof(id));
        ValidatePageTypeIfPresent(pageType);
        Mutate(
            () =>
            {
                var items = GetMutableItems(pageType);
                var index = FindIndex(items, id);
                if (index < 0)
                {
                    throw new KeyNotFoundException($"Toolbar item ID '{id}' was not found.");
                }

                var replacement = update(items[index]);
                ValidateItem(replacement);
                if (!StringComparer.Ordinal.Equals(id, replacement.Id))
                {
                    throw new InvalidOperationException("A toolbar update cannot change the stable item ID.");
                }

                if (items[index] == replacement)
                {
                    return false;
                }

                items[index] = replacement;
                StoreItems(pageType, items);
                return true;
            },
            FlourishRuntimeChangeKind.Updated,
            pageType,
            id
        );
    }

    private void Mutate(
        Func<bool> mutation,
        FlourishRuntimeChangeKind changeKind,
        Type? pageType,
        string? itemId
    )
    {
        FlourishToolbarSnapshot snapshot;
        lock (gate)
        {
            if (!mutation())
            {
                return;
            }

            version++;
            snapshot = CreateSnapshot();
        }

        Changed?.Invoke(
            this,
            new FlourishToolbarChangedEventArgs(snapshot, changeKind, pageType, itemId)
        );
    }

    private FlourishToolbarSnapshot CreateSnapshot()
    {
        var pages = options.DynamicToolbarItems.ToDictionary(
            pair => pair.Key,
            pair => new FlourishPageToolbarSnapshot(
                pair.Key,
                options.DynamicToolbarIconModes.GetValueOrDefault(pair.Key, true),
                pair.Value.ToArray()
            )
        );

        return new FlourishToolbarSnapshot(
            options.IsDynamicToolbarEnabled,
            options.ToolbarItems.ToArray(),
            pages,
            version
        );
    }

    private List<FlourishToolbarItem> GetMutableItems(Type? pageType)
    {
        if (pageType is null)
        {
            return [.. options.ToolbarItems];
        }

        return options.DynamicToolbarItems.TryGetValue(pageType, out var items)
            ? [.. items]
            : [];
    }

    private void StoreItems(Type? pageType, IReadOnlyList<FlourishToolbarItem> items)
    {
        if (pageType is null)
        {
            options.ToolbarItems.Clear();
            options.ToolbarItems.AddRange(items);
            return;
        }

        options.DynamicToolbarItems[pageType] = items.ToArray();
    }

    private static FlourishToolbarItem[] ValidateItems(IEnumerable<FlourishToolbarItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        var result = items.ToArray();
        foreach (var item in result)
        {
            ValidateItem(item);
        }

        var duplicate = result
            .GroupBy(item => item.Id, StringComparer.Ordinal)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null)
        {
            throw new InvalidOperationException(
                $"Toolbar item ID '{duplicate.Key}' is duplicated."
            );
        }

        return result;
    }

    private static void ValidateItem(FlourishToolbarItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        ValidateId(item.Id, nameof(item.Id));
        if (string.IsNullOrWhiteSpace(item.DisplayName))
        {
            throw new ArgumentException("Toolbar display name cannot be empty.", nameof(item));
        }
    }

    private static string ValidateId(string id, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("A stable ID is required.", parameterName);
        }

        return id;
    }

    private static void ValidatePageTypeIfPresent(Type? pageType)
    {
        if (pageType is not null)
        {
            ValidatePageType(pageType);
        }
    }

    private static void ValidatePageType(Type pageType)
    {
        ArgumentNullException.ThrowIfNull(pageType);
        if (!typeof(Page).IsAssignableFrom(pageType))
        {
            throw new ArgumentException(
                $"{pageType.FullName} must derive from System.Windows.Controls.Page.",
                nameof(pageType)
            );
        }
    }

    private static int FindIndex(IReadOnlyList<FlourishToolbarItem> items, string id)
    {
        for (var index = 0; index < items.Count; index++)
        {
            if (StringComparer.Ordinal.Equals(items[index].Id, id))
            {
                return index;
            }
        }

        return -1;
    }

    private static void Insert(List<FlourishToolbarItem> items, FlourishToolbarItem item, int? index)
    {
        if (index is null)
        {
            items.Add(item);
            return;
        }

        if (index < 0 || index > items.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        items.Insert(index.Value, item);
    }

    private static void ValidateMoveIndex(int index, int count)
    {
        if (index < 0 || index >= count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }
    }
}
