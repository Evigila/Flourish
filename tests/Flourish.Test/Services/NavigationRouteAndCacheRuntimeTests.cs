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
    public void RouteRegistry_RegisterAndDisposeSynchronizesLegacyOptionMaps()
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
        Assert.Equal(typeof(ReportsPage), options.PageTypesByNavigationKey["Reports"]);
        Assert.Equal(
            FlourishPageCacheMode.Enabled,
            options.PageCacheModesByPageType[typeof(ReportsPage)]
        );

        registration.Dispose();

        Assert.False(sut.Contains("Reports"));
        Assert.Empty(options.PageTypesByNavigationKey);
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
        options.PageCacheModesByPageType[typeof(ReportsPage)] = FlourishPageCacheMode.Enabled;
        var sut = new PageCacheService(factory.Object, options);

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
        var cache = new PageCacheService(provider, options, routes);

        Assert.Same(page, cache.GetPage(typeof(ReportsPage)));
        Assert.Same(page, cache.GetPage(typeof(ReportsPage)));
        Assert.True(cache.Contains(typeof(ReportsPage)));
    }

    private static Page CreatePage()
    {
        return (Page)RuntimeHelpers.GetUninitializedObject(typeof(Page));
    }

    private sealed class ReportsPage : Page { }
}
