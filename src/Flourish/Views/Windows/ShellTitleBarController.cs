using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Imaging;
using ArkheideSystem.Flourish.Services;

namespace ArkheideSystem.Flourish.Views.Windows;

internal sealed class ShellTitleBarController : IDisposable
{
    private const double EdgeSafeMargin = 14;
    private const double AnchorGap = 6;

    private readonly FlourishTitlebar titlebar;
    private readonly ApplicationInfoOverlay applicationInfo;
    private readonly FrameworkElement shellRoot;
    private readonly TitleBarService titleBarService;
    private readonly TitleBarSearchService searchService;
    private readonly ProjectSelectorController projectSelector;
    private readonly FlourishLocalizationService localization;
    private readonly Func<bool> isNavigationEnabled;
    private readonly Func<bool> isThemeEnabled;
    private readonly Func<bool> isProfileAvailable;
    private readonly TitleBarLogoLoadCoordinator logoCoordinator = new(
        TitleBarVisualAssets.LoadLogoAsync
    );
    private FrameworkElement? flyoutAnchor;
    private IInputElement? restoreFocusTarget;
    private ImageSource? currentLogoSource;
    private string currentLogoFallbackText = "F";
    private FlourishTitleBarState state;
    private long appliedVersion;
    private bool openedWithFocus;
    private bool isInitialized;
    private bool isDisposed;

    internal ShellTitleBarController(
        FlourishTitlebar titlebar,
        ApplicationInfoOverlay applicationInfo,
        FrameworkElement shellRoot,
        TitleBarService titleBarService,
        TitleBarSearchService searchService,
        ProjectSelectorController projectSelector,
        FlourishLocalizationService localization,
        Func<bool> isNavigationEnabled,
        Func<bool> isThemeEnabled,
        Func<bool> isProfileAvailable
    )
    {
        this.titlebar = titlebar ?? throw new ArgumentNullException(nameof(titlebar));
        this.applicationInfo =
            applicationInfo ?? throw new ArgumentNullException(nameof(applicationInfo));
        this.shellRoot = shellRoot ?? throw new ArgumentNullException(nameof(shellRoot));
        this.titleBarService =
            titleBarService ?? throw new ArgumentNullException(nameof(titleBarService));
        this.searchService =
            searchService ?? throw new ArgumentNullException(nameof(searchService));
        this.projectSelector =
            projectSelector ?? throw new ArgumentNullException(nameof(projectSelector));
        this.localization =
            localization ?? throw new ArgumentNullException(nameof(localization));
        this.isNavigationEnabled =
            isNavigationEnabled ?? throw new ArgumentNullException(nameof(isNavigationEnabled));
        this.isThemeEnabled =
            isThemeEnabled ?? throw new ArgumentNullException(nameof(isThemeEnabled));
        this.isProfileAvailable =
            isProfileAvailable ?? throw new ArgumentNullException(nameof(isProfileAvailable));
        state = titleBarService.Current;
        appliedVersion = titleBarService.CurrentVersion;
    }

    internal event EventHandler? Opening;

    internal event EventHandler<ShellTitleBarStateChangedEventArgs>? StateChanged;

    internal event EventHandler<FlourishProjectsChangedEventArgs>? ProjectChanged;

    internal event EventHandler<ShellTitleBarIconChangedEventArgs>? IconChanged;

    internal FlourishTitleBarState CurrentState => state;

    internal bool IsApplicationInfoOpen => applicationInfo.IsOpen;

    internal void Init()
    {
        ObjectDisposedException.ThrowIf(isDisposed, this);
        if (isInitialized)
        {
            return;
        }

        isInitialized = true;
        titlebar.LogoHoverRequested += Titlebar_LogoHoverRequested;
        titlebar.LogoClickRequested += Titlebar_LogoClickRequested;
        titlebar.InteractionStarted += Titlebar_InteractionStarted;
        titlebar.SearchTextChanged += Titlebar_SearchTextChanged;
        applicationInfo.DismissRequested += ApplicationInfo_DismissRequested;
        applicationInfo.PlacementInvalidated += ApplicationInfo_PlacementInvalidated;
        titleBarService.Changed += TitleBarService_Changed;
        searchService.ProgrammaticStateChanged += SearchService_ProgrammaticStateChanged;
        projectSelector.Opening += ProjectSelector_Opening;
        projectSelector.Changed += ProjectSelector_Changed;
        localization.Changed += Localization_Changed;

        titlebar.ApplyLocale(localization);
        projectSelector.Init(state);
        ApplyState(state, previous: null);
        ApplySearchState(searchService.Current);
    }

