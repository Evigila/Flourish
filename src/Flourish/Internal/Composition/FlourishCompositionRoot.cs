using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;
using ArkheideSystem.Flourish.Services;
using ArkheideSystem.Flourish.Views.Page;
using ArkheideSystem.Flourish.Views.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace ArkheideSystem.Flourish.Internal.Composition;

internal sealed class FlourishCompositionRoot(
    FlourishShellOptions shellOptions,
    FlourishDataOptions dataOptions,
    IReadOnlyList<Action<HostBuilderContext, IServiceCollection>> serviceConfigurations,
    IReadOnlyList<Action<IFlourishShellBuilder>> shellConfigurations,
    IReadOnlyList<Action<IFlourishTitlebarBuilder>> titleBarConfigurations,
    IReadOnlyList<Action<IFlourishNavigationBuilder>> navigationConfigurations,
    IReadOnlyList<Action<IFlourishCustomHandlerBuilder>> customHandlerConfigurations,
    IReadOnlyList<Action<IFlourishDynamicToolbarBuilder>> toolbarConfigurations,
    IReadOnlyList<Action<IFlourishMotionBuilder>> motionConfigurations,
    IReadOnlyList<Action<IFlourishWindowPropertyBuilder>> windowConfigurations,
    IReadOnlyList<Action<IFlourishStatusBarBuilder>> statusBarConfigurations
)
{
    private readonly FlourishShellOptions shellOptions = shellOptions;
    private readonly FlourishDataOptions dataOptions = dataOptions;
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
    private readonly IReadOnlyList<Action<IFlourishMotionBuilder>> motionConfigurations =
        motionConfigurations;
    private readonly IReadOnlyList<Action<IFlourishWindowPropertyBuilder>> windowConfigurations =
        windowConfigurations;
    private readonly IReadOnlyList<Action<IFlourishStatusBarBuilder>> statusBarConfigurations =
        statusBarConfigurations;
    private FlourishLocalizationService? localizationService;

    public void ConfigServices(HostBuilderContext context, IServiceCollection services)
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

        ApplyFlourishConfigurations();
        ApplyServiceCollectionRegistrations(services);
        FlourishPreferenceConfiguration.Apply(context.Configuration, dataOptions, shellOptions);
        localizationService = new FlourishLocalizationService(dataOptions);
        RegisterCoreServices(services);
    }

    private void ApplyFlourishConfigurations()
    {
        var shellBuilder = new FlourishShellBuilder(shellOptions);
        ApplyAndFreeze(
            shellBuilder,
            (IFlourishShellBuilder)shellBuilder,
            defaults: null,
            shellConfigurations
        );

        shellOptions.Profile.PageType = typeof(FlourishProfilePage);
        var titleBarBuilder = new FlourishTitlebarBuilder(shellOptions);
        ApplyAndFreeze(
            titleBarBuilder,
            (IFlourishTitlebarBuilder)titleBarBuilder,
            ApplyTitleBarDefaults,
            titleBarConfigurations
        );

        var navigationBuilder = new FlourishNavigationBuilder(shellOptions);
        ApplyAndFreeze(
            navigationBuilder,
            (IFlourishNavigationBuilder)navigationBuilder,
            ApplyNavigationDefaults,
            navigationConfigurations
        );

        var customHandlerBuilder = new FlourishCustomHandlerBuilder(shellOptions);
        ApplyAndFreeze(
            customHandlerBuilder,
            (IFlourishCustomHandlerBuilder)customHandlerBuilder,
            defaults: null,
            customHandlerConfigurations
        );

        var toolbarBuilder = new FlourishDynamicToolbarBuilder(shellOptions);
        ApplyAndFreeze(
            toolbarBuilder,
            (IFlourishDynamicToolbarBuilder)toolbarBuilder,
            defaults: null,
            toolbarConfigurations
        );

        var motionBuilder = new FlourishMotionBuilder(shellOptions.Motion);
        ApplyAndFreeze(
            motionBuilder,
            (IFlourishMotionBuilder)motionBuilder,
            ApplyMotionDefaults,
            motionConfigurations
        );

        var windowBuilder = new FlourishWindowPropertyBuilder(shellOptions);
        ApplyAndFreeze(
            windowBuilder,
            (IFlourishWindowPropertyBuilder)windowBuilder,
            ApplyWindowDefaults,
            windowConfigurations
        );

        var statusBarBuilder = new FlourishStatusBarBuilder(shellOptions);
        ApplyAndFreeze(
            statusBarBuilder,
            (IFlourishStatusBarBuilder)statusBarBuilder,
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

    private static void ApplyTitleBarDefaults(IFlourishTitlebarBuilder builder) =>
        builder
            .UseBreadcrumb()
            .UseNavigationToggle()
            .UseLogo()
            .InitApplicationTitle()
            .InitApplicationSubTitle()
            .InitUnnamedProjectPlaceholder()
            .UseProfile()
            .UseThemeToggle();

    private static void ApplyNavigationDefaults(IFlourishNavigationBuilder builder) =>
        builder.InitDirection().InitInitiallyOpen().InitPanelWidth();

    private static void ApplyMotionDefaults(IFlourishMotionBuilder builder) =>
        builder
            .UsePageTransition()
            .UseNavigationPanelTransition()
            .UseHoverRevealAnimation()
            .UseSystemReducedMotion();

    private static void ApplyWindowDefaults(IFlourishWindowPropertyBuilder builder) =>
        builder
            .InitWindowSize()
            .InitWindowMinSize()
            .InitWindowMaxSize()
            .InitWindowPosition()
            .InitWindowState()
            .InitWindowResizeMode()
            .InitShownInTaskbar();

    private static void ApplyStatusBarDefaults(IFlourishStatusBarBuilder builder) =>
        builder.AddStatusItem().UseLanConnectionStatus().UsePowerStatus();

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
        IReadOnlyList<NavigablePageRegistration> registeredPages =
            state?.NavigablePages ?? [];
        var registeredPagesByPageType = CreateRegisteredPagesByPageType(registeredPages);
        var registeredPagesByKey = CreateRegisteredPagesByKey(registeredPages);
        shellOptions.NavigationItems.Clear();
        shellOptions.FixedNavigationItems.Clear();
        shellOptions.InitialNavigationRoutes.Clear();
        shellOptions.InitialNavigationKey = null;
        shellOptions.InitialNavigationPageType = null;

        foreach (var page in registeredPagesByKey.Values)
        {
            shellOptions.InitialNavigationRoutes.Add(
                new FlourishNavigationRoute(
                    page.NavigationKey,
                    page.PageType,
                    page.CacheMode
                )
            );
        }

        var navigationGroups = shellOptions.IsNavigationPanelEnabled
            ? shellOptions
                .NavigationGroups.OrderBy(group => group.GroupId)
                .Select(CloneNavigationGroup)
                .ToList()
            : [];
        var fixedNavigationItems = shellOptions.IsNavigationPanelEnabled
            ? CloneNavigationItems(shellOptions.FixedNavigationItemDefinitions)
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

            shellOptions.NavigationItems.AddRange(group.Items);
        }

        shellOptions.FixedNavigationItems.AddRange(fixedNavigationItems);
    }

    private static IReadOnlyDictionary<Type, NavigablePageRegistration> CreateRegisteredPagesByPageType(
        IReadOnlyList<NavigablePageRegistration> registeredPages
    )
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

    private static IReadOnlyDictionary<string, NavigablePageRegistration> CreateRegisteredPagesByKey(
        IReadOnlyList<NavigablePageRegistration> registeredPages
    )
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

        shellOptions.InitialNavigationKey = initialItem.Key;
        shellOptions.InitialNavigationPageType = initialItem.PageType;
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
        services.AddSingleton(shellOptions);
        services.AddSingleton(shellOptions.Profile);
        services.AddSingleton(dataOptions);
        services.AddSingletonAdapter<
            FlourishConfigurationService,
            IFlourishConfiguration
        >();
        services.AddSingleton<FlourishShellWindow>();
        services.AddSingletonAdapter<NavigationPanelService, INavigationPanelService>();
        services.AddSingletonAdapter<NavigationMenuService, INavigationMenuService>();
        services.AddSingletonAdapter<FlourishToolbarService, IToolbarService>();
        services.AddSingletonAdapter<FlourishStatusService, IStatusBarService>();
        services.AddSingletonAdapter<ShellRegionService, IShellRegionService>();
        services.AddSingletonAdapter<
            FlourishBackgroundTaskService,
            IBackgroundTaskService
        >();
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
        services.AddSingletonAdapter<TitleBarService, ITitleBarService>();
        services.AddSingletonAdapter<ProjectCatalogStore, IProjectCatalogStore>();
        services.AddSingletonAdapter<ProjectService, IProjectService>();
        services.AddSingleton<IProjectSaveFileDialog, ProjectSaveFileDialog>();
        services.TryAddSingleton<IProjectBehavior, DefaultProjectBehavior>();
        services.AddSingletonAdapter<TitleBarSearchService, ITitleBarSearchService>();
        services.AddSingletonAdapter<WindowService, IWindowService>();
        services.AddSingletonAdapter<WindowCloseService, IWindowCloseService>();
        services.AddSingletonAdapter<ProfileFlyoutService, IProfileFlyoutService>();
        services.AddSingleton<WindowFrameFixService>();
        services.AddSingletonAdapter<NavigationRouteRegistry, INavigationRouteRegistry>();
        services.AddSingleton<PageHistoryService>();
        services.AddSingletonAdapter<PageCacheService, IPageCacheService>();
        services.AddSingletonAdapter<NavigationService, INavigationService>();
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
        services.AddSingleton<TContract>(provider =>
            provider.GetRequiredService<TConcrete>()
        );
}
