using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using ArkheideSystem.Flourish.Abstract;

namespace ArkheideSystem.Flourish.Navigation;

internal sealed class NavigationMenuStateService : INavigationMenuStateService
{
    private readonly Lock gate = new();
    private List<GroupState> groups = [];
    private List<NavigationMenuItem> fixedItems = [];
    private NavigationMenuSnapshot current = new([], [], Version: 0);
    private bool isTransactionActive;
    private long version;

    public NavigationMenuSnapshot Current => Volatile.Read(ref current);

    public event EventHandler<StateTransitionEventArgs<NavigationMenuSnapshot>>? Changed;

    public void Set(Action<INavigationMenuEditor> update)
    {
        ArgumentNullException.ThrowIfNull(update);

        NavigationMenuSnapshot previous;
        NavigationMenuSnapshot updated;
        lock (gate)
        {
            if (isTransactionActive)
            {
                throw new InvalidOperationException(
                    "A navigation menu transaction cannot start from another transaction."
                );
            }

            isTransactionActive = true;
            try
            {
                previous = current;
                List<GroupState> workingGroups = CloneGroups(groups);
                List<NavigationMenuItem> workingFixedItems = [.. fixedItems];
                var editor = new NavigationMenuEditor(workingGroups, workingFixedItems);

                try
                {
                    update(editor);
                }
                finally
                {
                    editor.Complete();
                }

                ValidateState(workingGroups, workingFixedItems);
                if (MenuEquals(groups, fixedItems, workingGroups, workingFixedItems))
                {
                    return;
                }

                groups = workingGroups;
                fixedItems = workingFixedItems;
                version++;
                updated = CreateSnapshot(groups, fixedItems, version);
                Volatile.Write(ref current, updated);
            }
            finally
            {
                isTransactionActive = false;
            }
        }

        Changed?.Invoke(
            this,
            new StateTransitionEventArgs<NavigationMenuSnapshot>(previous, updated)
        );
    }

    private static void ValidateState(
        IReadOnlyList<GroupState> groups,
        IReadOnlyList<NavigationMenuItem> fixedItems
    )
    {
        var groupIds = new HashSet<string>(StringComparer.Ordinal);
        var itemIds = new HashSet<string>(StringComparer.Ordinal);
        var navigationKeys = new HashSet<string>(StringComparer.Ordinal);

        foreach (GroupState group in groups)
        {
            ValidateStableValue(group.Id, "group ID");
            if (!groupIds.Add(group.Id))
            {
                throw new InvalidOperationException(
                    $"Navigation group ID '{group.Id}' is duplicated."
                );
            }

            ValidateItems(group.Items, $"navigation group '{group.Id}'", itemIds, navigationKeys);
        }

        ValidateItems(fixedItems, "fixed navigation items", itemIds, navigationKeys);
    }

    private static void ValidateItems(
        IReadOnlyList<NavigationMenuItem> items,
        string scope,
        HashSet<string> globalItemIds,
        HashSet<string> navigationKeys
    )
    {
        var itemsById = new Dictionary<string, NavigationMenuItem>(StringComparer.Ordinal);
        foreach (NavigationMenuItem item in items)
        {
            ArgumentNullException.ThrowIfNull(item);
            ValidateStableValue(item.Id, "item ID");
            if (!globalItemIds.Add(item.Id))
            {
                throw new InvalidOperationException(
                    $"Navigation item ID '{item.Id}' is duplicated."
                );
            }

            if (string.IsNullOrWhiteSpace(item.Label))
            {
                throw new InvalidOperationException(
                    $"Navigation item '{item.Id}' requires a label."
                );
            }

            if (item.IconGlyph is null)
            {
                throw new InvalidOperationException(
                    $"Navigation item '{item.Id}' requires a non-null icon glyph."
                );
            }

            if (!Enum.IsDefined(item.Kind))
            {
                throw new InvalidOperationException(
                    $"Navigation item '{item.Id}' has an unsupported kind."
                );
            }

            itemsById.Add(item.Id, item);
            if (item.Kind == NavigationMenuItemKind.Page)
            {
                ValidateStableValue(item.NavigationKey, "page navigation key");
                if (!navigationKeys.Add(item.NavigationKey!))
                {
                    throw new InvalidOperationException(
                        $"Navigation key '{item.NavigationKey}' is already present in the menu."
                    );
                }
            }
        }

        foreach (NavigationMenuItem item in items.Where(item => item.ParentId is not null))
        {
            if (!itemsById.TryGetValue(item.ParentId!, out NavigationMenuItem? parent))
            {
                throw new InvalidOperationException(
                    $"Parent item '{item.ParentId}' for '{item.Id}' was not found in {scope}."
                );
            }

            if (StringComparer.Ordinal.Equals(item.Id, item.ParentId))
            {
                throw new InvalidOperationException("A navigation item cannot parent itself.");
            }

            if (parent.ParentId is not null)
            {
                throw new InvalidOperationException(
                    "Runtime navigation trees support one parent/child level."
                );
            }
        }
    }

