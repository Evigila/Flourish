using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml.Linq;
using ArkheideSystem.Flourish.Controls;
using ArkheideSystem.Flourish.Internal.Configuration;
using ArkheideSystem.Flourish.Views.Windows;
using CustomScrollViewer = ArkheideSystem.Flourish.Controls.ScrollViewer;

namespace ArkheideSystem.Flourish.Test.Windows;

public sealed class FlourishShellNavigationLayoutTests
{
    private const string GenericThemeSource =
        "/Flourish;component/Themes/Generic.xaml";
    private const string XamlNamespace =
        "http://schemas.microsoft.com/winfx/2006/xaml";
    private static readonly string RepositoryRoot = FindRepositoryRoot();
    private static readonly string ShellXamlPath = Path.Combine(
        RepositoryRoot,
        "src",
        "Flourish",
        "Views",
        "Windows",
        "FlourishShellWindow.xaml"
    );
    private static readonly string TitlebarXamlPath = Path.Combine(
        RepositoryRoot,
        "src",
        "Flourish",
        "Views",
        "Windows",
        "TitleBar.xaml"
    );
    private static readonly string ShellCodePath = Path.Combine(
        RepositoryRoot,
        "src",
        "Flourish",
        "Views",
        "Windows",
        "FlourishShellWindow.xaml.cs"
    );
    private static readonly string ListBoxItemXamlPath = Path.Combine(
        RepositoryRoot,
        "src",
        "Flourish",
        "Controls",
        "ListBoxItem.xaml"
    );
    private static readonly string LayoutXamlPath = Path.Combine(
        RepositoryRoot,
        "src",
        "Flourish",
        "Themes",
        "Layout.xaml"
    );

    [Fact]
    public void CollapsedNavigationTemplate_ResetsIndentAndCentersItsFixedIconLayout()
    {
        var trigger = GetCollapsedNavigationTrigger();
        var setters = trigger
            .Elements()
            .Where(element => element.Name.LocalName == "Setter")
            .Where(element =>
                (string?)element.Attribute("TargetName") == "NavigationItemLayout"
            )
            .ToDictionary(
                element => (string)element.Attribute("Property")!,
                element => (string)element.Attribute("Value")!,
                StringComparer.Ordinal
            );

        Assert.Equal("0", setters["Margin"]);
        Assert.Equal("Center", setters["HorizontalAlignment"]);
        Assert.Equal("20", setters["Width"]);
    }

    [Fact]
    public void ShellNavigation_UsesExplicitFlourishControlState()
    {
        var nameName = XName.Get("Name", XamlNamespace);
        var shell = XDocument.Load(ShellXamlPath);
        var navigationLists = shell
            .Descendants()
            .Where(element => element.Name.LocalName == "FlourishListBox")
            .Where(element =>
                (string?)element.Attribute(nameName)
                    is "NavigationItemsHost" or "FixedNavigationItemsHost"
            )
            .ToArray();

        Assert.Equal(2, navigationLists.Length);
        Assert.All(
            navigationLists,
            list => Assert.Equal("Navigation", (string?)list.Attribute("Appearance"))
        );

        var compactBinding = (string?)GetCollapsedNavigationTrigger().Attribute("Binding");
        Assert.Contains("FlourishListBox", compactBinding, StringComparison.Ordinal);
        Assert.Contains("IsCompact", compactBinding, StringComparison.Ordinal);
        Assert.DoesNotContain("Tag", compactBinding, StringComparison.Ordinal);

        var shellCode = File.ReadAllText(ShellCodePath);
        Assert.Contains(
            "NavigationItemsHost.IsCompact = !isOpen;",
            shellCode,
            StringComparison.Ordinal
        );
        Assert.Contains(
            "FixedNavigationItemsHost.IsCompact = !isOpen;",
            shellCode,
            StringComparison.Ordinal
        );
    }

    [Fact]
    public void RuntimeNavigationChanges_ClearAndRebuildTheToolbarControlCache()
    {
        var shellCode = File.ReadAllText(ShellCodePath);
        var navigationChangedStart = shellCode.IndexOf(
            "private void NavigationMenuService_Changed(",
            StringComparison.Ordinal
        );
        var toolbarChangedStart = shellCode.IndexOf(
            "private void ToolbarService_Changed(",
            navigationChangedStart,
            StringComparison.Ordinal
        );
        var clearCacheStart = shellCode.IndexOf(
            "private void ClearToolbarButtonCache()",
            StringComparison.Ordinal
        );
        var nextMethodStart = shellCode.IndexOf(
            "private void UpdateRuntimeSurfaceVisibility(",
            clearCacheStart,
            StringComparison.Ordinal
        );

        Assert.True(navigationChangedStart >= 0);
        Assert.True(toolbarChangedStart > navigationChangedStart);
        Assert.True(clearCacheStart >= 0);
        Assert.True(nextMethodStart > clearCacheStart);

        var navigationChangedMethod = shellCode[
            navigationChangedStart..toolbarChangedStart
        ];
        var clearCacheMethod = shellCode[clearCacheStart..nextMethodStart];

        Assert.Contains(
            "ClearToolbarButtonCache();",
            navigationChangedMethod,
            StringComparison.Ordinal
        );
        Assert.Contains(
            "BuildToolbarItems(navigationService.CurrentSourcePageType, force: true);",
            navigationChangedMethod,
            StringComparison.Ordinal
        );
        Assert.Contains(
            "button.Click -= ToolbarButton_Click;",
            clearCacheMethod,
            StringComparison.Ordinal
        );
    }

