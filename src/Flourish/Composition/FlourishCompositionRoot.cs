using AcksheedSys.Flourish.Abstract;
using AcksheedSys.Flourish.Configuration;
using AcksheedSys.Flourish.Services;
using AcksheedSys.Flourish.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AcksheedSys.Flourish.Composition;

internal sealed class FlourishCompositionRoot(
    FlourishShellOptions shellOptions,
    IReadOnlyList<Action<HostBuilderContext, IServiceCollection>> serviceConfigurations,
    IReadOnlyList<Action<HostBuilderContext, IFlourishShellBuilder>> shellConfigurations,
    IReadOnlyList<Action<HostBuilderContext, IFlourishDynamicToolbarBuilder>> toolbarConfigurations,
    IReadOnlyList<Action<HostBuilderContext, IFlourishStatusBuilder>> statusConfigurations
)
{
    private readonly FlourishShellOptions shellOptions = shellOptions;
    private readonly IReadOnlyList<
        Action<HostBuilderContext, IServiceCollection>
    > serviceConfigurations = serviceConfigurations;
    private readonly IReadOnlyList<
        Action<HostBuilderContext, IFlourishShellBuilder>
    > shellConfigurations = shellConfigurations;
    private readonly IReadOnlyList<
        Action<HostBuilderContext, IFlourishDynamicToolbarBuilder>
    > toolbarConfigurations = toolbarConfigurations;
    private readonly IReadOnlyList<
        Action<HostBuilderContext, IFlourishStatusBuilder>
    > statusConfigurations = statusConfigurations;

    public void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        foreach (var configureServices in serviceConfigurations)
        {
            configureServices(context, services);
        }

        ApplyFlourishConfigurations(context);
        ApplyServiceCollectionRegistrations(services);
        RegisterCoreServices(services);
    }

    private void ApplyFlourishConfigurations(HostBuilderContext context)
    {
        var shellBuilder = new FlourishShellBuilder(shellOptions, context);
        foreach (var configureShell in shellConfigurations)
        {
            configureShell(context, shellBuilder);
        }

        var toolbarBuilder = new FlourishDynamicToolbarBuilder(shellOptions);
        foreach (var configureToolbar in toolbarConfigurations)
        {
            configureToolbar(context, toolbarBuilder);
        }

        var statusBuilder = new FlourishStatusBuilder(shellOptions);
        foreach (var configureStatus in statusConfigurations)
        {
            configureStatus(context, statusBuilder);
        }
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
            shellOptions.NavigationGroups.Count > 0 || shellOptions.FixedNavigationItems.Count > 0;

        shellOptions.NavigationItems.Clear();
        shellOptions.PageCacheModesByPageType.Clear();
        foreach (var page in registeredPages.Values)
        {
            shellOptions.PageCacheModesByPageType[page.PageType] = page.CacheMode;
        }

        if (!hasConfiguredNavigation)
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

            shellOptions.NavigationGroups.Add(legacyGroup);
        }

        foreach (var group in shellOptions.NavigationGroups.OrderBy(group => group.GroupId))
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

        FinalizeNavigationItems(
            shellOptions.FixedNavigationItems,
            registeredPages,
            "fixed navigation items"
        );
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
                    $"Navigation parentID {item.ParentId} is duplicated in {scopeName}."
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
                    $"Navigation childID {child.ChildId} in {scopeName} does not match a parentID."
                );
            }

            parent.HasChildren = true;
            child.IsVisible = false;
        }
    }

    private void RegisterCoreServices(IServiceCollection services)
    {
        services.AddSingleton(shellOptions);
        services.AddSingleton<FlourishShellWindow>();
        services.AddSingleton<FlourishToolbarService>();
        services.AddSingleton<FlourishStatusService>();
        services.AddSingleton<TrayIconService>();
        services.AddSingleton<FontService>();
        services.AddSingleton<CommandParser>();
        services.AddSingleton<MaterialEffectService>();
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