    private static void ValidateStableValue(string? value, string description)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"A stable {description} is required.");
        }
    }

    private static NavigationMenuSnapshot CreateSnapshot(
        IReadOnlyList<GroupState> groups,
        IReadOnlyList<NavigationMenuItem> fixedItems,
        long version
    )
    {
        NavigationMenuGroup[] snapshotGroups = groups
            .Select(group => new NavigationMenuGroup(group.Id, group.Title, group.Items))
            .ToArray();
        return new NavigationMenuSnapshot(snapshotGroups, fixedItems, version);
    }

    private static List<GroupState> CloneGroups(IEnumerable<GroupState> groups)
    {
        return groups
            .Select(group => new GroupState(group.Id, group.Title, [.. group.Items]))
            .ToList();
    }

    private static bool MenuEquals(
        IReadOnlyList<GroupState> leftGroups,
        IReadOnlyList<NavigationMenuItem> leftFixedItems,
        IReadOnlyList<GroupState> rightGroups,
        IReadOnlyList<NavigationMenuItem> rightFixedItems
    )
    {
        if (
            leftGroups.Count != rightGroups.Count
            || !leftFixedItems.SequenceEqual(rightFixedItems)
        )
        {
            return false;
        }

        for (int index = 0; index < leftGroups.Count; index++)
        {
            GroupState left = leftGroups[index];
            GroupState right = rightGroups[index];
            if (
                !StringComparer.Ordinal.Equals(left.Id, right.Id)
                || !StringComparer.Ordinal.Equals(left.Title, right.Title)
                || !left.Items.SequenceEqual(right.Items)
            )
            {
                return false;
            }
        }

        return true;
    }

    private static bool TryFindItem(
        IReadOnlyList<GroupState> groups,
        List<NavigationMenuItem> fixedItems,
        string id,
        out ItemLocation location
    )
    {
        foreach (GroupState group in groups)
        {
            int index = FindItemIndex(group.Items, id);
            if (index >= 0)
            {
                location = new ItemLocation(group.Items, index, group.Id, IsFixed: false);
                return true;
            }
        }

        int fixedIndex = FindItemIndex(fixedItems, id);
        if (fixedIndex >= 0)
        {
            location = new ItemLocation(fixedItems, fixedIndex, GroupId: null, IsFixed: true);
            return true;
        }

        location = default;
        return false;
    }

    private static int FindItemIndex(IReadOnlyList<NavigationMenuItem> items, string id)
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

    private sealed class GroupState(
        string id,
        string? title,
        List<NavigationMenuItem> items
    )
    {
        public string Id { get; } = id;

        public string? Title { get; set; } = title;

        public List<NavigationMenuItem> Items { get; } = items;
    }

    private readonly record struct ItemLocation(
        List<NavigationMenuItem> Items,
        int Index,
        string? GroupId,
        bool IsFixed
    );

    private sealed class NavigationMenuEditor(
        List<GroupState> groups,
        List<NavigationMenuItem> fixedItems
    ) : INavigationMenuEditor
    {
        private readonly Lock gate = new();
        private bool isActive = true;

        public void AddGroup(string id, string? title = null)
        {
            Mutate(() => InsertGroupCore(id, title, index: null));
        }

        public void SetGroupIndex(string id, int index, string? title = null)
        {
            Mutate(() => InsertGroupCore(id, title, index));
        }

        public bool RemoveGroup(string id)
        {
            return Mutate(() =>
            {
                int index = FindGroupIndex(id);
                if (index < 0)
                {
                    return false;
                }

                groups.RemoveAt(index);
                return true;
            });
        }

        public void SetGroupOrder(string id, int newIndex)
        {
            Mutate(() =>
            {
                int oldIndex = RequireGroupIndex(id);
                ValidateMoveIndex(newIndex, groups.Count, nameof(newIndex));
                if (oldIndex == newIndex)
                {
                    return;
                }

                GroupState group = groups[oldIndex];
                groups.RemoveAt(oldIndex);
                groups.Insert(newIndex, group);
            });
        }

        public void SetGroupTitle(string id, string? title)
        {
            Mutate(() => groups[RequireGroupIndex(id)].Title = title);
        }

        public void AddItem(string groupId, NavigationMenuItem item)
        {
            Mutate(() => InsertItemCore(groupId, item, index: null));
        }

        public void SetItemIndex(string groupId, NavigationMenuItem item, int index)
        {
            Mutate(() => InsertItemCore(groupId, item, index));
        }

        public void AddFixedItem(NavigationMenuItem item)
        {
            Mutate(() => InsertFixedItemCore(item, index: null));
        }

        public void SetFixedItemIndex(NavigationMenuItem item, int index)
        {
            Mutate(() => InsertFixedItemCore(item, index));
        }

        public void SetItem(
            string? groupId,
            NavigationMenuItem item,
            bool isFixed = false,
            int? index = null
        )
        {
            Mutate(() =>
            {
                ArgumentNullException.ThrowIfNull(item);
                ItemLocation? existing = null;
                if (TryFindItem(groups, fixedItems, item.Id, out ItemLocation location))
                {
                    existing = location;
                    location.Items.RemoveAt(location.Index);
                }

                List<NavigationMenuItem> target = GetTarget(groupId, isFixed);
                int? targetIndex = index;
                if (
                    targetIndex is null
                    && existing is ItemLocation oldLocation
                    && ReferenceEquals(oldLocation.Items, target)
                )
                {
                    targetIndex = oldLocation.Index;
                }

                Insert(target, item, targetIndex);
            });
        }

        public bool RemoveItem(string id)
        {
            return Mutate(() =>
            {
                if (!TryFindItem(groups, fixedItems, id, out ItemLocation location))
                {
                    return false;
                }

                location.Items.RemoveAll(item =>
                    StringComparer.Ordinal.Equals(item.Id, id)
                    || StringComparer.Ordinal.Equals(item.ParentId, id)
                );
                return true;
            });
        }

        public void SetItemPosition(
            string id,
            string? targetGroupId,
            int newIndex,
            bool isFixed = false
        )
        {
            Mutate(() =>
            {
                if (!TryFindItem(groups, fixedItems, id, out ItemLocation location))
                {
                    throw new KeyNotFoundException(
                        $"Navigation item ID '{id}' was not found."
                    );
                }

                NavigationMenuItem[] moving = location
                    .Items.Where(item =>
                        StringComparer.Ordinal.Equals(item.Id, id)
                        || StringComparer.Ordinal.Equals(item.ParentId, id)
                    )
                    .ToArray();
                location.Items.RemoveAll(moving.Contains);
                List<NavigationMenuItem> target = GetTarget(targetGroupId, isFixed);
                if (newIndex < 0 || newIndex > target.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(newIndex));
                }

                target.InsertRange(newIndex, moving);
            });
        }

        public void SetItem(string id, Func<NavigationMenuItem, NavigationMenuItem> update)
        {
            ArgumentNullException.ThrowIfNull(update);
            Mutate(() =>
            {
                if (!TryFindItem(groups, fixedItems, id, out ItemLocation location))
                {
                    throw new KeyNotFoundException(
                        $"Navigation item ID '{id}' was not found."
                    );
                }

                NavigationMenuItem replacement =
                    update(location.Items[location.Index])
                    ?? throw new InvalidOperationException(
                        "The navigation item update returned null."
                    );
                if (!StringComparer.Ordinal.Equals(id, replacement.Id))
                {
                    throw new InvalidOperationException(
                        "A navigation item update cannot change its stable ID."
                    );
                }

                location.Items[location.Index] = replacement;
            });
        }

        public void SetItemVisible(string id, bool visible)
        {
            SetItem(id, item => item with { IsVisible = visible });
        }

        public void SetItemEnabled(string id, bool enabled)
        {
            SetItem(id, item => item with { IsEnabled = enabled });
        }

        public void SetItemExpanded(string id, bool expanded)
        {
            SetItem(id, item => item with { IsExpanded = expanded });
        }

        internal void Complete()
        {
            lock (gate)
            {
                isActive = false;
            }
        }

        private void InsertGroupCore(string id, string? title, int? index)
        {
            ValidateIdArgument(id, nameof(id), "group");
            if (FindGroupIndex(id) >= 0)
            {
                throw new InvalidOperationException(
                    $"Navigation group ID '{id}' is already registered."
                );
            }

            Insert(groups, new GroupState(id, title, []), index);
        }

        private void InsertItemCore(string groupId, NavigationMenuItem item, int? index)
        {
            ArgumentNullException.ThrowIfNull(item);
            EnsureItemIdAvailable(item.Id);
            Insert(groups[RequireGroupIndex(groupId)].Items, item, index);
        }

        private void InsertFixedItemCore(NavigationMenuItem item, int? index)
        {
            ArgumentNullException.ThrowIfNull(item);
            EnsureItemIdAvailable(item.Id);
            Insert(fixedItems, item, index);
        }

        private List<NavigationMenuItem> GetTarget(string? groupId, bool isFixed)
        {
            if (isFixed)
            {
                return fixedItems;
            }

            if (string.IsNullOrWhiteSpace(groupId))
            {
                throw new ArgumentException(
                    "A target group ID is required for non-fixed items.",
                    nameof(groupId)
                );
            }

            return groups[RequireGroupIndex(groupId)].Items;
        }

        private void EnsureItemIdAvailable(string id)
        {
            ValidateIdArgument(id, nameof(id), "item");
            if (TryFindItem(groups, fixedItems, id, out _))
            {
                throw new InvalidOperationException(
                    $"Navigation item ID '{id}' is already registered."
                );
            }
        }

        private int FindGroupIndex(string id)
        {
            return groups.FindIndex(group => StringComparer.Ordinal.Equals(group.Id, id));
        }

        private int RequireGroupIndex(string id)
        {
            ValidateIdArgument(id, nameof(id), "group");
            int index = FindGroupIndex(id);
            return index >= 0
                ? index
                : throw new KeyNotFoundException(
                    $"Navigation group ID '{id}' was not found."
                );
        }

        private void Mutate(Action mutation)
        {
            lock (gate)
            {
                EnsureActive();
                mutation();
            }
        }

        private T Mutate<T>(Func<T> mutation)
        {
            lock (gate)
            {
                EnsureActive();
                return mutation();
            }
        }

        private void EnsureActive()
        {
            if (!isActive)
            {
                throw new InvalidOperationException(
                    "The navigation menu editor cannot be used after its transaction completes."
                );
            }
        }

        private static void ValidateIdArgument(
            string id,
            string parameterName,
            string description
        )
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException(
                    $"A stable {description} ID is required.",
                    parameterName
                );
            }
        }

        private static void Insert<T>(List<T> items, T item, int? index)
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

        private static void ValidateMoveIndex(int index, int count, string parameterName)
        {
            if (index < 0 || index >= count)
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }
    }
}
