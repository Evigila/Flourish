using System.Windows.Controls;

namespace AckSS.Flourish.Abstract;

/// <summary>
/// Configures the visible navigation panel and navigation model.
/// </summary>
/// <example>
/// <code><![CDATA[
/// builder.ConfigureNavigation(navigation =>
/// {
///     navigation.SetDirection()
///         .SetInitiallyOpen()
///         .SetPanelWidth(openWidth: 260, closedWidth: 48)
///         .SetGroup("Navigation", groupId: 0, group =>
///     {
///         group.AddNavigableViewItem<HomePage>(isInitial: true);
///         group.AddNavigableItem("Refresh", "navigation.refresh", iconGlyph: "\uE72C");
///     });
/// });
/// ]]></code>
/// </example>
public interface IFlourishNavigationBuilder
{
    /// <summary>
    /// Sets the side of the shell where the navigation panel is displayed.
    /// </summary>
    /// <param name="direction">The navigation panel direction.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishNavigationBuilder SetDirection(
        NavigationPanelDirection direction = NavigationPanelDirection.Left
    );

    /// <summary>
    /// Sets whether the navigation panel is open when the shell is first displayed.
    /// </summary>
    /// <param name="enabled">A value indicating whether the navigation panel should start open.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishNavigationBuilder SetInitiallyOpen(bool enabled = true);

    /// <summary>
    /// Sets the navigation panel width and the resize range used by the splitter.
    /// </summary>
    /// <param name="openWidth">The width used by the open navigation panel.</param>
    /// <param name="closedWidth">The width used by the collapsed navigation panel.</param>
    /// <param name="maxWidth">The maximum open navigation panel width.</param>
    /// <param name="minWidth">The minimum open navigation panel width.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <remarks>
    /// The open width is also updated when users resize the panel with the splitter.
    /// </remarks>
    IFlourishNavigationBuilder SetPanelWidth(
        double openWidth = 220,
        double closedWidth = 48,
        double maxWidth = 420,
        double minWidth = 160
    );

    /// <summary>
    /// Sets the navigation panel title used by the legacy ungrouped navigation surface.
    /// </summary>
    /// <param name="title">The title displayed above legacy navigation items.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <remarks>
    /// When groups are configured, each group heading comes from the group display name. If group 0
    /// has no display name, Flourish does not reserve heading space at the top of the navigation panel.
    /// </remarks>
    IFlourishNavigationBuilder SetTitle(string title);

    /// <summary>
    /// Adds and configures a scrollable navigation group.
    /// </summary>
    /// <param name="displayName">The group heading. Required when <paramref name="groupId" /> is not 0.</param>
    /// <param name="groupId">The unique group ID. Lower IDs are displayed first. The default value is 0.</param>
    /// <param name="configureGroup">The group item configuration callback.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <remarks>
    /// Group IDs cannot be repeated. Group 0 may omit its display name; non-zero groups must provide
    /// one. Groups are displayed in ascending <paramref name="groupId" /> order and have extra
    /// spacing between them.
    /// </remarks>
    /// <example>
    /// <code><![CDATA[
    /// navigation.SetGroup("Navigation", groupId: 0, group =>
    /// {
    ///     group.AddNavigableViewItem<HomePage>(isInitial: true);
    ///     group.AddNavigableViewItem<GalleryPage>();
    /// });
    ///
    /// navigation.SetGroup("Tools", groupId: 1, group =>
    /// {
    ///     group.AddNavigableItem("Refresh", "gallery.refresh", iconGlyph: "\uE72C");
    /// });
    /// ]]></code>
    /// </example>
    IFlourishNavigationBuilder SetGroup(
        string? displayName = null,
        int groupId = 0,
        Action<IFlourishNavigationGroupBuilder>? configureGroup = null
    );

    /// <summary>
    /// Adds a fixed WPF page navigation item to the bottom of the navigation panel.
    /// </summary>
    /// <typeparam name="TPage">The registered page type to display.</typeparam>
    /// <param name="isInitial">A value indicating whether this page is the first page shown by the shell.</param>
    /// <param name="parentId">The optional parent node ID. Must be 0 when <paramref name="childId" /> is non-zero.</param>
    /// <param name="childId">The optional parent ID that this child follows. Must be 0 when <paramref name="parentId" /> is non-zero.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <remarks>
    /// The page must already be registered with <c>AddNavigable</c>. Fixed items are displayed in the
    /// bottom section of the navigation panel and are not affected by the scrollable group area.
    /// </remarks>
    /// <example>
    /// <code><![CDATA[
    /// navigation.AddFixedNavigableViewItem<SettingsPage>();
    /// ]]></code>
    /// </example>
    IFlourishNavigationBuilder AddFixedNavigableViewItem<TPage>(
        bool isInitial = false,
        int parentId = 0,
        int childId = 0
    )
        where TPage : Page;

    /// <summary>
    /// Adds a fixed command item to the bottom of the navigation panel.
    /// </summary>
    /// <param name="displayName">The text displayed by the fixed command item.</param>
    /// <param name="commandKey">The optional command key sent to <see cref="ICommandParser" /> when the item is invoked.</param>
    /// <param name="parentId">The optional parent node ID. Must be 0 when <paramref name="childId" /> is non-zero.</param>
    /// <param name="childId">The optional parent ID that this child follows. Must be 0 when <paramref name="parentId" /> is non-zero.</param>
    /// <param name="iconGlyph">The optional icon glyph displayed with the item.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <remarks>
    /// Command items trigger <see cref="ICommandParser" /> instead of navigating to a page. If a
    /// command item is also a parent node, clicking it toggles its children and does not execute the
    /// command key, so parent command items commonly pass <c>null</c> for <paramref name="commandKey" />.
    /// </remarks>
    /// <example>
    /// <code><![CDATA[
    /// navigation.AddFixedNavigableItem("About", "app.about", iconGlyph: "\uE946");
    /// ]]></code>
    /// </example>
    IFlourishNavigationBuilder AddFixedNavigableItem(
        string displayName,
        string? commandKey,
        int parentId = 0,
        int childId = 0,
        string? iconGlyph = null
    );
}
