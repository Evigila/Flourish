namespace ArkheideSystem.Flourish.Abstract.Builder;

/// <summary>
/// Configures motion and animation behavior for the Flourish shell.
/// </summary>
/// <example>
/// <code><![CDATA[
/// builder.ConfigMotion(motion =>
/// {
///     motion.UsePageTransition(
///         transition: FlourishPageTransition.EntranceFromBottom,
///         duration: TimeSpan.FromMilliseconds(180));
/// });
/// ]]></code>
/// </example>
public interface IFlourishMotionBuilder
{
    /// <summary>
    /// Enables the transition used when pages enter the content frame.
    /// </summary>
    /// <param name="enabled">A value indicating whether this transition should be enabled initially.</param>
    /// <param name="transition">The page transition to use.</param>
    /// <param name="duration">The duration used by the page transition.</param>
    /// <param name="usePersistedPreference">Whether the persisted user preference is restored and updated.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// motion.UsePageTransition(transition: FlourishPageTransition.Fade);
    /// ]]></code>
    /// </example>
    IFlourishMotionBuilder UsePageTransition(
        bool enabled = true,
        FlourishPageTransition transition = FlourishPageTransition.EntranceFromBottom,
        TimeSpan? duration = null,
        bool usePersistedPreference = true
    );

    /// <summary>
    /// Enables the transition used when the navigation panel opens or closes.
    /// </summary>
    /// <param name="enabled">A value indicating whether this transition should be enabled initially.</param>
    /// <param name="transition">The navigation panel transition to use.</param>
    /// <param name="duration">The duration used by the navigation panel transition.</param>
    /// <param name="usePersistedPreference">Whether the persisted user preference is restored and updated.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// motion.UseNavigationPanelTransition(
    ///     transition: FlourishNavigationPanelTransition.Resize);
    /// ]]></code>
    /// </example>
    IFlourishMotionBuilder UseNavigationPanelTransition(
        bool enabled = true,
        FlourishNavigationPanelTransition transition = FlourishNavigationPanelTransition.Resize,
        TimeSpan? duration = null,
        bool usePersistedPreference = true
    );

    /// <summary>
    /// Enables hover reveal animations.
    /// </summary>
    /// <param name="enabled">A value indicating whether hover reveal animation should be enabled initially.</param>
    /// <param name="duration">The duration used by hover reveal animations.</param>
    /// <param name="usePersistedPreference">Whether the persisted user preference is restored and updated.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// motion.UseHoverRevealAnimation(duration: TimeSpan.FromMilliseconds(140));
    /// ]]></code>
    /// </example>
    IFlourishMotionBuilder UseHoverRevealAnimation(
        bool enabled = true,
        TimeSpan? duration = null,
        bool usePersistedPreference = true
    );

    /// <summary>
    /// Controls whether Flourish should respect the operating system reduced-motion preference.
    /// </summary>
    /// <param name="enabled">A value indicating whether reduced-motion preferences should be respected.</param>
    /// <param name="usePersistedPreference">Whether the persisted user preference is restored and updated.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// motion.UseSystemReducedMotion();
    /// ]]></code>
    /// </example>
    IFlourishMotionBuilder UseSystemReducedMotion(
        bool enabled = true,
        bool usePersistedPreference = true
    );
}
