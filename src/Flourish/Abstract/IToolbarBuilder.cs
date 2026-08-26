using System.Windows.Controls;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Configures toolbar items that change according to the active page.
/// </summary>
/// <example>
/// <code><![CDATA[
/// builder.ConfigureToolbar(toolbar =>
/// {
///     toolbar.Set<ReportsPage>(
///         new FlourishToolbarItem("Export", "\uE898", "cmd_reports_export"));
/// });
/// ]]></code>
/// </example>
public interface IToolbarBuilder
{
    /// <summary>Enables or disables the page-specific toolbar surface.</summary>
    IToolbarBuilder SetEnabled(bool enabled = true);

    /// <summary>
    /// Creates toolbar items for the specified page type and controls whether item icons are displayed.
    /// </summary>
    /// <typeparam name="TPage">The page type associated with the toolbar items.</typeparam>
    /// <param name="iconOnly">A value indicating whether toolbar items display icons only.</param>
    /// <param name="items">The toolbar items displayed for the page.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// toolbar.Set<ReportsPage>(
    ///     iconOnly: true,
    ///     new FlourishToolbarItem("Export", "\uE898", "cmd_reports_export"));
    /// ]]></code>
    /// </example>
    IToolbarBuilder Set<TPage>(
        bool iconOnly,
        params FlourishToolbarItem[] items
    )
        where TPage : Page;

    /// <summary>Creates icon-only toolbar items for a page type.</summary>
    IToolbarBuilder Set<TPage>(params FlourishToolbarItem[] items)
        where TPage : Page => Set<TPage>(iconOnly: true, items);
}
