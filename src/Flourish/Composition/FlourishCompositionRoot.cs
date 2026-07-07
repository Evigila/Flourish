using AckSS.Flourish.Abstract;
using AckSS.Flourish.Configuration;
using AckSS.Flourish.Services;
using AckSS.Flourish.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AckSS.Flourish.Composition;

internal sealed class FlourishCompositionRoot(
    FlourishShellOptions shellOptions,
    FlourishDataOptions dataOptions,
    IReadOnlyList<Action<IFlourishDataBuilder>> dataConfigurations,
    IReadOnlyList<Action<HostBuilderContext, IServiceCollection>> serviceConfigurations,
    IReadOnlyList<Action<IFlourishShellBuilder>> shellConfigurations,
    IReadOnlyList<Action<IFlourishTitlebarBuilder>> titleBarConfigurations,
    IReadOnlyList<Action<IFlourishNavigationBuilder>> navigationConfigurations,
    IReadOnlyList<Action<IFlourishCustomHandlerBuilder>> customHandlerConfigurations,
    IReadOnlyList<Action<IFlourishDynamicToolbarBuilder>> toolbarConfigurations,
    IReadOnlyList<Action<IFlourishTipsBuilder>> tipsConfigurations,
    IReadOnlyList<Action<IFlourishMotionBuilder>> motionConfigurations,
    IReadOnlyList<Action<IFlourishWindowPropertyBuilder>> windowConfigurations,
    IReadOnlyList<Action<IFlourishFooterBuilder>> footerConfigurations
)
{
    private readonly FlourishShellOptions shellOptions = shellOptions;
    private readonly FlourishDataOptions dataOptions = dataOptions;
    private readonly IReadOnlyList<Action<IFlourishDataBuilder>> dataConfigurations =
        dataConfigurations;
    private readonly IReadOnlyList<
        Action<HostBuilderContext, IServiceCollection>
    > serviceConfigurations = serviceConfigurations;
    private readonly IReadOnlyList<Action<IFlourishShellBuilder>> shellConfigurations =
        shellConfigurations;
    private readonly IReadOnlyList<Action<IFlourishTitlebarBuilder>> titleBarConfigurations =
        titleBarConfigurations;
    private readonly IReadOnlyList<Action<IFlourishNavigationBuilder>> navigationConfigurations =
        navigationConfigurations;
    private readonly IReadOnlyList<Action<IFlourishCustomHandlerBuilder>> customHandlerConfigurations =
        customHandlerConfigurations;
    private readonly IReadOnlyList<Action<IFlourishDynamicToolbarBuilder>> toolbarConfigurations =
        toolbarConfigurations;
    private readonly IReadOnlyList<Action<IFlourishTipsBuilder>> tipsConfigurations =
        tipsConfigurations;
    private readonly IReadOnlyList<Action<IFlourishMotionBuilder>> motionConfigurations =
        motionConfigurations;
    private readonly IReadOnlyList<Action<IFlourishWindowPropertyBuilder>> windowConfigurations =
        windowConfigurations;
    private readonly IReadOnlyList<Action<IFlourishFooterBuilder>> footerConfigurations =
        footerConfigurations;

    public void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        foreach (var configureServices in serviceConfigurations)
        {
            configureServices(context, services);
        }

        ApplyFlourishConfigurations();
        ValidateDataConfiguration();
        ApplyServiceCollectionRegistrations(services);
        RegisterCoreServices(services);
    }

    private void ApplyFlourishConfigurations()
    {
        var shellBuilder = new FlourishShellBuilder(shellOptions);
        foreach (var configureShell in shellConfigurations)
        {
            configureShell(shellBuilder);
        }

        var titleBarBuilder = new FlourishTitlebarBuilder(shellOptions);
        foreach (var configureTitleBar in titleBarConfigurations)
        {
            configureTitleBar(titleBarBuilder);
        }

        var navigationBuilder = new FlourishNavigationBuilder(shellOptions);
        foreach (var configureNavigation in navigationConfigurations)
        {
            configureNavigation(navigationBuilder);
        }

        var customHandlerBuilder = new FlourishCustomHandlerBuilder(shellOptions);
        foreach (var configureCustomHandler in customHandlerConfigurations)
        {
            configureCustomHandler(customHandlerBuilder);
        }

        var toolbarBuilder = new FlourishDynamicToolbarBuilder(shellOptions);
        foreach (var configureToolbar in toolbarConfigurations)
        {
            configureToolbar(toolbarBuilder);
        }

        var tipsBuilder = new FlourishTipsBuilder(shellOptions.Tips);
        foreach (var configureTips in tipsConfigurations)
        {
            configureTips(tipsBuilder);
        }

        var motionBuilder = new FlourishMotionBuilder(shellOptions.Motion);
        foreach (var configureMotion in motionConfigurations)
        {
            configureMotion(motionBuilder);
        }

        var windowBuilder = new FlourishWindowPropertyBuilder(shellOptions);
        foreach (var configureWindow in windowConfigurations)
        {
            configureWindow(windowBuilder);
        }

        var footerBuilder = new FlourishFooterBuilder(shellOptions);
        foreach (var configureFooter in footerConfigurations)
        {
            configureFooter(footerBuilder);
        }

        var dataBuilder = new FlourishDataBuilder(dataOptions);
        foreach (var configureData in dataConfigurations)
        {
            configureData(dataBuilder);
        }
    }

    private void ValidateDataConfiguration()
    {
        if (!shellOptions.IsThemeEnabled && !dataOptions.HasConfiguration)
        {
            return;
        }

        dataOptions.GetRequiredAppName(shellOptions);
        dataOptions.GetRequiredCompanyName();
    }

    private void ApplyServiceCollectionRegistrations(IServiceCollection services)
    {
        FlourishServiceCollectionState? state = null;
        if (
            services
                .FirstOrDefault(descriptor =>
                    descriptor.ServiceType == typeof(FlourishServiceCollectionState)
                    && descriptor.ImplementationInstance is FlourishServiceCollectionState
                )
                ?.ImplementationInstance
            is FlourishServiceCollectionState existingState
        )
        {
            state = existingState;
        }

        ApplyNavigationRegistrations(state);
    }

    private void ApplyNavigationRegistrations(FlourishServiceCollectionState? state)
    {
        var registeredPages =
            state
                ?.NavigablePages.GroupBy(page => page.PageType)
                .ToDictionary(group => group.Key, group => group.Last())
            ?? [];
        var hasConfiguredNavigation =
            shellOptions.NavigationGroups.Count > 0
            || shellOptions.FixedNavigationItemDefinitions.Count > 0;

        shellOptions.NavigationItems.Clear();
        shellOptions.FixedNavigationItems.Clear();
        shellOptions.PageCacheModesByPageType.Clear();
        foreach (var page in registeredPages.Values)
        {
            shellOptions.PageCacheModesByPageType[page.PageType] = page.CacheMode;
        }

        var navigationGroups = shellOptions.IsNavigationPanelEnabled
            ? hasConfiguredNavigation
                ? shellOptions
                    .NavigationGroups.OrderBy(group => group.GroupId)
                    .Select(CloneNavigationGroup)
                    .ToList()
                : CreateLegacyNavigationGroups(registeredPages)
            : [];
        var fixedNavigationItems = shellOptions.IsNavigationPanelEnabled
            ? CloneNavigationItems(shellOptions.FixedNavigationItemDefinitions)
            : [];

        ValidateUniqueNavigationPageItems(navigationGroups, fixedNavigationItems);

        foreach (var group in navigationGroups)
        {
            if (!string.IsNullOrWhiteSpace(group.Title))
            {
                shellOptions.NavigationItems.Add(
                    new FlourishNavigationItem(
                        $"group:{group.GroupId}",
                        group.Title,
                        null,
                        group.GroupId,
                        FlourishNavigationItemKind.GroupHeader
                    )
                );
            }

            FinalizeNavigationItems(group.Items, registeredPages, $"group {group.GroupId}");
            shellOptions.NavigationItems.AddRange(group.Items);
        }

        FinalizeNavigationItems(fixedNavigationItems, registeredPages, "fixed navigation items");
        shellOptions.FixedNavigationItems.AddRange(fixedNavigationItems);
    }

    private IReadOnlyList<FlourishNavigationGroup> CreateLegacyNavigationGroups(
        IReadOnlyDictionary<Type, NavigablePageRegistration> registeredPages
    )
    {
        var legacyGroup = new FlourishNavigationGroup(
            0,
            string.IsNullOrWhiteSpace(shellOptions.PaneTitle) ? null : shellOptions.PaneTitle
        );

        foreach (var page in registeredPages.Values)
        {
            legacyGroup.Items.Add(
                new FlourishNavigationItem(
                    page.PageType.FullName ?? page.PageType.Name,
                    page.DisplayName ?? page.PageType.Name,
                    page.IconGlyph,
                    0,
                    FlourishNavigationItemKind.Page,
                    page.PageType
                )
            );
        }

        return [legacyGroup];
    }

    private static FlourishNavigationGroup CloneNavigationGroup(FlourishNavigationGroup source)
    {
        var group = new FlourishNavigationGroup(source.GroupId, source.Title);
        group.Items.AddRange(CloneNavigationItems(source.Items));
        return group;
    }

    private static List<FlourishNavigationItem> CloneNavigationItems(
        IEnumerable<FlourishNavigationItem> sourceItems
    )
    {
        var clonedItems = new List<FlourishNavigationItem>();
        foreach (var item in sourceItems)
        {
            clonedItems.Add(CloneNavigationItem(item));
        }

        return clonedItems;
    }

    private static FlourishNavigationItem CloneNavigationItem(FlourishNavigationItem item)
    {
        return new FlourishNavigationItem(
            item.Key,
            item.Label,
            item.IconGlyph,
            item.GroupId,
            item.Kind,
            item.PageType,
            commandKey: item.CommandKey,
            isInitial: item.IsInitial,
            isFixed: item.IsFixed,
            parentId: item.ParentId,
            childId: item.ChildId
        );
    }

    private static void ValidateUniqueNavigationPageItems(
        IReadOnlyList<FlourishNavigationGroup> navigationGroups,
        IReadOnlyList<FlourishNavigationItem> fixedNavigationItems
    )
    {
        var pageLocationsByType = new Dictionary<Type, List<string>>();

        foreach (var group in navigationGroups)
        {
            AddNavigationPageLocations(
                pageLocationsByType,
                group.Items,
                $"group {group.GroupId}"
            );
        }

        AddNavigationPageLocations(
            pageLocationsByType,
            fixedNavigationItems,
            "fixed navigation items"
        );

        var duplicatePages = pageLocationsByType
            .Where(pair => pair.Value.Count > 1)
            .Select(pair => $"{pair.Key.FullName}: {string.Join(", ", pair.Value)}")
            .ToArray();

        if (duplicatePages.Length > 0)
        {
            throw new InvalidOperationException(
                "A page can only be added to one navigation location. Duplicate navigable pages: "
                    + string.Join("; ", duplicatePages)
            );
        }
    }

    private static void AddNavigationPageLocations(
        Dictionary<Type, List<string>> pageLocationsByType,
        IReadOnlyList<FlourishNavigationItem> items,
        string scopeName
    )
    {
        for (var index = 0; index < items.Count; index++)
        {
            var item = items[index];
            if (!item.IsPageItem || item.PageType is null)
            {
                continue;
            }

            if (!pageLocationsByType.TryGetValue(item.PageType, out var locations))
            {
                locations = [];
                pageLocationsByType[item.PageType] = locations;
            }

            locations.Add($"{scopeName} item {index + 1} ({item.Label})");
        }
    }

    private void FinalizeNavigationItems(
        IReadOnlyList<FlourishNavigationItem> items,
        IReadOnlyDictionary<Type, NavigablePageRegistration> registeredPages,
        string scopeName
    )
    {
        var parentsById = new Dictionary<int, FlourishNavigationItem>();
        foreach (var item in items)
        {
            item.Validate();

            if (item.ParentId != 0 && !parentsById.TryAdd(item.ParentId, item))
            {
                throw new InvalidOperationException(
                    $"Navigation parentId {item.ParentId} is duplicated in {scopeName}."
                );
            }

            if (item.IsPageItem && item.PageType is not null)
            {
                if (!registeredPages.TryGetValue(item.PageType, out var page))
                {
                    throw new InvalidOperationException(
                        $"{item.PageType.FullName} must be registered with AddNavigable before it is added to the navigation panel."
                    );
                }

                if (item.Label == item.PageType.Name && !string.IsNullOrWhiteSpace(page.DisplayName))
                {
                    item.Label = page.DisplayName;
                }

                if (string.IsNullOrWhiteSpace(item.IconGlyph) && page.IconGlyph is not null)
                {
                    item.IconGlyph = page.IconGlyph;
                }
            }

            if (item.IsInitial && item.PageType is not null)
            {
                shellOptions.InitialNavigationPageType = item.PageType;
            }
        }

        foreach (var child in items.Where(item => item.ChildId != 0))
        {
            if (!parentsById.TryGetValue(child.ChildId, out var parent))
            {
                throw new InvalidOperationException(
                    $"Navigation childId {child.ChildId} in {scopeName} does not match a parentId."
                );
            }

            parent.HasChildren = true;
            child.IsVisible = false;
        }
    }

    private void RegisterCoreServices(IServiceCollection services)
    {
        services.AddSingleton(shellOptions);
        services.AddSingleton(dataOptions);
        services.AddSingleton<FlourishShellWindow>();
        services.AddSingleton<FlourishToolbarService>();
        services.AddSingleton<FlourishStatusService>();
        services.AddSingleton<IMessageService, MessageService>();
        services.AddSingleton<TrayIconService>();
        services.AddSingleton<FontService>();
        services.AddSingleton<CommandParser>();
        services.AddSingleton<MaterialEffectService>();
        services.AddSingleton<AppPreferenceService>();
        services.AddSingleton<ThemeService>();
        services.AddSingleton<FlourishMotionService>();
        services.AddSingleton<WindowFrameFixService>();
        services.AddSingleton<PageHistoryService>();
        services.AddSingleton<PageCacheService>();
        services.AddSingleton<NavigationService>();
        services.AddSingleton<INavigationService>(provider =>
            provider.GetRequiredService<NavigationService>()
        );
        services.AddSingleton<IFrameNavigationService>(provider =>
            provider.GetRequiredService<NavigationService>()
        );
    }
}
