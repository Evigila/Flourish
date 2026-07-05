using System.Windows.Controls;

namespace AcksheedSys.Flourish.Abstract;

/// <summary>
/// Configures the Flourish navigation panel.
/// </summary>
/// <example>
/// <code><![CDATA[
/// shell.UseNavigationPanel((_, nav) =>
/// {
///     nav.SetInitiallyOpen().SetTitle("Navigation");
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
    /// Sets the navigation panel title.
    /// </summary>
    /// <param name="title">The title displayed above navigation items.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// nav.SetTitle("Navigation");
    /// ]]></code>
    /// </example>
    IFlourishNavigationPanelBuilder SetTitle(string title);

    /// <summary>
    /// Adds and configures a navigation group. Group IDs are unique and control display order.
    /// </summary>
    /// <param name="displayName">The group heading. Required when <paramref name="groupId" /> is not 0.</param>
    /// <param name="groupId">The unique group ID. Lower IDs are displayed first.</param>
    /// <param name="configureGroup">The group item configuration callback.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishNavigationPanelBuilder SetGroup(
        string? displayName = null,
        int groupId = 0,
        Action<IFlourishNavigationGroupBuilder>? configureGroup = null
    );

    /// <summary>
    /// Adds a fixed WPF page navigation item to the bottom of the navigation panel.
    /// </summary>
    IFlourishNavigationPanelBuilder AddFixedNavigableViewItem<TPage>(
        bool isInitial = false,
        int parentId = 0,
        int childId = 0
    )
        where TPage : Page;

    /// <summary>
    /// Adds a fixed command item to the bottom of the navigation panel.
    /// </summary>
    IFlourishNavigationPanelBuilder AddFixedNavigableItem(
        string displayName,
        string? commandKey,
        int parentId = 0,
        int childId = 0,
        string? iconGlyph = null
    );
}
