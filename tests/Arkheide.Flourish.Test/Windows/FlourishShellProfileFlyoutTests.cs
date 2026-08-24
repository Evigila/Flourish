using System.IO;
using System.Windows.Controls;
using ArkheideSystem.Flourish.Internal.Configuration;
using ArkheideSystem.Flourish.Services;

namespace ArkheideSystem.Flourish.Test.Windows;

public sealed class FlourishShellProfileFlyoutTests
{
    private static readonly string ShellSource = File.ReadAllText(
        Path.Combine(
            TestPaths.RepositoryRoot,
            "src",
            "Flourish",
            "Views",
            "Windows",
            "FlourishShellWindow.xaml.cs"
        )
    );
    private static readonly string ProfileOverlaySource = File.ReadAllText(
        Path.Combine(
            TestPaths.RepositoryRoot,
            "src",
            "Flourish",
            "Views",
            "Windows",
            "ProfileOverlay.xaml.cs"
        )
    );
    private static readonly string ProfileControllerSource = File.ReadAllText(
        Path.Combine(
            TestPaths.RepositoryRoot,
            "src",
            "Flourish",
            "Views",
            "Windows",
            "ShellProfileController.cs"
        )
    );

    [Fact]
    public void ConfigureSurface_KeepsProfileChromeReadyWithoutMaterializingContent()
    {
        var method = GetSourceSection(
            ProfileControllerSource,
            "private void ConfigureSurface(",
            "private void EnsureProfileContent("
        );

        Assert.Contains("titlebar.SetProfile(profileService.CurrentProfile);", method);
        Assert.Contains(
            "profileService.ProfileChanged += ProfileService_ProfileChanged;",
            method
        );
        Assert.Contains(
            "profileService.ProfileChanged -= ProfileService_ProfileChanged;",
            method
        );
        Assert.Contains("isProfileServiceSubscribed = enabled;", method);
        Assert.Contains("ApplyFlyoutState(current, isAvailable);", method);
        Assert.DoesNotContain("GetServiceOrCreateInstance", method, StringComparison.Ordinal);
        Assert.DoesNotContain("overlay.SetContent", method, StringComparison.Ordinal);
    }

    [Fact]
    public void EnsureProfileContent_TracksTheConfiguredTypeOnlyAfterSuccessfulNavigation()
    {
        var method = GetSourceSection(
            ProfileControllerSource,
            "private void EnsureProfileContent(",
            "private void ApplyFlyoutState("
        );
        var factoryIndex = method.IndexOf(
            "ActivatorUtilities.GetServiceOrCreateInstance",
            StringComparison.Ordinal
        );
        var navigationIndex = method.IndexOf(
            "overlay.SetContent(page, state.ContentPageType)",
            StringComparison.Ordinal
        );
        var componentNavigationIndex = ProfileOverlaySource.IndexOf(
            "ProfileFrame.Navigate(page)",
            StringComparison.Ordinal
        );
        var assignmentIndex = ProfileOverlaySource.LastIndexOf(
            "materializedContentType = contentType;",
            StringComparison.Ordinal
        );

        Assert.Contains(
            "overlay.HasMaterializedContent(state.ContentPageType)",
            method
        );
        Assert.Contains(
            "fontService.ApplyToPage(page, state.ContentPageType);",
            method
        );
        Assert.True(factoryIndex >= 0, "The profile page is not resolved lazily.");
        Assert.True(
            navigationIndex > factoryIndex,
            "The profile page must be created before it is navigated."
        );
        Assert.True(
            assignmentIndex > componentNavigationIndex,
            "The configured type must only be committed after navigation succeeds."
        );
        Assert.DoesNotContain(
            "ProfileFrame.Content?.GetType()",
            method,
            StringComparison.Ordinal
        );
    }

    [Fact]
    public void ApplyFlyoutState_MaterializesOnlyVisibleContentAndRollsBackFailures()
    {
        var method = GetSourceSection(
            ProfileControllerSource,
            "private void ApplyFlyoutState(",
            "private void ProfileService_ProfileChanged("
        );
        var hiddenGuardIndex = method.IndexOf("if (!state.IsVisible)", StringComparison.Ordinal);
        var ensureIndex = method.IndexOf("EnsureProfileContent(state);", StringComparison.Ordinal);
        var visibleIndex = method.IndexOf(
            "overlay.Open();",
            StringComparison.Ordinal
        );

        Assert.StartsWith(
            "private void ApplyFlyoutState(",
            method.TrimStart(),
            StringComparison.Ordinal
        );
        Assert.Contains("overlay.Close();", method);
        Assert.True(hiddenGuardIndex >= 0, "The hidden state is not handled explicitly.");
        Assert.True(
            ensureIndex > hiddenGuardIndex,
            "Hidden flyouts must return before profile content is materialized."
        );
        Assert.True(
            visibleIndex > ensureIndex,
            "The overlay must only become visible after content navigation succeeds."
        );
        Assert.Contains("catch", method, StringComparison.Ordinal);
        Assert.Contains(
            "flyoutService.SynchronizeVisibility(false);",
            method
        );
        Assert.Contains("flourish.profile.content.error", method);
        Assert.DoesNotContain("throw;", method, StringComparison.Ordinal);
    }

