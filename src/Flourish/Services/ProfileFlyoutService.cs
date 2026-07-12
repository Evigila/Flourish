using System.Windows.Controls;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;

namespace ArkheideSystem.Flourish.Services;

internal sealed class ProfileFlyoutService(
    FlourishShellOptions shellOptions,
    FlourishProfileOptions profileOptions
) : IProfileFlyoutService
{
    private readonly object gate = new();
    private bool isVisible;
    private long version;

    public event EventHandler<FlourishProfileFlyoutChangedEventArgs>? Changed;

    public FlourishProfileFlyoutState Current
    {
        get
        {
            lock (gate)
            {
                return CreateSnapshot();
            }
        }
    }

    public void SetEnabled(bool enabled)
    {
        Update(() =>
        {
            shellOptions.IsProfileEnabled = enabled;
            if (!enabled)
            {
                isVisible = false;
            }
        });
    }

    public void Show()
    {
        Update(() =>
        {
            if (!shellOptions.IsProfileEnabled)
            {
                throw new InvalidOperationException("The profile feature is disabled.");
            }

            isVisible = true;
        });
    }

    public void Hide() => Update(() => isVisible = false);

    public void Toggle()
    {
        Update(() =>
        {
            if (!shellOptions.IsProfileEnabled)
            {
                throw new InvalidOperationException("The profile feature is disabled.");
            }

            isVisible = !isVisible;
        });
    }

    public void SetContentPage<TPage>() where TPage : Page => SetContentPage(typeof(TPage));

    public void SetContentPage(Type pageType)
    {
        ArgumentNullException.ThrowIfNull(pageType);
        if (
            !typeof(Page).IsAssignableFrom(pageType)
            || pageType.IsAbstract
            || pageType.ContainsGenericParameters
        )
        {
            throw new ArgumentException(
                $"{pageType.FullName} must be a closed, concrete System.Windows.Controls.Page type.",
                nameof(pageType)
            );
        }

        Update(() => profileOptions.PageType = pageType);
    }

    internal void SynchronizeVisibility(bool visible)
    {
        FlourishProfileFlyoutState snapshot;
        lock (gate)
        {
            if (isVisible == visible)
            {
                return;
            }

            isVisible = visible;
            version++;
            snapshot = CreateSnapshot();
        }

        Changed?.Invoke(this, new FlourishProfileFlyoutChangedEventArgs(snapshot));
    }

    private void Update(Action update)
    {
        FlourishProfileFlyoutState snapshot;
        lock (gate)
        {
            var previousEnabled = shellOptions.IsProfileEnabled;
            var previousVisible = isVisible;
            var previousPageType = profileOptions.PageType;
            update();
            if (
                previousEnabled == shellOptions.IsProfileEnabled
                && previousVisible == isVisible
                && previousPageType == profileOptions.PageType
            )
            {
                return;
            }

            version++;
            snapshot = CreateSnapshot();
        }

        Changed?.Invoke(this, new FlourishProfileFlyoutChangedEventArgs(snapshot));
    }

    private FlourishProfileFlyoutState CreateSnapshot()
    {
        return new FlourishProfileFlyoutState(
            shellOptions.IsProfileEnabled,
            isVisible,
            profileOptions.PageType,
            version
        );
    }
}
