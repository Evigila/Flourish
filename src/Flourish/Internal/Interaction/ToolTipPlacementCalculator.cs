using System.Windows;
using System.Windows.Controls.Primitives;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

namespace ArkheideSystem.Flourish.Internal.Interaction;

internal enum ToolTipPlacementRegion
{
    Content,
    StatusBar,
    UpperShell,
    NavigationLeft,
    NavigationRight,
}

/// <summary>
/// Calculates tooltip placement without depending on visual-tree traversal or UI events.
/// </summary>
internal static class ToolTipPlacementCalculator
{
    private const double Offset = 8;

    internal static CustomPopupPlacement Calculate(
        ToolTipPlacementRegion region,
        Point targetPosition,
        Size targetSize,
        Size popupSize,
        Size rootSize,
        double margin
    )
    {
        var targetCenter = new Point(
            targetPosition.X + targetSize.Width / 2,
            targetPosition.Y + targetSize.Height / 2
        );
        var placement = ChoosePlacement(region, targetCenter, rootSize);
        var rootPoint = ClampToRootBounds(
            CalculateRootPoint(placement, targetPosition, targetSize, popupSize),
            popupSize,
            rootSize,
            margin
        );

        return new CustomPopupPlacement(
            new Point(
                rootPoint.X - targetPosition.X,
                rootPoint.Y - targetPosition.Y
            ),
            GetPrimaryAxis(placement)
        );
    }

    internal static CustomPopupPlacement CreateFallback(Size targetSize)
    {
        return new CustomPopupPlacement(
            new Point(0, targetSize.Height + Offset),
            PopupPrimaryAxis.Vertical
        );
    }

    internal static bool IsLeftSide(
        Point elementPosition,
        Size elementSize,
        Size rootSize
    )
    {
        return elementPosition.X + elementSize.Width / 2 < rootSize.Width / 2;
    }

    internal static bool IsNonNegativeFinite(double value)
    {
        return !double.IsNaN(value) && !double.IsInfinity(value) && value >= 0;
    }

    private static PlacementMode ChoosePlacement(
        ToolTipPlacementRegion region,
        Point targetCenter,
        Size rootSize
    )
    {
        return region switch
        {
            ToolTipPlacementRegion.StatusBar => PlacementMode.Top,
            ToolTipPlacementRegion.UpperShell => PlacementMode.Bottom,
            ToolTipPlacementRegion.NavigationLeft => PlacementMode.Right,
            ToolTipPlacementRegion.NavigationRight => PlacementMode.Left,
            _ => ChooseNearestEdgePlacement(targetCenter, rootSize),
        };
    }

    private static PlacementMode ChooseNearestEdgePlacement(
        Point targetCenter,
        Size rootSize
    )
    {
        var distanceToTop = targetCenter.Y;
        var distanceToBottom = rootSize.Height - targetCenter.Y;
        var distanceToLeft = targetCenter.X;
        var distanceToRight = rootSize.Width - targetCenter.X;
        var nearestVerticalEdge = Math.Min(distanceToTop, distanceToBottom);
        var nearestHorizontalEdge = Math.Min(distanceToLeft, distanceToRight);

        if (nearestVerticalEdge <= nearestHorizontalEdge)
        {
            return distanceToTop <= distanceToBottom
                ? PlacementMode.Bottom
                : PlacementMode.Top;
        }

        return distanceToLeft <= distanceToRight
            ? PlacementMode.Right
            : PlacementMode.Left;
    }

    private static Point CalculateRootPoint(
        PlacementMode placement,
        Point targetPosition,
        Size targetSize,
        Size popupSize
    )
    {
        return placement switch
        {
            PlacementMode.Top => new Point(
                targetPosition.X + (targetSize.Width - popupSize.Width) / 2,
                targetPosition.Y - popupSize.Height - Offset
            ),
            PlacementMode.Left => new Point(
                targetPosition.X - popupSize.Width - Offset,
                targetPosition.Y + (targetSize.Height - popupSize.Height) / 2
            ),
            PlacementMode.Right => new Point(
                targetPosition.X + targetSize.Width + Offset,
                targetPosition.Y + (targetSize.Height - popupSize.Height) / 2
            ),
            _ => new Point(
                targetPosition.X + (targetSize.Width - popupSize.Width) / 2,
                targetPosition.Y + targetSize.Height + Offset
            ),
        };
    }

    private static Point ClampToRootBounds(
        Point rootPoint,
        Size popupSize,
        Size rootSize,
        double margin
    )
    {
        var minX = margin;
        var minY = margin;
        var maxX = Math.Max(minX, rootSize.Width - margin - popupSize.Width);
        var maxY = Math.Max(minY, rootSize.Height - margin - popupSize.Height);

        return new Point(
            Math.Min(Math.Max(rootPoint.X, minX), maxX),
            Math.Min(Math.Max(rootPoint.Y, minY), maxY)
        );
    }

    private static PopupPrimaryAxis GetPrimaryAxis(PlacementMode placement)
    {
        return placement is PlacementMode.Left or PlacementMode.Right
            ? PopupPrimaryAxis.Horizontal
            : PopupPrimaryAxis.Vertical;
    }
}
