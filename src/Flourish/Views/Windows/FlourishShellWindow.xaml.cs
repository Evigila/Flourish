using System.ComponentModel;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Controls;
using ArkheideSystem.Flourish.Internal.Configuration;
using ArkheideSystem.Flourish.Internal.Interaction;
using ArkheideSystem.Flourish.Services;
using Button = ArkheideSystem.Flourish.Controls.Button;
using TextBlock = ArkheideSystem.Flourish.Controls.FlourishTextBlock;
using TextBoxBase = System.Windows.Controls.Primitives.TextBoxBase;
using WpfComboBox = System.Windows.Controls.ComboBox;
using WpfPage = System.Windows.Controls.Page;
using WpfPanel = System.Windows.Controls.Panel;

namespace ArkheideSystem.Flourish.Views.Windows;

internal partial class FlourishShellWindow : Window
{
    private const string ProjectSaveCommandKey = "flourish.project.save";
    private const int BuiltInProjectBehaviorPriority = -1000;

    private readonly NavigationService navigationService;
    private readonly ShellNavigationController navigationController;
    private readonly ShellToolbarController toolbarController;
    private readonly ShellRegionService shellRegionService;
    private readonly IMessageService messageService;
    private readonly NotificationService notificationService;
    private readonly ShellNotificationController notificationController;
    private readonly TrayIconService trayIconService;
    private readonly ICommandRegistry commandRegistry;
    private readonly ICommandDispatcher commandDispatcher;
    private readonly ShortcutService shortcutService;
    private readonly FontService fontService;
    private readonly MaterialEffectService materialEffectService;
    private readonly ThemeService themeService;
    private readonly ContentLayoutService contentLayoutService;
    private readonly FlourishMotionService motionService;
    private readonly TitleBarService titleBarService;
    private readonly ShellTitleBarController titleBarController;
    private readonly ProjectService projectService;
    private readonly ProjectSelectorController projectSelectorController;
    private readonly WindowService windowService;
    private readonly WindowCloseService windowCloseService;
    private readonly ShellProfileController profileController;
    private readonly WindowFrameFixService windowFrameFixService;
    private readonly FlourishLocalizationService localizationService;
    private readonly IServiceProvider serviceProvider;
    private readonly FlourishShellOptions options;
    private readonly FlourishShellWindowFrame shellWindowFrame;
    private readonly ShellStatusSurfaceController statusSurfaceController;
    private readonly ICommandRegistration projectSaveCommandRegistration;
    private readonly IShortcutRegistration projectSaveShortcutRegistration;
    private readonly IWindowCloseGuardRegistration projectCloseGuardRegistration;
    private readonly Dictionary<string, RegionElementView> regionElementsById = new(
        StringComparer.Ordinal
    );
    private readonly NavigationPaneTransitionController navigationPaneTransition = new();
    private readonly PageTransitionController pageTransition = new();
    private double navigationPaneDragStartWidth;
    private IInputElement? statusFlyoutRestoreFocusTarget;
    private volatile bool isShellClosed;
    private bool statusFlyoutOpenedWithFocus;
    private bool allowClose;
    private bool closeRequestPending;

    private sealed record RegionElementView(
        FlourishRegionContent Definition,
        FrameworkElement Element
    );

