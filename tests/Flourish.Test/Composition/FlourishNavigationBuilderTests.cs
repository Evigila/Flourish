using System.Windows.Controls;
using ArkheideSystem.Flourish.Configuration;
using ArkheideSystem.Flourish.Composition;

namespace ArkheideSystem.Flourish.Test.Composition;

public sealed class FlourishNavigationBuilderTests
{
    [Fact]
    public void SetPanelWidth_WithValidValues_UpdatesOptions()
    {
        var options = new FlourishShellOptions();
        var sut = new FlourishNavigationBuilder(options);

        var result = sut.SetPanelWidth(
            openWidth: 280,
            closedWidth: 56,
            maxWidth: 500,
            minWidth: 180
        );

        Assert.Same(sut, result);
        Assert.Equal(280, options.OpenPaneWidth);
        Assert.Equal(56, options.ClosedPaneWidth);
        Assert.Equal(500, options.NavigationPaneMaxWidth);
        Assert.Equal(180, options.NavigationPaneMinWidth);
    }

    [Fact]
    public void SetPanelWidth_WhenClosedWidthExceedsOpenWidth_ThrowsArgumentOutOfRangeException()
    {
        var sut = new FlourishNavigationBuilder(new FlourishShellOptions());

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            sut.SetPanelWidth(openWidth: 200, closedWidth: 201, maxWidth: 400, minWidth: 100)
        );

        Assert.Equal("closedWidth", exception.ParamName);
    }

    [Fact]
    public void SetPanelWidth_WhenMinimumExceedsMaximum_ThrowsArgumentOutOfRangeException()
    {
        var sut = new FlourishNavigationBuilder(new FlourishShellOptions());

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            sut.SetPanelWidth(openWidth: 200, closedWidth: 48, maxWidth: 180, minWidth: 220)
        );

        Assert.Equal("minWidth", exception.ParamName);
    }

    [Theory]
    [InlineData(99)]
    [InlineData(401)]
    public void SetPanelWidth_WhenOpenWidthIsOutsideRange_ThrowsArgumentOutOfRangeException(
        double openWidth
    )
    {
        var sut = new FlourishNavigationBuilder(new FlourishShellOptions());

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            sut.SetPanelWidth(openWidth, closedWidth: 48, maxWidth: 400, minWidth: 100)
        );

        Assert.Equal("openWidth", exception.ParamName);
    }

    [Fact]
    public void SetPanelWidth_WithNonFiniteValue_ThrowsArgumentOutOfRangeException()
    {
        var sut = new FlourishNavigationBuilder(new FlourishShellOptions());

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            sut.SetPanelWidth(double.NaN)
        );

        Assert.Equal("openWidth", exception.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void SetGroup_WithUnnamedNonZeroGroup_ThrowsArgumentException(string? displayName)
    {
        var sut = new FlourishNavigationBuilder(new FlourishShellOptions());

        var exception = Assert.Throws<ArgumentException>(() =>
            sut.SetGroup(displayName, groupId: 1)
        );

        Assert.Equal("displayName", exception.ParamName);
    }

    [Fact]
    public void SetGroup_WithDuplicateGroupId_ThrowsInvalidOperationException()
    {
        var sut = new FlourishNavigationBuilder(new FlourishShellOptions());
        sut.SetGroup("First", groupId: 1);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            sut.SetGroup("Second", groupId: 1)
        );

        Assert.Contains("group ID 1", exception.Message);
    }

    [Fact]
    public void SetGroup_RecordsPageAndCommandDefinitions()
    {
        var options = new FlourishShellOptions();
        var sut = new FlourishNavigationBuilder(options);

        sut.SetGroup("Main", groupId: 1, group =>
        {
            group.AddNavigableViewItem<TestPage>(isInitial: true);
            group.AddNavigableItem("Refresh", "gallery.refresh", iconGlyph: "R");
        });

        var navigationGroup = Assert.Single(options.NavigationGroups);
        Assert.Equal(1, navigationGroup.GroupId);
        Assert.Equal("Main", navigationGroup.Title);
        Assert.Equal(2, navigationGroup.Items.Count);
        Assert.Equal(typeof(TestPage), navigationGroup.Items[0].PageType);
        Assert.True(navigationGroup.Items[0].IsInitial);
        Assert.Equal("gallery.refresh", navigationGroup.Items[1].CommandKey);
        Assert.Equal("R", navigationGroup.Items[1].IconGlyph);
    }

    [Fact]
    public void AddNavigableItem_WithParentAndChildIds_ThrowsArgumentException()
    {
        var sut = new FlourishNavigationBuilder(new FlourishShellOptions());

        var exception = Assert.Throws<ArgumentException>(() =>
            sut.SetGroup(null, groupId: 0, group =>
                group.AddNavigableItem("Invalid", null, parentId: 1, childId: 1)
            )
        );

        Assert.Contains("cannot both be non-zero", exception.Message);
    }

    [Fact]
    public void AddNavigableItem_WithDuplicateParentId_ThrowsInvalidOperationException()
    {
        var sut = new FlourishNavigationBuilder(new FlourishShellOptions());

        var exception = Assert.Throws<InvalidOperationException>(() =>
            sut.SetGroup(null, groupId: 0, group =>
            {
                group.AddNavigableItem("First", null, parentId: 7);
                group.AddNavigableItem("Second", null, parentId: 7);
            })
        );

        Assert.Contains("parentId 7", exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void AddFixedNavigableViewItem_WithBlankKey_ThrowsArgumentException(
        string navigationKey
    )
    {
        var sut = new FlourishNavigationBuilder(new FlourishShellOptions());

        var exception = Assert.Throws<ArgumentException>(() =>
            sut.AddFixedNavigableViewItem(navigationKey)
        );

        Assert.Equal("navigationKey", exception.ParamName);
    }

    private sealed class TestPage : Page { }
}
