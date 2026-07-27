namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Specifies the animation behavior used when a page enters the content frame.
/// </summary>
/// <example>
/// <code><![CDATA[
/// builder
///     .ConfigShell(shell => shell.UseMotion())
///     .ConfigMotion(motion =>
///         motion.UsePageTransition(
///             transition: FlourishPageTransition.EntranceFromBottom));
/// ]]></code>
/// </example>
public enum FlourishPageTransition
{
    /// <summary>
    /// Disables page transition animation.
    /// </summary>
    /// <example>
    /// <code><![CDATA[
    /// motion.UsePageTransition(transition: FlourishPageTransition.None);
    /// ]]></code>
    /// </example>
    None,

    /// <summary>
    /// Fades the page into view.
    /// </summary>
    /// <example>
    /// <code><![CDATA[
    /// motion.UsePageTransition(transition: FlourishPageTransition.Fade);
    /// ]]></code>
    /// </example>
    Fade,

    /// <summary>
    /// Moves and fades the page into view from the bottom edge.
    /// </summary>
    /// <example>
    /// <code><![CDATA[
    /// motion.UsePageTransition(
    ///     transition: FlourishPageTransition.EntranceFromBottom);
    /// ]]></code>
    /// </example>
    EntranceFromBottom,
}
