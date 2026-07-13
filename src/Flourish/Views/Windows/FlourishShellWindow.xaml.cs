using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.ComponentModel;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;
using ArkheideSystem.Flourish.Internal.Imaging;
using ArkheideSystem.Flourish.Internal.Interaction;
using ArkheideSystem.Flourish.Controls;
using ArkheideSystem.Flourish.Services;
using Microsoft.Extensions.DependencyInjection;
using Button = ArkheideSystem.Flourish.Controls.FlourishButton;
using ListBox = ArkheideSystem.Flourish.Controls.FlourishListBox;
using Orientation = System.Windows.Controls.Orientation;
using TextBlock = ArkheideSystem.Flourish.Controls.FlourishTextBlock;
using TextBoxBase = System.Windows.Controls.Primitives.TextBoxBase;
using WpfPage = System.Windows.Controls.Page;
using WpfPanel = System.Windows.Controls.Panel;
using WpfComboBox = System.Windows.Controls.ComboBox;

namespace ArkheideSystem.Flourish.Views.Windows;

internal partial class FlourishShellWindow : Window
{
    private readonly INavigationService navigationService;
    private readonly IFrameNavigationService frameNavigationService;
    private readonly NavigationPanelService navigationPanelService;
    private readonly NavigationMenuService navigationMenuService;
    private readonly FlourishToolbarService toolbarService;
    private readonly FlourishStatusService statusService;
    private readonly ShellRegionService shellRegionService;
    private readonly IBackgroundTaskService backgroundTaskService;
    private readonly IMessageService messageService;
    private readonly NotificationService notificationService;
    private readonly TrayIconService trayIconService;
    private readonly CommandParser commandParser;
    private readonly ShortcutService shortcutService;
    private readonly FontService fontService;
    private readonly MaterialEffectService materialEffectService;
    private readonly ThemeService themeService;
    private readonly FlourishMotionService motionService;
    private readonly TitleBarService titleBarService;
    private readonly TitleBarSearchService titleBarSearchService;
    private readonly WindowService windowService;
    private readonly WindowCloseService windowCloseService;
    private readonly ProfileFlyoutService profileFlyoutService;
    private readonly ShellFeatureService shellFeatureService;
    private readonly WindowFrameFixService windowFrameFixService;
    private readonly IProfileService profileService;
    private readonly FlourishLocalizationService localizationService;
    private readonly IServiceProvider serviceProvider;
    private readonly FlourishShellOptions options;
    private readonly FlourishShellWindowFrame shellWindowFrame;
    private readonly Dictionary<string, FlourishNavigationItem> navigationItemsByKey = new(
        StringComparer.Ordinal
    );
    private readonly Dictionary<Type, FlourishNavigationItem> navigationItemsByPage = [];
    private readonly Dictionary<NavigationTreeKey, FlourishNavigationItem> navigationParentsByKey =
        [];
    private readonly Dictionary<
        NavigationTreeKey,
        List<FlourishNavigationItem>
    > navigationChildrenByParentKey = [];
    private readonly Dictionary<Type, IReadOnlyList<Button>> toolbarButtonsByPageType = [];
    private readonly Dictionary<string, RegionElementView> regionElementsById = new(
        StringComparer.Ordinal
    );
    private readonly Dictionary<Guid, BackgroundTaskIconView> backgroundTaskIconsById = [];
    private readonly Dictionary<Guid, BackgroundTaskRowView> backgroundTaskRowsById = [];
    private readonly object backgroundTaskRefreshGate = new();
    private readonly NavigationPaneTransitionController navigationPaneTransition = new();
    private readonly PageTransitionController pageTransition = new();
    private readonly TitleBarLogoLoadCoordinator titleBarLogoLoadCoordinator = new(
        TitleBarVisualAssets.LoadLogoAsync
    );
    private readonly DispatcherTimer backgroundTaskRefreshTimer;
    private IReadOnlyList<Button>? defaultToolbarButtons;
    private FlourishNavigationItem? firstNavigationItem;
    private Type? activeToolbarPageType;
    private FlourishNavigationItem? selectedNavigationItem;
    private FlourishNavigationItem? activeChildParentItem;
    private bool isDefaultToolbarActive;
    private bool isPaneOpen = true;
    private bool suppressNavigationSelection;
    private double navigationPaneDragStartWidth;
    private FrameworkElement? statusFlyoutAnchor;
    private Guid? statusFlyoutAnchorTaskId;
    private StatusFlyoutKind statusFlyoutKind;
    private IReadOnlyList<FlourishBackgroundTaskInfo> backgroundTasks = [];
    private IReadOnlyList<FlourishBackgroundTaskInfo> pendingBackgroundTasks = [];
    private TextBlock? backgroundTaskEmptyText;
    private IInputElement? statusFlyoutRestoreFocusTarget;
    private bool backgroundTaskRefreshPending;
    private bool backgroundTaskRefreshLoopActive;
    private volatile bool isShellClosed;
    private bool statusFlyoutOpenedWithFocus;
    private bool allowClose;
    private bool closeRequestPending;
    private bool isProfileServiceSubscribed;
    private Type? materializedProfileConfiguredPageType;
    private ImageSource? currentTitleBarLogoSource;
    private string currentTitleBarLogoFallbackText = "F";
    private bool isTitleBarLogoVisible;

    private readonly record struct NavigationTreeKey(bool IsFixed, int GroupId, int RelationshipId);

    private enum StatusFlyoutKind
    {
        None,
        BackgroundTasks,
        System,
    }

    private sealed class BackgroundTaskIconView(
        Button button,
        TextBlock icon,
        TextBlock toolTipName,
        TextBlock toolTipDescription,
        TextBlock toolTipState
    )
    {
        public Button Button { get; } = button;

        public TextBlock Icon { get; } = icon;

        public TextBlock ToolTipName { get; } = toolTipName;

        public TextBlock ToolTipDescription { get; } = toolTipDescription;

        public TextBlock ToolTipState { get; } = toolTipState;
    }

    private sealed class BackgroundTaskRowView(
        Border container,
        TextBlock icon,
        TextBlock name,
        TextBlock description,
        TextBlock state,
        Button cancelButton
    )
    {
        public Border Container { get; } = container;

        public TextBlock Icon { get; } = icon;

        public TextBlock Name { get; } = name;

        public TextBlock Description { get; } = description;

        public TextBlock State { get; } = state;

        public Button CancelButton { get; } = cancelButton;
    }

    private sealed record RegionElementView(
        FlourishRegionContent Definition,
        FrameworkElement Element
    );

    public FlourishShellWindow(
        INavigationService navigationService,
        IFrameNavigationService frameNavigationService,
        NavigationPanelService navigationPanelService,
        NavigationMenuService navigationMenuService,
        FlourishToolbarService toolbarService,
        FlourishStatusService statusService,
        ShellRegionService shellRegionService,
        IBackgroundTaskService backgroundTaskService,
        IMessageService messageService,
        NotificationService notificationService,
        TrayIconService trayIconService,
        CommandParser commandParser,
        ShortcutService shortcutService,
        FontService fontService,
        MaterialEffectService materialEffectService,
        ThemeService themeService,
        FlourishMotionService motionService,
        TitleBarService titleBarService,
        TitleBarSearchService titleBarSearchService,
        WindowService windowService,
        WindowCloseService windowCloseService,
        ProfileFlyoutService profileFlyoutService,
        ShellFeatureService shellFeatureService,
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
        backgroundTaskRefreshTimer = new DispatcherTimer(
            DispatcherPriority.Background,
            Dispatcher
        )
        {
            Interval = TimeSpan.FromMilliseconds(33),
        };
        backgroundTaskRefreshTimer.Tick += BackgroundTaskRefreshTimer_Tick;

        this.navigationService = navigationService;
        this.frameNavigationService = frameNavigationService;
        this.navigationPanelService = navigationPanelService;
        this.navigationMenuService = navigationMenuService;
        this.toolbarService = toolbarService;
        this.statusService = statusService;
        this.shellRegionService = shellRegionService;
        this.backgroundTaskService = backgroundTaskService;
        this.messageService = messageService;
        this.notificationService = notificationService;
        this.trayIconService = trayIconService;
        this.commandParser = commandParser;
        this.shortcutService = shortcutService;
        this.fontService = fontService;
        this.materialEffectService = materialEffectService;
        this.themeService = themeService;
        this.motionService = motionService;
        this.titleBarService = titleBarService;
        this.titleBarSearchService = titleBarSearchService;
        this.windowService = windowService;
        this.windowCloseService = windowCloseService;
        this.profileFlyoutService = profileFlyoutService;
        this.shellFeatureService = shellFeatureService;
        this.windowFrameFixService = windowFrameFixService;
        this.profileService = profileService;
        this.localizationService = localizationService;
        this.serviceProvider = serviceProvider;
        this.options = options;

        Titlebar.ApplyLocale(localizationService);
        ApplyOptions();
        windowService.Attach(this);
        windowCloseService.Attach(RequestCloseCoreAsync);
        BuildRegionContents();
        ConfigureProfileSurface();
        BuildToolbarItems();
        BuildNavigationItems();
        BuildStatusItems();
        BuildNotifications(notificationService.ActiveNotifications);
        navigationPanelService.Changed += NavigationPanelService_Changed;
        navigationMenuService.Changed += NavigationMenuService_Changed;
        toolbarService.Changed += ToolbarService_Changed;
        statusService.Changed += StatusService_Changed;
        shellRegionService.Changed += ShellRegionService_Changed;
        titleBarService.Changed += TitleBarService_Changed;
        titleBarSearchService.StateChanged += TitleBarSearchService_StateChanged;
        notificationService.NotificationsChanged += NotificationService_NotificationsChanged;
        profileFlyoutService.Changed += ProfileFlyoutService_Changed;
        shellFeatureService.Changed += ShellFeatureService_Changed;
        localizationService.Changed += LocalizationService_Changed;
        fontService.Changed += FontService_Changed;
        motionService.Changed += MotionService_Changed;
        commandParser.Changed += CommandParser_Changed;
        commandParser.CanExecuteChanged += CommandParser_CanExecuteChanged;
        backgroundTaskService.TasksChanged += BackgroundTaskService_TasksChanged;
        RefreshBackgroundTaskStatus(backgroundTaskService.ActiveTasks);
        AttachTitlebarEvents();

        themeService.ThemeChanged += ThemeService_ThemeChanged;
        StateChanged += MainWindow_StateChanged;
        Closing += ShellWindow_Closing;
        Closed += ShellWindow_Closed;
        Loaded += ShellWindow_Loaded;
        PreviewKeyDown += ShellWindow_PreviewKeyDown;
        frameNavigationService.Initialize(RootFrame);
        navigationService.Navigated += RootFrame_Navigated;

        NavigateToInitialPage();
    }

    private void ApplyOptions()
    {
        ApplyWindowOptions();
        Title = options.Title;
        RequestTitleBarLogo(
            options.LogoPath,
            options.LogoFallbackText,
            options.IsTitlebarLogoEnabled
        );
        Titlebar.SetTitle(options.Title);
        Titlebar.SetSubtitle(options.Subtitle);
        Titlebar.SetSearchPlaceholder(options.SearchPlaceholder);
        Titlebar.ConfigureVisibility(
            options.IsTitlebarSearchEnabled,
            IsBreadcrumbFeatureEnabled(),
            options.IsTitlebarNavigationToggleEnabled && options.IsNavigationPanelEnabled,
            options.IsTitlebarLogoEnabled,
            options.IsTitlebarTitleEnabled,
            options.IsTitlebarSubtitleEnabled,
            options.IsTitlebarThemeToggleEnabled && options.IsThemeEnabled,
            options.IsProfileEnabled && options.IsTitlebarProfileEnabled
        );
        UpdateStatusBarVisibility();
        AutomationProperties.SetName(
            SystemStatusButton,
            localizationService.Get(FlourishLocaleKeys.SystemStatusTitle)
        );
        NavigationPaneBorder.Visibility = options.IsNavigationPanelEnabled
            ? Visibility.Visible
            : Visibility.Collapsed;
        BreadcrumbHost.Visibility = IsBreadcrumbFeatureEnabled()
            ? Visibility.Visible
            : Visibility.Collapsed;
        UpdateTitlebarBreadcrumbNavigation();

        NormalizeNavigationPaneWidths();
        ApplyNavigationPanelPlacement();
        isPaneOpen = navigationPanelService.Current.IsOpen;
        ApplyNavigationPaneState();
        windowFrameFixService.Attach(this, options.IsTitlebarEnabled);
        materialEffectService.Attach(
            this,
            options.IsMaterialEffectEnabled ? options.MaterialEffect : MaterialEffect.None,
            "AppBackgroundBrush"
        );
        themeService.Attach(this);
        ApplyThemeState();
        trayIconService.Initialize(this, options.Title);
    }

