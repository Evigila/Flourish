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
        var versions = new List<long>();
        sut.Changed += (_, args) =>
        {
            changes.Add(args.State);
            versions.Add(args.Version);
        };

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
        Assert.Equal([1, 2, 3, 4, 5], versions);
        Assert.Throws<ArgumentException>(() => sut.SetApplicationTitle("  "));
        Assert.Throws<ArgumentException>(() =>
            sut.SetUnnamedProjectPlaceholder("  ")
        );
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            sut.SetElementVisible((TitleBarElement)int.MaxValue, true)
        );
    }

    [Fact]
    public void TitleBarService_SetEnabledSuppressesNoOpsAndPreservesMaterialRequest()
    {
        var options = new FlourishShellOptions
        {
            IsTitlebarEnabled = true,
            IsMaterialEffectEnabled = true,
            MaterialEffect = MaterialEffect.Mica,
        };
        ITitleBarService sut = new TitleBarService(options);
        var changes = new List<FlourishTitleBarChangedEventArgs>();
        sut.Changed += (_, args) => changes.Add(args);

        sut.SetEnabled(false);
        sut.SetEnabled(false);
        sut.SetEnabled(true);
        sut.SetEnabled(true);

        Assert.True(sut.Current.IsEnabled);
        Assert.Equal(2, changes.Count);
        Assert.Equal([1L, 2L], changes.Select(change => change.Version));
        Assert.True(options.IsMaterialEffectEnabled);
        Assert.Equal(MaterialEffect.Mica, options.MaterialEffect);
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
    public async Task TitleBarSearchService_UserQueryPublishesStateAndCancelsStaleWork()
    {
        using var sut = new TitleBarSearchService(
            new FlourishShellOptions(),
            new Mock<IServiceProvider>().Object,
            NullLogger<TitleBarSearchService>.Instance
        );
        var states = new List<FlourishTitleBarSearchState>();
        var queries = new List<FlourishTitleBarSearchChangedEventArgs>();
        sut.StateChanged += (_, args) =>
        {
            states.Add(args.State);
            Assert.Equal(args.State, sut.Current);
        };
        sut.QueryChanged += (_, args) => queries.Add(args);
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
        Assert.Equal(["first", "second"], queries.Select(query => query.Text));
        Assert.Equal([1L, 2L], queries.Select(query => query.Sequence));
    }

    [Fact]
    public void TitleBarSearchService_SynchronousQueryObserversDoNotAllocateCancellationSource()
    {
        using var sut = new TitleBarSearchService(
            new FlourishShellOptions(),
            new Mock<IServiceProvider>().Object,
            NullLogger<TitleBarSearchService>.Instance
        );
        FlourishTitleBarSearchChangedEventArgs? query = null;
        sut.QueryChanged += (_, args) => query = args;

        sut.PublishFromView("typed");

        Assert.NotNull(query);
        Assert.Equal("typed", query.Text);
        Assert.Null(GetActiveQueryDispatch(sut));
    }

    [Fact]
    public async Task TitleBarSearchService_NewQueryCancelsWorkAfterLastSubscriberLeaves()
    {
        using var sut = new TitleBarSearchService(
            new FlourishShellOptions(),
            new Mock<IServiceProvider>().Object,
            NullLogger<TitleBarSearchService>.Instance
        );
        var started = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        var canceled = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        var subscription = sut.Subscribe(async (_, token) =>
        {
            started.SetResult();
            try
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, token);
            }
            catch (OperationCanceledException)
            {
                canceled.SetResult();
                throw;
            }
        });

        sut.PublishFromView("first");
        await started.Task.WaitAsync(TimeSpan.FromSeconds(2));
        subscription.Dispose();
        sut.PublishFromView("second");

        await canceled.Task.WaitAsync(TimeSpan.FromSeconds(2));
        Assert.Equal("second", sut.Current.Text);
        Assert.Null(GetActiveQueryDispatch(sut));
    }

    [Fact]
    public async Task TitleBarSearchService_CancelCallbackFailureDoesNotBlockNewQuery()
    {
        using var sut = new TitleBarSearchService(
            new FlourishShellOptions(),
            new Mock<IServiceProvider>().Object,
            NullLogger<TitleBarSearchService>.Instance
        );
        var firstStarted = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        var secondHandled = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        using var subscription = sut.Subscribe(async (args, token) =>
        {
            if (args.Text == "second")
            {
                secondHandled.SetResult();
                return;
            }

            using var registration = token.Register(() =>
                throw new InvalidOperationException("cancel callback failed")
            );
            firstStarted.SetResult();
            await Task.Delay(Timeout.InfiniteTimeSpan, token);
        });

        sut.PublishFromView("first");
        await firstStarted.Task.WaitAsync(TimeSpan.FromSeconds(2));

        var error = Record.Exception(() => sut.PublishFromView("second"));

        Assert.Null(error);
        await secondHandled.Task.WaitAsync(TimeSpan.FromSeconds(2));
        Assert.Equal("second", sut.Current.Text);
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
        var stateChanges = 0;
        var programmaticStateChanges = 0;
        var queryChanges = 0;
        sut.StateChanged += (_, _) => stateChanges++;
        sut.ProgrammaticStateChanged += (_, _) => programmaticStateChanges++;
        sut.QueryChanged += (_, _) => queryChanges++;
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
        Assert.Equal(5, stateChanges);
        Assert.Equal(4, programmaticStateChanges);
        Assert.Equal(1, queryChanges);
        Assert.Equal("Find demos", sut.Current.Placeholder);
        Assert.False(sut.Current.FocusRequested);
        Assert.Equal(5, sut.Current.Version);
        Assert.Throws<ArgumentException>(() => sut.SetPlaceholder(""));
    }

    private static object? GetActiveQueryDispatch(
        TitleBarSearchService service
    )
    {
        var field = typeof(TitleBarSearchService).GetField(
            "activeQueryDispatch",
            System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.NonPublic
        );
        Assert.NotNull(field);
        return field.GetValue(service);
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

    private sealed class TestProfilePage : Page;

    private abstract class AbstractProfilePage : Page;
}
