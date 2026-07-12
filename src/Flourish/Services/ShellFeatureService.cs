using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;

namespace ArkheideSystem.Flourish.Services;

internal sealed class ShellFeatureService(FlourishShellOptions options)
    : IShellFeatureService
{
    private readonly object gate = new();
    private long version;

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
                case ShellFeature.ToolTips:
                    options.IsTipsEnabled = enabled;
                    break;
                case ShellFeature.Motion:
                    options.Motion.IsEnabled = enabled;
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

    private FlourishShellFeatureState CreateSnapshot()
    {
        return new FlourishShellFeatureState(
            options.IsTitlebarEnabled,
            options.IsNavigationPanelEnabled,
            options.IsDynamicToolbarEnabled,
            options.IsStatusBarEnabled,
            options.IsTipsEnabled,
            options.Motion.IsEnabled,
            options.IsProfileEnabled,
            version
        );
    }
}
