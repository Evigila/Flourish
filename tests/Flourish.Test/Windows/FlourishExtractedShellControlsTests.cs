using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using ArkheideSystem.Flourish.Views.Windows;

namespace ArkheideSystem.Flourish.Test.Windows;

public sealed class FlourishExtractedShellControlsTests
{
    private static readonly string WindowsViewRoot = Path.Combine(
        TestPaths.RepositoryRoot,
        "src",
        "Flourish",
        "Views",
        "Windows"
    );

    [Fact]
    public void Shell_UsesExtractedContentAndNotificationHosts()
    {
        var document = XDocument.Load(
            Path.Combine(WindowsViewRoot, "FlourishShellWindow.xaml")
        );
        var names = document
            .Descendants()
            .SelectMany(element => element.Attributes())
            .Where(attribute => attribute.Name.LocalName == "Name")
            .Select(attribute => attribute.Value)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Contains("ContentHost", names);
        Assert.Contains("NotificationHost", names);
        Assert.DoesNotContain("ToolbarLayoutHost", names);
        Assert.DoesNotContain("NotificationItemsHost", names);
    }

    [Fact]
    public void Shell_LeavesExtractedControlEventsToTheirControllers()
    {
        var document = XDocument.Load(
            Path.Combine(WindowsViewRoot, "FlourishShellWindow.xaml")
        );

        Assert.Null(FindNamedElement(document, "NavigationPane").Attribute("ItemRequested"));
        Assert.Null(FindNamedElement(document, "StatusBar").Attribute("AnchorRequested"));
        Assert.Null(FindNamedElement(document, "StatusBar").Attribute("InteractionStarted"));
        Assert.Null(FindNamedElement(document, "ProfileOverlay").Attribute("DismissRequested"));
        Assert.Null(
            FindNamedElement(document, "ProfileOverlay").Attribute("PlacementInvalidated")
        );
        Assert.Null(FindNamedElement(document, "StatusOverlay").Attribute("DismissRequested"));
        Assert.Null(
            FindNamedElement(document, "StatusOverlay").Attribute("PlacementInvalidated")
        );
    }

    [Fact]
    public void Shell_InitializesToolbarControllerBeforeApplyingRegions()
    {
        var source = File.ReadAllText(
            Path.Combine(WindowsViewRoot, "FlourishShellWindow.xaml.cs")
        );

        var initializeIndex = source.IndexOf(
            "toolbarController.Init();",
            StringComparison.Ordinal
        );
        var regionsIndex = source.IndexOf("BuildRegionContents();", StringComparison.Ordinal);

        Assert.True(initializeIndex >= 0);
        Assert.True(regionsIndex > initializeIndex);
    }

    [Fact]
    public void ContentHost_PreservesTheFiveRowShellContentContract()
    {
        var document = XDocument.Load(
            Path.Combine(WindowsViewRoot, "FlourishShellContentHost.xaml")
        );
        var layout = FindNamedElement(document, "ContentAreaGrid");
        var rows = layout
            .Elements()
            .Single(element => element.Name.LocalName == "Grid.RowDefinitions")
            .Elements()
            .Select(element => element.Attribute("Height")?.Value ?? string.Empty)
            .ToArray();

        Assert.Equal(["Auto", "Auto", "Auto", "*", "Auto"], rows);
        Assert.Equal("FlourishToolbar", FindNamedElement(document, "ToolbarView").Name.LocalName);
        Assert.Equal("Frame", FindNamedElement(document, "RootFrame").Name.LocalName);
        Assert.Equal(
            "Grid",
            FindNamedElement(document, "PageTransitionContentHost").Name.LocalName
        );
        Assert.Equal(
            "Grid",
            FindNamedElement(document, "ContentOverlayRegionHost").Name.LocalName
        );
    }

    [Fact]
    public void ContentHost_ExposesNarrowViewStateAndRegionOperations()
    {
        StaTest.Run(() =>
        {
            var sut = new FlourishShellContentHost();
            var header = new Border();

            Assert.Same(sut, sut.LayoutHost);
            Assert.IsType<Frame>(sut.NavigationFrame);
            Assert.Null(sut.CurrentPage);
            Assert.Equal(4, sut.CenteredHosts.Count);
            Assert.Contains(sut.Toolbar, sut.CenteredHosts);

            sut.SetBreadcrumb("Application / Page");
            sut.ApplyCenteredLayout(920);
            sut.SetRegionContent(FlourishRegion.ContentHeader, [header]);

            Assert.All(sut.CenteredHosts, host => Assert.Equal(920, host.MaxWidth));
            var headerHost = Assert.IsType<StackPanel>(
                sut.FindName("ContentHeaderRegionHost")
            );
            Assert.Same(header, headerHost.Children[0]);
        });
    }

