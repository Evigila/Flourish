using System.Linq;

using System;
using System.Collections.Generic;

using ArkheideSystem.Flourish.Abstract;
using System.Windows.Controls;
using ArkheideSystem.Flourish.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ArkheideSystem.Flourish.Navigation;

internal sealed class NavigationBuilder(
    NavigationOptions options,
    IServiceCollection services
)
    : BuilderMutationGuard,
        INavigationBuilder
{
    private const int FixedItemsGroupId = int.MaxValue;

    internal NavigationBuilder(NavigationOptions options)
        : this(options, new ServiceCollection()) { }

    public INavigationBuilder SetEnabled(bool enabled = true)
    {
        ThrowIfFrozen();
        options.IsNavigationPanelEnabled = enabled;
        return this;
    }

    public INavigationBuilder AddNavigable<TPage>(
        string displayName,
        string iconGlyph,
        PageCacheMode cacheMode = PageCacheMode.Enabled,
        bool isInitial = false,
        int groupId = 0,
        string? groupName = null,
        int parentId = 0,
        int childId = 0
    )
        where TPage : Page
    {
        ThrowIfFrozen();
        if (groupId != 0 && string.IsNullOrWhiteSpace(groupName))
        {
            throw new ArgumentException(
                "A non-default navigation group requires a display name.",
                nameof(groupName)
            );
        }

        var group = options.NavigationGroups.FirstOrDefault(candidate =>
            candidate.GroupId == groupId
        );
        if (group is null)
        {
            group = new NavigationGroupDefinition(groupId, groupName);
            options.NavigationGroups.Add(group);
        }
        else if (
            groupId != 0
            && !string.Equals(group.Title, groupName, StringComparison.Ordinal)
        )
        {
            throw new InvalidOperationException(
                $"Navigation group ID {groupId} is already configured with a different name."
            );
        }

        services.AddNavigable<TPage>(displayName, iconGlyph, cacheMode);
        AddPageItem(group.Items, groupId, false, typeof(TPage), isInitial, parentId, childId);
        return this;
    }


    public INavigationBuilder SetDirection(
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

    public INavigationBuilder SetInitiallyOpen(
        bool enabled = true,
        bool usePersistedPreference = true
    )
    {
        ThrowIfFrozen();
        options.IsNavigationPanelInitiallyOpen = enabled;
        options.UsePersistedNavigationOpenState = usePersistedPreference;
        return this;
    }

    public INavigationBuilder SetPanelWidth(
        double openWidth = 250,
        double closedWidth = 64,
        double maxWidth = 520,
        double minWidth = 180,
        bool usePersistedPreference = true
    )
    {
        ThrowIfFrozen();
        ValidatePositiveFinite(openWidth, nameof(openWidth));
        NavigationPanelDimensions.ValidateCollapsedWidth(closedWidth, nameof(closedWidth));
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

    public INavigationBuilder SetLastNavigationPersistence(bool enabled = true)
    {
        ThrowIfFrozen();
        options.UsePersistedLastNavigation = enabled;
        return this;
    }

    public INavigationBuilder AddGroup(
        string? displayName = null,
        int groupId = 0,
        Action<INavigationGroupBuilder>? configureGroup = null
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

        var group = new NavigationGroupDefinition(groupId, displayName);
        options.NavigationGroups.Add(group);
        if (configureGroup is not null)
        {
            var groupBuilder = new NavigationGroupBuilder(group.Items, groupId, false);
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

    public INavigationBuilder AddFixedNavigableViewItem<TPage>(
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

    public INavigationBuilder AddFixedNavigableItem(
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
        List<NavigationItemDefinition> items,
        int groupId,
        bool isFixed,
        Type pageType,
        bool isInitial,
        int parentId,
        int childId
    )
    {
        ValidateParentChild(items, parentId, childId);
        var navigationKey = ServiceCollectionExtensions.CreateDefaultNavigationKey(
            pageType
        );

        items.Add(
            new NavigationItemDefinition(
                navigationKey,
                pageType.Name,
                null,
                groupId,
                NavigationItemKind.Page,
                pageType,
                isInitial: isInitial,
                isFixed: isFixed,
                parentId: parentId,
                childId: childId
            )
        );
    }

    private static void AddCommandItem(
        List<NavigationItemDefinition> items,
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
            new NavigationItemDefinition(
                $"command:{groupId}:{items.Count}:{commandKey ?? displayName}",
                displayName,
                iconGlyph,
                groupId,
                NavigationItemKind.Command,
                commandKey: commandKey,
                isFixed: isFixed,
                parentId: parentId,
                childId: childId
            )
        );
    }

    private static void ValidateParentChild(
        IEnumerable<NavigationItemDefinition> items,
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

    private sealed class NavigationGroupBuilder(
        List<NavigationItemDefinition> items,
        int groupId,
        bool isFixed
    ) : BuilderMutationGuard, INavigationGroupBuilder
    {
        public INavigationGroupBuilder AddNavigableViewItem<TPage>(
            bool isInitial = false,
            int parentId = 0,
            int childId = 0
        )
            where TPage : Page
        {
            ThrowIfFrozen();
            AddPageItem(items, groupId, isFixed, typeof(TPage), isInitial, parentId, childId);
            return this;
        }

        public INavigationGroupBuilder AddNavigableItem(
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
