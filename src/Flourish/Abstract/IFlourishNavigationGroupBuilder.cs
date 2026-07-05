using System.Windows.Controls;

namespace AcksheedSys.Flourish.Abstract;

/// <summary>
/// Configures items displayed inside a Flourish navigation panel group.
/// </summary>
public interface IFlourishNavigationGroupBuilder
{
    /// <summary>
    /// Adds a registered WPF page to this navigation group.
    /// </summary>
    IFlourishNavigationGroupBuilder AddNavigableViewItem<TPage>(
        bool isInitial = false,
        int parentId = 0,
        int childId = 0
    )
        where TPage : Page;

    /// <summary>
    /// Adds a command item to this navigation group.
    /// </summary>
    IFlourishNavigationGroupBuilder AddNavigableItem(
        string displayName,
        string? commandKey,
        int parentId = 0,
        int childId = 0,
        string? iconGlyph = null
    );
}