    public FlourishShellWindow(
        NavigationService navigationService,
        NavigationPanelService navigationPanelService,
        NavigationMenuService navigationMenuService,
        FlourishToolbarService toolbarService,
        FlourishStatusService statusService,
        ShellRegionService shellRegionService,
        IBackgroundTaskService backgroundTaskService,
        IMessageService messageService,
        NotificationService notificationService,
        TrayIconService trayIconService,
        ICommandRegistry commandRegistry,
        ICommandDispatcher commandDispatcher,
        ShortcutService shortcutService,
        FontService fontService,
        MaterialEffectService materialEffectService,
        ThemeService themeService,
        ContentLayoutService contentLayoutService,
        FlourishMotionService motionService,
        TitleBarService titleBarService,
        ProjectService projectService,
        IProjectBehavior projectBehavior,
        TitleBarSearchService titleBarSearchService,
        WindowService windowService,
        WindowCloseService windowCloseService,
        ProfileFlyoutService profileFlyoutService,
        WindowFrameFixService windowFrameFixService,
        IProfileService profileService,
        FlourishLocalizationService localizationService,
        IServiceProvider serviceProvider,
        FlourishShellOptions options
    )
    {
        themeService.Initialize(System.Windows.Application.Current);
        InitializeComponent();
        shellWindowFrame = new FlourishShellWindowFrame(this, ShellBorder);

        this.navigationService = navigationService;
        this.shellRegionService = shellRegionService;
        this.messageService = messageService;
        this.notificationService = notificationService;
        this.trayIconService = trayIconService;
        this.commandRegistry = commandRegistry;
        this.commandDispatcher = commandDispatcher;
        toolbarController = new ShellToolbarController(
            ContentHost.Toolbar,
            toolbarService,
            commandRegistry,
            commandDispatcher
        );
        notificationController = new ShellNotificationController(
            NotificationHost,
            notificationService,
            commandDispatcher
        );
        this.shortcutService = shortcutService;
        this.fontService = fontService;
        this.materialEffectService = materialEffectService;
        this.themeService = themeService;
        this.contentLayoutService = contentLayoutService;
        this.motionService = motionService;
        this.titleBarService = titleBarService;
        this.projectService = projectService;
        this.windowService = windowService;
        this.windowCloseService = windowCloseService;
        this.windowFrameFixService = windowFrameFixService;
        this.localizationService = localizationService;
        this.serviceProvider = serviceProvider;
        this.options = options;
        projectSelectorController = new ProjectSelectorController(
            Titlebar,
            projectService,
            projectBehavior,
            localizationService,
            notificationService
        );
        navigationController = new ShellNavigationController(
            NavigationPane,
            ContentHost,
            Titlebar,
            navigationService,
            navigationPanelService,
            navigationMenuService,
            commandDispatcher,
            options,
            titleBarService.Current
        );
        navigationController.LayoutRequested += NavigationController_LayoutRequested;
        profileController = new ShellProfileController(
            Titlebar,
            ProfileOverlay,
            profileFlyoutService,
            profileService,
            titleBarService,
            fontService,
            notificationService,
            serviceProvider,
            options.IsTitlebarProfileEnabled
        );
        profileController.Opening += ProfileController_Opening;
        profileController.PlacementRequested += ProfileController_PlacementRequested;
        statusSurfaceController = new ShellStatusSurfaceController(
            statusService,
            backgroundTaskService,
            localizationService,
            StatusBar,
            StatusOverlay,
            Dispatcher
        );
        statusSurfaceController.OpenRequested += StatusSurfaceController_OpenRequested;
        statusSurfaceController.CloseRequested += StatusSurfaceController_CloseRequested;
        statusSurfaceController.VisualStateChanged += StatusSurfaceController_VisualStateChanged;
        statusSurfaceController.AnchorChanged += StatusSurfaceController_AnchorChanged;
        statusSurfaceController.ContentFocusInvalidated +=
            StatusSurfaceController_ContentFocusInvalidated;
        statusSurfaceController.Start();
        titleBarController = new ShellTitleBarController(
            Titlebar,
            ApplicationInfoOverlay,
            ShellRootGrid,
            titleBarService,
            titleBarSearchService,
            projectSelectorController,
            localizationService,
            () => navigationController.IsPanelEnabled,
            () => options.IsThemeEnabled,
            () => profileController.IsAvailable
        );
        titleBarController.Opening += TitleBarController_Opening;
        titleBarController.StateChanged += TitleBarController_StateChanged;
        titleBarController.ProjectChanged += TitleBarController_ProjectChanged;
        titleBarController.IconChanged += TitleBarController_IconChanged;
        titleBarController.Init();
        ApplyOptions();
        windowService.Attach(this);
        windowCloseService.Attach(RequestCloseCoreAsync);
        projectSaveCommandRegistration = commandRegistry.Register(
            ProjectSaveCommandKey,
            SaveActiveProjectCommandAsync,
            _ => projectSelectorController.CanSave,
            new CommandRegistrationOptions
            {
                DuplicatePolicy = CommandDuplicatePolicy.Append,
                Priority = BuiltInProjectBehaviorPriority,
            }
        );
        projectSaveShortcutRegistration = shortcutService.Register(
            new KeyGesture(Key.S, ModifierKeys.Control),
            ProjectSaveCommandKey,
            options: new ShortcutRegistrationOptions
            {
                ConflictPolicy = ShortcutConflictPolicy.Append,
                Priority = BuiltInProjectBehaviorPriority,
                AllowWhenTextInputFocused = true,
            }
        );
        projectCloseGuardRegistration = windowCloseService.RegisterGuard(
            "flourish.project.behavior",
            ProjectBehaviorCloseGuardAsync
        );
        toolbarController.Init();
        BuildRegionContents();
        navigationController.Init();
        shellRegionService.Changed += ShellRegionService_Changed;
        contentLayoutService.Changed += ContentLayoutService_Changed;
        fontService.Changed += FontService_Changed;
        motionService.Changed += MotionService_Changed;
        AttachTitlebarEvents();

        themeService.Changed += ThemeService_ThemeChanged;
        StateChanged += MainWindow_StateChanged;
        Closing += ShellWindow_Closing;
        Closed += ShellWindow_Closed;
        Loaded += ShellWindow_Loaded;
        PreviewKeyDown += ShellWindow_PreviewKeyDown;
        navigationService.Init(ContentHost.NavigationFrame);
        navigationService.Navigated += RootFrame_Navigated;

        navigationController.NavigateInitial();
    }

    private void ApplyOptions()
    {
        ApplyWindowOptions();
        Title = options.ApplicationTitle;
        statusSurfaceController.RefreshVisibility();
        ApplyContentLayoutOptions();

        NormalizeNavigationPaneWidths();
        ApplyNavigationPanelPlacement(navigationController.CurrentPanelState.Direction);
        ApplyNavigationPaneState(navigationController.CurrentPanelState);
        windowFrameFixService.Attach(this, titleBarService.Current.IsEnabled);
        materialEffectService.Attach(
            this,
            options.IsMaterialEffectEnabled ? options.MaterialEffect : MaterialEffect.None,
            "FlourishShellBackgroundBrush"
        );
        themeService.Attach(this);
        ApplyThemeState();
        trayIconService.Initialize(this, options.ApplicationTitle);
    }

    private async void ShellWindow_Loaded(object sender, RoutedEventArgs e)
    {
        Loaded -= ShellWindow_Loaded;
        await profileController.InitializeAsync();
    }

    private void ApplyWindowOptions()
    {
        MinWidth = options.WindowMinWidth;
        MinHeight = options.WindowMinHeight;
        MaxWidth = options.WindowMaxWidth;
        MaxHeight = options.WindowMaxHeight;
        Width = options.WindowWidth;
        Height = options.WindowHeight;
        WindowStartupLocation = options.WindowStartupLocation;
        ResizeMode = options.WindowResizeMode;
        Topmost = options.WindowTopmost;
        ShowInTaskbar = options.WindowShowInTaskbar;

        if (options.WindowLeft is { } left)
        {
            Left = left;
        }

        if (options.WindowTop is { } top)
        {
            Top = top;
        }

        if (
            options.UsePersistedWindowPosition
            && WindowStartupLocation == WindowStartupLocation.Manual
        )
        {
            KeepPersistedWindowReachable();
        }

        WindowState = options.WindowState;
        ApplyTitleBarFeatureState();
        Titlebar.SetMaximizeEnabled(
            ResizeMode is ResizeMode.CanResize or ResizeMode.CanResizeWithGrip
        );
    }

    private void KeepPersistedWindowReachable()
    {
        const double reachableEdge = 64;
        var virtualLeft = SystemParameters.VirtualScreenLeft;
        var virtualTop = SystemParameters.VirtualScreenTop;
        var virtualRight = virtualLeft + SystemParameters.VirtualScreenWidth;
        var virtualBottom = virtualTop + SystemParameters.VirtualScreenHeight;
        Left = Math.Clamp(Left, virtualLeft - Width + reachableEdge, virtualRight - reachableEdge);
        Top = Math.Clamp(Top, virtualTop, virtualBottom - reachableEdge);
    }