    internal void ApplyPendingSearchFocus()
    {
        if (isDisposed || !state.IsEnabled || !searchService.Current.FocusRequested)
        {
            return;
        }

        _ = titlebar.Dispatcher.BeginInvoke(
            DispatcherPriority.Input,
            new Action(() =>
            {
                if (
                    !isDisposed
                    && state.IsEnabled
                    && searchService.Current.FocusRequested
                )
                {
                    titlebar.FocusSearchBox();
                    searchService.AcknowledgeFocusRequest();
                }
            })
        );
    }

    internal void SetApplicationInfoBody(IReadOnlyList<FrameworkElement> elements)
    {
        ObjectDisposedException.ThrowIf(isDisposed, this);
        applicationInfo.SetBody(elements);
        if (applicationInfo.IsOpen)
        {
            RefreshApplicationInfo();
        }
    }

    internal void CloseApplicationInfo(bool restoreFocus = true)
    {
        if (!applicationInfo.IsOpen)
        {
            return;
        }

        applicationInfo.Close();
        var target = restoreFocusTarget ?? flyoutAnchor;
        var shouldRestore = restoreFocus && openedWithFocus;
        flyoutAnchor = null;
        restoreFocusTarget = null;
        openedWithFocus = false;
        if (shouldRestore && target is not null)
        {
            Keyboard.Focus(target);
        }
    }

    internal void UpdateApplicationInfoPosition()
    {
        if (
            !applicationInfo.IsOpen
            || flyoutAnchor is null
            || shellRoot.ActualWidth <= EdgeSafeMargin * 2
            || shellRoot.ActualHeight <= EdgeSafeMargin * 2
        )
        {
            return;
        }

        var topLeft = flyoutAnchor.TranslatePoint(new System.Windows.Point(), shellRoot);
        var anchor = new Rect(
            topLeft,
            new System.Windows.Size(flyoutAnchor.ActualWidth, flyoutAnchor.ActualHeight)
        );
        var availableWidth = Math.Max(0, shellRoot.ActualWidth - EdgeSafeMargin * 2);
        var cardWidth = applicationInfo.CardActualWidth > 0
            ? Math.Min(applicationInfo.CardActualWidth, availableWidth)
            : Math.Min(applicationInfo.CardWidth, availableWidth);
        var desiredLeft = anchor.Left + (anchor.Width - cardWidth) / 2;
        var maximumLeft = Math.Max(
            EdgeSafeMargin,
            shellRoot.ActualWidth - cardWidth - EdgeSafeMargin
        );
        var left = Math.Clamp(desiredLeft, EdgeSafeMargin, maximumLeft);
        var top = Math.Max(EdgeSafeMargin, anchor.Bottom + AnchorGap);
        applicationInfo.SetLayout(
            left,
            top,
            availableWidth,
            Math.Max(0, shellRoot.ActualHeight - top - EdgeSafeMargin)
        );
    }

    public void Dispose()
    {
        if (isDisposed)
        {
            return;
        }

        isDisposed = true;
        if (isInitialized)
        {
            titlebar.LogoHoverRequested -= Titlebar_LogoHoverRequested;
            titlebar.LogoClickRequested -= Titlebar_LogoClickRequested;
            titlebar.InteractionStarted -= Titlebar_InteractionStarted;
            titlebar.SearchTextChanged -= Titlebar_SearchTextChanged;
            applicationInfo.DismissRequested -= ApplicationInfo_DismissRequested;
            applicationInfo.PlacementInvalidated -= ApplicationInfo_PlacementInvalidated;
            titleBarService.Changed -= TitleBarService_Changed;
            searchService.ProgrammaticStateChanged -= SearchService_ProgrammaticStateChanged;
            projectSelector.Opening -= ProjectSelector_Opening;
            projectSelector.Changed -= ProjectSelector_Changed;
            localization.Changed -= Localization_Changed;
        }

        logoCoordinator.Dispose();
        CloseApplicationInfo(restoreFocus: false);
        Opening = null;
        StateChanged = null;
        ProjectChanged = null;
        IconChanged = null;
    }

