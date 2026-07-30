using System.IO;
using System.Xml.Linq;

namespace ArkheideSystem.Flourish.Test.Windows;

public sealed class FlourishShellRenderingContractTests
{
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
    private static readonly string ShellCodePath = Path.Combine(
        RepositoryRoot,
        "src",
        "Flourish",
        "Views",
        "Windows",
        "FlourishShellWindow.xaml.cs"
    );
    private static readonly string NotificationControllerCodePath = Path.Combine(
        RepositoryRoot,
        "src",
        "Flourish",
        "Views",
        "Windows",
        "ShellNotificationController.cs"
    );
    private static string ViewPath(string fileName) =>
        Path.Combine(RepositoryRoot, "src", "Flourish", "Views", "Windows", fileName);

    [Fact]
    public void ShellFloatingSurfaces_UseTheSharedOverlayControl()
    {
        foreach (
            var (fileName, cardName) in new[]
            {
                ("ApplicationInfoOverlay.xaml", "TitleBarFlyoutCard"),
                ("ProfileOverlay.xaml", "ProfileCard"),
                ("StatusOverlay.xaml", "StatusFlyoutCard"),
            }
        )
        {
            var document = XDocument.Load(ViewPath(fileName));
            var card = FindNamedElement(document, cardName);

            Assert.DoesNotContain(
                card.DescendantsAndSelf().Attributes(),
                attribute => attribute.Name.LocalName == "Effect"
            );
            Assert.Equal("Overlay", card.Name.LocalName);
        }

        Assert.Equal(
            "Temporary",
            (string?)
                FindNamedElement(
                    XDocument.Load(ViewPath("ApplicationInfoOverlay.xaml")),
                    "TitleBarFlyoutCard"
                ).Attribute("Variant")
        );
        Assert.Equal(
            "Strong",
            (string?)
                FindNamedElement(
                    XDocument.Load(ViewPath("ProfileOverlay.xaml")),
                    "ProfileCard"
                ).Attribute("Variant")
        );
        Assert.Equal(
            "StatusFlyoutCard_DismissRequested",
            (string?)
                FindNamedElement(
                    XDocument.Load(ViewPath("StatusOverlay.xaml")),
                    "StatusFlyoutCard"
                ).Attribute("DismissRequested")
        );

        var buildNotifications = GetMethod(
            File.ReadAllText(NotificationControllerCodePath),
            "private void BuildNotifications(",
            "private async void NotificationAction_Click("
        );

        Assert.DoesNotContain("EffectProperty", buildNotifications, StringComparison.Ordinal);
        Assert.DoesNotContain(
            "FlourishElevation2Effect",
            buildNotifications,
            StringComparison.Ordinal
        );
        Assert.Contains("FlourishControlStrokeBrush", buildNotifications, StringComparison.Ordinal);
        Assert.Contains(
            "FlourishControlBorderThickness",
            buildNotifications,
            StringComparison.Ordinal
        );
        Assert.Contains("viewsById", buildNotifications, StringComparison.Ordinal);
        Assert.Contains(
            "SynchronizePanelChildren(host.Items, desiredViews)",
            buildNotifications,
            StringComparison.Ordinal
        );
        Assert.DoesNotContain(
            "host.Items.Children.Clear()",
            buildNotifications,
            StringComparison.Ordinal
        );

        var changedHandler = GetMethod(
            File.ReadAllText(NotificationControllerCodePath),
            "private void NotificationService_NotificationsChanged(",
            "private void FlushPendingNotifications("
        );
        Assert.Contains("pendingNotifications = e.Notifications", changedHandler);
        Assert.Contains("e.Version <= pendingVersion", changedHandler);
        Assert.DoesNotContain("notificationService.ActiveNotifications", changedHandler);
        var shellCode = File.ReadAllText(ShellCodePath);
        Assert.Contains("notificationController = new ShellNotificationController(", shellCode);
        Assert.Contains("notificationController.Dispose();", shellCode);
        Assert.DoesNotContain("private void BuildNotifications(", shellCode);
    }

    [Fact]
    public void StatusFlyoutItems_UseARecyclingVirtualizingPanel()
    {
        var document = XDocument.Load(ViewPath("StatusOverlay.xaml"));
        var host = FindNamedElement(document, "StatusFlyoutContentHost");
        var card = FindNamedElement(document, "StatusFlyoutCard");

        Assert.Equal("ItemsControl", host.Name.LocalName);
        Assert.Equal("480", (string?)card.Attribute("MaxHeight"));
        Assert.Equal(
            "True",
            GetAttribute(host, "VirtualizingPanel.IsVirtualizing")
        );
        Assert.Equal(
            "Recycling",
            GetAttribute(host, "VirtualizingPanel.VirtualizationMode")
        );
        Assert.Equal("True", GetAttribute(host, "ScrollViewer.CanContentScroll"));
        Assert.Equal(
            "{StaticResource FlourishVirtualizingItemsControlTemplate}",
            GetAttribute(host, "Template")
        );
        Assert.Contains(
            host.Descendants(),
            element => element.Name.LocalName == "VirtualizingStackPanel"
        );

        var statusOverlayCode = File.ReadAllText(ViewPath("StatusOverlay.xaml.cs"));
        Assert.Contains(
            "internal void SetItems(",
            statusOverlayCode,
            StringComparison.Ordinal
        );
        Assert.DoesNotContain(
            "StatusFlyoutContentHost.Children",
            statusOverlayCode,
            StringComparison.Ordinal
        );
    }

    private static XElement FindNamedElement(XDocument document, string name)
    {
        var nameName = XName.Get("Name", XamlNamespace);
        return document
            .Descendants()
            .Single(element => (string?)element.Attribute(nameName) == name);
    }

    private static string? GetAttribute(XElement element, string localName)
    {
        return (string?)element
            .Attributes()
            .SingleOrDefault(attribute => attribute.Name.LocalName == localName);
    }

    private static string GetMethod(string source, string startMarker, string endMarker)
    {
        var start = source.IndexOf(startMarker, StringComparison.Ordinal);
        var end = source.IndexOf(endMarker, start, StringComparison.Ordinal);
        Assert.True(start >= 0, $"Could not find method marker '{startMarker}'.");
        Assert.True(end > start, $"Could not find method marker '{endMarker}'.");
        return source[start..end];
    }
}
