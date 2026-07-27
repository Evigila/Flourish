namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Specifies the animation behavior used when the navigation panel opens or closes.
/// </summary>
/// <example>
/// <code><![CDATA[
/// builder
///     .ConfigShell(shell => shell.UseMotion())
///     .ConfigMotion(motion =>
///         motion.UseNavigationPanelTransition(
///             transition: FlourishNavigationPanelTransition.Resize));
/// ]]></code>
/// </example>
public enum FlourishNavigationPanelTransition
{
    /// <summary>
    /// Disables navigation panel transition animation.
    /// </summary>
    /// <example>
    /// <code><![CDATA[
    /// motion.UseNavigationPanelTransition(
    ///     transition: FlourishNavigationPanelTransition.None);
    /// ]]></code>
    /// </example>
    None,

    /// <summary>
    /// Animates a visual resize, preserves the width and natural horizontal scale of capped
    /// centered Shell content, and commits the final layout width when the transition ends.
    /// </summary>
    /// <example>
    /// <code><![CDATA[
    /// motion.UseNavigationPanelTransition(
    ///     transition: FlourishNavigationPanelTransition.Resize);
    /// ]]></code>
    /// </example>
    Resize,
}
