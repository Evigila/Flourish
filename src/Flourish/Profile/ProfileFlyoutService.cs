using System;
using System.Threading;

using ArkheideSystem.Flourish.Abstract;
using System.Windows.Controls;

namespace ArkheideSystem.Flourish.Profile;

internal sealed class ProfileFlyoutService(
    FlourishProfileOptions profileOptions
) : IProfileFlyoutService
{
    private readonly Lock gate = new();
    private bool isVisible;
    private FlourishProfileFlyoutState current = new(
        profileOptions.IsProfileEnabled,
        false,
        profileOptions.PageType,
        0
    );
    private long version;

    public event EventHandler<FlourishStateChangedEventArgs<FlourishProfileFlyoutState>>? Changed;

    public FlourishProfileFlyoutState Current => Volatile.Read(ref current);

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
            Volatile.Write(ref current, snapshot);
        }

        Changed?.Invoke(this, new FlourishStateChangedEventArgs<FlourishProfileFlyoutState>(snapshot));
    }

    private void Update(Action update)
    {
        FlourishProfileFlyoutState snapshot;
        lock (gate)
        {
            var previousEnabled = profileOptions.IsProfileEnabled;
            var previousVisible = isVisible;
            var previousPageType = profileOptions.PageType;
            update();
            if (
                previousEnabled == profileOptions.IsProfileEnabled
                && previousVisible == isVisible
                && previousPageType == profileOptions.PageType
            )
            {
                return;
            }

            version++;
            snapshot = CreateSnapshot();
            Volatile.Write(ref current, snapshot);
        }

        Changed?.Invoke(this, new FlourishStateChangedEventArgs<FlourishProfileFlyoutState>(snapshot));
    }

    private FlourishProfileFlyoutState CreateSnapshot()
    {
        return new FlourishProfileFlyoutState(
            profileOptions.IsProfileEnabled,
            isVisible,
            profileOptions.PageType,
            version
        );
    }
}
