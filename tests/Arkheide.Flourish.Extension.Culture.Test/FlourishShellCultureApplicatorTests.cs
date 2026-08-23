using System.Windows.Threading;

namespace Arkheide.Flourish.Extension.Culture.Test;

public sealed class FlourishShellCultureApplicatorTests
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
            var sourceItem = FlourishNavigationMenuItem.Page(
                "home",
                "Home",
                NavigationToken,
                "icon"
            );
            var fixedItem = FlourishNavigationMenuItem.Command(
                "fixed",
                NavigationToken,
                "fixed-icon",
                "command"
            );
            var navigation = new Mock<INavigationMenuService>();
            navigation
                .SetupGet(value => value.Current)
                .Returns(
                    new FlourishNavigationMenuSnapshot(
                        [new FlourishNavigationMenuGroup("group", GroupToken, [sourceItem])],
                        [fixedItem],
                        1
                    )
                );
            var editor = new Mock<INavigationMenuEditor>();
            string? groupTitle = null;
            FlourishNavigationMenuItem? localizedItem = null;
            FlourishNavigationMenuItem? localizedFixedItem = null;
            editor
                .Setup(value => value.SetGroupTitle("group", It.IsAny<string?>()))
                .Callback((string _, string? title) => groupTitle = title);
            editor
                .Setup(value => value.SetItem("home", It.IsAny<Func<FlourishNavigationMenuItem, FlourishNavigationMenuItem>>()))
                .Callback(
                    (string _, Func<FlourishNavigationMenuItem, FlourishNavigationMenuItem> update) =>
                        localizedItem = update(sourceItem)
                );
            editor
                .Setup(value => value.SetItem("fixed", It.IsAny<Func<FlourishNavigationMenuItem, FlourishNavigationMenuItem>>()))
                .Callback(
                    (string _, Func<FlourishNavigationMenuItem, FlourishNavigationMenuItem> update) =>
                        localizedFixedItem = update(fixedItem)
                );
            navigation
                .Setup(value => value.Set(It.IsAny<Action<INavigationMenuEditor>>()))
                .Callback((Action<INavigationMenuEditor> update) => update(editor.Object));

            var titleBar = new Mock<ITitleBarService>();
            titleBar.SetupGet(value => value.Current).Returns(CreateTitleBarState());
            var search = new Mock<ITitleBarSearchService>();
            search
                .SetupGet(value => value.Current)
                .Returns(new FlourishTitleBarSearchState(string.Empty, SearchToken, true, false, 1));

            var toolbarItem = new FlourishToolbarItem(ToolbarToken, "icon", "command")
            {
                Id = "toolbar",
            };
            var toolbar = new Mock<IToolbarService>();
            toolbar
                .SetupGet(value => value.Current)
                .Returns(
                    new FlourishToolbarSnapshot(
                        true,
                        [toolbarItem],
                        new Dictionary<Type, FlourishPageToolbarSnapshot>(),
                        1
                    )
                );
            IReadOnlyList<FlourishToolbarItem>? localizedToolbar = null;
            toolbar
                .Setup(value => value.SetDefault(It.IsAny<IEnumerable<FlourishToolbarItem>>()))
                .Callback((IEnumerable<FlourishToolbarItem> items) => localizedToolbar = items.ToArray());

            var statusBar = new Mock<IStatusBarService>();
            statusBar
                .SetupGet(value => value.Current)
                .Returns(
                    new FlourishStatusBarSnapshot(
                        true,
                        false,
                        false,
                        [new FlourishStatusItem("status", StatusToken, "icon")],
                        1
                    )
                );

            using var sut = new FlourishShellCultureApplicator(
                navigation.Object,
                titleBar.Object,
                search.Object,
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
            titleBar.Verify(value => value.SetApplicationSubTitle("Component gallery"), Times.Once);
            titleBar.Verify(value => value.SetUnnamedProjectPlaceholder("Unnamed project"), Times.Once);
            search.Verify(value => value.SetPlaceholder("Search"), Times.Once);
            Assert.Equal("Run", Assert.Single(localizedToolbar!).DisplayName);
            statusBar.Verify(value => value.SetItemText("status", "Ready"), Times.Once);

            Localizer.Current.SetCulture("zh-CN");

            Assert.Equal("导航", groupTitle);
            Assert.Equal("主页", localizedItem?.Label);
            Assert.Equal("主页", localizedFixedItem?.Label);
            titleBar.Verify(value => value.SetApplicationTitle("应用"), Times.Once);
            titleBar.Verify(value => value.SetApplicationSubTitle("组件库"), Times.Once);
            titleBar.Verify(value => value.SetUnnamedProjectPlaceholder("未命名项目"), Times.Once);
            search.Verify(value => value.SetPlaceholder("搜索"), Times.Once);
            Assert.Equal("运行", Assert.Single(localizedToolbar!).DisplayName);
            statusBar.Verify(value => value.SetItemText("status", "就绪"), Times.Once);
        }
        finally
        {
            Localizer.Current.SetCulture(previous);
        }
    }

    private static FlourishTitleBarState CreateTitleBarState() =>
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
