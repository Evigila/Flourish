using System.Windows.Controls;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;

namespace ArkheideSystem.Flourish.Internal.Composition;

internal sealed class FlourishNavigationBuilder(FlourishShellOptions options)
    : FlourishBuilderMutationGuard,
        IFlourishNavigationBuilder
{
    private const int FixedItemsGroupId = int.MaxValue;

    public IFlourishNavigationBuilder InitDirection(
        NavigationPanelDirection direction = NavigationPanelDirection.Left,
        bool usePersistedPreference = true
    )
    {
        ThrowIfFrozen();
        ValidateEnum(direction, nameof(direction));
        options.NavigationPanelDirection = direction;
        options.UsePersistedNavigationDirection = usePersistedPreference;
        return this;
    }

    public IFlourishNavigationBuilder InitInitiallyOpen(
        bool enabled = true,
        bool usePersistedPreference = true
    )
    {
        ThrowIfFrozen();
        options.IsNavigationPanelInitiallyOpen = enabled;
        options.UsePersistedNavigationOpenState = usePersistedPreference;
        return this;
    }

    public IFlourishNavigationBuilder InitPanelWidth(
        double openWidth = 250,
        double closedWidth = 64,
        double maxWidth = 520,
        double minWidth = 180,
        bool usePersistedPreference = true
    )
    {
        ThrowIfFrozen();
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
        options.UsePersistedNavigationWidth = usePersistedPreference;
        return this;
    }

    public IFlourishNavigationBuilder UseLastNavigation(
        bool usePersistedPreference = true
    )
    {
        ThrowIfFrozen();
        options.UsePersistedLastNavigation = usePersistedPreference;
        return this;
    }

    public IFlourishNavigationBuilder InitGroup(
        string? displayName = null,
        int groupId = 0,
        Action<IFlourishNavigationGroupBuilder>? configureGroup = null
    )
    {
        ThrowIfFrozen();
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
        if (configureGroup is not null)
        {
            var groupBuilder = new FlourishNavigationGroupBuilder(
                group.Items,
                groupId,
                false
            );
            try
            {
                configureGroup(groupBuilder);
            }
            finally
            {
                groupBuilder.Freeze();
            }
        }

        return this;
    }

    public IFlourishNavigationBuilder InitFixedNavigableViewItem<TPage>(
        bool isInitial = false,
        int parentId = 0,
        int childId = 0
    )
        where TPage : Page
    {
        ThrowIfFrozen();
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

    public IFlourishNavigationBuilder InitFixedNavigableItem(
        string displayName,
        string? iconGlyph,
        string? commandKey,
        int parentId = 0,
        int childId = 0
    )
    {
        ThrowIfFrozen();
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
    ) : FlourishBuilderMutationGuard, IFlourishNavigationGroupBuilder
    {
        public IFlourishNavigationGroupBuilder InitNavigableViewItem<TPage>(
            bool isInitial = false,
            int parentId = 0,
            int childId = 0
        )
            where TPage : Page
        {
            ThrowIfFrozen();
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

        public IFlourishNavigationGroupBuilder InitNavigableItem(
            string displayName,
            string? iconGlyph,
            string? commandKey,
            int parentId = 0,
            int childId = 0
        )
        {
            ThrowIfFrozen();
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
