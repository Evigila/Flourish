using System.IO;
using System.Xml.Linq;

namespace ArkheideSystem.Flourish.Test.Windows;

public sealed class FlourishShellTitleBarFlyoutTests
{
    private const string XamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";
    private static readonly string RepositoryRoot = TestPaths.RepositoryRoot;
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
    private static readonly string ApplicationInfoXamlPath = Path.Combine(
        RepositoryRoot,
        "src",
        "Flourish",
        "Views",
        "Windows",
        "ApplicationInfoOverlay.xaml"
    );
    private static readonly string ApplicationInfoCode = File.ReadAllText(
        Path.ChangeExtension(ApplicationInfoXamlPath, ".xaml.cs")
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
    private static readonly string TitleBarControllerCode = File.ReadAllText(
        Path.Combine(
            RepositoryRoot,
            "src",
            "Flourish",
            "Views",
            "Windows",
            "ShellTitleBarController.cs"
        )
    );
    private static readonly string ProjectSelectorCode = File.ReadAllText(
        Path.Combine(
            RepositoryRoot,
            "src",
            "Flourish",
            "Views",
            "Windows",
            "ProjectSelectorController.cs"
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
            "titlebar.TitleSelectionChanged += Titlebar_TitleSelectionChanged;",
            ProjectSelectorCode
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
        var applicationInfoDocument = XDocument.Load(ApplicationInfoXamlPath);
        var titleLogo = FindNamedElement(titleDocument, "LogoImage");
        var overlayLogo = FindNamedElement(
            applicationInfoDocument,
            "ApplicationInfoLogoImage"
        );

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
        var overlayDocument = XDocument.Load(ApplicationInfoXamlPath);
        var titleBar = FindNamedElement(document, "Titlebar");
        var overlay = FindNamedElement(document, "ApplicationInfoOverlay");
        var overlayCanvas = FindNamedElement(overlayDocument, "OverlayCanvas");
        var card = FindNamedElement(overlayDocument, "TitleBarFlyoutCard");

        Assert.Equal("3", GetAttribute(overlay, "Grid.RowSpan"));
        Assert.Equal("Transparent", (string?)overlayCanvas.Attribute("Background"));
        Assert.Equal(
            "Collapsed",
            (string?)overlayDocument.Root!.Attribute("Visibility")
        );
        Assert.Equal("True", (string?)overlayCanvas.Attribute("ClipToBounds"));
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
            ProjectSelectorCode,
            "private FlourishComboBoxItem CreateProjectItem(",
            "private FlourishComboBoxItem CreateProjectPlaceholderItem("
        );
        var selection = GetMethod(
            ProjectSelectorCode,
            "private async void Titlebar_TitleSelectionChanged(",
            "private async void ProjectDeleteMenuItem_Click("
        );
        var deletion = GetMethod(
            ProjectSelectorCode,
            "private async void ProjectDeleteMenuItem_Click(",
            "private async Task<bool> ExecuteBehaviorAsync("
        );

        Assert.Contains("projectBehavior.ActivateProjectAsync(projectId, token)", selection);
        Assert.Contains("projectBehavior.CreateProjectAsync", selection);
        Assert.Contains("projectBehavior.DeleteProjectAsync(projectId, token)", deletion);
        Assert.Contains("new WpfContextMenu", itemFactory);
        Assert.Contains("FlourishLocaleKeys.ProjectDelete", itemFactory);
        Assert.Contains("suppressSelectionChanged", selection);
        Assert.Contains("selector.IsDropDownOpen = false;", selection);
        Assert.DoesNotContain("BuildItems(", selection, StringComparison.Ordinal);
        Assert.Contains("ProjectService_Changed", ProjectSelectorCode);
        Assert.Contains("FlourishFontSizeStandard", ProjectSelectorCode);
        Assert.DoesNotContain("SetActiveProject", selection, StringComparison.Ordinal);
        Assert.DoesNotContain("RemoveProject", deletion, StringComparison.Ordinal);
    }

    [Fact]
    public void ProjectSelector_UsesApplicationOnlyOrAllProjectsWithNewProjectAction()
    {
        var build = GetMethod(
            ProjectSelectorCode,
            "private void BuildItems(",
            "private void UpdateProjectItem("
        );

        Assert.Contains("projectState.IsMultiProjectEnabled", build);
        Assert.Contains("foreach (var project in projectState.Projects)", build);
        Assert.Contains("CreateNewProjectItem()", build);
        Assert.Contains("CreateApplicationTitleItem()", build);
        Assert.Contains("projectItemsById", build);
        Assert.Contains("SynchronizeItems(selector, desiredItems);", build);
        Assert.DoesNotContain("selector.Items.Clear();", build, StringComparison.Ordinal);
    }

    [Fact]
    public void ProjectSaveShortcutAndCloseGuard_UseTheReplaceableBehavior()
    {
        Assert.Contains("new KeyGesture(Key.S, ModifierKeys.Control)", ShellCode);
        Assert.Contains("ConflictPolicy = ShortcutConflictPolicy.Append", ShellCode);
        Assert.Contains("Priority = BuiltInProjectBehaviorPriority", ShellCode);
        Assert.Contains("AllowWhenTextInputFocused = true", ShellCode);
        Assert.Contains("projectSelectorController.SaveAsync", ShellCode);
        Assert.Contains("projectSelectorController.CheckCloseAsync", ShellCode);
        Assert.Contains("projectBehavior.SaveActiveProjectAsync", ProjectSelectorCode);
        Assert.Contains("projectBehavior.CanCloseAsync", ProjectSelectorCode);
        Assert.Contains("!projectService.Current.IsMultiProjectEnabled", ProjectSelectorCode);
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
        Assert.Contains("statusSurfaceController.CancelActiveTasks();", cancellation);
        Assert.Contains(
            "var activeTaskCount = statusSurfaceController.ActiveTaskCount;",
            closeRequest
        );
        Assert.Contains("if (activeTaskCount > 0)", closeRequest);
        Assert.Contains("ConfirmBackgroundTasksCloseRequestAsync", closeRequest);
        Assert.Contains("CancelActiveBackgroundTasks();", closeRequest);
        Assert.Contains("else if (!await ConfirmCloseRequestAsync", closeRequest);
    }

    [Fact]
    public void DisplayedTitle_UsesProjectOrPlaceholderOnlyInMultiProjectMode()
    {
        var method = GetMethod(
            ProjectSelectorCode,
            "internal string GetDisplayedTitle(",
            "internal async ValueTask<CommandResult> SaveAsync("
        );

        Assert.Contains("projectState.IsMultiProjectEnabled", method);
        Assert.Contains("GetProjectDisplayTitle(activeProject, state)", method);
        Assert.Contains("state.UnnamedProjectPlaceholder", method);
        Assert.Contains("state.ApplicationTitle", method);
    }

    [Fact]
    public void LogoInformation_ExposesProjectTitleOnlyInMultiProjectMode()
    {
        Assert.Contains("projectState.IsMultiProjectEnabled", ApplicationInfoCode);
        Assert.Contains(
            "projectState.ActiveProject is { } activeProject",
            ApplicationInfoCode
        );
    }

    [Fact]
    public void LogoInformationBody_UsesTheApplicationInfoShellRegion()
    {
        var document = XDocument.Load(ApplicationInfoXamlPath);
        var bodyScroller = FindNamedElement(document, "ApplicationInfoBodyScrollViewer");
        var routing = GetMethod(
            "private void SetRegionContent(",
            "private void StopNavigationPaneAnimations("
        );

        Assert.Contains("case FlourishRegion.TitlebarApplicationInfo:", routing);
        Assert.Contains(
            "titleBarController.SetApplicationInfoBody(elements);",
            routing
        );
        Assert.Contains("applicationInfo.SetBody(elements);", TitleBarControllerCode);
        Assert.Equal(
            "clr-namespace:ArkheideSystem.Flourish.Controls",
            bodyScroller.Name.NamespaceName
        );
        Assert.Equal("Auto", (string?)bodyScroller.Attribute("VerticalScrollBarVisibility"));
        Assert.Equal(
            "Disabled",
            (string?)bodyScroller.Attribute("HorizontalScrollBarVisibility")
        );
        Assert.Contains("CloseApplicationInfo();", TitleBarControllerCode);
        Assert.Contains("applicationInfo.IsOpen", TitleBarControllerCode);
        var shellDocument = XDocument.Load(ShellXamlPath);
        var applicationInfo = FindNamedElement(shellDocument, "ApplicationInfoOverlay");
        Assert.Null(applicationInfo.Attribute("DismissRequested"));
        Assert.Null(applicationInfo.Attribute("PlacementInvalidated"));
    }

    [Fact]
    public void TitleBarController_OwnsVersionedStateSearchLogoAndFlyoutLifetime()
    {
        Assert.Contains("e.Version <= appliedVersion", TitleBarControllerCode);
        Assert.Contains("logoCoordinator.IsCurrent(result)", TitleBarControllerCode);
        Assert.Contains("searchService.AcknowledgeFocusRequest();", TitleBarControllerCode);
        Assert.Contains("if (!openedWithFocus)", TitleBarControllerCode);
        Assert.Contains("applicationInfo.FocusContent();", TitleBarControllerCode);
        Assert.Contains("Math.Clamp(desiredLeft", TitleBarControllerCode);
        Assert.Contains("if (isDisposed || !applicationInfo.IsOpen)", TitleBarControllerCode);
        Assert.Contains("logoCoordinator.Dispose();", TitleBarControllerCode);
    }

    [Fact]
    public void ProjectChanges_UseVersionedEventSnapshotsAndRejectOutOfOrderUpdates()
    {
        var method = GetMethod(
            ProjectSelectorCode,
            "private void ProjectService_Changed(",
            "private void LocalizationService_Changed("
        );

        Assert.Contains("e.Current.Version <= appliedProjectVersion", method);
        Assert.Contains("appliedProjectVersion = e.Current.Version;", method);
        Assert.Contains("Refresh(e.Current);", method);
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
        return GetMethod(ShellCode, startMarker, endMarker);
    }

    private static string GetMethod(
        string source,
        string startMarker,
        string endMarker
    )
    {
        var start = source.IndexOf(startMarker, StringComparison.Ordinal);
        var end = source.IndexOf(endMarker, start + startMarker.Length, StringComparison.Ordinal);
        Assert.True(start >= 0, $"Could not find source marker: {startMarker}");
        Assert.True(end > start, $"Could not find source marker: {endMarker}");
        return source[start..end];
    }
}
