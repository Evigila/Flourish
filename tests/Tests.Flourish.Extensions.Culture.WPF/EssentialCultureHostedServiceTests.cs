using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using ArkheideSystem.Essential.Culture;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Extensions.Culture.WPF;
using Moq;
using Xunit;

namespace ArkheideSystem.Tests.Flourish.Extensions.Culture.WPF;

public sealed class EssentialCultureHostedServiceTests
{
    [Fact]
    public async Task Lifecycle_SynchronizesOnlyFromFlourishAndUnsubscribesOnStop()
    {
        var previous = Localizer.Current.Culture;
        try
        {
            Localizer.Current.SetCulture("zh-CN");
            var localization = new Mock<ILocalizationService>();
            localization
                .SetupGet(value => value.Current)
                .Returns(new LocalizationState("en-US", ["en-US", "zh-CN"]));
            var applicator = CreateApplicator();
            var sut = new EssentialCultureHostedService(localization.Object, applicator);

            await sut.StartAsync(CancellationToken.None);
            Assert.Equal("en-US", Localizer.Current.Culture);

            localization.Raise(
                value => value.Changed += null,
                new LocalizationChangedEventArgs(
                    LocalizationChangeKind.LocaleChanged,
                    "en-US",
                    "zh-CN",
                    ["zh-CN"],
                    null
                )
            );
            Assert.Equal("zh-CN", Localizer.Current.Culture);

            Localizer.Current.SetCulture("en-US");
            localization.Verify(value => value.SetLocale(It.IsAny<string>()), Times.Never);

            await sut.StopAsync(CancellationToken.None);
            localization.Raise(
                value => value.Changed += null,
                new LocalizationChangedEventArgs(
                    LocalizationChangeKind.LocaleChanged,
                    "en-US",
                    "zh-CN",
                    ["zh-CN"],
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

    private static ShellCultureApplicator CreateApplicator()
    {
        var navigation = new Mock<INavigationService>();
        navigation
            .SetupGet(value => value.Current)
            .Returns(CreateNavigationState(new NavigationMenuSnapshot([], [], 0)));

        var titleBar = new Mock<ITitleBarService>();
        titleBar.SetupGet(value => value.Current).Returns(CreateTitleBarState());

        var toolbar = new Mock<IToolbarService>();
        toolbar
            .SetupGet(value => value.Current)
            .Returns(
                new ToolbarSnapshot(
                    false,
                    [],
                    new Dictionary<Type, PageToolbarSnapshot>(),
                    0
                )
            );

        var statusBar = new Mock<IStatusBarService>();
        statusBar
            .SetupGet(value => value.Current)
            .Returns(new StatusBarSnapshot(false, false, false, [], 0));

        return new ShellCultureApplicator(
            navigation.Object,
            titleBar.Object,
            toolbar.Object,
            statusBar.Object
        );
    }

    private static NavigationState CreateNavigationState(
        NavigationMenuSnapshot menu
    ) =>
        new(
            null,
            null,
            null,
            false,
            false,
            new NavigationRouteSnapshot(
                new Dictionary<string, NavigationRoute>(),
                0
            ),
            menu,
            new NavigationPanelState(
                true,
                true,
                NavigationPanelDirection.Left,
                260,
                64,
                180,
                480,
                0
            ),
            new PageCacheSnapshot(
                new Dictionary<Type, PageCacheMode>(),
                [],
                0
            ),
            0
        );

    private static TitleBarState CreateTitleBarState() =>
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
