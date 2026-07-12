using System.Windows;
using System.Windows.Controls.Primitives;
using ArkheideSystem.Flourish.Internal.Interaction;

namespace ArkheideSystem.Flourish.Test.Controls;

public sealed class ToolTipPlacementCalculatorTests
{
    public static TheoryData<
        int,
        Point,
        PopupPrimaryAxis
    > ShellRegions =>
        new()
        {
            {
                (int)ToolTipPlacementRegion.StatusBar,
                new Point(-10, -38),
                PopupPrimaryAxis.Vertical
            },
            {
                (int)ToolTipPlacementRegion.UpperShell,
                new Point(-10, 28),
                PopupPrimaryAxis.Vertical
            },
            {
                (int)ToolTipPlacementRegion.NavigationLeft,
                new Point(48, -5),
                PopupPrimaryAxis.Horizontal
            },
            {
                (int)ToolTipPlacementRegion.NavigationRight,
                new Point(-68, -5),
                PopupPrimaryAxis.Horizontal
            },
        };

    public static TheoryData<Point, Point, PopupPrimaryAxis> ContentEdges =>
        new()
        {
            { new Point(140, 0), new Point(-10, 28), PopupPrimaryAxis.Vertical },
            { new Point(140, 180), new Point(-10, -38), PopupPrimaryAxis.Vertical },
            { new Point(0, 90), new Point(28, -5), PopupPrimaryAxis.Horizontal },
            { new Point(280, 90), new Point(-48, -5), PopupPrimaryAxis.Horizontal },
        };

    [Theory]
    [MemberData(nameof(ShellRegions))]
    public void Calculate_UsesThePreferredSideForKnownShellRegions(
        int region,
        Point expectedPoint,
        PopupPrimaryAxis expectedAxis
    )
    {
        var placement = ToolTipPlacementCalculator.Calculate(
            (ToolTipPlacementRegion)region,
            new Point(100, 80),
            new Size(40, 20),
            new Size(60, 30),
            new Size(300, 200),
            5
        );

        Assert.Equal(expectedPoint, placement.Point);
        Assert.Equal(expectedAxis, placement.PrimaryAxis);
    }

    [Theory]
    [MemberData(nameof(ContentEdges))]
    public void Calculate_ContentPlacementOpensAwayFromTheNearestEdge(
        Point targetPosition,
        Point expectedPoint,
        PopupPrimaryAxis expectedAxis
    )
    {
        var placement = ToolTipPlacementCalculator.Calculate(
            ToolTipPlacementRegion.Content,
            targetPosition,
            new Size(20, 20),
            new Size(40, 30),
            new Size(300, 200),
            0
        );

        Assert.Equal(expectedPoint, placement.Point);
        Assert.Equal(expectedAxis, placement.PrimaryAxis);
    }

    [Fact]
    public void Calculate_ClampsThePopupInsideTheRootMargin()
    {
        var placement = ToolTipPlacementCalculator.Calculate(
            ToolTipPlacementRegion.StatusBar,
            new Point(0, 0),
            new Size(20, 20),
            new Size(80, 50),
            new Size(100, 60),
            5
        );

        Assert.Equal(new Point(5, 5), placement.Point);
        Assert.Equal(PopupPrimaryAxis.Vertical, placement.PrimaryAxis);
    }

    [Fact]
    public void CreateFallback_OpensBelowTheTargetWithTheStableOffset()
    {
        var placement = ToolTipPlacementCalculator.CreateFallback(new Size(40, 24));

        Assert.Equal(new Point(0, 32), placement.Point);
        Assert.Equal(PopupPrimaryAxis.Vertical, placement.PrimaryAxis);
    }

    [Theory]
    [InlineData(0, 80, 300, true)]
    [InlineData(110, 80, 300, false)]
    [InlineData(220, 80, 300, false)]
    public void IsLeftSide_UsesTheElementCenter(
        double elementX,
        double elementWidth,
        double rootWidth,
        bool expected
    )
    {
        var actual = ToolTipPlacementCalculator.IsLeftSide(
            new Point(elementX, 0),
            new Size(elementWidth, 100),
            new Size(rootWidth, 200)
        );

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(5.5, true)]
    [InlineData(-0.1, false)]
    [InlineData(double.NaN, false)]
    [InlineData(double.PositiveInfinity, false)]
    public void IsNonNegativeFinite_RejectsInvalidMargins(double value, bool expected)
    {
        Assert.Equal(expected, ToolTipPlacementCalculator.IsNonNegativeFinite(value));
    }
}
