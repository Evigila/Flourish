using System.Windows.Controls;
using ArkheideSystem.Flourish.Internal.Interaction;

namespace ArkheideSystem.Flourish.Test.Internal.Interaction;

public sealed class NavigationPaneColumnLayoutTests
{
    [Fact]
    public void ApplyConstraints_DirectionRoundTripClearsConstraintsFromContentColumn()
    {
        var leftColumn = new ColumnDefinition();
        var rightColumn = new ColumnDefinition();

        NavigationPaneColumnLayout.ApplyConstraints(
            leftColumn,
            rightColumn,
            isOpen: true,
            paneMinWidth: 150,
            paneMaxWidth: 480
        );
        AssertColumns(leftColumn, rightColumn, 150, 480);

        NavigationPaneColumnLayout.ApplyConstraints(
            rightColumn,
            leftColumn,
            isOpen: true,
            paneMinWidth: 150,
            paneMaxWidth: 480
        );
        AssertColumns(rightColumn, leftColumn, 150, 480);

        NavigationPaneColumnLayout.ApplyConstraints(
            leftColumn,
            rightColumn,
            isOpen: true,
            paneMinWidth: 150,
            paneMaxWidth: 480
        );
        AssertColumns(leftColumn, rightColumn, 150, 480);
    }

    private static void AssertColumns(
        ColumnDefinition paneColumn,
        ColumnDefinition contentColumn,
        double paneMinWidth,
        double paneMaxWidth
    )
    {
        Assert.Equal(paneMinWidth, paneColumn.MinWidth);
        Assert.Equal(paneMaxWidth, paneColumn.MaxWidth);
        Assert.Equal(0, contentColumn.MinWidth);
        Assert.Equal(double.PositiveInfinity, contentColumn.MaxWidth);
    }
}
