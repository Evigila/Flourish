using System.Linq;

using System;
using System.Collections.Generic;

using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Configuration;
using ArkheideSystem.Flourish.Appearance;
using ArkheideSystem.Flourish.BackgroundTasks;
using ArkheideSystem.Flourish.Commands;
using ArkheideSystem.Flourish.Layout;
using ArkheideSystem.Flourish.Localization;
using ArkheideSystem.Flourish.Messaging;
using ArkheideSystem.Flourish.Motion;
using ArkheideSystem.Flourish.Navigation;
using ArkheideSystem.Flourish.Profile;
using ArkheideSystem.Flourish.Projects;
using ArkheideSystem.Flourish.Shell.Regions;
using ArkheideSystem.Flourish.Shell.StatusBar;
using ArkheideSystem.Flourish.Shell.TitleBar;
using ArkheideSystem.Flourish.Shell.Toolbar;
using ArkheideSystem.Flourish.ToolTips;
using ArkheideSystem.Flourish.Windowing;
using ArkheideSystem.Flourish.Views.Page;
using ArkheideSystem.Flourish.Views.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace ArkheideSystem.Flourish.Hosting;

internal sealed class FlourishCompositionRoot(
    FlourishApplicationOptions applicationOptions,
    FlourishDataOptions dataOptions,
    IReadOnlyList<Action<HostBuilderContext, IServiceCollection>> serviceConfigurations,
    IReadOnlyList<Action<IAppearanceBuilder>> appearanceConfigurations,
    IReadOnlyList<Action<IFontBuilder>> fontConfigurations,
    IReadOnlyList<Action<ILayoutBuilder>> layoutConfigurations,
    IReadOnlyList<Action<IToolTipBuilder>> toolTipConfigurations,
    IReadOnlyList<Action<IProjectBuilder>> projectConfigurations,
    IReadOnlyList<Action<ITitleBarBuilder>> titleBarConfigurations,
    IReadOnlyList<Action<INavigationBuilder>> navigationConfigurations,
    IReadOnlyList<Action<ICustomContentBuilder>> customHandlerConfigurations,
    IReadOnlyList<Action<IToolbarBuilder>> toolbarConfigurations,
    IReadOnlyList<Action<IMotionBuilder>> motionConfigurations,
    IReadOnlyList<Action<IWindowBuilder>> windowConfigurations,
    IReadOnlyList<Action<IStatusBarBuilder>> statusBarConfigurations
)
{
    private readonly FlourishApplicationOptions applicationOptions = applicationOptions;
    private readonly FlourishDataOptions dataOptions = dataOptions;
    private readonly IReadOnlyList<
        Action<HostBuilderContext, IServiceCollection>
    > serviceConfigurations = serviceConfigurations;
    private readonly IReadOnlyList<Action<IAppearanceBuilder>> appearanceConfigurations =
        appearanceConfigurations;
    private readonly IReadOnlyList<Action<IFontBuilder>> fontConfigurations = fontConfigurations;
    private readonly IReadOnlyList<Action<ILayoutBuilder>> layoutConfigurations =
        layoutConfigurations;
    private readonly IReadOnlyList<Action<IToolTipBuilder>> toolTipConfigurations =
        toolTipConfigurations;
    private readonly IReadOnlyList<Action<IProjectBuilder>> projectConfigurations =
        projectConfigurations;
    private readonly IReadOnlyList<Action<ITitleBarBuilder>> titleBarConfigurations =
        titleBarConfigurations;
    private readonly IReadOnlyList<Action<INavigationBuilder>> navigationConfigurations =
        navigationConfigurations;
    private readonly IReadOnlyList<
        Action<ICustomContentBuilder>
    > customHandlerConfigurations = customHandlerConfigurations;
    private readonly IReadOnlyList<Action<IToolbarBuilder>> toolbarConfigurations =
        toolbarConfigurations;
    private readonly IReadOnlyList<Action<IMotionBuilder>> motionConfigurations =
        motionConfigurations;
    private readonly IReadOnlyList<Action<IWindowBuilder>> windowConfigurations =
        windowConfigurations;
    private readonly IReadOnlyList<Action<IStatusBarBuilder>> statusBarConfigurations =
        statusBarConfigurations;
    private FlourishLocalizationService? localizationService;

    public void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        // Hosted services stop in reverse registration order. Register the settings
        // writer first so every producer can finish queuing its final update before
        // the writer itself is stopped and flushed.
        services.AddSingleton<IHostedService>(provider =>
            provider.GetRequiredService<AppPreferenceService>()
        );
        // Startup command parsers run before application-hosted services and stop after them,
        // keeping their mappings available for the complete application-service lifetime.
        services.AddSingleton<IHostedService, CommandParserHostedService>();
        foreach (var configureServices in serviceConfigurations)
        {
            configureServices(context, services);
        }

        ApplyFlourishConfigurations(services);
        ApplyServiceCollectionRegistrations(services);
        FlourishPreferenceLoader.Apply(context.Configuration, dataOptions, applicationOptions);
        localizationService = new FlourishLocalizationService(dataOptions);
        RegisterCoreServices(services);
    }

    private void ApplyFlourishConfigurations(IServiceCollection services)
    {
        var appearanceBuilder = new AppearanceBuilder(applicationOptions.Appearance);
        ApplyAndFreeze(
            appearanceBuilder,
            (IAppearanceBuilder)appearanceBuilder,
            defaults: null,
            appearanceConfigurations
        );

        var fontBuilder = new FontBuilder(applicationOptions.Appearance);
        ApplyAndFreeze(fontBuilder, (IFontBuilder)fontBuilder, defaults: null, fontConfigurations);

        var layoutBuilder = new LayoutBuilder(applicationOptions.Layout);
        ApplyAndFreeze(
            layoutBuilder,
            (ILayoutBuilder)layoutBuilder,
            defaults: null,
            layoutConfigurations
        );

        var toolTipBuilder = new ToolTipBuilder(applicationOptions.Tips);
        ApplyAndFreeze(
            toolTipBuilder,
            (IToolTipBuilder)toolTipBuilder,
            defaults: null,
            toolTipConfigurations
        );

        var projectBuilder = new ProjectBuilder(applicationOptions.Projects);
        ApplyAndFreeze(
            projectBuilder,
            (IProjectBuilder)projectBuilder,
            defaults: null,
            projectConfigurations
        );

        applicationOptions.Profile.PageType = typeof(FlourishProfilePage);
        var titleBarBuilder = new TitleBarBuilder(
            applicationOptions.TitleBar,
            applicationOptions.Projects,
            applicationOptions.Appearance,
            applicationOptions.Profile
        );
        ApplyAndFreeze(
            titleBarBuilder,
            (ITitleBarBuilder)titleBarBuilder,
            ApplyTitleBarDefaults,
            titleBarConfigurations
        );

        var navigationBuilder = new NavigationBuilder(applicationOptions.Navigation, services);
        ApplyAndFreeze(
            navigationBuilder,
            (INavigationBuilder)navigationBuilder,
            ApplyNavigationDefaults,
            navigationConfigurations
        );

        var customHandlerBuilder = new CustomContentBuilder(applicationOptions.Regions);
        ApplyAndFreeze(
            customHandlerBuilder,
            (ICustomContentBuilder)customHandlerBuilder,
            defaults: null,
            customHandlerConfigurations
        );

        var toolbarBuilder = new ToolbarBuilder(applicationOptions.Toolbar);
        ApplyAndFreeze(
            toolbarBuilder,
            (IToolbarBuilder)toolbarBuilder,
            defaults: null,
            toolbarConfigurations
        );

        var motionBuilder = new MotionBuilder(applicationOptions.Motion);
        ApplyAndFreeze(
            motionBuilder,
            (IMotionBuilder)motionBuilder,
            ApplyMotionDefaults,
            motionConfigurations
        );

        var windowBuilder = new WindowBuilder(applicationOptions.Window);
        ApplyAndFreeze(
            windowBuilder,
            (IWindowBuilder)windowBuilder,
            ApplyWindowDefaults,
            windowConfigurations
        );

        var statusBarBuilder = new StatusBarBuilder(applicationOptions.StatusBar);
        ApplyAndFreeze(
            statusBarBuilder,
            (IStatusBarBuilder)statusBarBuilder,
            ApplyStatusBarDefaults,
            statusBarConfigurations
        );
    }

    private static void ApplyAndFreeze<TBuilder>(
        FlourishBuilderMutationGuard mutationGuard,
        TBuilder builder,
        Action<TBuilder>? defaults,
        IReadOnlyList<Action<TBuilder>> configurations
    )
    {
        try
        {
            defaults?.Invoke(builder);
            foreach (var configure in configurations)
            {
                configure(builder);
            }
        }
        finally
        {
            mutationGuard.Freeze();
        }
    }

    private static void ApplyTitleBarDefaults(ITitleBarBuilder builder) =>
        builder
            .SetBreadcrumbMode()
            .SetNavigationToggle()
            .SetLogo()
            .SetApplicationTitle()
            .SetApplicationSubtitle()
            .SetUnnamedProjectPlaceholder()
            .SetProfile()
            .SetThemeToggle();

    private static void ApplyNavigationDefaults(INavigationBuilder builder) =>
        builder.SetDirection().SetInitiallyOpen().SetPanelWidth();

    private static void ApplyMotionDefaults(IMotionBuilder builder) =>
        builder
            .SetPageTransition()
            .SetNavigationPanelTransition()
            .SetHoverReveal()
            .SetRespectSystemReducedMotion();

    private static void ApplyWindowDefaults(IWindowBuilder builder) =>
        builder
            .SetSize()
            .SetMinimumSize()
            .SetMaximumSize()
            .SetStartupLocation()
            .SetState()
            .SetResizeMode()
            .SetShownInTaskbar();

    private static void ApplyStatusBarDefaults(IStatusBarBuilder builder) =>
        builder.AddStatusItem().SetLanStatusEnabled().SetPowerStatusEnabled();

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
        IReadOnlyList<NavigablePageRegistration> registeredPages = state?.NavigablePages ?? [];
        var registeredPagesByPageType = CreateRegisteredPagesByPageType(registeredPages);
        var registeredPagesByKey = CreateRegisteredPagesByKey(registeredPages);
        var navigationOptions = applicationOptions.Navigation;
        navigationOptions.NavigationItems.Clear();
        navigationOptions.FixedNavigationItems.Clear();
        navigationOptions.InitialNavigationRoutes.Clear();
        navigationOptions.InitialNavigationKey = null;
        navigationOptions.InitialNavigationPageType = null;

        foreach (var page in registeredPagesByKey.Values)
        {
            navigationOptions.InitialNavigationRoutes.Add(
                new FlourishNavigationRoute(page.NavigationKey, page.PageType, page.CacheMode)
            );
        }

        var navigationGroups = navigationOptions.IsNavigationPanelEnabled
            ? navigationOptions
                .NavigationGroups.OrderBy(group => group.GroupId)
                .Select(CloneNavigationGroup)
                .ToList()
            : [];
        var fixedNavigationItems = navigationOptions.IsNavigationPanelEnabled
            ? CloneNavigationItems(navigationOptions.FixedNavigationItemDefinitions)
            : [];

        foreach (var group in navigationGroups)
        {
            FinalizeNavigationItems(
                group.Items,
                registeredPagesByPageType,
                $"group {group.GroupId}"
            );
        }

        FinalizeNavigationItems(
            fixedNavigationItems,
            registeredPagesByPageType,
            "fixed navigation items"
        );

        ValidateUniqueNavigationPageItems(navigationGroups, fixedNavigationItems);
        ApplyInitialNavigationItem(navigationGroups, fixedNavigationItems);

        foreach (var group in navigationGroups)
        {
            if (!string.IsNullOrWhiteSpace(group.Title))
            {
                navigationOptions.NavigationItems.Add(
                    new FlourishNavigationItem(
                        $"group:{group.GroupId}",
                        group.Title,
                        null,
                        group.GroupId,
                        FlourishNavigationItemKind.GroupHeader
                    )
                );
            }

            navigationOptions.NavigationItems.AddRange(group.Items);
        }

        navigationOptions.FixedNavigationItems.AddRange(fixedNavigationItems);
    }

    private static IReadOnlyDictionary<
        Type,
        NavigablePageRegistration
    > CreateRegisteredPagesByPageType(IReadOnlyList<NavigablePageRegistration> registeredPages)
    {
        var duplicatePageTypes = registeredPages
            .GroupBy(page => page.PageType)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key.FullName ?? group.Key.Name)
            .ToArray();

        if (duplicatePageTypes.Length > 0)
        {
            throw new InvalidOperationException(
                "Navigable page types must be unique. Duplicate page types: "
                    + string.Join(", ", duplicatePageTypes)
            );
        }

        return registeredPages.ToDictionary(page => page.PageType);
    }

    private static IReadOnlyDictionary<
        string,
        NavigablePageRegistration
    > CreateRegisteredPagesByKey(IReadOnlyList<NavigablePageRegistration> registeredPages)
    {
        var duplicateKeys = registeredPages
            .GroupBy(page => page.NavigationKey, StringComparer.Ordinal)
            .Where(group => group.Count() > 1)
            .Select(group => new
            {
                Key = group.Key,
                PageTypes = group.Select(page => page.PageType.FullName ?? page.PageType.Name),
            })
            .ToArray();

        if (duplicateKeys.Length > 0)
        {
            throw new InvalidOperationException(
                "Navigation keys must be unique. Duplicate keys: "
                    + string.Join(
                        "; ",
                        duplicateKeys.Select(duplicate =>
                            $"'{duplicate.Key}' ({string.Join(", ", duplicate.PageTypes)})"
                        )
                    )
            );
        }

        return registeredPages.ToDictionary(page => page.NavigationKey, StringComparer.Ordinal);
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
            AddNavigationPageLocations(pageLocationsByType, group.Items, $"group {group.GroupId}");
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

    private void ApplyInitialNavigationItem(
        IReadOnlyList<FlourishNavigationGroup> navigationGroups,
        IReadOnlyList<FlourishNavigationItem> fixedNavigationItems
    )
    {
        var initialItems = navigationGroups
            .SelectMany(group => group.Items)
            .Concat(fixedNavigationItems)
            .Where(item => item.IsInitial && item.IsPageItem && item.PageType is not null)
            .ToArray();

        if (initialItems.Length > 1)
        {
            throw new InvalidOperationException(
                "Only one navigable page can be configured as the initial page."
            );
        }

        if (initialItems.FirstOrDefault() is not { } initialItem)
        {
            return;
        }

        applicationOptions.Navigation.InitialNavigationKey = initialItem.Key;
        applicationOptions.Navigation.InitialNavigationPageType = initialItem.PageType;
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
        IReadOnlyDictionary<Type, NavigablePageRegistration> registeredPagesByPageType,
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

            if (item.IsPageItem)
            {
                var page = registeredPagesByPageType.GetValueOrDefault(item.PageType!);

                if (page is null)
                {
                    throw new InvalidOperationException(
                        $"{item.PageType!.FullName} must be registered with AddNavigable before it is added to the navigation panel."
                    );
                }

                item.Key = page.NavigationKey;
                item.PageType = page.PageType;

                if (
                    (item.Label == page.PageType.Name || item.Label == page.NavigationKey)
                    && !string.IsNullOrWhiteSpace(page.DisplayName)
                )
                {
                    item.Label = page.DisplayName;
                }

                if (string.IsNullOrWhiteSpace(item.IconGlyph) && page.IconGlyph is not null)
                {
                    item.IconGlyph = page.IconGlyph;
                }
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
        services.AddSingleton(
            localizationService
                ?? throw new InvalidOperationException("Flourish localization is not initialized.")
        );
        services.AddSingleton<IFlourishLocalization>(provider =>
            provider.GetRequiredService<FlourishLocalizationService>()
        );
        services.AddSingleton(applicationOptions.Appearance);
        services.AddSingleton(applicationOptions.Window);
        services.AddSingleton(applicationOptions.Layout);
        services.AddSingleton(applicationOptions.Navigation);
        services.AddSingleton(applicationOptions.Projects);
        services.AddSingleton(applicationOptions.TitleBar);
        services.AddSingleton(applicationOptions.Toolbar);
        services.AddSingleton(applicationOptions.StatusBar);
        services.AddSingleton(applicationOptions.Regions);
        services.AddSingleton(applicationOptions.Motion);
        services.AddSingleton(applicationOptions.Tips);
        services.AddSingleton(applicationOptions.Profile);
        services.AddSingleton(dataOptions);
        services.AddSingleton<FlourishShellWindow>();
        services.AddSingleton<NavigationPanelService>();
        services.AddSingleton<NavigationMenuService>();
        services.AddSingletonAdapter<FlourishToolbarService, IToolbarService>();
        services.AddSingletonAdapter<FlourishStatusService, IStatusBarService>();
        services.AddSingletonAdapter<ShellRegionService, IShellRegionService>();
        services.AddSingletonAdapter<FlourishBackgroundTaskService, IBackgroundTaskService>();
        services.AddSingleton<IHostedService>(provider =>
            provider.GetRequiredService<FlourishBackgroundTaskService>()
        );
        services.AddSingleton<IMessageService, MessageService>();
        services.AddSingletonAdapter<NotificationService, INotificationService>();
        services.AddSingletonAdapter<TrayIconService, ITrayService>();
        services.AddSingletonAdapter<FontService, IFontService>();
        services.AddSingletonAdapter<CommandDispatcher, ICommandRegistry>();
        services.AddSingletonAlias<CommandDispatcher, ICommandDispatcher>();
        services.AddSingletonAdapter<ShortcutService, IShortcutService>();
        services.AddSingletonAdapter<MaterialEffectService, IMaterialEffectService>();
        services.AddSingletonAdapter<AppearanceService, IAppearanceService>();
        services.AddSingletonAdapter<ContentLayoutService, IContentLayoutService>();
        services.AddSingletonAdapter<AppPreferenceService, IFlourishSettingsStore>();
        services.AddSingleton<FlourishPreferencePersistenceService>();
        services.AddSingleton<IHostedService>(provider =>
            provider.GetRequiredService<FlourishPreferencePersistenceService>()
        );
        services.AddSingleton<ProfileSecretStore>();
        services.TryAddSingleton<IProfileAuthService, SimpleProfileAuthService>();
        services.TryAddSingleton<IProfileService, ProfileService>();
        services.AddSingletonAdapter<ThemeService, IThemeService>();
        services.AddSingletonAdapter<FlourishMotionService, IMotionService>();
        services.AddSingletonAdapter<FlourishToolTipService, IToolTipService>();
        services.AddSingletonAdapter<ScrollService, IScrollService>();
        services.AddSingleton<TitleBarService>();
        services.AddSingleton<TitleBarSearchService>();
        services.AddSingletonAdapter<TitleBarRuntimeFacade, ITitleBarService>();
        services.AddSingletonAdapter<ProjectCatalogStore, IProjectCatalogStore>();
        services.AddSingletonAdapter<ProjectService, IProjectService>();
        services.AddSingleton<IProjectSaveFileDialog, ProjectSaveFileDialog>();
        services.TryAddSingleton<IProjectBehavior, DefaultProjectBehavior>();
        services.AddSingletonAdapter<WindowService, IWindowService>();
        services.AddSingletonAdapter<WindowCloseService, IWindowCloseService>();
        services.AddSingletonAdapter<ProfileFlyoutService, IProfileFlyoutService>();
        services.AddSingleton<WindowFrameFixService>();
        services.AddSingleton<NavigationRouteRegistry>();
        services.AddSingleton<PageHistoryService>();
        services.AddSingleton<PageCacheService>();
        services.AddSingleton<NavigationService>();
        services.AddSingletonAdapter<NavigationRuntimeFacade, INavigationService>();
    }
}

internal static class FlourishSingletonRegistrationExtensions
{
    internal static IServiceCollection AddSingletonAdapter<TConcrete, TContract>(
        this IServiceCollection services
    )
        where TConcrete : class, TContract
        where TContract : class
    {
        services.AddSingleton<TConcrete>();
        return services.AddSingletonAlias<TConcrete, TContract>();
    }

    internal static IServiceCollection AddSingletonAlias<TConcrete, TContract>(
        this IServiceCollection services
    )
        where TConcrete : class, TContract
        where TContract : class =>
        services.AddSingleton<TContract>(provider => provider.GetRequiredService<TConcrete>());
}