    [Fact]
    public void ProfileFlyoutRuntimeChanges_UseTheSingleLazyConfigurationPath()
    {
        var method = GetSourceSection(
            ProfileControllerSource,
            "private void FlyoutService_Changed(",
            "private void TitleBarService_Changed("
        );

        Assert.Contains("DispatchIfActive(() => ConfigureSurface(e.State));", method);
        Assert.DoesNotContain("ApplyFlyoutState", method, StringComparison.Ordinal);
        Assert.Equal(1, CountOccurrences(ProfileControllerSource, "EnsureProfileContent(state)"));
    }

    [Fact]
    public void RuntimeDispatch_RechecksWindowLifetimeWhenQueuedWorkActuallyExecutes()
    {
        var method = GetSourceSection(
            ShellSource,
            "private void DispatchRuntimeChange(Action action)",
            "private void UpdateRuntimeSurfaceVisibility()"
        );

        Assert.Contains("void ExecuteIfActive()", method);
        Assert.Contains("new Action(ExecuteIfActive)", method);
        Assert.Contains("Dispatcher.HasShutdownFinished", method);
        Assert.True(CountOccurrences(method, "isShellClosed") >= 2);
    }

    [Fact]
    public void TitleBarProfileVisibility_ReconfiguresTheProfileSubscription()
    {
        var method = GetSourceSection(
            ProfileControllerSource,
            "private void TitleBarService_Changed(",
            "private void FontService_Changed("
        );

        Assert.Contains("DispatchIfActive(() => ConfigureSurface());", method);
        Assert.Contains("IsProfileVisible: true", ProfileControllerSource);
        Assert.Contains("SetProfileSubscription(isAvailable);", ProfileControllerSource);
    }

    [Fact]
    public void ShellWindow_DelegatesProfileOwnershipToTheController()
    {
        Assert.Contains("new ShellProfileController(", ShellSource);
        Assert.Contains("profileController.InitializeAsync()", ShellSource);
        Assert.Contains("profileController.Hide();", ShellSource);
        Assert.Contains("profileController.Dispose();", ShellSource);
        Assert.DoesNotContain("ConfigureProfileSurface", ShellSource, StringComparison.Ordinal);
        Assert.DoesNotContain("ProfileFlyoutService_Changed", ShellSource, StringComparison.Ordinal);
    }

    [Fact]
    public void ProfileFlyoutService_PageChangesRemainHiddenUntilExplicitlyShown()
    {
        var shellOptions = new FlourishShellOptions { IsProfileEnabled = true };
        var profileOptions = new FlourishProfileOptions
        {
            PageType = typeof(FirstProfilePage),
        };
        var sut = new ProfileFlyoutService(shellOptions, profileOptions);

        sut.Show();
        sut.Hide();
        sut.SetContentPage<SecondProfilePage>();

        Assert.False(sut.Current.IsVisible);
        Assert.Equal(typeof(SecondProfilePage), sut.Current.ContentPageType);

        sut.SetEnabled(false);
        sut.SetEnabled(true);

        Assert.False(sut.Current.IsVisible);
        Assert.Equal(typeof(SecondProfilePage), sut.Current.ContentPageType);
    }

    private static string GetSourceSection(
        string source,
        string startMarker,
        string endMarker
    )
    {
        var start = source.IndexOf(startMarker, StringComparison.Ordinal);
        var end = source.IndexOf(
            endMarker,
            start + startMarker.Length,
            StringComparison.Ordinal
        );

        Assert.True(start >= 0, $"Could not find source marker: {startMarker}");
        Assert.True(end > start, $"Could not find source marker: {endMarker}");
        return source[start..end];
    }

    private static int CountOccurrences(string source, string value)
    {
        var count = 0;
        var offset = 0;
        while ((offset = source.IndexOf(value, offset, StringComparison.Ordinal)) >= 0)
        {
            count++;
            offset += value.Length;
        }

        return count;
    }

    private sealed class FirstProfilePage : Page;

    private sealed class SecondProfilePage : Page;
}
