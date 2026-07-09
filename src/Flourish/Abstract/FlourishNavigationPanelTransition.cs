namespace AckSS.Flourish.Abstract;

/// <summary>
/// Specifies the animation behavior used when the navigation panel opens or closes.
/// </summary>
/// <example>
/// <code><![CDATA[
/// shell.UseMotion((_, motion) =>
/// {
///     motion.EnableNavigationPanelTransition(FlourishNavigationPanelTransition.Resize);
/// });
/// ]]></code>
/// </example>
public enum FlourishNavigationPanelTransition
{
    /// <summary>
    /// Disables navigation panel transition animation.
    /// </summary>
    /// <example>
    /// <code><![CDATA[
    /// motion.EnableNavigationPanelTransition(FlourishNavigationPanelTransition.None);
    /// ]]></code>
    /// </example>
    None,

    /// <summary>
    /// Animates the navigation panel by resizing its layout column.
    /// </summary>
    /// <example>
    /// <code><![CDATA[
    /// motion.EnableNavigationPanelTransition(FlourishNavigationPanelTransition.Resize);
    /// ]]></code>
    /// </example>
    Resize,
}
