using ArkheideSystem.Flourish.Abstract;
using System.Windows.Controls;

namespace ArkheideSystem.Flourish.Navigation;

internal static class NavigationPaneColumnLayout
{
    internal static void ApplyConstraints(
        ColumnDefinition paneColumn,
        ColumnDefinition contentColumn,
        bool isOpen,
        double paneMinWidth,
        double paneMaxWidth
    )
    {
        contentColumn.MinWidth = 0;
        contentColumn.MaxWidth = double.PositiveInfinity;
        paneColumn.MinWidth = isOpen ? paneMinWidth : 0;
        paneColumn.MaxWidth = isOpen ? paneMaxWidth : double.PositiveInfinity;
    }
}
