using System.Windows.Controls;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Configures toolbar items that change according to the active page.
/// </summary>
/// <example>
/// <code><![CDATA[
/// builder.ConfigureDynamicToolbar(toolbar =>
/// {
///     toolbar.CreateToolbarItems<ReportsPage>(
///         new FlourishToolbarItem("Export", "\uE898", "reports.export"));
/// });
/// ]]></code>
/// </example>
public interface IFlourishDynamicToolbarBuilder
{
    /// <summary>
    /// Creates toolbar items for the specified page type.
    /// </summary>
    /// <typeparam name="TPage">The page type associated with the toolbar items.</typeparam>
    /// <param name="items">The toolbar items displayed for the page.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// toolbar.CreateToolbarItems<ReportsPage>(
    ///     new FlourishToolbarItem("Refresh", "\uE72C", "reports.refresh"),
    ///     new FlourishToolbarItem("Export", "\uE898", "reports.export"));
    /// ]]></code>
    /// </example>
    IFlourishDynamicToolbarBuilder CreateToolbarItems<TPage>(params FlourishToolbarItem[] items)
        where TPage : Page;

    /// <summary>
    /// Creates toolbar items for the specified page type and controls whether item icons are displayed.
    /// </summary>
    /// <typeparam name="TPage">The page type associated with the toolbar items.</typeparam>
    /// <param name="icon">A value indicating whether toolbar item icons should be displayed.</param>
    /// <param name="items">The toolbar items displayed for the page.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// toolbar.CreateToolbarItems<ReportsPage>(
    ///     icon: true,
    ///     new FlourishToolbarItem("Export", "\uE898", "reports.export"));
    /// ]]></code>
    /// </example>
    IFlourishDynamicToolbarBuilder CreateToolbarItems<TPage>(
        bool icon,
        params FlourishToolbarItem[] items
    )
        where TPage : Page;
}
