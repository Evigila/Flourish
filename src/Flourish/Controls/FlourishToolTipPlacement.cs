using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Point = System.Windows.Point;
using Size = System.Windows.Size;
using ToolTip = System.Windows.Controls.ToolTip;

namespace ArkheideSystem.Flourish.Controls;

/// <summary>
/// Provides shell-region-aware placement for Flourish tooltips.
/// </summary>
public static class FlourishToolTipPlacement
{
    /// <summary>
    /// Identifies whether a tooltip should use shell-region-aware placement.
    /// </summary>
    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(FlourishToolTipPlacement),
            new PropertyMetadata(false, OnIsEnabledChanged)
        );

    private const double DefaultSpawnableMargin = 5;
    private const double Offset = 8;

    /// <summary>
    /// Gets whether shell-region-aware tooltip placement is enabled.
    /// </summary>
    /// <param name="element">The tooltip to read from.</param>
    /// <returns><see langword="true" /> when shell-region-aware placement is enabled.</returns>
    public static bool GetIsEnabled(DependencyObject element)
    {
        return (bool)element.GetValue(IsEnabledProperty);
    }

    /// <summary>
    /// Sets whether shell-region-aware tooltip placement is enabled.
    /// </summary>
    /// <param name="element">The tooltip to configure.</param>
    /// <param name="value">A value indicating whether shell-region-aware placement is enabled.</param>
    public static void SetIsEnabled(DependencyObject element, bool value)
    {
        element.SetValue(IsEnabledProperty, value);
    }

    private static void OnIsEnabledChanged(
        DependencyObject element,
        DependencyPropertyChangedEventArgs e
    )
    {
        if (element is not ToolTip toolTip)
        {
            return;
        }

        toolTip.CustomPopupPlacementCallback = (bool)e.NewValue
            ? (popupSize, targetSize, offset) =>
                CreatePopupPlacement(toolTip, popupSize, targetSize)
            : null;
    }

    private static CustomPopupPlacement[] CreatePopupPlacement(
        ToolTip toolTip,
        Size popupSize,
        Size targetSize
    )
    {
        if (toolTip.PlacementTarget is not FrameworkElement target)
        {
            return [CreateFallbackPlacement(targetSize)];
        }

        var root = Window.GetWindow(target) as FrameworkElement;
        if (root is null || root.ActualWidth <= 0 || root.ActualHeight <= 0)
        {
            return [CreateFallbackPlacement(targetSize)];
        }

        if (!TryGetElementPosition(target, root, out var targetPosition))
        {
            return [CreateFallbackPlacement(targetSize)];
        }

        var targetCenter = new Point(
            targetPosition.X + targetSize.Width / 2,
            targetPosition.Y + targetSize.Height / 2
        );
        var placement = ChoosePlacement(target, root, targetCenter);
        var rootPoint = ClampToRootBounds(
            CalculateRootPoint(placement, targetPosition, targetSize, popupSize),
            popupSize,
            root.ActualWidth,
            root.ActualHeight,
            GetSpawnableMargin(target)
        );
        var targetRelativePoint = new Point(
            rootPoint.X - targetPosition.X,
            rootPoint.Y - targetPosition.Y
        );

        return [new CustomPopupPlacement(targetRelativePoint, GetPrimaryAxis(placement))];
    }

    private static PlacementMode ChoosePlacement(
        FrameworkElement target,
        FrameworkElement root,
        Point targetCenter
    )
    {
        if (FindAncestorByName(target, "StatusBarBorder") is not null)
        {
            return PlacementMode.Top;
        }

        if (
            FindAncestorByName(target, "Titlebar") is not null
            || FindAncestorByTypeName(target, "FlourishTitlebar") is not null
            || FindAncestorByName(target, "ToolbarHostBorder") is not null
            || FindAncestorByName(target, "BreadcrumbHost") is not null
        )
        {
            return PlacementMode.Bottom;
        }

        if (FindAncestorByName(target, "NavigationPaneBorder") is { } navigationPane)
        {
            return IsLeftSide(navigationPane, root) ? PlacementMode.Right : PlacementMode.Left;
        }

        return ChooseNearestEdgePlacement(targetCenter, root.ActualWidth, root.ActualHeight);
    }

    private static PlacementMode ChooseNearestEdgePlacement(
        Point targetCenter,
        double rootWidth,
        double rootHeight
    )
    {
        var distanceToTop = targetCenter.Y;
        var distanceToBottom = rootHeight - targetCenter.Y;
        var distanceToLeft = targetCenter.X;
        var distanceToRight = rootWidth - targetCenter.X;
        var nearestVerticalEdge = Math.Min(distanceToTop, distanceToBottom);
        var nearestHorizontalEdge = Math.Min(distanceToLeft, distanceToRight);

        if (nearestVerticalEdge <= nearestHorizontalEdge)
        {
            return distanceToTop <= distanceToBottom ? PlacementMode.Bottom : PlacementMode.Top;
        }

        return distanceToLeft <= distanceToRight ? PlacementMode.Right : PlacementMode.Left;
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
        double rootWidth,
        double rootHeight,
        double margin
    )
    {
        var minX = margin;
        var minY = margin;
        var maxX = Math.Max(minX, rootWidth - margin - popupSize.Width);
        var maxY = Math.Max(minY, rootHeight - margin - popupSize.Height);

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

    private static CustomPopupPlacement CreateFallbackPlacement(Size targetSize)
    {
        return new CustomPopupPlacement(
            new Point(0, targetSize.Height + Offset),
            PopupPrimaryAxis.Vertical
        );
    }

    private static double GetSpawnableMargin(FrameworkElement target)
    {
        return target.TryFindResource("FlourishToolTipSpawnableMargin") switch
        {
            double margin when IsNonNegativeFinite(margin) => margin,
            int margin when margin >= 0 => margin,
            _ => DefaultSpawnableMargin,
        };
    }

    private static bool IsLeftSide(FrameworkElement element, FrameworkElement root)
    {
        return TryGetElementPosition(element, root, out var position)
            && position.X + element.ActualWidth / 2 < root.ActualWidth / 2;
    }

    private static FrameworkElement? FindAncestorByName(DependencyObject source, string name)
    {
        var current = source;
        while (current is not null)
        {
            if (
                current is FrameworkElement { Name: var currentName } element
                && string.Equals(currentName, name, StringComparison.Ordinal)
            )
            {
                return element;
            }

            current = VisualTreeHelper.GetParent(current);
        }

        return null;
    }

    private static FrameworkElement? FindAncestorByTypeName(
        DependencyObject source,
        string typeName
    )
    {
        var current = source;
        while (current is not null)
        {
            if (
                current is FrameworkElement element
                && string.Equals(element.GetType().Name, typeName, StringComparison.Ordinal)
            )
            {
                return element;
            }

            current = VisualTreeHelper.GetParent(current);
        }

        return null;
    }

    private static bool TryGetElementPosition(
        FrameworkElement element,
        FrameworkElement root,
        out Point position
    )
    {
        try
        {
            position = element.TransformToAncestor(root).Transform(new Point());
            return true;
        }
        catch (InvalidOperationException)
        {
            position = new Point();
            return false;
        }
    }

    private static bool IsNonNegativeFinite(double value)
    {
        return !double.IsNaN(value) && !double.IsInfinity(value) && value >= 0;
    }
}
