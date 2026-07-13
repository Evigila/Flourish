using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;

namespace ArkheideSystem.Flourish.Services;

internal sealed class ShellFeatureService : IShellFeatureService
{
    private readonly object gate = new();
    private readonly FlourishShellOptions options;
    private readonly FlourishMotionService motionService;
    private readonly FlourishToolTipService toolTipService;
    private long version;

    public ShellFeatureService(
        FlourishShellOptions options,
        FlourishMotionService motionService,
        FlourishToolTipService toolTipService
    )
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        this.motionService =
            motionService ?? throw new ArgumentNullException(nameof(motionService));
        this.toolTipService =
            toolTipService ?? throw new ArgumentNullException(nameof(toolTipService));
        motionService.Changed += MotionService_Changed;
        toolTipService.Changed += ToolTipService_Changed;
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
            throw new ArgumentOutOfRangeException(nameof(feature), feature, "Unknown shell feature.");
        }

        if (feature == ShellFeature.Motion)
        {
            motionService.SetEnabled(enabled);
            return;
        }

        if (feature == ShellFeature.ToolTips)
        {
            toolTipService.SetEnabled(enabled);
            return;
        }

        FlourishShellFeatureState state;
        lock (gate)
        {
            if (CreateSnapshot().IsEnabled(feature) == enabled)
            {
                return;
            }

            switch (feature)
            {
                case ShellFeature.TitleBar:
                    options.IsTitlebarEnabled = enabled;
                    break;
                case ShellFeature.Navigation:
                    options.IsNavigationPanelEnabled = enabled;
                    break;
                case ShellFeature.DynamicToolbar:
                    options.IsDynamicToolbarEnabled = enabled;
                    break;
                case ShellFeature.StatusContent:
                    options.IsStatusBarEnabled = enabled;
                    break;
                case ShellFeature.Profile:
                    options.IsProfileEnabled = enabled;
                    break;
            }

            version++;
            state = CreateSnapshot();
        }

        Changed?.Invoke(this, new FlourishShellFeatureChangedEventArgs(feature, state));
    }

    private void MotionService_Changed(
        object? sender,
        FlourishMotionChangedEventArgs e
    )
    {
        if (e.Previous.IsEnabled == e.Current.IsEnabled)
        {
            return;
        }

        FlourishShellFeatureState state;
        lock (gate)
        {
            version++;
            state = CreateSnapshot(e.Current.IsEnabled);
        }

        Changed?.Invoke(
            this,
            new FlourishShellFeatureChangedEventArgs(ShellFeature.Motion, state)
        );
    }

    private void ToolTipService_Changed(
        object? sender,
        FlourishToolTipChangedEventArgs e
    )
    {
        if (e.Previous.IsEnabled == e.Current.IsEnabled)
        {
            return;
        }

        FlourishShellFeatureState state;
        lock (gate)
        {
            version++;
            state = CreateSnapshot(toolTipsEnabled: e.Current.IsEnabled);
        }

        Changed?.Invoke(
            this,
            new FlourishShellFeatureChangedEventArgs(ShellFeature.ToolTips, state)
        );
    }

    private FlourishShellFeatureState CreateSnapshot(
        bool? motionEnabled = null,
        bool? toolTipsEnabled = null
    )
    {
        return new FlourishShellFeatureState(
            options.IsTitlebarEnabled,
            options.IsNavigationPanelEnabled,
            options.IsDynamicToolbarEnabled,
            options.IsStatusBarEnabled,
            toolTipsEnabled ?? toolTipService.Current.IsEnabled,
            motionEnabled ?? motionService.Current.IsEnabled,
            options.IsProfileEnabled,
            version
        );
    }
}
