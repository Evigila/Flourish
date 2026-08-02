using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml.Linq;
using ArkheideSystem.Flourish.Controls;
using ArkheideSystem.Flourish.Internal.Configuration;
using ArkheideSystem.Flourish.Views.Windows;
using FlourishButton = ArkheideSystem.Flourish.Controls.Button;
using ListBox = ArkheideSystem.Flourish.Controls.ListBox;
using CustomScrollViewer = ArkheideSystem.Flourish.Controls.ScrollViewer;

namespace ArkheideSystem.Flourish.Test.Windows;

public sealed class FlourishShellNavigationLayoutTests
{
    private const string GenericThemeSource =
        "/Flourish;component/Themes/Generic.xaml";
    private const string XamlNamespace =
        "http://schemas.microsoft.com/winfx/2006/xaml";
    private static readonly string RepositoryRoot = TestPaths.RepositoryRoot;
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
    private static readonly string StatusBarXamlPath = Path.Combine(
        RepositoryRoot,
        "src",
        "Flourish",
        "Views",
        "Windows",
        "FlourishStatusBar.xaml"
    );
    private static readonly string ContentHostXamlPath = Path.Combine(
        RepositoryRoot,
        "src",
        "Flourish",
        "Views",
        "Windows",
        "FlourishShellContentHost.xaml"
    );
    private static readonly string NavigationPaneXamlPath = Path.Combine(
        RepositoryRoot,
        "src",
        "Flourish",
        "Views",
        "Windows",
        "FlourishNavigationPane.xaml"
    );
    private static readonly string ShellCodePath = Path.Combine(
        RepositoryRoot,
        "src",
        "Flourish",
        "Views",
        "Windows",
        "FlourishShellWindow.xaml.cs"
    );
    private static readonly string ToolbarControllerCodePath = Path.Combine(
        RepositoryRoot,
        "src",
        "Flourish",
        "Internal",
        "Interaction",
        "ShellToolbarController.cs"
    );
    private static readonly string NavigationControllerCodePath = Path.Combine(
        RepositoryRoot,
        "src",
        "Flourish",
        "Internal",
        "Interaction",
        "ShellNavigationController.cs"
    );
    private static readonly string StatusControllerCodePath = Path.Combine(
        RepositoryRoot,
        "src",
        "Flourish",
        "Views",
        "Windows",
        "ShellStatusSurfaceController.cs"
    );
    private static readonly string StatusBarCodePath = Path.ChangeExtension(
        StatusBarXamlPath,
        ".xaml.cs"
    );
    private static readonly string NavigationPaneCodePath = Path.ChangeExtension(
        NavigationPaneXamlPath,
        ".xaml.cs"
    );
    private static readonly string StatusItemViewCachePath = Path.Combine(
        RepositoryRoot,
        "src",
        "Flourish",
        "Internal",
        "Interaction",
        "StatusItemViewCache.cs"
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
        var navigationPane = XDocument.Load(NavigationPaneXamlPath);
        var navigationLists = navigationPane
            .Descendants()
            .Where(element => element.Name.LocalName == "BunchedListBox")
            .Where(element =>
                (string?)element.Attribute(nameName)
                    is "NavigationItemsHost" or "FixedNavigationItemsHost"
            )
            .ToArray();

        Assert.Equal(2, navigationLists.Length);
        Assert.All(
            navigationLists,
            list => Assert.Equal("Borderless", (string?)list.Attribute("Appearance"))
        );

        var compactBinding = (string?)GetCollapsedNavigationTrigger().Attribute("Binding");
        Assert.Contains("ListBox", compactBinding, StringComparison.Ordinal);
        Assert.Contains("IsCompact", compactBinding, StringComparison.Ordinal);
        Assert.DoesNotContain("Tag", compactBinding, StringComparison.Ordinal);

        var navigationPaneCode = File.ReadAllText(NavigationPaneCodePath);
        Assert.Contains(
            "NavigationItemsHost.IsCompact = compact;",
            navigationPaneCode,
            StringComparison.Ordinal
        );
        Assert.Contains(
            "FixedNavigationItemsHost.IsCompact = compact;",
            navigationPaneCode,
            StringComparison.Ordinal
        );
    }

