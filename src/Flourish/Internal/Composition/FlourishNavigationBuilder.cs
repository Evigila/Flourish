using System.Windows.Controls;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;

namespace ArkheideSystem.Flourish.Internal.Composition;

internal sealed class FlourishNavigationBuilder(FlourishShellOptions options)
    : IFlourishNavigationBuilder
{
    private const int FixedItemsGroupId = int.MaxValue;

    public IFlourishNavigationBuilder SetDirection(
        NavigationPanelDirection direction = NavigationPanelDirection.Left
    )
    {
        ValidateEnum(direction, nameof(direction));
        options.NavigationPanelDirection = direction;
        return this;
    }

    public IFlourishNavigationBuilder SetInitiallyOpen(bool enabled = true)
    {
        options.IsNavigationPanelInitiallyOpen = enabled;
        return this;
    }

    public IFlourishNavigationBuilder SetPanelWidth(
        double openWidth = 220,
        double closedWidth = 56,
        double maxWidth = 420,
        double minWidth = 160
    )
    {
        ValidatePositiveFinite(openWidth, nameof(openWidth));
        NavigationPanelDimensions.ValidateCollapsedWidth(
            closedWidth,
            nameof(closedWidth)
        );
        ValidatePositiveFinite(minWidth, nameof(minWidth));
        ValidatePositiveFinite(maxWidth, nameof(maxWidth));

        if (closedWidth > openWidth)
        {
            throw new ArgumentOutOfRangeException(
                nameof(closedWidth),
                closedWidth,
                "Closed navigation panel width cannot exceed open width."
            );
        }

        if (minWidth > maxWidth)
        {
            throw new ArgumentOutOfRangeException(
                nameof(minWidth),
                minWidth,
                "Minimum navigation panel width cannot exceed maximum width."
            );
        }

        if (openWidth < minWidth || openWidth > maxWidth)
        {
            throw new ArgumentOutOfRangeException(
                nameof(openWidth),
                openWidth,
                "Open navigation panel width must be within the minimum and maximum width range."
            );
        }

        options.OpenPaneWidth = openWidth;
        options.ClosedPaneWidth = closedWidth;
        options.NavigationPaneMinWidth = minWidth;
        options.NavigationPaneMaxWidth = maxWidth;
        return this;
    }

    public IFlourishNavigationBuilder SetTitle(string title)
    {
        options.PaneTitle = title;
        return this;
    }

    public IFlourishNavigationBuilder SetGroup(
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

    public IFlourishNavigationBuilder AddFixedNavigableViewItem<TPage>(
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

    public IFlourishNavigationBuilder AddFixedNavigableItem(
        string displayName,
        string? iconGlyph,
        string? commandKey,
        int parentId = 0,
        int childId = 0
    )
    {
        AddCommandItem(
            options.FixedNavigationItemDefinitions,
            FixedItemsGroupId,
            isFixed: true,
            displayName,
            iconGlyph,
            commandKey,
            parentId,
            childId
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
        var navigationKey =
            FlourishServiceCollectionExtensions.CreateDefaultNavigationKey(pageType);

        items.Add(
            new FlourishNavigationItem(
                navigationKey,
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
        string? iconGlyph,
        string? commandKey,
        int parentId,
        int childId
    )
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException(
                "Navigation command items require a display name.",
                nameof(displayName)
            );
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
            string? iconGlyph,
            string? commandKey,
            int parentId = 0,
            int childId = 0
        )
        {
            AddCommandItem(
                items,
                groupId,
                isFixed,
                displayName,
                iconGlyph,
                commandKey,
                parentId,
                childId
            );
            return this;
        }
    }

    private static void ValidatePositiveFinite(double value, string parameterName)
    {
        ValidateFinite(value, parameterName);
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "Value must be greater than 0."
            );
        }
    }

    private static void ValidateFinite(double value, string parameterName)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            throw new ArgumentOutOfRangeException(parameterName, value, "Value must be finite.");
        }
    }

    private static void ValidateEnum<TEnum>(TEnum value, string parameterName)
        where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(value))
        {
            throw new ArgumentOutOfRangeException(parameterName, value, "Unknown value.");
        }
    }
}