    private void TitleBarService_Changed(object? sender, FlourishTitleBarChangedEventArgs e)
    {
        Dispatch(() =>
        {
            if (e.Version <= appliedVersion)
            {
                return;
            }

            appliedVersion = e.Version;
            var previous = state;
            state = e.State;
            ApplyState(state, previous);
            StateChanged?.Invoke(
                this,
                new ShellTitleBarStateChangedEventArgs(previous, state)
            );
        });
    }

    private void ProjectSelector_Changed(object? sender, FlourishProjectsChangedEventArgs e)
    {
        Dispatch(() =>
        {
            ApplyVisibility(searchService.Current.IsVisible);
            titlebar.SetDisplayTitle(projectSelector.GetDisplayedTitle(state, e.Current));
            EnsureApplicationInfoAvailable();
            if (applicationInfo.IsOpen)
            {
                RefreshApplicationInfo();
                UpdateApplicationInfoPosition();
            }

            ProjectChanged?.Invoke(this, e);
        });
    }

    private void ProjectSelector_Opening(object? sender, EventArgs e)
    {
        CloseApplicationInfo(restoreFocus: false);
        Opening?.Invoke(this, EventArgs.Empty);
    }

    private void SearchService_ProgrammaticStateChanged(
        object? sender,
        FlourishTitleBarSearchStateChangedEventArgs e
    )
    {
        Dispatch(() =>
        {
            if (searchService.IsCurrentVersion(e.State.Version))
            {
                ApplySearchState(e.State);
            }
        });
    }

    private void ApplySearchState(FlourishTitleBarSearchState searchState)
    {
        titlebar.SetSearchPlaceholder(searchState.Placeholder);
        titlebar.SetSearchText(searchState.Text);
        ApplyVisibility(searchState.IsVisible);
        if (searchState.FocusRequested && state.IsEnabled)
        {
            titlebar.FocusSearchBox();
            searchService.AcknowledgeFocusRequest();
        }
    }

    private void ApplyState(
        FlourishTitleBarState current,
        FlourishTitleBarState? previous
    )
    {
        state = current;
        projectSelector.SetTitleState(current);
        titlebar.SetDisplayTitle(projectSelector.GetDisplayedTitle(current));
        titlebar.SetSearchPlaceholder(current.SearchPlaceholder);
        ApplyVisibility(searchService.Current.IsVisible);

        if (
            previous is null
            || !StringComparer.Ordinal.Equals(previous.LogoPath, current.LogoPath)
            || !StringComparer.Ordinal.Equals(
                previous.LogoFallbackText,
                current.LogoFallbackText
            )
            || previous.IsLogoVisible != current.IsLogoVisible
        )
        {
            RequestLogo(current.LogoPath, current.LogoFallbackText, current.IsLogoVisible);
        }

        EnsureApplicationInfoAvailable();
        if (applicationInfo.IsOpen)
        {
            RefreshApplicationInfo();
            UpdateApplicationInfoPosition();
        }
    }

    private void ApplyVisibility(bool searchVisible)
    {
        var project = projectSelector.Current;
        titlebar.ConfigureVisibility(
            searchVisible,
            state.IsBreadcrumbVisible
                && state.BreadcrumbMode != BreadcrumbShowOption.Hidden,
            state.IsNavigationToggleVisible && isNavigationEnabled(),
            state.IsLogoVisible,
            state.IsTitleVisible || project.IsMultiProjectEnabled,
            state.IsThemeToggleVisible && isThemeEnabled(),
            isProfileAvailable()
        );
    }

    private void RequestLogo(string? path, string fallbackText, bool isVisible)
    {
        if (isDisposed)
        {
            return;
        }

        currentLogoFallbackText = fallbackText;
        var request = logoCoordinator.Request(path);
        if (request.Completion.IsCompletedSuccessfully)
        {
            var result = request.Completion.Result;
            if (logoCoordinator.IsCurrent(result))
            {
                ApplyLogo(result.Source, isVisible);
            }
            return;
        }

        ApplyLogo(
            request.IsNewRequest ? TitleBarVisualAssets.DefaultLogoSource : currentLogoSource,
            isVisible
        );
        if (request.IsNewRequest)
        {
            _ = CompleteLogoRequestAsync(request);
        }
    }

