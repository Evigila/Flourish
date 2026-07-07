namespace AckSS.Flourish.Abstract;

/// <summary>
/// Specifies when breadcrumb navigation is displayed in the Flourish title bar.
/// </summary>
/// <example>
/// <code><![CDATA[
/// builder.ConfigureTitleBar(titlebar =>
/// {
///     titlebar.SetBreadcrumbBehavior(BreadcrumbShowOption.Auto);
/// });
/// ]]></code>
/// </example>
public enum BreadcrumbShowOption
{
    /// <summary>
    /// Always displays breadcrumb navigation when the title bar is visible.
    /// </summary>
    /// <example>
    /// <code><![CDATA[
    /// titlebar.SetBreadcrumbBehavior(BreadcrumbShowOption.Always);
    /// ]]></code>
    /// </example>
    Always,

    /// <summary>
    /// Lets Flourish decide whether breadcrumb navigation should be displayed for the current navigation state.
    /// </summary>
    /// <example>
    /// <code><![CDATA[
    /// titlebar.SetBreadcrumbBehavior(BreadcrumbShowOption.Auto);
    /// ]]></code>
    /// </example>
    Auto,

    /// <summary>
    /// Hides breadcrumb navigation.
    /// </summary>
    /// <example>
    /// <code><![CDATA[
    /// titlebar.SetBreadcrumbBehavior(BreadcrumbShowOption.Hidden);
    /// ]]></code>
    /// </example>
    Hidden,
}
