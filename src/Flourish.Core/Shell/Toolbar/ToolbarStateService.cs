using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;

using ArkheideSystem.Flourish.Abstract;

namespace ArkheideSystem.Flourish.Shell.Toolbar;

internal sealed class ToolbarStateService : IToolbarStateService
{
    private readonly Lock gate = new();
    private readonly ToolbarStateOptions options;
    private ToolbarStateSnapshot current;
    private long version;

    public ToolbarStateService(ToolbarStateOptions options)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        ValidateSeed(options);
        current = CreateSnapshot();
    }

    public event EventHandler<ToolbarStateChangedEventArgs>? Changed;

    public ToolbarStateSnapshot Current => Volatile.Read(ref current);

    public void SetEnabled(bool enabled)
    {
        Mutate(
            () =>
            {
                if (options.IsEnabled == enabled)
                {
                    return false;
                }

                options.IsEnabled = enabled;
                return true;
            },
            CollectionChangeKind.Updated,
            viewKey: null,
            itemId: null
        );
    }

    public void SetDefault(IEnumerable<ToolbarItem> items)
    {
        ToolbarItem[] replacement = ValidateItems(items);
        Mutate(
            () =>
            {
                if (options.DefaultItems.SequenceEqual(replacement))
                {
                    return false;
                }

                options.DefaultItems.Clear();
                options.DefaultItems.AddRange(replacement);
                return true;
            },
            CollectionChangeKind.Reset,
            viewKey: null,
            itemId: null
        );
    }

    public void SetView(string viewKey, IEnumerable<ToolbarItem> items, bool iconOnly = true)
    {
        viewKey = ValidateViewKey(viewKey, nameof(viewKey));
        ToolbarItem[] replacement = ValidateItems(items);
        Mutate(
            () =>
            {
                if (
                    options.ViewItems.TryGetValue(viewKey, out IReadOnlyList<ToolbarItem>? existing)
                    && existing.SequenceEqual(replacement)
                    && options.ViewIconModes.GetValueOrDefault(viewKey, true) == iconOnly
                )
                {
                    return false;
                }

                options.ViewItems[viewKey] = replacement;
                options.ViewIconModes[viewKey] = iconOnly;
                return true;
            },
            CollectionChangeKind.Reset,
            viewKey,
            itemId: null
        );
    }

    public void AddItem(ToolbarItem item, string? viewKey = null, int? index = null)
    {
        viewKey = ValidateOptionalViewKey(viewKey);
        ValidateItem(item);
        Mutate(
            () =>
            {
                List<ToolbarItem> items = GetMutableItems(viewKey);
                if (FindIndex(items, item.Id) >= 0)
                {
                    throw new InvalidOperationException(
                        $"Toolbar item ID '{item.Id}' is already registered."
                    );
                }

                Insert(items, item, index);
                StoreItems(viewKey, items);
                return true;
            },
            CollectionChangeKind.Added,
            viewKey,
            item.Id
        );
    }

    public void SetItem(ToolbarItem item, string? viewKey = null, int? index = null)
    {
        viewKey = ValidateOptionalViewKey(viewKey);
        ValidateItem(item);
        Mutate(
            () =>
            {
                List<ToolbarItem> items = GetMutableItems(viewKey);
                int existingIndex = FindIndex(items, item.Id);
                if (existingIndex >= 0)
                {
                    if (
                        items[existingIndex] == item
                        && (index is null || index.Value == existingIndex)
                    )
                    {
                        return false;
                    }

                    items.RemoveAt(existingIndex);
                    Insert(items, item, index ?? existingIndex);
                }
                else
                {
                    Insert(items, item, index);
                }

                StoreItems(viewKey, items);
                return true;
            },
            CollectionChangeKind.Updated,
            viewKey,
            item.Id
        );
    }

    public bool Remove(string id, string? viewKey = null)
    {
        id = ValidateId(id, nameof(id));
        viewKey = ValidateOptionalViewKey(viewKey);
        bool removed = false;
        Mutate(
            () =>
            {
                List<ToolbarItem> items = GetMutableItems(viewKey);
                int index = FindIndex(items, id);
                if (index < 0)
                {
                    return false;
                }

                items.RemoveAt(index);
                StoreItems(viewKey, items);
                removed = true;
                return true;
            },
            CollectionChangeKind.Removed,
            viewKey,
            id
        );
        return removed;
    }

    public void RemoveAll(string? viewKey = null)
    {
        viewKey = ValidateOptionalViewKey(viewKey);
        Mutate(
            () =>
            {
                List<ToolbarItem> items = GetMutableItems(viewKey);
                if (items.Count == 0)
                {
                    return false;
                }

                items.Clear();
                StoreItems(viewKey, items);
                return true;
            },
            CollectionChangeKind.Reset,
            viewKey,
            itemId: null
        );
    }

    public void SetOrder(string id, int newIndex, string? viewKey = null)
    {
        id = ValidateId(id, nameof(id));
        viewKey = ValidateOptionalViewKey(viewKey);
        Mutate(
            () =>
            {
                List<ToolbarItem> items = GetMutableItems(viewKey);
                int oldIndex = FindIndex(items, id);
                if (oldIndex < 0)
                {
                    throw new KeyNotFoundException($"Toolbar item ID '{id}' was not found.");
                }

                ValidateMoveIndex(newIndex, items.Count);
                if (oldIndex == newIndex)
                {
                    return false;
                }

                ToolbarItem item = items[oldIndex];
                items.RemoveAt(oldIndex);
                items.Insert(newIndex, item);
                StoreItems(viewKey, items);
                return true;
            },
            CollectionChangeKind.Moved,
            viewKey,
            id
        );
    }

    public void SetItemEnabled(string id, bool enabled, string? viewKey = null)
    {
        UpdateItem(id, viewKey, item => item with { IsEnabled = enabled });
    }

    public void SetItemVisible(string id, bool visible, string? viewKey = null)
    {
        UpdateItem(id, viewKey, item => item with { IsVisible = visible });
    }

    public void SetIconOnly(string viewKey, bool iconOnly)
    {
        viewKey = ValidateViewKey(viewKey, nameof(viewKey));
        Mutate(
            () =>
            {
                bool existing = options.ViewIconModes.GetValueOrDefault(viewKey, true);
                if (existing == iconOnly)
                {
                    return false;
                }

                options.ViewIconModes[viewKey] = iconOnly;
                return true;
            },
            CollectionChangeKind.Updated,
            viewKey,
            itemId: null
        );
    }

    public IReadOnlyList<ToolbarItem> GetItems(string? viewKey = null)
    {
        viewKey = ValidateOptionalViewKey(viewKey);
        ToolbarStateSnapshot snapshot = Volatile.Read(ref current);
        if (
            snapshot.IsEnabled
            && viewKey is not null
            && snapshot.Views.TryGetValue(viewKey, out ToolbarViewState? view)
        )
        {
            return view.Items;
        }

        return snapshot.DefaultItems;
    }

    public bool GetIconOnly(string viewKey)
    {
        viewKey = ValidateViewKey(viewKey, nameof(viewKey));
        ToolbarStateSnapshot snapshot = Volatile.Read(ref current);
        return snapshot.Views.TryGetValue(viewKey, out ToolbarViewState? view)
            ? view.IconOnly
            : true;
    }

    private void UpdateItem(
        string id,
        string? viewKey,
        Func<ToolbarItem, ToolbarItem> update
    )
    {
        id = ValidateId(id, nameof(id));
        viewKey = ValidateOptionalViewKey(viewKey);
        Mutate(
            () =>
            {
                List<ToolbarItem> items = GetMutableItems(viewKey);
                int index = FindIndex(items, id);
                if (index < 0)
                {
                    throw new KeyNotFoundException($"Toolbar item ID '{id}' was not found.");
                }

                ToolbarItem replacement = update(items[index]);
                ValidateItem(replacement);
                if (!StringComparer.Ordinal.Equals(id, replacement.Id))
                {
                    throw new InvalidOperationException(
                        "A toolbar update cannot change the stable item ID."
                    );
                }

                if (items[index] == replacement)
                {
                    return false;
                }

                items[index] = replacement;
                StoreItems(viewKey, items);
                return true;
            },
            CollectionChangeKind.Updated,
            viewKey,
            id
        );
    }

    private void Mutate(
        Func<bool> mutation,
        CollectionChangeKind changeKind,
        string? viewKey,
        string? itemId
    )
    {
        ToolbarStateSnapshot snapshot;
        lock (gate)
        {
            if (!mutation())
            {
                return;
            }

            version++;
            snapshot = CreateSnapshot();
            Volatile.Write(ref current, snapshot);
        }

        Changed?.Invoke(
            this,
            new ToolbarStateChangedEventArgs(snapshot, changeKind, viewKey, itemId)
        );
    }

    private ToolbarStateSnapshot CreateSnapshot()
    {
        var keys = new HashSet<string>(options.ViewItems.Keys, StringComparer.Ordinal);
        keys.UnionWith(options.ViewIconModes.Keys);
        Dictionary<string, ToolbarViewState> views = keys.ToDictionary(
            key => key,
            key =>
            {
                IReadOnlyList<ToolbarItem> items = options.ViewItems.GetValueOrDefault(key, []);
                return new ToolbarViewState(
                    key,
                    options.ViewIconModes.GetValueOrDefault(key, true),
                    Array.AsReadOnly(items.ToArray())
                );
            },
            StringComparer.Ordinal
        );

        return new ToolbarStateSnapshot(
            options.IsEnabled,
            Array.AsReadOnly(options.DefaultItems.ToArray()),
            new ReadOnlyDictionary<string, ToolbarViewState>(views),
            version
        );
    }

    private List<ToolbarItem> GetMutableItems(string? viewKey)
    {
        if (viewKey is null)
        {
            return [.. options.DefaultItems];
        }

        return options.ViewItems.TryGetValue(viewKey, out IReadOnlyList<ToolbarItem>? items)
            ? [.. items]
            : [];
    }

    private void StoreItems(string? viewKey, IReadOnlyList<ToolbarItem> items)
    {
        if (viewKey is null)
        {
            options.DefaultItems.Clear();
            options.DefaultItems.AddRange(items);
            return;
        }

        options.ViewItems[viewKey] = items.ToArray();
    }

    private static void ValidateSeed(ToolbarStateOptions options)
    {
        ValidateItems(options.DefaultItems);
        foreach (KeyValuePair<string, IReadOnlyList<ToolbarItem>> pair in options.ViewItems)
        {
            ValidateViewKey(pair.Key, "viewKey");
            ValidateItems(pair.Value);
        }

        foreach (string viewKey in options.ViewIconModes.Keys)
        {
            ValidateViewKey(viewKey, "viewKey");
        }
    }

    private static ToolbarItem[] ValidateItems(IEnumerable<ToolbarItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        ToolbarItem[] result = items.ToArray();
        foreach (ToolbarItem item in result)
        {
            ValidateItem(item);
        }

        IGrouping<string, ToolbarItem>? duplicate = result
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

    private static void ValidateItem(ToolbarItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        ValidateId(item.Id, nameof(item.Id));
        if (string.IsNullOrWhiteSpace(item.DisplayName))
        {
            throw new ArgumentException("Toolbar display name cannot be empty.", nameof(item));
        }

        ArgumentNullException.ThrowIfNull(item.IconGlyph);
    }

    private static string ValidateId(string id, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("A stable ID is required.", parameterName);
        }

        return id;
    }

    private static string? ValidateOptionalViewKey(string? viewKey)
    {
        return viewKey is null ? null : ValidateViewKey(viewKey, nameof(viewKey));
    }

    private static string ValidateViewKey(string viewKey, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(viewKey))
        {
            throw new ArgumentException("A stable view key is required.", parameterName);
        }

        return viewKey;
    }

    private static int FindIndex(IReadOnlyList<ToolbarItem> items, string id)
    {
        for (int index = 0; index < items.Count; index++)
        {
            if (StringComparer.Ordinal.Equals(items[index].Id, id))
            {
                return index;
            }
        }

        return -1;
    }

    private static void Insert(List<ToolbarItem> items, ToolbarItem item, int? index)
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
