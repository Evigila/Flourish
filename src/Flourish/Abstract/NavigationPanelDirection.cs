namespace AckSS.Flourish.Abstract;

/// <summary>
/// Specifies the side of the shell where the navigation panel is displayed.
/// </summary>
/// <example>
/// <code><![CDATA[
/// builder.ConfigureNavigation(nav =>
/// {
///     nav.SetDirection(NavigationPanelDirection.Left);
/// });
/// ]]></code>
/// </example>
public enum NavigationPanelDirection
{
    /// <summary>
    /// Displays the navigation panel on the left side of the shell.
    /// </summary>
    /// <example>
    /// <code><![CDATA[
    /// nav.SetDirection(NavigationPanelDirection.Left);
    /// ]]></code>
    /// </example>
    Left,

    /// <summary>
    /// Displays the navigation panel on the right side of the shell.
    /// </summary>
    /// <example>
    /// <code><![CDATA[
    /// nav.SetDirection(NavigationPanelDirection.Right);
    /// ]]></code>
    /// </example>
    Right,
}
