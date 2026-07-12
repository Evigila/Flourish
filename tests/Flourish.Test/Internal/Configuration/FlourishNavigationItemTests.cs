using System.Windows;
using System.Windows.Controls;
using ArkheideSystem.Flourish.Internal.Configuration;

namespace ArkheideSystem.Flourish.Test.Internal.Configuration;

public sealed class FlourishNavigationItemTests
{
    [Fact]
    public void Constructor_WithPageValues_ExposesValuesAndDerivedState()
    {
        var sut = new FlourishNavigationItem(
            "home",
            "Home",
            null,
            groupId: 3,
            FlourishNavigationItemKind.Page,
            typeof(TestPage),
            isInitial: true,
            isFixed: true,
            parentId: 7
        );

        Assert.Equal("home", sut.Key);
        Assert.Equal("Home", sut.Label);
        Assert.Empty(sut.IconGlyph);
        Assert.Equal(3, sut.GroupId);
        Assert.Equal(FlourishNavigationItemKind.Page, sut.Kind);
        Assert.Equal(typeof(TestPage), sut.PageType);
        Assert.Null(sut.CommandKey);
        Assert.True(sut.IsInitial);
        Assert.True(sut.IsFixed);
        Assert.Equal(7, sut.ParentId);
        Assert.Equal(0, sut.ChildId);
        Assert.False(sut.IsGroupHeader);
        Assert.True(sut.IsNavigationItem);
        Assert.True(sut.IsPageItem);
        Assert.False(sut.IsCommandItem);
        Assert.True(sut.IsParent);
        Assert.False(sut.IsChild);
        Assert.Equal(new Thickness(), sut.IndentMargin);
    }

    [Fact]
    public void Constructor_WithCommandValues_ExposesCommandDerivedState()
    {
        var sut = new FlourishNavigationItem(
            "command:refresh",
            "Refresh",
            "R",
            groupId: 2,
            FlourishNavigationItemKind.Command,
            commandKey: "refresh",
            childId: 9
        );

        Assert.True(sut.IsNavigationItem);
        Assert.True(sut.IsCommandItem);
        Assert.False(sut.IsPageItem);
        Assert.False(sut.IsGroupHeader);
        Assert.False(sut.IsParent);
        Assert.True(sut.IsChild);
        Assert.Equal("refresh", sut.CommandKey);
        Assert.Equal(new Thickness(16, 0, 0, 0), sut.IndentMargin);
    }

    [Fact]
    public void Constructor_WithGroupHeader_ExposesHeaderDerivedState()
    {
        var sut = new FlourishNavigationItem(
            "group:2",
            "Settings",
            null,
            groupId: 2,
            FlourishNavigationItemKind.GroupHeader
        );

        Assert.True(sut.IsGroupHeader);
        Assert.False(sut.IsNavigationItem);
        Assert.False(sut.IsPageItem);
        Assert.False(sut.IsCommandItem);
    }

    [Fact]
    public void IsActiveChildParent_WhenValueChanges_RaisesPropertyChangedOnce()
    {
        var sut = CreateCommandItem();
        var propertyNames = ObservePropertyChanges(sut);

        sut.IsActiveChildParent = true;
        sut.IsActiveChildParent = true;

        Assert.True(sut.IsActiveChildParent);
        Assert.Equal(["IsActiveChildParent"], propertyNames);
    }

    [Fact]
    public void IsVisible_WhenValueChanges_RaisesPropertyChangedOnce()
    {
        var sut = CreateCommandItem();
        var propertyNames = ObservePropertyChanges(sut);

        sut.IsVisible = false;
        sut.IsVisible = false;

        Assert.False(sut.IsVisible);
        Assert.Equal(["IsVisible"], propertyNames);
    }

    [Fact]
    public void IsExpanded_WhenValueChanges_RaisesNotificationsForStateAndGlyph()
    {
        var sut = CreateCommandItem();
        sut.HasChildren = true;
        var propertyNames = ObservePropertyChanges(sut);

        sut.IsExpanded = true;
        sut.IsExpanded = true;

        Assert.True(sut.IsExpanded);
        Assert.Equal("\uE70D", sut.ExpandGlyph);
        Assert.Equal(["IsExpanded", "ExpandGlyph"], propertyNames);
    }

    [Fact]
    public void ExpandGlyph_ReflectsChildrenAndExpansionState()
    {
        var sut = CreateCommandItem();

        Assert.Empty(sut.ExpandGlyph);

        sut.HasChildren = true;
        Assert.Equal("\uE76C", sut.ExpandGlyph);

        sut.IsExpanded = true;
        Assert.Equal("\uE70D", sut.ExpandGlyph);
    }

    [Fact]
    public void Validate_WithPageTypeDerivedFromPage_DoesNotThrow()
    {
        var sut = new FlourishNavigationItem(
            "page",
            "Page",
            null,
            groupId: 0,
            FlourishNavigationItemKind.Page,
            typeof(TestPage)
        );

        sut.Validate();
    }

    [Fact]
    public void Validate_WithPageItemUsingNonPageType_ThrowsArgumentException()
    {
        var sut = new FlourishNavigationItem(
            "invalid",
            "Invalid",
            null,
            groupId: 0,
            FlourishNavigationItemKind.Page,
            typeof(string)
        );

        var exception = Assert.Throws<ArgumentException>(sut.Validate);

        Assert.Equal("PageType", exception.ParamName);
    }

    [Fact]
    public void Validate_WithNullPageType_ThrowsInvalidOperationException()
    {
        var pageWithoutType = new FlourishNavigationItem(
            "unresolved",
            "Unresolved",
            null,
            groupId: 0,
            FlourishNavigationItemKind.Page
        );
        var exception = Assert.Throws<InvalidOperationException>(pageWithoutType.Validate);

        Assert.Contains("Page type", exception.Message);
    }

    [Fact]
    public void Validate_WithNonPageItemUsingUnusedPageType_DoesNotThrow()
    {
        var commandWithUnusedPageType = new FlourishNavigationItem(
            "command",
            "Command",
            null,
            groupId: 0,
            FlourishNavigationItemKind.Command,
            typeof(string)
        );

        commandWithUnusedPageType.Validate();
    }

    private static FlourishNavigationItem CreateCommandItem()
    {
        return new FlourishNavigationItem(
            "command",
            "Command",
            null,
            groupId: 0,
            FlourishNavigationItemKind.Command
        );
    }

    private static List<string?> ObservePropertyChanges(FlourishNavigationItem item)
    {
        var propertyNames = new List<string?>();
        item.PropertyChanged += (_, args) => propertyNames.Add(args.PropertyName);
        return propertyNames;
    }

    private sealed class TestPage : Page { }
}
