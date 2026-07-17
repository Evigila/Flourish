using System.IO;

namespace ArkheideSystem.Flourish.Test.Windows;

public sealed class GalleryProjectRuntimePageTests
{
    private static readonly string RepositoryRoot = FindRepositoryRoot();
    private static readonly string ProjectPageXaml = File.ReadAllText(
        Path.Combine(RepositoryRoot, "src", "Gallery", "Views", "ProjectRuntimePage.xaml")
    );
    private static readonly string ProjectPageCode = File.ReadAllText(
        Path.Combine(RepositoryRoot, "src", "Gallery", "Views", "ProjectRuntimePage.xaml.cs")
    );

    [Fact]
    public void MultiProjectOperations_AreGroupedSeparatelyFromTheModeToggle()
    {
        Assert.Contains("x:Name=\"ProjectCollectionControls\"", ProjectPageXaml);
        Assert.Contains("x:Name=\"ActiveProjectControls\"", ProjectPageXaml);
        Assert.Contains("x:Name=\"MultiProjectBehaviorControls\"", ProjectPageXaml);
        Assert.Contains("x:Name=\"MultiProjectEnabledBox\"", ProjectPageXaml);

        var toggleIndex = ProjectPageXaml.IndexOf(
            "x:Name=\"MultiProjectEnabledBox\"",
            StringComparison.Ordinal
        );
        var behaviorControlsIndex = ProjectPageXaml.IndexOf(
            "x:Name=\"MultiProjectBehaviorControls\"",
            StringComparison.Ordinal
        );

        Assert.True(toggleIndex >= 0);
        Assert.True(behaviorControlsIndex > toggleIndex);
    }

    [Fact]
    public void RefreshState_GatesEveryMultiProjectOperationGroupFromRuntimeState()
    {
        Assert.Contains(
            "ProjectCollectionControls.IsEnabled = current.IsMultiProjectEnabled;",
            ProjectPageCode
        );
        Assert.Contains(
            "ActiveProjectControls.IsEnabled = current.IsMultiProjectEnabled;",
            ProjectPageCode
        );
        Assert.Contains(
            "MultiProjectBehaviorControls.IsEnabled = current.IsMultiProjectEnabled;",
            ProjectPageCode
        );
        Assert.DoesNotContain(
            "MultiProjectEnabledBox.IsEnabled = current.IsMultiProjectEnabled;",
            ProjectPageCode
        );
    }

    [Fact]
    public void ModeToggle_RefreshesAvailabilityImmediatelyAfterUpdatingTheService()
    {
        var handlerStart = ProjectPageCode.IndexOf(
            "private void MultiProjectEnabledBox_Changed",
            StringComparison.Ordinal
        );
        Assert.True(handlerStart >= 0);
        var nextHandler = ProjectPageCode.IndexOf(
            "private void UnnamedProjectPlaceholderBox_LostFocus",
            handlerStart,
            StringComparison.Ordinal
        );
        Assert.True(nextHandler > handlerStart);
        var handler = ProjectPageCode[handlerStart..nextHandler];

        Assert.Contains("projects.SetMultiProjectEnabled", handler);
        Assert.Contains("RefreshState();", handler);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (
                Directory.Exists(Path.Combine(directory.FullName, "src", "Flourish"))
                && Directory.Exists(Path.Combine(directory.FullName, "src", "Gallery"))
            )
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the repository root.");
    }
}
