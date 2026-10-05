using System;
using System.Collections.Generic;
using System.Linq;

using ArkheideSystem.Essential.Culture;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Extensions.Culture.WPF;
using Moq;
using System.Windows.Threading;
using Xunit;

namespace ArkheideSystem.Tests.Flourish.Extensions.Culture.WPF;

public sealed class ShellCultureApplicatorTests
{
    private const string GroupToken = "Key.NavigationGroup";
    private const string NavigationToken = "Key.NavigationItem";
    private const string TitleToken = "Key.ApplicationTitle";
    private const string SubTitleToken = "Key.ApplicationSubTitle";
    private const string UnnamedProjectToken = "Key.UnnamedProject";
    private const string SearchToken = "Key.SearchPlaceholder";
    private const string ToolbarToken = "Key.ToolbarItem";
    private const string StatusToken = "Key.StatusText";

    [Fact]
    public void Start_LocalizesPersistentShellTokensAndPreservesIdentity()
    {
        var previous = Localizer.Current.Culture;
        try
        {
            Localizer.Current.SetCulture("en-US");
            var sourceItem = NavigationMenuItem.Page(
                "home",
                "Home",
                NavigationToken,
                "icon"
            );
            var fixedItem = NavigationMenuItem.Command(
                "fixed",
                NavigationToken,
                "fixed-icon",
                "command"
            );
            var menu = new NavigationMenuSnapshot(
                [new NavigationMenuGroup("group", GroupToken, [sourceItem])],
                [fixedItem],
                1
            );
            var navigation = new Mock<INavigationService>();
            navigation
                .SetupGet(value => value.Current)
                .Returns(CreateNavigationState(menu));
            var editor = new Mock<INavigationMenuEditor>();
            string? groupTitle = null;
            NavigationMenuItem? localizedItem = null;
            NavigationMenuItem? localizedFixedItem = null;
            editor
                .Setup(value => value.SetGroupTitle("group", It.IsAny<string?>()))
                .Callback((string _, string? title) => groupTitle = title);
            editor
                .Setup(value => value.SetItem("home", It.IsAny<Func<NavigationMenuItem, NavigationMenuItem>>()))
                .Callback(
                    (string _, Func<NavigationMenuItem, NavigationMenuItem> update) =>
                        localizedItem = update(sourceItem)
                );
            editor
                .Setup(value => value.SetItem("fixed", It.IsAny<Func<NavigationMenuItem, NavigationMenuItem>>()))
                .Callback(
                    (string _, Func<NavigationMenuItem, NavigationMenuItem> update) =>
                        localizedFixedItem = update(fixedItem)
                );
            navigation
                .Setup(value => value.SetMenu(It.IsAny<Action<INavigationMenuEditor>>()))
                .Callback((Action<INavigationMenuEditor> update) => update(editor.Object));

            var titleBar = new Mock<ITitleBarService>();
            titleBar.SetupGet(value => value.Current).Returns(CreateTitleBarState());

            var toolbarItem = new ToolbarItem(ToolbarToken, "icon", "command")
            {
                Id = "toolbar",
            };
            var toolbar = new Mock<IToolbarService>();
            toolbar
                .SetupGet(value => value.Current)
                .Returns(
                    new ToolbarSnapshot(
                        true,
                        [toolbarItem],
                        new Dictionary<Type, PageToolbarSnapshot>(),
                        1
                    )
                );
            IReadOnlyList<ToolbarItem>? localizedToolbar = null;
            toolbar
                .Setup(value => value.SetDefault(It.IsAny<IEnumerable<ToolbarItem>>()))
                .Callback((IEnumerable<ToolbarItem> items) => localizedToolbar = items.ToArray());

            var statusBar = new Mock<IStatusBarService>();
            statusBar
                .SetupGet(value => value.Current)
                .Returns(
                    new StatusBarSnapshot(
                        true,
                        false,
                        false,
                        [new StatusBarItem("status", StatusToken, "icon")],
                        1
                    )
                );

            using var sut = new ShellCultureApplicator(
                navigation.Object,
                titleBar.Object,
                toolbar.Object,
                statusBar.Object
            );

            sut.Start(Dispatcher.CurrentDispatcher);

            Assert.Equal("Navigation", groupTitle);
            Assert.Equal("Home", localizedItem?.Label);
            Assert.Equal(sourceItem.Id, localizedItem?.Id);
            Assert.Equal(sourceItem.NavigationKey, localizedItem?.NavigationKey);
            Assert.Equal("Home", localizedFixedItem?.Label);
            Assert.Equal(fixedItem.CommandKey, localizedFixedItem?.CommandKey);
            titleBar.Verify(value => value.SetApplicationTitle("Application"), Times.Once);
            titleBar.Verify(value => value.SetApplicationSubtitle("Component gallery"), Times.Once);
            titleBar.Verify(value => value.SetUnnamedProjectPlaceholder("Unnamed project"), Times.Once);
            titleBar.Verify(value => value.SetSearchPlaceholder("Search"), Times.Once);
            Assert.Equal("Run", Assert.Single(localizedToolbar!).DisplayName);
            statusBar.Verify(value => value.SetItemText("status", "Ready"), Times.Once);

            Localizer.Current.SetCulture("zh-CN");

            Assert.Equal("导航", groupTitle);
            Assert.Equal("主页", localizedItem?.Label);
            Assert.Equal("主页", localizedFixedItem?.Label);
            titleBar.Verify(value => value.SetApplicationTitle("应用"), Times.Once);
            titleBar.Verify(value => value.SetApplicationSubtitle("组件库"), Times.Once);
            titleBar.Verify(value => value.SetUnnamedProjectPlaceholder("未命名项目"), Times.Once);
            titleBar.Verify(value => value.SetSearchPlaceholder("搜索"), Times.Once);
            Assert.Equal("运行", Assert.Single(localizedToolbar!).DisplayName);
            statusBar.Verify(value => value.SetItemText("status", "就绪"), Times.Once);
        }
        finally
        {
            Localizer.Current.SetCulture(previous);
        }
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
            TitleToken,
            SubTitleToken,
            UnnamedProjectToken,
            SearchToken,
            null,
            "Literal logo",
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
