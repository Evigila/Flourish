using System.IO;
using System.Xml.Linq;

namespace ArkheideSystem.Flourish.Test.Windows;

public sealed class FlourishShellRenderingContractTests
{
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
    private static readonly string ShellCodePath = Path.Combine(
        RepositoryRoot,
        "src",
        "Flourish",
        "Views",
        "Windows",
        "FlourishShellWindow.xaml.cs"
    );

    [Fact]
    public void TransientShellCards_UseStrokeChromeInsteadOfRealtimeEffects()
    {
        var document = XDocument.Load(ShellXamlPath);

        foreach (
            var cardName in new[] { "ProfileCard", "StatusFlyoutCard", "TitleBarFlyoutCard" }
        )
        {
            var card = FindNamedElement(document, cardName);

            Assert.DoesNotContain(
                card.DescendantsAndSelf().Attributes(),
                attribute => attribute.Name.LocalName == "Effect"
            );
            Assert.Contains(
                card.Descendants("{http://schemas.microsoft.com/winfx/2006/xaml/presentation}Border"),
                border =>
                    (string?)border.Attribute("BorderBrush")
                        == "{DynamicResource FlourishControlStrokeBrush}"
                    && (string?)border.Attribute("BorderThickness")
                        == "{DynamicResource FlourishControlBorderThickness}"
            );
        }

        var buildNotifications = GetMethod(
            File.ReadAllText(ShellCodePath),
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
    }

    [Fact]
    public void StatusFlyoutItems_UseARecyclingVirtualizingPanel()
    {
        var document = XDocument.Load(ShellXamlPath);
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

        var shellCode = File.ReadAllText(ShellCodePath);
        Assert.Contains(
            "SynchronizeItems(StatusFlyoutContentHost, desiredRows);",
            shellCode,
            StringComparison.Ordinal
        );
        Assert.DoesNotContain(
            "StatusFlyoutContentHost.Children",
            shellCode,
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

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, "src", "Flourish")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not find the Flourish repository root.");
    }
}
