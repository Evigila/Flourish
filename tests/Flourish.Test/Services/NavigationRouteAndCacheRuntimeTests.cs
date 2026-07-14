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
        Assert.Equal(
            FlourishPageCacheMode.Enabled,
            sut.Current.Routes["Reports"].CacheMode
        );
        Assert.Empty(options.InitialNavigationRoutes);

        registration.Dispose();

        Assert.False(sut.Contains("Reports"));
        Assert.Empty(sut.Current.Routes);
    }

    [Fact]
    public void RouteRegistry_StaleRegistrationCannotRemoveUpsertedRoute()
    {
        var sut = new NavigationRouteRegistry(new FlourishShellOptions());
        var oldRegistration = sut.Register(
            new FlourishNavigationRoute("Reports", typeof(ReportsPage))
        );
        var replacement = sut.Upsert(
            new FlourishNavigationRoute(
                "Reports",
                typeof(ReportsPage),
                FlourishPageCacheMode.Enabled
            )
        );

        oldRegistration.Dispose();

        Assert.True(sut.Contains("Reports"));
        replacement.Dispose();
        Assert.False(sut.Contains("Reports"));
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
        Assert.False(cache.Contains(typeof(ReportsPage)));
    }

    private static Page CreatePage()
    {
        return (Page)RuntimeHelpers.GetUninitializedObject(typeof(Page));
    }

    private sealed class ReportsPage : Page { }
}
