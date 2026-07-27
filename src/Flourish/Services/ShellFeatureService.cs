using ArkheideSystem.Flourish.Abstract;

namespace ArkheideSystem.Flourish.Services;

internal sealed class ShellFeatureService : IShellFeatureService
{
    private readonly Lock gate = new();
    private readonly ITitleBarService titleBarService;
    private readonly INavigationPanelService navigationPanelService;
    private readonly IToolbarService toolbarService;
    private readonly IStatusBarService statusBarService;
    private readonly IToolTipService toolTipService;
    private readonly IMotionService motionService;
    private readonly IProfileFlyoutService profileFlyoutService;
    private bool isTitleBarEnabled;
    private bool isNavigationEnabled;
    private bool isDynamicToolbarEnabled;
    private bool isStatusContentEnabled;
    private bool areToolTipsEnabled;
    private bool isMotionEnabled;
    private bool isProfileEnabled;
    private long version;

    public ShellFeatureService(
        ITitleBarService titleBarService,
        INavigationPanelService navigationPanelService,
        IToolbarService toolbarService,
        IStatusBarService statusBarService,
        IToolTipService toolTipService,
        IMotionService motionService,
        IProfileFlyoutService profileFlyoutService
    )
    {
        this.titleBarService =
            titleBarService ?? throw new ArgumentNullException(nameof(titleBarService));
        this.navigationPanelService =
            navigationPanelService
            ?? throw new ArgumentNullException(nameof(navigationPanelService));
        this.toolbarService =
            toolbarService ?? throw new ArgumentNullException(nameof(toolbarService));
        this.statusBarService =
            statusBarService ?? throw new ArgumentNullException(nameof(statusBarService));
        this.toolTipService =
            toolTipService ?? throw new ArgumentNullException(nameof(toolTipService));
        this.motionService =
            motionService ?? throw new ArgumentNullException(nameof(motionService));
        this.profileFlyoutService =
            profileFlyoutService
            ?? throw new ArgumentNullException(nameof(profileFlyoutService));

        isTitleBarEnabled = titleBarService.Current.IsEnabled;
        isNavigationEnabled = navigationPanelService.Current.IsEnabled;
        isDynamicToolbarEnabled = toolbarService.Current.IsEnabled;
        isStatusContentEnabled = statusBarService.Current.IsEnabled;
        areToolTipsEnabled = toolTipService.Current.IsEnabled;
        isMotionEnabled = motionService.Current.IsEnabled;
        isProfileEnabled = profileFlyoutService.Current.IsEnabled;

        titleBarService.Changed += TitleBarService_Changed;
        navigationPanelService.Changed += NavigationPanelService_Changed;
        toolbarService.Changed += ToolbarService_Changed;
        statusBarService.Changed += StatusBarService_Changed;
        toolTipService.Changed += ToolTipService_Changed;
        motionService.Changed += MotionService_Changed;
        profileFlyoutService.Changed += ProfileFlyoutService_Changed;
    }

    public event EventHandler<FlourishShellFeatureChangedEventArgs>? Changed;

    public FlourishShellFeatureState Current
    {
        get
        {
            lock (gate)
            {
                return CreateSnapshot();
            }
        }
    }

    public void SetEnabled(ShellFeature feature, bool enabled)
    {
        if (!Enum.IsDefined(feature))
        {
            throw new ArgumentOutOfRangeException(
                nameof(feature),
                feature,
                "Unknown shell feature."
            );
        }

        switch (feature)
        {
            case ShellFeature.TitleBar:
                titleBarService.SetEnabled(enabled);
                break;
            case ShellFeature.Navigation:
                navigationPanelService.SetEnabled(enabled);
                break;
            case ShellFeature.DynamicToolbar:
                toolbarService.SetEnabled(enabled);
                break;
            case ShellFeature.StatusContent:
                statusBarService.SetEnabled(enabled);
                break;
            case ShellFeature.ToolTips:
                toolTipService.SetEnabled(enabled);
                break;
            case ShellFeature.Motion:
                motionService.SetEnabled(enabled);
                break;
            case ShellFeature.Profile:
                profileFlyoutService.SetEnabled(enabled);
                break;
        }
    }

