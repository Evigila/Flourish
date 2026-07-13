using System.Windows.Controls;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;
using ArkheideSystem.Flourish.Internal.Composition;

namespace ArkheideSystem.Flourish.Test.Internal.Composition;

public sealed class FlourishNavigationBuilderTests
{
    [Fact]
    public void SetInitiallyOpen_UpdatesOptionsAndReturnsBuilder()
    {
        var options = new FlourishShellOptions();
        var sut = new FlourishNavigationBuilder(options);

        var result = sut.SetInitiallyOpen();

        Assert.Same(sut, result);
        Assert.True(options.IsNavigationPanelInitiallyOpen);
    }

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
    public void SetPanelWidth_DefaultsReserveTheAlignedCollapsedShellGeometry()
    {
        var options = new FlourishShellOptions();
        var sut = new FlourishNavigationBuilder(options);

        sut.SetPanelWidth();

        var closedWidthParameter = typeof(IFlourishNavigationBuilder)
            .GetMethod(nameof(IFlourishNavigationBuilder.SetPanelWidth))!
            .GetParameters()
            .Single(parameter => parameter.Name == "closedWidth");

        Assert.Equal(56d, closedWidthParameter.DefaultValue);
        Assert.Equal(NavigationPanelDimensions.MinimumCollapsedWidth, options.ClosedPaneWidth);
        Assert.Equal(56, options.ClosedPaneWidth);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(56)]
    public void SetPanelWidth_WithSupportedCollapsedBoundary_UpdatesOptions(
        double closedWidth
    )
    {
        var options = new FlourishShellOptions();
        var sut = new FlourishNavigationBuilder(options);

        sut.SetPanelWidth(280, closedWidth, 500, 180);

        Assert.Equal(closedWidth, options.ClosedPaneWidth);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(55)]
    public void SetPanelWidth_WithUndersizedVisibleCollapsedWidth_ThrowsArgumentOutOfRangeException(
        double closedWidth
    )
    {
        var options = new FlourishShellOptions();
        var sut = new FlourishNavigationBuilder(options);
        var before = options.ClosedPaneWidth;

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            sut.SetPanelWidth(280, closedWidth, 500, 180)
        );

        Assert.Equal("closedWidth", exception.ParamName);
        Assert.Equal(before, options.ClosedPaneWidth);
        Assert.Contains("0 (fully hidden) or at least 56", exception.Message);
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
            sut.SetPanelWidth(openWidth: 200, closedWidth: 56, maxWidth: 180, minWidth: 220)
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
            sut.SetPanelWidth(openWidth, closedWidth: 56, maxWidth: 400, minWidth: 100)
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

    [Fact]
    public void SetPanelWidth_WithNonPositiveOpenWidth_ThrowsArgumentOutOfRangeException()
    {
        var sut = new FlourishNavigationBuilder(new FlourishShellOptions());

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            sut.SetPanelWidth(openWidth: 0, closedWidth: 0, maxWidth: 400, minWidth: 100)
        );

        Assert.Equal("openWidth", exception.ParamName);
    }

    [Fact]
    public void SetPanelWidth_WithNegativeClosedWidth_ThrowsArgumentOutOfRangeException()
    {
        var sut = new FlourishNavigationBuilder(new FlourishShellOptions());

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            sut.SetPanelWidth(openWidth: 200, closedWidth: -1, maxWidth: 400, minWidth: 100)
        );

        Assert.Equal("closedWidth", exception.ParamName);
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
            group.AddNavigableItem("Refresh", "R", "gallery.refresh");
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
                group.AddNavigableItem("Invalid", null, null, parentId: 1, childId: 1)
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
                group.AddNavigableItem("First", null, null, parentId: 7);
                group.AddNavigableItem("Second", null, null, parentId: 7);
            })
        );

        Assert.Contains("parentId 7", exception.Message);
    }

    [Fact]
    public void AddFixedItems_RecordsGenericPageAndCommandDefinitions()
    {
        var options = new FlourishShellOptions();
        var sut = new FlourishNavigationBuilder(options);

        sut.AddFixedNavigableViewItem<TestPage>(isInitial: true, parentId: 7);
        sut.AddFixedNavigableItem(
            "Refresh",
            "R",
            "app.refresh",
            childId: 7
        );

        Assert.Collection(
            options.FixedNavigationItemDefinitions,
            page =>
            {
                Assert.True(page.IsPageItem);
                Assert.Equal(typeof(TestPage), page.PageType);
                Assert.True(page.IsInitial);
                Assert.True(page.IsFixed);
                Assert.Equal(7, page.ParentId);
            },
            command =>
            {
                Assert.True(command.IsCommandItem);
                Assert.Equal("Refresh", command.Label);
                Assert.Equal("app.refresh", command.CommandKey);
                Assert.Equal("R", command.IconGlyph);
                Assert.True(command.IsFixed);
                Assert.Equal(7, command.ChildId);
            }
        );
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AddFixedNavigableItem_WithMissingDisplayName_ThrowsArgumentException(
        string? displayName
    )
    {
        var sut = new FlourishNavigationBuilder(new FlourishShellOptions());

        var exception = Assert.Throws<ArgumentException>(() =>
            sut.AddFixedNavigableItem(displayName!, null, "command")
        );

        Assert.Equal("displayName", exception.ParamName);
    }

    private sealed class TestPage : Page { }
}