    private void ConfigureProfileSurface()
    {
        var state = profileFlyoutService.Current;
        if (
            options.IsTitlebarEnabled
            && state.IsEnabled
            && options.IsTitlebarProfileEnabled
        )
        {
            Titlebar.SetProfile(profileService.CurrentProfile);
            if (!isProfileServiceSubscribed)
            {
                profileService.ProfileChanged += ProfileService_ProfileChanged;
                isProfileServiceSubscribed = true;
            }
        }
        else if (isProfileServiceSubscribed)
        {
            profileService.ProfileChanged -= ProfileService_ProfileChanged;
            isProfileServiceSubscribed = false;
        }

        ApplyProfileFlyoutState(state);
    }

    private void EnsureProfileContent(FlourishProfileFlyoutState state)
    {
        if (
            materializedProfileConfiguredPageType == state.ContentPageType
            && ProfileFrame.Content is WpfPage
        )
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
        if (!ProfileFrame.Navigate(page))
        {
            throw new InvalidOperationException(
                $"Navigation to profile page {state.ContentPageType.FullName} was rejected."
            );
        }

        materializedProfileConfiguredPageType = state.ContentPageType;
    }

    private async void ShellWindow_Loaded(object sender, RoutedEventArgs e)
    {
        Loaded -= ShellWindow_Loaded;
        if (!options.IsProfileEnabled)
        {
            return;
        }

        try
        {
            await profileService.InitializeAsync();
            Titlebar.SetProfile(profileService.CurrentProfile);
        }
        catch (Exception error)
        {
            System.Diagnostics.Debug.WriteLine(
                $"Flourish profile initialization failed: {error}"
            );
        }
    }

    private void ProfileService_ProfileChanged(object? sender, ProfileChangedEventArgs e)
    {
        if (!Dispatcher.CheckAccess())
        {
            Dispatcher.Invoke(() => Titlebar.SetProfile(e.Profile));
            return;
        }

        Titlebar.SetProfile(e.Profile);
    }

    private void ApplyProfileFlyoutState(FlourishProfileFlyoutState state)
    {
        ProfileOverlay.Visibility = Visibility.Collapsed;
        if (!state.IsEnabled || !options.IsTitlebarEnabled || !options.IsTitlebarProfileEnabled)
        {
            profileFlyoutService.SynchronizeVisibility(false);
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
            profileFlyoutService.SynchronizeVisibility(false);
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

        ProfileOverlay.Visibility = Visibility.Visible;
        CloseStatusFlyout(restoreFocus: false);
        Dispatcher.BeginInvoke(new Action(UpdateProfileCardPosition));
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

        if (options.WindowTextFormattingMode is { } textFormattingMode)
        {
            TextOptions.SetTextFormattingMode(this, textFormattingMode);
        }

        if (options.WindowTextRenderingMode is { } textRenderingMode)
        {
            TextOptions.SetTextRenderingMode(this, textRenderingMode);
        }

        if (options.WindowSnapsToDevicePixels is { } snapsToDevicePixels)
        {
            SnapsToDevicePixels = snapsToDevicePixels;
        }

        if (options.WindowUseLayoutRounding is { } useLayoutRounding)
        {
            UseLayoutRounding = useLayoutRounding;
        }

        if (options.WindowLeft is { } left)
        {
            Left = left;
        }

        if (options.WindowTop is { } top)
        {
            Top = top;
        }

        WindowState = options.WindowState;
        ApplyTitleBarFeatureState();
        Titlebar.SetMaximizeEnabled(ResizeMode is ResizeMode.CanResize or ResizeMode.CanResizeWithGrip);
    }

    private void ApplyTitleBarFeatureState(bool refreshFrame = false)
    {
        var useCustomFrame = options.IsTitlebarEnabled;
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
            windowFrameFixService.ApplyFrameTransition(
                this,
                useCustomFrame,
                ApplyFrameAndSurface
            );
        }
        else
        {
            ApplyFrameAndSurface();
        }

        Titlebar.SetMaximized(WindowState == WindowState.Maximized);
        if (!options.IsTitlebarEnabled || !titleBarSearchService.Current.FocusRequested)
        {
            return;
        }

        Dispatcher.BeginInvoke(
            new Action(() =>
            {
                if (
                    options.IsTitlebarEnabled
                    && titleBarSearchService.Current.FocusRequested
                )
                {
                    Titlebar.FocusSearchBox();
                    titleBarSearchService.AcknowledgeFocusRequest();
                }
            })
        );
    }

    private void AttachTitlebarEvents()
    {
        Titlebar.BackRequested += Titlebar_BackRequested;
        Titlebar.ForwardRequested += Titlebar_ForwardRequested;
        Titlebar.NavigationToggleRequested += Titlebar_NavigationToggleRequested;
        Titlebar.MinimizeRequested += Titlebar_MinimizeRequested;
        Titlebar.MaximizeRequested += Titlebar_MaximizeRequested;
        Titlebar.CloseRequested += Titlebar_CloseRequested;
        Titlebar.DragRequested += Titlebar_DragRequested;
        Titlebar.ToggleWindowStateRequested += Titlebar_ToggleWindowStateRequested;
        Titlebar.ThemeToggleRequested += Titlebar_ThemeToggleRequested;
        Titlebar.ProfileToggleRequested += Titlebar_ProfileToggleRequested;
        Titlebar.SearchTextChanged += Titlebar_SearchTextChanged;
    }

    private void Titlebar_ProfileToggleRequested(object? sender, EventArgs e)
    {
        if (
            !options.IsTitlebarEnabled
            || !options.IsProfileEnabled
            || !options.IsTitlebarProfileEnabled
        )
        {
            return;
        }

        CloseStatusFlyout(restoreFocus: false);
        profileFlyoutService.Toggle();
    }

    private void CloseProfileOverlay()
    {
        profileFlyoutService.Hide();
    }

    private void ProfileOverlay_PreviewMouseLeftButtonDown(
        object sender,
        MouseButtonEventArgs e
    )
    {
        var position = e.GetPosition(ProfileCard);
        if (
            position.X >= 0
            && position.Y >= 0
            && position.X <= ProfileCard.ActualWidth
            && position.Y <= ProfileCard.ActualHeight
        )
        {
            return;
        }

        CloseProfileOverlay();
        e.Handled = true;
    }

    private async void ShellWindow_PreviewKeyDown(
        object sender,
        System.Windows.Input.KeyEventArgs e
    )
    {
        if (e.Key == Key.Escape)
        {
            if (StatusFlyoutOverlay.Visibility == Visibility.Visible)
            {
                CloseStatusFlyout();
                e.Handled = true;
                return;
            }

            if (ProfileOverlay.Visibility == Visibility.Visible)
            {
                CloseProfileOverlay();
                e.Handled = true;
                return;
            }
        }

        var key = e.Key == Key.System ? e.SystemKey : e.Key;
        var modifiers = e.KeyboardDevice.Modifiers;
        if (
            ShouldIgnoreShortcutInput(
                key,
                modifiers,
                e.KeyboardDevice.IsKeyDown(Key.RightAlt)
            )
        )
        {
            return;
        }

        var isTextInputFocused = IsTextInputTarget(
            e.KeyboardDevice.FocusedElement ?? e.OriginalSource
        );
        var context = new ShortcutResolutionContext("shell", navigationService.CurrentNavigationKey);
        if (
            !shortcutService.TryResolve(
                key,
                modifiers,
                context,
                isTextInputFocused,
                out var registration
            )
            || registration is null
        )
        {
            return;
        }

        e.Handled = true;
        await shortcutService.ExecuteAsync(registration.Gesture, context);
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
            || (
                isRightAltPressed
                && (modifiers & altGraphModifiers) == altGraphModifiers
            );
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

            current = current is Visual
                ? VisualTreeHelper.GetParent(current) ?? LogicalTreeHelper.GetParent(current)
                : LogicalTreeHelper.GetParent(current);
        }

