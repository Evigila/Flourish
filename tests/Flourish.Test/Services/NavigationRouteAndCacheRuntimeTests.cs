using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Navigation;

using System.Runtime.CompilerServices;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace ArkheideSystem.Flourish.Test.Services;

public sealed class NavigationRouteAndCacheRuntimeTests
{
    [Fact]
    public void RouteRegistry_RegisterAndDisposeOwnsRuntimeRouteState()
    {
        var options = new NavigationOptions();
        var sut = new NavigationRouteRegistry(options);
        var registration = sut.Append(
            new NavigationRoute(
                "Reports",
                typeof(ReportsPage),
                PageCacheMode.Enabled
            )
        );

        Assert.NotNull(sut.Get("Reports"));
        Assert.True(sut.TryGet(typeof(ReportsPage), out var indexedRoute));
        Assert.Equal("Reports", indexedRoute.NavigationKey);
        Assert.Equal(PageCacheMode.Enabled, sut.Current.Routes["Reports"].CacheMode);
        Assert.Empty(options.InitialNavigationRoutes);

        registration.Dispose();

        Assert.Null(sut.Get("Reports"));
        Assert.False(sut.TryGet(typeof(ReportsPage), out _));
        Assert.Empty(sut.Current.Routes);
    }

    [Fact]
    public void RouteRegistry_InitialPageTypeIndexTracksCacheModeUpdates()
    {
        var options = new NavigationOptions();
        options.InitialNavigationRoutes.Add(
            new NavigationRoute("Reports", typeof(ReportsPage))
        );
        var sut = new NavigationRouteRegistry(options);

        Assert.True(sut.TryGet(typeof(ReportsPage), out var initial));
        Assert.Equal(PageCacheMode.Disabled, initial.CacheMode);
        var initialVersion = sut.Current.Version;

        sut.SetCacheMode("Reports", PageCacheMode.Enabled);

        Assert.True(sut.TryGet(typeof(ReportsPage), out var updated));
        Assert.Equal(PageCacheMode.Enabled, updated.CacheMode);
        Assert.Equal(initialVersion + 1, sut.Current.Version);
    }

    [Fact]
    public void RouteRegistry_UpsertChangingPageTypeClearsOldMapping()
    {
        var sut = new NavigationRouteRegistry(new NavigationOptions());
        sut.Append(new NavigationRoute("Reports", typeof(ReportsPage)));

        sut.Set(new NavigationRoute("Reports", typeof(AnalyticsPage)));

        Assert.False(sut.TryGet(typeof(ReportsPage), out _));
        Assert.True(sut.TryGet(typeof(AnalyticsPage), out var replacement));
        Assert.Equal("Reports", replacement.NavigationKey);
    }

    [Fact]
    public void RouteRegistry_DuplicatePageTypeFailureDoesNotMutateIndexesOrVersion()
    {
        var sut = new NavigationRouteRegistry(new NavigationOptions());
        sut.Append(new NavigationRoute("Reports", typeof(ReportsPage)));
        sut.Append(new NavigationRoute("Analytics", typeof(AnalyticsPage)));
        var version = sut.Current.Version;

        Assert.Throws<InvalidOperationException>(() =>
            sut.Append(new NavigationRoute("Duplicate", typeof(ReportsPage)))
        );
        Assert.Throws<InvalidOperationException>(() =>
            sut.Set(new NavigationRoute("Analytics", typeof(ReportsPage)))
        );

        Assert.Equal(version, sut.Current.Version);
        Assert.Null(sut.Get("Duplicate"));
        Assert.True(sut.TryGet(typeof(ReportsPage), out var reports));
        Assert.Equal("Reports", reports.NavigationKey);
        Assert.True(sut.TryGet(typeof(AnalyticsPage), out var analytics));
        Assert.Equal("Analytics", analytics.NavigationKey);
    }

    [Fact]
    public void RouteRegistry_StaleRegistrationCannotRemoveUpsertedPageTypeMapping()
    {
        var sut = new NavigationRouteRegistry(new NavigationOptions());
        var oldRegistration = sut.Append(
            new NavigationRoute("Reports", typeof(ReportsPage))
        );
        var replacement = sut.Set(
            new NavigationRoute(
                "Reports",
                typeof(AnalyticsPage),
                PageCacheMode.Enabled
            )
        );

        oldRegistration.Dispose();

        Assert.NotNull(sut.Get("Reports"));
        Assert.False(sut.TryGet(typeof(ReportsPage), out _));
        Assert.True(sut.TryGet(typeof(AnalyticsPage), out var indexedReplacement));
        Assert.Equal(PageCacheMode.Enabled, indexedReplacement.CacheMode);
        replacement.Dispose();
        Assert.Null(sut.Get("Reports"));
        Assert.False(sut.TryGet(typeof(AnalyticsPage), out _));
    }

