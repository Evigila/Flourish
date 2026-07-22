namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Represents an immutable shell feature snapshot.
/// </summary>
/// <param name="IsTitleBarEnabled">Whether the title bar surface is enabled.</param>
/// <param name="IsNavigationEnabled">Whether the navigation surface is enabled.</param>
/// <param name="IsDynamicToolbarEnabled">Whether the dynamic toolbar is enabled.</param>
/// <param name="IsStatusContentEnabled">Whether status content is enabled.</param>
/// <param name="AreToolTipsEnabled">Whether shell tooltips use the Flourish presentation.</param>
/// <param name="IsMotionEnabled">Whether shell motion is enabled.</param>
/// <param name="IsProfileEnabled">Whether the profile feature is enabled.</param>
/// <param name="Version">A monotonically increasing state version.</param>
public sealed record FlourishShellFeatureState(
    bool IsTitleBarEnabled,
    bool IsNavigationEnabled,
    bool IsDynamicToolbarEnabled,
    bool IsStatusContentEnabled,
    bool AreToolTipsEnabled,
    bool IsMotionEnabled,
    bool IsProfileEnabled,
    long Version
)
{
    /// <summary>
    /// Gets whether a particular feature is enabled in this snapshot.
    /// </summary>
    /// <param name="feature">The feature to inspect.</param>
    /// <returns><see langword="true"/> when the feature is enabled; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="feature"/> is not defined.</exception>
    public bool IsEnabled(ShellFeature feature) => feature switch
    {
        ShellFeature.TitleBar => IsTitleBarEnabled,
        ShellFeature.Navigation => IsNavigationEnabled,
        ShellFeature.DynamicToolbar => IsDynamicToolbarEnabled,
        ShellFeature.StatusContent => IsStatusContentEnabled,
        ShellFeature.ToolTips => AreToolTipsEnabled,
        ShellFeature.Motion => IsMotionEnabled,
        ShellFeature.Profile => IsProfileEnabled,
        _ => throw new ArgumentOutOfRangeException(nameof(feature), feature, "Unknown shell feature."),
    };
}
