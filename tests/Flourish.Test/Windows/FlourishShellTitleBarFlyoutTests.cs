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
    public void BrandIdentity_UsesALogoButtonAndDirectTitleComboBoxWithoutSubtitleText()
    {
        var document = XDocument.Load(TitleBarXamlPath);
        var logo = FindNamedElement(document, "LogoButton");
        var title = FindNamedElement(document, "TitleComboBox");

        Assert.Equal("Button", logo.Name.LocalName);
        Assert.Equal("FlourishComboBox", title.Name.LocalName);
        Assert.Equal("Text", (string?)logo.Attribute("Variant"));
        Assert.Equal("0,0,2,0", (string?)logo.Attribute("Margin"));
        Assert.Equal("2,0,0,0", (string?)title.Attribute("Margin"));
        Assert.Equal(
            "{DynamicResource FlourishFontSizeLarge}",
            (string?)title.Attribute("FontSize")
        );
        Assert.Equal(
            "TitleComboBox_SelectionChanged",
            (string?)title.Attribute("SelectionChanged")
        );
        Assert.Contains(
            "Titlebar.TitleSelectionChanged += ProjectComboBox_SelectionChanged;",
            ShellCode
        );
        Assert.DoesNotContain(
            document.Descendants(),
            element => (string?)element.Attribute(XName.Get("Name", XamlNamespace)) == "SubtitleText"
        );
    }

    [Fact]
    public void BrandLogos_PreserveTransparentArtworkWithoutCroppingOrTint()
    {
        var titleDocument = XDocument.Load(TitleBarXamlPath);
        var shellDocument = XDocument.Load(ShellXamlPath);
        var titleLogo = FindNamedElement(titleDocument, "LogoImage");
        var overlayLogo = FindNamedElement(shellDocument, "ApplicationInfoLogoImage");

        Assert.Equal("Uniform", (string?)titleLogo.Attribute("Stretch"));
        Assert.Equal("Uniform", (string?)overlayLogo.Attribute("Stretch"));
        Assert.Null(
            titleLogo
                .Ancestors()
                .First(element => element.Name.LocalName == "Border")
                .Attribute("Background")
        );
        Assert.Null(
            overlayLogo
                .Ancestors()
                .First(element => element.Name.LocalName == "Border")
                .Attribute("Background")
        );
    }

    [Fact]
    public void ProjectSurface_IsTheLargeTitleComboBoxAndDoesNotUseAnIndependentView()
    {
        var titleDocument = XDocument.Load(TitleBarXamlPath);
        var shellDocument = XDocument.Load(ShellXamlPath);
        var selector = FindNamedElement(titleDocument, "TitleComboBox");

        Assert.Equal("FlourishComboBox", selector.Name.LocalName);
        Assert.Equal(
            "{DynamicResource FlourishFontSizeLarge}",
            (string?)selector.Attribute("FontSize")
        );
        Assert.DoesNotContain(
            shellDocument.Descendants(),
            element =>
                (string?)element.Attribute(XName.Get("Name", XamlNamespace))
                == "ProjectMenuContent"
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
    public void ProjectSelector_RoutesLifecycleOperationsThroughReplaceableBehavior()
    {
        var itemFactory = GetMethod(
            "private FlourishComboBoxItem CreateProjectComboBoxItem(",
            "private FlourishComboBoxItem CreateProjectPlaceholderComboBoxItem("
        );
        var selection = GetMethod(
            "private async void ProjectComboBox_SelectionChanged(",
            "private async void ProjectDeleteMenuItem_Click("
        );
        var deletion = GetMethod(
            "private async void ProjectDeleteMenuItem_Click(",
            "private async Task<bool> ExecuteProjectBehaviorAsync("
        );

        Assert.Contains("projectBehavior.ActivateProjectAsync(projectId, token)", selection);
        Assert.Contains("projectBehavior.CreateProjectAsync", selection);
        Assert.Contains("projectBehavior.DeleteProjectAsync(projectId, token)", deletion);
        Assert.Contains("new System.Windows.Controls.ContextMenu", itemFactory);
        Assert.Contains("FlourishLocaleKeys.ProjectDelete", itemFactory);
        Assert.Contains("suppressProjectSelectionChanged", selection);
        Assert.Contains("ProjectComboBox.IsDropDownOpen = false;", selection);
        Assert.DoesNotContain("BuildTitleSelectorItems();", selection, StringComparison.Ordinal);
        Assert.Contains("ProjectService_Changed", ShellCode);
        Assert.Contains("FlourishFontSizeStandard", ShellCode);
        Assert.DoesNotContain("SetActiveProject", selection, StringComparison.Ordinal);
        Assert.DoesNotContain("RemoveProject", deletion, StringComparison.Ordinal);
    }

    [Fact]
    public void ProjectSelector_UsesApplicationOnlyOrAllProjectsWithNewProjectAction()
    {
        var build = GetMethod(
            "private void BuildTitleSelectorItems(",
            "private static FlourishComboBoxItem CreateApplicationTitleComboBoxItem("
        );

        Assert.Contains("projectState.IsMultiProjectEnabled", build);
        Assert.Contains("foreach (var project in projectState.Projects)", build);
        Assert.Contains("CreateNewProjectComboBoxItem()", build);
        Assert.Contains("CreateApplicationTitleComboBoxItem(titleState.ApplicationTitle)", build);
        Assert.Contains("projectSelectorItemsById", build);
        Assert.Contains("SynchronizeItems(ProjectComboBox, desiredItems);", build);
        Assert.DoesNotContain("ProjectComboBox.Items.Clear();", build, StringComparison.Ordinal);
    }

    [Fact]
    public void ProjectSaveShortcutAndCloseGuard_UseTheReplaceableBehavior()
    {
        Assert.Contains("new KeyGesture(Key.S, ModifierKeys.Control)", ShellCode);
        Assert.Contains("ConflictPolicy = ShortcutConflictPolicy.Append", ShellCode);
        Assert.Contains("Priority = BuiltInProjectBehaviorPriority", ShellCode);
        Assert.Contains("AllowWhenTextInputFocused = true", ShellCode);
        Assert.Contains("projectBehavior.SaveActiveProjectAsync", ShellCode);
        Assert.Contains("projectBehavior.CanCloseAsync", ShellCode);
        Assert.Contains("!projectService.Current.IsMultiProjectEnabled", ShellCode);
        Assert.Contains("context.Reason != WindowCloseRequestReason.Tray", ShellCode);
        Assert.Contains("windowCloseService.Behavior == WindowCloseBehavior.MinimizeToTray", ShellCode);
    }

    [Fact]
    public void ClosePrompt_ReplacesTheStandardConfirmationWhenBackgroundTasksAreActive()
    {
        var backgroundTaskPrompt = GetMethod(
            "private async Task<bool> ConfirmBackgroundTasksCloseRequestAsync(",
            "private void CancelActiveBackgroundTasks()"
        );
        var cancellation = GetMethod(
            "private void CancelActiveBackgroundTasks()",
            "private async ValueTask<bool> RequestCloseCoreAsync("
        );
        var closeRequest = GetMethod(
            "private async ValueTask<bool> RequestCloseCoreAsync(",
            "private void ShellWindow_Closing("
        );

        Assert.Contains("WindowBackgroundTasksClosePrompt", backgroundTaskPrompt);
        Assert.Contains("WindowBackgroundTasksCloseTitle", backgroundTaskPrompt);
        Assert.Contains("WindowBackgroundTasksKeepRunning", backgroundTaskPrompt);
        Assert.Contains("WindowBackgroundTasksStopAndExit", backgroundTaskPrompt);
        Assert.Contains("MessageBoxImage.Warning", backgroundTaskPrompt);
        Assert.Contains("backgroundTaskService.ActiveTasks", cancellation);
        Assert.Contains("backgroundTaskService.CancelTask(task.Id);", cancellation);
        Assert.Contains("var activeTaskCount = backgroundTaskService.ActiveTasks.Count;", closeRequest);
        Assert.Contains("if (activeTaskCount > 0)", closeRequest);
        Assert.Contains("ConfirmBackgroundTasksCloseRequestAsync", closeRequest);
        Assert.Contains("CancelActiveBackgroundTasks();", closeRequest);
        Assert.Contains("else if (!await ConfirmCloseRequestAsync", closeRequest);
    }

    [Fact]
    public void DisplayedTitle_UsesProjectOrPlaceholderOnlyInMultiProjectMode()
    {
        var method = GetMethod(
            "private string GetDisplayedTitle()",
            "private void OpenApplicationInfoFlyout("
        );

        Assert.Contains("projectState.IsMultiProjectEnabled", method);
        Assert.Contains("GetProjectDisplayTitle(activeProject, titleState)", method);
        Assert.Contains("titleState.UnnamedProjectPlaceholder", method);
        Assert.Contains("titleState.ApplicationTitle", method);
    }

    [Fact]
    public void LogoInformation_ExposesProjectTitleOnlyInMultiProjectMode()
    {
        var method = GetMethod(
            "private void BuildApplicationInfoFlyoutContent()",
            "private void BuildTitleSelectorItems("
        );

        Assert.Contains("projectState.IsMultiProjectEnabled", method);
        Assert.Contains("projectState.ActiveProject is { } activeProject", method);
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
    public void ProjectChanges_UseVersionedEventSnapshotsAndRejectOutOfOrderUpdates()
    {
        var method = GetMethod(
            "private void ProjectService_Changed(",
            "private void ApplyTitleBarState("
        );

        Assert.Contains("var projectState = e.Current;", method);
        Assert.Contains("projectState.Version <= appliedProjectVersion", method);
        Assert.Contains("RefreshTitleSelector(titleState, projectState);", method);
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
