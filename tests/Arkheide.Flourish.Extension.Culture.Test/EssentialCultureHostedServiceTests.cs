namespace Arkheide.Flourish.Extension.Culture.Test;

public sealed class EssentialCultureHostedServiceTests
{
    [Fact]
    public async Task Lifecycle_SynchronizesOnlyFromFlourishAndUnsubscribesOnStop()
    {
        var previous = Localizer.Current.Culture;
        try
        {
            Localizer.Current.SetCulture("zh-CN");
            var flourishLocalization = new Mock<IFlourishLocalization>();
            flourishLocalization.SetupGet(value => value.CurrentLocale).Returns("en-US");
            var applicator = CreateApplicator();
            var sut = new EssentialCultureHostedService(flourishLocalization.Object, applicator);

            await sut.StartAsync(CancellationToken.None);
            Assert.Equal("en-US", Localizer.Current.Culture);

            flourishLocalization.Raise(
                value => value.Changed += null,
                new FlourishLocalizationChangedEventArgs(
                    FlourishLocalizationChangeKind.LocaleChanged,
                    "en-US",
                    "zh-CN",
                    "zh-CN",
                    null
                )
            );
            Assert.Equal("zh-CN", Localizer.Current.Culture);

            Localizer.Current.SetCulture("en-US");
            flourishLocalization.Verify(
                value => value.SetLocale(It.IsAny<string>()),
                Times.Never
            );

            await sut.StopAsync(CancellationToken.None);
            flourishLocalization.Raise(
                value => value.Changed += null,
                new FlourishLocalizationChangedEventArgs(
                    FlourishLocalizationChangeKind.LocaleChanged,
                    "en-US",
                    "zh-CN",
                    "zh-CN",
                    null
                )
            );
            Assert.Equal("en-US", Localizer.Current.Culture);
        }
        finally
        {
            Localizer.Current.SetCulture(previous);
        }
    }

    private static FlourishShellCultureApplicator CreateApplicator()
    {
        var navigation = new Mock<INavigationMenuService>();
        navigation
            .SetupGet(value => value.Current)
            .Returns(new FlourishNavigationMenuSnapshot([], [], 0));

        var titleBar = new Mock<ITitleBarService>();
        titleBar.SetupGet(value => value.Current).Returns(CreateTitleBarState());

        var search = new Mock<ITitleBarSearchService>();
        search
            .SetupGet(value => value.Current)
            .Returns(new FlourishTitleBarSearchState(string.Empty, "Search", true, false, 0));

        var toolbar = new Mock<IToolbarService>();
        toolbar
            .SetupGet(value => value.Current)
            .Returns(
                new FlourishToolbarSnapshot(
                    false,
                    [],
                    new Dictionary<Type, FlourishPageToolbarSnapshot>(),
                    0
                )
            );

        var statusBar = new Mock<IStatusBarService>();
        statusBar
            .SetupGet(value => value.Current)
            .Returns(new FlourishStatusBarSnapshot(false, false, false, [], 0));

        return new FlourishShellCultureApplicator(
            navigation.Object,
            titleBar.Object,
            search.Object,
            toolbar.Object,
            statusBar.Object
        );
    }

    private static FlourishTitleBarState CreateTitleBarState() =>
        new(
            "Application",
            string.Empty,
            "Unnamed project",
            "Search",
            null,
            "Application",
            true,
            true,
            false,
            true,
            false,
            true,
            true,
            true,
            true,
            true,
            BreadcrumbShowOption.Auto
        );
}
