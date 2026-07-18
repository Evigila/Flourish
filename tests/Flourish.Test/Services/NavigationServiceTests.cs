using System.Runtime.CompilerServices;
using System.Windows.Controls;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;
using ArkheideSystem.Flourish.Services;
using Moq;

namespace ArkheideSystem.Flourish.Test.Services;

public sealed class NavigationServiceTests
{
    [Fact]
    public void Navigate_BeforeInitialization_ThrowsWithoutCreatingPage()
    {
        var fixture = CreateFixture(initialize: false);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            fixture.Sut.Navigate(HomeKey)
        );

        Assert.Contains("initialized with a frame", exception.Message);
        fixture.PageProvider.VerifyNoOtherCalls();
        fixture.ContentHost.VerifyNoOtherCalls();
    }

    [Fact]
    public void Navigate_WithUnknownKey_ThrowsWithoutCreatingPage()
    {
        var fixture = CreateFixture();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            fixture.Sut.Navigate("Setings")
        );

        Assert.Contains("Setings", exception.Message);
        Assert.Contains("spelling", exception.Message);
        Assert.Contains("casing", exception.Message);
        Assert.Contains("SettingsPage", exception.Message);
        fixture.PageProvider.VerifyNoOtherCalls();
        fixture.ContentHost.VerifyNoOtherCalls();
    }

    [Fact]
    public void Navigate_WhenHostAccepts_CommitsStateAndRaisesEvent()
    {
        var fixture = CreateFixture();
        var page = CreatePage();
        var parameter = new object();
        FlourishNavigatedEventArgs? navigated = null;
        fixture.PageProvider.Setup(provider => provider.GetPage(typeof(HomePage))).Returns(page);
        fixture.ContentHost.Setup(host => host.Navigate(page)).Returns(true);
        fixture.Sut.Navigated += (_, args) => navigated = args;

        var result = fixture.Sut.Navigate(HomeKey, parameter);

        Assert.True(result);
        Assert.Equal(HomeKey, fixture.Sut.CurrentNavigationKey);
        Assert.Equal(typeof(HomePage), fixture.Sut.CurrentSourcePageType);
        Assert.False(fixture.Sut.CanGoBack);
        Assert.NotNull(navigated);
        Assert.Equal(HomeKey, navigated.NavigationKey);
        Assert.Equal(typeof(HomePage), navigated.SourcePageType);
        Assert.Same(page, navigated.Page);
        Assert.Same(parameter, navigated.Parameter);
    }

    [Fact]
    public void Navigate_WhenHostRejects_DoesNotCommitStateHistoryOrEvent()
    {
        var fixture = CreateFixture();
        var homePage = CreatePage();
        var settingsPage = CreatePage();
        var eventCount = 0;
        fixture.PageProvider
            .Setup(provider => provider.GetPage(typeof(HomePage)))
            .Returns(homePage);
        fixture.PageProvider
            .Setup(provider => provider.GetPage(typeof(SettingsPage)))
            .Returns(settingsPage);
        fixture.ContentHost.Setup(host => host.Navigate(homePage)).Returns(true);
        fixture.ContentHost.Setup(host => host.Navigate(settingsPage)).Returns(false);
        fixture.Sut.Navigated += (_, _) => eventCount++;
        Assert.True(fixture.Sut.Navigate(HomeKey, "original"));

        var result = fixture.Sut.Navigate(SettingsKey, "rejected");

        Assert.False(result);
        Assert.Equal(HomeKey, fixture.Sut.CurrentNavigationKey);
        Assert.Equal(typeof(HomePage), fixture.Sut.CurrentSourcePageType);
        Assert.False(fixture.Sut.CanGoBack);
        Assert.Equal(1, eventCount);
    }

    [Fact]
    public void Navigate_ToSameKeyAndEqualParameter_IsIgnored()
    {
        var fixture = CreateFixture();
        var page = CreatePage();
        fixture.PageProvider.Setup(provider => provider.GetPage(typeof(HomePage))).Returns(page);
        fixture.ContentHost.Setup(host => host.Navigate(page)).Returns(true);
        Assert.True(fixture.Sut.Navigate(HomeKey, "same"));

        var result = fixture.Sut.Navigate(HomeKey, string.Concat("sa", "me"));

        Assert.False(result);
        Assert.False(fixture.Sut.CanGoBack);
        fixture.PageProvider.Verify(provider => provider.GetPage(typeof(HomePage)), Times.Once);
        fixture.ContentHost.Verify(host => host.Navigate(page), Times.Once);
    }

    [Fact]
    public void Navigate_ToSameKeyWithDifferentParameter_AddsPreviousParameterToHistory()
    {
        var fixture = CreateFixture();
        var page = CreatePage();
        FlourishNavigatedEventArgs? lastEvent = null;
        fixture.PageProvider.Setup(provider => provider.GetPage(typeof(HomePage))).Returns(page);
        fixture.ContentHost.Setup(host => host.Navigate(page)).Returns(true);
        fixture.Sut.Navigated += (_, args) => lastEvent = args;
        Assert.True(fixture.Sut.Navigate(HomeKey, "first"));

        var result = fixture.Sut.Navigate(HomeKey, "second");

        Assert.True(result);
        Assert.True(fixture.Sut.CanGoBack);
        Assert.True(fixture.History.TryPopBack(out var previous));
        Assert.Equal(new FlourishPageStackEntry(HomeKey, "first"), previous);
        Assert.Equal("second", lastEvent?.Parameter);
    }

    [Fact]
    public void Navigate_WithAddToBackStackFalse_DoesNotAddCurrentPage()
    {
        var fixture = CreateFixture();
        SetupSuccessfulPages(fixture, out _, out var settingsPage, out _);
        Assert.True(fixture.Sut.Navigate(HomeKey));

        var result = fixture.Sut.Navigate(SettingsKey, addToBackStack: false);

        Assert.True(result);
        Assert.Equal(SettingsKey, fixture.Sut.CurrentNavigationKey);
        Assert.False(fixture.Sut.CanGoBack);
        fixture.ContentHost.Verify(host => host.Navigate(settingsPage), Times.Once);
    }

    [Fact]
    public void GoBackAndForward_RestoreKeysParametersAndHistory()
    {
        var fixture = CreateFixture();
        SetupSuccessfulPages(fixture, out _, out _, out _);
        var events = new List<FlourishNavigatedEventArgs>();
        fixture.Sut.Navigated += (_, args) => events.Add(args);
        Assert.True(fixture.Sut.Navigate(HomeKey, "home-parameter"));
        Assert.True(fixture.Sut.Navigate(SettingsKey, "settings-parameter"));

        Assert.True(fixture.Sut.GoBack());
        Assert.Equal(HomeKey, fixture.Sut.CurrentNavigationKey);
        Assert.Equal("home-parameter", events[^1].Parameter);
        Assert.False(fixture.Sut.CanGoBack);
        Assert.True(fixture.Sut.CanGoForward);

        Assert.True(fixture.Sut.GoForward());
        Assert.Equal(SettingsKey, fixture.Sut.CurrentNavigationKey);
        Assert.Equal("settings-parameter", events[^1].Parameter);
        Assert.True(fixture.Sut.CanGoBack);
        Assert.False(fixture.Sut.CanGoForward);
    }

    [Fact]
    public void GoBack_WhenHostRejects_RestoresBackEntryAndLeavesStateUnchanged()
    {
        var fixture = CreateFixture();
        var homePage = CreatePage();
        var settingsPage = CreatePage();
        var eventCount = 0;
        fixture.PageProvider
            .Setup(provider => provider.GetPage(typeof(HomePage)))
            .Returns(homePage);
        fixture.PageProvider
            .Setup(provider => provider.GetPage(typeof(SettingsPage)))
            .Returns(settingsPage);
        fixture.ContentHost
            .SetupSequence(host => host.Navigate(homePage))
            .Returns(true)
            .Returns(false);
        fixture.ContentHost.Setup(host => host.Navigate(settingsPage)).Returns(true);
        fixture.Sut.Navigated += (_, _) => eventCount++;
        Assert.True(fixture.Sut.Navigate(HomeKey, "home"));
        Assert.True(fixture.Sut.Navigate(SettingsKey, "settings"));

        var result = fixture.Sut.GoBack();

        Assert.False(result);
        Assert.Equal(SettingsKey, fixture.Sut.CurrentNavigationKey);
        Assert.True(fixture.Sut.CanGoBack);
        Assert.False(fixture.Sut.CanGoForward);
        Assert.Equal(2, eventCount);
        Assert.True(fixture.History.TryPopBack(out var restored));
        Assert.Equal(new FlourishPageStackEntry(HomeKey, "home"), restored);
    }

    [Fact]
    public void GoBack_WhenPageProviderThrows_RestoresBackEntryAndRethrows()
    {
        var fixture = CreateFixture();
        var homePage = CreatePage();
        var settingsPage = CreatePage();
        fixture.PageProvider
            .SetupSequence(provider => provider.GetPage(typeof(HomePage)))
            .Returns(homePage)
            .Throws(new PageCreationException());
        fixture.PageProvider
            .Setup(provider => provider.GetPage(typeof(SettingsPage)))
            .Returns(settingsPage);
        fixture.ContentHost.Setup(host => host.Navigate(homePage)).Returns(true);
        fixture.ContentHost.Setup(host => host.Navigate(settingsPage)).Returns(true);
        Assert.True(fixture.Sut.Navigate(HomeKey, "home"));
        Assert.True(fixture.Sut.Navigate(SettingsKey, "settings"));

        Assert.Throws<PageCreationException>(() => fixture.Sut.GoBack());

        Assert.Equal(SettingsKey, fixture.Sut.CurrentNavigationKey);
        Assert.True(fixture.Sut.CanGoBack);
        Assert.False(fixture.Sut.CanGoForward);
    }

    [Fact]
    public void GoForward_WhenHostRejects_RestoresForwardEntryAndLeavesStateUnchanged()
    {
        var fixture = CreateFixture();
        var homePage = CreatePage();
        var settingsPage = CreatePage();
        fixture.PageProvider
            .Setup(provider => provider.GetPage(typeof(HomePage)))
            .Returns(homePage);
        fixture.PageProvider
            .Setup(provider => provider.GetPage(typeof(SettingsPage)))
            .Returns(settingsPage);
        fixture.ContentHost.Setup(host => host.Navigate(homePage)).Returns(true);
        fixture.ContentHost
            .SetupSequence(host => host.Navigate(settingsPage))
            .Returns(true)
            .Returns(false);
        Assert.True(fixture.Sut.Navigate(HomeKey));
        Assert.True(fixture.Sut.Navigate(SettingsKey));
        Assert.True(fixture.Sut.GoBack());

        var result = fixture.Sut.GoForward();

        Assert.False(result);
        Assert.Equal(HomeKey, fixture.Sut.CurrentNavigationKey);
        Assert.False(fixture.Sut.CanGoBack);
        Assert.True(fixture.Sut.CanGoForward);
        Assert.True(fixture.History.TryPopForward(out var restored));
        Assert.Equal(new FlourishPageStackEntry(SettingsKey, null), restored);
    }

    [Fact]
    public void RejectedNewNavigation_PreservesExistingForwardHistory()
    {
        var fixture = CreateFixture();
        SetupSuccessfulPages(fixture, out _, out _, out var galleryPage);
        fixture.ContentHost.Setup(host => host.Navigate(galleryPage)).Returns(false);
        Assert.True(fixture.Sut.Navigate(HomeKey));
        Assert.True(fixture.Sut.Navigate(SettingsKey));
        Assert.True(fixture.Sut.GoBack());
        Assert.True(fixture.Sut.CanGoForward);

        var result = fixture.Sut.Navigate(GalleryKey);

        Assert.False(result);
        Assert.Equal(HomeKey, fixture.Sut.CurrentNavigationKey);
        Assert.False(fixture.Sut.CanGoBack);
        Assert.True(fixture.Sut.CanGoForward);
    }

    [Fact]
    public void ClearBackStack_LeavesForwardHistoryUntouched()
    {
        var fixture = CreateFixture();
        SetupSuccessfulPages(fixture, out _, out _, out _);
        Assert.True(fixture.Sut.Navigate(HomeKey));
        Assert.True(fixture.Sut.Navigate(SettingsKey));
        Assert.True(fixture.Sut.Navigate(GalleryKey));
        Assert.True(fixture.Sut.GoBack());
        Assert.True(fixture.Sut.CanGoBack);
        Assert.True(fixture.Sut.CanGoForward);

        fixture.Sut.ClearBackStack();

        Assert.False(fixture.Sut.CanGoBack);
        Assert.True(fixture.Sut.CanGoForward);
        Assert.True(fixture.Sut.GoForward());
        Assert.Equal(GalleryKey, fixture.Sut.CurrentNavigationKey);
    }

    [Fact]
    public void GoBackAndGoForward_WithEmptyHistory_ReturnFalseWithoutDependencies()
    {
        var fixture = CreateFixture();

        Assert.False(fixture.Sut.GoBack());
        Assert.False(fixture.Sut.GoForward());
        fixture.PageProvider.VerifyNoOtherCalls();
        fixture.ContentHost.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ConcurrentNavigations_AreSerializedAcrossTheHostAndHistoryCommit()
    {
        var options = new FlourishShellOptions();
        Register(options, HomeKey, typeof(HomePage));
        Register(options, SettingsKey, typeof(SettingsPage));
        var pageProvider = new Mock<INavigationPageProvider>(MockBehavior.Strict);
        pageProvider
            .Setup(provider => provider.GetPage(typeof(HomePage)))
            .Returns(CreatePage());
        pageProvider
            .Setup(provider => provider.GetPage(typeof(SettingsPage)))
            .Returns(CreatePage());
        var host = new BlockingNavigationContentHost();
        var sut = new NavigationService(
            pageProvider.Object,
            new PageHistoryService(),
            new NavigationRouteRegistry(options)
        );
        sut.Initialize(host);
        using var secondStarted = new ManualResetEventSlim();

        var firstNavigation = Task.Run(() => sut.Navigate(HomeKey));
        Task<bool>? secondNavigation = null;
        try
        {
            Assert.True(host.FirstCallEntered.Wait(TimeSpan.FromSeconds(5)));
            secondNavigation = Task.Run(() =>
            {
                secondStarted.Set();
                return sut.Navigate(SettingsKey);
            });
            Assert.True(secondStarted.Wait(TimeSpan.FromSeconds(5)));

            Assert.False(host.SecondCallEntered.Wait(TimeSpan.FromMilliseconds(200)));
        }
        finally
        {
            host.ReleaseFirstCall.Set();
        }

        Assert.True(await firstNavigation.WaitAsync(TimeSpan.FromSeconds(5)));
        Assert.NotNull(secondNavigation);
        Assert.True(await secondNavigation.WaitAsync(TimeSpan.FromSeconds(5)));
        Assert.Equal(1, host.MaximumConcurrentCalls);
        Assert.Equal(SettingsKey, sut.CurrentNavigationKey);
        Assert.True(sut.CanGoBack);
    }

    [Fact]
    public async Task RouteEventsArrivingOutOfOrderDoNotRemoveHistoryForReRegisteredRoute()
    {
        var options = new FlourishShellOptions();
        Register(options, HomeKey, typeof(HomePage));
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
        var history = new PageHistoryService();
        history.Push(new FlourishPageStackEntry(HomeKey, Parameter: null));
        _ = new NavigationService(
            new Mock<INavigationPageProvider>(MockBehavior.Strict).Object,
            history,
            routes
        );

        var remove = Task.Run(() => routes.Remove(HomeKey));
        Assert.True(firstEventEntered.Wait(TimeSpan.FromSeconds(5)));
        INavigationRouteRegistration replacement;
        try
        {
            replacement = routes.Register(
                new FlourishNavigationRoute(HomeKey, typeof(HomePage))
            );
        }
        finally
        {
            releaseFirstEvent.Set();
        }

        Assert.True(await remove.WaitAsync(TimeSpan.FromSeconds(5)));
        Assert.Contains(history.BackStack, entry => entry.NavigationKey == HomeKey);
        replacement.Dispose();
    }

    private static NavigationFixture CreateFixture(bool initialize = true)
    {
        var options = new FlourishShellOptions();
        Register(options, HomeKey, typeof(HomePage));
        Register(options, SettingsKey, typeof(SettingsPage));
        Register(options, GalleryKey, typeof(GalleryPage));

        var pageProvider = new Mock<INavigationPageProvider>(MockBehavior.Strict);
        var contentHost = new Mock<INavigationContentHost>(MockBehavior.Strict);
        var history = new PageHistoryService();
        var sut = new NavigationService(
            pageProvider.Object,
            history,
            new NavigationRouteRegistry(options)
        );
        if (initialize)
        {
            sut.Initialize(contentHost.Object);
        }

        return new NavigationFixture(sut, pageProvider, contentHost, history);
    }

    private static void Register(FlourishShellOptions options, string key, Type pageType)
    {
        options.InitialNavigationRoutes.Add(
            new FlourishNavigationRoute(key, pageType)
        );
    }

    private static void SetupSuccessfulPages(
        NavigationFixture fixture,
        out Page homePage,
        out Page settingsPage,
        out Page galleryPage
    )
    {
        var home = CreatePage();
        var settings = CreatePage();
        var gallery = CreatePage();
        homePage = home;
        settingsPage = settings;
        galleryPage = gallery;
        fixture.PageProvider
            .Setup(provider => provider.GetPage(typeof(HomePage)))
            .Returns(home);
        fixture.PageProvider
            .Setup(provider => provider.GetPage(typeof(SettingsPage)))
            .Returns(settings);
        fixture.PageProvider
            .Setup(provider => provider.GetPage(typeof(GalleryPage)))
            .Returns(gallery);
        fixture.ContentHost.Setup(host => host.Navigate(home)).Returns(true);
        fixture.ContentHost.Setup(host => host.Navigate(settings)).Returns(true);
        fixture.ContentHost.Setup(host => host.Navigate(gallery)).Returns(true);
    }

    private static Page CreatePage()
    {
        return (Page)RuntimeHelpers.GetUninitializedObject(typeof(Page));
    }

    private sealed record NavigationFixture(
        NavigationService Sut,
        Mock<INavigationPageProvider> PageProvider,
        Mock<INavigationContentHost> ContentHost,
        PageHistoryService History
    );

    private sealed class HomePage : Page { }

    private sealed class SettingsPage : Page { }

    private sealed class GalleryPage : Page { }

    private sealed class PageCreationException : Exception { }

    private sealed class BlockingNavigationContentHost : INavigationContentHost
    {
        private int activeCalls;
        private int callCount;
        private int maximumConcurrentCalls;

        public ManualResetEventSlim FirstCallEntered { get; } = new();

        public ManualResetEventSlim SecondCallEntered { get; } = new();

        public ManualResetEventSlim ReleaseFirstCall { get; } = new();

        public int MaximumConcurrentCalls => Volatile.Read(ref maximumConcurrentCalls);

        public bool Navigate(Page page)
        {
            var currentActiveCalls = Interlocked.Increment(ref activeCalls);
            UpdateMaximumConcurrentCalls(currentActiveCalls);
            var call = Interlocked.Increment(ref callCount);
            try
            {
                if (call == 1)
                {
                    FirstCallEntered.Set();
                    Assert.True(ReleaseFirstCall.Wait(TimeSpan.FromSeconds(5)));
                }
                else if (call == 2)
                {
                    SecondCallEntered.Set();
                }

                return true;
            }
            finally
            {
                Interlocked.Decrement(ref activeCalls);
            }
        }

        private void UpdateMaximumConcurrentCalls(int concurrentCalls)
        {
            var observed = Volatile.Read(ref maximumConcurrentCalls);
            while (
                concurrentCalls > observed
                && Interlocked.CompareExchange(
                    ref maximumConcurrentCalls,
                    concurrentCalls,
                    observed
                ) != observed
            )
            {
                observed = Volatile.Read(ref maximumConcurrentCalls);
            }
        }
    }

    private const string HomeKey = "Home";
    private const string SettingsKey = "Settings";
    private const string GalleryKey = "Gallery";
}