        return false;
    }

    private static bool IsTextCompositionKey(Key key) =>
        key is Key.KanaMode
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
        key is Key.LeftAlt
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
            ApplyNavigationPaneState();
        }

        UpdateProfileCardPosition();
        UpdateStatusFlyoutPosition();
    }

    private void ProfileCard_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateProfileCardPosition();
    }

    private void ProfileFrame_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (e.NewSize.Width <= 0 || e.NewSize.Height <= 0)
        {
            ProfileFrame.Clip = null;
            return;
        }

        var innerCornerRadius = TryFindResource("FlourishOverlayCornerRadius")
            is CornerRadius cornerRadius
            ? Math.Max(0, cornerRadius.TopLeft)
            : 0;
        ProfileFrame.Clip = new RectangleGeometry(
            new Rect(new System.Windows.Point(), e.NewSize),
            innerCornerRadius,
            innerCornerRadius
        );
    }

    private void UpdateProfileCardPosition()
    {
        const double shadowSafeMargin = 14;
        const double anchorGap = 6;
        if (
            ProfileOverlay.Visibility != Visibility.Visible
            || ShellRootGrid.ActualWidth <= shadowSafeMargin * 2
            || ShellRootGrid.ActualHeight <= shadowSafeMargin * 2
        )
        {
            return;
        }

        var anchor = Titlebar.GetProfileButtonBounds(ShellRootGrid);
        var availableWidth = Math.Max(
            0,
            ShellRootGrid.ActualWidth - shadowSafeMargin * 2
        );
        ProfileCard.MaxWidth = availableWidth;

        var cardWidth = ProfileCard.ActualWidth > 0
            ? Math.Min(ProfileCard.ActualWidth, availableWidth)
            : Math.Min(ProfileCard.Width, availableWidth);
        var desiredLeft = anchor.Left + (anchor.Width - cardWidth) / 2;
        var maximumLeft = Math.Max(
            shadowSafeMargin,
            ShellRootGrid.ActualWidth - cardWidth - shadowSafeMargin
        );
        var left = Math.Clamp(desiredLeft, shadowSafeMargin, maximumLeft);

        var top = Math.Max(shadowSafeMargin, anchor.Bottom + anchorGap);
        ProfileCard.MaxHeight = Math.Max(
            0,
            ShellRootGrid.ActualHeight - top - shadowSafeMargin
        );

        Canvas.SetLeft(ProfileCard, left);
        Canvas.SetTop(ProfileCard, top);
    }

    private void BackgroundTaskService_TasksChanged(
        object? sender,
        FlourishBackgroundTasksChangedEventArgs e
    )
    {
        var shouldStartRefreshLoop = false;
        lock (backgroundTaskRefreshGate)
        {
            if (isShellClosed)
            {
                return;
            }

            pendingBackgroundTasks = e.Tasks.ToArray();
            backgroundTaskRefreshPending = true;
            if (!backgroundTaskRefreshLoopActive)
            {
                backgroundTaskRefreshLoopActive = true;
                shouldStartRefreshLoop = true;
            }
        }

        if (!shouldStartRefreshLoop)
        {
            return;
        }

        if (Dispatcher.HasShutdownStarted || Dispatcher.HasShutdownFinished)
        {
            ResetBackgroundTaskRefreshLoop();
            return;
        }

        try
        {
            Dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(StartBackgroundTaskRefreshTimer)
            );
        }
        catch (InvalidOperationException)
        {
            ResetBackgroundTaskRefreshLoop();
        }
    }

    private void BackgroundTaskRefreshTimer_Tick(object? sender, EventArgs e)
    {
        IReadOnlyList<FlourishBackgroundTaskInfo>? tasks = null;
        lock (backgroundTaskRefreshGate)
        {
            if (backgroundTaskRefreshPending)
            {
                tasks = pendingBackgroundTasks;
                backgroundTaskRefreshPending = false;
            }
            else
            {
                backgroundTaskRefreshLoopActive = false;
                backgroundTaskRefreshTimer.Stop();
            }
        }

        if (tasks is not null)
        {
            // Read the service again on the UI thread so an older cross-thread event can never
            // overwrite a newer state after Dispatcher scheduling reorders notifications.
            RefreshBackgroundTaskStatus(backgroundTaskService.ActiveTasks);
        }
    }

    private void StartBackgroundTaskRefreshTimer()
    {
        lock (backgroundTaskRefreshGate)
        {
            if (
                isShellClosed
                || !backgroundTaskRefreshLoopActive
                || Dispatcher.HasShutdownStarted
            )
            {
                return;
            }
        }

        backgroundTaskRefreshTimer.Start();
    }

    private void ResetBackgroundTaskRefreshLoop()
    {
        lock (backgroundTaskRefreshGate)
        {
            backgroundTaskRefreshPending = false;
            backgroundTaskRefreshLoopActive = false;
            pendingBackgroundTasks = [];
        }
    }

    private void UpdateStatusBarVisibility()
    {
        var hasBackgroundTasks = backgroundTasks.Count > 0;
        var showConfiguredContent = options.IsStatusBarEnabled;
        var showStatusBar = showConfiguredContent || hasBackgroundTasks;

        StatusBarBorder.Visibility = showStatusBar
            ? Visibility.Visible
            : Visibility.Collapsed;
        StatusItemsHost.Visibility =
            showConfiguredContent && StatusItemsHost.Children.Count > 0
                ? Visibility.Visible
                : Visibility.Collapsed;
        SystemStatusButton.Visibility =
            showConfiguredContent
            && (
                statusService.IsLANConnectionStatusEnabled
                || statusService.IsPowerStatusEnabled
            )
                ? Visibility.Visible
                : Visibility.Collapsed;
        FooterStartRegionHost.Visibility =
            showConfiguredContent && FooterStartRegionHost.Children.Count > 0
                ? Visibility.Visible
                : Visibility.Collapsed;
        FooterEndRegionHost.Visibility =
            showConfiguredContent && FooterEndRegionHost.Children.Count > 0
                ? Visibility.Visible
                : Visibility.Collapsed;

        if (!showStatusBar && StatusFlyoutOverlay.Visibility == Visibility.Visible)
        {
            CloseStatusFlyout(restoreFocus: false);
        }
    }

    private static void SynchronizePanelChildren(
        WpfPanel panel,
        IReadOnlyList<UIElement> desiredChildren
    )
    {
        for (var index = 0; index < desiredChildren.Count; index++)
        {
            var desired = desiredChildren[index];
            if (
                index < panel.Children.Count
                && ReferenceEquals(panel.Children[index], desired)
            )
            {
                continue;
            }

            var existingIndex = panel.Children.IndexOf(desired);
            if (existingIndex >= 0)
            {
                panel.Children.RemoveAt(existingIndex);
            }

            panel.Children.Insert(index, desired);
        }

        while (panel.Children.Count > desiredChildren.Count)
        {
            panel.Children.RemoveAt(panel.Children.Count - 1);
        }
    }

    private void RefreshBackgroundTaskStatus(
        IReadOnlyList<FlourishBackgroundTaskInfo> tasks
    )
    {
        backgroundTasks = tasks.ToArray();
        var runningTasks = backgroundTasks
            .Where(task =>
                task.State
                is FlourishBackgroundTaskState.Running
                    or FlourishBackgroundTaskState.Cancelling
            )
            .ToArray();
        var queuedTasks = backgroundTasks
            .Where(task => task.State == FlourishBackgroundTaskState.Queued)
            .ToArray();

        var runningTaskIds = runningTasks.Select(task => task.Id).ToHashSet();
        var desiredTaskButtons = new List<UIElement>(runningTasks.Length);
        foreach (var task in runningTasks)
        {
            if (!backgroundTaskIconsById.TryGetValue(task.Id, out var iconView))
            {
                iconView = CreateBackgroundTaskIconView(task);
                backgroundTaskIconsById.Add(task.Id, iconView);
            }

            UpdateBackgroundTaskIconView(iconView, task);
            desiredTaskButtons.Add(iconView.Button);
        }

        SynchronizePanelChildren(BackgroundTaskItemsHost, desiredTaskButtons);
        foreach (var taskId in backgroundTaskIconsById.Keys.Except(runningTaskIds).ToArray())
        {
            backgroundTaskIconsById.Remove(taskId);
        }

        BackgroundTaskQueueButton.Visibility = queuedTasks.Length > 0
            ? Visibility.Visible
            : Visibility.Collapsed;
        BackgroundTaskQueueCountText.Text = queuedTasks.Length.ToString();
        AutomationProperties.SetName(
            BackgroundTaskQueueButton,
            localizationService.Format(
                FlourishLocaleKeys.BackgroundTaskWaitingCount,
                queuedTasks.Length
            )
        );

        UpdateStatusBarVisibility();

        if (statusFlyoutKind != StatusFlyoutKind.BackgroundTasks)
        {
            return;
        }

        if (backgroundTasks.Count == 0)
        {
            CloseStatusFlyout(restoreFocus: false);
            return;
        }

        BuildBackgroundTaskFlyoutContent();
        if (
            statusFlyoutAnchorTaskId is { } anchorTaskId
            && backgroundTaskIconsById.TryGetValue(anchorTaskId, out var anchorIcon)
        )
        {
            statusFlyoutAnchor = anchorIcon.Button;
        }
        else
        {
            statusFlyoutAnchorTaskId = null;
            statusFlyoutAnchor = queuedTasks.Length > 0
                ? BackgroundTaskQueueButton
                : desiredTaskButtons.OfType<FrameworkElement>().FirstOrDefault();
            if (statusFlyoutOpenedWithFocus)
            {
                statusFlyoutRestoreFocusTarget = statusFlyoutAnchor;
            }
        }

        if (statusFlyoutOpenedWithFocus && !StatusFlyoutCard.IsKeyboardFocusWithin)
        {
            FocusStatusFlyoutContent();
        }

        UpdateStatusFlyoutPosition();
    }

    private BackgroundTaskIconView CreateBackgroundTaskIconView(
        FlourishBackgroundTaskInfo task
    )
    {
        var icon = new TextBlock
        {
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        BindIconTypography(icon, "FlourishFontSizeCaption");
        var toolTipName = new TextBlock { FontWeight = FontWeights.SemiBold };
        var toolTipDescription = new TextBlock
        {
            Margin = new Thickness(0, 3, 0, 0),
            MaxWidth = 270,
            TextWrapping = TextWrapping.Wrap,
        };
        toolTipDescription.SetResourceReference(
            TextBlock.ForegroundProperty,
            "FlourishNeutralForeground2Brush"
        );
        var toolTipState = new TextBlock { Margin = new Thickness(0, 4, 0, 0) };
        toolTipState.SetResourceReference(TextBlock.ForegroundProperty, "FlourishNeutralForeground2Brush");
        var toolTip = new StackPanel();
        toolTip.Children.Add(toolTipName);
        toolTip.Children.Add(toolTipDescription);
        toolTip.Children.Add(toolTipState);

        var button = new Button
        {
            Content = icon,
            Appearance = FlourishButtonAppearance.Subtle,
            Variant = FlourishButtonVariant.StatusIcon,
            Tag = task.Id,
            ToolTip = new FlourishToolTip { Content = toolTip },
        };
        button.Click += BackgroundTaskIconButton_Click;
        return new BackgroundTaskIconView(
            button,
            icon,
            toolTipName,
            toolTipDescription,
            toolTipState
        );
    }

    private void UpdateBackgroundTaskIconView(
        BackgroundTaskIconView view,
        FlourishBackgroundTaskInfo task
    )
    {
        view.Icon.Text = task.Metadata.IconGlyph ?? "\uE895";
        view.ToolTipName.Text = task.Metadata.Name;
        view.ToolTipDescription.Text = task.Metadata.Description ?? string.Empty;
        view.ToolTipDescription.Visibility = task.Metadata.Description is null
            ? Visibility.Collapsed
            : Visibility.Visible;
        view.ToolTipState.Text = FormatBackgroundTaskState(task);
        AutomationProperties.SetName(
            view.Button,
            $"{task.Metadata.Name}, {GetBackgroundTaskStateText(task.State)}"
        );
    }

    private void BackgroundTaskIconButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement anchor)
        {
            OpenBackgroundTaskFlyout(anchor, focusFlyout: true);
        }
    }

    private void BackgroundTaskQueueButton_MouseEnter(
        object sender,
        System.Windows.Input.MouseEventArgs e
    )
    {
        if (statusFlyoutOpenedWithFocus)
        {
            return;
        }

        OpenBackgroundTaskFlyout(BackgroundTaskQueueButton, focusFlyout: false);
    }

    private void BackgroundTaskQueueButton_Click(object sender, RoutedEventArgs e)
    {
        OpenBackgroundTaskFlyout(BackgroundTaskQueueButton, focusFlyout: true);
    }

    private void OpenBackgroundTaskFlyout(FrameworkElement anchor, bool focusFlyout)
    {
        statusFlyoutAnchorTaskId = anchor.Tag is Guid taskId ? taskId : null;
        BuildBackgroundTaskFlyoutContent();
        OpenStatusFlyout(anchor, StatusFlyoutKind.BackgroundTasks, focusFlyout);
    }

    private void BuildBackgroundTaskFlyoutContent()
    {
        var title = localizationService.Get(FlourishLocaleKeys.BackgroundTaskTitle);
        StatusFlyoutTitle.Text = title;
        AutomationProperties.SetName(StatusFlyoutTitle, title);
        AutomationProperties.SetName(StatusFlyoutCard, title);

        var activeTaskIds = backgroundTasks.Select(task => task.Id).ToHashSet();
        var desiredRows = new List<UIElement>(Math.Max(1, backgroundTasks.Count));
        if (backgroundTasks.Count == 0)
        {
            backgroundTaskEmptyText ??= new TextBlock();
            backgroundTaskEmptyText.Text = localizationService.Get(
                FlourishLocaleKeys.BackgroundTaskNoActiveTasks
            );
            backgroundTaskEmptyText.SetResourceReference(
                TextBlock.ForegroundProperty,
                "FlourishNeutralForeground2Brush"
            );
            desiredRows.Add(backgroundTaskEmptyText);
        }
        else
        {
            foreach (var task in backgroundTasks)
            {
                if (!backgroundTaskRowsById.TryGetValue(task.Id, out var rowView))
                {
                    rowView = CreateBackgroundTaskRowView(task);
                    backgroundTaskRowsById.Add(task.Id, rowView);
                }

                UpdateBackgroundTaskRowView(rowView, task);
                desiredRows.Add(rowView.Container);
            }
        }

        SynchronizePanelChildren(StatusFlyoutContentHost, desiredRows);
        foreach (var taskId in backgroundTaskRowsById.Keys.Except(activeTaskIds).ToArray())
        {
            backgroundTaskRowsById.Remove(taskId);
        }
    }

    private BackgroundTaskRowView CreateBackgroundTaskRowView(
        FlourishBackgroundTaskInfo task
    )
    {
        var row = new Border
        {
            Margin = new Thickness(0, 0, 0, 8),
            Padding = new Thickness(10, 8, 10, 8),
        };
        row.SetResourceReference(Border.BackgroundProperty, "FlourishNeutralBackground2Brush");
        row.SetResourceReference(Border.BorderBrushProperty, "FlourishSurfaceStrokeBrush");
        row.SetResourceReference(
            Border.BorderThicknessProperty,
            "FlourishSurfaceBorderThickness"
        );
        row.SetResourceReference(Border.CornerRadiusProperty, "FlourishSurfaceCornerRadius");

        var layout = new Grid();
        layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(24) });
        layout.ColumnDefinitions.Add(
            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
        );
        layout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var icon = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Top,
        };
        BindIconTypography(icon, "FlourishFontSizeTitlebarIcon");
        icon.SetResourceReference(TextBlock.ForegroundProperty, "FlourishNeutralForeground2Brush");
        layout.Children.Add(icon);

        var details = new StackPanel();
        Grid.SetColumn(details, 1);
        var name = new TextBlock
        {
            FontWeight = FontWeights.SemiBold,
            TextTrimming = TextTrimming.CharacterEllipsis,
        };
        BindTextSize(name, "FlourishFontSizeCaption");
        details.Children.Add(name);
        var description = new TextBlock
        {
            Margin = new Thickness(0, 2, 8, 0),
            MaxWidth = 220,
            TextWrapping = TextWrapping.Wrap,
        };
        description.SetResourceReference(TextBlock.ForegroundProperty, "FlourishNeutralForeground2Brush");
        details.Children.Add(description);

        var state = new TextBlock
        {
            Margin = new Thickness(0, 3, 8, 0),
        };
        BindTextSize(state, "FlourishFontSizeSmall");
        state.SetResourceReference(TextBlock.ForegroundProperty, "FlourishNeutralForeground2Brush");
        details.Children.Add(state);
        layout.Children.Add(details);

        var cancelButton = new Button
        {
            MinWidth = 58,
            Padding = new Thickness(10, 0, 10, 0),
            Tag = task.Id,
            Content = localizationService.Get(FlourishLocaleKeys.BackgroundTaskCancel),
            Variant = FlourishButtonVariant.Action,
        };
        Grid.SetColumn(cancelButton, 2);
        cancelButton.Click += CancelBackgroundTaskButton_Click;
        layout.Children.Add(cancelButton);

        row.Child = layout;
        return new BackgroundTaskRowView(
            row,
            icon,
            name,
            description,
            state,
            cancelButton
        );
    }

    private void UpdateBackgroundTaskRowView(
        BackgroundTaskRowView view,
        FlourishBackgroundTaskInfo task
    )
    {
        view.Icon.Text = task.Metadata.IconGlyph ?? "\uE895";
        view.Name.Text = task.Metadata.Name;
        view.Description.Text = task.Metadata.Description ?? string.Empty;
        view.Description.Visibility = task.Metadata.Description is null
            ? Visibility.Collapsed
            : Visibility.Visible;
        view.State.Text = FormatBackgroundTaskState(task);
        view.CancelButton.Tag = task.Id;
        view.CancelButton.IsEnabled = task.State != FlourishBackgroundTaskState.Cancelling;
        AutomationProperties.SetName(
            view.CancelButton,
            $"{localizationService.Get(FlourishLocaleKeys.BackgroundTaskCancel)} {task.Metadata.Name}"
        );
    }

    private void CancelBackgroundTaskButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: Guid taskId })
        {
            backgroundTaskService.CancelTask(taskId);
        }
    }

    private string FormatBackgroundTaskState(FlourishBackgroundTaskInfo task)
    {
        var text = GetBackgroundTaskStateText(task.State);
        return task.Progress is { } progress ? $"{text} · {progress:P0}" : text;
    }

    private string GetBackgroundTaskStateText(FlourishBackgroundTaskState state)
    {
        return localizationService.Get(
            state switch
            {
                FlourishBackgroundTaskState.Queued => FlourishLocaleKeys.BackgroundTaskQueued,
                FlourishBackgroundTaskState.Cancelling => FlourishLocaleKeys.BackgroundTaskCancelling,
                _ => FlourishLocaleKeys.BackgroundTaskRunning,
            }
        );
    }

    private void SystemStatusButton_MouseEnter(
        object sender,
        System.Windows.Input.MouseEventArgs e
    )
    {
        if (statusFlyoutOpenedWithFocus)
        {
            return;
        }

        OpenSystemStatusFlyout(focusFlyout: false);
    }

    private void SystemStatusButton_Click(object sender, RoutedEventArgs e)
    {
        OpenSystemStatusFlyout(focusFlyout: true);
    }

    private void OpenSystemStatusFlyout(bool focusFlyout)
    {
        var title = localizationService.Get(FlourishLocaleKeys.SystemStatusTitle);
        StatusFlyoutTitle.Text = title;
        AutomationProperties.SetName(StatusFlyoutTitle, title);
        AutomationProperties.SetName(StatusFlyoutCard, title);
        StatusFlyoutContentHost.Children.Clear();

        if (statusService.IsLANConnectionStatusEnabled)
        {
            var networkState = localizationService.Get(
                FlourishLocaleKeys.SystemStatusUnknown
            );
            try
            {
                networkState = localizationService.Get(
                    System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable()
                        ? FlourishLocaleKeys.StatusConnected
                        : FlourishLocaleKeys.StatusDisconnected
                );
            }
            catch (Exception error)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Flourish network status query failed: {error}"
                );
            }

            StatusFlyoutContentHost.Children.Add(
                CreateStatusDetailRow(
                    "\uE701",
                    localizationService.Get(FlourishLocaleKeys.SystemStatusNetwork),
                    networkState
                )
            );
        }

        if (statusService.IsPowerStatusEnabled)
        {
            var powerSource = localizationService.Get(
                FlourishLocaleKeys.SystemStatusUnknown
            );
            try
            {
                var powerStatus = System.Windows.Forms.SystemInformation.PowerStatus;
                powerSource = powerStatus.PowerLineStatus switch
                {
                    System.Windows.Forms.PowerLineStatus.Online => localizationService.Get(
                        FlourishLocaleKeys.SystemStatusAC
                    ),
                    System.Windows.Forms.PowerLineStatus.Offline => localizationService.Get(
                        FlourishLocaleKeys.SystemStatusBattery
                    ),
                    _ => localizationService.Get(FlourishLocaleKeys.SystemStatusUnknown),
                };
                var batteryStatus = powerStatus.BatteryChargeStatus;
                var hasNoSystemBattery = batteryStatus.HasFlag(
                    System.Windows.Forms.BatteryChargeStatus.NoSystemBattery
                );
                var hasUsableBattery =
                    !hasNoSystemBattery
                    && !batteryStatus.HasFlag(
                        System.Windows.Forms.BatteryChargeStatus.Unknown
                    );
                if (
                    hasNoSystemBattery
                    && powerStatus.PowerLineStatus
                        != System.Windows.Forms.PowerLineStatus.Online
                )
                {
                    powerSource = localizationService.Get(
                        FlourishLocaleKeys.SystemStatusUnknown
                    );
                }

                if (
                    hasUsableBattery
                    && powerStatus.BatteryLifePercent is >= 0 and <= 1
                )
                {
                    powerSource = $"{powerSource} · {powerStatus.BatteryLifePercent:P0}";
                }
            }
            catch (Exception error)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Flourish power status query failed: {error}"
                );
            }

            StatusFlyoutContentHost.Children.Add(
                CreateStatusDetailRow(
                    "\uE850",
                    localizationService.Get(FlourishLocaleKeys.SystemStatusPower),
                    powerSource
                )
            );
        }

        OpenStatusFlyout(SystemStatusButton, StatusFlyoutKind.System, focusFlyout);
    }

    private FrameworkElement CreateStatusDetailRow(
        string iconGlyph,
        string label,
        string value
    )
    {
        var row = new Grid { Margin = new Thickness(0, 0, 0, 8) };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(24) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var icon = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            Text = iconGlyph,
        };
        BindIconTypography(icon, "FlourishFontSizeTitlebarIcon");
        icon.SetResourceReference(TextBlock.ForegroundProperty, "FlourishNeutralForeground2Brush");
        row.Children.Add(icon);

        var text = new StackPanel();
        Grid.SetColumn(text, 1);
        var labelText = new TextBlock
        {
            Text = label,
        };
        BindTextSize(labelText, "FlourishFontSizeSmall");
        labelText.SetResourceReference(TextBlock.ForegroundProperty, "FlourishNeutralForeground2Brush");
        text.Children.Add(labelText);
        var valueText = new TextBlock
        {
            Margin = new Thickness(0, 2, 0, 0),
            Text = value,
        };
        BindTextSize(valueText, "FlourishFontSizeCaption");
        valueText.SetResourceReference(TextBlock.ForegroundProperty, "FlourishNeutralForeground1Brush");
        text.Children.Add(valueText);
        row.Children.Add(text);
        return row;
    }

    private void OpenStatusFlyout(
        FrameworkElement anchor,
        StatusFlyoutKind kind,
        bool focusFlyout
    )
    {
        CloseProfileOverlay();
        var wasVisible = StatusFlyoutOverlay.Visibility == Visibility.Visible;
        if (focusFlyout && !statusFlyoutOpenedWithFocus)
        {
            statusFlyoutOpenedWithFocus = true;
            statusFlyoutRestoreFocusTarget = anchor;
        }
        else if (!focusFlyout && !wasVisible)
        {
            statusFlyoutOpenedWithFocus = false;
            statusFlyoutRestoreFocusTarget = null;
        }

        if (kind != StatusFlyoutKind.BackgroundTasks)
        {
            statusFlyoutAnchorTaskId = null;
        }

        statusFlyoutAnchor = anchor;
        statusFlyoutKind = kind;
        StatusFlyoutOverlay.Visibility = Visibility.Visible;
        Dispatcher.BeginInvoke(
            new Action(() =>
            {
                UpdateStatusFlyoutPosition();
                if (focusFlyout)
                {
                    FocusStatusFlyoutContent();
                }
            })
        );
    }

    private void FocusStatusFlyoutContent()
    {
        if (statusFlyoutKind == StatusFlyoutKind.BackgroundTasks)
        {
            foreach (var task in backgroundTasks)
            {
                if (
                    backgroundTaskRowsById.TryGetValue(task.Id, out var row)
                    && row.CancelButton.IsEnabled
                    && row.CancelButton.Focus()
                )
                {
                    return;
                }
            }
        }

        if (!StatusFlyoutTitle.Focus())
        {
            StatusFlyoutCard.Focus();
        }
    }

    private void CloseStatusFlyout(bool restoreFocus = true)
    {
        if (StatusFlyoutOverlay.Visibility != Visibility.Visible)
        {
            return;
        }

        StatusFlyoutOverlay.Visibility = Visibility.Collapsed;
        var previousAnchor = statusFlyoutAnchor;
        var shouldRestoreFocus = restoreFocus && statusFlyoutOpenedWithFocus;
        var restoreTarget = statusFlyoutRestoreFocusTarget ?? previousAnchor;
        statusFlyoutAnchor = null;
        statusFlyoutAnchorTaskId = null;
        statusFlyoutKind = StatusFlyoutKind.None;
        statusFlyoutOpenedWithFocus = false;
        statusFlyoutRestoreFocusTarget = null;
        if (shouldRestoreFocus && restoreTarget is not null)
        {
            Keyboard.Focus(restoreTarget);
        }
    }

    private void StatusFlyoutOverlay_PreviewMouseLeftButtonDown(
        object sender,
        MouseButtonEventArgs e
    )
    {
        var position = e.GetPosition(StatusFlyoutCard);
        if (
            position.X >= 0
            && position.Y >= 0
            && position.X <= StatusFlyoutCard.ActualWidth
            && position.Y <= StatusFlyoutCard.ActualHeight
        )
        {
            return;
        }

        CloseStatusFlyout();
        e.Handled = true;
    }

    private void StatusBarBorder_PreviewMouseLeftButtonDown(
        object sender,
        MouseButtonEventArgs e
    )
    {
        if (
            StatusFlyoutOverlay.Visibility != Visibility.Visible
            || statusFlyoutAnchor?.IsMouseOver == true
        )
        {
            return;
        }

        // The status bar is intentionally above the transparent overlay so its buttons remain
        // clickable after a hover-open. Let the new click take focus naturally.
        CloseStatusFlyout(restoreFocus: false);
    }

    private void StatusFlyoutCard_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateStatusFlyoutPosition();
    }

    private void UpdateStatusFlyoutPosition()
    {
        const double shadowSafeMargin = 14;
        const double anchorGap = 6;
        if (
            StatusFlyoutOverlay.Visibility != Visibility.Visible
            || statusFlyoutAnchor is null
            || ShellRootGrid.ActualWidth <= shadowSafeMargin * 2
            || ShellRootGrid.ActualHeight <= shadowSafeMargin * 2
        )
        {
            return;
        }

        var anchorTopLeft = statusFlyoutAnchor.TranslatePoint(
            new System.Windows.Point(),
            ShellRootGrid
        );
        var anchor = new Rect(
            anchorTopLeft,
            new System.Windows.Size(
                statusFlyoutAnchor.ActualWidth,
                statusFlyoutAnchor.ActualHeight
            )
        );
        var availableWidth = Math.Max(
            0,
            ShellRootGrid.ActualWidth - shadowSafeMargin * 2
        );
        StatusFlyoutCard.MaxWidth = availableWidth;
        StatusFlyoutCard.MaxHeight = Math.Max(
            0,
            anchor.Top - shadowSafeMargin - anchorGap
        );

        var cardWidth = StatusFlyoutCard.ActualWidth > 0
            ? Math.Min(StatusFlyoutCard.ActualWidth, availableWidth)
            : Math.Min(StatusFlyoutCard.Width, availableWidth);
        var cardHeight = Math.Min(
            StatusFlyoutCard.ActualHeight,
            StatusFlyoutCard.MaxHeight
        );
        var desiredLeft = anchor.Left + (anchor.Width - cardWidth) / 2;
        var maximumLeft = Math.Max(
            shadowSafeMargin,
            ShellRootGrid.ActualWidth - cardWidth - shadowSafeMargin
        );
        var left = Math.Clamp(desiredLeft, shadowSafeMargin, maximumLeft);
        var top = Math.Max(
            shadowSafeMargin,
            anchor.Top - cardHeight - anchorGap
        );

        Canvas.SetLeft(StatusFlyoutCard, left);
        Canvas.SetTop(StatusFlyoutCard, top);
    }

    private void ApplyNavigationPanelPlacement()
    {
        if (options.NavigationPanelDirection == NavigationPanelDirection.Right)
        {
            NavigationItemsHost.FlowDirection = System.Windows.FlowDirection.RightToLeft;
            FixedNavigationItemsHost.FlowDirection = System.Windows.FlowDirection.RightToLeft;
            Grid.SetColumn(ContentAreaGrid, 0);
            Grid.SetColumn(NavigationPaneTransitionHost, 1);
            Grid.SetColumn(NavigationPaneSplitter, 1);
            NavigationPaneSplitter.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            NavigationPaneSplitter.ResizeBehavior = GridResizeBehavior.PreviousAndCurrent;
            NavigationPaneBorder.BorderThickness = new Thickness(1, 0, 0, 0);
            NavigationPaneBorder.SetResourceReference(
                Border.PaddingProperty,
                "FlourishNavigationPaneRightPadding"
            );
            return;
        }

        NavigationItemsHost.FlowDirection = System.Windows.FlowDirection.LeftToRight;
        FixedNavigationItemsHost.FlowDirection = System.Windows.FlowDirection.LeftToRight;
        Grid.SetColumn(NavigationPaneTransitionHost, 0);
        Grid.SetColumn(ContentAreaGrid, 1);
        Grid.SetColumn(NavigationPaneSplitter, 0);
        NavigationPaneSplitter.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
        NavigationPaneSplitter.ResizeBehavior = GridResizeBehavior.CurrentAndNext;
        NavigationPaneBorder.BorderThickness = new Thickness(0, 0, 1, 0);
        NavigationPaneBorder.SetResourceReference(
            Border.PaddingProperty,
            "FlourishNavigationPaneLeftPadding"
        );
    }

    private ColumnDefinition GetNavigationPaneColumn()
    {
        return options.NavigationPanelDirection == NavigationPanelDirection.Right
            ? ContentColumn
            : PaneColumn;
    }

    private ColumnDefinition GetContentColumn()
    {
        return options.NavigationPanelDirection == NavigationPanelDirection.Right
            ? PaneColumn
            : ContentColumn;
    }

    private void SetNavigationPaneWidth(double width)
    {
        GetNavigationPaneColumn().Width = new GridLength(width);
        GetContentColumn().Width = new GridLength(1, GridUnitType.Star);
    }

    private void NormalizeNavigationPaneWidths()
    {
        options.OpenPaneWidth = CoerceOpenPaneWidth(options.OpenPaneWidth);
        options.ClosedPaneWidth = Math.Min(options.ClosedPaneWidth, options.OpenPaneWidth);
    }

    private double CoerceOpenPaneWidth(double width)
    {
        return Math.Min(
            Math.Max(width, options.NavigationPaneMinWidth),
            options.NavigationPaneMaxWidth
        );
    }

    private void ApplyNavigationPaneColumnConstraints(bool isOpen)
    {
        var paneColumn = GetNavigationPaneColumn();
        paneColumn.MinWidth = isOpen ? options.NavigationPaneMinWidth : 0;
        paneColumn.MaxWidth = isOpen
            ? options.NavigationPaneMaxWidth
            : double.PositiveInfinity;
    }

    private void UpdateNavigationPaneSplitterState()
    {
        var isSplitterEnabled = options.IsNavigationPanelEnabled && isPaneOpen;
        NavigationPaneSplitter.IsEnabled = isSplitterEnabled;
        NavigationPaneSplitter.Visibility = isSplitterEnabled
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private void ApplyNavigationPaneState(bool animate = false)
    {
        var isNavigationVisible = options.IsNavigationPanelEnabled;
        var isOpen = isNavigationVisible && isPaneOpen;
        var paneWidth = !isNavigationVisible
            ? 0
            : isOpen
                ? CoerceOpenPaneWidth(options.OpenPaneWidth)
                : options.ClosedPaneWidth;

        if (isOpen)
        {
            ApplyNavigationPaneChrome(isOpen);
        }

        if (!animate)
        {
            StopNavigationPaneAnimations();
            ApplyNavigationPaneColumnConstraints(isOpen);
            SetNavigationPaneWidth(paneWidth);
            ApplyNavigationPaneChrome(isOpen);
            UpdateNavigationPaneSplitterState();
            return;
        }

        NavigationPaneSplitter.IsEnabled = false;
        NavigationPaneSplitter.Visibility = Visibility.Collapsed;
        var paneColumn = GetNavigationPaneColumn();
        var committedWidth = paneColumn.ActualWidth > 0
            ? paneColumn.ActualWidth
            : paneColumn.Width.Value;

        motionService.AnimateNavigationPane(
            navigationPaneTransition,
            new NavigationPaneTransitionTarget(
                WorkAreaGrid,
                NavigationPaneTransitionHost,
                ContentAreaGrid,
                ContentAreaScaleTransform,
                ContentAreaTranslateTransform,
                options.NavigationPanelDirection
            ),
            committedWidth,
            paneWidth,
            CoerceOpenPaneWidth(options.OpenPaneWidth),
            Math.Abs(options.OpenPaneWidth - options.ClosedPaneWidth),
            () =>
            {
                ApplyNavigationPaneColumnConstraints(isOpen);
                SetNavigationPaneWidth(paneWidth);
                ApplyNavigationPaneChrome(isOpen);
                UpdateNavigationPaneSplitterState();
            }
        );
    }

    private void NavigationPaneSplitter_DragCompleted(object sender, DragCompletedEventArgs e)
    {
        if (!options.IsNavigationPanelEnabled || !isPaneOpen)
        {
            return;
        }

        var horizontalChange =
            options.NavigationPanelDirection == NavigationPanelDirection.Right
                ? -e.HorizontalChange
                : e.HorizontalChange;
        var paneWidth = e.Canceled
            ? navigationPaneDragStartWidth
            : CoerceOpenPaneWidth(navigationPaneDragStartWidth + horizontalChange);

        navigationPanelService.RecordOpenWidth(paneWidth);
        ApplyNavigationPaneColumnConstraints(isOpen: true);
        SetNavigationPaneWidth(paneWidth);
        RefreshWorkAreaLayout();
    }

    private void NavigationPaneSplitter_DragStarted(object sender, DragStartedEventArgs e)
    {
        navigationPaneDragStartWidth = CoerceOpenPaneWidth(
            GetNavigationPaneColumn().ActualWidth
        );
    }

    private void RefreshWorkAreaLayout()
    {
        WorkAreaGrid.InvalidateMeasure();
        WorkAreaGrid.InvalidateArrange();
        WorkAreaGrid.UpdateLayout();
    }

    private void ApplyNavigationPaneChrome(bool isOpen)
    {
        NavigationItemsHost.IsCompact = !isOpen;
        FixedNavigationItemsHost.IsCompact = !isOpen;
    }

    private void BuildToolbarItems(Type? pageType = null, bool force = false)
    {
        if (!options.IsDynamicToolbarEnabled)
        {
            ToolbarItemsHost.Children.Clear();
            ToolbarHostBorder.Visibility = Visibility.Collapsed;
            activeToolbarPageType = null;
            isDefaultToolbarActive = false;
            return;
        }

        if (
            !force
            && activeToolbarPageType == pageType
            && isDefaultToolbarActive == (pageType is null)
        )
        {
            return;
        }

        ToolbarItemsHost.Children.Clear();
        var toolbarButtons = GetToolbarButtons(pageType);

        foreach (var button in toolbarButtons)
        {
            ToolbarItemsHost.Children.Add(button);
        }

        ToolbarHostBorder.Visibility =
            toolbarButtons.Count > 0
            || ToolbarStartRegionHost.Children.Count > 0
            || ToolbarEndRegionHost.Children.Count > 0
                ? Visibility.Visible
                : Visibility.Collapsed;

        activeToolbarPageType = pageType;
        isDefaultToolbarActive = pageType is null;
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
            && ReferenceEquals(cached.Definition, content)
        )
        {
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

    private void SetRegionContent(
        FlourishRegion region,
        IReadOnlyList<FrameworkElement> elements
    )
    {
        switch (region)
        {
            case FlourishRegion.TitlebarStart:
            case FlourishRegion.TitlebarCenter:
            case FlourishRegion.TitlebarEnd:
            case FlourishRegion.TitlebarProfile:
                Titlebar.SetRegionContent(region, elements);
                break;
            case FlourishRegion.NavigationHeader:
                SetPanelContent(NavigationHeaderRegionHost, elements);
                break;
            case FlourishRegion.NavigationFooter:
                SetPanelContent(NavigationFooterRegionHost, elements);
                break;
            case FlourishRegion.ContentHeader:
                SetPanelContent(ContentHeaderRegionHost, elements);
                break;
            case FlourishRegion.ContentFooter:
                SetPanelContent(ContentFooterRegionHost, elements);
                break;
            case FlourishRegion.ContentOverlay:
                SetPanelContent(ContentOverlayRegionHost, elements);
                break;
            case FlourishRegion.ToolbarStart:
                SetPanelContent(ToolbarStartRegionHost, elements);
                break;
            case FlourishRegion.ToolbarEnd:
                SetPanelContent(ToolbarEndRegionHost, elements);
                break;
            case FlourishRegion.FooterStart:
                SetPanelContent(FooterStartRegionHost, elements);
                break;
            case FlourishRegion.FooterEnd:
                SetPanelContent(FooterEndRegionHost, elements);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(region), region, "Unknown shell region.");
        }
    }

    private static void SetPanelContent(
        WpfPanel host,
        IReadOnlyList<FrameworkElement> elements
    )
    {
        host.Children.Clear();
        foreach (var element in elements)
        {
            host.Children.Add(element);
        }

        host.Visibility = elements.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private IReadOnlyList<Button> GetToolbarButtons(Type? pageType)
    {
        if (pageType is null)
        {
            return defaultToolbarButtons ??= CreateToolbarButtons(
                toolbarService.GetToolbarItems(),
                showIconOnly: false
            );
        }

        if (!toolbarButtonsByPageType.TryGetValue(pageType, out var toolbarButtons))
        {
            toolbarButtons = CreateToolbarButtons(
                toolbarService.GetToolbarItems(pageType),
                ShouldShowToolbarIconOnly(pageType)
            );
            toolbarButtonsByPageType[pageType] = toolbarButtons;
        }

        return toolbarButtons;
    }

    private IReadOnlyList<Button> CreateToolbarButtons(
        IReadOnlyList<FlourishToolbarItem> items,
        bool showIconOnly
    )
    {
        var buttons = new List<Button>(items.Count);
        foreach (var item in items)
        {
            if (!item.IsVisible)
            {
                continue;
            }

            var useIconOnly = showIconOnly && !string.IsNullOrWhiteSpace(item.IconGlyph);
            var button = new Button
            {
                Content = useIconOnly
                    ? CreateIconContent(item.IconGlyph)
                    : CreateIconTextContent(item.IconGlyph, item.DisplayName),
                Margin = buttons.Count > 0 ? new Thickness(2, 0, 0, 0) : new Thickness(),
                ToolTip = new FlourishToolTip { Content = item.DisplayName },
                Appearance = FlourishButtonAppearance.Subtle,
                Variant = FlourishButtonVariant.Toolbar,
                Tag = item.CommandKey,
                Width = useIconOnly ? 30 : double.NaN,
                IsEnabled = item.IsEnabled
                    && (
                        string.IsNullOrWhiteSpace(item.CommandKey)
                        || commandParser.CanExecute(
                            item.CommandKey,
                            source: CommandSource.Toolbar
                        )
                    ),
            };
            button.Click += ToolbarButton_Click;
            buttons.Add(button);
        }

        return buttons;
    }

    private bool ShouldShowToolbarIconOnly(Type pageType)
    {
        return toolbarService.ShouldShowIconOnly(pageType);
    }

    private async void ToolbarButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string commandKey })
        {
            await commandParser.ExecuteAsync(
                commandKey,
                source: CommandSource.Toolbar
            );
        }
    }

    private void BuildNavigationItems()
    {
        var selectedNavigationKey = navigationService.CurrentNavigationKey;
        navigationItemsByKey.Clear();
        navigationItemsByPage.Clear();
        navigationParentsByKey.Clear();
        navigationChildrenByParentKey.Clear();
        activeChildParentItem = null;
        firstNavigationItem = null;
        selectedNavigationItem = null;

        foreach (var item in options.NavigationItems.Concat(options.FixedNavigationItems))
        {
            item.Validate();
            if (!item.IsNavigationItem)
            {
                continue;
            }

            if (!navigationItemsByKey.ContainsKey(item.Key))
            {
                navigationItemsByKey[item.Key] = item;
            }

            IndexNavigationTreeItem(item);

            if (item.PageType is not null)
            {
                firstNavigationItem ??= item;
                navigationItemsByPage[item.PageType] = item;
            }
        }

        NavigationItemsHost.ItemsSource = null;
        FixedNavigationItemsHost.ItemsSource = null;
        NavigationItemsHost.ItemsSource = options.NavigationItems;
        FixedNavigationItemsHost.ItemsSource = options.FixedNavigationItems;
        FixedNavigationItemsBorder.Visibility =
            options.FixedNavigationItems.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

        if (selectedNavigationKey is not null && GetNavigationItem(selectedNavigationKey) is { } selected)
        {
            SelectNavigationItem(selected);
        }
        else
        {
            RestoreSelectedNavigationItem();
        }
    }

    private void IndexNavigationTreeItem(FlourishNavigationItem item)
    {
        if (item.ParentId != 0)
        {
            navigationParentsByKey[CreateNavigationTreeKey(item, item.ParentId)] = item;
        }

        if (item.ChildId == 0)
        {
            return;
        }

        var childKey = CreateNavigationTreeKey(item, item.ChildId);
        if (!navigationChildrenByParentKey.TryGetValue(childKey, out var children))
        {
            children = [];
            navigationChildrenByParentKey[childKey] = children;
        }

        children.Add(item);
    }

    private void BuildStatusItems()
    {
        StatusItemsHost.Children.Clear();

        foreach (var item in statusService.StatusItems)
        {
            if (!item.IsVisible)
            {
                continue;
            }

            var status = new StackPanel
            {
                Margin =
                    StatusItemsHost.Children.Count > 0
                        ? new Thickness(14, 0, 0, 0)
                        : new Thickness(),
                Orientation = Orientation.Horizontal,
            };
            var iconText = new TextBlock
            {
                VerticalAlignment = VerticalAlignment.Center,
                Text = item.IconGlyph,
            };
            BindIconTypography(iconText, "FlourishFontSizeCaption");
            iconText.SetResourceReference(TextBlock.ForegroundProperty, "FlourishNeutralForeground2Brush");
            status.Children.Add(iconText);

            var labelText = new TextBlock
            {
                Margin = new Thickness(5, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Text = item.Text,
            };
            BindTextSize(labelText, "FlourishFontSizeCaption");
            labelText.SetResourceReference(TextBlock.ForegroundProperty, "FlourishNeutralForeground2Brush");
            status.Children.Add(labelText);

            StatusItemsHost.Children.Add(status);
        }

        UpdateStatusBarVisibility();
    }

    private void BuildNotifications(IReadOnlyList<FlourishNotificationInfo> notifications)
    {
        NotificationItemsHost.Children.Clear();
        foreach (var info in notifications.Reverse().Take(5))
        {
            var definition = info.Notification;
            var layout = new Grid();
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            layout.ColumnDefinitions.Add(
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
            );
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var icon = new TextBlock
            {
                Width = 24,
                Margin = new Thickness(0, 2, 10, 0),
                VerticalAlignment = VerticalAlignment.Top,
                FontSize = 18,
                Text = definition.IconGlyph ?? GetNotificationGlyph(definition.Severity),
            };
            BindIconTypography(icon);
            icon.SetResourceReference(
                TextBlock.ForegroundProperty,
                "FlourishPrimaryForegroundBrush"
            );
            layout.Children.Add(icon);

            var content = new StackPanel();
            content.Children.Add(
                new FlourishTextBlock
                {
                    Role = FlourishTextRole.CardTitle,
                    Text = definition.Title,
                }
            );
            var message = new FlourishTextBlock
            {
                Margin = new Thickness(0, 4, 0, 0),
                Role = FlourishTextRole.Description,
                Text = definition.Message,
            };
            content.Children.Add(message);

            if (!string.IsNullOrWhiteSpace(definition.CommandKey))
            {
                var action = new Button
                {
                    Margin = new Thickness(0, 8, 0, 0),
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                    Content = "Run action",
                    Tag = info,
                    Appearance = FlourishButtonAppearance.Subtle,
                    Variant = FlourishButtonVariant.Toolbar,
                };
                action.Click += NotificationAction_Click;
                content.Children.Add(action);
            }

            Grid.SetColumn(content, 1);
            layout.Children.Add(content);

            var dismiss = new Button
            {
                Width = 28,
                Height = 28,
                Margin = new Thickness(8, 0, 0, 0),
                Content = CreateIconContent("\uE711"),
                Tag = definition.Id,
                Appearance = FlourishButtonAppearance.Subtle,
                Variant = FlourishButtonVariant.Toolbar,
                ToolTip = new FlourishToolTip { Content = "Dismiss" },
            };
            dismiss.Click += NotificationDismiss_Click;
            Grid.SetColumn(dismiss, 2);
            layout.Children.Add(dismiss);

            var surface = new Border
            {
                Padding = new Thickness(14, 12, 10, 12),
                Child = layout,
            };
            surface.SetResourceReference(
                Border.BackgroundProperty,
                "FlourishCardBackgroundBrush"
            );
            surface.SetResourceReference(
                Border.BorderBrushProperty,
                "FlourishSurfaceStrokeBrush"
            );
            surface.SetResourceReference(
                Border.BorderThicknessProperty,
                "FlourishSurfaceBorderThickness"
            );
            surface.SetResourceReference(
                Border.CornerRadiusProperty,
                "FlourishOverlayCornerRadius"
            );

            var shadow = new Border { IsHitTestVisible = false };
            shadow.SetResourceReference(
                Border.BackgroundProperty,
                "FlourishNeutralForeground1Brush"
            );
            shadow.SetResourceReference(
                Border.CornerRadiusProperty,
                "FlourishOverlayCornerRadius"
            );
            shadow.SetResourceReference(
                UIElement.EffectProperty,
                "FlourishElevation2Effect"
            );

            var card = new Grid { Margin = new Thickness(0, 0, 0, 14) };
            card.Children.Add(shadow);
            card.Children.Add(surface);
            AutomationProperties.SetName(card, $"{definition.Title}: {definition.Message}");
            NotificationItemsHost.Children.Add(card);
        }

        NotificationItemsHost.Visibility = NotificationItemsHost.Children.Count > 0
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private async void NotificationAction_Click(object sender, RoutedEventArgs e)
    {
        if (
            sender is not Button { Tag: FlourishNotificationInfo info }
            || string.IsNullOrWhiteSpace(info.Notification.CommandKey)
        )
        {
            return;
        }

        await commandParser.ExecuteAsync(
            info.Notification.CommandKey,
            info.Notification,
            CommandSource.Notification
        );
    }

    private void StopNavigationPaneAnimations()
    {
        navigationPaneTransition.Cancel();
    }

    private void NotificationDismiss_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string id })
        {
            notificationService.Dismiss(id);
        }
    }

    private static string GetNotificationGlyph(FlourishNotificationSeverity severity) =>
        severity switch
        {
            FlourishNotificationSeverity.Success => "\uE930",
            FlourishNotificationSeverity.Warning => "\uE7BA",
            FlourishNotificationSeverity.Error => "\uEA39",
            _ => "\uE946",
        };

    private void NavigationPanelService_Changed(
        object? sender,
        FlourishNavigationPanelChangedEventArgs e
    )
    {
        DispatchRuntimeChange(
            () =>
            {
                ApplyNavigationPanelRuntimeState(e.Animate);
                var current = navigationPanelService.Current;
                Titlebar.ConfigureVisibility(
                    options.IsTitlebarSearchEnabled,
                    IsBreadcrumbFeatureEnabled(),
                    options.IsTitlebarNavigationToggleEnabled && current.IsEnabled,
                    options.IsTitlebarLogoEnabled,
                    options.IsTitlebarTitleEnabled,
                    options.IsTitlebarSubtitleEnabled,
                    options.IsTitlebarThemeToggleEnabled && options.IsThemeEnabled,
                    options.IsProfileEnabled && options.IsTitlebarProfileEnabled
                );
            }
        );
    }

    private void ApplyNavigationPanelRuntimeState(bool animate)
    {
        if (!animate)
        {
            StopNavigationPaneAnimations();
        }

        var current = navigationPanelService.Current;
        isPaneOpen = current.IsOpen;
        NavigationPaneBorder.Visibility = current.IsEnabled
            ? Visibility.Visible
            : Visibility.Collapsed;
        NormalizeNavigationPaneWidths();
        ApplyNavigationPanelPlacement();
        ApplyNavigationPaneState(animate);
    }

    private void NavigationMenuService_Changed(
        object? sender,
        FlourishNavigationMenuChangedEventArgs e
    )
    {
        DispatchRuntimeChange(
            () =>
            {
                BuildNavigationItems();
                ClearToolbarButtonCache();
                BuildToolbarItems(navigationService.CurrentSourcePageType, force: true);
                if (navigationService.CurrentSourcePageType is { } pageType)
                {
                    UpdateBreadcrumb(pageType);
                }
            }
        );
    }

    private void ToolbarService_Changed(object? sender, FlourishToolbarChangedEventArgs e)
    {
        DispatchRuntimeChange(
            () =>
            {
                ClearToolbarButtonCache();
                BuildToolbarItems(navigationService.CurrentSourcePageType, force: true);
            }
        );
    }

    private void StatusService_Changed(object? sender, FlourishStatusBarChangedEventArgs e)
    {
        DispatchRuntimeChange(BuildStatusItems);
    }

    private void ShellRegionService_Changed(
        object? sender,
        FlourishShellRegionChangedEventArgs e
    )
    {
        DispatchRuntimeChange(() => BuildRegionContents(e.Region));
    }

    private void TitleBarService_Changed(object? sender, FlourishTitleBarChangedEventArgs e)
    {
        DispatchRuntimeChange(() =>
        {
            var current = titleBarService.Current;
            ApplyTitleBarState(current);
            var shouldSubscribeToProfile =
                options.IsTitlebarEnabled
                && profileFlyoutService.Current.IsEnabled
                && current.IsProfileVisible;
            if (shouldSubscribeToProfile != isProfileServiceSubscribed)
            {
                ConfigureProfileSurface();
            }
        });
    }

    private void ApplyTitleBarState(FlourishTitleBarState state)
    {
        Title = state.Title;
        Titlebar.SetTitle(state.Title);
        Titlebar.SetSubtitle(state.Subtitle);
        Titlebar.SetSearchPlaceholder(state.SearchPlaceholder);
        RequestTitleBarLogo(state.LogoPath, state.LogoFallbackText, state.IsLogoVisible);
        Titlebar.ConfigureVisibility(
            state.IsSearchVisible,
            state.IsBreadcrumbVisible && state.BreadcrumbMode != BreadcrumbShowOption.Hidden,
            state.IsNavigationToggleVisible && options.IsNavigationPanelEnabled,
            state.IsLogoVisible,
            state.IsTitleVisible,
            state.IsSubtitleVisible,
            state.IsThemeToggleVisible && options.IsThemeEnabled,
            state.IsProfileVisible && options.IsProfileEnabled
        );
        UpdateTitlebarBreadcrumbNavigation();
        if (navigationService.CurrentSourcePageType is { } pageType)
        {
            UpdateBreadcrumb(pageType);
        }
    }

    private void RequestTitleBarLogo(
        string? logoPath,
        string fallbackText,
        bool isVisible
    )
    {
        if (isShellClosed)
        {
            return;
        }

        currentTitleBarLogoFallbackText = fallbackText;
        isTitleBarLogoVisible = isVisible;

        var request = titleBarLogoLoadCoordinator.Request(logoPath);
        if (request.Completion.IsCompletedSuccessfully)
        {
            var result = request.Completion.Result;
            if (titleBarLogoLoadCoordinator.IsCurrent(result))
            {
                ApplyTitleBarLogoSource(result.Source);
            }

            return;
        }

        ApplyTitleBarLogoSource(
            request.IsNewRequest
                ? TitleBarVisualAssets.DefaultLogoSource
                : currentTitleBarLogoSource
        );
        if (request.IsNewRequest)
        {
            _ = CompleteTitleBarLogoRequestAsync(request);
        }
    }

    private async Task CompleteTitleBarLogoRequestAsync(TitleBarLogoLoadRequest request)
    {
        TitleBarLogoLoadResult result;
        try
        {
            result = await request.Completion.ConfigureAwait(false);
        }
        catch (Exception)
        {
            // The production loader converts recoverable file/decoder failures into a null
            // result. An unexpected loader failure must not escape a fire-and-forget UI update.
            return;
        }

        if (!result.IsCurrent)
        {
            return;
        }

        DispatchRuntimeChange(
            () =>
            {
                if (titleBarLogoLoadCoordinator.IsCurrent(result))
                {
                    ApplyTitleBarLogoSource(result.Source);
                }
            }
        );
    }

    private void ApplyTitleBarLogoSource(ImageSource? source)
    {
        var effectiveSource = source ?? TitleBarVisualAssets.DefaultLogoSource;
        currentTitleBarLogoSource = effectiveSource;
        Titlebar.SetLogo(effectiveSource, currentTitleBarLogoFallbackText);
        Icon = isTitleBarLogoVisible ? effectiveSource : null;
    }

    private void TitleBarSearchService_StateChanged(
        object? sender,
        FlourishTitleBarSearchStateChangedEventArgs e
    )
    {
        DispatchRuntimeChange(() =>
        {
            var current = titleBarSearchService.Current;
            if (current.Version != e.State.Version)
            {
                return;
            }

            Titlebar.SetSearchPlaceholder(current.Placeholder);
            Titlebar.SetSearchText(current.Text);
            var titleState = titleBarService.Current;
            Titlebar.ConfigureVisibility(
                current.IsVisible,
                titleState.IsBreadcrumbVisible
                    && titleState.BreadcrumbMode != BreadcrumbShowOption.Hidden,
                titleState.IsNavigationToggleVisible && options.IsNavigationPanelEnabled,
                titleState.IsLogoVisible,
                titleState.IsTitleVisible,
                titleState.IsSubtitleVisible,
                titleState.IsThemeToggleVisible && options.IsThemeEnabled,
                titleState.IsProfileVisible && options.IsProfileEnabled
            );
            if (current.FocusRequested && options.IsTitlebarEnabled)
            {
                Titlebar.FocusSearchBox();
                titleBarSearchService.AcknowledgeFocusRequest();
            }
        });
    }

    private void NotificationService_NotificationsChanged(
        object? sender,
        FlourishNotificationsChangedEventArgs e
    )
    {
        DispatchRuntimeChange(() => BuildNotifications(notificationService.ActiveNotifications));
    }

    private void ProfileFlyoutService_Changed(
        object? sender,
        FlourishProfileFlyoutChangedEventArgs e
    )
    {
        DispatchRuntimeChange(ConfigureProfileSurface);
    }

    private void ShellFeatureService_Changed(
        object? sender,
        FlourishShellFeatureChangedEventArgs e
    )
    {
        DispatchRuntimeChange(() =>
        {
            switch (e.Feature)
            {
                case ShellFeature.TitleBar:
                    ApplyTitleBarState(titleBarService.Current);
                    ConfigureProfileSurface();
                    ApplyTitleBarFeatureState(refreshFrame: true);
                    break;
                case ShellFeature.Navigation:
                    ApplyNavigationPanelRuntimeState(animate: false);
                    ApplyTitleBarState(titleBarService.Current);
                    break;
                case ShellFeature.DynamicToolbar:
                    BuildToolbarItems(navigationService.CurrentSourcePageType, force: true);
                    break;
                case ShellFeature.StatusContent:
                    UpdateStatusBarVisibility();
                    break;
                case ShellFeature.Profile:
                    ConfigureProfileSurface();
                    ApplyTitleBarState(titleBarService.Current);
                    break;
            }
        });
    }

    private void LocalizationService_Changed(
        object? sender,
        FlourishLocalizationChangedEventArgs e
    )
    {
        DispatchRuntimeChange(() =>
        {
            Titlebar.ApplyLocale(localizationService);
            AutomationProperties.SetName(
                SystemStatusButton,
                localizationService.Get(FlourishLocaleKeys.SystemStatusTitle)
            );
            RefreshBackgroundTaskStatus(backgroundTaskService.ActiveTasks);
            switch (statusFlyoutKind)
            {
                case StatusFlyoutKind.BackgroundTasks:
                    OpenBackgroundTaskFlyout(
                        statusFlyoutAnchor ?? BackgroundTaskQueueButton,
                        statusFlyoutOpenedWithFocus
                    );
                    break;
                case StatusFlyoutKind.System:
                    OpenSystemStatusFlyout(statusFlyoutOpenedWithFocus);
                    break;
            }
        });
    }

    private void FontService_Changed(object? sender, FlourishFontChangedEventArgs e)
    {
        if (
            e.ChangeKind != FlourishFontChangeKind.PageOverride
            || e.AffectedPageType is not { } affectedPageType
        )
        {
            return;
        }

        DispatchRuntimeChange(() =>
        {
            var contentPageType = navigationService.CurrentSourcePageType;
            if (
                RootFrame.Content is WpfPage contentPage
                && (contentPageType ?? contentPage.GetType()) == affectedPageType
            )
            {
                fontService.ApplyToPage(
                    contentPage,
                    contentPageType ?? contentPage.GetType()
                );
            }

            var profilePageType = profileFlyoutService.Current.ContentPageType;
            if (
                ProfileFrame.Content is WpfPage profilePage
                && profilePageType == affectedPageType
            )
            {
                fontService.ApplyToPage(profilePage, profilePageType);
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
                || e.Current.NavigationPanelTransition
                    == FlourishNavigationPanelTransition.None
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
                ApplyNavigationPaneState();
            }
        });
    }

    private void CommandParser_Changed(object? sender, CommandRegistryChangedEventArgs e)
    {
        DispatchRuntimeChange(RefreshCommandAvailability);
    }

    private void CommandParser_CanExecuteChanged(
        object? sender,
        CommandCanExecuteChangedEventArgs e
    )
    {
        DispatchRuntimeChange(RefreshCommandAvailability);
    }

    private void RefreshCommandAvailability()
    {
        ClearToolbarButtonCache();
        BuildToolbarItems(navigationService.CurrentSourcePageType, force: true);
        NavigationItemsHost.Items.Refresh();
        FixedNavigationItemsHost.Items.Refresh();
    }

    private void DispatchRuntimeChange(Action action)
    {
        void ExecuteIfActive()
        {
            if (
                isShellClosed
                || Dispatcher.HasShutdownStarted
                || Dispatcher.HasShutdownFinished
            )
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

        _ = Dispatcher.BeginInvoke(
            DispatcherPriority.DataBind,
            new Action(ExecuteIfActive)
        );
    }

    private void ClearToolbarButtonCache()
    {
        if (defaultToolbarButtons is not null)
        {
            foreach (var button in defaultToolbarButtons)
            {
                button.Click -= ToolbarButton_Click;
            }
        }

        foreach (var buttons in toolbarButtonsByPageType.Values)
        {
            foreach (var button in buttons)
            {
                button.Click -= ToolbarButton_Click;
            }
        }

        defaultToolbarButtons = null;
        toolbarButtonsByPageType.Clear();
        activeToolbarPageType = null;
        isDefaultToolbarActive = false;
    }

    private void UpdateRuntimeSurfaceVisibility()
    {
        ToolbarHostBorder.Visibility =
            options.IsDynamicToolbarEnabled
            && (
                ToolbarItemsHost.Children.Count > 0
                || ToolbarStartRegionHost.Children.Count > 0
                || ToolbarEndRegionHost.Children.Count > 0
            )
                ? Visibility.Visible
                : Visibility.Collapsed;
        UpdateStatusBarVisibility();
    }

    private StackPanel CreateIconTextContent(string iconGlyph, string label)
    {
        var content = new StackPanel { Orientation = Orientation.Horizontal };
        var icon = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            Text = iconGlyph,
        };
        BindIconTypography(icon, "FlourishFontSizeTitlebarIcon");
        content.Children.Add(icon);
        content.Children.Add(
            new TextBlock
            {
                Margin = new Thickness(5, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Text = label,
            }
        );
        return content;
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
        BindIconTypography(icon, "FlourishFontSizeTitlebarIcon");
        return icon;
    }

    private static void BindIconTypography(TextBlock textBlock, string? sizeResourceKey = null)
    {
        textBlock.SetResourceReference(TextBlock.FontFamilyProperty, "FlourishIconFontFamily");

        if (sizeResourceKey is not null)
        {
            BindTextSize(textBlock, sizeResourceKey);
        }
    }

    private static void BindTextSize(TextBlock textBlock, string sizeResourceKey)
    {
        textBlock.SetResourceReference(TextBlock.FontSizeProperty, sizeResourceKey);
    }

    private void NavigateToInitialPage()
    {
        var initialItem =
            GetNavigationItem(options.InitialNavigationKey)
            ?? (
                options.InitialNavigationPageType is null
                    ? null
                    : navigationItemsByPage.GetValueOrDefault(options.InitialNavigationPageType)
            )
            ?? firstNavigationItem;

        if (initialItem is null)
        {
            return;
        }

        SelectNavigationItem(initialItem);
        ActivateNavigationItem(initialItem, false, toggleChildren: false);
    }

    private FlourishNavigationItem? GetNavigationItem(string? key)
    {
        return key is not null && navigationItemsByKey.TryGetValue(key, out var item) ? item : null;
    }

    private void Titlebar_BackRequested(object? sender, EventArgs e)
    {
        if (navigationService.CanGoBack)
        {
            navigationService.GoBack();
        }
    }

    private void Titlebar_ForwardRequested(object? sender, EventArgs e)
    {
        if (navigationService.CanGoForward)
        {
            navigationService.GoForward();
        }
    }

    private void Titlebar_NavigationToggleRequested(object? sender, EventArgs e)
    {
        if (!options.IsNavigationPanelEnabled)
        {
            return;
        }

        var shouldOpen = !isPaneOpen;
        if (!shouldOpen)
        {
            CollapseAllNavigationChildren();
        }

        navigationPanelService.Toggle(animate: true);

        if (shouldOpen)
        {
            RestoreSelectedNavigationItem();
        }
    }

    private void NavigationItemsHost_PreviewMouseLeftButtonDown(
        object sender,
        MouseButtonEventArgs e
    )
    {
        if (suppressNavigationSelection || sender is not ListBox)
        {
            return;
        }

        if (GetNavigationItemFromInputSource(e.OriginalSource) is not { } item)
        {
            return;
        }

        e.Handled = true;
        if (!item.IsNavigationItem || !item.IsEnabled)
        {
            return;
        }

        OpenNavigationPaneForCollapsedParent(item);

        if (item.IsCommandItem)
        {
            ActivateCommandNavigationItem(item);
            return;
        }

        SelectNavigationItem(item);
        ActivateNavigationItem(item, true);
    }

    private void NavigationItemsHost_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (suppressNavigationSelection || sender is not ListBox listBox)
        {
            return;
        }

        if (e.Key is not (Key.Enter or Key.Space))
        {
            return;
        }

        var item =
            GetNavigationItemFromInputSource(e.OriginalSource)
            ?? listBox.SelectedItem as FlourishNavigationItem;
        if (item is null || !item.IsNavigationItem || !item.IsEnabled)
        {
            return;
        }

        e.Handled = true;
        OpenNavigationPaneForCollapsedParent(item);

        if (item.IsCommandItem)
        {
            ActivateCommandNavigationItem(item);
            return;
        }

        SelectNavigationItem(item);
        ActivateNavigationItem(item, true);
    }

    private void NavigationItemsHost_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e
    )
    {
        if (suppressNavigationSelection || sender is not ListBox listBox)
        {
            return;
        }

        if (listBox.SelectedItem is not FlourishNavigationItem item)
        {
            return;
        }

        OpenNavigationPaneForCollapsedParent(item);

        if (!item.IsPageItem || !item.IsEnabled)
        {
            RestoreSelectedNavigationItem();
            return;
        }

        SelectNavigationItem(item);
        ActivateNavigationItem(item, true, toggleChildren: false);
    }

    private void ActivateCommandNavigationItem(FlourishNavigationItem item)
    {
        ActivateNavigationItem(item, true);
        RestoreSelectedNavigationItem();
        Keyboard.ClearFocus();
    }

    private void RootFrame_Navigated(object? sender, FlourishNavigatedEventArgs e)
    {
        fontService.ApplyToPage(e.Page, e.SourcePageType);
        UpdateTitlebarBreadcrumbNavigation();
        BuildToolbarItems(e.SourcePageType);
        UpdateBreadcrumb(e.SourcePageType);

        SelectNavigationItem(e.SourcePageType);
        motionService.AnimatePageEntrance(
            pageTransition,
            new PageTransitionTarget(
                RootFrame,
                PageTransitionChrome,
                PageTransitionScaleTransform
            )
        );
    }

    private void UpdateBreadcrumb(Type sourcePageType)
    {
        if (
            !IsBreadcrumbFeatureEnabled()
        )
        {
            BreadcrumbHost.Visibility = Visibility.Collapsed;
            return;
        }

        if (
            options.BreadcrumbShowOption == BreadcrumbShowOption.Auto
            && !HasBreadcrumbNavigation()
        )
        {
            BreadcrumbHost.Visibility = Visibility.Collapsed;
            return;
        }

        var label =
            navigationItemsByPage.GetValueOrDefault(sourcePageType)?.Label
            ?? sourcePageType.Name;

        BreadcrumbText.Text = $"{options.Title} / {label}";
        BreadcrumbHost.Visibility = Visibility.Visible;
    }

    private void ActivateNavigationItem(
        FlourishNavigationItem item,
        bool addToBackStack,
        bool toggleChildren = true
    )
    {
        if (!item.IsEnabled)
        {
            return;
        }

        if (toggleChildren)
        {
            OpenNavigationPaneForCollapsedParent(item);
        }

        if (toggleChildren && item.HasChildren)
        {
            ToggleChildItems(item);
        }

        if (item.IsCommandItem)
        {
            if (!item.HasChildren && !string.IsNullOrWhiteSpace(item.CommandKey))
            {
                _ = commandParser.ExecuteAsync(
                    item.CommandKey,
                    source: CommandSource.Navigation
                ).AsTask();
            }

            return;
        }

        if (item.PageType is null || navigationService.CurrentNavigationKey == item.Key)
        {
            return;
        }

        navigationService.Navigate(item.Key, addToBackStack: addToBackStack);

        if (!addToBackStack && navigationService.CanGoBack)
        {
            navigationService.ClearBackStack();
            UpdateTitlebarBreadcrumbNavigation();
        }
    }

    private void ToggleChildItems(FlourishNavigationItem parent)
    {
        parent.IsExpanded = !parent.IsExpanded;
        navigationMenuService.RecordExpansion(parent.Id, parent.IsExpanded);
        SetChildItemsVisibility(parent, parent.IsExpanded);
    }

    private void OpenNavigationPaneForCollapsedParent(FlourishNavigationItem item)
    {
        if (isPaneOpen || !item.HasChildren)
        {
            return;
        }

        navigationPanelService.Open(animate: true);
    }

    private void CollapseAllNavigationChildren()
    {
        foreach (var parent in navigationParentsByKey.Values)
        {
            parent.IsExpanded = false;
            navigationMenuService.RecordExpansion(parent.Id, expanded: false);
            SetChildItemsVisibility(parent, false);
        }
    }

    private void ExpandAncestorsForSelection(FlourishNavigationItem item)
    {
        if (item.ChildId == 0)
        {
            return;
        }

        var parent = FindParentNavigationItem(item);
        if (parent is null)
        {
            return;
        }

        parent.IsExpanded = true;
        navigationMenuService.RecordExpansion(parent.Id, expanded: true);
        SetChildItemsVisibility(parent, true);
    }

    private void SetChildItemsVisibility(FlourishNavigationItem parent, bool isVisible)
    {
        foreach (var child in GetChildNavigationItems(parent))
        {
            child.IsTreeVisible = isVisible;
        }
    }

    private IEnumerable<FlourishNavigationItem> GetChildNavigationItems(
        FlourishNavigationItem parent
    )
    {
        return parent.ParentId != 0
            && navigationChildrenByParentKey.TryGetValue(
                CreateNavigationTreeKey(parent, parent.ParentId),
                out var children
            )
            ? children
            : [];
    }

    private FlourishNavigationItem? FindParentNavigationItem(FlourishNavigationItem child)
    {
        return child.ChildId == 0
            ? null
            : navigationParentsByKey.GetValueOrDefault(
                CreateNavigationTreeKey(child, child.ChildId)
            );
    }

    private static NavigationTreeKey CreateNavigationTreeKey(
        FlourishNavigationItem item,
        int relationshipId
    )
    {
        return new NavigationTreeKey(item.IsFixed, item.GroupId, relationshipId);
    }

    private bool IsBreadcrumbFeatureEnabled()
    {
        return options.IsBreadcrumbEnabled
            && options.BreadcrumbShowOption != BreadcrumbShowOption.Hidden;
    }

    private bool HasBreadcrumbNavigation()
    {
        return navigationService.CanGoBack || navigationService.CanGoForward;
    }

    private void UpdateTitlebarBreadcrumbNavigation()
    {
        if (!options.IsTitlebarEnabled)
        {
            return;
        }

        var isVisible = IsBreadcrumbFeatureEnabled()
            && (
                options.BreadcrumbShowOption == BreadcrumbShowOption.Always
                || HasBreadcrumbNavigation()
            );

        Titlebar.SetBreadcrumbNavigationState(
            isVisible,
            navigationService.CanGoBack,
            navigationService.CanGoForward
        );
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

    private void Titlebar_SearchTextChanged(object? sender, string searchText)
    {
        titleBarSearchService.PublishFromView(searchText);
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
            new(
                "no",
                localizationService.Get(FlourishLocaleKeys.MessageBoxNo)
            )
            {
                IsDefault = true,
                IsCancel = true,
            },
            new(
                "yes",
                localizationService.Get(FlourishLocaleKeys.MessageBoxYes)
            )
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
                && !await ConfirmCloseRequestAsync(cancellationToken)
            )
            {
                return false;
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
        lock (backgroundTaskRefreshGate)
        {
            isShellClosed = true;
        }

        backgroundTaskService.TasksChanged -= BackgroundTaskService_TasksChanged;
        navigationPanelService.Changed -= NavigationPanelService_Changed;
        navigationMenuService.Changed -= NavigationMenuService_Changed;
        toolbarService.Changed -= ToolbarService_Changed;
        statusService.Changed -= StatusService_Changed;
        shellRegionService.Changed -= ShellRegionService_Changed;
        titleBarService.Changed -= TitleBarService_Changed;
        titleBarLogoLoadCoordinator.Dispose();
        titleBarSearchService.StateChanged -= TitleBarSearchService_StateChanged;
        notificationService.NotificationsChanged -= NotificationService_NotificationsChanged;
        profileFlyoutService.Changed -= ProfileFlyoutService_Changed;
        shellFeatureService.Changed -= ShellFeatureService_Changed;
        localizationService.Changed -= LocalizationService_Changed;
        fontService.Changed -= FontService_Changed;
        motionService.Changed -= MotionService_Changed;
        commandParser.Changed -= CommandParser_Changed;
        commandParser.CanExecuteChanged -= CommandParser_CanExecuteChanged;
        navigationService.Navigated -= RootFrame_Navigated;
        backgroundTaskRefreshTimer.Stop();
        backgroundTaskRefreshTimer.Tick -= BackgroundTaskRefreshTimer_Tick;
        ResetBackgroundTaskRefreshLoop();
        if (isProfileServiceSubscribed)
        {
            profileService.ProfileChanged -= ProfileService_ProfileChanged;
            isProfileServiceSubscribed = false;
        }
        themeService.ThemeChanged -= ThemeService_ThemeChanged;
        themeService.Detach(this);
        materialEffectService.Detach(this);
        pageTransition.Cancel();
        StopNavigationPaneAnimations();
        windowCloseService.Detach();
        ClearToolbarButtonCache();
        foreach (var regionElement in regionElementsById.Values)
        {
            DisposeRegionElement(regionElement.Element);
        }

        regionElementsById.Clear();
    }

    private void MainWindow_StateChanged(object? sender, EventArgs e)
    {
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

    private void SelectNavigationItem(Type pageType)
    {
        if (navigationItemsByPage.TryGetValue(pageType, out var item))
        {
            SelectNavigationItem(item);
        }
    }

    private void SelectNavigationItem(FlourishNavigationItem item)
    {
        if (!item.IsPageItem)
        {
            RestoreSelectedNavigationItem();
            return;
        }

        selectedNavigationItem = item;
        suppressNavigationSelection = true;
        try
        {
            ExpandAncestorsForSelection(item);
            UpdateActiveChildParent(item);

            if (item.IsFixed)
            {
                NavigationItemsHost.SelectedItem = null;
                FixedNavigationItemsHost.SelectedItem = item;
                return;
            }

            FixedNavigationItemsHost.SelectedItem = null;
            NavigationItemsHost.SelectedItem = item;
        }
        finally
        {
            suppressNavigationSelection = false;
        }
    }

    private void RestoreSelectedNavigationItem()
    {
        if (selectedNavigationItem is not null)
        {
            SelectNavigationItem(selectedNavigationItem);
            return;
        }

        suppressNavigationSelection = true;
        try
        {
            NavigationItemsHost.SelectedItem = null;
            FixedNavigationItemsHost.SelectedItem = null;
        }
        finally
        {
            suppressNavigationSelection = false;
        }
    }

    private void UpdateActiveChildParent(FlourishNavigationItem activeItem)
    {
        var parent = activeItem.IsPageItem && activeItem.ChildId != 0
            ? FindParentNavigationItem(activeItem)
            : null;

        if (activeChildParentItem == parent)
        {
            return;
        }

        if (activeChildParentItem is not null)
        {
            activeChildParentItem.IsActiveChildParent = false;
        }

        activeChildParentItem = parent;
        if (activeChildParentItem is not null)
        {
            activeChildParentItem.IsActiveChildParent = true;
        }
    }

    private static T? FindAncestor<T>(DependencyObject source)
        where T : DependencyObject
    {
        var current = source;
        while (current is not null)
        {
            if (current is T match)
            {
                return match;
            }

            current = VisualTreeHelper.GetParent(current);
        }

        return null;
    }

    private static FlourishNavigationItem? GetNavigationItemFromInputSource(object source)
    {
        return source is DependencyObject dependencyObject
            && FindAncestor<ListBoxItem>(dependencyObject)?.DataContext is FlourishNavigationItem item
            ? item
            : null;
    }
}