    [Fact]
    public void NavigationContainer_UsesAppearanceAndCompactTriggersFromItsControlDictionary()
    {
        var document = XDocument.Load(ListBoxItemXamlPath);
        var trigger = document
            .Descendants()
            .Where(element => element.Name.LocalName == "MultiDataTrigger")
            .Single(element =>
                HasTriggerCondition(element, "Appearance", "Navigation")
                && HasTriggerCondition(element, "IsCompact", "True")
            );
        var setters = GetSetterValues(trigger);

        Assert.Equal(
            "{DynamicResource FlourishShellCommandButtonWidth}",
            setters["Width"]
        );
        Assert.Equal(
            "{DynamicResource FlourishShellCommandButtonHeight}",
            setters["Height"]
        );
        Assert.Equal(
            "{DynamicResource FlourishCollapsedNavigationItemMargin}",
            setters["Margin"]
        );
        Assert.Equal("Left", setters["HorizontalAlignment"]);
        Assert.Equal("Center", setters["HorizontalContentAlignment"]);
    }

    [Fact]
    public void ShellChromeInsets_ComposeOneAlignedWindowEdgeBaseline()
    {
        var keyName = XName.Get("Key", XamlNamespace);
        var layout = XDocument.Load(LayoutXamlPath);
        var outerInset = GetDoubleResource(
            layout,
            keyName,
            "FlourishShellOuterInset"
        );
        var titlebarSpacer = GetDoubleResource(
            layout,
            keyName,
            "FlourishTitlebarLeadingSpacerWidth"
        );
        var titlebarMargin = GetThicknessResource(
            layout,
            keyName,
            "FlourishTitlebarNavigationToggleMargin"
        );
        var collapsedItemMargin = GetThicknessResource(
            layout,
            keyName,
            "FlourishCollapsedNavigationItemMargin"
        );
        var leftPadding = GetThicknessResource(
            layout,
            keyName,
            "FlourishNavigationPaneLeftPadding"
        );
        var rightPadding = GetThicknessResource(
            layout,
            keyName,
            "FlourishNavigationPaneRightPadding"
        );
        var customRegionMargin = GetThicknessResource(
            layout,
            keyName,
            "FlourishNavigationCustomRegionMargin"
        );
        var statusBarPadding = GetThicknessResource(
            layout,
            keyName,
            "FlourishStatusBarPadding"
        );

        Assert.Equal(8, outerInset);
        Assert.Equal(outerInset, titlebarSpacer);
        Assert.Equal(new Thickness(outerInset, 0, outerInset, 0), leftPadding);
        Assert.Equal(new Thickness(outerInset, 0, outerInset, 0), rightPadding);
        Assert.Equal(new Thickness(4, 0, 4, 0), customRegionMargin);
        Assert.Equal(new Thickness(12, 3, 12, 3), statusBarPadding);
        Assert.Equal(
            statusBarPadding.Left,
            titlebarSpacer + titlebarMargin.Left
        );
        Assert.Equal(
            statusBarPadding.Left,
            leftPadding.Left + collapsedItemMargin.Left
        );
        Assert.Equal(
            statusBarPadding.Right,
            rightPadding.Right + collapsedItemMargin.Left
        );
    }

    [Fact]
    public void ShellChromeInsets_AreAppliedWithoutMovingContentOrCaptionEdges()
    {
        var shell = XDocument.Load(ShellXamlPath);
        var navigationPane = FindNamedElement(shell, "NavigationPaneBorder");
        var navigationHeader = FindNamedElement(shell, "NavigationHeaderRegionHost");
        var navigationFooter = FindNamedElement(shell, "NavigationFooterRegionHost");
        var transitionHost = FindNamedElement(shell, "NavigationPaneTransitionHost");
        var contentArea = FindNamedElement(shell, "ContentAreaGrid");
        var contentHeader = FindNamedElement(shell, "ContentHeaderRegionHost");
        var toolbarLayout = FindNamedElement(shell, "ToolbarLayoutHost");
        var breadcrumbLayout = FindNamedElement(shell, "BreadcrumbLayoutHost");
        var contentFooter = FindNamedElement(shell, "ContentFooterRegionHost");
        var statusBar = FindNamedElement(shell, "StatusBarBorder");

        Assert.Equal(
            "{DynamicResource FlourishNavigationPaneLeftPadding}",
            (string?)navigationPane.Attribute("Padding")
        );
        Assert.Equal(
            "{DynamicResource FlourishNavigationCustomRegionMargin}",
            (string?)navigationHeader.Attribute("Margin")
        );
        Assert.Equal(
            "{DynamicResource FlourishNavigationCustomRegionMargin}",
            (string?)navigationFooter.Attribute("Margin")
        );
        Assert.Equal(
            "{DynamicResource FlourishStatusBarPadding}",
            (string?)statusBar.Attribute("Padding")
        );
        Assert.Null(contentArea.Attribute("Margin"));
        Assert.All(
            new[] { contentHeader, toolbarLayout, breadcrumbLayout, contentFooter },
            host =>
                Assert.Equal(
                    "{DynamicResource FlourishContentBodyMargin}",
                    (string?)host.Attribute("Margin")
                )
        );
        Assert.Null(transitionHost.Attribute("Margin"));

        var shellCode = File.ReadAllText(ShellCodePath);
        var placementStart = shellCode.IndexOf(
            "private void ApplyNavigationPanelPlacement()",
            StringComparison.Ordinal
        );
        var nextMethodStart = shellCode.IndexOf(
            "private ColumnDefinition GetNavigationPaneColumn()",
            placementStart,
            StringComparison.Ordinal
        );

        Assert.True(placementStart >= 0);
        Assert.True(nextMethodStart > placementStart);
        var placementMethod = shellCode[placementStart..nextMethodStart];
        Assert.Contains("Border.PaddingProperty", placementMethod, StringComparison.Ordinal);
        Assert.Contains(
            "\"FlourishNavigationPaneLeftPadding\"",
            placementMethod,
            StringComparison.Ordinal
        );
        Assert.Contains(
            "\"FlourishNavigationPaneRightPadding\"",
            placementMethod,
            StringComparison.Ordinal
        );
        Assert.Contains(
            "NavigationItemsHost.FlowDirection = System.Windows.FlowDirection.RightToLeft;",
            placementMethod,
            StringComparison.Ordinal
        );
        Assert.Contains(
            "FixedNavigationItemsHost.FlowDirection = System.Windows.FlowDirection.RightToLeft;",
            placementMethod,
            StringComparison.Ordinal
        );
        Assert.Contains(
            "NavigationItemsHost.FlowDirection = System.Windows.FlowDirection.LeftToRight;",
            placementMethod,
            StringComparison.Ordinal
        );
        Assert.Contains(
            "FixedNavigationItemsHost.FlowDirection = System.Windows.FlowDirection.LeftToRight;",
            placementMethod,
            StringComparison.Ordinal
        );
    }

