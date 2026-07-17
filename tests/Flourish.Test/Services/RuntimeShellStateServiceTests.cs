using System.Windows.Controls;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;
using ArkheideSystem.Flourish.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace ArkheideSystem.Flourish.Test.Services;

public sealed class RuntimeShellStateServiceTests
{
    [Fact]
    public void TitleBarService_UpdatesApplicationIdentityLogoDetailsAndRaisesOnlyMaterialChanges()
    {
        var options = new FlourishShellOptions();
        ITitleBarService sut = new TitleBarService(options);
        var changes = new List<FlourishTitleBarState>();
        sut.Changed += (_, args) => changes.Add(args.State);

        sut.SetApplicationIdentity("Runtime Gallery", "Live APIs");
        sut.SetApplicationIdentity("Runtime Gallery", "Live APIs");
        sut.SetUnnamedProjectPlaceholder("Untitled workspace");
        sut.SetLogo(
            null,
            "RG",
            showApplicationTitle: false,
            showApplicationSubTitle: false,
            showProjectTitle: true
        );
        sut.SetLogo(
            null,
            "RG",
            showApplicationTitle: false,
            showApplicationSubTitle: false,
            showProjectTitle: true
        );
        sut.SetElementVisible(TitleBarElement.Search, true);
        sut.SetBreadcrumbMode(BreadcrumbShowOption.Hidden);
        sut.SetBreadcrumbMode(BreadcrumbShowOption.Hidden);

        Assert.Equal("Runtime Gallery", sut.Current.ApplicationTitle);
        Assert.Equal("Live APIs", sut.Current.ApplicationSubTitle);
        Assert.Equal("Untitled workspace", sut.Current.UnnamedProjectPlaceholder);
        Assert.Equal("RG", sut.Current.LogoFallbackText);
        Assert.False(sut.Current.ShowApplicationTitle);
        Assert.False(sut.Current.ShowApplicationSubTitle);
        Assert.True(sut.Current.ShowProjectTitle);
        Assert.True(sut.Current.IsLogoVisible);
        Assert.True(sut.Current.IsTitleVisible);
        Assert.True(sut.Current.IsSearchVisible);
        Assert.False(sut.Current.IsBreadcrumbVisible);
        Assert.Equal(5, changes.Count);
        Assert.Throws<ArgumentException>(() => sut.SetApplicationTitle("  "));
        Assert.Throws<ArgumentException>(() =>
            sut.SetUnnamedProjectPlaceholder("  ")
        );
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            sut.SetElementVisible((TitleBarElement)int.MaxValue, true)
        );
    }

    [Fact]
    public void TitleBarService_MultiProjectModeMakesTheTitleButtonVisible()
    {
        var options = new FlourishShellOptions { IsMultiProjectEnabled = true };
        ITitleBarService sut = new TitleBarService(options);

        Assert.True(sut.Current.IsTitleVisible);
        Assert.Equal("Unnamed project", sut.Current.UnnamedProjectPlaceholder);
    }

    [Fact]
    public async Task TitleBarSearchService_PublishFromViewUpdatesStateAndCancelsStaleQuery()
    {
        var sut = new TitleBarSearchService(
            new FlourishShellOptions(),
            new Mock<IServiceProvider>().Object,
            NullLogger<TitleBarSearchService>.Instance
        );
        var states = new List<FlourishTitleBarSearchState>();
        sut.StateChanged += (_, args) =>
        {
            states.Add(args.State);
            Assert.Equal(args.State, sut.Current);
        };
        var firstStarted = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        var firstCanceled = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        var secondHandled = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        using var waiting = sut.Subscribe(async (args, token) =>
        {
            if (args.Text == "first")
            {
                firstStarted.SetResult();
                try
                {
                    await Task.Delay(Timeout.InfiniteTimeSpan, token);
                }
                catch (OperationCanceledException)
                {
                    firstCanceled.SetResult();
                    throw;
                }
            }
        });
        using var failing = sut.Subscribe((_, _) => throw new InvalidOperationException("boom"));
        using var succeeding = sut.Subscribe((args, _) =>
        {
            if (args.Text == "second")
            {
                secondHandled.SetResult();
            }

            return ValueTask.CompletedTask;
        });

        sut.PublishFromView("first");
        await firstStarted.Task.WaitAsync(TimeSpan.FromSeconds(2));
        sut.PublishFromView("second");

        await firstCanceled.Task.WaitAsync(TimeSpan.FromSeconds(2));
        await secondHandled.Task.WaitAsync(TimeSpan.FromSeconds(2));
        Assert.Equal("second", sut.Current.Text);
        Assert.Equal(2, sut.Current.Version);
        Assert.Equal(["first", "second"], states.Select(state => state.Text));
        Assert.Equal([1L, 2L], states.Select(state => state.Version));
    }

    [Fact]
    public void TitleBarSearchService_TracksStateAndSubscriptionLease()
    {
        using var sut = new TitleBarSearchService(
            new FlourishShellOptions(),
            new Mock<IServiceProvider>().Object,
            NullLogger<TitleBarSearchService>.Instance
        );
        var calls = 0;
        var subscription = sut.Subscribe((_, _) =>
        {
            calls++;
            return ValueTask.CompletedTask;
        });
        subscription.Dispose();

        sut.SetVisible(true);
        sut.SetPlaceholder("Find demos");
        sut.SetText("runtime");
        sut.Focus();
        sut.Focus();
        sut.PublishFromView("typed");

        Assert.Equal(0, calls);
        Assert.Equal("Find demos", sut.Current.Placeholder);
        Assert.False(sut.Current.FocusRequested);
        Assert.Throws<ArgumentException>(() => sut.SetPlaceholder(""));
    }

    [Fact]
    public void ProfileFlyoutService_ValidatesPagesAndSynchronizesVisibilityEvents()
    {
        var shellOptions = new FlourishShellOptions();
        var profileOptions = new FlourishProfileOptions();
        var sut = new ProfileFlyoutService(shellOptions, profileOptions);
        var changes = new List<FlourishProfileFlyoutState>();
        sut.Changed += (_, args) => changes.Add(args.State);

        sut.SetEnabled(true);
        sut.SetContentPage<TestProfilePage>();
        sut.Show();
        sut.SynchronizeVisibility(false);

        Assert.False(sut.Current.IsVisible);
        Assert.Equal(typeof(TestProfilePage), sut.Current.ContentPageType);
        Assert.Equal(4, changes.Count);
        Assert.Throws<ArgumentException>(() => sut.SetContentPage(typeof(string)));
        Assert.Throws<ArgumentException>(() => sut.SetContentPage(typeof(AbstractProfilePage)));
        sut.SetEnabled(false);
        Assert.Throws<InvalidOperationException>(sut.Show);
    }

    [Fact]
    public void ShellFeatureService_MapsEveryFeatureAndSuppressesNoOpEvents()
    {
        var options = new FlourishShellOptions();
        var motion = new FlourishMotionService(options);
        var toolTips = new FlourishToolTipService(options);
        IShellFeatureService sut = new ShellFeatureService(options, motion, toolTips);
        var changes = new List<FlourishShellFeatureChangedEventArgs>();
        sut.Changed += (_, args) => changes.Add(args);

        foreach (var feature in Enum.GetValues<ShellFeature>())
        {
            sut.SetEnabled(feature, true);
            sut.SetEnabled(feature, true);
            Assert.True(sut.Current.IsEnabled(feature));
        }

        Assert.Equal(Enum.GetValues<ShellFeature>().Length, changes.Count);
        Assert.Equal(changes.Count, sut.Current.Version);
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            sut.SetEnabled((ShellFeature)int.MaxValue, true)
        );
    }

    [Fact]
    public void ShellFeatureService_TitleBarAndNavigationCanBeDisabledAndRestoredRepeatedly()
    {
        var options = new FlourishShellOptions
        {
            IsTitlebarEnabled = true,
            IsNavigationPanelEnabled = true,
            IsNavigationPanelInitiallyOpen = true,
            IsMaterialEffectEnabled = true,
            MaterialEffect = MaterialEffect.Mica,
        };
        var motion = new FlourishMotionService(options);
        var toolTips = new FlourishToolTipService(options);
        IShellFeatureService sut = new ShellFeatureService(options, motion, toolTips);
        var changes = new List<FlourishShellFeatureChangedEventArgs>();
        sut.Changed += (_, args) => changes.Add(args);

        foreach (var feature in new[] { ShellFeature.TitleBar, ShellFeature.Navigation })
        {
            sut.SetEnabled(feature, false);
            sut.SetEnabled(feature, false);
            sut.SetEnabled(feature, true);
            sut.SetEnabled(feature, true);
        }

        Assert.True(sut.Current.IsTitleBarEnabled);
        Assert.True(sut.Current.IsNavigationEnabled);
        Assert.True(options.IsNavigationPanelInitiallyOpen);
        Assert.True(options.IsMaterialEffectEnabled);
        Assert.Equal(MaterialEffect.Mica, options.MaterialEffect);
        Assert.Equal(
            new[]
            {
                (ShellFeature.TitleBar, false, 1L),
                (ShellFeature.TitleBar, true, 2L),
                (ShellFeature.Navigation, false, 3L),
                (ShellFeature.Navigation, true, 4L),
            },
            changes.Select(change =>
                (change.Feature, change.State.IsEnabled(change.Feature), change.State.Version)
            )
        );
    }

    [Fact]
    public void ShellFeatureService_AndMotionServicePublishOneSynchronizedEventPerChange()
    {
        var options = new FlourishShellOptions();
        var motion = new FlourishMotionService(options);
        var toolTips = new FlourishToolTipService(options);
        IShellFeatureService sut = new ShellFeatureService(options, motion, toolTips);
        var motionChanges = new List<FlourishMotionChangedEventArgs>();
        var featureChanges = new List<FlourishShellFeatureChangedEventArgs>();
        motion.Changed += (_, args) => motionChanges.Add(args);
        sut.Changed += (_, args) => featureChanges.Add(args);

        sut.SetEnabled(ShellFeature.Motion, true);
        sut.SetEnabled(ShellFeature.Motion, true);
        motion.SetEnabled(false);
        motion.SetEnabled(false);

        Assert.False(motion.Current.IsEnabled);
        Assert.False(sut.Current.IsMotionEnabled);
        Assert.Equal(2, motionChanges.Count);
        Assert.Equal(
            new[]
            {
                (ShellFeature.Motion, true, 1L),
                (ShellFeature.Motion, false, 2L),
            },
            featureChanges.Select(change =>
                (change.Feature, change.State.IsMotionEnabled, change.State.Version)
            )
        );
    }

    [Fact]
    public void ShellFeatureService_AndToolTipServicePublishOneSynchronizedEventPerEnablementChange()
    {
        var options = new FlourishShellOptions();
        var motion = new FlourishMotionService(options);
        var toolTips = new FlourishToolTipService(options);
        IShellFeatureService sut = new ShellFeatureService(options, motion, toolTips);
        var toolTipChanges = new List<FlourishToolTipChangedEventArgs>();
        var featureChanges = new List<FlourishShellFeatureChangedEventArgs>();
        toolTips.Changed += (_, args) => toolTipChanges.Add(args);
        sut.Changed += (_, args) => featureChanges.Add(args);

        sut.SetEnabled(ShellFeature.ToolTips, true);
        sut.SetEnabled(ShellFeature.ToolTips, true);
        toolTips.SetEnabled(false);
        toolTips.SetEnabled(false);
        toolTips.Configure(420, 7);

        Assert.True(toolTips.Current.IsEnabled);
        Assert.True(sut.Current.AreToolTipsEnabled);
        Assert.Equal(3, toolTipChanges.Count);
        Assert.Equal(
            new[]
            {
                (ShellFeature.ToolTips, true, 1L),
                (ShellFeature.ToolTips, false, 2L),
                (ShellFeature.ToolTips, true, 3L),
            },
            featureChanges.Select(change =>
                (change.Feature, change.State.AreToolTipsEnabled, change.State.Version)
            )
        );
    }

    private sealed class TestProfilePage : Page;

    private abstract class AbstractProfilePage : Page;
}
