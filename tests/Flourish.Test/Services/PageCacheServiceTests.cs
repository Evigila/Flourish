using System.Runtime.CompilerServices;
using System.Windows.Controls;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;
using ArkheideSystem.Flourish.Services;
using Moq;

namespace ArkheideSystem.Flourish.Test.Services;

public sealed class PageCacheServiceTests
{
    [Fact]
    public void GetPage_WhenCacheEnabled_CreatesOnceAndReturnsSamePage()
    {
        var page = CreatePage();
        var factory = new Mock<IPageFactory>(MockBehavior.Strict);
        factory.Setup(value => value.Create(typeof(CacheablePage))).Returns(page);
        var routes = CreateRoutes(typeof(CacheablePage), FlourishPageCacheMode.Enabled);
        var sut = new PageCacheService(factory.Object, routes);

        var first = sut.GetPage(typeof(CacheablePage));
        var second = sut.GetPage(typeof(CacheablePage));

        Assert.Same(page, first);
        Assert.Same(first, second);
        factory.Verify(value => value.Create(typeof(CacheablePage)), Times.Once);
    }

    [Fact]
    public void GetPage_WhenCacheDisabled_CreatesEveryTime()
    {
        var firstPage = CreatePage();
        var secondPage = CreatePage();
        var factory = new Mock<IPageFactory>(MockBehavior.Strict);
        factory
            .SetupSequence(value => value.Create(typeof(TransientPage)))
            .Returns(firstPage)
            .Returns(secondPage);
        var routes = CreateRoutes(typeof(TransientPage), FlourishPageCacheMode.Disabled);
        var sut = new PageCacheService(factory.Object, routes);

        var first = sut.GetPage(typeof(TransientPage));
        var second = sut.GetPage(typeof(TransientPage));

        Assert.Same(firstPage, first);
        Assert.Same(secondPage, second);
        Assert.NotSame(first, second);
        factory.Verify(value => value.Create(typeof(TransientPage)), Times.Exactly(2));
    }

    [Fact]
    public void GetPage_WhenCacheModeUnconfigured_CreatesEveryTime()
    {
        var factory = new Mock<IPageFactory>(MockBehavior.Strict);
        factory
            .SetupSequence(value => value.Create(typeof(TransientPage)))
            .Returns(CreatePage())
            .Returns(CreatePage());
        var sut = new PageCacheService(
            factory.Object,
            new NavigationRouteRegistry(new FlourishShellOptions())
        );

        var first = sut.GetPage(typeof(TransientPage));
        var second = sut.GetPage(typeof(TransientPage));

        Assert.NotSame(first, second);
        factory.Verify(value => value.Create(typeof(TransientPage)), Times.Exactly(2));
    }

    [Fact]
    public void GetPage_UsesCacheModeSnapshotFromConstruction()
    {
        var page = CreatePage();
        var factory = new Mock<IPageFactory>(MockBehavior.Strict);
        factory.Setup(value => value.Create(typeof(CacheablePage))).Returns(page);
        var options = CreateOptions(typeof(CacheablePage), FlourishPageCacheMode.Enabled);
        var sut = new PageCacheService(
            factory.Object,
            new NavigationRouteRegistry(options)
        );
        options.InitialNavigationRoutes.Clear();

        var first = sut.GetPage(typeof(CacheablePage));
        var second = sut.GetPage(typeof(CacheablePage));

        Assert.Same(first, second);
        factory.Verify(value => value.Create(typeof(CacheablePage)), Times.Once);
    }

    [Fact]
    public void GetPage_WhenFactoryThrows_DoesNotCacheFailure()
    {
        var page = CreatePage();
        var factory = new Mock<IPageFactory>(MockBehavior.Strict);
        factory
            .SetupSequence(value => value.Create(typeof(CacheablePage)))
            .Throws(new PageCreationException())
            .Returns(page);
        var routes = CreateRoutes(typeof(CacheablePage), FlourishPageCacheMode.Enabled);
        var sut = new PageCacheService(factory.Object, routes);

        Assert.Throws<PageCreationException>(() => sut.GetPage(typeof(CacheablePage)));
        Assert.Same(page, sut.GetPage(typeof(CacheablePage)));
        factory.Verify(value => value.Create(typeof(CacheablePage)), Times.Exactly(2));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void GetPage_WhenFactoryReturnsNonPage_Throws(bool returnsNull)
    {
        var factory = new Mock<IPageFactory>(MockBehavior.Strict);
        factory
            .Setup(value => value.Create(typeof(NotAPage)))
            .Returns(returnsNull ? null : new NotAPage());
        var sut = new PageCacheService(
            factory.Object,
            new NavigationRouteRegistry(new FlourishShellOptions())
        );

        var exception = Assert.Throws<InvalidOperationException>(() =>
            sut.GetPage(typeof(NotAPage))
        );

        Assert.Contains(typeof(NotAPage).FullName!, exception.Message);
        Assert.Contains("must derive", exception.Message);
    }

    [Fact]
    public void GetPage_WithNullType_ThrowsBeforeCallingFactory()
    {
        var factory = new Mock<IPageFactory>(MockBehavior.Strict);
        var sut = new PageCacheService(
            factory.Object,
            new NavigationRouteRegistry(new FlourishShellOptions())
        );

        Assert.Throws<ArgumentNullException>(() => sut.GetPage(null!));
        factory.VerifyNoOtherCalls();
    }

    private static FlourishShellOptions CreateOptions(
        Type pageType,
        FlourishPageCacheMode cacheMode
    )
    {
        var options = new FlourishShellOptions();
        options.InitialNavigationRoutes.Add(
            new FlourishNavigationRoute(pageType.Name, pageType, cacheMode)
        );
        return options;
    }

    private static NavigationRouteRegistry CreateRoutes(
        Type pageType,
        FlourishPageCacheMode cacheMode
    )
    {
        return new NavigationRouteRegistry(CreateOptions(pageType, cacheMode));
    }

    private static Page CreatePage()
    {
        return (Page)RuntimeHelpers.GetUninitializedObject(typeof(Page));
    }

    private sealed class CacheablePage : Page { }

    private sealed class TransientPage : Page { }

    private sealed class NotAPage { }

    private sealed class PageCreationException : Exception { }
}