    [Theory]
    [InlineData(FlowDirection.LeftToRight, 64)]
    [InlineData(FlowDirection.LeftToRight, 72)]
    [InlineData(FlowDirection.RightToLeft, 64)]
    [InlineData(FlowDirection.RightToLeft, 72)]
    public void CollapsedNavigation_MirrorsTheSharedOuterBaselineAtAnyPaneWidth(
        FlowDirection flowDirection,
        double paneWidth
    )
    {
        RunInSta(() =>
        {
            var titlebarGeometry = GetTitlebarLeadingButtonGeometry();
            var resources = LoadResourceDictionary(GenericThemeSource);
            var itemTemplate = LoadNavigationItemTemplate();
            var isRightPlaced = flowDirection == FlowDirection.RightToLeft;
            var panePadding = (Thickness)resources[
                isRightPlaced
                    ? "FlourishNavigationPaneRightPadding"
                    : "FlourishNavigationPaneLeftPadding"
            ];
            var parent = new NavigationLayoutItem(new Thickness());
            var child = new NavigationLayoutItem(new Thickness(16, 0, 0, 0));
            var listBox = new FlourishListBox
            {
                Height = 64,
                Appearance = FlourishListBoxAppearance.Navigation,
                FlowDirection = flowDirection,
                IsCompact = true,
                ItemTemplate = itemTemplate,
                ItemsSource = new[]
                {
                    parent,
                    child,
                    new NavigationLayoutItem(new Thickness()),
                },
            };
            var navigationHost = new Border
            {
                Width = paneWidth,
                Height = 64,
                BorderThickness = isRightPlaced
                    ? new Thickness(1, 0, 0, 0)
                    : new Thickness(0, 0, 1, 0),
                Padding = panePadding,
                Child = listBox,
            };
            var window = new Window
            {
                Width = 120,
                Height = 160,
                Left = -10000,
                Top = -10000,
                ShowActivated = false,
                ShowInTaskbar = false,
                Content = navigationHost,
            };
            window.Resources.MergedDictionaries.Add(resources);

            try
            {
                window.Show();
                window.UpdateLayout();

                var parentLayout = GetLayoutSnapshot(listBox, parent, navigationHost);
                var childLayout = GetLayoutSnapshot(listBox, child, navigationHost);
                var scrollViewer = Assert.IsType<CustomScrollViewer>(
                    FindVisualDescendant<CustomScrollViewer>(listBox)
                );
                var scrollPresenter = Assert.IsType<ScrollContentPresenter>(
                    FindVisualDescendant(listBox, "PART_ScrollContentPresenter")
                );
                var verticalScrollBar = Assert.IsType<FlourishScrollBar>(
                    FindVisualDescendant(listBox, "PART_VerticalScrollBar")
                );
                var parentBounds = parentLayout.IconBounds;
                var childBounds = childLayout.IconBounds;
                var scrollBarBounds = GetBounds(verticalScrollBar, navigationHost);
                var expectedIconCenter =
                    isRightPlaced
                        ? navigationHost.ActualWidth
                            - titlebarGeometry.Left
                            - titlebarGeometry.Width / 2
                        : titlebarGeometry.Left + titlebarGeometry.Width / 2;
                var parentContainer = Assert.IsType<FlourishListBoxItem>(
                    listBox.ItemContainerGenerator.ContainerFromItem(parent)
                );
                var childContainer = Assert.IsType<FlourishListBoxItem>(
                    listBox.ItemContainerGenerator.ContainerFromItem(child)
                );

                Assert.True(scrollViewer.ScrollableHeight > 0);
                Assert.Equal(Visibility.Visible, verticalScrollBar.Visibility);
                Assert.InRange(
                    Math.Abs(
                        GetBounds(scrollPresenter, navigationHost).Width
                            + scrollBarBounds.Width
                            - listBox.ActualWidth
                    ),
                    0,
                    0.5
                );
                var outerGap = isRightPlaced
                    ? navigationHost.ActualWidth - parentLayout.ContainerBounds.Right
                    : parentLayout.ContainerBounds.Left;
                Assert.Equal(titlebarGeometry.Left, outerGap, 3);
                Assert.Equal(titlebarGeometry.Width, parentLayout.ContainerBounds.Width, 3);
                Assert.Equal(titlebarGeometry.Height, parentLayout.ContainerBounds.Height, 3);
                Assert.Equal(parentLayout.ContainerBounds.Size, parentLayout.HoverSize);
                Assert.Equal(childLayout.ContainerBounds.Size, childLayout.HoverSize);
                Assert.Equal(5, scrollBarBounds.Width, 3);
                if (isRightPlaced)
                {
                    Assert.True(
                        scrollBarBounds.Right <= parentLayout.ContainerBounds.Left + 0.5,
                        $"Compact scrollbar {scrollBarBounds} overlaps right-placed parent item {parentLayout.ContainerBounds}."
                    );
                    Assert.True(
                        scrollBarBounds.Right <= childLayout.ContainerBounds.Left + 0.5,
                        $"Compact scrollbar {scrollBarBounds} overlaps right-placed child item {childLayout.ContainerBounds}."
                    );
                }
                else
                {
                    Assert.True(
                        parentLayout.ContainerBounds.Right <= scrollBarBounds.Left + 0.5,
                        $"Collapsed parent item {parentLayout.ContainerBounds} overlaps compact scrollbar {scrollBarBounds}."
                    );
                    Assert.True(
                        childLayout.ContainerBounds.Right <= scrollBarBounds.Left + 0.5,
                        $"Collapsed child item {childLayout.ContainerBounds} overlaps compact scrollbar {scrollBarBounds}."
                    );
                }

                Assert.Equal(flowDirection, scrollViewer.FlowDirection);
                Assert.Equal(FlowDirection.LeftToRight, parentContainer.FlowDirection);
                Assert.Equal(FlowDirection.LeftToRight, childContainer.FlowDirection);
                Assert.True(
                    Math.Abs(GetHorizontalCenter(parentBounds) - expectedIconCenter) <= 0.5,
                    $"Parent layout {parentLayout} does not align with the title-bar button center at {expectedIconCenter}."
                );
                Assert.True(
                    Math.Abs(GetHorizontalCenter(childBounds) - expectedIconCenter) <= 0.5,
                    $"Child layout {childLayout} does not align with the title-bar button center at {expectedIconCenter}."
                );
                Assert.True(
                    Math.Abs(
                        GetHorizontalCenter(parentBounds)
                            - GetHorizontalCenter(childBounds)
                    ) <= 0.5,
                    $"Parent and child icon bounds diverge: {parentBounds} versus {childBounds}."
                );
                AssertIconIsInsideViewport(parentBounds, navigationHost.ActualWidth);
                AssertIconIsInsideViewport(childBounds, navigationHost.ActualWidth);

                scrollViewer.ScrollToEnd();
                window.UpdateLayout();
                Assert.True(scrollViewer.VerticalOffset > 0);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void TitlebarLeadingButtons_ExposeTheSharedCollapsedNavigationBaseline()
    {
        var geometry = GetTitlebarLeadingButtonGeometry();

        Assert.Equal(12, geometry.Left);
        Assert.Equal(38, geometry.Width);
        Assert.Equal(32, geometry.Height);
        Assert.Equal("{DynamicResource FlourishFontSizeNavigationIcon}", geometry.IconFontSize);
    }

    [Fact]
    public void TitlebarAndStatusIconHosts_ClearTheCommonButtonMinimumGeometry()
    {
        var titlebar = XDocument.Load(TitlebarXamlPath);
        var shell = XDocument.Load(ShellXamlPath);

        foreach (
            var name in new[]
            {
                "BackButton",
                "ForwardButton",
                "NavigationToggleButton",
                "ThemeToggleButton",
            }
        )
        {
            AssertCompactIconButton(
                titlebar,
                name,
                "{DynamicResource FlourishShellCommandButtonWidth}",
                "{DynamicResource FlourishShellCommandButtonHeight}"
            );
        }

        AssertCompactIconButton(titlebar, "ProfileButton", "34", "32");
        AssertCompactIconButton(shell, "BackgroundTaskQueueButton", "26", "22");
        AssertCompactIconButton(shell, "SystemStatusButton", "26", "22");
    }

    [Fact]
    public void BreadcrumbFeatureRefresh_DoesNotMakeAnEmptyHostConsumeLeadingSpace()
    {
        RunInSta(() =>
        {
            var window = new Window
            {
                Width = 520,
                Height = 100,
                Left = -10000,
                Top = -10000,
                ShowActivated = false,
                ShowInTaskbar = false,
            };
            window.Resources.MergedDictionaries.Add(
                LoadResourceDictionary(GenericThemeSource)
            );
            var titlebar = new FlourishTitlebar { Height = 40 };
            window.Content = titlebar;
            titlebar.SetBreadcrumbNavigationState(
                isVisible: false,
                canGoBack: false,
                canGoForward: false
            );
            ConfigureTitlebarForNavigationOnly(titlebar);

            try
            {
                window.Show();
                window.UpdateLayout();

                var breadcrumbHost = Assert.IsType<StackPanel>(
                    titlebar.FindName("BreadcrumbNavigationHost")
                );
                var navigationToggle = Assert.IsType<IconButton>(
                    titlebar.FindName("NavigationToggleButton")
                );
                var initialLeft = GetBounds(navigationToggle, titlebar).Left;

                // Runtime navigation changes reapply feature flags. They must not overwrite
                // the separate navigation-history state and expose an empty Auto column.
                ConfigureTitlebarForNavigationOnly(titlebar);
                window.UpdateLayout();

                Assert.Equal(Visibility.Collapsed, breadcrumbHost.Visibility);
                Assert.Equal(12, initialLeft, 3);
                Assert.Equal(initialLeft, GetBounds(navigationToggle, titlebar).Left, 3);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void MinimumCollapsedWidth_MatchesTheVisibleShellGeometry()
    {
        var keyName = XName.Get("Key", XamlNamespace);
        var nameName = XName.Get("Name", XamlNamespace);
        var layout = XDocument.Load(LayoutXamlPath);
        var shell = XDocument.Load(ShellXamlPath);
        var titlebarGeometry = GetTitlebarLeadingButtonGeometry();
        var compactScrollBarWidth = GetDoubleResource(
            layout,
            keyName,
            "FlourishCompactScrollBarWidth"
        );
        var paneBorder = shell
            .Descendants()
            .Single(element =>
                element.Name.LocalName == "Border"
                && (string?)element.Attribute(nameName) == "NavigationPaneBorder"
            );
        var dividerWidth = ParseThickness(
            (string)paneBorder.Attribute("BorderThickness")!,
            "NavigationPaneBorder.BorderThickness"
        ).Right;

        var leftPadding = GetThicknessResource(
            layout,
            keyName,
            "FlourishNavigationPaneLeftPadding"
        );
        var itemMargin = GetThicknessResource(
            layout,
            keyName,
            "FlourishCollapsedNavigationItemMargin"
        );
        var rightPadding = GetThicknessResource(
            layout,
            keyName,
            "FlourishNavigationPaneRightPadding"
        );

        Assert.Equal(64, NavigationPanelDimensions.MinimumCollapsedWidth);
        Assert.Equal(leftPadding.Left, leftPadding.Right);
        Assert.Equal(rightPadding.Left, rightPadding.Right);
        Assert.Equal(titlebarGeometry.Left, leftPadding.Left + itemMargin.Left);
        Assert.Equal(
            titlebarGeometry.Left,
            rightPadding.Right + itemMargin.Left
        );
        Assert.Equal(
            NavigationPanelDimensions.MinimumCollapsedWidth,
            titlebarGeometry.Left
                + titlebarGeometry.Width
                + leftPadding.Right
                + compactScrollBarWidth
                + dividerWidth
        );
    }

    [Fact]
    public void TitlebarLeadingSpacer_PreservesFlushCaptionButtonColumns()
    {
        var nameName = XName.Get("Name", XamlNamespace);
        var titlebar = XDocument.Load(TitlebarXamlPath);
        var rootGrid = titlebar
            .Root!
            .Elements()
            .Single(element => element.Name.LocalName == "Grid");
        var columns = rootGrid
            .Elements()
            .Single(element => element.Name.LocalName == "Grid.ColumnDefinitions")
            .Elements()
            .Where(element => element.Name.LocalName == "ColumnDefinition")
            .ToArray();

        Assert.Equal(13, columns.Length);
        Assert.Equal(
            "{DynamicResource FlourishTitlebarLeadingSpacerWidth}",
            (string?)columns[0].Attribute("Width")
        );

        var captionColumns = new Dictionary<string, string>
        {
            ["MinimizeButton"] = "10",
            ["MaximizeButton"] = "11",
            ["CloseButton"] = "12",
        };
        foreach (var (buttonName, expectedColumn) in captionColumns)
        {
            var button = rootGrid
                .Elements()
                .Single(element => (string?)element.Attribute(nameName) == buttonName);
            Assert.Equal(expectedColumn, (string?)button.Attribute("Grid.Column"));
            Assert.Null(button.Attribute("Margin"));
        }
    }

    [Theory]
    [InlineData(FlowDirection.LeftToRight)]
    [InlineData(FlowDirection.RightToLeft)]
    public void ExpandedNavigation_MirrorsScrollbarAndOuterRowBaselineWithoutReversingContent(
        FlowDirection flowDirection
    )
    {
        RunInSta(() =>
        {
            var resources = LoadResourceDictionary(GenericThemeSource);
            var itemTemplate = LoadNavigationItemTemplate();
            var isRightPlaced = flowDirection == FlowDirection.RightToLeft;
            var panePadding = (Thickness)resources[
                isRightPlaced
                    ? "FlourishNavigationPaneRightPadding"
                    : "FlourishNavigationPaneLeftPadding"
            ];
            var parent = new NavigationLayoutItem(new Thickness());
            var child = new NavigationLayoutItem(new Thickness(16, 0, 0, 0));
            var listBox = new FlourishListBox
            {
                Height = 64,
                Appearance = FlourishListBoxAppearance.Navigation,
                FlowDirection = flowDirection,
                IsCompact = false,
                ItemTemplate = itemTemplate,
                ItemsSource = new[]
                {
                    parent,
                    child,
                    new NavigationLayoutItem(new Thickness()),
                },
            };
            var navigationHost = new Border
            {
                Width = 220,
                Height = 64,
                BorderThickness = isRightPlaced
                    ? new Thickness(1, 0, 0, 0)
                    : new Thickness(0, 0, 1, 0),
                Padding = panePadding,
                Child = listBox,
            };
            var window = new Window
            {
                Width = 280,
                Height = 160,
                Left = -10000,
                Top = -10000,
                ShowActivated = false,
                ShowInTaskbar = false,
                Content = navigationHost,
            };
            window.Resources.MergedDictionaries.Add(resources);

            try
            {
                window.Show();
                window.UpdateLayout();

                var parentLayout = GetLayoutSnapshot(listBox, parent, navigationHost);
                var childLayout = GetLayoutSnapshot(listBox, child, navigationHost);
                var scrollPresenter = Assert.IsType<ScrollContentPresenter>(
                    FindVisualDescendant(listBox, "PART_ScrollContentPresenter")
                );
                var verticalScrollBar = Assert.IsType<FlourishScrollBar>(
                    FindVisualDescendant(listBox, "PART_VerticalScrollBar")
                );
                var presenterBounds = GetBounds(scrollPresenter, navigationHost);
                var scrollBarBounds = GetBounds(verticalScrollBar, navigationHost);
                var parentContainer = Assert.IsType<FlourishListBoxItem>(
                    listBox.ItemContainerGenerator.ContainerFromItem(parent)
                );
                var childContainer = Assert.IsType<FlourishListBoxItem>(
                    listBox.ItemContainerGenerator.ContainerFromItem(child)
                );
                var outerGap = isRightPlaced
                    ? navigationHost.ActualWidth - parentLayout.ContainerBounds.Right
                    : parentLayout.ContainerBounds.Left;

                Assert.Equal(Visibility.Visible, verticalScrollBar.Visibility);
                Assert.Equal(7, scrollBarBounds.Width, 3);
                if (isRightPlaced)
                {
                    Assert.True(scrollBarBounds.Right <= presenterBounds.Left + 0.5);
                }
                else
                {
                    Assert.True(presenterBounds.Right <= scrollBarBounds.Left + 0.5);
                }

                Assert.Equal(12, outerGap, 3);
                Assert.Equal(flowDirection, scrollPresenter.FlowDirection);
                Assert.Equal(FlowDirection.LeftToRight, parentContainer.FlowDirection);
                Assert.Equal(FlowDirection.LeftToRight, childContainer.FlowDirection);
                Assert.Equal(new Thickness(), parentLayout.ItemLayoutMargin);
                Assert.Equal(new Thickness(16, 0, 0, 0), childLayout.ItemLayoutMargin);
                Assert.True(
                    childLayout.ItemLayoutBounds.Left
                        > parentLayout.ItemLayoutBounds.Left,
                    $"Child indent was mirrored: parent {parentLayout.ItemLayoutBounds}, child {childLayout.ItemLayoutBounds}."
                );
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Theory]
    [InlineData(FlowDirection.LeftToRight)]
    [InlineData(FlowDirection.RightToLeft)]
    public void NavigationCustomRegion_MirrorsTheTwelveDipOuterBaseline(
        FlowDirection flowDirection
    )
    {
        RunInSta(() =>
        {
            var resources = LoadResourceDictionary(GenericThemeSource);
            var isRightPlaced = flowDirection == FlowDirection.RightToLeft;
            var region = new StackPanel
            {
                Margin = (Thickness)resources["FlourishNavigationCustomRegionMargin"],
            };
            region.Children.Add(new Border { Height = 24 });
            var navigationHost = new Border
            {
                Width = 220,
                Height = 64,
                BorderThickness = isRightPlaced
                    ? new Thickness(1, 0, 0, 0)
                    : new Thickness(0, 0, 1, 0),
                Padding = (Thickness)resources[
                    isRightPlaced
                        ? "FlourishNavigationPaneRightPadding"
                        : "FlourishNavigationPaneLeftPadding"
                ],
                Child = region,
            };
            var window = new Window
            {
                Width = 280,
                Height = 160,
                Left = -10000,
                Top = -10000,
                ShowActivated = false,
                ShowInTaskbar = false,
                Content = navigationHost,
            };
            window.Resources.MergedDictionaries.Add(resources);

            try
            {
                window.Show();
                window.UpdateLayout();

                var regionBounds = GetBounds(region, navigationHost);
                var outerGap = isRightPlaced
                    ? navigationHost.ActualWidth - regionBounds.Right
                    : regionBounds.Left;

                Assert.Equal(12, outerGap, 3);
                Assert.Equal(FlowDirection.LeftToRight, region.FlowDirection);
            }
            finally
            {
                window.Close();
            }
        });
    }

    private static Dictionary<string, string> GetSetterValues(XElement trigger)
    {
        return trigger
            .Elements()
            .Where(element => element.Name.LocalName == "Setter")
            .ToDictionary(
                element => (string)element.Attribute("Property")!,
                element => (string)element.Attribute("Value")!,
                StringComparer.Ordinal
            );
    }

    private static bool HasTriggerCondition(
        XElement trigger,
        string bindingProperty,
        string expectedValue
    )
    {
        return trigger
            .Descendants()
            .Where(element => element.Name.LocalName == "Condition")
            .Any(condition =>
                ((string?)condition.Attribute("Binding"))?.Contains(
                    bindingProperty,
                    StringComparison.Ordinal
                ) == true
                && (string?)condition.Attribute("Value") == expectedValue
            );
    }

    private static XElement FindNamedElement(XDocument document, string name)
    {
        var nameName = XName.Get("Name", XamlNamespace);
        return document
            .Descendants()
            .Single(element => (string?)element.Attribute(nameName) == name);
    }

    private static void ConfigureTitlebarForNavigationOnly(FlourishTitlebar titlebar)
    {
        titlebar.ConfigureVisibility(
            enableSearch: false,
            enableBreadcrumb: true,
            enableNavToggle: true,
            enableLogo: false,
            enableTitle: false,
            enableSubTitle: false,
            enableThemeToggle: false,
            enableProfile: false
        );
    }

    private static XElement GetCollapsedNavigationTrigger()
    {
        var document = XDocument.Load(ShellXamlPath);
        var trigger = document
            .Descendants()
            .Where(element => element.Name.LocalName == "DataTrigger")
            .Single(element =>
                (string?)element.Attribute("Value") == "True"
                && ((string?)element.Attribute("Binding"))?.Contains(
                    "IsCompact",
                    StringComparison.Ordinal
                ) == true
                && ((string?)element.Attribute("Binding"))?.Contains(
                    "FlourishListBox",
                    StringComparison.Ordinal
                ) == true
            );

        return trigger;
    }

    private static TitlebarLeadingGeometry GetTitlebarLeadingButtonGeometry()
    {
        var keyName = XName.Get("Key", XamlNamespace);
        var nameName = XName.Get("Name", XamlNamespace);
        var layout = XDocument.Load(LayoutXamlPath);
        var titlebar = XDocument.Load(TitlebarXamlPath);
        var navigationToggle = titlebar
            .Descendants()
            .Single(element =>
                element.Name.LocalName == "IconButton"
                && (string?)element.Attribute(nameName) == "NavigationToggleButton"
            );
        var navigationIcon = navigationToggle
            .Descendants()
            .Single(element => element.Name.LocalName == "FlourishTextBlock");

        Assert.Equal(
            "{DynamicResource FlourishShellCommandButtonWidth}",
            (string?)navigationToggle.Attribute("Width")
        );
        Assert.Equal(
            "{DynamicResource FlourishShellCommandButtonHeight}",
            (string?)navigationToggle.Attribute("Height")
        );
        Assert.Equal(
            "{DynamicResource FlourishTitlebarNavigationToggleMargin}",
            (string?)navigationToggle.Attribute("Margin")
        );

        var width = GetDoubleResource(layout, keyName, "FlourishShellCommandButtonWidth");
        var height = GetDoubleResource(layout, keyName, "FlourishShellCommandButtonHeight");
        var leadingSpacer = GetDoubleResource(
            layout,
            keyName,
            "FlourishTitlebarLeadingSpacerWidth"
        );
        var margin = GetThicknessResource(
            layout,
            keyName,
            "FlourishTitlebarNavigationToggleMargin"
        );

        return new TitlebarLeadingGeometry(
            leadingSpacer + margin.Left,
            width,
            height,
            (string)navigationIcon.Attribute("FontSize")!
        );
    }

    private static void AssertCompactIconButton(
        XDocument document,
        string name,
        string width,
        string height
    )
    {
        var button = FindNamedElement(document, name);

        Assert.Equal(width, (string?)button.Attribute("Width"));
        Assert.Equal(height, (string?)button.Attribute("Height"));
        Assert.Equal("0", (string?)button.Attribute("MinWidth"));
        Assert.Equal("0", (string?)button.Attribute("MinHeight"));
        Assert.Equal("0", (string?)button.Attribute("Padding"));
    }

    private static double GetDoubleResource(
        XDocument document,
        XName keyName,
        string resourceKey
    )
    {
        var resource = document
            .Descendants()
            .Single(element => (string?)element.Attribute(keyName) == resourceKey);
        return double.Parse(resource.Value, System.Globalization.CultureInfo.InvariantCulture);
    }

    private static Thickness GetThicknessResource(
        XDocument document,
        XName keyName,
        string resourceKey
    )
    {
        var resource = document
            .Descendants()
            .Single(element => (string?)element.Attribute(keyName) == resourceKey);
        return ParseThickness(resource.Value, resourceKey);
    }

    private static Thickness ParseThickness(string value, string description)
    {
        var values = value
            .Split(',')
            .Select(value =>
                double.Parse(value, System.Globalization.CultureInfo.InvariantCulture)
            )
            .ToArray();

        return values.Length switch
        {
            1 => new Thickness(values[0]),
            2 => new Thickness(values[0], values[1], values[0], values[1]),
            4 => new Thickness(values[0], values[1], values[2], values[3]),
            _ => throw new InvalidDataException(
                $"{description} is not a WPF Thickness."
            ),
        };
    }

    private static DataTemplate LoadNavigationItemTemplate()
    {
        var document = XDocument.Load(ShellXamlPath);
        var keyName = XName.Get("Key", XamlNamespace);
        var source = document
            .Descendants()
            .Single(element =>
                element.Name.LocalName == "DataTemplate"
                && (string?)element.Attribute(keyName) == "FlourishNavigationItemTemplate"
            );
        var standalone = new XElement(source);
        standalone.Attribute(keyName)?.Remove();
        standalone.SetAttributeValue(XNamespace.Xmlns + "x", XamlNamespace);
        standalone.SetAttributeValue(
            XNamespace.Xmlns + "control",
            "clr-namespace:ArkheideSystem.Flourish.Controls;assembly=Flourish"
        );
        var looseReaderControlsNamespace = XNamespace.Get(
            "clr-namespace:ArkheideSystem.Flourish.Controls;assembly=Flourish"
        );
        standalone
            .Descendants()
            .Where(element =>
                element.Name.NamespaceName
                    == "clr-namespace:ArkheideSystem.Flourish.Controls"
            )
            .ToList()
            .ForEach(element =>
                element.Name = looseReaderControlsNamespace + element.Name.LocalName
            );

        return Assert.IsType<DataTemplate>(
            XamlReader.Parse(standalone.ToString(SaveOptions.DisableFormatting))
        );
    }

    private static LayoutSnapshot GetLayoutSnapshot(
        FlourishListBox listBox,
        NavigationLayoutItem item,
        Visual? ancestor = null
    )
    {
        var container = Assert.IsType<FlourishListBoxItem>(
            listBox.ItemContainerGenerator.ContainerFromItem(item)
        );
        container.ApplyTemplate();
        var icon = Assert.IsType<FlourishTextBlock>(
            FindVisualDescendant(container, "NavigationItemIcon")
        );
        var layout = Assert.IsType<Grid>(
            FindVisualDescendant(container, "NavigationItemLayout")
        );
        var root = Assert.IsType<Grid>(
            FindVisualDescendant(container, "NavigationTemplateRoot")
        );
        var hoverChrome = Assert.IsType<Border>(
            FindVisualDescendant(container, "HoverChrome")
        );

        return new LayoutSnapshot(
            GetBounds(container, ancestor ?? listBox),
            hoverChrome.RenderSize,
            GetBounds(root, ancestor ?? listBox),
            GetBounds(layout, ancestor ?? listBox),
            GetBounds(icon, ancestor ?? listBox),
            container.HorizontalAlignment,
            container.HorizontalContentAlignment,
            container.Margin,
            container.Padding,
            layout.HorizontalAlignment,
            layout.Margin
        );
    }

    private static Rect GetBounds(FrameworkElement element, Visual ancestor)
    {
        return element.TransformToAncestor(ancestor).TransformBounds(
            new Rect(new Point(), element.RenderSize)
        );
    }

    private static FrameworkElement? FindVisualDescendant(
        DependencyObject root,
        string name
    )
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is FrameworkElement { Name: var childName } element && childName == name)
            {
                return element;
            }

            if (FindVisualDescendant(child, name) is { } descendant)
            {
                return descendant;
            }
        }

        return null;
    }

    private static T? FindVisualDescendant<T>(DependencyObject root)
        where T : DependencyObject
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is T match)
            {
                return match;
            }

            if (FindVisualDescendant<T>(child) is { } descendant)
            {
                return descendant;
            }
        }

        return null;
    }

    private static double GetHorizontalCenter(Rect bounds)
    {
        return bounds.Left + bounds.Width / 2;
    }

    private static void AssertIconIsInsideViewport(Rect bounds, double viewportWidth)
    {
        Assert.True(bounds.Left >= -0.5, $"Icon starts outside the pane at {bounds.Left}.");
        Assert.True(
            bounds.Right <= viewportWidth + 0.5,
            $"Icon ends outside the pane at {bounds.Right}; pane width is {viewportWidth}."
        );
    }

    private static ResourceDictionary LoadResourceDictionary(string source)
    {
        return Assert.IsType<ResourceDictionary>(
            Application.LoadComponent(new Uri(source, UriKind.Relative))
        );
    }

    private static void RunInSta(Action action)
    {
        Exception? error = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception exception)
            {
                error = exception;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (error is not null)
        {
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(error).Throw();
        }
    }

    private static string FindRepositoryRoot()
    {
        for (
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            directory is not null;
            directory = directory.Parent
        )
        {
            if (
                File.Exists(Path.Combine(directory.FullName, "Flourish.slnx"))
                && Directory.Exists(Path.Combine(directory.FullName, "src", "Flourish"))
            )
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException(
            $"Could not locate the Flourish repository above {AppContext.BaseDirectory}."
        );
    }

    private sealed class NavigationLayoutItem(Thickness indentMargin)
    {
        public string Label { get; } = "Navigation item";

        public string IconGlyph { get; } = "\uE80F";

        public string ExpandGlyph { get; } = string.Empty;

        public Thickness IndentMargin { get; } = indentMargin;

        public bool IsGroupHeader { get; } = false;

        public bool IsActiveChildParent { get; } = false;

        public bool IsVisible { get; } = true;

        public bool IsEnabled { get; } = true;

        public bool IsCommandItem { get; } = false;
    }

    private sealed record LayoutSnapshot(
        Rect ContainerBounds,
        Size HoverSize,
        Rect RootBounds,
        Rect ItemLayoutBounds,
        Rect IconBounds,
        HorizontalAlignment ContainerAlignment,
        HorizontalAlignment ContentAlignment,
        Thickness ContainerMargin,
        Thickness ContainerPadding,
        HorizontalAlignment ItemLayoutAlignment,
        Thickness ItemLayoutMargin
    );

    private sealed record TitlebarLeadingGeometry(
        double Left,
        double Width,
        double Height,
        string IconFontSize
    );
}
