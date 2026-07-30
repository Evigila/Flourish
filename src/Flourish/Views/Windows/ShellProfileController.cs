using System.Windows.Threading;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Services;
using Microsoft.Extensions.DependencyInjection;
using WpfPage = System.Windows.Controls.Page;

namespace ArkheideSystem.Flourish.Views.Windows;

internal sealed class ShellProfileController : IDisposable
{
    private readonly FlourishTitlebar titlebar;
    private readonly ProfileOverlay overlay;
    private readonly ProfileFlyoutService flyoutService;
    private readonly IProfileService profileService;
    private readonly TitleBarService titleBarService;
    private readonly FontService fontService;
    private readonly NotificationService notificationService;
    private readonly IServiceProvider serviceProvider;
    private readonly Dispatcher dispatcher;
    private readonly bool isProfileConfigured;
    private bool isProfileServiceSubscribed;
    private volatile bool isDisposed;

    internal ShellProfileController(
        FlourishTitlebar titlebar,
        ProfileOverlay overlay,
        ProfileFlyoutService flyoutService,
        IProfileService profileService,
        TitleBarService titleBarService,
        FontService fontService,
        NotificationService notificationService,
        IServiceProvider serviceProvider,
        bool isProfileConfigured
    )
    {
        this.titlebar = titlebar ?? throw new ArgumentNullException(nameof(titlebar));
        this.overlay = overlay ?? throw new ArgumentNullException(nameof(overlay));
        this.flyoutService =
            flyoutService ?? throw new ArgumentNullException(nameof(flyoutService));
        this.profileService =
            profileService ?? throw new ArgumentNullException(nameof(profileService));
        this.titleBarService =
            titleBarService ?? throw new ArgumentNullException(nameof(titleBarService));
        this.fontService = fontService ?? throw new ArgumentNullException(nameof(fontService));
        this.notificationService =
            notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        this.serviceProvider =
            serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        this.isProfileConfigured = isProfileConfigured;
        dispatcher = overlay.Dispatcher;

        titlebar.ProfileToggleRequested += Titlebar_ProfileToggleRequested;
        overlay.DismissRequested += Overlay_DismissRequested;
        overlay.PlacementInvalidated += Overlay_PlacementInvalidated;
        flyoutService.Changed += FlyoutService_Changed;
        titleBarService.Changed += TitleBarService_Changed;
        fontService.Changed += FontService_Changed;
        ConfigureSurface();
    }

    internal event EventHandler? Opening;

    internal event EventHandler? PlacementRequested;

    internal bool IsOpen => overlay.IsOpen;

    internal bool IsAvailable =>
        isProfileConfigured
        && titleBarService.Current is { IsEnabled: true, IsProfileVisible: true }
        && flyoutService.Current.IsEnabled;

    internal async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (isDisposed || !flyoutService.Current.IsEnabled)
        {
            return;
        }

        try
        {
            await profileService.InitializeAsync(cancellationToken);
            DispatchIfActive(() =>
            {
                if (IsAvailable)
                {
                    titlebar.SetProfile(profileService.CurrentProfile);
                }
            });
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
        catch (Exception error)
        {
            System.Diagnostics.Debug.WriteLine(
                $"Flourish profile initialization failed: {error}"
            );
        }
    }

    internal void Toggle()
    {
        if (!isDisposed && IsAvailable)
        {
            flyoutService.Toggle();
        }
    }

    internal void Hide()
    {
        if (!isDisposed)
        {
            flyoutService.Hide();
        }
    }

    internal void RequestPlacement()
    {
        if (!isDisposed && overlay.IsOpen)
        {
            PlacementRequested?.Invoke(this, EventArgs.Empty);
        }
    }

    public void Dispose()
    {
        if (isDisposed)
        {
            return;
        }

        isDisposed = true;
        titlebar.ProfileToggleRequested -= Titlebar_ProfileToggleRequested;
        overlay.DismissRequested -= Overlay_DismissRequested;
        overlay.PlacementInvalidated -= Overlay_PlacementInvalidated;
        flyoutService.Changed -= FlyoutService_Changed;
        titleBarService.Changed -= TitleBarService_Changed;
        fontService.Changed -= FontService_Changed;
        SetProfileSubscription(enabled: false);
    }

    private void ConfigureSurface(
        FlourishProfileFlyoutState? state = null
    )
    {
        if (isDisposed)
        {
            return;
        }

        var current = state ?? flyoutService.Current;
        var isAvailable =
            isProfileConfigured
            && titleBarService.Current is { IsEnabled: true, IsProfileVisible: true }
            && current.IsEnabled;
        titlebar.SetProfileAvailability(isAvailable);
        SetProfileSubscription(isAvailable);
        if (isAvailable)
        {
            titlebar.SetProfile(profileService.CurrentProfile);
        }

        ApplyFlyoutState(current, isAvailable);
    }

