using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;

namespace ArkheideSystem.Flourish.Services;

internal sealed class NavigationMenuService : INavigationMenuService
{
    private const int FixedGroupId = int.MaxValue;
    private readonly object gate = new();
    private readonly FlourishShellOptions options;
    private readonly NavigationRouteRegistry routeRegistry;
    private List<GroupState> groups;
    private List<FlourishNavigationMenuItem> fixedItems;
    private long version;

    public NavigationMenuService(
        FlourishShellOptions options,
        NavigationRouteRegistry routeRegistry
    )
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        this.routeRegistry =
            routeRegistry ?? throw new ArgumentNullException(nameof(routeRegistry));
        (groups, fixedItems) = CreateSeedState(options);
        ValidateState(groups, fixedItems);
        if (
            options.NavigationItems.Count == 0
            && options.FixedNavigationItems.Count == 0
            && (groups.Count > 0 || fixedItems.Count > 0)
        )
        {
            RebuildOptions(groups, fixedItems);
        }

        routeRegistry.Changed += RouteRegistry_Changed;
    }

    public event EventHandler<FlourishNavigationMenuChangedEventArgs>? Changed;

    public FlourishNavigationMenuSnapshot Current
    {
        get
        {
            lock (gate)
            {
                return CreateSnapshot(groups, fixedItems, version);
            }
        }
    }

    public void Update(Action<INavigationMenuEditor> update)
    {
        ArgumentNullException.ThrowIfNull(update);
        FlourishNavigationMenuSnapshot previous;
        FlourishNavigationMenuSnapshot current;
        lock (gate)
        {
            previous = CreateSnapshot(groups, fixedItems, version);
            var workingGroups = CloneGroups(groups);
            var workingFixedItems = new List<FlourishNavigationMenuItem>(fixedItems);
            var editor = new NavigationMenuEditor(workingGroups, workingFixedItems);

            update(editor);
            ValidateState(workingGroups, workingFixedItems);
            RebuildOptions(workingGroups, workingFixedItems);

            groups = workingGroups;
            fixedItems = workingFixedItems;
            version++;
            current = CreateSnapshot(groups, fixedItems, version);
        }

        Changed?.Invoke(
            this,
            new FlourishNavigationMenuChangedEventArgs(previous, current)
        );
    }

    internal void RecordExpansion(string itemId, bool expanded)
    {
        lock (gate)
        {
            if (TryFindItem(groups, fixedItems, itemId, out var location))
            {
                location.Items[location.Index] = location.Items[location.Index] with
                {
                    IsExpanded = expanded,
                };
            }
        }
    }

    private void RouteRegistry_Changed(
        object? sender,
        FlourishNavigationRoutesChangedEventArgs e
    )
    {
        if (
            e.ChangeKind == FlourishRuntimeChangeKind.Added
            || (
                e.ChangeKind == FlourishRuntimeChangeKind.Updated
                && e.PreviousRoute?.PageType == e.Route?.PageType
            )
        )
        {
            return;
        }

        var navigationKey = e.PreviousRoute?.NavigationKey ?? e.Route?.NavigationKey;
        if (navigationKey is null)
        {
            return;
        }

        FlourishNavigationMenuSnapshot? previous = null;
        FlourishNavigationMenuSnapshot? current = null;
        lock (gate)
        {
            var hasReferencedItem = groups
                .SelectMany(group => group.Items)
                .Concat(fixedItems)
                .Any(item =>
                    item.Kind == FlourishNavigationMenuItemKind.Page
                    && StringComparer.Ordinal.Equals(item.NavigationKey, navigationKey)
                );
            if (!hasReferencedItem)
            {
                return;
            }

            previous = CreateSnapshot(groups, fixedItems, version);
            if (e.ChangeKind == FlourishRuntimeChangeKind.Removed)
            {
                foreach (var group in groups)
                {
                    group.Items.RemoveAll(item =>
                        StringComparer.Ordinal.Equals(item.NavigationKey, navigationKey)
                    );
                }

                fixedItems.RemoveAll(item =>
                    StringComparer.Ordinal.Equals(item.NavigationKey, navigationKey)
                );
            }

            RebuildOptions(groups, fixedItems);
            version++;
            current = CreateSnapshot(groups, fixedItems, version);
        }

        Changed?.Invoke(
            this,
            new FlourishNavigationMenuChangedEventArgs(previous, current)
        );
    }

    private void RebuildOptions(
        IReadOnlyList<GroupState> newGroups,
        IReadOnlyList<FlourishNavigationMenuItem> newFixedItems
    )
    {
        var navigationItems = new List<FlourishNavigationItem>();
        for (var groupIndex = 0; groupIndex < newGroups.Count; groupIndex++)
        {
            var group = newGroups[groupIndex];
            if (!string.IsNullOrWhiteSpace(group.Title))
            {
                navigationItems.Add(
                    new FlourishNavigationItem(
                        $"group:{group.Id}",
                        group.Title,
                        iconGlyph: null,
                        groupIndex,
                        FlourishNavigationItemKind.GroupHeader,
                        id: $"group-header:{group.Id}"
                    )
                );
            }

            navigationItems.AddRange(CreateInternalItems(group.Items, groupIndex, isFixed: false));
        }

        var fixedNavigationItems = CreateInternalItems(
            newFixedItems,
            FixedGroupId,
            isFixed: true
        );

        options.NavigationItems.Clear();
        options.NavigationItems.AddRange(navigationItems);
        options.FixedNavigationItems.Clear();
        options.FixedNavigationItems.AddRange(fixedNavigationItems);
    }

    private IReadOnlyList<FlourishNavigationItem> CreateInternalItems(
        IReadOnlyList<FlourishNavigationMenuItem> source,
        int groupId,
        bool isFixed
    )
    {
        var parentRelationshipIds = new Dictionary<string, int>(StringComparer.Ordinal);
        var referencedParentIds = source
            .Where(item => item.ParentId is not null)
            .Select(item => item.ParentId!)
            .ToHashSet(StringComparer.Ordinal);
        var nextRelationshipId = 1;
        foreach (var item in source)
        {
            if (referencedParentIds.Contains(item.Id))
            {
                parentRelationshipIds[item.Id] = nextRelationshipId++;
            }
        }

        var publicItemsById = source.ToDictionary(item => item.Id, StringComparer.Ordinal);
        var result = new List<FlourishNavigationItem>(source.Count);
        foreach (var item in source)
        {
            var isPage = item.Kind == FlourishNavigationMenuItemKind.Page;
            var navigationKey = isPage ? item.NavigationKey! : item.Id;
            var pageType = isPage
                ? options.PageTypesByNavigationKey[item.NavigationKey!]
                : null;
            var parentId = parentRelationshipIds.GetValueOrDefault(item.Id);
            var childId = item.ParentId is null
                ? 0
                : parentRelationshipIds[item.ParentId];
            var internalItem = new FlourishNavigationItem(
                navigationKey,
                item.Label,
                item.IconGlyph,
                groupId,
                isPage ? FlourishNavigationItemKind.Page : FlourishNavigationItemKind.Command,
                pageType,
                item.CommandKey,
                isFixed: isFixed,
                parentId: parentId,
                childId: childId,
                id: item.Id
            )
            {
                HasChildren = parentId != 0,
                IsExpanded = item.IsExpanded,
                IsEnabled = item.IsEnabled,
                IsExplicitlyVisible = item.IsVisible,
            };

            if (item.ParentId is not null)
            {
                var parent = publicItemsById[item.ParentId];
                internalItem.IsTreeVisible = parent.IsExpanded && parent.IsVisible;
            }

            result.Add(internalItem);
        }

        return result;
    }

    private static (List<GroupState>, List<FlourishNavigationMenuItem>) CreateSeedState(
        FlourishShellOptions options
    )
    {
        if (
            options.NavigationItems.Count == 0
            && (
                options.NavigationGroups.Count > 0
                || options.FixedNavigationItemDefinitions.Count > 0
            )
        )
        {
            return CreateSeedStateFromBuilderDefinitions(options);
        }

        if (
            options.NavigationItems.Count == 0
            && options.PageTypesByNavigationKey.Count > 0
        )
        {
            var items = options.PageTypesByNavigationKey
                .Select(pair => FlourishNavigationMenuItem.Page(
                    pair.Key,
                    pair.Key,
                    pair.Value.Name
                ))
                .ToList();
            return (
                [
                    new GroupState(
                        "group:0",
                        string.IsNullOrWhiteSpace(options.PaneTitle) ? null : options.PaneTitle,
                        items
                    ),
                ],
                []
            );
        }

        var groups = new List<GroupState>();
        var groupsById = new Dictionary<int, GroupState>();
        var usedItemIds = new HashSet<string>(StringComparer.Ordinal);

        foreach (var item in options.NavigationItems)
        {
            if (!groupsById.TryGetValue(item.GroupId, out var group))
            {
                group = new GroupState($"group:{item.GroupId}", title: null, []);
                groupsById[item.GroupId] = group;
                groups.Add(group);
            }

            if (item.IsGroupHeader)
            {
                group.Title = item.Label;
            }
        }

        foreach (var group in groups)
        {
            var numericId = int.Parse(group.Id.AsSpan("group:".Length));
            var source = options.NavigationItems
                .Where(item => item.GroupId == numericId && item.IsNavigationItem)
                .ToArray();
            group.Items.AddRange(ConvertSeedItems(source, usedItemIds, options));
        }

        var fixedItems = ConvertSeedItems(options.FixedNavigationItems, usedItemIds, options);
        return (groups, fixedItems);
    }

    private static (List<GroupState>, List<FlourishNavigationMenuItem>) CreateSeedStateFromBuilderDefinitions(
        FlourishShellOptions options
    )
    {
        var usedItemIds = new HashSet<string>(StringComparer.Ordinal);
        var groups = options.NavigationGroups
            .OrderBy(group => group.GroupId)
            .Select(group => new GroupState(
                $"group:{group.GroupId}",
                group.Title,
                ConvertSeedItems(group.Items, usedItemIds, options)
            ))
            .ToList();
        var fixedItems = ConvertSeedItems(
            options.FixedNavigationItemDefinitions,
            usedItemIds,
            options
        );
        return (groups, fixedItems);
    }

    private static List<FlourishNavigationMenuItem> ConvertSeedItems(
        IReadOnlyList<FlourishNavigationItem> source,
        HashSet<string> usedItemIds,
        FlourishShellOptions options
    )
    {
        var publicIdsByInternalItem = new Dictionary<FlourishNavigationItem, string>();
        foreach (var item in source)
        {
            var baseId = string.IsNullOrWhiteSpace(item.Id) ? item.Key : item.Id;
            var id = baseId;
            var suffix = 2;
            while (!usedItemIds.Add(id))
            {
                id = $"{baseId}:{suffix++}";
            }

            publicIdsByInternalItem[item] = id;
        }

        var parentsByRelationshipId = source
            .Where(item => item.ParentId != 0)
            .ToDictionary(item => item.ParentId);
        var result = new List<FlourishNavigationMenuItem>(source.Count);
        foreach (var item in source)
        {
            string? parentId = null;
            if (
                item.ChildId != 0
                && parentsByRelationshipId.TryGetValue(item.ChildId, out var parent)
            )
            {
                parentId = publicIdsByInternalItem[parent];
            }

            result.Add(
                new FlourishNavigationMenuItem(
                    publicIdsByInternalItem[item],
                    item.Label,
                    item.IsPageItem
                        ? FlourishNavigationMenuItemKind.Page
                        : FlourishNavigationMenuItemKind.Command,
                    item.IconGlyph,
                    navigationKey: item.IsPageItem
                        ? options.NavigationKeysByPageType.GetValueOrDefault(item.PageType!, item.Key)
                        : null,
                    commandKey: item.CommandKey,
                    parentId: parentId
                )
                {
                    IsVisible = item.IsExplicitlyVisible,
                    IsEnabled = item.IsEnabled,
                    IsExpanded = item.IsExpanded,
                }
            );
        }

        return result;
    }

    private void ValidateState(
        IReadOnlyList<GroupState> newGroups,
        IReadOnlyList<FlourishNavigationMenuItem> newFixedItems
    )
    {
        var groupIds = new HashSet<string>(StringComparer.Ordinal);
        var itemIds = new HashSet<string>(StringComparer.Ordinal);
        var navigationKeys = new HashSet<string>(StringComparer.Ordinal);

        foreach (var group in newGroups)
        {
            ValidateId(group.Id, "group ID");
            if (!groupIds.Add(group.Id))
            {
                throw new InvalidOperationException(
                    $"Navigation group ID '{group.Id}' is duplicated."
                );
            }

            ValidateItems(group.Items, group.Id, itemIds, navigationKeys);
        }

        ValidateItems(newFixedItems, "fixed navigation items", itemIds, navigationKeys);
    }

    private void ValidateItems(
        IReadOnlyList<FlourishNavigationMenuItem> items,
        string scope,
        HashSet<string> globalItemIds,
        HashSet<string> navigationKeys
    )
    {
        var itemsById = new Dictionary<string, FlourishNavigationMenuItem>(StringComparer.Ordinal);
        foreach (var item in items)
        {
            ValidateId(item.Id, "item ID");
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

            if (!itemsById.TryAdd(item.Id, item))
            {
                throw new InvalidOperationException(
                    $"Navigation item ID '{item.Id}' is duplicated in {scope}."
                );
            }

            if (item.Kind == FlourishNavigationMenuItemKind.Page)
            {
                if (string.IsNullOrWhiteSpace(item.NavigationKey))
                {
                    throw new InvalidOperationException(
                        $"Page item '{item.Id}' requires a navigation key."
                    );
                }

                if (!routeRegistry.Contains(item.NavigationKey))
                {
                    throw new InvalidOperationException(
                        $"Navigation key '{item.NavigationKey}' is not registered."
                    );
                }

                if (!navigationKeys.Add(item.NavigationKey))
                {
                    throw new InvalidOperationException(
                        $"Navigation key '{item.NavigationKey}' is already present in the menu."
                    );
                }
            }
        }

        foreach (var item in items.Where(item => item.ParentId is not null))
        {
            if (!itemsById.TryGetValue(item.ParentId!, out var parent))
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

    private static void ValidateId(string id, string description)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new InvalidOperationException($"A stable {description} is required.");
        }
    }

    private static FlourishNavigationMenuSnapshot CreateSnapshot(
        IReadOnlyList<GroupState> sourceGroups,
        IReadOnlyList<FlourishNavigationMenuItem> sourceFixedItems,
        long snapshotVersion
    )
    {
        var snapshotGroups = sourceGroups
            .Select(group => new FlourishNavigationMenuGroup(
                group.Id,
                group.Title,
                group.Items.ToArray()
            ))
            .ToArray();
        return new FlourishNavigationMenuSnapshot(
            snapshotGroups,
            sourceFixedItems.ToArray(),
            snapshotVersion
        );
    }

    private static List<GroupState> CloneGroups(IEnumerable<GroupState> source)
    {
        return source
            .Select(group => new GroupState(group.Id, group.Title, [.. group.Items]))
            .ToList();
    }

    private static bool TryFindItem(
        IReadOnlyList<GroupState> sourceGroups,
        List<FlourishNavigationMenuItem> sourceFixedItems,
        string id,
        out ItemLocation location
    )
    {
        foreach (var group in sourceGroups)
        {
            var index = group.Items.FindIndex(item => StringComparer.Ordinal.Equals(item.Id, id));
            if (index >= 0)
            {
                location = new ItemLocation(group.Items, index, group.Id, IsFixed: false);
                return true;
            }
        }

        var fixedIndex = sourceFixedItems.FindIndex(item =>
            StringComparer.Ordinal.Equals(item.Id, id)
        );
        if (fixedIndex >= 0)
        {
            location = new ItemLocation(sourceFixedItems, fixedIndex, GroupId: null, IsFixed: true);
            return true;
        }

        location = default;
        return false;
    }

    private sealed class GroupState(
        string id,
        string? title,
        List<FlourishNavigationMenuItem> items
    )
    {
        public string Id { get; } = id;

        public string? Title { get; set; } = title;

        public List<FlourishNavigationMenuItem> Items { get; } = items;
    }

    private readonly record struct ItemLocation(
        List<FlourishNavigationMenuItem> Items,
        int Index,
        string? GroupId,
        bool IsFixed
    );

    private sealed class NavigationMenuEditor(
        List<GroupState> groups,
        List<FlourishNavigationMenuItem> fixedItems
    ) : INavigationMenuEditor
    {
        public void AddGroup(string id, string? title = null, int? index = null)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("A stable group ID is required.", nameof(id));
            }

            if (groups.Any(group => StringComparer.Ordinal.Equals(group.Id, id)))
            {
                throw new InvalidOperationException(
                    $"Navigation group ID '{id}' is already registered."
                );
            }

            Insert(groups, new GroupState(id, title, []), index);
        }

        public bool RemoveGroup(string id)
        {
            var index = FindGroupIndex(id);
            if (index < 0)
            {
                return false;
            }

            groups.RemoveAt(index);
            return true;
        }

        public void MoveGroup(string id, int newIndex)
        {
            var oldIndex = RequireGroupIndex(id);
            ValidateMoveIndex(newIndex, groups.Count, nameof(newIndex));
            if (oldIndex == newIndex)
            {
                return;
            }

            var group = groups[oldIndex];
            groups.RemoveAt(oldIndex);
            groups.Insert(newIndex, group);
        }

        public void SetGroupTitle(string id, string? title)
        {
            groups[RequireGroupIndex(id)].Title = title;
        }

        public void AddItem(
            string groupId,
            FlourishNavigationMenuItem item,
            int? index = null
        )
        {
            ArgumentNullException.ThrowIfNull(item);
            EnsureItemIdAvailable(item.Id);
            Insert(groups[RequireGroupIndex(groupId)].Items, item, index);
        }

        public void AddFixedItem(FlourishNavigationMenuItem item, int? index = null)
        {
            ArgumentNullException.ThrowIfNull(item);
            EnsureItemIdAvailable(item.Id);
            Insert(fixedItems, item, index);
        }

        public void UpsertItem(
            string? groupId,
            FlourishNavigationMenuItem item,
            bool isFixed = false,
            int? index = null
        )
        {
            ArgumentNullException.ThrowIfNull(item);
            int? oldIndex = null;
            if (TryFindItem(groups, fixedItems, item.Id, out var existing))
            {
                oldIndex = existing.Index;
                existing.Items.RemoveAt(existing.Index);
            }

            var target = GetTarget(groupId, isFixed);
            Insert(target, item, index ?? oldIndex);
        }

        public bool RemoveItem(string id)
        {
            if (!TryFindItem(groups, fixedItems, id, out var location))
            {
                return false;
            }

            location.Items.RemoveAll(item =>
                StringComparer.Ordinal.Equals(item.Id, id)
                || StringComparer.Ordinal.Equals(item.ParentId, id)
            );
            return true;
        }

        public void MoveItem(
            string id,
            string? targetGroupId,
            int newIndex,
            bool isFixed = false
        )
        {
            if (!TryFindItem(groups, fixedItems, id, out var location))
            {
                throw new KeyNotFoundException($"Navigation item ID '{id}' was not found.");
            }

            var moving = location.Items
                .Where(item =>
                    StringComparer.Ordinal.Equals(item.Id, id)
                    || StringComparer.Ordinal.Equals(item.ParentId, id)
                )
                .ToArray();
            location.Items.RemoveAll(item => moving.Contains(item));
            var target = GetTarget(targetGroupId, isFixed);
            if (newIndex < 0 || newIndex > target.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(newIndex));
            }

            target.InsertRange(newIndex, moving);
        }

        public void UpdateItem(
            string id,
            Func<FlourishNavigationMenuItem, FlourishNavigationMenuItem> update
        )
        {
            ArgumentNullException.ThrowIfNull(update);
            if (!TryFindItem(groups, fixedItems, id, out var location))
            {
                throw new KeyNotFoundException($"Navigation item ID '{id}' was not found.");
            }

            var replacement = update(location.Items[location.Index])
                ?? throw new InvalidOperationException("The navigation item update returned null.");
            if (!StringComparer.Ordinal.Equals(id, replacement.Id))
            {
                throw new InvalidOperationException(
                    "A navigation item update cannot change its stable ID."
                );
            }

            location.Items[location.Index] = replacement;
        }

        public void SetItemVisible(string id, bool visible)
        {
            UpdateItem(id, item => item with { IsVisible = visible });
        }

        public void SetItemEnabled(string id, bool enabled)
        {
            UpdateItem(id, item => item with { IsEnabled = enabled });
        }

        public void SetItemExpanded(string id, bool expanded)
        {
            UpdateItem(id, item => item with { IsExpanded = expanded });
        }

        private List<FlourishNavigationMenuItem> GetTarget(string? groupId, bool isFixed)
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
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("A stable item ID is required.", nameof(id));
            }

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
            var index = FindGroupIndex(id);
            return index >= 0
                ? index
                : throw new KeyNotFoundException(
                    $"Navigation group ID '{id}' was not found."
                );
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