    private void ApplyTitleBarFeatureState(bool refreshFrame = false)
    {
        var useCustomFrame = titleBarService.Current.IsEnabled;
        var frameMode = useCustomFrame
            ? FlourishShellWindowFrameMode.Custom
            : FlourishShellWindowFrameMode.Native;

        void ApplyFrameAndSurface()
        {
            if (useCustomFrame)
            {
                shellWindowFrame.Apply(frameMode);
                Titlebar.Visibility = Visibility.Visible;
            }
            else
            {
                Titlebar.Visibility = Visibility.Collapsed;
                shellWindowFrame.Apply(frameMode);
            }

            ShellRootGrid.UpdateLayout();
            if (refreshFrame)
            {
                materialEffectService.Reapply(this);
            }
        }

        if (refreshFrame)
        {
            windowFrameFixService.ApplyFrameTransition(this, useCustomFrame, ApplyFrameAndSurface);
        }
        else
        {
            ApplyFrameAndSurface();
        }

        Titlebar.SetMaximized(WindowState == WindowState.Maximized);
        titleBarController.ApplyPendingSearchFocus();
    }

    private void AttachTitlebarEvents()
    {
        Titlebar.MinimizeRequested += Titlebar_MinimizeRequested;
        Titlebar.MaximizeRequested += Titlebar_MaximizeRequested;
        Titlebar.CloseRequested += Titlebar_CloseRequested;
        Titlebar.DragRequested += Titlebar_DragRequested;
        Titlebar.ToggleWindowStateRequested += Titlebar_ToggleWindowStateRequested;
        Titlebar.ThemeToggleRequested += Titlebar_ThemeToggleRequested;
    }

    private void TitleBarController_Opening(object? sender, EventArgs e)
    {
        CloseStatusFlyout(restoreFocus: false);
        profileController.Hide();
    }

    private void ProfileController_Opening(object? sender, EventArgs e)
    {
        CloseStatusFlyout(restoreFocus: false);
        titleBarController.CloseApplicationInfo(restoreFocus: false);
    }

    private void ProfileController_PlacementRequested(object? sender, EventArgs e) =>
        UpdateProfileCardPosition();

    private async void ShellWindow_PreviewKeyDown(
        object sender,
        System.Windows.Input.KeyEventArgs e
    )
    {
        if (e.Key == Key.Escape)
        {
            if (titleBarController.IsApplicationInfoOpen)
            {
                titleBarController.CloseApplicationInfo();
                e.Handled = true;
                return;
            }

            if (statusSurfaceController.IsOpen)
            {
                CloseStatusFlyout();
                e.Handled = true;
                return;
            }

            if (profileController.IsOpen)
            {
                profileController.Hide();
                e.Handled = true;
                return;
            }
        }

        var key = e.Key == Key.System ? e.SystemKey : e.Key;
        var modifiers = e.KeyboardDevice.Modifiers;
        if (ShouldIgnoreShortcutInput(key, modifiers, e.KeyboardDevice.IsKeyDown(Key.RightAlt)))
        {
            return;
        }

        if (!shortcutService.HasRegistrations(key, modifiers))
        {
            return;
        }

        var isTextInputFocused = IsTextInputTarget(
            e.KeyboardDevice.FocusedElement ?? e.OriginalSource
        );
        var context = new ShortcutResolutionContext(
            "shell",
            navigationService.CurrentNavigationKey
        );
        if (
            !shortcutService.TryResolve(
                key,
                modifiers,
                context,
                isTextInputFocused,
                out var registration
            ) || registration is null
        )
        {
            return;
        }

        e.Handled = true;
        await shortcutService.ExecuteResolvedAsync(registration);
    }

    internal static bool ShouldIgnoreShortcutInput(
        Key key,
        ModifierKeys modifiers,
        bool isRightAltPressed
    )
    {
        const ModifierKeys altGraphModifiers = ModifierKeys.Control | ModifierKeys.Alt;
        return key is Key.None or Key.System
            || IsModifierKey(key)
            || IsTextCompositionKey(key)
            || (isRightAltPressed && (modifiers & altGraphModifiers) == altGraphModifiers);
    }

    internal static bool IsTextInputTarget(object? target)
    {
        var current = target as DependencyObject;
        while (current is not null)
        {
            if (
                current is TextBoxBase or PasswordBox
                || current is WpfComboBox { IsEditable: true }
            )
            {
                return true;
            }

            current =
                current is Visual
                    ? VisualTreeHelper.GetParent(current) ?? LogicalTreeHelper.GetParent(current)
                    : LogicalTreeHelper.GetParent(current);
        }

        return false;
    }

    private static bool IsTextCompositionKey(Key key) =>
        key
            is Key.KanaMode
                or Key.JunjaMode
                or Key.FinalMode
                or Key.HanjaMode
                or Key.ImeConvert
                or Key.ImeNonConvert
                or Key.ImeAccept
                or Key.ImeModeChange
                or Key.ImeProcessed
                or Key.DbeAlphanumeric
                or Key.DbeKatakana
                or Key.DbeHiragana
                or Key.DbeSbcsChar
                or Key.DbeDbcsChar
                or Key.DbeRoman
                or Key.DbeNoRoman
                or Key.DbeEnterWordRegisterMode
                or Key.DbeEnterImeConfigureMode
                or Key.DbeFlushString
                or Key.DbeCodeInput
                or Key.DbeNoCodeInput
                or Key.DbeDetermineString
                or Key.DbeEnterDialogConversionMode
                or Key.DeadCharProcessed;

    private static bool IsModifierKey(Key key) =>
        key
            is Key.LeftAlt
                or Key.RightAlt
                or Key.LeftCtrl
                or Key.RightCtrl
                or Key.LeftShift
                or Key.RightShift
                or Key.LWin
                or Key.RWin;

    private void ShellRootGrid_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (navigationPaneTransition.IsActive)
        {
            ApplyNavigationPaneState(navigationController.CurrentPanelState);
        }