    private void SetProfileSubscription(bool enabled)
    {
        if (enabled == isProfileServiceSubscribed)
        {
            return;
        }

        if (enabled)
        {
            profileService.ProfileChanged += ProfileService_ProfileChanged;
        }
        else
        {
            profileService.ProfileChanged -= ProfileService_ProfileChanged;
        }

        isProfileServiceSubscribed = enabled;
    }

    private void EnsureProfileContent(FlourishProfileFlyoutState state)
    {
        if (overlay.HasMaterializedContent(state.ContentPageType))
        {
            return;
        }

        var profilePage = ActivatorUtilities.GetServiceOrCreateInstance(
            serviceProvider,
            state.ContentPageType
        );
        if (profilePage is not WpfPage page)
        {
            throw new InvalidOperationException(
                $"Configured profile page {state.ContentPageType.FullName} is not a WPF Page."
            );
        }

        fontService.ApplyToPage(page, state.ContentPageType);
        if (!overlay.SetContent(page, state.ContentPageType))
        {
            throw new InvalidOperationException(
                $"Navigation to profile page {state.ContentPageType.FullName} was rejected."
            );
        }
    }

    private void ApplyFlyoutState(
        FlourishProfileFlyoutState state,
        bool isAvailable
    )
    {
        overlay.Close();
        if (!isAvailable)
        {
            flyoutService.SynchronizeVisibility(false);
            return;
        }

        if (!state.IsVisible)
        {
            return;
        }

        try
        {
            EnsureProfileContent(state);
        }
        catch (Exception error)
        {
            flyoutService.SynchronizeVisibility(false);
            System.Diagnostics.Debug.WriteLine(
                $"Flourish profile content initialization failed: {error}"
            );
            notificationService.Upsert(
                new FlourishNotification(
                    "flourish.profile.content.error",
                    "Profile unavailable",
                    error.Message,
                    FlourishNotificationSeverity.Error,
                    Duration: TimeSpan.FromSeconds(8)
                )
            );
            return;
        }

        Opening?.Invoke(this, EventArgs.Empty);
        if (isDisposed)
        {
            return;
        }

        overlay.Open();
        _ = dispatcher.BeginInvoke(
            DispatcherPriority.Loaded,
            new Action(RequestPlacement)
        );
    }

    private void ProfileService_ProfileChanged(object? sender, ProfileChangedEventArgs e)
    {
        DispatchIfActive(() =>
        {
            if (IsAvailable)
            {
                titlebar.SetProfile(e.Profile);
            }
        });
    }

    private void FlyoutService_Changed(
        object? sender,
        FlourishProfileFlyoutChangedEventArgs e
    )
    {
        DispatchIfActive(() => ConfigureSurface(e.State));
    }

    private void TitleBarService_Changed(
        object? sender,
        FlourishTitleBarChangedEventArgs e
    )
    {
        DispatchIfActive(() => ConfigureSurface());
    }

    private void FontService_Changed(object? sender, FlourishFontChangedEventArgs e)
    {
        if (e.ChangeKind == FlourishFontChangeKind.Icon)
        {
            return;
        }

        DispatchIfActive(() =>
        {
            var pageType = flyoutService.Current.ContentPageType;
            if (
                overlay.ContentPage is { } page
                && (e.AffectedPageType is null || pageType == e.AffectedPageType)
            )
            {
                fontService.ApplyToPage(page, pageType);
            }
        });
    }

    private void Titlebar_ProfileToggleRequested(object? sender, EventArgs e) => Toggle();

    private void Overlay_DismissRequested(object? sender, EventArgs e) => Hide();

    private void Overlay_PlacementInvalidated(object? sender, EventArgs e) =>
        RequestPlacement();

    private void DispatchIfActive(Action action)
    {
        void ExecuteIfActive()
        {
            if (
                isDisposed
                || dispatcher.HasShutdownStarted
                || dispatcher.HasShutdownFinished
            )
            {
                return;
            }

            action();
        }

        if (dispatcher.CheckAccess())
        {
            ExecuteIfActive();
            return;
        }

        if (
            isDisposed
            || dispatcher.HasShutdownStarted
            || dispatcher.HasShutdownFinished
        )
        {
            return;
        }

        _ = dispatcher.BeginInvoke(
            DispatcherPriority.DataBind,
            new Action(ExecuteIfActive)
        );
    }
}
