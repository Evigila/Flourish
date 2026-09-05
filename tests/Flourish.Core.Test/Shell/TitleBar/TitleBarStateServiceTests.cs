using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Projects;
using ArkheideSystem.Flourish.Shell.TitleBar;

using Microsoft.Extensions.Logging.Abstractions;

namespace ArkheideSystem.Flourish.Core.Test.Shell.TitleBar;

public sealed class RuntimeShellStateServiceTests
{
    [Fact]
    public void TitleBarService_UpdatesApplicationIdentityLogoDetailsAndRaisesOnlyMaterialChanges()
    {
        var options = new TitleBarOptions();
        var sut = new TitleBarService(options, new ProjectOptions());
        var changes = new List<TitleBarState>();
        var versions = new List<long>();
        sut.Changed += (_, args) =>
        {
            changes.Add(args.Current);
            versions.Add(args.Current.Version);
        };

        sut.SetApplicationIdentity("Runtime Gallery", "Live APIs");
        sut.SetApplicationIdentity("Runtime Gallery", "Live APIs");
        sut.SetUnnamedProjectPlaceholder("Untitled workspace");
        sut.SetLogo(
            null,
            "RG",
            showApplicationTitle: false,
            showApplicationSubtitle: false,
            showProjectTitle: true
        );
        sut.SetLogo(
            null,
            "RG",
            showApplicationTitle: false,
            showApplicationSubtitle: false,
            showProjectTitle: true
        );
        sut.SetElementVisible(TitleBarElement.Search, true);
        sut.SetBreadcrumbMode(BreadcrumbShowOption.Hidden);
        sut.SetBreadcrumbMode(BreadcrumbShowOption.Hidden);

        Assert.Equal("Runtime Gallery", sut.Current.ApplicationTitle);
        Assert.Equal("Live APIs", sut.Current.ApplicationSubtitle);
        Assert.Equal("Untitled workspace", sut.Current.UnnamedProjectPlaceholder);
        Assert.Equal("RG", sut.Current.LogoFallbackText);
        Assert.False(sut.Current.ShowApplicationTitle);
        Assert.False(sut.Current.ShowApplicationSubtitle);
        Assert.True(sut.Current.ShowProjectTitle);
        Assert.True(sut.Current.IsLogoVisible);
        Assert.True(sut.Current.IsTitleVisible);
        Assert.True(sut.Current.IsSearchVisible);
        Assert.False(sut.Current.IsBreadcrumbVisible);
        Assert.Equal(5, changes.Count);
        Assert.Equal([1, 2, 3, 4, 5], versions);
        Assert.Throws<ArgumentException>(() => sut.SetApplicationTitle("  "));
        Assert.Throws<ArgumentException>(() => sut.SetUnnamedProjectPlaceholder("  "));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            sut.SetElementVisible((TitleBarElement)int.MaxValue, true)
        );
    }

    [Fact]
    public void TitleBarService_SetEnabledSuppressesNoOpsAndPreservesMaterialRequest()
    {
        var options = new TitleBarOptions { IsTitlebarEnabled = true };
        var sut = new TitleBarService(options, new ProjectOptions());
        var changes = new List<StateChangedEventArgs<TitleBarState>>();
        sut.Changed += (_, args) => changes.Add(args);

        sut.SetEnabled(false);
        sut.SetEnabled(false);
        sut.SetEnabled(true);
        sut.SetEnabled(true);

        Assert.True(sut.Current.IsEnabled);
        Assert.Equal(2, changes.Count);
        Assert.Equal([1L, 2L], changes.Select(change => change.Current.Version));
    }

    [Fact]
    public void TitleBarService_MultiProjectModeMakesTheTitleButtonVisible()
    {
        var options = new ProjectOptions { IsMultiProjectEnabled = true };
        var sut = new TitleBarService(new TitleBarOptions(), options);

        Assert.True(sut.Current.IsTitleVisible);
        Assert.Equal("Unnamed project", sut.Current.UnnamedProjectPlaceholder);
    }

    [Fact]
    public async Task TitleBarSearchService_UserQueryPublishesStateAndCancelsStaleWork()
    {
        using var sut = new TitleBarSearchService(
            new TitleBarOptions(),
            EmptyServiceProvider.Instance,
            NullLogger<TitleBarSearchService>.Instance
        );
        var states = new List<TitleBarSearchState>();
        var queries = new List<TitleBarSearchQuery>();
        sut.Changed += (_, args) =>
        {
            states.Add(args.Current);
            Assert.Equal(args.Current, sut.Current);
        };
        using var queryObserver = sut.Subscribe(
            (args, _) =>
            {
                queries.Add(args);
                return ValueTask.CompletedTask;
            }
        );
        var firstStarted = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        var firstCanceled = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        var secondHandled = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        using var waiting = sut.Subscribe(
            async (args, token) =>
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
            }
        );
        using var failing = sut.Subscribe((_, _) => throw new InvalidOperationException("boom"));
        using var succeeding = sut.Subscribe(
            (args, _) =>
            {
                if (args.Text == "second")
                {
                    secondHandled.SetResult();
                }

                return ValueTask.CompletedTask;
            }
        );

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
    public void TitleBarSearchService_PublishWithoutSubscribersDoesNotAllocateCancellationSource()
    {
        using var sut = new TitleBarSearchService(
            new TitleBarOptions(),
            EmptyServiceProvider.Instance,
            NullLogger<TitleBarSearchService>.Instance
        );
        sut.PublishFromView("typed");

        Assert.Equal("typed", sut.Current.Text);
        Assert.Null(GetActiveQueryDispatch(sut));
    }

    [Fact]
    public async Task TitleBarSearchService_NewQueryCancelsWorkAfterLastSubscriberLeaves()
    {
        using var sut = new TitleBarSearchService(
            new TitleBarOptions(),
            EmptyServiceProvider.Instance,
            NullLogger<TitleBarSearchService>.Instance
        );
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var canceled = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var subscription = sut.Subscribe(
            async (_, token) =>
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
            }
        );

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
            new TitleBarOptions(),
            EmptyServiceProvider.Instance,
            NullLogger<TitleBarSearchService>.Instance
        );
        var firstStarted = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        var secondHandled = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        using var subscription = sut.Subscribe(
            async (args, token) =>
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
            }
        );

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
            new TitleBarOptions(),
            EmptyServiceProvider.Instance,
            NullLogger<TitleBarSearchService>.Instance
        );
        var calls = 0;
        var stateChanges = 0;
        var programmaticStateChanges = 0;
        sut.Changed += (_, _) => stateChanges++;
        sut.ProgrammaticStateChanged += (_, _) => programmaticStateChanges++;
        var subscription = sut.Subscribe(
            (_, _) =>
            {
                calls++;
                return ValueTask.CompletedTask;
            }
        );
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
        Assert.Equal("Find demos", sut.Current.Placeholder);
        Assert.False(sut.Current.FocusRequested);
        Assert.Equal(5, sut.Current.Version);
        Assert.Throws<ArgumentException>(() => sut.SetPlaceholder(""));
    }

    private static object? GetActiveQueryDispatch(TitleBarSearchService service)
    {
        var field = typeof(TitleBarSearchService).GetField(
            "activeQueryDispatch",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic
        );
        Assert.NotNull(field);
        return field.GetValue(service);
    }

    [Fact]
    public void TitleBarSearchService_ProgrammaticChangesDoNotPublishQueries()
    {
        using var sut = new TitleBarSearchService(
            new TitleBarOptions(),
            EmptyServiceProvider.Instance,
            NullLogger<TitleBarSearchService>.Instance
        );
        var queryCount = 0;
        using var subscription = sut.Subscribe(
            (_, _) =>
            {
                queryCount++;
                return ValueTask.CompletedTask;
            }
        );

        sut.SetText("preset");
        sut.Clear();
        sut.Focus();

        Assert.Equal(0, queryCount);
        Assert.Equal(string.Empty, sut.Current.Text);
        Assert.True(sut.Current.FocusRequested);
        Assert.Equal(3, sut.Current.Version);

        sut.AcknowledgeFocusRequest();

        Assert.False(sut.Current.FocusRequested);
        Assert.Equal(3, sut.Current.Version);
    }

    [Fact]
    public void TitleBarSearchService_ConfiguredCallbackReceivesProviderAndViewText()
    {
        IServiceProvider? receivedProvider = null;
        string? receivedText = null;
        var options = new TitleBarOptions
        {
            TitlebarSearchTextChanged = (provider, text) =>
            {
                receivedProvider = provider;
                receivedText = text;
            },
        };
        using var sut = new TitleBarSearchService(
            options,
            EmptyServiceProvider.Instance,
            NullLogger<TitleBarSearchService>.Instance
        );

        sut.PublishFromView("typed");

        Assert.Same(EmptyServiceProvider.Instance, receivedProvider);
        Assert.Equal("typed", receivedText);
    }

    [Fact]
    public void TitleBarSearchService_DisposeRejectsProgrammaticMutationAndIgnoresViewEvents()
    {
        var sut = new TitleBarSearchService(
            new TitleBarOptions(),
            EmptyServiceProvider.Instance,
            NullLogger<TitleBarSearchService>.Instance
        );
        sut.SetText("before");

        sut.Dispose();
        sut.Dispose();
        sut.PublishFromView("ignored");

        Assert.Equal("before", sut.Current.Text);
        Assert.Throws<ObjectDisposedException>(() => sut.SetText("after"));
        Assert.Throws<ObjectDisposedException>(() => sut.Subscribe((_, _) => default));
    }

    private sealed class EmptyServiceProvider : IServiceProvider
    {
        public static EmptyServiceProvider Instance { get; } = new();

        public object? GetService(Type serviceType) => null;
    }
}