    [Fact]
    public void RuntimeNavigationAndToolbarChanges_KeepInvalidationTargeted()
    {
        var navigationCode = File.ReadAllText(NavigationControllerCodePath);
        var toolbarCode = File.ReadAllText(ToolbarControllerCodePath);
        var navigationChangedStart = navigationCode.IndexOf(
            "private void MenuService_Changed(",
            StringComparison.Ordinal
        );
        var navigationChangedEnd = navigationCode.IndexOf(
            "private void ApplyPanelView(",
            navigationChangedStart,
            StringComparison.Ordinal
        );
        var toolbarChangedStart = toolbarCode.IndexOf(
            "private void Service_Changed(",
            StringComparison.Ordinal
        );
        var clearCacheStart = toolbarCode.IndexOf(
            "private void ClearButtonCache()",
            StringComparison.Ordinal
        );

        Assert.True(navigationChangedStart >= 0);
        Assert.True(navigationChangedEnd > navigationChangedStart);
        Assert.True(toolbarChangedStart >= 0);
        Assert.True(clearCacheStart >= 0);

        var navigationChangedMethod = navigationCode[
            navigationChangedStart..navigationChangedEnd
        ];
        var clearCacheMethod = toolbarCode[clearCacheStart..];

        Assert.DoesNotContain(
            "ClearButtonCache();",
            navigationChangedMethod,
            StringComparison.Ordinal
        );
        Assert.DoesNotContain("toolbarController.SetPage(", navigationChangedMethod, StringComparison.Ordinal);
        var toolbarChangedMethod = toolbarCode[toolbarChangedStart..clearCacheStart];
        Assert.Contains("InvalidateButtonCache(e.PageType, e.Current);", toolbarChangedMethod);
        Assert.Contains("e.PageType == activePageType", toolbarChangedMethod);
        Assert.Contains(
            "button.Click -= Button_Click;",
            clearCacheMethod,
            StringComparison.Ordinal
        );
    }

    [Fact]
    public void CommandAvailabilityChanges_UpdateIndexedButtonsWithoutRebuildingControls()
    {
        var toolbarCode = File.ReadAllText(ToolbarControllerCodePath);
        var handlersStart = toolbarCode.IndexOf(
            "private void CommandRegistry_Changed(",
            StringComparison.Ordinal
        );
        var dispatchStart = toolbarCode.IndexOf(
            "private void Dispatch(",
            handlersStart,
            StringComparison.Ordinal
        );

        Assert.True(handlersStart >= 0);
        Assert.True(dispatchStart > handlersStart);

        var handlers = toolbarCode[handlersStart..dispatchStart];
        Assert.Equal(
            2,
            handlers.Split(
                "commandButtons.Refresh(e.CommandKey)",
                StringSplitOptions.None
            ).Length - 1
        );
        Assert.DoesNotContain("ClearButtonCache", handlers, StringComparison.Ordinal);
        Assert.DoesNotContain("Build(", handlers, StringComparison.Ordinal);
        Assert.DoesNotContain("Items.Refresh", handlers, StringComparison.Ordinal);
    }

