using System;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Represents the runtime state of the Flourish profile flyout.
/// </summary>
/// <param name="IsEnabled">Whether the profile feature is enabled.</param>
/// <param name="IsVisible">Whether the profile flyout is open.</param>
/// <param name="ContentPageType">The WPF page type displayed in the flyout.</param>
/// <param name="Version">A monotonically increasing state version.</param>
public sealed record ProfileFlyoutState(
    bool IsEnabled,
    bool IsVisible,
    Type ContentPageType,
    long Version
);
