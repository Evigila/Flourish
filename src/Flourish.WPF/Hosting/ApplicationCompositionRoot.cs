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

internal sealed class ApplicationCompositionRoot(
    ApplicationOptions applicationOptions,
    ApplicationDataOptions dataOptions,
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
    private readonly ApplicationOptions applicationOptions = applicationOptions;
    private readonly ApplicationDataOptions dataOptions = dataOptions;
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
    private LocalizationService? localizationService;

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
        PreferenceLoader.Apply(context.Configuration, dataOptions, applicationOptions);
        localizationService = new LocalizationService(dataOptions);
        services.AddCoreServices(
            new CoreServiceOptions(
                dataOptions,
                localizationService
                    ?? throw new InvalidOperationException(
                        "Flourish localization is not initialized."
                    ),
                applicationOptions.Layout,
                applicationOptions.Projects,
                applicationOptions.TitleBar,
                applicationOptions.StatusBar,
                applicationOptions.Motion,
                applicationOptions.Profile
            )
        );
        RegisterWpfServices(services);
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

        applicationOptions.ProfileView.PageType = typeof(ProfilePage);
        var titleBarBuilder = new TitleBarBuilder(
            applicationOptions.TitleBar,
            applicationOptions.Projects,
            applicationOptions.Appearance,
            applicationOptions.Profile,
            applicationOptions.ProfileView
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
        BuilderMutationGuard mutationGuard,
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
        NavigablePageRegistrationState? state = null;
        if (
            services
                .FirstOrDefault(descriptor =>
                    descriptor.ServiceType == typeof(NavigablePageRegistrationState)
                    && descriptor.ImplementationInstance is NavigablePageRegistrationState
                )
                ?.ImplementationInstance
            is NavigablePageRegistrationState existingState
        )
        {
            state = existingState;
        }

        ApplyNavigationRegistrations(state);
    }

    private void ApplyNavigationRegistrations(NavigablePageRegistrationState? state)
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
                new NavigationRoute(page.NavigationKey, page.PageType, page.CacheMode)
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
                    new NavigationItemDefinition(
                        $"group:{group.GroupId}",
                        group.Title,
                        null,
                        group.GroupId,
                        NavigationItemKind.GroupHeader
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

    private static NavigationGroupDefinition CloneNavigationGroup(NavigationGroupDefinition source)
    {
        var group = new NavigationGroupDefinition(source.GroupId, source.Title);
        group.Items.AddRange(CloneNavigationItems(source.Items));
        return group;
    }

    private static List<NavigationItemDefinition> CloneNavigationItems(
        IEnumerable<NavigationItemDefinition> sourceItems
    )
    {
        var clonedItems = new List<NavigationItemDefinition>();
        foreach (var item in sourceItems)
        {
            clonedItems.Add(CloneNavigationItem(item));
        }

        return clonedItems;
    }

    private static NavigationItemDefinition CloneNavigationItem(NavigationItemDefinition item)
    {
        return new NavigationItemDefinition(
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
        IReadOnlyList<NavigationGroupDefinition> navigationGroups,
        IReadOnlyList<NavigationItemDefinition> fixedNavigationItems
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
        IReadOnlyList<NavigationGroupDefinition> navigationGroups,
        IReadOnlyList<NavigationItemDefinition> fixedNavigationItems
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
        IReadOnlyList<NavigationItemDefinition> items,
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
        IReadOnlyList<NavigationItemDefinition> items,
        IReadOnlyDictionary<Type, NavigablePageRegistration> registeredPagesByPageType,
        string scopeName
    )
    {
        var parentsById = new Dictionary<int, NavigationItemDefinition>();
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

    private void RegisterWpfServices(IServiceCollection services)
    {
        services.AddSingleton(applicationOptions.Appearance);
        services.AddSingleton(applicationOptions.Window);
        services.AddSingleton(applicationOptions.Navigation);
        services.AddSingleton(applicationOptions.Toolbar);
        services.AddSingleton(applicationOptions.Regions);
        services.AddSingleton(applicationOptions.Tips);
        services.AddSingleton(applicationOptions.ProfileView);
        services.AddSingleton<ShellWindow>();
        services.AddSingleton<NavigationPanelService>();
        services.AddSingleton<NavigationMenuService>();
        services.AddSingletonAdapter<ToolbarService, IToolbarService>();
        services.AddSingleton<IToolbarStateService>(provider =>
            provider.GetRequiredService<ToolbarService>().State
        );
        services.AddSingletonAdapter<ShellRegionService, IShellRegionService>();
        services.AddSingleton<IMessageService, MessageService>();
        services.AddSingletonAdapter<TrayIconService, ITrayService>();
        services.AddSingletonAdapter<FontService, IFontService>();
        services.AddSingletonAdapter<ShortcutService, IShortcutService>();
        services.AddSingletonAdapter<MaterialEffectService, IMaterialEffectService>();
        services.AddSingletonAdapter<AppearanceService, IAppearanceService>();
        services.AddSingleton<PreferencePersistenceService>();
        services.AddSingleton<IHostedService>(provider =>
            provider.GetRequiredService<PreferencePersistenceService>()
        );
        services.AddSingleton<ProfileSecretStore>();
        services.AddSingleton<IProfileCredentialStore>(provider =>
            provider.GetRequiredService<ProfileSecretStore>()
        );
        services.AddSingletonAdapter<ThemeService, IThemeService>();
        services.AddSingletonAdapter<MotionService, IMotionService>();
        services.AddSingletonAdapter<ToolTipService, IToolTipService>();
        services.AddSingletonAdapter<ScrollService, IScrollService>();
        services.AddSingletonAdapter<TitleBarRuntimeFacade, ITitleBarService>();
        services.AddSingleton<IProjectSaveFileDialog, ProjectSaveFileDialog>();
        services.TryAddSingleton<IProjectBehavior, DefaultProjectBehavior>();
        services.AddSingletonAdapter<WindowService, IWindowService>();
        services.AddSingleton(provider =>
            new WindowCloseService(
                provider,
                provider.GetRequiredService<WindowOptions>().IsTrayExitEnabled
                    ? WindowCloseBehavior.MinimizeToTray
                    : WindowCloseBehavior.Prompt
            )
        );
        services.AddSingletonAlias<WindowCloseService, IWindowCloseService>();
        services.AddSingleton<WindowCloseOptionSynchronizer>();
        services.AddSingleton<IHostedService>(provider =>
            provider.GetRequiredService<WindowCloseOptionSynchronizer>()
        );
        services.AddSingletonAdapter<ProfileFlyoutService, IProfileFlyoutService>();
        services.AddSingleton<WindowFrameFixService>();
        services.AddSingleton<NavigationRouteRegistry>();
        services.AddSingleton<PageHistoryService>();
        services.AddSingleton<PageCacheService>();
        services.AddSingleton<NavigationService>();
        services.AddSingletonAdapter<NavigationRuntimeFacade, INavigationService>();
    }
}
