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
///     group.AddNavigableItem("Refresh", "navigation.refresh", iconGlyph: "\uE72C");
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
    /// Adds a registered WPF page to this navigation group by navigation key.
    /// </summary>
    /// <param name="navigationKey">The key registered with <c>AddNavigable</c>.</param>
    /// <param name="isInitial">A value indicating whether this page is the first page shown by the shell.</param>
    /// <param name="parentId">The optional parent node ID. Must be 0 when <paramref name="childId" /> is non-zero.</param>
    /// <param name="childId">The optional parent ID that this child follows. Must be 0 when <paramref name="parentId" /> is non-zero.</param>
    /// <returns>The current group builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// group.AddNavigableViewItem(NavigationRoutes.Home, isInitial: true);
    /// ]]></code>
    /// </example>
    IFlourishNavigationGroupBuilder AddNavigableViewItem(
        string navigationKey,
        bool isInitial = false,
        int parentId = 0,
        int childId = 0
    );

    /// <summary>
    /// Adds a command item to this navigation group.
    /// </summary>
    /// <param name="displayName">The text displayed by the command item.</param>
    /// <param name="commandKey">The optional command key sent to <see cref="ICommandParser" /> when the item is invoked.</param>
    /// <param name="parentId">The optional parent node ID. Must be 0 when <paramref name="childId" /> is non-zero.</param>
    /// <param name="childId">The optional parent ID that this child follows. Must be 0 when <paramref name="parentId" /> is non-zero.</param>
    /// <param name="iconGlyph">The optional icon glyph displayed with the item.</param>
    /// <returns>The current group builder for chained configuration.</returns>
    /// <remarks>
    /// Command items execute <see cref="ICommandParser" /> and do not remain selected after they are
    /// invoked. If a command item is also a parent node, clicking it toggles its children and does
    /// not execute <paramref name="commandKey" />.
    /// </remarks>
    /// <example>
    /// <code><![CDATA[
    /// group.AddNavigableItem("Hello", "demo.hello", iconGlyph: "\uE8F2");
    /// group.AddNavigableItem("World", "demo.world", iconGlyph: "\uE774");
    /// ]]></code>
    /// </example>
    IFlourishNavigationGroupBuilder AddNavigableItem(
        string displayName,
        string? commandKey,
        int parentId = 0,
        int childId = 0,
        string? iconGlyph = null
    );
}
