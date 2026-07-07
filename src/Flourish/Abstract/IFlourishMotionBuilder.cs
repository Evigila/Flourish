namespace AckSS.Flourish.Abstract;

/// <summary>
/// Configures motion and animation behavior for the Flourish shell.
/// </summary>
/// <example>
/// <code><![CDATA[
/// builder.ConfigureMotion(motion =>
/// {
///     motion.SetDuration(TimeSpan.FromMilliseconds(180));
/// });
/// ]]></code>
/// </example>
public interface IFlourishMotionBuilder
{
    /// <summary>
    /// Sets the default motion duration.
    /// </summary>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// motion.SetDuration();
    /// ]]></code>
    /// </example>
    IFlourishMotionBuilder SetDuration();

    /// <summary>
    /// Sets the motion duration.
    /// </summary>
    /// <param name="duration">The duration used by Flourish animations.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// motion.SetDuration(TimeSpan.FromMilliseconds(220));
    /// ]]></code>
    /// </example>
    IFlourishMotionBuilder SetDuration(TimeSpan duration);

    /// <summary>
    /// Sets the transition used when pages enter the content frame.
    /// </summary>
    /// <param name="transition">The page transition to use.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// motion.SetPageTransition(FlourishPageTransition.Fade);
    /// ]]></code>
    /// </example>
    IFlourishMotionBuilder SetPageTransition(
        FlourishPageTransition transition = FlourishPageTransition.EntranceFromBottom
    );

    /// <summary>
    /// Sets the transition used when the navigation panel opens or closes.
    /// </summary>
    /// <param name="transition">The navigation panel transition to use.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// motion.SetNavigationPanelTransition(FlourishNavigationPanelTransition.Resize);
    /// ]]></code>
    /// </example>
    IFlourishMotionBuilder SetNavigationPanelTransition(
        FlourishNavigationPanelTransition transition = FlourishNavigationPanelTransition.Resize
    );

    /// <summary>
    /// Enables or disables hover reveal animations.
    /// </summary>
    /// <param name="enabled">A value indicating whether hover reveal animations should be enabled.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// motion.SetHoverReveal(enabled: true);
    /// ]]></code>
    /// </example>
    IFlourishMotionBuilder SetHoverReveal(bool enabled = true);

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
