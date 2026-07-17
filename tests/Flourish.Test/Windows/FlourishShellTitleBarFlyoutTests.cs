using System.IO;
using System.Xml.Linq;

namespace ArkheideSystem.Flourish.Test.Windows;

public sealed class FlourishShellTitleBarFlyoutTests
{
    private const string XamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";
    private static readonly string RepositoryRoot = FindRepositoryRoot();
    private static readonly string TitleBarXamlPath = Path.Combine(
        RepositoryRoot,
        "src",
        "Flourish",
        "Views",
        "Windows",
        "TitleBar.xaml"
    );
    private static readonly string ShellXamlPath = Path.Combine(
        RepositoryRoot,
        "src",
        "Flourish",
        "Views",
        "Windows",
        "FlourishShellWindow.xaml"
    );
    private static readonly string ShellCode = File.ReadAllText(
        Path.Combine(
            RepositoryRoot,
            "src",
            "Flourish",
            "Views",
            "Windows",
            "FlourishShellWindow.xaml.cs"
        )
    );

    [Fact]
    public void BrandIdentity_UsesInteractiveButtonFamilyWithoutDirectSubtitleText()
    {
        var document = XDocument.Load(TitleBarXamlPath);

        Assert.Equal("IconButton", FindNamedElement(document, "LogoButton").Name.LocalName);
        Assert.Equal("Button", FindNamedElement(document, "TitleButton").Name.LocalName);
        Assert.Equal("Text", (string?)FindNamedElement(document, "LogoButton").Attribute("Variant"));
        Assert.Equal("Text", (string?)FindNamedElement(document, "TitleButton").Attribute("Variant"));
        Assert.DoesNotContain(
            document.Descendants(),
            element => (string?)element.Attribute(XName.Get("Name", XamlNamespace)) == "SubtitleText"
        );
    }

    [Fact]
    public void TitleBarFlyout_IsWindowBoundedAndKeepsItsTriggersAboveTheOverlay()
    {
        var document = XDocument.Load(ShellXamlPath);
        var titleBar = FindNamedElement(document, "Titlebar");
        var overlay = FindNamedElement(document, "TitleBarFlyoutOverlay");
        var card = FindNamedElement(document, "TitleBarFlyoutCard");

        Assert.Equal("3", GetAttribute(overlay, "Grid.RowSpan"));
        Assert.Equal("Transparent", (string?)overlay.Attribute("Background"));
        Assert.Equal("Collapsed", (string?)overlay.Attribute("Visibility"));
        Assert.Equal("True", (string?)overlay.Attribute("ClipToBounds"));
        Assert.True(
            int.Parse(GetAttribute(titleBar, "Panel.ZIndex")!)
                > int.Parse(GetAttribute(overlay, "Panel.ZIndex")!)
        );
        Assert.Equal("Cycle", GetAttribute(card, "KeyboardNavigation.TabNavigation"));
    }

    [Fact]
    public void ProjectMenu_RequestsApplicationWorkInsteadOfChangingTheActiveProjectDirectly()
    {
        var selection = GetMethod(
            "private void ProjectMenuItem_Click(",
            "private void NewProjectButton_Click("
        );
        var creation = GetMethod(
            "private void NewProjectButton_Click(",
            "private void FocusTitleBarFlyoutContent("
        );

        Assert.Contains("projectService.TryRequestProjectActivation(projectId);", selection);
        Assert.DoesNotContain("SetActiveProject", selection, StringComparison.Ordinal);
        Assert.Contains("projectService.RequestNewProject();", creation);
        Assert.DoesNotContain("AddProject", creation, StringComparison.Ordinal);
    }

    [Fact]
    public void DisplayedTitle_UsesProjectOrPlaceholderOnlyInMultiProjectMode()
    {
        var method = GetMethod(
            "private string GetDisplayedTitle()",
            "private void OpenApplicationInfoFlyout("
        );

        Assert.Contains("projectState.IsMultiProjectEnabled", method);
        Assert.Contains("projectState.ActiveProject?.Name", method);
        Assert.Contains("titleState.UnnamedProjectPlaceholder", method);
        Assert.Contains("titleState.ApplicationTitle", method);
    }

    [Fact]
    public void LogoInformationBody_UsesTheApplicationInfoShellRegion()
    {
        var document = XDocument.Load(ShellXamlPath);
        var bodyScroller = FindNamedElement(document, "ApplicationInfoBodyScrollViewer");
        var routing = GetMethod(
            "private void SetRegionContent(",
            "private static void SetPanelContent("
        );

        Assert.Contains("case FlourishRegion.TitlebarApplicationInfo:", routing);
        Assert.Contains("SetPanelContent(ApplicationInfoBodyHost, elements);", routing);
        Assert.Equal("Auto", (string?)bodyScroller.Attribute("VerticalScrollBarVisibility"));
        Assert.Equal(
            "Disabled",
            (string?)bodyScroller.Attribute("HorizontalScrollBarVisibility")
        );
        Assert.Contains("CloseTitleBarFlyout();", ShellCode);
        Assert.Contains("TitleBarFlyoutOverlay.Visibility == Visibility.Visible", ShellCode);
    }

    [Fact]
    public void ProjectChanges_RenderTheLatestSnapshotInsteadOfAnOutOfOrderEventSnapshot()
    {
        var method = GetMethod(
            "private void ProjectService_Changed(",
            "private void ApplyTitleBarState("
        );

        Assert.Contains("var projectState = projectService.Current;", method);
        Assert.DoesNotContain("e.Current.IsMultiProjectEnabled", method, StringComparison.Ordinal);
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

    private static string GetMethod(string startMarker, string endMarker)
    {
        var start = ShellCode.IndexOf(startMarker, StringComparison.Ordinal);
        var end = ShellCode.IndexOf(endMarker, start + startMarker.Length, StringComparison.Ordinal);
        Assert.True(start >= 0, $"Could not find source marker: {startMarker}");
        Assert.True(end > start, $"Could not find source marker: {endMarker}");
        return ShellCode[start..end];
    }

    private static string FindRepositoryRoot()
    {
        for (
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            directory is not null;
            directory = directory.Parent
        )
        {
            if (File.Exists(Path.Combine(directory.FullName, "Flourish.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException("Could not locate the Flourish repository root.");
    }
}
