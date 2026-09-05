using System;
using System.Threading;

using ArkheideSystem.Flourish.Abstract;
using System.Windows.Controls;

namespace ArkheideSystem.Flourish.Profile;

internal sealed class ProfileFlyoutService(
    ProfileOptions profileOptions,
    ProfileViewOptions profileViewOptions
) : IProfileFlyoutService
{
    private readonly Lock gate = new();
    private bool isVisible;
    private ProfileFlyoutState current = new(
        profileOptions.IsProfileEnabled,
        false,
        profileViewOptions.PageType,
        0
    );
    private long version;

    public event EventHandler<StateChangedEventArgs<ProfileFlyoutState>>? Changed;

    public ProfileFlyoutState Current => Volatile.Read(ref current);

    public void SetEnabled(bool enabled)
    {
        Update(() =>
        {
            profileOptions.IsProfileEnabled = enabled;
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
            if (!profileOptions.IsProfileEnabled)
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
            if (!profileOptions.IsProfileEnabled)
            {
                throw new InvalidOperationException("The profile feature is disabled.");
            }

            isVisible = !isVisible;
        });
    }

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

        Update(() => profileViewOptions.PageType = pageType);
    }

    internal void SynchronizeVisibility(bool visible)
    {
        ProfileFlyoutState snapshot;
        lock (gate)
        {
            if (isVisible == visible)
            {
                return;
            }

            isVisible = visible;
            version++;
            snapshot = CreateSnapshot();
            Volatile.Write(ref current, snapshot);
        }

        Changed?.Invoke(this, new StateChangedEventArgs<ProfileFlyoutState>(snapshot));
    }

    private void Update(Action update)
    {
        ProfileFlyoutState snapshot;
        lock (gate)
        {
            var previousEnabled = profileOptions.IsProfileEnabled;
            var previousVisible = isVisible;
            var previousPageType = profileViewOptions.PageType;
            update();
            if (
                previousEnabled == profileOptions.IsProfileEnabled
                && previousVisible == isVisible
                && previousPageType == profileViewOptions.PageType
            )
            {
                return;
            }

            version++;
            snapshot = CreateSnapshot();
            Volatile.Write(ref current, snapshot);
        }

        Changed?.Invoke(this, new StateChangedEventArgs<ProfileFlyoutState>(snapshot));
    }

    private ProfileFlyoutState CreateSnapshot()
    {
        return new ProfileFlyoutState(
            profileOptions.IsProfileEnabled,
            isVisible,
            profileViewOptions.PageType,
            version
        );
    }
}