    private async Task CompleteLogoRequestAsync(TitleBarLogoLoadRequest request)
    {
        TitleBarLogoLoadResult result;
        try
        {
            result = await request.Completion.ConfigureAwait(false);
        }
        catch (Exception)
        {
            return;
        }

        if (!result.IsCurrent)
        {
            return;
        }

        Dispatch(() =>
        {
            if (logoCoordinator.IsCurrent(result))
            {
                ApplyLogo(result.Source, state.IsLogoVisible);
            }
        });
    }

    private void ApplyLogo(ImageSource? source, bool isVisible)
    {
        var effectiveSource = source ?? TitleBarVisualAssets.DefaultLogoSource;
        currentLogoSource = effectiveSource;
        titlebar.SetLogo(effectiveSource, currentLogoFallbackText);
        IconChanged?.Invoke(
            this,
            new ShellTitleBarIconChangedEventArgs(isVisible ? effectiveSource : null)
        );
        if (applicationInfo.IsOpen)
        {
            RefreshApplicationInfo();
        }
    }

    private void Titlebar_LogoHoverRequested(object? sender, EventArgs e)
    {
        if (!openedWithFocus)
        {
            OpenApplicationInfo(focus: false);
        }
    }

    private void Titlebar_LogoClickRequested(object? sender, EventArgs e) =>
        OpenApplicationInfo(focus: true);

    private void Titlebar_InteractionStarted(object? sender, EventArgs e) =>
        CloseApplicationInfo(restoreFocus: false);

    private void Titlebar_SearchTextChanged(object? sender, string text) =>
        searchService.PublishFromView(text);

    private void OpenApplicationInfo(bool focus)
    {
        if (!state.IsEnabled || !state.IsLogoVisible)
        {
            return;
        }

        Opening?.Invoke(this, EventArgs.Empty);
        RefreshApplicationInfo();
        var anchor = titlebar.GetLogoButtonAnchor();
        var wasOpen = applicationInfo.IsOpen;
        if (focus && !openedWithFocus)
        {
            openedWithFocus = true;
            restoreFocusTarget = anchor;
        }
        else if (!focus && !wasOpen)
        {
            openedWithFocus = false;
            restoreFocusTarget = null;
        }

        flyoutAnchor = anchor;
        applicationInfo.PlacementTarget = anchor;
        applicationInfo.Open();
        _ = titlebar.Dispatcher.BeginInvoke(
            DispatcherPriority.Loaded,
            new Action(() =>
            {
                if (isDisposed || !applicationInfo.IsOpen)
                {
                    return;
                }

                UpdateApplicationInfoPosition();
                if (focus)
                {
                    applicationInfo.FocusContent();
                }
            })
        );
    }

    private void RefreshApplicationInfo() =>
        applicationInfo.SetState(state, projectSelector.Current, currentLogoSource);

    private void EnsureApplicationInfoAvailable()
    {
        if (applicationInfo.IsOpen && (!state.IsEnabled || !state.IsLogoVisible))
        {
            CloseApplicationInfo(restoreFocus: false);
        }
    }

    private void ApplicationInfo_DismissRequested(object? sender, EventArgs e) =>
        CloseApplicationInfo();

    private void ApplicationInfo_PlacementInvalidated(object? sender, EventArgs e) =>
        UpdateApplicationInfoPosition();

    private void Localization_Changed(object? sender, FlourishLocalizationChangedEventArgs e) =>
        Dispatch(() => titlebar.ApplyLocale(localization));

    private void Dispatch(Action action)
    {
        void Execute()
        {
            if (!isDisposed)
            {
                action();
            }
        }

        if (titlebar.Dispatcher.CheckAccess())
        {
            Execute();
        }
        else if (
            !isDisposed
            && !titlebar.Dispatcher.HasShutdownStarted
            && !titlebar.Dispatcher.HasShutdownFinished
        )
        {
            _ = titlebar.Dispatcher.BeginInvoke(
                DispatcherPriority.DataBind,
                new Action(Execute)
            );
        }
    }
}

internal sealed class ShellTitleBarStateChangedEventArgs(
    FlourishTitleBarState previous,
    FlourishTitleBarState current
) : EventArgs
{
    internal FlourishTitleBarState Previous { get; } = previous;

    internal FlourishTitleBarState Current { get; } = current;
}

internal sealed class ShellTitleBarIconChangedEventArgs(ImageSource? icon) : EventArgs
{
    internal ImageSource? Icon { get; } = icon;
}
