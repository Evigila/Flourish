using System.IO;
using System.Windows.Controls;
using ArkheideSystem.Flourish.Internal.Configuration;
using ArkheideSystem.Flourish.Services;

namespace ArkheideSystem.Flourish.Test.Windows;

public sealed class FlourishShellProfileFlyoutTests
{
    private static readonly string ShellSource = File.ReadAllText(
        Path.Combine(
            FindRepositoryRoot(),
            "src",
            "Flourish",
            "Views",
            "Windows",
            "FlourishShellWindow.xaml.cs"
        )
    );

    [Fact]
    public void ConfigureProfileSurface_KeepsProfileChromeReadyWithoutMaterializingContent()
    {
        var method = GetSourceSection(
            "private void ConfigureProfileSurface()",
            "private void EnsureProfileContent("
        );

        Assert.Contains("Titlebar.SetProfile(profileService.CurrentProfile);", method);
        Assert.Contains(
            "profileService.ProfileChanged += ProfileService_ProfileChanged;",
            method
        );
        Assert.Contains(
            "profileService.ProfileChanged -= ProfileService_ProfileChanged;",
            method
        );
        Assert.Contains("isProfileServiceSubscribed = false;", method);
        Assert.Contains("ApplyProfileFlyoutState(state);", method);
        Assert.DoesNotContain("GetServiceOrCreateInstance", method, StringComparison.Ordinal);
        Assert.DoesNotContain("ProfileFrame.Navigate", method, StringComparison.Ordinal);
    }

    [Fact]
    public void EnsureProfileContent_TracksTheConfiguredTypeOnlyAfterSuccessfulNavigation()
    {
        var method = GetSourceSection(
            "private void EnsureProfileContent(",
            "private async void ShellWindow_Loaded("
        );
        var factoryIndex = method.IndexOf(
            "ActivatorUtilities.GetServiceOrCreateInstance",
            StringComparison.Ordinal
        );
        var navigationIndex = method.IndexOf(
            "ProfileFrame.Navigate(page)",
            StringComparison.Ordinal
        );
        var assignmentIndex = method.LastIndexOf(
            "materializedProfileConfiguredPageType = state.ContentPageType;",
            StringComparison.Ordinal
        );

        Assert.Contains(
            "materializedProfileConfiguredPageType == state.ContentPageType",
            method
        );
        Assert.Contains("ProfileFrame.Content is WpfPage", method);
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
            assignmentIndex > navigationIndex,
            "The configured type must only be committed after navigation succeeds."
        );
        Assert.DoesNotContain(
            "ProfileFrame.Content?.GetType()",
            method,
            StringComparison.Ordinal
        );
    }

    [Fact]
    public void ApplyProfileFlyoutState_MaterializesOnlyVisibleContentAndRollsBackFailures()
    {
        var method = GetSourceSection(
            "private void ApplyProfileFlyoutState(",
            "private void ApplyWindowOptions()"
        );
        var hiddenGuardIndex = method.IndexOf("if (!state.IsVisible)", StringComparison.Ordinal);
        var ensureIndex = method.IndexOf("EnsureProfileContent(state);", StringComparison.Ordinal);
        var visibleIndex = method.IndexOf(
            "ProfileOverlay.Visibility = Visibility.Visible;",
            StringComparison.Ordinal
        );

        Assert.StartsWith(
            "private void ApplyProfileFlyoutState(",
            method.TrimStart(),
            StringComparison.Ordinal
        );
        Assert.Contains("ProfileOverlay.Visibility = Visibility.Collapsed;", method);
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
            "profileFlyoutService.SynchronizeVisibility(false);",
            method
        );
        Assert.Contains("flourish.profile.content.error", method);
        Assert.DoesNotContain("throw;", method, StringComparison.Ordinal);
    }

    [Fact]
    public void ProfileFlyoutRuntimeChanges_UseTheSingleLazyConfigurationPath()
    {
        var method = GetSourceSection(
            "private void ProfileFlyoutService_Changed(",
            "private void ShellFeatureService_Changed("
        );

        Assert.Contains("DispatchRuntimeChange(ConfigureProfileSurface);", method);
        Assert.DoesNotContain("ApplyProfileFlyoutState", method, StringComparison.Ordinal);
        Assert.Equal(1, CountOccurrences(ShellSource, "EnsureProfileContent(state)"));
    }

    [Fact]
    public void RuntimeDispatch_RechecksWindowLifetimeWhenQueuedWorkActuallyExecutes()
    {
        var method = GetSourceSection(
            "private void DispatchRuntimeChange(Action action)",
            "private void ClearToolbarButtonCache()"
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
            "private void TitleBarService_Changed(",
            "private void ApplyTitleBarState("
        );

        Assert.Contains("current.IsProfileVisible", method);
        Assert.Contains(
            "shouldSubscribeToProfile != isProfileServiceSubscribed",
            method
        );
        Assert.Contains("ConfigureProfileSurface();", method);
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

    private static string GetSourceSection(string startMarker, string endMarker)
    {
        var start = ShellSource.IndexOf(startMarker, StringComparison.Ordinal);
        var end = ShellSource.IndexOf(
            endMarker,
            start + startMarker.Length,
            StringComparison.Ordinal
        );

        Assert.True(start >= 0, $"Could not find source marker: {startMarker}");
        Assert.True(end > start, $"Could not find source marker: {endMarker}");
        return ShellSource[start..end];
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

    private sealed class FirstProfilePage : Page;

    private sealed class SecondProfilePage : Page;
}