        UpdateProfileCardPosition();
        UpdateStatusFlyoutPosition();
        titleBarController.UpdateApplicationInfoPosition();
    }

    private void UpdateProfileCardPosition()
    {
        const double edgeSafeMargin = 14;
        const double anchorGap = 6;
        if (
            !profileController.IsOpen
            || ShellRootGrid.ActualWidth <= edgeSafeMargin * 2
            || ShellRootGrid.ActualHeight <= edgeSafeMargin * 2
        )
        {
            return;
        }

        var anchor = Titlebar.GetProfileButtonBounds(ShellRootGrid);
        var availableWidth = Math.Max(0, ShellRootGrid.ActualWidth - edgeSafeMargin * 2);
        var cardWidth =
            ProfileOverlay.CardActualWidth > 0
                ? Math.Min(ProfileOverlay.CardActualWidth, availableWidth)
                : Math.Min(ProfileOverlay.CardWidth, availableWidth);
        var desiredLeft = anchor.Left + (anchor.Width - cardWidth) / 2;
        var maximumLeft = Math.Max(
            edgeSafeMargin,
            ShellRootGrid.ActualWidth - cardWidth - edgeSafeMargin
        );
        var left = Math.Clamp(desiredLeft, edgeSafeMargin, maximumLeft);

        var top = Math.Max(edgeSafeMargin, anchor.Bottom + anchorGap);
        ProfileOverlay.SetLayout(
            left,
            top,
            availableWidth,
            Math.Max(0, ShellRootGrid.ActualHeight - top - edgeSafeMargin)
        );
    }

    private async ValueTask<CommandResult> SaveActiveProjectCommandAsync(
        CommandContext context,
        CancellationToken cancellationToken
    ) => await projectSelectorController.SaveAsync(context, cancellationToken);

    private async ValueTask<WindowCloseDecision> ProjectBehaviorCloseGuardAsync(
        WindowCloseContext context,
        CancellationToken cancellationToken
    )
    {
        if (
            context.Reason != WindowCloseRequestReason.Tray
            && windowCloseService.Behavior == WindowCloseBehavior.MinimizeToTray
        )
        {
            return WindowCloseDecision.Allow;
        }

        return await projectSelectorController.CheckCloseAsync(cancellationToken);
    }

    private void StatusSurfaceController_OpenRequested(
        object? sender,
        StatusSurfaceOpenRequestedEventArgs e
    )
    {
        if (!e.FocusRequested && statusFlyoutOpenedWithFocus)
        {
            return;
        }

        profileController.Hide();
        titleBarController.CloseApplicationInfo(restoreFocus: false);
        var wasVisible = statusSurfaceController.IsOpen;
        if (e.FocusRequested && !statusFlyoutOpenedWithFocus)
        {
            statusFlyoutOpenedWithFocus = true;
            statusFlyoutRestoreFocusTarget = e.Anchor;
        }
        else if (!e.FocusRequested && !wasVisible)
        {
            statusFlyoutOpenedWithFocus = false;
            statusFlyoutRestoreFocusTarget = null;
        }

        statusSurfaceController.Open();
        Dispatcher.BeginInvoke(
            new Action(() =>
            {
                UpdateStatusFlyoutPosition();
                if (e.FocusRequested)
                {
                    statusSurfaceController.FocusContent();
                }
            })
        );
    }

    private void StatusSurfaceController_CloseRequested(
        object? sender,
        StatusSurfaceCloseRequestedEventArgs e
    ) => CloseStatusFlyout(e.RestoreFocus);

    private void StatusSurfaceController_VisualStateChanged(object? sender, EventArgs e) =>
        UpdateStatusFlyoutPosition();

    private void StatusSurfaceController_AnchorChanged(
        object? sender,
        StatusSurfaceAnchorChangedEventArgs e
    )
    {
        if (statusFlyoutOpenedWithFocus && e.Anchor is not null)
        {
            statusFlyoutRestoreFocusTarget = e.Anchor;
        }
    }

    private void StatusSurfaceController_ContentFocusInvalidated(object? sender, EventArgs e)
    {
        if (statusFlyoutOpenedWithFocus)
        {
            statusSurfaceController.FocusContent();
        }
    }

    private void CloseStatusFlyout(bool restoreFocus = true)
    {
        if (!statusSurfaceController.IsOpen)
        {
            return;
        }

        var previousAnchor = statusSurfaceController.Close();
        var shouldRestoreFocus = restoreFocus && statusFlyoutOpenedWithFocus;
        var restoreTarget = statusFlyoutRestoreFocusTarget ?? previousAnchor;
        statusFlyoutOpenedWithFocus = false;
        statusFlyoutRestoreFocusTarget = null;
        if (shouldRestoreFocus && restoreTarget is not null)
        {
            Keyboard.Focus(restoreTarget);
        }
    }

    private void UpdateStatusFlyoutPosition()
    {
        const double edgeSafeMargin = 14;
        const double anchorGap = 6;
        var statusAnchor = statusSurfaceController.Anchor;
        if (
            !statusSurfaceController.IsOpen
            || statusAnchor is null
            || ShellRootGrid.ActualWidth <= edgeSafeMargin * 2
            || ShellRootGrid.ActualHeight <= edgeSafeMargin * 2
        )
        {
            return;
        }

        var anchorTopLeft = statusAnchor.TranslatePoint(new System.Windows.Point(), ShellRootGrid);
        var anchor = new Rect(
            anchorTopLeft,
            new System.Windows.Size(statusAnchor.ActualWidth, statusAnchor.ActualHeight)
        );
        var availableWidth = Math.Max(0, ShellRootGrid.ActualWidth - edgeSafeMargin * 2);
        var maxHeight = Math.Max(0, anchor.Top - edgeSafeMargin - anchorGap);
        var cardWidth =
            statusSurfaceController.CardActualWidth > 0
                ? Math.Min(statusSurfaceController.CardActualWidth, availableWidth)
                : Math.Min(statusSurfaceController.CardWidth, availableWidth);
        var cardHeight = Math.Min(statusSurfaceController.CardActualHeight, maxHeight);
        var desiredLeft = anchor.Left + (anchor.Width - cardWidth) / 2;
        var maximumLeft = Math.Max(
            edgeSafeMargin,
            ShellRootGrid.ActualWidth - cardWidth - edgeSafeMargin
        );
        var left = Math.Clamp(desiredLeft, edgeSafeMargin, maximumLeft);
        var top = Math.Max(edgeSafeMargin, anchor.Top - cardHeight - anchorGap);
        statusSurfaceController.SetLayout(left, top, availableWidth, maxHeight);
    }

    private void ApplyNavigationPanelPlacement(NavigationPanelDirection direction)
    {
        if (direction == NavigationPanelDirection.Right)
        {
            Grid.SetColumn(ContentHost.LayoutHost, 0);
            Grid.SetColumn(NavigationPane, 1);
            Grid.SetColumn(NavigationPaneSplitter, 1);
            NavigationPaneSplitter.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            NavigationPaneSplitter.ResizeBehavior = GridResizeBehavior.PreviousAndCurrent;
            return;
        }

        Grid.SetColumn(NavigationPane, 0);
        Grid.SetColumn(ContentHost.LayoutHost, 1);
        Grid.SetColumn(NavigationPaneSplitter, 0);
        NavigationPaneSplitter.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
        NavigationPaneSplitter.ResizeBehavior = GridResizeBehavior.CurrentAndNext;
    }

    private ColumnDefinition GetNavigationPaneColumn(NavigationPanelDirection direction)
    {
        return direction == NavigationPanelDirection.Right ? ContentColumn : PaneColumn;
    }

    private ColumnDefinition GetContentColumn(NavigationPanelDirection direction)
    {
        return direction == NavigationPanelDirection.Right ? PaneColumn : ContentColumn;
    }

    private void SetNavigationPaneWidth(double width, NavigationPanelDirection direction)
    {
        GetNavigationPaneColumn(direction).Width = new GridLength(width);
        GetContentColumn(direction).Width = new GridLength(1, GridUnitType.Star);
    }

    private void NormalizeNavigationPaneWidths()
    {
        var state = navigationController.CurrentPanelState;
        options.OpenPaneWidth = CoerceOpenPaneWidth(options.OpenPaneWidth, state);
        options.ClosedPaneWidth = Math.Min(options.ClosedPaneWidth, options.OpenPaneWidth);
    }

    private static double CoerceOpenPaneWidth(double width, FlourishNavigationPanelState state)
    {
        return Math.Min(Math.Max(width, state.MinWidth), state.MaxWidth);
    }

    private void ApplyNavigationPaneColumnConstraints(
        bool isOpen,
        FlourishNavigationPanelState state
    )
    {
        NavigationPaneColumnLayout.ApplyConstraints(
            GetNavigationPaneColumn(state.Direction),
            GetContentColumn(state.Direction),
            isOpen,
            state.MinWidth,
            state.MaxWidth
        );
    }

    private void UpdateNavigationPaneSplitterState(FlourishNavigationPanelState state)
    {
        var isSplitterEnabled = state.IsEnabled && state.IsOpen;
        NavigationPaneSplitter.IsEnabled = isSplitterEnabled;
        NavigationPaneSplitter.Visibility = isSplitterEnabled
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private void ApplyNavigationPaneState(FlourishNavigationPanelState state, bool animate = false)
    {
        var isNavigationVisible = state.IsEnabled;
        var isOpen = isNavigationVisible && state.IsOpen;
        var paneWidth =
            !isNavigationVisible ? 0
            : isOpen ? CoerceOpenPaneWidth(state.OpenWidth, state)
            : state.ClosedWidth;

        if (isOpen)
        {
            navigationController.CommitPaneChrome(isOpen);
        }

        if (!animate)
        {
            StopNavigationPaneAnimations();
            ApplyNavigationPaneColumnConstraints(isOpen, state);
            SetNavigationPaneWidth(paneWidth, state.Direction);
            navigationController.CommitPaneChrome(isOpen);
            UpdateNavigationPaneSplitterState(state);
            return;
        }

        NavigationPaneSplitter.IsEnabled = false;
        NavigationPaneSplitter.Visibility = Visibility.Collapsed;
        var paneColumn = GetNavigationPaneColumn(state.Direction);
        var committedWidth =
            paneColumn.ActualWidth > 0 ? paneColumn.ActualWidth : paneColumn.Width.Value;

        motionService.AnimateNavigationPane(
            navigationPaneTransition,
            new NavigationPaneTransitionTarget(
                WorkAreaGrid,
                NavigationPane.TransitionHost,
                ContentHost.LayoutHost,
                state.Direction,
                contentLayoutService.Current.IsCenterContentEnabled
                    ? GetCenteredContentTransitionHosts()
                    : null
            ),
            committedWidth,
            paneWidth,
            CoerceOpenPaneWidth(state.OpenWidth, state),
            Math.Abs(state.OpenWidth - state.ClosedWidth),
            () =>
            {
                ApplyNavigationPaneColumnConstraints(isOpen, state);
                SetNavigationPaneWidth(paneWidth, state.Direction);
                navigationController.CommitPaneChrome(isOpen);
                UpdateNavigationPaneSplitterState(state);
            }
        );
    }

    private IReadOnlyList<FrameworkElement> GetCenteredContentTransitionHosts()
    {
        var hosts = ContentHost.CenteredHosts.ToList();
        if (
            ContentHost.CurrentPage is WpfPage page
            && CenteredPageContentLayout.FindPresenter(page) is { } presenter
        )
        {
            hosts.Add(presenter);
        }

        return hosts;
    }

    private void NavigationPaneSplitter_DragCompleted(object sender, DragCompletedEventArgs e)
    {
        var state = navigationController.CurrentPanelState;
        if (!state.IsEnabled || !state.IsOpen)
        {
            return;
        }

        var horizontalChange =
            state.Direction == NavigationPanelDirection.Right
                ? -e.HorizontalChange
                : e.HorizontalChange;
        var paneWidth = e.Canceled
            ? navigationPaneDragStartWidth
            : CoerceOpenPaneWidth(navigationPaneDragStartWidth + horizontalChange, state);

        navigationController.RecordOpenWidth(paneWidth);
        ApplyNavigationPaneColumnConstraints(isOpen: true, state);
        SetNavigationPaneWidth(paneWidth, state.Direction);
        RefreshWorkAreaLayout();
    }

    private void NavigationPaneSplitter_DragStarted(object sender, DragStartedEventArgs e)
    {
        var state = navigationController.CurrentPanelState;
        navigationPaneDragStartWidth = CoerceOpenPaneWidth(
            GetNavigationPaneColumn(state.Direction).ActualWidth,
            state
        );
    }

    private void RefreshWorkAreaLayout()
    {
        WorkAreaGrid.InvalidateMeasure();
        WorkAreaGrid.InvalidateArrange();
        WorkAreaGrid.UpdateLayout();
    }

    private void BuildRegionContents(FlourishRegion? changedRegion = null)
    {
        var regions = changedRegion is null
            ? Enum.GetValues<FlourishRegion>()
            : [changedRegion.Value];
        foreach (var region in regions)
        {
            var definitions = shellRegionService.GetContents(region);
            var activeIds = definitions
                .Select(content => content.Id)
                .ToHashSet(StringComparer.Ordinal);
            foreach (
                var removed in regionElementsById
                    .Where(pair =>
                        pair.Value.Definition.Region == region && !activeIds.Contains(pair.Key)
                    )
                    .Select(pair => pair.Key)
                    .ToArray()
            )
            {
                DisposeRegionElement(regionElementsById[removed].Element);
                regionElementsById.Remove(removed);
            }

            var elements = definitions.Select(GetOrCreateRegionElement).ToList();

            SetRegionContent(region, elements);
        }

        UpdateRuntimeSurfaceVisibility();
    }

    private FrameworkElement GetOrCreateRegionElement(FlourishRegionContent content)
    {
        if (
            regionElementsById.TryGetValue(content.Id, out var cached)
            && cached.Definition.Region == content.Region
            && cached.Definition.ContentFactory.Equals(content.ContentFactory)
        )
        {
            regionElementsById[content.Id] = new RegionElementView(content, cached.Element);
            return cached.Element;
        }

        if (cached is not null)
        {
            DisposeRegionElement(cached.Element);
            regionElementsById.Remove(content.Id);
        }

        var element = content.CreateContent(serviceProvider);
        if (element.Parent is not null)
        {
            throw new InvalidOperationException(
                $"The content factory for region {content.Region} returned an element that already has a parent."
            );
        }

        regionElementsById[content.Id] = new RegionElementView(content, element);
        return element;
    }

    private static void DisposeRegionElement(FrameworkElement element)
    {
        if (element is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    private void SetRegionContent(FlourishRegion region, IReadOnlyList<FrameworkElement> elements)
    {
        switch (region)
        {
            case FlourishRegion.TitlebarStart:
            case FlourishRegion.TitlebarCenter:
            case FlourishRegion.TitlebarEnd:
            case FlourishRegion.TitlebarProfile:
                Titlebar.SetRegionContent(region, elements);
                break;
            case FlourishRegion.TitlebarApplicationInfo:
                titleBarController.SetApplicationInfoBody(elements);
                break;
            case FlourishRegion.NavigationHeader:
                NavigationPane.SetRegionContent(region, elements);
                break;
            case FlourishRegion.NavigationFooter:
                NavigationPane.SetRegionContent(region, elements);
                break;
            case FlourishRegion.ContentHeader:
            case FlourishRegion.ContentFooter:
            case FlourishRegion.ContentOverlay:
            case FlourishRegion.ToolbarStart:
            case FlourishRegion.ToolbarEnd:
                ContentHost.SetRegionContent(region, elements);
                break;
            case FlourishRegion.FooterStart:
                statusSurfaceController.SetRegionContent(isStart: true, elements);
                break;
            case FlourishRegion.FooterEnd:
                statusSurfaceController.SetRegionContent(isStart: false, elements);
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(region),
                    region,
                    "Unknown shell region."
                );
        }
    }

    private void StopNavigationPaneAnimations()
    {
        navigationPaneTransition.Cancel();
    }

    private void NavigationController_LayoutRequested(
        object? sender,
        NavigationLayoutRequestedEventArgs e
    )
    {
        DispatchRuntimeChange(() =>
        {
            if (!e.Animate)
            {
                StopNavigationPaneAnimations();
            }
            ApplyNavigationPanelPlacement(e.State.Direction);
            ApplyNavigationPaneState(e.State, e.Animate);
        });
    }

    private void ShellRegionService_Changed(object? sender, FlourishShellRegionChangedEventArgs e)
    {
        DispatchRuntimeChange(() => BuildRegionContents(e.Region));
    }

    private void TitleBarController_StateChanged(
        object? sender,
        ShellTitleBarStateChangedEventArgs e
    )
    {
        Title = e.Current.ApplicationTitle;
        navigationController.ApplyTitleBarState(e.Current);
        if (e.Previous.IsEnabled != e.Current.IsEnabled)
        {
            ApplyTitleBarFeatureState(refreshFrame: true);
        }
    }

    private void TitleBarController_ProjectChanged(
        object? sender,
        FlourishProjectsChangedEventArgs e
    ) => projectSaveCommandRegistration.NotifyCanExecuteChanged();

    private void TitleBarController_IconChanged(
        object? sender,
        ShellTitleBarIconChangedEventArgs e
    ) => Icon = e.Icon;

    private void FontService_Changed(object? sender, FlourishFontChangedEventArgs e)
    {
        if (e.ChangeKind == FlourishFontChangeKind.Icon)
        {
            return;
        }

        var affectedPageType = e.AffectedPageType;

        DispatchRuntimeChange(() =>
        {
            var contentPageType = navigationService.CurrentSourcePageType;
            if (
                ContentHost.CurrentPage is WpfPage contentPage
                && (
                    affectedPageType is null
                    || (contentPageType ?? contentPage.GetType()) == affectedPageType
                )
            )
            {
                fontService.ApplyToPage(contentPage, contentPageType ?? contentPage.GetType());
            }
        });
    }

    private void MotionService_Changed(object? sender, FlourishMotionChangedEventArgs e)
    {
        var cancelPageTransition =
            pageTransition.IsActive
            && (!e.CanAnimate || e.Current.PageTransition == FlourishPageTransition.None);
        var resetNavigationPane =
            navigationPaneTransition.IsActive
            && (
                !e.CanAnimate
                || e.Current.NavigationPanelTransition == FlourishNavigationPanelTransition.None
            );
        if (!cancelPageTransition && !resetNavigationPane)
        {
            return;
        }

        DispatchRuntimeChange(() =>
        {
            if (cancelPageTransition)
            {
                pageTransition.Cancel();
            }

            if (resetNavigationPane)
            {
                ApplyNavigationPaneState(navigationController.CurrentPanelState);
            }
        });
    }

    private void DispatchRuntimeChange(Action action)
    {
        void ExecuteIfActive()
        {
            if (isShellClosed || Dispatcher.HasShutdownStarted || Dispatcher.HasShutdownFinished)
            {
                return;
            }

            action();
        }

        if (Dispatcher.CheckAccess())
        {
            ExecuteIfActive();
            return;
        }

        if (isShellClosed || Dispatcher.HasShutdownStarted || Dispatcher.HasShutdownFinished)
        {
            return;
        }

        _ = Dispatcher.BeginInvoke(DispatcherPriority.DataBind, new Action(ExecuteIfActive));
    }

    private void UpdateRuntimeSurfaceVisibility()
    {
        toolbarController.RefreshVisibility();
        statusSurfaceController.RefreshVisibility();
    }

    private TextBlock CreateIconContent(string iconGlyph)
    {
        var icon = new TextBlock
        {
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            Text = iconGlyph,
            TextAlignment = System.Windows.TextAlignment.Center,
        };
        BindIconTypography(icon, "FlourishFontSizeIcon");
        return icon;
    }

    private static void BindIconTypography(TextBlock textBlock, string? sizeResourceKey = null)
    {
        textBlock.SetResourceReference(TextBlock.FontFamilyProperty, "FlourishIconFontFamily");
        textBlock.TextAlignment = System.Windows.TextAlignment.Center;
        textBlock.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;

        if (sizeResourceKey is not null)
        {
            BindTextSize(textBlock, sizeResourceKey);
            textBlock.SetResourceReference(TextBlock.LineHeightProperty, sizeResourceKey);
        }
    }

    private static void BindTextSize(TextBlock textBlock, string sizeResourceKey)
    {
        textBlock.SetResourceReference(TextBlock.FontSizeProperty, sizeResourceKey);
    }

    private void RootFrame_Navigated(object? sender, FlourishNavigatedEventArgs e)
    {
        CenteredPageContentLayout.Apply(e.Page, GetCenteredContentWidth());

        fontService.ApplyToPage(e.Page, e.SourcePageType);
        navigationController.OnNavigated(e);
        toolbarController.SetPage(e.SourcePageType);
        motionService.AnimatePageEntrance(
            pageTransition,
            new PageTransitionTarget(ContentHost.TransitionHost)
        );
    }

    private void ApplyContentLayoutOptions()
    {
        var maximumWidth = GetCenteredContentWidth() ?? double.PositiveInfinity;

        ContentHost.ApplyCenteredLayout(maximumWidth);
    }

    private double? GetCenteredContentWidth()
    {
        var layout = contentLayoutService.Current;
        return layout.IsCenterContentEnabled ? layout.ContentWidth : null;
    }

    private void ContentLayoutService_Changed(
        object? sender,
        FlourishContentLayoutChangedEventArgs e
    )
    {
        DispatchRuntimeChange(() =>
        {
            StopNavigationPaneAnimations();
            ApplyContentLayoutOptions();
            if (ContentHost.CurrentPage is WpfPage page)
            {
                CenteredPageContentLayout.Apply(
                    page,
                    e.Current.IsCenterContentEnabled ? e.Current.ContentWidth : null
                );
            }

            WorkAreaGrid.InvalidateMeasure();
            WorkAreaGrid.InvalidateArrange();
        });
    }

    private void Titlebar_DragRequested(object? sender, EventArgs e)
    {
        DragMove();
    }

    private void Titlebar_ToggleWindowStateRequested(object? sender, EventArgs e)
    {
        ToggleWindowState();
    }

    private void Titlebar_MinimizeRequested(object? sender, EventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void Titlebar_MaximizeRequested(object? sender, EventArgs e)
    {
        if (ResizeMode is not (ResizeMode.CanResize or ResizeMode.CanResizeWithGrip))
        {
            return;
        }

        ToggleWindowState();
    }

    private async void Titlebar_CloseRequested(object? sender, EventArgs e)
    {
        try
        {
            await windowCloseService.RequestCloseAsync(WindowCloseRequestReason.TitleBar);
        }
        catch (Exception error)
        {
            ShowCloseFailure(error);
        }
    }

    private void Titlebar_ThemeToggleRequested(object? sender, EventArgs e)
    {
        themeService.ToggleTheme();
    }

    private void ThemeService_ThemeChanged(object? sender, FlourishThemeChangedEventArgs e)
    {
        ApplyThemeState();
    }

    private void ApplyThemeState()
    {
        materialEffectService.SetDarkMode(this, themeService.IsDark);
        Titlebar.SetThemeToggleState(themeService.CurrentTheme, themeService.EffectiveTheme);
    }

    private async Task<bool> ConfirmCloseRequestAsync(CancellationToken cancellationToken)
    {
        FlourishMessageOption[] closeOptions =
        [
            new("no", localizationService.Get(FlourishLocaleKeys.MessageBoxNo))
            {
                IsDefault = true,
                IsCancel = true,
            },
            new("yes", localizationService.Get(FlourishLocaleKeys.MessageBoxYes))
            {
                IsPrimary = true,
            },
        ];

        return (
                await messageService.ShowAsync(
                    this,
                    localizationService.Get(FlourishLocaleKeys.WindowClosePrompt),
                    localizationService.Get(FlourishLocaleKeys.WindowCloseTitle),
                    closeOptions,
                    MessageBoxImage.Question,
                    cancellationToken: cancellationToken
                )
            )?.Id == "yes";
    }

    private async Task<bool> ConfirmBackgroundTasksCloseRequestAsync(
        int activeTaskCount,
        CancellationToken cancellationToken
    )
    {
        FlourishMessageOption[] closeOptions =
        [
            new(
                "keep-running",
                localizationService.Get(FlourishLocaleKeys.WindowBackgroundTasksKeepRunning)
            )
            {
                IsDefault = true,
                IsCancel = true,
            },
            new(
                "stop-and-exit",
                localizationService.Get(FlourishLocaleKeys.WindowBackgroundTasksStopAndExit)
            )
            {
                IsPrimary = true,
            },
        ];

        return (
                await messageService.ShowAsync(
                    this,
                    localizationService.Format(
                        FlourishLocaleKeys.WindowBackgroundTasksClosePrompt,
                        activeTaskCount
                    ),
                    localizationService.Get(FlourishLocaleKeys.WindowBackgroundTasksCloseTitle),
                    closeOptions,
                    MessageBoxImage.Warning,
                    cancellationToken: cancellationToken
                )
            )?.Id == "stop-and-exit";
    }

    private void CancelActiveBackgroundTasks()
    {
        statusSurfaceController.CancelActiveTasks();
    }

    private async ValueTask<bool> RequestCloseCoreAsync(
        WindowCloseRequestReason reason,
        CancellationToken cancellationToken
    )
    {
        if (!Dispatcher.CheckAccess())
        {
            return await Dispatcher
                .InvokeAsync(
                    () => RequestCloseCoreAsync(reason, cancellationToken).AsTask(),
                    DispatcherPriority.Send,
                    cancellationToken
                )
                .Task.Unwrap();
        }

        if (closeRequestPending || isShellClosed)
        {
            return false;
        }

        closeRequestPending = true;
        try
        {
            if (
                reason != WindowCloseRequestReason.Tray
                && windowCloseService.Behavior == WindowCloseBehavior.MinimizeToTray
                && trayIconService.MinimizeToTray()
            )
            {
                return true;
            }

            if (
                reason != WindowCloseRequestReason.Tray
                && windowCloseService.Behavior == WindowCloseBehavior.Prompt
            )
            {
                var activeTaskCount = statusSurfaceController.ActiveTaskCount;
                if (activeTaskCount > 0)
                {
                    if (
                        !await ConfirmBackgroundTasksCloseRequestAsync(
                            activeTaskCount,
                            cancellationToken
                        )
                    )
                    {
                        return false;
                    }

                    CancelActiveBackgroundTasks();
                }
                else if (!await ConfirmCloseRequestAsync(cancellationToken))
                {
                    return false;
                }
            }

            allowClose = true;
            Close();
            return true;
        }
        finally
        {
            closeRequestPending = false;
        }
    }

    private void ShellWindow_Closing(object? sender, CancelEventArgs e)
    {
        if (allowClose || isShellClosed)
        {
            return;
        }

        e.Cancel = true;
        if (!closeRequestPending)
        {
            _ = RequestWindowCloseAsync();
        }
    }

    private async Task RequestWindowCloseAsync()
    {
        try
        {
            await windowCloseService.RequestCloseAsync(WindowCloseRequestReason.Window);
        }
        catch (Exception error)
        {
            ShowCloseFailure(error);
        }
    }

    private void ShowCloseFailure(Exception error)
    {
        notificationService.Upsert(
            new FlourishNotification(
                "flourish.close.error",
                "Close request failed",
                error.Message,
                FlourishNotificationSeverity.Error,
                Duration: TimeSpan.FromSeconds(8)
            )
        );
    }

    private void ShellWindow_Closed(object? sender, EventArgs e)
    {
        isShellClosed = true;

        projectSaveShortcutRegistration.Dispose();
        projectSaveCommandRegistration.Dispose();
        projectCloseGuardRegistration.Dispose();
        titleBarController.Opening -= TitleBarController_Opening;
        titleBarController.StateChanged -= TitleBarController_StateChanged;
        titleBarController.ProjectChanged -= TitleBarController_ProjectChanged;
        titleBarController.IconChanged -= TitleBarController_IconChanged;
        titleBarController.Dispose();
        projectSelectorController.Dispose();
        navigationController.LayoutRequested -= NavigationController_LayoutRequested;
        navigationController.Dispose();
        shellRegionService.Changed -= ShellRegionService_Changed;
        notificationController.Dispose();
        profileController.Opening -= ProfileController_Opening;
        profileController.PlacementRequested -= ProfileController_PlacementRequested;
        profileController.Dispose();
        contentLayoutService.Changed -= ContentLayoutService_Changed;
        fontService.Changed -= FontService_Changed;
        motionService.Changed -= MotionService_Changed;
        navigationService.Navigated -= RootFrame_Navigated;
        Titlebar.MinimizeRequested -= Titlebar_MinimizeRequested;
        Titlebar.MaximizeRequested -= Titlebar_MaximizeRequested;
        Titlebar.CloseRequested -= Titlebar_CloseRequested;
        Titlebar.DragRequested -= Titlebar_DragRequested;
        Titlebar.ToggleWindowStateRequested -= Titlebar_ToggleWindowStateRequested;
        Titlebar.ThemeToggleRequested -= Titlebar_ThemeToggleRequested;
        statusSurfaceController.OpenRequested -= StatusSurfaceController_OpenRequested;
        statusSurfaceController.CloseRequested -= StatusSurfaceController_CloseRequested;
        statusSurfaceController.VisualStateChanged -= StatusSurfaceController_VisualStateChanged;
        statusSurfaceController.AnchorChanged -= StatusSurfaceController_AnchorChanged;
        statusSurfaceController.ContentFocusInvalidated -=
            StatusSurfaceController_ContentFocusInvalidated;
        statusSurfaceController.Dispose();
        themeService.Changed -= ThemeService_ThemeChanged;
        themeService.Detach(this);
        materialEffectService.Detach(this);
        pageTransition.Cancel();
        StopNavigationPaneAnimations();
        windowCloseService.Detach();
        toolbarController.Dispose();
        foreach (var regionElement in regionElementsById.Values)
        {
            DisposeRegionElement(regionElement.Element);
        }

        regionElementsById.Clear();
    }

    private void MainWindow_StateChanged(object? sender, EventArgs e)
    {
        shellWindowFrame.UpdateWindowState();
        windowFrameFixService.RefreshFrame(this, titleBarService.Current.IsEnabled);
        Titlebar.SetMaximized(WindowState == WindowState.Maximized);
    }

    private void ToggleWindowState()
    {
        if (ResizeMode is not (ResizeMode.CanResize or ResizeMode.CanResizeWithGrip))
        {
            return;
        }

        WindowState =
            WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }
}
