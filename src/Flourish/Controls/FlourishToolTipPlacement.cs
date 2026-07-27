using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using ArkheideSystem.Flourish.Internal.Interaction;
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
            ? (popupSize, targetSize, _) =>
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
            return [ToolTipPlacementCalculator.CreateFallback(targetSize)];
        }

        var root = Window.GetWindow(target) as FrameworkElement;
        if (root is null || root.ActualWidth <= 0 || root.ActualHeight <= 0)
        {
            return [ToolTipPlacementCalculator.CreateFallback(targetSize)];
        }

        if (!TryGetElementPosition(target, root, out var targetPosition))
        {
            return [ToolTipPlacementCalculator.CreateFallback(targetSize)];
        }

        return
        [
            ToolTipPlacementCalculator.Calculate(
                ResolveRegion(target, root),
                targetPosition,
                targetSize,
                popupSize,
                new Size(root.ActualWidth, root.ActualHeight),
                GetSpawnableMargin(target)
            ),
        ];
    }

    internal static ToolTipPlacementRegion ResolveRegion(
        FrameworkElement target,
        FrameworkElement root
    )
    {
        var isInStatusBar = false;
        var isInUpperShell = false;
        FrameworkElement? navigationPane = null;

        for (
            DependencyObject? current = target;
            current is not null;
            current = VisualTreeHelper.GetParent(current)
        )
        {
            if (current is not FrameworkElement element)
            {
                continue;
            }

            var name = element.Name;
            isInStatusBar |= string.Equals(
                name,
                "StatusBarBorder",
                StringComparison.Ordinal
            );
            isInUpperShell |=
                string.Equals(name, "Titlebar", StringComparison.Ordinal)
                || string.Equals(
                    element.GetType().Name,
                    "FlourishTitlebar",
                    StringComparison.Ordinal
                )
                || string.Equals(name, "ToolbarHostBorder", StringComparison.Ordinal)
                || string.Equals(name, "BreadcrumbHost", StringComparison.Ordinal);

            if (
                navigationPane is null
                && string.Equals(
                    name,
                    "NavigationPaneBorder",
                    StringComparison.Ordinal
                )
            )
            {
                navigationPane = element;
            }
        }

        // Preserve the original category precedence even for unusual nested templates.
        if (isInStatusBar)
        {
            return ToolTipPlacementRegion.StatusBar;
        }

        if (isInUpperShell)
        {
            return ToolTipPlacementRegion.UpperShell;
        }

        if (navigationPane is not null)
        {
            return IsLeftSide(navigationPane, root)
                ? ToolTipPlacementRegion.NavigationLeft
                : ToolTipPlacementRegion.NavigationRight;
        }

        return ToolTipPlacementRegion.Content;
    }

    private static double GetSpawnableMargin(FrameworkElement target)
    {
        return target.TryFindResource("FlourishToolTipSpawnableMargin") switch
        {
            double margin when ToolTipPlacementCalculator.IsNonNegativeFinite(margin) =>
                margin,
            int margin when margin >= 0 => margin,
            _ => DefaultSpawnableMargin,
        };
    }

    private static bool IsLeftSide(FrameworkElement element, FrameworkElement root)
    {
        return TryGetElementPosition(element, root, out var position)
            && ToolTipPlacementCalculator.IsLeftSide(
                position,
                new Size(element.ActualWidth, element.ActualHeight),
                new Size(root.ActualWidth, root.ActualHeight)
            );
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
}