    private void TitleBarService_Changed(object? sender, FlourishTitleBarChangedEventArgs e) =>
        PublishIfChanged(ShellFeature.TitleBar, e.State.IsEnabled);

    private void NavigationPanelService_Changed(
        object? sender,
        FlourishNavigationPanelChangedEventArgs e
    ) => PublishIfChanged(ShellFeature.Navigation, e.Current.IsEnabled);

    private void ToolbarService_Changed(object? sender, FlourishToolbarChangedEventArgs e) =>
        PublishIfChanged(ShellFeature.DynamicToolbar, e.Current.IsEnabled);

    private void StatusBarService_Changed(object? sender, FlourishStatusBarChangedEventArgs e) =>
        PublishIfChanged(ShellFeature.StatusContent, e.Current.IsEnabled);

    private void ToolTipService_Changed(object? sender, FlourishToolTipChangedEventArgs e) =>
        PublishIfChanged(ShellFeature.ToolTips, e.Current.IsEnabled);

    private void MotionService_Changed(object? sender, FlourishMotionChangedEventArgs e) =>
        PublishIfChanged(ShellFeature.Motion, e.Current.IsEnabled);

    private void ProfileFlyoutService_Changed(
        object? sender,
        FlourishProfileFlyoutChangedEventArgs e
    ) => PublishIfChanged(ShellFeature.Profile, e.State.IsEnabled);

    private void PublishIfChanged(ShellFeature feature, bool enabled)
    {
        FlourishShellFeatureState state;
        lock (gate)
        {
            if (GetCachedEnabled(feature) == enabled)
            {
                return;
            }

            SetCachedEnabled(feature, enabled);
            version++;
            state = CreateSnapshot();
        }

        Changed?.Invoke(this, new FlourishShellFeatureChangedEventArgs(feature, state));
    }

    private bool GetCachedEnabled(ShellFeature feature) => feature switch
    {
        ShellFeature.TitleBar => isTitleBarEnabled,
        ShellFeature.Navigation => isNavigationEnabled,
        ShellFeature.DynamicToolbar => isDynamicToolbarEnabled,
        ShellFeature.StatusContent => isStatusContentEnabled,
        ShellFeature.ToolTips => areToolTipsEnabled,
        ShellFeature.Motion => isMotionEnabled,
        ShellFeature.Profile => isProfileEnabled,
        _ => throw new ArgumentOutOfRangeException(nameof(feature), feature, "Unknown shell feature."),
    };

    private void SetCachedEnabled(ShellFeature feature, bool enabled)
    {
        switch (feature)
        {
            case ShellFeature.TitleBar:
                isTitleBarEnabled = enabled;
                break;
            case ShellFeature.Navigation:
                isNavigationEnabled = enabled;
                break;
            case ShellFeature.DynamicToolbar:
                isDynamicToolbarEnabled = enabled;
                break;
            case ShellFeature.StatusContent:
                isStatusContentEnabled = enabled;
                break;
            case ShellFeature.ToolTips:
                areToolTipsEnabled = enabled;
                break;
            case ShellFeature.Motion:
                isMotionEnabled = enabled;
                break;
            case ShellFeature.Profile:
                isProfileEnabled = enabled;
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(feature),
                    feature,
                    "Unknown shell feature."
                );
        }
    }

    private FlourishShellFeatureState CreateSnapshot() =>
        new(
            isTitleBarEnabled,
            isNavigationEnabled,
            isDynamicToolbarEnabled,
            isStatusContentEnabled,
            areToolTipsEnabled,
            isMotionEnabled,
            isProfileEnabled,
            version
        );
}
