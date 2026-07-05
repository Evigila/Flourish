using System.Windows.Controls;

namespace AcksheedSys.Flourish.Abstract;

/// <summary>
/// Configures the Flourish navigation panel.
/// </summary>
/// <example>
/// <code><![CDATA[
/// shell.UseNavigationPanel((_, nav) =>
/// {
///     nav.SetInitiallyOpen()
///        .SetGroup("Navigation", groupId: 0, group =>
///        {
///            group.AddNavigableViewItem<HomePage>(isInitial: true);
///            group.AddNavigableItem("Refresh", "home.refresh", iconGlyph: "\uE72C");
///        })
///        .AddFixedNavigableViewItem<SettingsPage>();
/// });
/// ]]></code>
/// </example>
public interface IFlourishNavigationPanelBuilder
{
    /// <summary>
    /// Enables or disables the navigation panel.
    /// </summary>
    /// <param name="enabled">A value indicating whether the navigation panel should be enabled.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// nav.SetEnabled(true);
    /// ]]></code>
    /// </example>
    IFlourishNavigationPanelBuilder SetEnabled(bool enabled = true);

    /// <summary>
    /// Sets the side of the shell where the navigation panel is displayed.
    /// </summary>
    /// <param name="direction">The navigation panel direction.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// nav.SetDirection(NavigationPanelDirection.Left);
    /// ]]></code>
    /// </example>
    IFlourishNavigationPanelBuilder SetDirection(
        NavigationPanelDirection direction = NavigationPanelDirection.Left
    );

    /// <summary>
    /// Sets whether the navigation panel is open when the shell is first displayed.
    /// </summary>
    /// <param name="enabled">A value indicating whether the navigation panel should start open.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// nav.SetInitiallyOpen(false);
    /// ]]></code>
    /// </example>
    IFlourishNavigationPanelBuilder SetInitiallyOpen(bool enabled = true);

    /// <summary>
    /// Sets the navigation panel width used when the panel is open or collapsed.
    /// </summary>
    /// <param name="openWidth">The width used by the open navigation panel.</param>
    /// <param name="closedWidth">The width used by the collapsed navigation panel.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <remarks>
    /// The open width is also updated when users resize the panel with the splitter.
    /// </remarks>
    /// <example>
    /// <code><![CDATA[
    /// nav.SetPaneWidth(openWidth: 260, closedWidth: 48);
    /// ]]></code>
    /// </example>
    IFlourishNavigationPanelBuilder SetPaneWidth(
        double openWidth = 220,
        double closedWidth = 48
    );

    /// <summary>
    /// Sets the minimum and maximum width allowed while resizing the open navigation panel.
    /// </summary>
    /// <param name="minWidth">The minimum open navigation panel width.</param>
    /// <param name="maxWidth">The maximum open navigation panel width.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// nav.SetPaneWidthLimits(minWidth: 180, maxWidth: 480);
    /// ]]></code>
    /// </example>
    IFlourishNavigationPanelBuilder SetPaneWidthLimits(
        double minWidth = 160,
        double maxWidth = 420
    );

    /// <summary>
    /// Sets the navigation panel title used by the legacy ungrouped navigation surface.
    /// </summary>
    /// <param name="title">The title displayed above legacy navigation items.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <remarks>
    /// When groups are configured with <see cref="SetGroup" />, each group heading comes from the
    /// group display name. If group 0 has no display name, Flourish does not reserve heading space
    /// at the top of the navigation panel.
    /// </remarks>
    /// <example>
    /// <code><![CDATA[
    /// nav.SetTitle("Navigation");
    /// ]]></code>
    /// </example>
    IFlourishNavigationPanelBuilder SetTitle(string title);

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
    /// nav.SetGroup("Navigation", groupId: 0, group =>
    /// {
    ///     group.AddNavigableViewItem<HomePage>(isInitial: true);
    ///     group.AddNavigableViewItem<GalleryPage>();
    /// });
    ///
    /// nav.SetGroup("Tools", groupId: 1, group =>
    /// {
    ///     group.AddNavigableItem("Refresh", "gallery.refresh", iconGlyph: "\uE72C");
    /// });
    /// ]]></code>
    /// </example>
    IFlourishNavigationPanelBuilder SetGroup(
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
    /// nav.AddFixedNavigableViewItem<SettingsPage>();
    /// ]]></code>
    /// </example>
    IFlourishNavigationPanelBuilder AddFixedNavigableViewItem<TPage>(
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
    /// nav.AddFixedNavigableItem("About", "app.about", iconGlyph: "\uE946");
    /// ]]></code>
    /// </example>
    IFlourishNavigationPanelBuilder AddFixedNavigableItem(
        string displayName,
        string? commandKey,
        int parentId = 0,
        int childId = 0,
        string? iconGlyph = null
    );
}