    [Fact]
    public void RouteRegistry_RemoveClearsPageTypeMapping()
    {
        var options = new NavigationOptions();
        options.InitialNavigationRoutes.Add(
            new NavigationRoute("Reports", typeof(ReportsPage))
        );
        var sut = new NavigationRouteRegistry(options);

        Assert.True(sut.Remove("Reports"));

        Assert.Null(sut.Get("Reports"));
        Assert.False(sut.TryGet(typeof(ReportsPage), out _));
        Assert.Empty(sut.Current.Routes);
    }

    [Fact]
    public void PageCache_RuntimeDisableEvictsExistingPage()
    {
        var page = CreatePage();
        var factory = new Mock<IPageFactory>(MockBehavior.Strict);
        factory.Setup(value => value.Create(typeof(ReportsPage))).Returns(page);
        var options = new NavigationOptions();
        options.InitialNavigationRoutes.Add(
            new NavigationRoute(
                "Reports",
                typeof(ReportsPage),
                PageCacheMode.Enabled
            )
        );
        var sut = new PageCacheService(factory.Object, new NavigationRouteRegistry(options));

        Assert.Same(page, sut.GetPage(typeof(ReportsPage)));
        Assert.True(sut.Contains(typeof(ReportsPage)));

        sut.SetCacheMode(typeof(ReportsPage), PageCacheMode.Disabled);

        Assert.False(sut.Contains(typeof(ReportsPage)));
        Assert.Equal(PageCacheMode.Disabled, sut.Current.CacheModes[typeof(ReportsPage)]);
    }

    [Fact]
    public void RuntimeRouteFactory_CreatesAndCachesPageWithoutRootPageRegistration()
    {
        using var provider = new ServiceCollection().BuildServiceProvider();
        var options = new NavigationOptions();
        var routes = new NavigationRouteRegistry(provider, options);
        var page = CreatePage();
        routes.Append(
            new NavigationRoute(
                "Reports",
                typeof(ReportsPage),
                PageCacheMode.Enabled,
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
        var options = new NavigationOptions();
        options.InitialNavigationRoutes.Add(
            new NavigationRoute("Reports", typeof(ReportsPage))
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
            routes.SetCacheMode("Reports", PageCacheMode.Enabled)
        );
        Assert.True(firstEventEntered.Wait(TimeSpan.FromSeconds(5)));
        try
        {
            routes.SetCacheMode("Reports", PageCacheMode.Disabled);
        }
        finally
        {
            releaseFirstEvent.Set();
        }

        await firstMutation.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal(PageCacheMode.Disabled, cache.Current.CacheModes[typeof(ReportsPage)]);
    }

    [Fact]
    public void PageCache_ReentrantFactoryDisableDoesNotCacheCreatedPage()
    {
        using var provider = new ServiceCollection().BuildServiceProvider();
        var page = CreatePage();
        PageCacheService? cache = null;
        var options = new NavigationOptions();
        options.InitialNavigationRoutes.Add(
            new NavigationRoute(
                "Reports",
                typeof(ReportsPage),
                PageCacheMode.Enabled,
                _ =>
                {
                    cache!.SetCacheMode(typeof(ReportsPage), PageCacheMode.Disabled);
                    return page;
                }
            )
        );
        var routes = new NavigationRouteRegistry(provider, options);
        cache = new PageCacheService(provider, routes);

        Assert.Same(page, cache.GetPage(typeof(ReportsPage)));
        Assert.Equal(PageCacheMode.Disabled, cache.Current.CacheModes[typeof(ReportsPage)]);
        Assert.True(routes.TryGet(typeof(ReportsPage), out var updatedRoute));
        Assert.Equal(PageCacheMode.Disabled, updatedRoute.CacheMode);
        Assert.False(cache.Contains(typeof(ReportsPage)));
    }

    private static Page CreatePage()
    {
        return (Page)RuntimeHelpers.GetUninitializedObject(typeof(Page));
    }

    private sealed class ReportsPage : Page { }

    private sealed class AnalyticsPage : Page { }
}
