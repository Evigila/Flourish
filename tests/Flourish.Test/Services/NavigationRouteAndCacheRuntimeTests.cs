using System.Runtime.CompilerServices;
using System.Windows.Controls;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;
using ArkheideSystem.Flourish.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace ArkheideSystem.Flourish.Test.Services;

public sealed class NavigationRouteAndCacheRuntimeTests
{
    [Fact]
    public void RouteRegistry_RegisterAndDisposeOwnsRuntimeRouteState()
    {
        var options = new FlourishShellOptions();
        var sut = new NavigationRouteRegistry(options);
        var registration = sut.Register(
            new FlourishNavigationRoute(
                "Reports",
                typeof(ReportsPage),
                FlourishPageCacheMode.Enabled
            )
        );

        Assert.True(sut.Contains("Reports"));
        Assert.True(sut.TryGet(typeof(ReportsPage), out var indexedRoute));
        Assert.Equal("Reports", indexedRoute.NavigationKey);
        Assert.Equal(
            FlourishPageCacheMode.Enabled,
            sut.Current.Routes["Reports"].CacheMode
        );
        Assert.Empty(options.InitialNavigationRoutes);

        registration.Dispose();

        Assert.False(sut.Contains("Reports"));
        Assert.False(sut.TryGet(typeof(ReportsPage), out _));
        Assert.Empty(sut.Current.Routes);
    }

    [Fact]
    public void RouteRegistry_InitialPageTypeIndexTracksCacheModeUpdates()
    {
        var options = new FlourishShellOptions();
        options.InitialNavigationRoutes.Add(
            new FlourishNavigationRoute("Reports", typeof(ReportsPage))
        );
        var sut = new NavigationRouteRegistry(options);

        Assert.True(sut.TryGet(typeof(ReportsPage), out var initial));
        Assert.Equal(FlourishPageCacheMode.Disabled, initial.CacheMode);
        var initialVersion = sut.Current.Version;

        sut.SetCacheMode("Reports", FlourishPageCacheMode.Enabled);

        Assert.True(sut.TryGet(typeof(ReportsPage), out var updated));
        Assert.Equal(FlourishPageCacheMode.Enabled, updated.CacheMode);
        Assert.Equal(initialVersion + 1, sut.Current.Version);
    }

    [Fact]
    public void RouteRegistry_UpsertChangingPageTypeClearsOldMapping()
    {
        var sut = new NavigationRouteRegistry(new FlourishShellOptions());
        sut.Register(new FlourishNavigationRoute("Reports", typeof(ReportsPage)));

        sut.Upsert(new FlourishNavigationRoute("Reports", typeof(AnalyticsPage)));

        Assert.False(sut.TryGet(typeof(ReportsPage), out _));
        Assert.True(sut.TryGet(typeof(AnalyticsPage), out var replacement));
        Assert.Equal("Reports", replacement.NavigationKey);
    }

    [Fact]
    public void RouteRegistry_DuplicatePageTypeFailureDoesNotMutateIndexesOrVersion()
    {
        var sut = new NavigationRouteRegistry(new FlourishShellOptions());
        sut.Register(new FlourishNavigationRoute("Reports", typeof(ReportsPage)));
        sut.Register(new FlourishNavigationRoute("Analytics", typeof(AnalyticsPage)));
        var version = sut.Current.Version;

        Assert.Throws<InvalidOperationException>(() =>
            sut.Register(new FlourishNavigationRoute("Duplicate", typeof(ReportsPage)))
        );
        Assert.Throws<InvalidOperationException>(() =>
            sut.Upsert(new FlourishNavigationRoute("Analytics", typeof(ReportsPage)))
        );

        Assert.Equal(version, sut.Current.Version);
        Assert.False(sut.Contains("Duplicate"));
        Assert.True(sut.TryGet(typeof(ReportsPage), out var reports));
        Assert.Equal("Reports", reports.NavigationKey);
        Assert.True(sut.TryGet(typeof(AnalyticsPage), out var analytics));
        Assert.Equal("Analytics", analytics.NavigationKey);
    }

    [Fact]
    public void RouteRegistry_StaleRegistrationCannotRemoveUpsertedPageTypeMapping()
    {
        var sut = new NavigationRouteRegistry(new FlourishShellOptions());
        var oldRegistration = sut.Register(
            new FlourishNavigationRoute("Reports", typeof(ReportsPage))
        );
        var replacement = sut.Upsert(
            new FlourishNavigationRoute(
                "Reports",
                typeof(AnalyticsPage),
                FlourishPageCacheMode.Enabled
            )
        );

        oldRegistration.Dispose();

        Assert.True(sut.Contains("Reports"));
        Assert.False(sut.TryGet(typeof(ReportsPage), out _));
        Assert.True(sut.TryGet(typeof(AnalyticsPage), out var indexedReplacement));
        Assert.Equal(FlourishPageCacheMode.Enabled, indexedReplacement.CacheMode);
        replacement.Dispose();
        Assert.False(sut.Contains("Reports"));
        Assert.False(sut.TryGet(typeof(AnalyticsPage), out _));
    }

