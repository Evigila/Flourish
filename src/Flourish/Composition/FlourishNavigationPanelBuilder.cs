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
        int GroupID = 0,
        Action<IFlourishNavigationGroupBuilder>? configureGroup = null
    )
    {
        if (GroupID != 0 && string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException(
                "Navigation groups with a non-zero GroupID require a display name.",
                nameof(displayName)
            );
        }

        if (options.NavigationGroups.Any(group => group.GroupId == GroupID))
        {
            throw new InvalidOperationException(
                $"Navigation group ID {GroupID} has already been configured."
            );
        }

        var group = new FlourishNavigationGroup(GroupID, displayName);
        options.NavigationGroups.Add(group);
        configureGroup?.Invoke(new FlourishNavigationGroupBuilder(group.Items, GroupID, false));

        return this;
    }

    public IFlourishNavigationPanelBuilder AddFixedNavigableViewItem<TPage>(
        bool isInitial = false,
        int parentID = 0,
        int childID = 0
    )
        where TPage : Page
    {
        AddPageItem(
            options.FixedNavigationItems,
            FixedItemsGroupId,
            isFixed: true,
            typeof(TPage),
            isInitial,
            parentID,
            childID
        );
        return this;
    }

    public IFlourishNavigationPanelBuilder AddFixedNavigableItem(
        string displayName,
        string? CommandKey,
        int parentID = 0,
        int childID = 0,
        string? iconGlyph = null
    )
    {
        AddCommandItem(
            options.FixedNavigationItems,
            FixedItemsGroupId,
            isFixed: true,
            displayName,
            CommandKey,
            parentID,
            childID,
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
        int parentID,
        int childID
    )
    {
        ValidateParentChild(items, parentID, childID);

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
                parentId: parentID,
                childId: childID
            )
        );
    }

    private static void AddCommandItem(
        List<FlourishNavigationItem> items,
        int groupId,
        bool isFixed,
        string displayName,
        string? commandKey,
        int parentID,
        int childID,
        string? iconGlyph
    )
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("Navigation command items require a display name.", nameof(displayName));
        }

        ValidateParentChild(items, parentID, childID);

        items.Add(
            new FlourishNavigationItem(
                $"command:{groupId}:{items.Count}:{commandKey ?? displayName}",
                displayName,
                iconGlyph,
                groupId,
                FlourishNavigationItemKind.Command,
                commandKey: commandKey,
                isFixed: isFixed,
                parentId: parentID,
                childId: childID
            )
        );
    }

    private static void ValidateParentChild(
        IEnumerable<FlourishNavigationItem> items,
        int parentID,
        int childID
    )
    {
        if (parentID != 0 && childID != 0)
        {
            throw new ArgumentException("parentID and childID cannot both be non-zero.");
        }

        if (parentID != 0 && items.Any(item => item.ParentId == parentID))
        {
            throw new InvalidOperationException(
                $"Navigation parentID {parentID} is already used in this group."
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
            int parentID = 0,
            int childID = 0
        )
            where TPage : Page
        {
            AddPageItem(
                items,
                groupId,
                isFixed,
                typeof(TPage),
                isInitial,
                parentID,
                childID
            );
            return this;
        }

        public IFlourishNavigationGroupBuilder AddNavigableItem(
            string displayName,
            string? CommandKey,
            int parentID = 0,
            int childID = 0,
            string? iconGlyph = null
        )
        {
            AddCommandItem(
                items,
                groupId,
                isFixed,
                displayName,
                CommandKey,
                parentID,
                childID,
                iconGlyph
            );
            return this;
        }
    }
}
