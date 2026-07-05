using System.Windows.Controls;
using AcksheedSys.Flourish.Abstract;
using AcksheedSys.Flourish.Configuration;

namespace AcksheedSys.Flourish.Composition;

internal sealed class FlourishNavigationPanelBuilder(FlourishShellOptions options)
    : IFlourishNavigationPanelBuilder
{
    private const int FixedItemsGroupId = int.MaxValue;

    public IFlourishNavigationPanelBuilder SetEnabled(bool enabled = true)
    {
        options.IsNavigationPanelEnabled = enabled;
        return this;
    }

    public IFlourishNavigationPanelBuilder SetDirection(
        NavigationPanelDirection direction = NavigationPanelDirection.Left
    )
    {
        options.NavigationPanelDirection = direction;
        return this;
    }

    public IFlourishNavigationPanelBuilder SetInitiallyOpen(bool enabled = true)
    {
        options.IsNavigationPanelInitiallyOpen = enabled;
        return this;
    }

    public IFlourishNavigationPanelBuilder SetTitle(string title)
    {
        options.PaneTitle = title;
        return this;
    }

    public IFlourishNavigationPanelBuilder SetGroup(
        string? displayName = null,
        int groupId = 0,
        Action<IFlourishNavigationGroupBuilder>? configureGroup = null
    )
    {
        if (groupId != 0 && string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException(
                "Navigation groups with a non-zero groupId require a display name.",
                nameof(displayName)
            );
        }

        if (options.NavigationGroups.Any(group => group.GroupId == groupId))
        {
            throw new InvalidOperationException(
                $"Navigation group ID {groupId} has already been configured."
            );
        }

        var group = new FlourishNavigationGroup(groupId, displayName);
        options.NavigationGroups.Add(group);
        configureGroup?.Invoke(new FlourishNavigationGroupBuilder(group.Items, groupId, false));

        return this;
    }

    public IFlourishNavigationPanelBuilder AddFixedNavigableViewItem<TPage>(
        bool isInitial = false,
        int parentId = 0,
        int childId = 0
    )
        where TPage : Page
    {
        AddPageItem(
            options.FixedNavigationItemDefinitions,
            FixedItemsGroupId,
            isFixed: true,
            typeof(TPage),
            isInitial,
            parentId,
            childId
        );
        return this;
    }

    public IFlourishNavigationPanelBuilder AddFixedNavigableItem(
        string displayName,
        string? commandKey,
        int parentId = 0,
        int childId = 0,
        string? iconGlyph = null
    )
    {
        AddCommandItem(
            options.FixedNavigationItemDefinitions,
            FixedItemsGroupId,
            isFixed: true,
            displayName,
            commandKey,
            parentId,
            childId,
            iconGlyph
        );
        return this;
    }

    private static void AddPageItem(
        List<FlourishNavigationItem> items,
        int groupId,
        bool isFixed,
        Type pageType,
        bool isInitial,
        int parentId,
        int childId
    )
    {
        ValidateParentChild(items, parentId, childId);

        var key = pageType.FullName ?? pageType.Name;
        items.Add(
            new FlourishNavigationItem(
                key,
                pageType.Name,
                null,
                groupId,
                FlourishNavigationItemKind.Page,
                pageType,
                isInitial: isInitial,
                isFixed: isFixed,
                parentId: parentId,
                childId: childId
            )
        );
    }

    private static void AddCommandItem(
        List<FlourishNavigationItem> items,
        int groupId,
        bool isFixed,
        string displayName,
        string? commandKey,
        int parentId,
        int childId,
        string? iconGlyph
    )
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("Navigation command items require a display name.", nameof(displayName));
        }

        ValidateParentChild(items, parentId, childId);

        items.Add(
            new FlourishNavigationItem(
                $"command:{groupId}:{items.Count}:{commandKey ?? displayName}",
                displayName,
                iconGlyph,
                groupId,
                FlourishNavigationItemKind.Command,
                commandKey: commandKey,
                isFixed: isFixed,
                parentId: parentId,
                childId: childId
            )
        );
    }

    private static void ValidateParentChild(
        IEnumerable<FlourishNavigationItem> items,
        int parentId,
        int childId
    )
    {
        if (parentId != 0 && childId != 0)
        {
            throw new ArgumentException("parentId and childId cannot both be non-zero.");
        }

        if (parentId != 0 && items.Any(item => item.ParentId == parentId))
        {
            throw new InvalidOperationException(
                $"Navigation parentId {parentId} is already used in this group."
            );
        }
    }

    private sealed class FlourishNavigationGroupBuilder(
        List<FlourishNavigationItem> items,
        int groupId,
        bool isFixed
    ) : IFlourishNavigationGroupBuilder
    {
        public IFlourishNavigationGroupBuilder AddNavigableViewItem<TPage>(
            bool isInitial = false,
            int parentId = 0,
            int childId = 0
        )
            where TPage : Page
        {
            AddPageItem(
                items,
                groupId,
                isFixed,
                typeof(TPage),
                isInitial,
                parentId,
                childId
            );
            return this;
        }

        public IFlourishNavigationGroupBuilder AddNavigableItem(
            string displayName,
            string? commandKey,
            int parentId = 0,
            int childId = 0,
            string? iconGlyph = null
        )
        {
            AddCommandItem(
                items,
                groupId,
                isFixed,
                displayName,
                commandKey,
                parentId,
                childId,
                iconGlyph
            );
            return this;
        }
    }
}
