namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Specifies when breadcrumb navigation is displayed in the Flourish title bar.
/// </summary>
/// <example>
/// <code><![CDATA[
/// builder.ConfigTitleBar(titlebar =>
/// {
///     titlebar.UseBreadcrumb(option: BreadcrumbShowOption.Auto);
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
    /// titlebar.UseBreadcrumb(option: BreadcrumbShowOption.Always);
    /// ]]></code>
    /// </example>
    Always,

    /// <summary>
    /// Displays breadcrumb navigation when back or forward navigation is available; otherwise, hides it.
    /// </summary>
    /// <example>
    /// <code><![CDATA[
    /// titlebar.UseBreadcrumb(option: BreadcrumbShowOption.Auto);
    /// ]]></code>
    /// </example>
    Auto,

    /// <summary>
    /// Hides breadcrumb navigation.
    /// </summary>
    /// <example>
    /// <code><![CDATA[
    /// titlebar.UseBreadcrumb(option: BreadcrumbShowOption.Hidden);
    /// ]]></code>
    /// </example>
    Hidden,
}
