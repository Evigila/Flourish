using System.Windows.Controls;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Configures toolbar items that change according to the active page.
/// </summary>
/// <example>
/// <code><![CDATA[
/// builder.ConfigureDynamicToolbar(toolbar =>
/// {
///     toolbar.CreateToolbarItems<HomePage>(new FlourishToolbarItem("Open", "\uE8E5", "home.open"));
/// });
/// ]]></code>
/// </example>
public interface IFlourishDynamicToolbarBuilder
{
    /// <summary>
    /// Creates toolbar items for the specified page type.
    /// </summary>
    /// <param name="pageType">The page type associated with the toolbar items.</param>
    /// <param name="items">The toolbar items displayed for the page.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// toolbar.CreateToolbarItems(
    ///     typeof(HomePage),
    ///     new FlourishToolbarItem("Open", "\uE8E5", "home.open"));
    /// ]]></code>
    /// </example>
    IFlourishDynamicToolbarBuilder CreateToolbarItems(Type pageType, params FlourishToolbarItem[] items);

    /// <summary>
    /// Creates toolbar items for the specified page type and controls whether item icons are displayed.
    /// </summary>
    /// <param name="pageType">The page type associated with the toolbar items.</param>
    /// <param name="icon">A value indicating whether toolbar item icons should be displayed.</param>
    /// <param name="items">The toolbar items displayed for the page.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// toolbar.CreateToolbarItems(
    ///     typeof(ReportsPage),
    ///     icon: true,
    ///     new FlourishToolbarItem("Export", "\uE898", "reports.export"));
    /// ]]></code>
    /// </example>
    IFlourishDynamicToolbarBuilder CreateToolbarItems(
        Type pageType,
        bool icon,
        params FlourishToolbarItem[] items
    );

    /// <summary>
    /// Creates toolbar items for the specified page type.
    /// </summary>
    /// <typeparam name="TPage">The page type associated with the toolbar items.</typeparam>
    /// <param name="items">The toolbar items displayed for the page.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// toolbar.CreateToolbarItems<HomePage>(
    ///     new FlourishToolbarItem("Open", "\uE8E5", "home.open"),
    ///     new FlourishToolbarItem("Save", "\uE74E", "home.save"));
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
