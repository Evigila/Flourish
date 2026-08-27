namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Specifies the animation behavior used when a page enters the content frame.
/// </summary>
/// <example>
/// <code><![CDATA[
/// builder.ConfigureMotion(motion => motion
///     .SetEnabled()
///     .SetPageTransition(
///         transition: PageTransition.EntranceFromBottom));
/// ]]></code>
/// </example>
public enum PageTransition
{
    /// <summary>
    /// Disables page transition animation.
    /// </summary>
    /// <example>
    /// <code><![CDATA[
    /// motion.SetPageTransition(transition: PageTransition.None);
    /// ]]></code>
    /// </example>
    None,

    /// <summary>
    /// Fades the page into view.
    /// </summary>
    /// <example>
    /// <code><![CDATA[
    /// motion.SetPageTransition(transition: PageTransition.Fade);
    /// ]]></code>
    /// </example>
    Fade,

    /// <summary>
    /// Moves and fades the page into view from the bottom edge.
    /// </summary>
    /// <example>
    /// <code><![CDATA[
    /// motion.SetPageTransition(
    ///     transition: PageTransition.EntranceFromBottom);
    /// ]]></code>
    /// </example>
    EntranceFromBottom,
}