    [Fact]
    public void NavigationContainer_UsesAppearanceAndCompactTriggersFromItsControlDictionary()
    {
        var document = XDocument.Load(ListBoxItemXamlPath);
        var trigger = document
            .Descendants()
            .Where(element => element.Name.LocalName == "MultiDataTrigger")
            .Single(element =>
                HasTriggerCondition(element, "Appearance", "Borderless")
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
    public void NavigationGroupHeader_DeclaresTheSmallTypographyTier()
    {
        var navigationPane = XDocument.Load(NavigationPaneXamlPath);
        var groupHeader = FindNamedElement(navigationPane, "NavigationGroupHeader");

        Assert.Equal(
            "{DynamicResource FlourishFontSizeSmall}",
            (string?)groupHeader.Attribute("FontSize")
        );
        Assert.Equal(
            "{DynamicResource FlourishLineHeightSmall}",
            (string?)groupHeader.Attribute("LineHeight")
        );
        Assert.Equal(
            "{DynamicResource FlourishTypographyBottomSpaceSmall}",
            (string?)groupHeader.Attribute("Padding")
        );
        Assert.Equal("Bold", (string?)groupHeader.Attribute("FontWeight"));
    }

    [Fact]
    public void NavigationItemLabel_UsesTheSharedControlTextStyleWithoutALocalOffset()
    {
        var navigationPane = XDocument.Load(NavigationPaneXamlPath);
        var label = FindNamedElement(navigationPane, "NavigationItemLabel");

        Assert.Null(label.Attribute("Padding"));
        Assert.Equal(
            "{DynamicResource FlourishControlTextBlockStyle}",
            (string?)label.Attribute("Style")
        );
    }

    [Fact]
    public void NavigationGroupHeader_ResolvesSmallMetricsAndReplacesTheItemLayoutAtRuntime()
    {
        StaTest.Run(() =>
        {
            var item = new NavigationLayoutItem(
                new Thickness(),
                isGroupHeader: true
            );
            var listBox = new ListBox
            {
                Width = 240,
                Height = 64,
                Appearance = ListBoxAppearance.Borderless,
                ItemTemplate = LoadNavigationItemTemplate(),
                ItemsSource = new[] { item },
            };
            var window = new Window
            {
                Width = 280,
                Height = 120,
                Left = -10000,
                Top = -10000,
                ShowActivated = false,
                ShowInTaskbar = false,
                Content = listBox,
            };
            window.Resources.MergedDictionaries.Add(
                LoadResourceDictionary(GenericThemeSource)
            );

            try
            {
                window.Show();
                window.UpdateLayout();

                var container = Assert.IsType<FlourishListBoxItem>(
                    listBox.ItemContainerGenerator.ContainerFromItem(item)
                );
                container.ApplyTemplate();
                window.UpdateLayout();
                var groupHeader = Assert.IsType<FlourishTextBlock>(
                    FindVisualDescendant(container, "NavigationGroupHeader")
                );
                var itemLayout = Assert.IsType<Grid>(
                    FindVisualDescendant(container, "NavigationItemLayout")
                );

                Assert.Equal(Visibility.Visible, groupHeader.Visibility);
                Assert.Equal(Visibility.Collapsed, itemLayout.Visibility);
                Assert.Equal(12d, groupHeader.FontSize);
                Assert.Equal(14d, groupHeader.LineHeight);
                Assert.Equal(new Thickness(0, 0, 0, 1), groupHeader.Padding);
                Assert.Equal(FontWeights.Bold, groupHeader.FontWeight);
                Assert.Equal(
                    LineStackingStrategy.BlockLineHeight,
                    groupHeader.LineStackingStrategy
                );
            }
            finally
            {
                window.Close();
            }
        });
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
        Assert.Equal(new Thickness(outerInset, 0, 0, 0), leftPadding);
        Assert.Equal(new Thickness(0, 0, outerInset, 0), rightPadding);
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
        var contentHostDocument = XDocument.Load(ContentHostXamlPath);
        var navigationPaneDocument = XDocument.Load(NavigationPaneXamlPath);
        var navigationPane = FindNamedElement(navigationPaneDocument, "NavigationPaneBorder");
        var navigationHeader = FindNamedElement(
            navigationPaneDocument,
            "NavigationHeaderRegionHost"
        );
        var navigationFooter = FindNamedElement(
            navigationPaneDocument,
            "NavigationFooterRegionHost"
        );
        var transitionHost = FindNamedElement(
            navigationPaneDocument,
            "NavigationPaneTransitionHost"
        );
        var contentArea = FindNamedElement(contentHostDocument, "ContentAreaGrid");
        var contentHeader = FindNamedElement(contentHostDocument, "ContentHeaderRegionHost");
        var toolbarLayout = FindNamedElement(contentHostDocument, "ToolbarView");
        var breadcrumbLayout = FindNamedElement(contentHostDocument, "BreadcrumbLayoutHost");
        var contentFooter = FindNamedElement(contentHostDocument, "ContentFooterRegionHost");
        var statusBar = FindNamedElement(
            XDocument.Load(StatusBarXamlPath),
            "StatusBarBorder"
        );

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
        var navigationControllerCode = File.ReadAllText(NavigationControllerCodePath);
        var navigationPaneCode = File.ReadAllText(NavigationPaneCodePath);
        var placementStart = shellCode.IndexOf(
            "private void ApplyNavigationPanelPlacement(NavigationPanelDirection direction)",
            StringComparison.Ordinal
        );
        var nextMethodStart = shellCode.IndexOf(
            "private ColumnDefinition GetNavigationPaneColumn(NavigationPanelDirection direction)",
            placementStart,
            StringComparison.Ordinal
        );

        Assert.True(placementStart >= 0);
        Assert.True(nextMethodStart > placementStart);
        var placementMethod = shellCode[placementStart..nextMethodStart];
        Assert.Contains("Grid.SetColumn(NavigationPane,", placementMethod, StringComparison.Ordinal);
        Assert.Contains(
            "pane.SetDirection(state.Direction);",
            navigationControllerCode,
            StringComparison.Ordinal
        );
        Assert.Contains(
            "\"FlourishNavigationPaneLeftPadding\"",
            navigationPaneCode,
            StringComparison.Ordinal
        );
        Assert.Contains(
            "\"FlourishNavigationPaneRightPadding\"",
            navigationPaneCode,
            StringComparison.Ordinal
        );
        Assert.Contains(
            "NavigationItemsHost.FlowDirection = flowDirection;",
            navigationPaneCode,
            StringComparison.Ordinal
        );
        Assert.Contains(
            "FixedNavigationItemsHost.FlowDirection = flowDirection;",
            navigationPaneCode,
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
        StaTest.Run(() =>
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
            var listBox = new ListBox
            {
                Height = 64,
                Appearance = ListBoxAppearance.Borderless,
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
                Assert.Equal(10, scrollBarBounds.Width, 3);
                if (isRightPlaced)
                {
                    Assert.True(
                        scrollBarBounds.Right <= parentLayout.ContainerBounds.Left + 0.5,
                        $"Standard scrollbar {scrollBarBounds} overlaps right-placed parent item {parentLayout.ContainerBounds}."
                    );
                    Assert.True(
                        scrollBarBounds.Right <= childLayout.ContainerBounds.Left + 0.5,
                        $"Standard scrollbar {scrollBarBounds} overlaps right-placed child item {childLayout.ContainerBounds}."
                    );
                }
                else
                {
                    Assert.True(
                        parentLayout.ContainerBounds.Right <= scrollBarBounds.Left + 0.5,
                        $"Collapsed parent item {parentLayout.ContainerBounds} overlaps standard scrollbar {scrollBarBounds}."
                    );
                    Assert.True(
                        childLayout.ContainerBounds.Right <= scrollBarBounds.Left + 0.5,
                        $"Collapsed child item {childLayout.ContainerBounds} overlaps standard scrollbar {scrollBarBounds}."
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
        Assert.Equal("{DynamicResource FlourishIconFontSizeTitlebar}", geometry.IconFontSize);
    }

    [Fact]
    public void ShellIconContextsUseDedicatedSizesAndCenterStatusHosts()
    {
        var titlebar = XDocument.Load(TitlebarXamlPath);
        var navigationPane = XDocument.Load(NavigationPaneXamlPath);
        var statusBar = XDocument.Load(StatusBarXamlPath);

        foreach (var name in new[] { "BackButton", "ForwardButton", "NavigationToggleButton" })
        {
            var icon = FindNamedElement(titlebar, name)
                .Descendants()
                .Single(element => element.Name.LocalName == "FlourishTextBlock");
            AssertIconTypography(icon, "FlourishIconFontSizeTitlebar");
        }

        AssertIconTypography(
            FindNamedElement(titlebar, "MaximizeButtonIcon"),
            "FlourishIconFontSizeWindowCaption"
        );
        AssertIconTypography(
            FindNamedElement(navigationPane, "NavigationItemIcon"),
            "FlourishIconFontSizeNavigation"
        );
        AssertIconTypography(
            FindNamedElement(navigationPane, "NavigationItemExpander"),
            "FlourishIconFontSizeNavigation"
        );

        var queueButton = FindNamedElement(statusBar, "BackgroundTaskQueueButton");
        Assert.Equal("Center", (string?)queueButton.Attribute("VerticalAlignment"));
        var queueCount = FindNamedElement(statusBar, "BackgroundTaskQueueCountText");
        Assert.Equal("Status", (string?)queueCount.Attribute("Role"));
        Assert.Null(queueCount.Attribute("FontFamily"));
        Assert.DoesNotContain(
            queueButton.Descendants(),
            element =>
                element.Name.LocalName is "Grid" or "Border"
                || ((string?)element.Attribute("Margin"))?.Contains('-') == true
        );

        var systemButton = FindNamedElement(statusBar, "SystemStatusButton");
        Assert.Equal("Center", (string?)systemButton.Attribute("VerticalAlignment"));
        AssertIconTypography(
            systemButton.Descendants().Single(element =>
                element.Name.LocalName == "FlourishTextBlock"
            ),
            "FlourishIconFontSizeStatusBar"
        );

        var controlsRoot = Path.Combine(RepositoryRoot, "src", "Flourish", "Controls");
        var caption = XDocument.Load(Path.Combine(controlsRoot, "WindowCaptionButton.xaml"));
        var captionHost = caption.Descendants().Single(element =>
            element.Name.LocalName == "ContentPresenter"
            && (string?)element.Attribute("TextElement.FontFamily")
                == "{DynamicResource FlourishIconFontFamily}"
        );
        Assert.Equal(
            "{DynamicResource FlourishIconFontSizeWindowCaption}",
            (string?)captionHost.Attribute("TextElement.FontSize")
        );
        Assert.Equal(
            "{DynamicResource FlourishIconFontSizeWindowCaption}",
            (string?)captionHost.Attribute("TextBlock.LineHeight")
        );

        var search = XDocument.Load(Path.Combine(controlsRoot, "SearchBox.xaml"));
        var searchIcon = search.Descendants().Single(element =>
            element.Name.LocalName == "TextBlock"
            && (string?)element.Attribute("FontFamily")
                == "{DynamicResource FlourishIconFontFamily}"
        );
        AssertIconTypography(searchIcon, "FlourishIconFontSizeTitlebarSearch");

        var statusControllerSource = File.ReadAllText(StatusControllerCodePath);
        var statusItemSource = File.ReadAllText(StatusItemViewCachePath);
        foreach (
            var expectedCall in new[]
            {
                "BindIconTypography(icon, \"FlourishIconFontSizeStatusBarBackgroundTask\");",
                "BindIconTypography(icon, \"FlourishIconFontSizeBackgroundTaskView\");",
                "BindIconTypography(icon, \"FlourishIconFontSizeSystemStatusView\");",
            }
        )
        {
            Assert.Contains(expectedCall, statusControllerSource, StringComparison.Ordinal);
        }

        AssertSourceBlockCentersVertically(
            statusItemSource,
            "var root = new WpfStackPanel",
            "root.Children.Add(iconText)"
        );
        Assert.Contains("\"FlourishIconFontSizeStatusBar\"", statusItemSource);
        AssertSourceBlockCentersVertically(
            statusControllerSource,
            "var button = new Button",
            "button.Click"
        );
    }

    [Fact]
    public void StatusBarConfiguredLabelsUseSmallTextAndQueuedTasksUseAPlainCount()
    {
        var statusBar = XDocument.Load(StatusBarXamlPath);
        var queueButton = FindNamedElement(statusBar, "BackgroundTaskQueueButton");
        var queueCount = FindNamedElement(statusBar, "BackgroundTaskQueueCountText");

        Assert.Equal("Collapsed", (string?)queueButton.Attribute("Visibility"));
        Assert.Equal("Center", (string?)queueCount.Attribute("HorizontalAlignment"));
        Assert.Equal("Center", (string?)queueCount.Attribute("VerticalAlignment"));
        Assert.Equal("Center", (string?)queueCount.Attribute("TextAlignment"));
        Assert.Equal("Status", (string?)queueCount.Attribute("Role"));
        Assert.DoesNotContain(
            queueButton.Descendants(),
            element =>
                element.Name.LocalName == "Border"
                || (string?)element.Attribute("FontFamily")
                    == "{DynamicResource FlourishIconFontFamily}"
                || ((string?)element.Attribute("Margin"))?.Contains('-') == true
        );

        var statusControllerSource = File.ReadAllText(StatusControllerCodePath);
        var statusItemsBlock = File.ReadAllText(StatusItemViewCachePath);
        Assert.Contains(
            "\"FlourishFontSizeSmall\"",
            statusItemsBlock,
            StringComparison.Ordinal
        );

        var backgroundTasksBlock = GetSourceBlock(
            statusControllerSource,
            "private void RefreshBackgroundTaskStatus(",
            "private BackgroundTaskIconView CreateBackgroundTaskIconView("
        );
        var statusBarSource = File.ReadAllText(StatusBarCodePath);
        Assert.Contains(
            "count > 0 ? Visibility.Visible : Visibility.Collapsed",
            statusBarSource,
            StringComparison.Ordinal
        );
        Assert.Contains(
            "BackgroundTaskQueueCountText.Text = count.ToString();",
            statusBarSource,
            StringComparison.Ordinal
        );
        Assert.Contains(
            "statusBar.SetQueueState(",
            backgroundTasksBlock,
            StringComparison.Ordinal
        );
        Assert.Contains(
            "FlourishLocaleKeys.BackgroundTaskWaitingCount",
            backgroundTasksBlock,
            StringComparison.Ordinal
        );
    }

    [Fact]
    public void StatusRuntimeChangesUseVersionedSnapshotsWithoutRebuildingThePanel()
    {
        var statusControllerSource = File.ReadAllText(StatusControllerCodePath);
        var handler = GetSourceBlock(
            statusControllerSource,
            "private void StatusService_Changed(",
            "private void BackgroundTaskService_TasksChanged("
        );
        var cacheSource = File.ReadAllText(StatusItemViewCachePath);

        Assert.Contains("pendingStatusChange = e", handler, StringComparison.Ordinal);
        Assert.Contains("FlushPendingStatusChange", handler, StringComparison.Ordinal);
        Assert.Contains("statusItemViews.Apply(change)", handler, StringComparison.Ordinal);
        Assert.Contains("statusBarSnapshot = change.Current", handler, StringComparison.Ordinal);
        Assert.DoesNotContain("statusService.Current", handler, StringComparison.Ordinal);
        Assert.DoesNotContain(
            "statusService.StatusItems",
            statusControllerSource,
            StringComparison.Ordinal
        );
        Assert.DoesNotContain("host.Children.Clear()", cacheSource, StringComparison.Ordinal);
        Assert.Contains("snapshot.Version <= appliedVersion", cacheSource, StringComparison.Ordinal);
        Assert.Contains("snapshot.Version != appliedVersion + 1", cacheSource, StringComparison.Ordinal);
    }

    [Fact]
    public void BackgroundTaskRefreshHotPath_ReusesImmutableSnapshotsAndAvoidsLinqCopies()
    {
        var statusControllerSource = File.ReadAllText(StatusControllerCodePath);
        var changedHandler = GetSourceBlock(
            statusControllerSource,
            "private void BackgroundTaskService_TasksChanged(",
            "private void BackgroundTaskRefreshTimer_Tick("
        );
        Assert.Contains(
            "pendingBackgroundTasks = e.Tasks;",
            changedHandler,
            StringComparison.Ordinal
        );
        Assert.DoesNotContain("ToArray(", changedHandler, StringComparison.Ordinal);

        var timerTick = GetSourceBlock(
            statusControllerSource,
            "private void BackgroundTaskRefreshTimer_Tick(",
            "private void StartBackgroundTaskRefreshTimer("
        );
        Assert.Contains(
            "RefreshBackgroundTaskStatus(tasks);",
            timerTick,
            StringComparison.Ordinal
        );
        Assert.DoesNotContain("ActiveTasks", timerTick, StringComparison.Ordinal);
        Assert.DoesNotContain("ToArray(", timerTick, StringComparison.Ordinal);

        var refresh = GetSourceBlock(
            statusControllerSource,
            "private void RefreshBackgroundTaskStatus(",
            "private BackgroundTaskIconView CreateBackgroundTaskIconView("
        );
        Assert.Contains("backgroundTasks = tasks;", refresh, StringComparison.Ordinal);
        Assert.Contains("foreach (var task in backgroundTasks)", refresh, StringComparison.Ordinal);
        Assert.Contains("queuedTaskCount++;", refresh, StringComparison.Ordinal);
        foreach (
            var allocationPattern in new[]
            {
                "ToArray(",
                ".Where(",
                ".Select(",
                ".ToHashSet(",
                ".Except(",
            }
        )
        {
            Assert.DoesNotContain(allocationPattern, refresh, StringComparison.Ordinal);
        }

        var flyoutRefresh = GetSourceBlock(
            statusControllerSource,
            "private void BuildBackgroundTaskFlyoutContent()",
            "private BackgroundTaskRowView CreateBackgroundTaskRowView("
        );
        Assert.DoesNotContain(".ToHashSet(", flyoutRefresh, StringComparison.Ordinal);
        Assert.DoesNotContain(".Except(", flyoutRefresh, StringComparison.Ordinal);
        Assert.DoesNotContain("ToArray(", flyoutRefresh, StringComparison.Ordinal);

        var staleRemoval = GetSourceBlock(
            statusControllerSource,
            "private static void RemoveStaleBackgroundTaskViews<TView>(",
            "internal sealed class StatusSurfaceOpenRequestedEventArgs("
        );
        Assert.Contains("List<Guid>? staleIds = null;", staleRemoval, StringComparison.Ordinal);
        Assert.Contains("staleIds ??= []", staleRemoval, StringComparison.Ordinal);
        Assert.DoesNotContain(".Except(", staleRemoval, StringComparison.Ordinal);
        Assert.DoesNotContain("ToArray(", staleRemoval, StringComparison.Ordinal);
    }

    [Fact]
    public void TitlebarAndStatusIconHosts_ClearTheCommonButtonMinimumGeometry()
    {
        var titlebar = XDocument.Load(TitlebarXamlPath);
        var statusBar = XDocument.Load(StatusBarXamlPath);

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
            AssertCompactIconOnlyButton(
                titlebar,
                name,
                "{DynamicResource FlourishShellCommandButtonWidth}",
                "{DynamicResource FlourishShellCommandButtonHeight}"
            );
        }

        AssertCompactIconOnlyButton(titlebar, "ProfileButton", "34", "32");
        AssertCompactIconOnlyButton(statusBar, "BackgroundTaskQueueButton", "26", "22");
        AssertCompactIconOnlyButton(statusBar, "SystemStatusButton", "26", "22");
    }

    [Fact]
    public void BreadcrumbFeatureRefresh_DoesNotMakeAnEmptyHostConsumeLeadingSpace()
    {
        StaTest.Run(() =>
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
                var navigationToggle = Assert.IsType<FlourishButton>(
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
        var navigationPane = XDocument.Load(NavigationPaneXamlPath);
        var titlebarGeometry = GetTitlebarLeadingButtonGeometry();
        var scrollBarWidth = GetDoubleResource(
            layout,
            keyName,
            "FlourishScrollBarWidth"
        );
        var paneBorder = navigationPane
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
        Assert.Equal(leftPadding.Right, rightPadding.Left);
        Assert.Equal(titlebarGeometry.Left, leftPadding.Left + itemMargin.Left);
        Assert.Equal(
            titlebarGeometry.Left,
            rightPadding.Right + itemMargin.Left
        );
        var requiredVisibleWidth =
            titlebarGeometry.Left
            + titlebarGeometry.Width
            + leftPadding.Right
            + scrollBarWidth
            + dividerWidth;
        Assert.True(
            NavigationPanelDimensions.MinimumCollapsedWidth >= requiredVisibleWidth,
            $"The collapsed pane width must contain its icon, scrollbar, and divider; required {requiredVisibleWidth}."
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
        StaTest.Run(() =>
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
            var listBox = new ListBox
            {
                Height = 64,
                Appearance = ListBoxAppearance.Borderless,
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
                Assert.Equal(10, scrollBarBounds.Width, 3);
                if (isRightPlaced)
                {
                    Assert.True(scrollBarBounds.Right <= presenterBounds.Left + 0.5);
                    Assert.Equal(
                        navigationHost.BorderThickness.Left,
                        scrollBarBounds.Left,
                        3
                    );
                }
                else
                {
                    Assert.True(presenterBounds.Right <= scrollBarBounds.Left + 0.5);
                    Assert.Equal(
                        navigationHost.ActualWidth
                            - navigationHost.BorderThickness.Right,
                        scrollBarBounds.Right,
                        3
                    );
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

    [Fact]
    public void ExpandedNavigation_ScrollbarMeetsSplitterWithoutLosingItsHitTarget()
    {
        StaTest.Run(() =>
        {
            var resources = LoadResourceDictionary(GenericThemeSource);
            var listBox = new ListBox
            {
                Appearance = ListBoxAppearance.Borderless,
                ItemsSource = Enumerable.Range(0, 20).Select(_ => new NavigationLayoutItem(new Thickness())),
                ItemTemplate = LoadNavigationItemTemplate(),
            };
            var navigationHost = new Border
            {
                BorderThickness = new Thickness(0, 0, 1, 0),
                Padding = (Thickness)resources["FlourishNavigationPaneLeftPadding"],
                Child = listBox,
            };
            var splitter = new FlourishGridSplitter
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                Variant = FlourishGridSplitterVariant.NavigationPane,
            };
            var layout = new Grid { Width = 280, Height = 96 };
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(220) });
            layout.ColumnDefinitions.Add(
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
            );
            Grid.SetColumn(navigationHost, 0);
            Grid.SetColumn(splitter, 0);
            Panel.SetZIndex(splitter, 20);
            layout.Children.Add(navigationHost);
            layout.Children.Add(splitter);

            var window = new Window
            {
                Width = 300,
                Height = 180,
                Left = -10000,
                Top = -10000,
                ShowActivated = false,
                ShowInTaskbar = false,
                Content = layout,
            };
            window.Resources.MergedDictionaries.Add(resources);

            try
            {
                window.Show();
                window.UpdateLayout();

                var scrollBar = Assert.IsType<FlourishScrollBar>(
                    FindVisualDescendant(listBox, "PART_VerticalScrollBar")
                );
                var scrollBarBounds = GetBounds(scrollBar, layout);
                var splitterBounds = GetBounds(splitter, layout);

                Assert.Equal(Visibility.Visible, scrollBar.Visibility);
                Assert.Equal(
                    navigationHost.ActualWidth - navigationHost.BorderThickness.Right,
                    scrollBarBounds.Right,
                    3
                );
                Assert.True(
                    splitterBounds.Left <= scrollBarBounds.Right + 0.5,
                    $"A gap remains between scrollbar {scrollBarBounds} and splitter {splitterBounds}."
                );
                Assert.True(
                    splitterBounds.Left - scrollBarBounds.Left >= scrollBarBounds.Width / 2,
                    $"The splitter hit target obscures too much of the scrollbar: scrollbar {scrollBarBounds}, splitter {splitterBounds}."
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
        StaTest.Run(() =>
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

    private static void AssertIconTypography(XElement element, string sizeResourceName)
    {
        var expected = $"{{DynamicResource {sizeResourceName}}}";
        Assert.Equal(expected, (string?)element.Attribute("FontSize"));
        Assert.Equal(expected, (string?)element.Attribute("LineHeight"));
    }

    private static void AssertSourceBlockCentersVertically(
        string source,
        string startMarker,
        string endMarker
    )
    {
        var start = source.IndexOf(startMarker, StringComparison.Ordinal);
        var end = source.IndexOf(endMarker, start, StringComparison.Ordinal);
        Assert.True(start >= 0 && end > start);
        Assert.Contains(
            "VerticalAlignment = VerticalAlignment.Center",
            source[start..end],
            StringComparison.Ordinal
        );
    }

    private static string GetSourceBlock(string source, string startMarker, string endMarker)
    {
        var start = source.IndexOf(startMarker, StringComparison.Ordinal);
        var end = source.IndexOf(endMarker, start, StringComparison.Ordinal);
        Assert.True(start >= 0 && end > start);
        return source[start..end];
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
            enableThemeToggle: false,
            enableProfile: false
        );
    }

    private static XElement GetCollapsedNavigationTrigger()
    {
        var document = XDocument.Load(NavigationPaneXamlPath);
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
                    "ListBox",
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
                element.Name.LocalName == "Button"
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

    private static void AssertCompactIconOnlyButton(
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
        var document = XDocument.Load(NavigationPaneXamlPath);
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
        ListBox listBox,
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

    private sealed class NavigationLayoutItem(
        Thickness indentMargin,
        bool isGroupHeader = false
    )
    {
        public string Label { get; } = "Navigation item";

        public string IconGlyph { get; } = "\uE80F";

        public string ExpandGlyph { get; } = string.Empty;

        public Thickness IndentMargin { get; } = indentMargin;

        public bool IsGroupHeader { get; } = isGroupHeader;

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
