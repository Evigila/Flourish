using System.Windows;
using System.Windows.Controls;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;
using ArkheideSystem.Flourish.Views.Windows;
using NavigationListBox = ArkheideSystem.Flourish.Controls.BunchedListBox;

namespace ArkheideSystem.Flourish.Test.Windows;

public sealed class FlourishNavigationPaneTests
{
    [Fact]
    public void SetSelectedItem_SuppressesRequestsAndKeepsTheTwoListsExclusive()
    {
        StaTest.Run(() =>
        {
            var sut = new FlourishNavigationPane();
            var main = CreatePageItem("main");
            var fixedItem = CreatePageItem("fixed", isFixed: true);
            var requested = 0;
            sut.ItemRequested += (_, _) => requested++;
            sut.SetItems([main], [fixedItem]);
            var mainList = Assert.IsType<NavigationListBox>(
                sut.FindName("NavigationItemsHost")
            );
            var fixedList = Assert.IsType<NavigationListBox>(
                sut.FindName("FixedNavigationItemsHost")
            );

            sut.SetSelectedItem(main);
            Assert.Same(main, mainList.SelectedItem);
            Assert.Null(fixedList.SelectedItem);

            sut.SetSelectedItem(fixedItem);
            Assert.Null(mainList.SelectedItem);
            Assert.Same(fixedItem, fixedList.SelectedItem);

            sut.SetSelectedItem(null);
            Assert.Null(mainList.SelectedItem);
            Assert.Null(fixedList.SelectedItem);
            Assert.Equal(0, requested);
        });
    }

    [Fact]
    public void UserSelection_RaisesOneSelectionRequest()
    {
        StaTest.Run(() =>
        {
            var sut = new FlourishNavigationPane();
            var item = CreatePageItem("page");
            NavigationItemRequestedEventArgs? request = null;
            sut.SetItems([item], []);
            sut.ItemRequested += (_, e) => request = e;

            Assert.IsType<NavigationListBox>(
                sut.FindName("NavigationItemsHost")
            ).SelectedItem = item;

            Assert.NotNull(request);
            Assert.Same(item, request.Item);
            Assert.Equal(NavigationItemRequestKind.Selection, request.Kind);
        });
    }

    [Fact]
    public void SetCompactAndDirection_UpdateBothNavigationListsAndPaneEdge()
    {
        StaTest.Run(() =>
        {
            var sut = new FlourishNavigationPane();
            var mainList = Assert.IsType<NavigationListBox>(
                sut.FindName("NavigationItemsHost")
            );
            var fixedList = Assert.IsType<NavigationListBox>(
                sut.FindName("FixedNavigationItemsHost")
            );
            var border = Assert.IsType<Border>(sut.FindName("NavigationPaneBorder"));

            sut.SetCompact(true);
            sut.SetDirection(NavigationPanelDirection.Right);

            Assert.True(mainList.IsCompact);
            Assert.True(fixedList.IsCompact);
            Assert.Equal(FlowDirection.RightToLeft, mainList.FlowDirection);
            Assert.Equal(FlowDirection.RightToLeft, fixedList.FlowDirection);
            Assert.Equal(new Thickness(1, 0, 0, 0), border.BorderThickness);

            sut.SetDirection(NavigationPanelDirection.Left);

            Assert.Equal(FlowDirection.LeftToRight, mainList.FlowDirection);
            Assert.Equal(FlowDirection.LeftToRight, fixedList.FlowDirection);
            Assert.Equal(new Thickness(0, 0, 1, 0), border.BorderThickness);
        });
    }

    private static FlourishNavigationItem CreatePageItem(
        string key,
        bool isFixed = false
    ) =>
        new(
            key,
            key,
            null,
            groupId: 0,
            FlourishNavigationItemKind.Page,
            typeof(TestPage),
            isFixed: isFixed
        );

    private sealed class TestPage : Page;
}
