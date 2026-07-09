namespace AckSS.Flourish.Abstract;

/// <summary>
/// Configures motion and animation behavior for the Flourish shell.
/// </summary>
/// <example>
/// <code><![CDATA[
/// builder.ConfigureMotion(motion =>
/// {
///     motion.EnablePageTransition(
///         FlourishPageTransition.EntranceFromBottom,
///         TimeSpan.FromMilliseconds(180));
/// });
/// ]]></code>
/// </example>
public interface IFlourishMotionBuilder
{
    /// <summary>
    /// Enables the transition used when pages enter the content frame.
    /// </summary>
    /// <param name="transition">The page transition to use.</param>
    /// <param name="duration">The duration used by the page transition.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// motion.EnablePageTransition(FlourishPageTransition.Fade);
    /// ]]></code>
    /// </example>
    IFlourishMotionBuilder EnablePageTransition(
        FlourishPageTransition transition = FlourishPageTransition.EntranceFromBottom,
        TimeSpan? duration = null
    );

    /// <summary>
    /// Enables the transition used when the navigation panel opens or closes.
    /// </summary>
    /// <param name="transition">The navigation panel transition to use.</param>
    /// <param name="duration">The duration used by the navigation panel transition.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// motion.EnableNavigationPanelTransition(FlourishNavigationPanelTransition.Resize);
    /// ]]></code>
    /// </example>
    IFlourishMotionBuilder EnableNavigationPanelTransition(
        FlourishNavigationPanelTransition transition = FlourishNavigationPanelTransition.Resize,
        TimeSpan? duration = null
    );

    /// <summary>
    /// Enables hover reveal animations.
    /// </summary>
    /// <param name="duration">The duration used by hover reveal animations.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// motion.EnableHoverRevealAnimation(TimeSpan.FromMilliseconds(140));
    /// ]]></code>
    /// </example>
    IFlourishMotionBuilder EnableHoverRevealAnimation(TimeSpan? duration = null);

    /// <summary>
    /// Controls whether Flourish should respect the operating system reduced-motion preference.
    /// </summary>
    /// <param name="enabled">A value indicating whether reduced-motion preferences should be respected.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// motion.RespectSystemReducedMotion();
    /// ]]></code>
    /// </example>
    IFlourishMotionBuilder RespectSystemReducedMotion(bool enabled = true);
}
