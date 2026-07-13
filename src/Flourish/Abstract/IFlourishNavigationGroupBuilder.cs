using System.Windows.Controls;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Configures items displayed inside a Flourish navigation panel group.
/// </summary>
/// <example>
/// <code><![CDATA[
/// navigation.SetGroup("Navigation", groupId: 0, group =>
/// {
///     group.AddNavigableViewItem<HomePage>(isInitial: true);
///     group.AddNavigableItem("Refresh", "\uE72C", "navigation.refresh");
/// });
/// ]]></code>
/// </example>
public interface IFlourishNavigationGroupBuilder
{
    /// <summary>
    /// Adds a registered WPF page to this navigation group.
    /// </summary>
    /// <typeparam name="TPage">The registered page type to display.</typeparam>
    /// <param name="isInitial">A value indicating whether this page is the first page shown by the shell.</param>
    /// <param name="parentId">The optional parent node ID. Must be 0 when <paramref name="childId" /> is non-zero.</param>
    /// <param name="childId">The optional parent ID that this child follows. Must be 0 when <paramref name="parentId" /> is non-zero.</param>
    /// <returns>The current group builder for chained configuration.</returns>
    /// <remarks>
    /// The page display name, icon glyph, and cache mode come from <c>AddNavigable</c>. A page can
    /// only be added to one navigation position across all groups and fixed items.
    /// </remarks>
    /// <example>
    /// <code><![CDATA[
    /// group.AddNavigableViewItem<HomePage>(isInitial: true);
    /// group.AddNavigableViewItem<ReportsPage>();
    /// ]]></code>
    /// </example>
    IFlourishNavigationGroupBuilder AddNavigableViewItem<TPage>(
        bool isInitial = false,
        int parentId = 0,
        int childId = 0
    )
        where TPage : Page;

    /// <summary>
    /// Adds a command item to this navigation group.
    /// </summary>
    /// <param name="displayName">The text displayed by the command item.</param>
    /// <param name="iconGlyph">The icon glyph displayed with the item, or <see langword="null" /> to omit it.</param>
    /// <param name="commandKey">The command key dispatched through <see cref="ICommandDispatcher" />, or <see langword="null" /> for an item that only groups children.</param>
    /// <param name="parentId">The optional parent node ID. Must be 0 when <paramref name="childId" /> is non-zero.</param>
    /// <param name="childId">The optional parent ID that this child follows. Must be 0 when <paramref name="parentId" /> is non-zero.</param>
    /// <returns>The current group builder for chained configuration.</returns>
    /// <remarks>
    /// Command items dispatch through <see cref="ICommandDispatcher" /> and do not remain selected after they are
    /// invoked. If a command item is also a parent node, clicking it toggles its children and does
    /// not execute <paramref name="commandKey" />.
    /// </remarks>
    /// <example>
    /// <code><![CDATA[
    /// group.AddNavigableItem("Refresh", "\uE72C", "reports.refresh");
    /// group.AddNavigableItem("Export", "\uE898", "reports.export");
    /// ]]></code>
    /// </example>
    IFlourishNavigationGroupBuilder AddNavigableItem(
        string displayName,
        string? iconGlyph,
        string? commandKey,
        int parentId = 0,
        int childId = 0
    );
}