    [Fact]
    public void RouteRegistry_RemoveClearsPageTypeMapping()
    {
        var options = new FlourishShellOptions();
        options.InitialNavigationRoutes.Add(
            new FlourishNavigationRoute("Reports", typeof(ReportsPage))
        );
        var sut = new NavigationRouteRegistry(options);

        Assert.True(sut.Remove("Reports"));

        Assert.False(sut.Contains("Reports"));
        Assert.False(sut.TryGet(typeof(ReportsPage), out _));
        Assert.Empty(sut.Current.Routes);
    }

    [Fact]
    public void PageCache_RuntimeDisableEvictsExistingPage()
    {
        var page = CreatePage();
        var factory = new Mock<IPageFactory>(MockBehavior.Strict);
        factory.Setup(value => value.Create(typeof(ReportsPage))).Returns(page);
        var options = new FlourishShellOptions();
        options.InitialNavigationRoutes.Add(
            new FlourishNavigationRoute(
                "Reports",
                typeof(ReportsPage),
                FlourishPageCacheMode.Enabled
            )
        );
        var sut = new PageCacheService(
            factory.Object,
            new NavigationRouteRegistry(options)
        );

        Assert.Same(page, sut.GetPage(typeof(ReportsPage)));
        Assert.True(sut.Contains(typeof(ReportsPage)));

        sut.SetCacheMode(typeof(ReportsPage), FlourishPageCacheMode.Disabled);

        Assert.False(sut.Contains(typeof(ReportsPage)));
        Assert.Equal(
            FlourishPageCacheMode.Disabled,
            sut.Current.CacheModes[typeof(ReportsPage)]
        );
    }

    [Fact]
    public void RuntimeRouteFactory_CreatesAndCachesPageWithoutRootPageRegistration()
    {
        using var provider = new ServiceCollection().BuildServiceProvider();
        var options = new FlourishShellOptions();
        var routes = new NavigationRouteRegistry(provider, options);
        var page = CreatePage();
        routes.Register(
            new FlourishNavigationRoute(
                "Reports",
                typeof(ReportsPage),
                FlourishPageCacheMode.Enabled,
                _ => page
            )
        );
        var cache = new PageCacheService(provider, routes);

        Assert.Same(page, cache.GetPage(typeof(ReportsPage)));
        Assert.Same(page, cache.GetPage(typeof(ReportsPage)));
        Assert.True(cache.Contains(typeof(ReportsPage)));
    }

    [Fact]
    public async Task PageCache_OutOfOrderRouteEventsKeepNewestSnapshot()
    {
        var options = new FlourishShellOptions();
        options.InitialNavigationRoutes.Add(
            new FlourishNavigationRoute("Reports", typeof(ReportsPage))
        );
        var routes = new NavigationRouteRegistry(options);
        using var firstEventEntered = new ManualResetEventSlim();
        using var releaseFirstEvent = new ManualResetEventSlim();
        routes.Changed += (_, change) =>
        {
            if (change.Current.Version == 1)
            {
                firstEventEntered.Set();
                Assert.True(releaseFirstEvent.Wait(TimeSpan.FromSeconds(5)));
            }
        };
        var cache = new PageCacheService(
            new Mock<IPageFactory>(MockBehavior.Strict).Object,
            routes
        );

        var firstMutation = Task.Run(() =>
            routes.SetCacheMode("Reports", FlourishPageCacheMode.Enabled)
        );
        Assert.True(firstEventEntered.Wait(TimeSpan.FromSeconds(5)));
        try
        {
            routes.SetCacheMode("Reports", FlourishPageCacheMode.Disabled);
        }
        finally
        {
            releaseFirstEvent.Set();
        }

        await firstMutation.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal(
            FlourishPageCacheMode.Disabled,
            cache.Current.CacheModes[typeof(ReportsPage)]
        );
    }

    [Fact]
    public void PageCache_ReentrantFactoryDisableDoesNotCacheCreatedPage()
    {
        using var provider = new ServiceCollection().BuildServiceProvider();
        var page = CreatePage();
        PageCacheService? cache = null;
        var options = new FlourishShellOptions();
        options.InitialNavigationRoutes.Add(
            new FlourishNavigationRoute(
                "Reports",
                typeof(ReportsPage),
                FlourishPageCacheMode.Enabled,
                _ =>
                {
                    cache!.SetCacheMode(
                        typeof(ReportsPage),
                        FlourishPageCacheMode.Disabled
                    );
                    return page;
                }
            )
        );
        var routes = new NavigationRouteRegistry(provider, options);
        cache = new PageCacheService(provider, routes);

        Assert.Same(page, cache.GetPage(typeof(ReportsPage)));
        Assert.Equal(
            FlourishPageCacheMode.Disabled,
            cache.Current.CacheModes[typeof(ReportsPage)]
        );
        Assert.True(routes.TryGet(typeof(ReportsPage), out var updatedRoute));
        Assert.Equal(FlourishPageCacheMode.Disabled, updatedRoute.CacheMode);
        Assert.False(cache.Contains(typeof(ReportsPage)));
    }

    private static Page CreatePage()
    {
        return (Page)RuntimeHelpers.GetUninitializedObject(typeof(Page));
    }

    private sealed class ReportsPage : Page { }

    private sealed class AnalyticsPage : Page { }
}
