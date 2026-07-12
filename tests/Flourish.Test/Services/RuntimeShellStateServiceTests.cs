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
    public void TitleBarService_UpdatesContentVisibilityAndRaisesOnlyMaterialChanges()
    {
        var options = new FlourishShellOptions();
        ITitleBarService sut = new TitleBarService(options);
        var changes = new List<FlourishTitleBarState>();
        sut.Changed += (_, args) => changes.Add(args.State);

        sut.SetIdentity("Runtime Gallery", "Live APIs");
        sut.SetLogo(null, "RG");
        sut.SetElementVisible(TitleBarElement.Search, true);
        sut.SetBreadcrumbMode(BreadcrumbShowOption.Hidden);
        sut.SetBreadcrumbMode(BreadcrumbShowOption.Hidden);

        Assert.Equal("Runtime Gallery", sut.Current.Title);
        Assert.Equal("Live APIs", sut.Current.Subtitle);
        Assert.Equal("RG", sut.Current.LogoFallbackText);
        Assert.True(sut.Current.IsSearchVisible);
        Assert.False(sut.Current.IsBreadcrumbVisible);
        Assert.Equal(4, changes.Count);
        Assert.Throws<ArgumentException>(() => sut.SetTitle("  "));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            sut.SetElementVisible((TitleBarElement)int.MaxValue, true)
        );
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
        IShellFeatureService sut = new ShellFeatureService(options);
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
        IShellFeatureService sut = new ShellFeatureService(options);
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

    private sealed class TestProfilePage : Page;

    private abstract class AbstractProfilePage : Page;
}