    [Fact]
    public void Shell_UsesExtractedStatusAndOverlayHostsAtTheRootLayer()
    {
        var document = XDocument.Load(
            Path.Combine(WindowsViewRoot, "FlourishShellWindow.xaml")
        );
        var statusBar = FindNamedElement(document, "StatusBar");
        var applicationInfo = FindNamedElement(document, "ApplicationInfoOverlay");
        var profile = FindNamedElement(document, "ProfileOverlay");
        var status = FindNamedElement(document, "StatusOverlay");

        Assert.Equal("FlourishStatusBar", statusBar.Name.LocalName);
        Assert.Equal("2", GetAttribute(statusBar, "Grid.Row"));
        Assert.Equal("120", GetAttribute(statusBar, "Panel.ZIndex"));
        AssertOverlayHost(applicationInfo, "145");
        AssertOverlayHost(profile, "130");
        AssertOverlayHost(status, "110");
    }

    [Fact]
    public void ExtractedControls_PreserveTheirShellVisualContracts()
    {
        var toolbar = XDocument.Load(
            Path.Combine(WindowsViewRoot, "FlourishToolbar.xaml")
        );
        var notification = XDocument.Load(
            Path.Combine(WindowsViewRoot, "FlourishNotificationHost.xaml")
        );

        Assert.Equal("UserControl", toolbar.Root!.Name.LocalName);
        Assert.Equal("UserControl", notification.Root!.Name.LocalName);
        Assert.Equal("Border", FindNamedElement(toolbar, "ToolbarHostBorder").Name.LocalName);
        Assert.Equal("StackPanel", FindNamedElement(toolbar, "StartRegionHost").Name.LocalName);
        Assert.Equal("StackPanel", FindNamedElement(toolbar, "ItemHost").Name.LocalName);
        Assert.Equal("StackPanel", FindNamedElement(toolbar, "EndRegionHost").Name.LocalName);

        var notificationItems = FindNamedElement(notification, "ItemHost");
        Assert.Equal("360", (string?)notificationItems.Attribute("Width"));
        Assert.Equal("0,52,16,0", (string?)notificationItems.Attribute("Margin"));
        Assert.Equal(
            "Polite",
            notificationItems
                .Attributes()
                .Single(attribute =>
                    attribute.Name.LocalName.EndsWith(
                        "LiveSetting",
                        StringComparison.Ordinal
                    )
                )
                .Value
        );
    }

    [Fact]
    public void Toolbar_UpdateVisibility_ReflectsEnablementAndHostedContent()
    {
        StaTest.Run(() =>
        {
            var sut = new FlourishToolbar();
            sut.Items.Children.Add(new Border());

            sut.UpdateVisibility(isEnabled: true);
            Assert.Equal(Visibility.Visible, Assert.IsType<Border>(sut.Content).Visibility);

            sut.UpdateVisibility(isEnabled: false);
            Assert.Equal(Visibility.Collapsed, Assert.IsType<Border>(sut.Content).Visibility);
        });
    }

    [Fact]
    public void NotificationHost_UpdateVisibility_ReflectsHostedContent()
    {
        StaTest.Run(() =>
        {
            var sut = new FlourishNotificationHost();

            sut.UpdateVisibility();
            Assert.Equal(Visibility.Collapsed, sut.Visibility);

            sut.Items.Children.Add(new Border());
            sut.UpdateVisibility();
            Assert.Equal(Visibility.Visible, sut.Visibility);
        });
    }

    private static XElement FindNamedElement(XDocument document, string name) =>
        document
            .Descendants()
            .Single(element =>
                element
                    .Attributes()
                    .Any(attribute =>
                        attribute.Name.LocalName == "Name" && attribute.Value == name
                    )
            );

    private static string? GetAttribute(XElement element, string localName) =>
        element
            .Attributes()
            .Single(attribute => attribute.Name.LocalName == localName)
            .Value;

    private static void AssertOverlayHost(XElement element, string zIndex)
    {
        Assert.Equal("3", GetAttribute(element, "Grid.RowSpan"));
        Assert.Equal(zIndex, GetAttribute(element, "Panel.ZIndex"));
    }

}
