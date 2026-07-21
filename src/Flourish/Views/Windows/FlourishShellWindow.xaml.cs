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
using ArkheideSystem.Flourish.Internal.Imaging;
using ArkheideSystem.Flourish.Internal.Interaction;
using ArkheideSystem.Flourish.Services;
using Microsoft.Extensions.DependencyInjection;
using Button = ArkheideSystem.Flourish.Controls.Button;
using ListBox = ArkheideSystem.Flourish.Controls.FlourishListBox;
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

    private readonly INavigationService navigationService;
    private readonly IFrameNavigationService frameNavigationService;
    private readonly NavigationPanelService navigationPanelService;
    private readonly NavigationMenuService navigationMenuService;
    private readonly FlourishToolbarService toolbarService;
    private readonly FlourishStatusService statusService;
    private readonly StatusItemViewCache statusItemViews;
    private readonly ShellRegionService shellRegionService;
    private readonly IBackgroundTaskService backgroundTaskService;
    private readonly IMessageService messageService;
    private readonly NotificationService notificationService;
    private readonly TrayIconService trayIconService;
    private readonly ICommandRegistry commandRegistry;
    private readonly ICommandDispatcher commandDispatcher;
    private readonly ShortcutService shortcutService;
    private readonly FontService fontService;
    private readonly MaterialEffectService materialEffectService;
    private readonly ThemeService themeService;
    private readonly FlourishMotionService motionService;
    private readonly TitleBarService titleBarService;
    private readonly ProjectService projectService;
    private readonly IProjectBehavior projectBehavior;
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
    private readonly ICommandRegistration projectSaveCommandRegistration;
    private readonly IShortcutRegistration projectSaveShortcutRegistration;
    private readonly IWindowCloseGuardRegistration projectCloseGuardRegistration;
    private readonly CancellationTokenSource projectBehaviorCancellation = new();
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
    private readonly ToolbarCommandButtonIndex toolbarCommandButtons;
    private readonly Dictionary<string, RegionElementView> regionElementsById = new(
        StringComparer.Ordinal
    );
    private readonly Dictionary<string, FlourishComboBoxItem> projectSelectorItemsById = new(
        StringComparer.Ordinal
    );
    private readonly Dictionary<Guid, BackgroundTaskIconView> backgroundTaskIconsById = [];
    private readonly Dictionary<Guid, BackgroundTaskRowView> backgroundTaskRowsById = [];
    private readonly Dictionary<string, NotificationItemView> notificationViewsById = new(
        StringComparer.Ordinal
    );
    private readonly Lock backgroundTaskRefreshGate = new();
    private readonly Lock notificationRefreshGate = new();
    private readonly Lock statusRefreshGate = new();
    private readonly NavigationPaneTransitionController navigationPaneTransition = new();
    private readonly PageTransitionController pageTransition = new();
    private readonly TitleBarLogoLoadCoordinator titleBarLogoLoadCoordinator = new(
        TitleBarVisualAssets.LoadLogoAsync
    );
    private readonly DispatcherTimer backgroundTaskRefreshTimer;
    private FlourishStatusBarSnapshot statusBarSnapshot;
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
    private bool notificationRefreshPending;
    private IReadOnlyList<FlourishNotificationInfo> pendingNotifications = [];
    private long pendingNotificationVersion;
    private long appliedNotificationVersion;
    private volatile bool isShellClosed;
    private bool statusFlyoutOpenedWithFocus;
    private FrameworkElement? titleBarFlyoutAnchor;
    private IInputElement? titleBarFlyoutRestoreFocusTarget;
    private TitleBarFlyoutKind titleBarFlyoutKind;
    private bool titleBarFlyoutOpenedWithFocus;
    private bool suppressProjectSelectionChanged;
    private bool isProjectBehaviorPending;
    private bool allowClose;
    private bool closeRequestPending;
    private bool isProfileServiceSubscribed;
    private Type? materializedProfileConfiguredPageType;
    private ImageSource? currentTitleBarLogoSource;
    private string currentTitleBarLogoFallbackText = "F";
    private bool isTitleBarLogoVisible;
    private bool lastToolbarEnabled;
    private long appliedToolbarVersion;
    private FlourishTitleBarState? appliedTitleBarState;
    private long appliedTitleBarVersion;
    private long appliedProjectVersion;
    private FlourishStatusBarChangedEventArgs? pendingStatusChange;
    private bool statusRefreshPending;
    private FlourishComboBoxItem? applicationTitleSelectorItem;
    private FlourishComboBoxItem? projectPlaceholderSelectorItem;
    private FlourishComboBoxItem? newProjectSelectorItem;

    private readonly record struct NavigationTreeKey(bool IsFixed, int GroupId, int RelationshipId);

    private enum StatusFlyoutKind
    {
        None,
        BackgroundTasks,
        System,
    }

    private enum TitleBarFlyoutKind
    {
        None,
        ApplicationInfo,
    }

    private enum ProjectMenuItemKind
    {
        Application,
        Project,
        Placeholder,
        NewProject,
    }

    private sealed record ProjectMenuItemTag(ProjectMenuItemKind Kind, string? ProjectId = null);

    private FlourishComboBox ProjectComboBox => Titlebar.TitleSelector;

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

    private sealed class NotificationItemView(
        Border container,
        TextBlock icon,
        TextBlock title,
        TextBlock message,
        Button action,
        Button dismiss
    )
    {
        public Border Container { get; } = container;

        public TextBlock Icon { get; } = icon;

        public TextBlock Title { get; } = title;

        public TextBlock Message { get; } = message;

        public Button Action { get; } = action;

        public Button Dismiss { get; } = dismiss;

        public long Version { get; set; } = -1;
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
        ICommandRegistry commandRegistry,
        ICommandDispatcher commandDispatcher,
        ShortcutService shortcutService,
        FontService fontService,
        MaterialEffectService materialEffectService,
        ThemeService themeService,
        FlourishMotionService motionService,
        TitleBarService titleBarService,
        ProjectService projectService,
        IProjectBehavior projectBehavior,
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
        backgroundTaskRefreshTimer = new DispatcherTimer(DispatcherPriority.Background, Dispatcher)
        {
            Interval = TimeSpan.FromMilliseconds(33),
        };
        backgroundTaskRefreshTimer.Tick += BackgroundTaskRefreshTimer_Tick;

        this.navigationService = navigationService;
        this.frameNavigationService = frameNavigationService;
        this.navigationPanelService = navigationPanelService;
        this.navigationMenuService = navigationMenuService;
        this.toolbarService = toolbarService;
        var toolbarSnapshot = toolbarService.Current;
        lastToolbarEnabled = toolbarSnapshot.IsEnabled;
        appliedToolbarVersion = toolbarSnapshot.Version;
        this.statusService = statusService;
        statusItemViews = new StatusItemViewCache(StatusItemsHost);
        statusBarSnapshot = statusService.Current;
        this.shellRegionService = shellRegionService;
        this.backgroundTaskService = backgroundTaskService;
        this.messageService = messageService;
        this.notificationService = notificationService;
        appliedNotificationVersion = notificationService.CurrentVersion;
        this.trayIconService = trayIconService;
        this.commandRegistry = commandRegistry;
        this.commandDispatcher = commandDispatcher;
        toolbarCommandButtons = new ToolbarCommandButtonIndex(commandDispatcher);
        this.shortcutService = shortcutService;
        this.fontService = fontService;
        this.materialEffectService = materialEffectService;
        this.themeService = themeService;
        this.motionService = motionService;
        this.titleBarService = titleBarService;
        appliedTitleBarVersion = titleBarService.CurrentVersion;
        this.projectService = projectService;
        appliedProjectVersion = projectService.Current.Version;
        this.projectBehavior = projectBehavior;
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
        appliedTitleBarState = titleBarService.Current;
        windowService.Attach(this);
        windowCloseService.Attach(RequestCloseCoreAsync);
        projectSaveCommandRegistration = commandRegistry.Register(
            ProjectSaveCommandKey,
            SaveActiveProjectCommandAsync,
            _ =>
                projectService.Current.IsMultiProjectEnabled
                && projectService.Current.ActiveProject is not null,
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
        BuildRegionContents();
        ConfigureProfileSurface();
        BuildToolbarItems();
        BuildNavigationItems();
        statusService.Changed += StatusService_Changed;
        statusBarSnapshot = statusService.Current;
        statusItemViews.Synchronize(statusBarSnapshot);
        UpdateStatusBarVisibility();
        BuildNotifications(notificationService.ActiveNotifications);
        navigationPanelService.Changed += NavigationPanelService_Changed;
        navigationMenuService.Changed += NavigationMenuService_Changed;
        toolbarService.Changed += ToolbarService_Changed;
        shellRegionService.Changed += ShellRegionService_Changed;
        titleBarService.Changed += TitleBarService_Changed;
        projectService.Changed += ProjectService_Changed;
        titleBarSearchService.ProgrammaticStateChanged +=
            TitleBarSearchService_StateChanged;
        notificationService.NotificationsChanged += NotificationService_NotificationsChanged;
        profileFlyoutService.Changed += ProfileFlyoutService_Changed;
        shellFeatureService.Changed += ShellFeatureService_Changed;
        localizationService.Changed += LocalizationService_Changed;
        fontService.Changed += FontService_Changed;
        motionService.Changed += MotionService_Changed;
        commandRegistry.Changed += CommandRegistry_Changed;
        commandRegistry.CanExecuteChanged += CommandRegistry_CanExecuteChanged;
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
        Title = options.ApplicationTitle;
        RequestTitleBarLogo(
            options.LogoPath,
            options.LogoFallbackText,
            options.IsTitlebarLogoEnabled
        );
        Titlebar.SetDisplayTitle(GetDisplayedTitle());
        BuildTitleSelectorItems();
        Titlebar.SetSearchPlaceholder(options.SearchPlaceholder);
        Titlebar.ConfigureVisibility(
            options.IsTitlebarSearchEnabled,
            IsBreadcrumbFeatureEnabled(),
            options.IsTitlebarNavigationToggleEnabled && options.IsNavigationPanelEnabled,
            options.IsTitlebarLogoEnabled,
            options.IsTitlebarTitleEnabled || options.IsMultiProjectEnabled,
            options.IsTitlebarThemeToggleEnabled && options.IsThemeEnabled,
            options.IsProfileEnabled && options.IsTitlebarProfileEnabled
        );
        UpdateStatusBarVisibility();
        ApplyContentLayoutOptions();
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
            "FlourishShellBackgroundBrush"
        );
        themeService.Attach(this);
        ApplyThemeState();
        trayIconService.Initialize(this, options.ApplicationTitle);
    }

    private void ConfigureProfileSurface()
    {
        var state = profileFlyoutService.Current;
        if (options.IsTitlebarEnabled && state.IsEnabled && options.IsTitlebarProfileEnabled)
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
            System.Diagnostics.Debug.WriteLine($"Flourish profile initialization failed: {error}");
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
        CloseTitleBarFlyout(restoreFocus: false);
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
        Titlebar.SetMaximizeEnabled(
            ResizeMode is ResizeMode.CanResize or ResizeMode.CanResizeWithGrip
        );
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
            windowFrameFixService.ApplyFrameTransition(this, useCustomFrame, ApplyFrameAndSurface);
        }
        else
        {
            ApplyFrameAndSurface();
        }

        Titlebar.SetMaximized(WindowState == WindowState.Maximized);
        EnsureTitleBarFlyoutIsAvailable();
        if (!options.IsTitlebarEnabled || !titleBarSearchService.Current.FocusRequested)
        {
            return;
        }

        Dispatcher.BeginInvoke(
            new Action(() =>
            {
                if (options.IsTitlebarEnabled && titleBarSearchService.Current.FocusRequested)
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
        Titlebar.LogoHoverRequested += Titlebar_LogoHoverRequested;
        Titlebar.LogoClickRequested += Titlebar_LogoClickRequested;
        Titlebar.TitleSelectionChanged += ProjectComboBox_SelectionChanged;
        Titlebar.TitleDropDownOpened += Titlebar_TitleDropDownOpened;
        Titlebar.InteractionStarted += Titlebar_InteractionStarted;
        Titlebar.SearchTextChanged += Titlebar_SearchTextChanged;
    }

    private void Titlebar_InteractionStarted(object? sender, EventArgs e)
    {
        CloseTitleBarFlyout(restoreFocus: false);
    }

    private void Titlebar_LogoHoverRequested(object? sender, EventArgs e)
    {
        if (titleBarFlyoutOpenedWithFocus)
        {
            return;
        }

        OpenApplicationInfoFlyout(focusFlyout: false);
    }

    private void Titlebar_LogoClickRequested(object? sender, EventArgs e)
    {
        OpenApplicationInfoFlyout(focusFlyout: true);
    }

    private void Titlebar_TitleDropDownOpened(object? sender, EventArgs e)
    {
        CloseStatusFlyout(restoreFocus: false);
        CloseProfileOverlay();
        CloseTitleBarFlyout(restoreFocus: false);
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
        CloseTitleBarFlyout(restoreFocus: false);
        profileFlyoutService.Toggle();
    }

    private void CloseProfileOverlay()
    {
        profileFlyoutService.Hide();
    }

    private void ProfileOverlay_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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
            if (TitleBarFlyoutOverlay.Visibility == Visibility.Visible)
            {
                CloseTitleBarFlyout();
                e.Handled = true;
                return;
            }

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
            ApplyNavigationPaneState();
        }

        UpdateProfileCardPosition();
        UpdateStatusFlyoutPosition();
        UpdateTitleBarFlyoutPosition();
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
        const double edgeSafeMargin = 14;
        const double anchorGap = 6;
        if (
            ProfileOverlay.Visibility != Visibility.Visible
            || ShellRootGrid.ActualWidth <= edgeSafeMargin * 2
            || ShellRootGrid.ActualHeight <= edgeSafeMargin * 2
        )
        {
            return;
        }

        var anchor = Titlebar.GetProfileButtonBounds(ShellRootGrid);
        var availableWidth = Math.Max(0, ShellRootGrid.ActualWidth - edgeSafeMargin * 2);
        ProfileCard.MaxWidth = availableWidth;

        var cardWidth =
            ProfileCard.ActualWidth > 0
                ? Math.Min(ProfileCard.ActualWidth, availableWidth)
                : Math.Min(ProfileCard.Width, availableWidth);
        var desiredLeft = anchor.Left + (anchor.Width - cardWidth) / 2;
        var maximumLeft = Math.Max(
            edgeSafeMargin,
            ShellRootGrid.ActualWidth - cardWidth - edgeSafeMargin
        );
        var left = Math.Clamp(desiredLeft, edgeSafeMargin, maximumLeft);

        var top = Math.Max(edgeSafeMargin, anchor.Bottom + anchorGap);
        ProfileCard.MaxHeight = Math.Max(0, ShellRootGrid.ActualHeight - top - edgeSafeMargin);

        Canvas.SetLeft(ProfileCard, left);
        Canvas.SetTop(ProfileCard, top);
    }

    private string GetDisplayedTitle()
    {
        return GetDisplayedTitle(titleBarService.Current, projectService.Current);
    }

    private static string GetDisplayedTitle(
        FlourishTitleBarState titleState,
        FlourishProjectSnapshot projectState
    )
    {
        return projectState.IsMultiProjectEnabled
            ? projectState.ActiveProject is { } activeProject
                ? GetProjectDisplayTitle(activeProject, titleState)
                : titleState.UnnamedProjectPlaceholder
            : titleState.ApplicationTitle;
    }

    private static string GetProjectDisplayTitle(
        FlourishProject project,
        FlourishTitleBarState titleState
    )
    {
        return project.StoragePath is null
            ? titleState.UnnamedProjectPlaceholder
            : project.Name;
    }

    private void EnsureTitleBarFlyoutIsAvailable()
    {
        if (TitleBarFlyoutOverlay.Visibility != Visibility.Visible)
        {
            return;
        }

        var titleState = titleBarService.Current;
        var isAvailable = options.IsTitlebarEnabled
            && (
                titleBarFlyoutKind switch
                {
                    TitleBarFlyoutKind.ApplicationInfo => titleState.IsLogoVisible,
                    _ => false,
                }
            );
        if (!isAvailable)
        {
            CloseTitleBarFlyout(restoreFocus: false);
        }
    }

    private void OpenApplicationInfoFlyout(bool focusFlyout)
    {
        if (!options.IsTitlebarEnabled || !titleBarService.Current.IsLogoVisible)
        {
            return;
        }

        BuildApplicationInfoFlyoutContent();
        OpenTitleBarFlyout(
            Titlebar.GetLogoButtonAnchor(),
            TitleBarFlyoutKind.ApplicationInfo,
            focusFlyout
        );
    }

    private void OpenTitleBarFlyout(
        FrameworkElement anchor,
        TitleBarFlyoutKind kind,
        bool focusFlyout
    )
    {
        CloseStatusFlyout(restoreFocus: false);
        CloseProfileOverlay();

        var wasVisible = TitleBarFlyoutOverlay.Visibility == Visibility.Visible;
        if (focusFlyout && !titleBarFlyoutOpenedWithFocus)
        {
            titleBarFlyoutOpenedWithFocus = true;
            titleBarFlyoutRestoreFocusTarget = anchor;
        }
        else if (!focusFlyout && !wasVisible)
        {
            titleBarFlyoutOpenedWithFocus = false;
            titleBarFlyoutRestoreFocusTarget = null;
        }

        titleBarFlyoutAnchor = anchor;
        titleBarFlyoutKind = kind;
        TitleBarFlyoutCard.PlacementTarget = anchor;
        TitleBarFlyoutCard.Variant = OverlayVariant.Temporary;
        ApplicationInfoContent.Visibility =
            kind == TitleBarFlyoutKind.ApplicationInfo
                ? Visibility.Visible
                : Visibility.Collapsed;
        TitleBarFlyoutOverlay.Visibility = Visibility.Visible;
        Dispatcher.BeginInvoke(
            new Action(() =>
            {
                UpdateTitleBarFlyoutPosition();
                if (focusFlyout)
                {
                    FocusTitleBarFlyoutContent();
                }
            })
        );
    }

    private void BuildApplicationInfoFlyoutContent()
    {
        var titleState = titleBarService.Current;
        var projectState = projectService.Current;
        var source = currentTitleBarLogoSource;
        ApplicationInfoLogoImage.Source = source;
        ApplicationInfoLogoImage.Visibility = source is null
            ? Visibility.Collapsed
            : Visibility.Visible;
        ApplicationInfoLogoFallback.Text = string.IsNullOrWhiteSpace(
            titleState.LogoFallbackText
        )
            ? "F"
            : titleState.LogoFallbackText[..1];
        ApplicationInfoLogoFallback.Visibility = source is null
            ? Visibility.Visible
            : Visibility.Collapsed;

        ApplicationInfoTitle.Text = titleState.ApplicationTitle;
        ApplicationInfoTitle.Visibility =
            titleState.ShowApplicationTitle
            && !string.IsNullOrWhiteSpace(titleState.ApplicationTitle)
                ? Visibility.Visible
                : Visibility.Collapsed;
        ApplicationInfoSubTitle.Text = titleState.ApplicationSubTitle;
        ApplicationInfoSubTitle.Visibility =
            titleState.ShowApplicationSubTitle
            && !string.IsNullOrWhiteSpace(titleState.ApplicationSubTitle)
                ? Visibility.Visible
                : Visibility.Collapsed;

        var projectTitle = projectState.IsMultiProjectEnabled
            && projectState.ActiveProject is { } activeProject
            ? GetProjectDisplayTitle(activeProject, titleState)
            : null;

        ApplicationInfoProjectTitle.Text = projectTitle ?? string.Empty;
        ApplicationInfoProjectTitle.Visibility =
            titleState.ShowProjectTitle && !string.IsNullOrWhiteSpace(projectTitle)
                ? Visibility.Visible
                : Visibility.Collapsed;
        ApplicationInfoBodyScrollViewer.Visibility =
            ApplicationInfoBodyHost.Children.Count > 0
            ? Visibility.Visible
            : Visibility.Collapsed;

        var identityParts = new[]
        {
            ApplicationInfoTitle.Visibility == Visibility.Visible
                ? ApplicationInfoTitle.Text
                : null,
            ApplicationInfoSubTitle.Visibility == Visibility.Visible
                ? ApplicationInfoSubTitle.Text
                : null,
            ApplicationInfoProjectTitle.Visibility == Visibility.Visible
                ? ApplicationInfoProjectTitle.Text
                : null,
        }.Where(value => !string.IsNullOrWhiteSpace(value));
        AutomationProperties.SetName(TitleBarFlyoutCard, string.Join(", ", identityParts));
    }

    private void BuildTitleSelectorItems(
        FlourishTitleBarState? titleState = null,
        FlourishProjectSnapshot? projectState = null
    )
    {
        titleState ??= titleBarService.Current;
        projectState ??= projectService.Current;
        FlourishComboBoxItem? selectedItem = null;
        var desiredItems = new List<UIElement>(projectState.Projects.Count + 2);
        suppressProjectSelectionChanged = true;
        try
        {
            var activeIds = projectState.Projects
                .Select(project => project.Id)
                .ToHashSet(StringComparer.Ordinal);
            foreach (
                var removedId in projectSelectorItemsById.Keys
                    .Where(id => !activeIds.Contains(id))
                    .ToArray()
            )
            {
                projectSelectorItemsById.Remove(removedId);
            }

            if (projectState.IsMultiProjectEnabled)
            {
                foreach (var project in projectState.Projects)
                {
                    if (!projectSelectorItemsById.TryGetValue(project.Id, out var item))
                    {
                        item = CreateProjectComboBoxItem(project, titleState);
                        projectSelectorItemsById.Add(project.Id, item);
                    }
                    else
                    {
                        UpdateProjectComboBoxItem(item, project, titleState);
                    }

                    desiredItems.Add(item);
                    if (
                        StringComparer.Ordinal.Equals(
                            projectState.ActiveProject?.Id,
                            project.Id
                        )
                    )
                    {
                        selectedItem = item;
                    }
                }

                if (selectedItem is null)
                {
                    selectedItem = projectPlaceholderSelectorItem ??=
                        CreateProjectPlaceholderComboBoxItem(titleState);
                    selectedItem.Content = titleState.UnnamedProjectPlaceholder;
                    desiredItems.Add(selectedItem);
                }

                newProjectSelectorItem ??= CreateNewProjectComboBoxItem();
                var newProjectLabel = localizationService.Get(
                    FlourishLocaleKeys.TitleBarNewProject
                );
                newProjectSelectorItem.Content = newProjectLabel;
                AutomationProperties.SetName(newProjectSelectorItem, newProjectLabel);
                desiredItems.Add(newProjectSelectorItem);
            }
            else
            {
                selectedItem = applicationTitleSelectorItem ??=
                    CreateApplicationTitleComboBoxItem(titleState.ApplicationTitle);
                selectedItem.Content = titleState.ApplicationTitle;
                AutomationProperties.SetName(selectedItem, titleState.ApplicationTitle);
                desiredItems.Add(selectedItem);
            }

            SynchronizeItems(ProjectComboBox, desiredItems);
            ProjectComboBox.SelectedItem = selectedItem;
        }
        finally
        {
            suppressProjectSelectionChanged = false;
        }

        var displayTitle = GetDisplayedTitle(titleState, projectState);
        AutomationProperties.SetName(ProjectComboBox, displayTitle);
    }

    private void UpdateProjectComboBoxItem(
        FlourishComboBoxItem item,
        FlourishProject project,
        FlourishTitleBarState titleState
    )
    {
        var displayTitle = GetProjectDisplayTitle(project, titleState);
        if (!StringComparer.Ordinal.Equals(item.Content as string, displayTitle))
        {
            item.Content = displayTitle;
            AutomationProperties.SetName(item, displayTitle);
        }

        if (
            item.Tag is not ProjectMenuItemTag tag
            || tag.Kind != ProjectMenuItemKind.Project
            || !StringComparer.Ordinal.Equals(tag.ProjectId, project.Id)
        )
        {
            item.Tag = new ProjectMenuItemTag(ProjectMenuItemKind.Project, project.Id);
        }

        if (project.StoragePath is null)
        {
            item.ToolTip = null;
        }
        else if (item.ToolTip is FlourishToolTip toolTip)
        {
            if (!StringComparer.Ordinal.Equals(toolTip.Content as string, project.StoragePath))
            {
                toolTip.Content = project.StoragePath;
            }
        }
        else
        {
            item.ToolTip = new FlourishToolTip { Content = project.StoragePath };
        }
        if (
            item.ContextMenu?.Items.Count > 0
            && item.ContextMenu.Items[0] is System.Windows.Controls.MenuItem deleteItem
        )
        {
            deleteItem.Header = localizationService.Get(FlourishLocaleKeys.ProjectDelete);
            deleteItem.Tag = project.Id;
        }
    }

    private static FlourishComboBoxItem CreateApplicationTitleComboBoxItem(string title)
    {
        var item = new FlourishComboBoxItem
        {
            Content = title,
            Tag = new ProjectMenuItemTag(ProjectMenuItemKind.Application),
        };
        ConfigureTitleSelectorDropDownItem(item);
        AutomationProperties.SetName(item, title);
        return item;
    }

    private FlourishComboBoxItem CreateProjectComboBoxItem(
        FlourishProject project,
        FlourishTitleBarState titleState
    )
    {
        var displayTitle = GetProjectDisplayTitle(project, titleState);
        var item = new FlourishComboBoxItem
        {
            Content = displayTitle,
            Tag = new ProjectMenuItemTag(ProjectMenuItemKind.Project, project.Id),
        };
        ConfigureTitleSelectorDropDownItem(item);
        if (project.StoragePath is not null)
        {
            item.ToolTip = new FlourishToolTip { Content = project.StoragePath };
        }

        var deleteItem = new System.Windows.Controls.MenuItem
        {
            Header = localizationService.Get(FlourishLocaleKeys.ProjectDelete),
            Tag = project.Id,
        };
        deleteItem.SetResourceReference(
            System.Windows.Controls.MenuItem.FontSizeProperty,
            "FlourishFontSizeStandard"
        );
        deleteItem.Click += ProjectDeleteMenuItem_Click;
        item.ContextMenu = new System.Windows.Controls.ContextMenu
        {
            Items = { deleteItem },
        };
        AutomationProperties.SetName(item, displayTitle);
        return item;
    }

    private FlourishComboBoxItem CreateProjectPlaceholderComboBoxItem(
        FlourishTitleBarState titleState
    )
    {
        var item = new FlourishComboBoxItem
        {
            Content = titleState.UnnamedProjectPlaceholder,
            IsEnabled = false,
            Tag = new ProjectMenuItemTag(ProjectMenuItemKind.Placeholder),
        };
        ConfigureTitleSelectorDropDownItem(item);
        return item;
    }

    private FlourishComboBoxItem CreateNewProjectComboBoxItem()
    {
        var label = localizationService.Get(FlourishLocaleKeys.TitleBarNewProject);
        var item = new FlourishComboBoxItem
        {
            Content = label,
            Tag = new ProjectMenuItemTag(ProjectMenuItemKind.NewProject),
        };
        ConfigureTitleSelectorDropDownItem(item);
        AutomationProperties.SetName(item, label);
        return item;
    }

    private static void ConfigureTitleSelectorDropDownItem(FlourishComboBoxItem item)
    {
        item.SetResourceReference(
            System.Windows.Controls.Control.FontSizeProperty,
            "FlourishFontSizeStandard"
        );
        item.FontWeight = FontWeights.Normal;
    }

    private async void ProjectComboBox_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e
    )
    {
        if (
            suppressProjectSelectionChanged
            || ProjectComboBox.SelectedItem is not FlourishComboBoxItem selectedItem
            || selectedItem.Tag is not ProjectMenuItemTag selection
        )
        {
            return;
        }

        switch (selection.Kind)
        {
            case ProjectMenuItemKind.Project when selection.ProjectId is { } projectId:
                if (
                    StringComparer.Ordinal.Equals(
                        projectService.Current.ActiveProject?.Id,
                        projectId
                    )
                )
                {
                    return;
                }

                ProjectComboBox.IsDropDownOpen = false;
                await ExecuteProjectBehaviorAsync(
                    "activate",
                    "Project activation failed",
                    token => projectBehavior.ActivateProjectAsync(projectId, token)
                );
                break;
            case ProjectMenuItemKind.NewProject:
                ProjectComboBox.IsDropDownOpen = false;
                await ExecuteProjectBehaviorAsync(
                    "create",
                    "Project creation failed",
                    projectBehavior.CreateProjectAsync
                );
                break;
        }
    }

    private async void ProjectDeleteMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not System.Windows.Controls.MenuItem { Tag: string projectId })
        {
            return;
        }

        await ExecuteProjectBehaviorAsync(
            "delete",
            "Project deletion failed",
            token => projectBehavior.DeleteProjectAsync(projectId, token)
        );
    }

    private async Task<bool> ExecuteProjectBehaviorAsync(
        string operationId,
        string failureTitle,
        Func<CancellationToken, ValueTask<bool>> operation
    )
    {
        if (isProjectBehaviorPending || isShellClosed)
        {
            return false;
        }

        isProjectBehaviorPending = true;
        ProjectComboBox.IsEnabled = false;
        try
        {
            return await operation(projectBehaviorCancellation.Token);
        }
        catch (OperationCanceledException)
            when (projectBehaviorCancellation.IsCancellationRequested)
        {
            return false;
        }
        catch (Exception error)
        {
            ShowProjectBehaviorFailure(operationId, failureTitle, error);
            return false;
        }
        finally
        {
            isProjectBehaviorPending = false;
            if (!isShellClosed)
            {
                ProjectComboBox.IsEnabled = true;
            }
        }
    }

    private async ValueTask<CommandResult> SaveActiveProjectCommandAsync(
        CommandContext context,
        CancellationToken cancellationToken
    )
    {
        if (!projectService.Current.IsMultiProjectEnabled)
        {
            return CommandResult.NotHandled;
        }

        using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            projectBehaviorCancellation.Token
        );
        try
        {
            return await projectBehavior.SaveActiveProjectAsync(linkedCancellation.Token)
                ? CommandResult.Handled
                : CommandResult.NotHandled;
        }
        catch (OperationCanceledException) when (linkedCancellation.IsCancellationRequested)
        {
            return CommandResult.Canceled;
        }
        catch (Exception error)
        {
            ShowProjectBehaviorFailure("save", "Project save failed", error);
            return CommandResult.Failed(error);
        }
    }

    private async ValueTask<WindowCloseDecision> ProjectBehaviorCloseGuardAsync(
        WindowCloseContext context,
        CancellationToken cancellationToken
    )
    {
        if (!projectService.Current.IsMultiProjectEnabled)
        {
            return WindowCloseDecision.Allow;
        }

        if (
            context.Reason != WindowCloseRequestReason.Tray
            && windowCloseService.Behavior == WindowCloseBehavior.MinimizeToTray
        )
        {
            return WindowCloseDecision.Allow;
        }

        try
        {
            return await projectBehavior.CanCloseAsync(cancellationToken)
                ? WindowCloseDecision.Allow
                : WindowCloseDecision.Cancel;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception error)
        {
            ShowProjectBehaviorFailure("close", "Project close check failed", error);
            return WindowCloseDecision.Cancel;
        }
    }

    private void ShowProjectBehaviorFailure(
        string operationId,
        string failureTitle,
        Exception error
    )
    {
        DispatchRuntimeChange(() =>
        {
            notificationService.Upsert(
                new FlourishNotification(
                    $"flourish.project.{operationId}.error",
                    failureTitle,
                    error.Message,
                    FlourishNotificationSeverity.Error,
                    Duration: TimeSpan.FromSeconds(8)
                )
            );
        });
    }

    private void FocusTitleBarFlyoutContent()
    {
        TitleBarFlyoutCard.Focus();
    }

    private void RefreshTitleSelector(
        FlourishTitleBarState? titleState = null,
        FlourishProjectSnapshot? projectState = null
    )
    {
        var hadKeyboardFocus = ProjectComboBox.IsKeyboardFocusWithin;
        var wasDropDownOpen = ProjectComboBox.IsDropDownOpen;

        BuildTitleSelectorItems(titleState, projectState);
        if (!hadKeyboardFocus && !wasDropDownOpen)
        {
            return;
        }

        Dispatcher.BeginInvoke(
            DispatcherPriority.Input,
            new Action(() =>
            {
                if (isShellClosed || ProjectComboBox.Visibility != Visibility.Visible)
                {
                    return;
                }

                if (hadKeyboardFocus || wasDropDownOpen)
                {
                    ProjectComboBox.Focus();
                }

                if (wasDropDownOpen)
                {
                    ProjectComboBox.IsDropDownOpen = true;
                }
            })
        );
    }

    private void CloseTitleBarFlyout(bool restoreFocus = true)
    {
        if (TitleBarFlyoutOverlay.Visibility != Visibility.Visible)
        {
            return;
        }

        TitleBarFlyoutOverlay.Visibility = Visibility.Collapsed;
        var restoreTarget = titleBarFlyoutRestoreFocusTarget ?? titleBarFlyoutAnchor;
        var shouldRestoreFocus = restoreFocus && titleBarFlyoutOpenedWithFocus;
        titleBarFlyoutAnchor = null;
        TitleBarFlyoutCard.PlacementTarget = null;
        titleBarFlyoutRestoreFocusTarget = null;
        titleBarFlyoutKind = TitleBarFlyoutKind.None;
        titleBarFlyoutOpenedWithFocus = false;
        if (shouldRestoreFocus && restoreTarget is not null)
        {
            Keyboard.Focus(restoreTarget);
        }
    }

    private void TitleBarFlyoutOverlay_PreviewMouseLeftButtonDown(
        object sender,
        MouseButtonEventArgs e
    )
    {
        var position = e.GetPosition(TitleBarFlyoutCard);
        if (
            position.X >= 0
            && position.Y >= 0
            && position.X <= TitleBarFlyoutCard.ActualWidth
            && position.Y <= TitleBarFlyoutCard.ActualHeight
        )
        {
            return;
        }

        CloseTitleBarFlyout();
        e.Handled = true;
    }

    private void TitleBarFlyoutCard_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateTitleBarFlyoutPosition();
    }

    private void TitleBarFlyoutCard_DismissRequested(object sender, RoutedEventArgs e)
    {
        CloseTitleBarFlyout();
        e.Handled = true;
    }

    private void UpdateTitleBarFlyoutPosition()
    {
        const double edgeSafeMargin = 14;
        const double anchorGap = 6;
        if (
            TitleBarFlyoutOverlay.Visibility != Visibility.Visible
            || titleBarFlyoutAnchor is null
            || ShellRootGrid.ActualWidth <= edgeSafeMargin * 2
            || ShellRootGrid.ActualHeight <= edgeSafeMargin * 2
        )
        {
            return;
        }

        var topLeft = titleBarFlyoutAnchor.TranslatePoint(
            new System.Windows.Point(),
            ShellRootGrid
        );
        var anchor = new Rect(
            topLeft,
            new System.Windows.Size(
                titleBarFlyoutAnchor.ActualWidth,
                titleBarFlyoutAnchor.ActualHeight
            )
        );
        var availableWidth = Math.Max(0, ShellRootGrid.ActualWidth - edgeSafeMargin * 2);
        TitleBarFlyoutCard.MaxWidth = availableWidth;
        var cardWidth = TitleBarFlyoutCard.ActualWidth > 0
            ? Math.Min(TitleBarFlyoutCard.ActualWidth, availableWidth)
            : Math.Min(TitleBarFlyoutCard.Width, availableWidth);
        var desiredLeft = titleBarFlyoutKind == TitleBarFlyoutKind.ApplicationInfo
            ? anchor.Left + (anchor.Width - cardWidth) / 2
            : anchor.Left;
        var maximumLeft = Math.Max(
            edgeSafeMargin,
            ShellRootGrid.ActualWidth - cardWidth - edgeSafeMargin
        );
        var left = Math.Clamp(desiredLeft, edgeSafeMargin, maximumLeft);
        var top = Math.Max(edgeSafeMargin, anchor.Bottom + anchorGap);
        TitleBarFlyoutCard.MaxHeight = Math.Max(
            0,
            ShellRootGrid.ActualHeight - top - edgeSafeMargin
        );
        Canvas.SetLeft(TitleBarFlyoutCard, left);
        Canvas.SetTop(TitleBarFlyoutCard, top);
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

            // The background-task service serializes notifications through its
            // notification gate, and the event args own an immutable snapshot.
            pendingBackgroundTasks = e.Tasks;
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
            RefreshBackgroundTaskStatus(tasks);
        }
    }

    private void StartBackgroundTaskRefreshTimer()
    {
        lock (backgroundTaskRefreshGate)
        {
            if (isShellClosed || !backgroundTaskRefreshLoopActive || Dispatcher.HasShutdownStarted)
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
        var showConfiguredContent = statusBarSnapshot.IsEnabled;
        var showStatusBar = showConfiguredContent || hasBackgroundTasks;

        StatusBarBorder.Visibility = showStatusBar ? Visibility.Visible : Visibility.Collapsed;
        StatusItemsHost.Visibility =
            showConfiguredContent && StatusItemsHost.Children.Count > 0
                ? Visibility.Visible
                : Visibility.Collapsed;
        SystemStatusButton.Visibility =
            showConfiguredContent
            && (statusBarSnapshot.IsLanStatusEnabled || statusBarSnapshot.IsPowerStatusEnabled)
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
            if (index < panel.Children.Count && ReferenceEquals(panel.Children[index], desired))
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

    private static void SynchronizeItems(
        ItemsControl itemsControl,
        IReadOnlyList<UIElement> desiredItems
    )
    {
        for (var index = 0; index < desiredItems.Count; index++)
        {
            var desired = desiredItems[index];
            if (
                index < itemsControl.Items.Count
                && ReferenceEquals(itemsControl.Items[index], desired)
            )
            {
                continue;
            }

            var existingIndex = itemsControl.Items.IndexOf(desired);
            if (existingIndex >= 0)
            {
                itemsControl.Items.RemoveAt(existingIndex);
            }

            itemsControl.Items.Insert(index, desired);
        }

        while (itemsControl.Items.Count > desiredItems.Count)
        {
            itemsControl.Items.RemoveAt(itemsControl.Items.Count - 1);
        }
    }

    private void RefreshBackgroundTaskStatus(IReadOnlyList<FlourishBackgroundTaskInfo> tasks)
    {
        backgroundTasks = tasks;
        var runningTaskIds = new HashSet<Guid>(backgroundTasks.Count);
        var desiredTaskButtons = new List<UIElement>(backgroundTasks.Count);
        var queuedTaskCount = 0;
        foreach (var task in backgroundTasks)
        {
            if (task.State == FlourishBackgroundTaskState.Queued)
            {
                queuedTaskCount++;
                continue;
            }

            if (
                task.State
                is not (
                    FlourishBackgroundTaskState.Running
                    or FlourishBackgroundTaskState.Cancelling
                )
            )
            {
                continue;
            }

            runningTaskIds.Add(task.Id);
            if (!backgroundTaskIconsById.TryGetValue(task.Id, out var iconView))
            {
                iconView = CreateBackgroundTaskIconView(task);
                backgroundTaskIconsById.Add(task.Id, iconView);
            }

            UpdateBackgroundTaskIconView(iconView, task);
            desiredTaskButtons.Add(iconView.Button);
        }

        SynchronizePanelChildren(BackgroundTaskItemsHost, desiredTaskButtons);
        RemoveStaleBackgroundTaskViews(backgroundTaskIconsById, runningTaskIds);

        BackgroundTaskQueueButton.Visibility =
            queuedTaskCount > 0 ? Visibility.Visible : Visibility.Collapsed;
        BackgroundTaskQueueCountText.Text = queuedTaskCount.ToString();
        AutomationProperties.SetName(
            BackgroundTaskQueueButton,
            localizationService.Format(
                FlourishLocaleKeys.BackgroundTaskWaitingCount,
                queuedTaskCount
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
            statusFlyoutAnchor =
                queuedTaskCount > 0
                    ? BackgroundTaskQueueButton
                    : desiredTaskButtons.Count > 0
                        ? (FrameworkElement)desiredTaskButtons[0]
                        : null;
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

    private BackgroundTaskIconView CreateBackgroundTaskIconView(FlourishBackgroundTaskInfo task)
    {
        var icon = new TextBlock
        {
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        BindIconTypography(icon, "FlourishIconFontSizeStatusBarBackgroundTask");
        var toolTipName = new TextBlock { FontWeight = FontWeights.Bold };
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
        toolTipState.SetResourceReference(
            TextBlock.ForegroundProperty,
            "FlourishNeutralForeground2Brush"
        );
        var toolTip = new StackPanel();
        toolTip.Children.Add(toolTipName);
        toolTip.Children.Add(toolTipDescription);
        toolTip.Children.Add(toolTipState);

        var button = new IconButton
        {
            Width = 26,
            Height = 22,
            MinWidth = 0,
            MinHeight = 0,
            Padding = new Thickness(),
            VerticalAlignment = VerticalAlignment.Center,
            Icon = icon,
            Variant = ButtonVariant.Text,
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

        var activeTaskIds = new HashSet<Guid>(backgroundTasks.Count);
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
                activeTaskIds.Add(task.Id);
                if (!backgroundTaskRowsById.TryGetValue(task.Id, out var rowView))
                {
                    rowView = CreateBackgroundTaskRowView(task);
                    backgroundTaskRowsById.Add(task.Id, rowView);
                }

                UpdateBackgroundTaskRowView(rowView, task);
                desiredRows.Add(rowView.Container);
            }
        }

        SynchronizeItems(StatusFlyoutContentHost, desiredRows);
        RemoveStaleBackgroundTaskViews(backgroundTaskRowsById, activeTaskIds);
    }

    private static void RemoveStaleBackgroundTaskViews<TView>(
        Dictionary<Guid, TView> viewsById,
        HashSet<Guid> activeIds
    )
    {
        List<Guid>? staleIds = null;
        foreach (var taskId in viewsById.Keys)
        {
            if (!activeIds.Contains(taskId))
            {
                (staleIds ??= []).Add(taskId);
            }
        }

        if (staleIds is null)
        {
            return;
        }

        foreach (var taskId in staleIds)
        {
            viewsById.Remove(taskId);
        }
    }

    private BackgroundTaskRowView CreateBackgroundTaskRowView(FlourishBackgroundTaskInfo task)
    {
        var row = new Border
        {
            Margin = new Thickness(0, 0, 0, 8),
            Padding = new Thickness(10, 8, 10, 8),
        };
        row.SetResourceReference(Border.BackgroundProperty, "FlourishNeutralBackground2Brush");
        row.SetResourceReference(Border.BorderBrushProperty, "FlourishSurfaceStrokeBrush");
        row.SetResourceReference(Border.BorderThicknessProperty, "FlourishSurfaceBorderThickness");
        row.SetResourceReference(Border.CornerRadiusProperty, "FlourishSurfaceCornerRadius");

        var layout = new Grid();
        layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(24) });
        layout.ColumnDefinitions.Add(
            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
        );
        layout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var icon = new TextBlock { VerticalAlignment = VerticalAlignment.Top };
        BindIconTypography(icon, "FlourishIconFontSizeBackgroundTaskView");
        icon.SetResourceReference(TextBlock.ForegroundProperty, "FlourishNeutralForeground2Brush");
        layout.Children.Add(icon);

        var details = new StackPanel();
        Grid.SetColumn(details, 1);
        var name = new TextBlock
        {
            FontWeight = FontWeights.Bold,
            TextTrimming = TextTrimming.CharacterEllipsis,
        };
        BindTextSize(name, "FlourishFontSizeStandard");
        details.Children.Add(name);
        var description = new TextBlock
        {
            Margin = new Thickness(0, 2, 8, 0),
            MaxWidth = 220,
            TextWrapping = TextWrapping.Wrap,
        };
        description.SetResourceReference(
            TextBlock.ForegroundProperty,
            "FlourishNeutralForeground2Brush"
        );
        details.Children.Add(description);

        var state = new TextBlock { Margin = new Thickness(0, 3, 8, 0) };
        BindTextSize(state, "FlourishFontSizeStandard");
        state.SetResourceReference(TextBlock.ForegroundProperty, "FlourishNeutralForeground2Brush");
        details.Children.Add(state);
        layout.Children.Add(details);

        var cancelButton = new Button
        {
            MinWidth = 58,
            Padding = new Thickness(10, 0, 10, 0),
            Tag = task.Id,
            Content = localizationService.Get(FlourishLocaleKeys.BackgroundTaskCancel),
        };
        Grid.SetColumn(cancelButton, 2);
        cancelButton.Click += CancelBackgroundTaskButton_Click;
        layout.Children.Add(cancelButton);

        row.Child = layout;
        return new BackgroundTaskRowView(row, icon, name, description, state, cancelButton);
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
                FlourishBackgroundTaskState.Cancelling =>
                    FlourishLocaleKeys.BackgroundTaskCancelling,
                _ => FlourishLocaleKeys.BackgroundTaskRunning,
            }
        );
    }

    private void SystemStatusButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
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
        StatusFlyoutContentHost.Items.Clear();

        if (statusBarSnapshot.IsLanStatusEnabled)
        {
            var networkState = localizationService.Get(FlourishLocaleKeys.SystemStatusUnknown);
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

            StatusFlyoutContentHost.Items.Add(
                CreateStatusDetailRow(
                    "\uE701",
                    localizationService.Get(FlourishLocaleKeys.SystemStatusNetwork),
                    networkState
                )
            );
        }

        if (statusBarSnapshot.IsPowerStatusEnabled)
        {
            var powerSource = localizationService.Get(FlourishLocaleKeys.SystemStatusUnknown);
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
                    && !batteryStatus.HasFlag(System.Windows.Forms.BatteryChargeStatus.Unknown);
                if (
                    hasNoSystemBattery
                    && powerStatus.PowerLineStatus != System.Windows.Forms.PowerLineStatus.Online
                )
                {
                    powerSource = localizationService.Get(FlourishLocaleKeys.SystemStatusUnknown);
                }

                if (hasUsableBattery && powerStatus.BatteryLifePercent is >= 0 and <= 1)
                {
                    powerSource = $"{powerSource} · {powerStatus.BatteryLifePercent:P0}";
                }
            }
            catch (Exception error)
            {
                System.Diagnostics.Debug.WriteLine($"Flourish power status query failed: {error}");
            }

            StatusFlyoutContentHost.Items.Add(
                CreateStatusDetailRow(
                    "\uE850",
                    localizationService.Get(FlourishLocaleKeys.SystemStatusPower),
                    powerSource
                )
            );
        }

        OpenStatusFlyout(SystemStatusButton, StatusFlyoutKind.System, focusFlyout);
    }

    private FrameworkElement CreateStatusDetailRow(string iconGlyph, string label, string value)
    {
        var row = new Grid { Margin = new Thickness(0, 0, 0, 8) };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(24) });
        row.ColumnDefinitions.Add(
            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
        );

        var icon = new TextBlock { VerticalAlignment = VerticalAlignment.Center, Text = iconGlyph };
        BindIconTypography(icon, "FlourishIconFontSizeSystemStatusView");
        icon.SetResourceReference(TextBlock.ForegroundProperty, "FlourishNeutralForeground2Brush");
        row.Children.Add(icon);

        var text = new StackPanel();
        Grid.SetColumn(text, 1);
        var labelText = new TextBlock { Text = label };
        BindTextSize(labelText, "FlourishFontSizeStandard");
        labelText.SetResourceReference(
            TextBlock.ForegroundProperty,
            "FlourishNeutralForeground2Brush"
        );
        text.Children.Add(labelText);
        var valueText = new TextBlock { Margin = new Thickness(0, 2, 0, 0), Text = value };
        BindTextSize(valueText, "FlourishFontSizeStandard");
        valueText.SetResourceReference(
            TextBlock.ForegroundProperty,
            "FlourishNeutralForeground1Brush"
        );
        text.Children.Add(valueText);
        row.Children.Add(text);
        return row;
    }

    private void OpenStatusFlyout(FrameworkElement anchor, StatusFlyoutKind kind, bool focusFlyout)
    {
        CloseProfileOverlay();
        CloseTitleBarFlyout(restoreFocus: false);
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
        StatusFlyoutCard.PlacementTarget = anchor;
        StatusFlyoutCard.Variant = kind == StatusFlyoutKind.System
            ? OverlayVariant.Temporary
            : OverlayVariant.Strong;
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
        StatusFlyoutCard.PlacementTarget = null;
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

    private void StatusBarBorder_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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

    private void StatusFlyoutCard_DismissRequested(object sender, RoutedEventArgs e)
    {
        CloseStatusFlyout();
        e.Handled = true;
    }

    private void UpdateStatusFlyoutPosition()
    {
        const double edgeSafeMargin = 14;
        const double anchorGap = 6;
        if (
            StatusFlyoutOverlay.Visibility != Visibility.Visible
            || statusFlyoutAnchor is null
            || ShellRootGrid.ActualWidth <= edgeSafeMargin * 2
            || ShellRootGrid.ActualHeight <= edgeSafeMargin * 2
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
            new System.Windows.Size(statusFlyoutAnchor.ActualWidth, statusFlyoutAnchor.ActualHeight)
        );
        var availableWidth = Math.Max(0, ShellRootGrid.ActualWidth - edgeSafeMargin * 2);
        StatusFlyoutCard.MaxWidth = availableWidth;
        StatusFlyoutCard.MaxHeight = Math.Max(0, anchor.Top - edgeSafeMargin - anchorGap);

        var cardWidth =
            StatusFlyoutCard.ActualWidth > 0
                ? Math.Min(StatusFlyoutCard.ActualWidth, availableWidth)
                : Math.Min(StatusFlyoutCard.Width, availableWidth);
        var cardHeight = Math.Min(StatusFlyoutCard.ActualHeight, StatusFlyoutCard.MaxHeight);
        var desiredLeft = anchor.Left + (anchor.Width - cardWidth) / 2;
        var maximumLeft = Math.Max(
            edgeSafeMargin,
            ShellRootGrid.ActualWidth - cardWidth - edgeSafeMargin
        );
        var left = Math.Clamp(desiredLeft, edgeSafeMargin, maximumLeft);
        var top = Math.Max(edgeSafeMargin, anchor.Top - cardHeight - anchorGap);

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
        NavigationPaneColumnLayout.ApplyConstraints(
            GetNavigationPaneColumn(),
            GetContentColumn(),
            isOpen,
            options.NavigationPaneMinWidth,
            options.NavigationPaneMaxWidth
        );
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
        var paneWidth =
            !isNavigationVisible ? 0
            : isOpen ? CoerceOpenPaneWidth(options.OpenPaneWidth)
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
        var committedWidth =
            paneColumn.ActualWidth > 0 ? paneColumn.ActualWidth : paneColumn.Width.Value;

        motionService.AnimateNavigationPane(
            navigationPaneTransition,
            new NavigationPaneTransitionTarget(
                WorkAreaGrid,
                NavigationPaneTransitionHost,
                ContentAreaGrid,
                options.NavigationPanelDirection,
                options.IsCenterContentEnabled
                    ? GetCenteredContentTransitionHosts()
                    : null
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

    private IReadOnlyList<FrameworkElement> GetCenteredContentTransitionHosts()
    {
        var hosts = new List<FrameworkElement>
        {
            ContentHeaderRegionHost,
            ToolbarLayoutHost,
            BreadcrumbLayoutHost,
            ContentFooterRegionHost,
        };
        if (
            RootFrame.Content is WpfPage page
            && CenteredPageContentLayout.FindPresenter(page) is { } presenter
        )
        {
            hosts.Add(presenter);
        }

        return hosts;
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
        navigationPaneDragStartWidth = CoerceOpenPaneWidth(GetNavigationPaneColumn().ActualWidth);
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
                SetPanelContent(ApplicationInfoBodyHost, elements);
                if (titleBarFlyoutKind == TitleBarFlyoutKind.ApplicationInfo)
                {
                    BuildApplicationInfoFlyoutContent();
                }
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
                throw new ArgumentOutOfRangeException(
                    nameof(region),
                    region,
                    "Unknown shell region."
                );
        }
    }

    private static void SetPanelContent(WpfPanel host, IReadOnlyList<FrameworkElement> elements)
    {
        SynchronizePanelChildren(host, elements);
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

            var hasIcon = !string.IsNullOrWhiteSpace(item.IconGlyph);
            var useIconOnly = showIconOnly && hasIcon;
            Button button = hasIcon
                ? new IconButton { Icon = item.IconGlyph }
                : new Button();
            button.Content = useIconOnly ? null : item.DisplayName;
            button.Margin =
                buttons.Count > 0 ? new Thickness(2, 0, 0, 0) : new Thickness();
            button.ToolTip = new FlourishToolTip { Content = item.DisplayName };
            button.Variant = ButtonVariant.Text;
            button.Tag = item;
            button.Width = useIconOnly ? 30 : double.NaN;
            button.Height = 28;
            button.MinWidth = useIconOnly ? 0 : 28;
            button.MinHeight = 0;
            button.Padding = new Thickness(7, 0, 7, 0);
            toolbarCommandButtons.Track(button, item);
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
        if (
            sender is Button
            {
                Tag: FlourishToolbarItem { CommandKey: string commandKey },
            }
            && !string.IsNullOrWhiteSpace(commandKey)
        )
        {
            await commandDispatcher.ExecuteAsync(commandKey, source: CommandSource.Toolbar);
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

        if (
            selectedNavigationKey is not null
            && GetNavigationItem(selectedNavigationKey) is { } selected
        )
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

    private void BuildNotifications(IReadOnlyList<FlourishNotificationInfo> notifications)
    {
        var activeIds = notifications
            .Select(info => info.Notification.Id)
            .ToHashSet(StringComparer.Ordinal);
        foreach (
            var removedId in notificationViewsById.Keys
                .Where(id => !activeIds.Contains(id))
                .ToArray()
        )
        {
            notificationViewsById.Remove(removedId);
        }

        var visibleNotifications = notifications.Reverse().Take(5).ToArray();
        var desiredViews = new List<UIElement>(visibleNotifications.Length);
        foreach (var info in visibleNotifications)
        {
            var view = GetOrCreateNotificationView(info.Notification.Id);
            UpdateNotificationView(view, info);
            desiredViews.Add(view.Container);
        }

        SynchronizePanelChildren(NotificationItemsHost, desiredViews);
        NotificationItemsHost.Visibility =
            NotificationItemsHost.Children.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private NotificationItemView GetOrCreateNotificationView(string id)
    {
        if (notificationViewsById.TryGetValue(id, out var cached))
        {
            return cached;
        }

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
        };
        BindIconTypography(icon, "FlourishFontSizeIcon");
        icon.SetResourceReference(TextBlock.ForegroundProperty, "FlourishPrimaryForegroundBrush");
        layout.Children.Add(icon);

        var title = new FlourishTextBlock { Role = FlourishTextRole.CardTitle };
        var message = new FlourishTextBlock
        {
            Margin = new Thickness(0, 4, 0, 0),
            Role = FlourishTextRole.Description,
        };
        var action = new Button
        {
            Margin = new Thickness(0, 8, 0, 0),
            HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
            Height = 28,
            MinWidth = 28,
            MinHeight = 0,
            Padding = new Thickness(7, 0, 7, 0),
            Content = "Run action",
            Variant = ButtonVariant.Text,
        };
        action.Click += NotificationAction_Click;
        var content = new StackPanel { Children = { title, message, action } };
        Grid.SetColumn(content, 1);
        layout.Children.Add(content);

        var dismiss = new IconButton
        {
            Width = 28,
            Height = 28,
            MinWidth = 0,
            MinHeight = 0,
            Padding = new Thickness(),
            Margin = new Thickness(8, 0, 0, 0),
            Icon = CreateIconContent("\uE711"),
            Tag = id,
            Variant = ButtonVariant.Text,
            ToolTip = new FlourishToolTip { Content = "Dismiss" },
        };
        dismiss.Click += NotificationDismiss_Click;
        Grid.SetColumn(dismiss, 2);
        layout.Children.Add(dismiss);

        var surface = new Border
        {
            Padding = new Thickness(14, 12, 10, 12),
            Margin = new Thickness(0, 0, 0, 14),
            Child = layout,
        };
        surface.SetResourceReference(Border.BackgroundProperty, "FlourishCardBackgroundBrush");
        surface.SetResourceReference(Border.BorderBrushProperty, "FlourishControlStrokeBrush");
        surface.SetResourceReference(
            Border.BorderThicknessProperty,
            "FlourishControlBorderThickness"
        );
        surface.SetResourceReference(Border.CornerRadiusProperty, "FlourishOverlayCornerRadius");

        var view = new NotificationItemView(surface, icon, title, message, action, dismiss);
        notificationViewsById.Add(id, view);
        return view;
    }

    private static void UpdateNotificationView(
        NotificationItemView view,
        FlourishNotificationInfo info
    )
    {
        if (view.Version == info.Version)
        {
            return;
        }

        var definition = info.Notification;
        view.Icon.Text = definition.IconGlyph ?? GetNotificationGlyph(definition.Severity);
        view.Title.Text = definition.Title;
        view.Message.Text = definition.Message;
        view.Action.Tag = info;
        view.Action.Visibility = string.IsNullOrWhiteSpace(definition.CommandKey)
            ? Visibility.Collapsed
            : Visibility.Visible;
        view.Dismiss.Tag = definition.Id;
        AutomationProperties.SetName(
            view.Container,
            $"{definition.Title}: {definition.Message}"
        );
        view.Version = info.Version;
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

        await commandDispatcher.ExecuteAsync(
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
        DispatchRuntimeChange(() =>
        {
            ApplyNavigationPanelRuntimeState(e.Animate);
            var current = navigationPanelService.Current;
            Titlebar.ConfigureVisibility(
                options.IsTitlebarSearchEnabled,
                IsBreadcrumbFeatureEnabled(),
                options.IsTitlebarNavigationToggleEnabled && current.IsEnabled,
                options.IsTitlebarLogoEnabled,
                options.IsTitlebarTitleEnabled || projectService.Current.IsMultiProjectEnabled,
                options.IsTitlebarThemeToggleEnabled && options.IsThemeEnabled,
                options.IsProfileEnabled && options.IsTitlebarProfileEnabled
            );
        });
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
        DispatchRuntimeChange(() =>
        {
            BuildNavigationItems();
            if (navigationService.CurrentSourcePageType is { } pageType)
            {
                UpdateBreadcrumb(pageType);
            }
        });
    }

    private void ToolbarService_Changed(object? sender, FlourishToolbarChangedEventArgs e)
    {
        DispatchRuntimeChange(() =>
        {
            if (e.Current.Version <= appliedToolbarVersion)
            {
                return;
            }

            appliedToolbarVersion = e.Current.Version;
            var enabledChanged = lastToolbarEnabled != e.Current.IsEnabled;
            lastToolbarEnabled = e.Current.IsEnabled;
            if (enabledChanged)
            {
                ClearToolbarButtonCache();
                BuildToolbarItems(navigationService.CurrentSourcePageType, force: true);
                return;
            }

            InvalidateToolbarButtonCache(e.PageType, e.Current);
            var currentPageType = navigationService.CurrentSourcePageType;
            if (
                e.PageType is null
                    ? currentPageType is null || !e.Current.Pages.ContainsKey(currentPageType)
                    : e.PageType == currentPageType
            )
            {
                BuildToolbarItems(currentPageType, force: true);
            }
        });
    }

    private void StatusService_Changed(object? sender, FlourishStatusBarChangedEventArgs e)
    {
        lock (statusRefreshGate)
        {
            if (
                pendingStatusChange is not null
                && pendingStatusChange.Current.Version >= e.Current.Version
            )
            {
                return;
            }

            pendingStatusChange = e;
            if (statusRefreshPending)
            {
                return;
            }

            statusRefreshPending = true;
        }

        if (isShellClosed || Dispatcher.HasShutdownStarted || Dispatcher.HasShutdownFinished)
        {
            return;
        }

        _ = Dispatcher.BeginInvoke(
            DispatcherPriority.DataBind,
            new Action(FlushPendingStatusChange)
        );
    }

    private void FlushPendingStatusChange()
    {
        FlourishStatusBarChangedEventArgs? change;
        lock (statusRefreshGate)
        {
            change = pendingStatusChange;
            pendingStatusChange = null;
            statusRefreshPending = false;
        }

        if (isShellClosed || change is null || !statusItemViews.Apply(change))
        {
            return;
        }

        statusBarSnapshot = change.Current;
        UpdateStatusBarVisibility();
    }

    private void ShellRegionService_Changed(object? sender, FlourishShellRegionChangedEventArgs e)
    {
        DispatchRuntimeChange(() => BuildRegionContents(e.Region));
    }

    private void TitleBarService_Changed(object? sender, FlourishTitleBarChangedEventArgs e)
    {
        DispatchRuntimeChange(() =>
        {
            if (e.Version <= appliedTitleBarVersion)
            {
                return;
            }

            appliedTitleBarVersion = e.Version;
            var current = e.State;
            ApplyTitleBarStateChanges(current);
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

    private void ProjectService_Changed(object? sender, FlourishProjectsChangedEventArgs e)
    {
        DispatchRuntimeChange(() =>
        {
            var titleState = titleBarService.Current;
            var projectState = e.Current;
            if (projectState.Version <= appliedProjectVersion)
            {
                return;
            }

            appliedProjectVersion = projectState.Version;
            projectSaveCommandRegistration.NotifyCanExecuteChanged();
            Titlebar.SetDisplayTitle(GetDisplayedTitle(titleState, projectState));
            RefreshTitleSelector(titleState, projectState);
            Titlebar.ConfigureVisibility(
                titleState.IsSearchVisible,
                titleState.IsBreadcrumbVisible
                    && titleState.BreadcrumbMode != BreadcrumbShowOption.Hidden,
                titleState.IsNavigationToggleVisible && options.IsNavigationPanelEnabled,
                titleState.IsLogoVisible,
                titleState.IsTitleVisible || projectState.IsMultiProjectEnabled,
                titleState.IsThemeToggleVisible && options.IsThemeEnabled,
                titleState.IsProfileVisible && options.IsProfileEnabled
            );
            EnsureTitleBarFlyoutIsAvailable();

            if (titleBarFlyoutKind == TitleBarFlyoutKind.ApplicationInfo)
            {
                BuildApplicationInfoFlyoutContent();
            }

            UpdateTitleBarFlyoutPosition();
        });
    }

    private void ApplyTitleBarState(FlourishTitleBarState state)
    {
        var projectState = projectService.Current;
        Title = state.ApplicationTitle;
        Titlebar.SetDisplayTitle(GetDisplayedTitle(state, projectState));
        RefreshTitleSelector();
        Titlebar.SetSearchPlaceholder(state.SearchPlaceholder);
        RequestTitleBarLogo(state.LogoPath, state.LogoFallbackText, state.IsLogoVisible);
        Titlebar.ConfigureVisibility(
            state.IsSearchVisible,
            state.IsBreadcrumbVisible && state.BreadcrumbMode != BreadcrumbShowOption.Hidden,
            state.IsNavigationToggleVisible && options.IsNavigationPanelEnabled,
            state.IsLogoVisible,
            state.IsTitleVisible || projectState.IsMultiProjectEnabled,
            state.IsThemeToggleVisible && options.IsThemeEnabled,
            state.IsProfileVisible && options.IsProfileEnabled
        );
        EnsureTitleBarFlyoutIsAvailable();
        UpdateTitlebarBreadcrumbNavigation();
        if (titleBarFlyoutKind == TitleBarFlyoutKind.ApplicationInfo)
        {
            BuildApplicationInfoFlyoutContent();
            UpdateTitleBarFlyoutPosition();
        }
        if (navigationService.CurrentSourcePageType is { } pageType)
        {
            UpdateBreadcrumb(pageType);
        }

        appliedTitleBarState = state;
    }

    private void ApplyTitleBarStateChanges(FlourishTitleBarState current)
    {
        var previous = appliedTitleBarState;
        if (previous is null)
        {
            ApplyTitleBarState(current);
            return;
        }

        var projectState = projectService.Current;
        var selectorChanged =
            !StringComparer.Ordinal.Equals(previous.ApplicationTitle, current.ApplicationTitle)
            || !StringComparer.Ordinal.Equals(
                previous.UnnamedProjectPlaceholder,
                current.UnnamedProjectPlaceholder
            );
        if (!StringComparer.Ordinal.Equals(previous.ApplicationTitle, current.ApplicationTitle))
        {
            Title = current.ApplicationTitle;
        }

        if (selectorChanged)
        {
            Titlebar.SetDisplayTitle(GetDisplayedTitle(current, projectState));
            RefreshTitleSelector(current, projectState);
        }

        if (!StringComparer.Ordinal.Equals(previous.SearchPlaceholder, current.SearchPlaceholder))
        {
            Titlebar.SetSearchPlaceholder(current.SearchPlaceholder);
        }

        var logoChanged =
            !StringComparer.Ordinal.Equals(previous.LogoPath, current.LogoPath)
            || !StringComparer.Ordinal.Equals(previous.LogoFallbackText, current.LogoFallbackText)
            || previous.IsLogoVisible != current.IsLogoVisible;
        if (logoChanged)
        {
            RequestTitleBarLogo(
                current.LogoPath,
                current.LogoFallbackText,
                current.IsLogoVisible
            );
        }

        var visibilityChanged =
            previous.IsSearchVisible != current.IsSearchVisible
            || previous.IsBreadcrumbVisible != current.IsBreadcrumbVisible
            || previous.IsNavigationToggleVisible != current.IsNavigationToggleVisible
            || previous.IsLogoVisible != current.IsLogoVisible
            || previous.IsTitleVisible != current.IsTitleVisible
            || previous.IsThemeToggleVisible != current.IsThemeToggleVisible
            || previous.IsProfileVisible != current.IsProfileVisible
            || previous.BreadcrumbMode != current.BreadcrumbMode;
        if (visibilityChanged)
        {
            Titlebar.ConfigureVisibility(
                current.IsSearchVisible,
                current.IsBreadcrumbVisible
                    && current.BreadcrumbMode != BreadcrumbShowOption.Hidden,
                current.IsNavigationToggleVisible && options.IsNavigationPanelEnabled,
                current.IsLogoVisible,
                current.IsTitleVisible || projectState.IsMultiProjectEnabled,
                current.IsThemeToggleVisible && options.IsThemeEnabled,
                current.IsProfileVisible && options.IsProfileEnabled
            );
            EnsureTitleBarFlyoutIsAvailable();
        }

        if (
            previous.IsBreadcrumbVisible != current.IsBreadcrumbVisible
            || previous.BreadcrumbMode != current.BreadcrumbMode
        )
        {
            UpdateTitlebarBreadcrumbNavigation();
            if (navigationService.CurrentSourcePageType is { } pageType)
            {
                UpdateBreadcrumb(pageType);
            }
        }

        var flyoutContentChanged =
            selectorChanged
            || !StringComparer.Ordinal.Equals(
                previous.ApplicationSubTitle,
                current.ApplicationSubTitle
            )
            || previous.ShowApplicationTitle != current.ShowApplicationTitle
            || previous.ShowApplicationSubTitle != current.ShowApplicationSubTitle
            || previous.ShowProjectTitle != current.ShowProjectTitle;
        if (flyoutContentChanged && titleBarFlyoutKind == TitleBarFlyoutKind.ApplicationInfo)
        {
            BuildApplicationInfoFlyoutContent();
            UpdateTitleBarFlyoutPosition();
        }

        appliedTitleBarState = current;
    }

    private void RequestTitleBarLogo(string? logoPath, string fallbackText, bool isVisible)
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

        DispatchRuntimeChange(() =>
        {
            if (titleBarLogoLoadCoordinator.IsCurrent(result))
            {
                ApplyTitleBarLogoSource(result.Source);
            }
        });
    }

    private void ApplyTitleBarLogoSource(ImageSource? source)
    {
        var effectiveSource = source ?? TitleBarVisualAssets.DefaultLogoSource;
        currentTitleBarLogoSource = effectiveSource;
        Titlebar.SetLogo(effectiveSource, currentTitleBarLogoFallbackText);
        Icon = isTitleBarLogoVisible ? effectiveSource : null;
        if (titleBarFlyoutKind == TitleBarFlyoutKind.ApplicationInfo)
        {
            BuildApplicationInfoFlyoutContent();
        }
    }

    private void TitleBarSearchService_StateChanged(
        object? sender,
        FlourishTitleBarSearchStateChangedEventArgs e
    )
    {
        DispatchRuntimeChange(() =>
        {
            var current = e.State;
            if (!titleBarSearchService.IsCurrentVersion(current.Version))
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
                titleState.IsTitleVisible || projectService.Current.IsMultiProjectEnabled,
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
        lock (notificationRefreshGate)
        {
            if (
                e.Version <= appliedNotificationVersion
                || e.Version <= pendingNotificationVersion
            )
            {
                return;
            }

            pendingNotifications = e.Notifications;
            pendingNotificationVersion = e.Version;
            if (notificationRefreshPending)
            {
                return;
            }

            notificationRefreshPending = true;
        }

        if (isShellClosed || Dispatcher.HasShutdownStarted || Dispatcher.HasShutdownFinished)
        {
            return;
        }

        _ = Dispatcher.BeginInvoke(
            DispatcherPriority.DataBind,
            new Action(() =>
            {
                if (!isShellClosed)
                {
                    FlushPendingNotifications();
                }
            })
        );
    }

    private void FlushPendingNotifications()
    {
        IReadOnlyList<FlourishNotificationInfo> notifications;
        long version;
        lock (notificationRefreshGate)
        {
            notifications = pendingNotifications;
            version = pendingNotificationVersion;
            notificationRefreshPending = false;
        }

        if (version <= appliedNotificationVersion)
        {
            return;
        }

        appliedNotificationVersion = version;
        BuildNotifications(notifications);
    }

    private void ProfileFlyoutService_Changed(
        object? sender,
        FlourishProfileFlyoutChangedEventArgs e
    )
    {
        DispatchRuntimeChange(ConfigureProfileSurface);
    }

    private void ShellFeatureService_Changed(object? sender, FlourishShellFeatureChangedEventArgs e)
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

    private void LocalizationService_Changed(object? sender, FlourishLocalizationChangedEventArgs e)
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

            RefreshTitleSelector();
        });
    }

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
                RootFrame.Content is WpfPage contentPage
                && (
                    affectedPageType is null
                    || (contentPageType ?? contentPage.GetType()) == affectedPageType
                )
            )
            {
                fontService.ApplyToPage(contentPage, contentPageType ?? contentPage.GetType());
            }

            var profilePageType = profileFlyoutService.Current.ContentPageType;
            if (
                ProfileFrame.Content is WpfPage profilePage
                && (affectedPageType is null || profilePageType == affectedPageType)
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
                ApplyNavigationPaneState();
            }
        });
    }

    private void CommandRegistry_Changed(object? sender, CommandRegistryChangedEventArgs e)
    {
        DispatchRuntimeChange(() => RefreshCommandAvailability(e.CommandKey));
    }

    private void CommandRegistry_CanExecuteChanged(
        object? sender,
        CommandCanExecuteChangedEventArgs e
    )
    {
        DispatchRuntimeChange(() => RefreshCommandAvailability(e.CommandKey));
    }

    private void RefreshCommandAvailability(string? commandKey)
    {
        toolbarCommandButtons.Refresh(commandKey);
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

    private void ClearToolbarButtonCache()
    {
        toolbarCommandButtons.Clear();
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

    private void InvalidateToolbarButtonCache(
        Type? pageType,
        FlourishToolbarSnapshot snapshot
    )
    {
        if (pageType is not null)
        {
            if (toolbarButtonsByPageType.Remove(pageType, out var buttons))
            {
                ReleaseToolbarButtons(buttons);
            }

            return;
        }

        if (defaultToolbarButtons is not null)
        {
            ReleaseToolbarButtons(defaultToolbarButtons);
            defaultToolbarButtons = null;
        }

        foreach (
            var fallbackPageType in toolbarButtonsByPageType.Keys
                .Where(candidate => !snapshot.Pages.ContainsKey(candidate))
                .ToArray()
        )
        {
            var buttons = toolbarButtonsByPageType[fallbackPageType];
            toolbarButtonsByPageType.Remove(fallbackPageType);
            ReleaseToolbarButtons(buttons);
        }
    }

    private void ReleaseToolbarButtons(IReadOnlyList<Button> buttons)
    {
        toolbarCommandButtons.Untrack(buttons);
        foreach (var button in buttons)
        {
            button.Click -= ToolbarButton_Click;
        }
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

    private void NavigationItemsHost_PreviewKeyDown(
        object sender,
        System.Windows.Input.KeyEventArgs e
    )
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

    private void NavigationItemsHost_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
        CenteredPageContentLayout.Apply(
            e.Page,
            options.IsCenterContentEnabled ? options.CenterContentWidth : null
        );

        fontService.ApplyToPage(e.Page, e.SourcePageType);
        UpdateTitlebarBreadcrumbNavigation();
        BuildToolbarItems(e.SourcePageType);
        UpdateBreadcrumb(e.SourcePageType);

        SelectNavigationItem(e.SourcePageType);
        motionService.AnimatePageEntrance(
            pageTransition,
            new PageTransitionTarget(PageTransitionContentHost)
        );
    }

    private void ApplyContentLayoutOptions()
    {
        var maximumWidth = options.IsCenterContentEnabled
            ? options.CenterContentWidth
            : double.PositiveInfinity;

        foreach (
            var host in new FrameworkElement[]
            {
                ContentHeaderRegionHost,
                ToolbarLayoutHost,
                BreadcrumbLayoutHost,
                ContentFooterRegionHost,
            }
        )
        {
            host.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            host.MaxWidth = maximumWidth;
        }
    }

    private void UpdateBreadcrumb(Type sourcePageType)
    {
        if (!IsBreadcrumbFeatureEnabled())
        {
            BreadcrumbHost.Visibility = Visibility.Collapsed;
            return;
        }

        if (options.BreadcrumbShowOption == BreadcrumbShowOption.Auto && !HasBreadcrumbNavigation())
        {
            BreadcrumbHost.Visibility = Visibility.Collapsed;
            return;
        }

        var label =
            navigationItemsByPage.GetValueOrDefault(sourcePageType)?.Label ?? sourcePageType.Name;

        BreadcrumbText.Text = $"{options.ApplicationTitle} / {label}";
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
                _ = commandDispatcher
                    .ExecuteAsync(item.CommandKey, source: CommandSource.Navigation)
                    .AsTask();
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
        return
            parent.ParentId != 0
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

        var isVisible =
            IsBreadcrumbFeatureEnabled()
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
                localizationService.Get(
                    FlourishLocaleKeys.WindowBackgroundTasksKeepRunning
                )
            )
            {
                IsDefault = true,
                IsCancel = true,
            },
            new(
                "stop-and-exit",
                localizationService.Get(
                    FlourishLocaleKeys.WindowBackgroundTasksStopAndExit
                )
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
                    localizationService.Get(
                        FlourishLocaleKeys.WindowBackgroundTasksCloseTitle
                    ),
                    closeOptions,
                    MessageBoxImage.Warning,
                    cancellationToken: cancellationToken
                )
            )?.Id == "stop-and-exit";
    }

    private void CancelActiveBackgroundTasks()
    {
        foreach (var task in backgroundTaskService.ActiveTasks)
        {
            backgroundTaskService.CancelTask(task.Id);
        }
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
                var activeTaskCount = backgroundTaskService.ActiveTasks.Count;
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
        lock (backgroundTaskRefreshGate)
        {
            isShellClosed = true;
        }

        projectBehaviorCancellation.Cancel();
        projectSaveShortcutRegistration.Dispose();
        projectSaveCommandRegistration.Dispose();
        projectCloseGuardRegistration.Dispose();
        backgroundTaskService.TasksChanged -= BackgroundTaskService_TasksChanged;
        navigationPanelService.Changed -= NavigationPanelService_Changed;
        navigationMenuService.Changed -= NavigationMenuService_Changed;
        toolbarService.Changed -= ToolbarService_Changed;
        statusService.Changed -= StatusService_Changed;
        shellRegionService.Changed -= ShellRegionService_Changed;
        titleBarService.Changed -= TitleBarService_Changed;
        projectService.Changed -= ProjectService_Changed;
        titleBarLogoLoadCoordinator.Dispose();
        titleBarSearchService.ProgrammaticStateChanged -=
            TitleBarSearchService_StateChanged;
        notificationService.NotificationsChanged -= NotificationService_NotificationsChanged;
        profileFlyoutService.Changed -= ProfileFlyoutService_Changed;
        shellFeatureService.Changed -= ShellFeatureService_Changed;
        localizationService.Changed -= LocalizationService_Changed;
        fontService.Changed -= FontService_Changed;
        motionService.Changed -= MotionService_Changed;
        commandRegistry.Changed -= CommandRegistry_Changed;
        commandRegistry.CanExecuteChanged -= CommandRegistry_CanExecuteChanged;
        navigationService.Navigated -= RootFrame_Navigated;
        Titlebar.BackRequested -= Titlebar_BackRequested;
        Titlebar.ForwardRequested -= Titlebar_ForwardRequested;
        Titlebar.NavigationToggleRequested -= Titlebar_NavigationToggleRequested;
        Titlebar.MinimizeRequested -= Titlebar_MinimizeRequested;
        Titlebar.MaximizeRequested -= Titlebar_MaximizeRequested;
        Titlebar.CloseRequested -= Titlebar_CloseRequested;
        Titlebar.DragRequested -= Titlebar_DragRequested;
        Titlebar.ToggleWindowStateRequested -= Titlebar_ToggleWindowStateRequested;
        Titlebar.ThemeToggleRequested -= Titlebar_ThemeToggleRequested;
        Titlebar.ProfileToggleRequested -= Titlebar_ProfileToggleRequested;
        Titlebar.LogoHoverRequested -= Titlebar_LogoHoverRequested;
        Titlebar.LogoClickRequested -= Titlebar_LogoClickRequested;
        Titlebar.TitleSelectionChanged -= ProjectComboBox_SelectionChanged;
        Titlebar.TitleDropDownOpened -= Titlebar_TitleDropDownOpened;
        Titlebar.InteractionStarted -= Titlebar_InteractionStarted;
        Titlebar.SearchTextChanged -= Titlebar_SearchTextChanged;
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
        projectBehaviorCancellation.Dispose();
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
        var parent =
            activeItem.IsPageItem && activeItem.ChildId != 0
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
        return
            source is DependencyObject dependencyObject
            && FindAncestor<ListBoxItem>(dependencyObject)?.DataContext
                is FlourishNavigationItem item
            ? item
            : null;
    }
}
