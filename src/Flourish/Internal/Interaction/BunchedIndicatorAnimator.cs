using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace ArkheideSystem.Flourish.Internal.Interaction;

/// <summary>
/// Moves one parent-owned indicator between item-container bounds without restarting
/// its visibility state.
/// </summary>
internal sealed class BunchedIndicatorAnimator
{
    private const double GeometryTolerance = 0.1;
    private static readonly CubicEase MovementEasing = CreateMovementEasing();

    internal Rect GetCurrentBounds(FrameworkElement indicator)
    {
        ArgumentNullException.ThrowIfNull(indicator);
        return new Rect(
            NormalizeCoordinate(Canvas.GetLeft(indicator)),
            NormalizeCoordinate(Canvas.GetTop(indicator)),
            NormalizeLength(indicator.Width),
            NormalizeLength(indicator.Height)
        );
    }

    internal void SetBounds(FrameworkElement indicator, Rect bounds)
    {
        ArgumentNullException.ThrowIfNull(indicator);
        ValidateBounds(bounds);
        StopGeometry(indicator);
        ApplyBounds(indicator, bounds);
    }

    internal bool Move(FrameworkElement indicator, Rect bounds, TimeSpan duration, bool animate)
    {
        ArgumentNullException.ThrowIfNull(indicator);
        ValidateBounds(bounds);

        var current = GetCurrentBounds(indicator);
        StopGeometry(indicator);
        ApplyBounds(indicator, bounds);

        if (
            !animate
            || duration <= TimeSpan.Zero
            || indicator.Opacity <= 0
            || AreClose(current, bounds)
        )
        {
            return false;
        }

        AnimateDouble(indicator, Canvas.LeftProperty, current.Left, bounds.Left, duration);
        AnimateDouble(indicator, Canvas.TopProperty, current.Top, bounds.Top, duration);
        AnimateDouble(
            indicator,
            FrameworkElement.WidthProperty,
            current.Width,
            bounds.Width,
            duration
        );
        AnimateDouble(
            indicator,
            FrameworkElement.HeightProperty,
            current.Height,
            bounds.Height,
            duration
        );
        return true;
    }

    internal void Show(FrameworkElement indicator, TimeSpan duration, bool animate)
    {
        AnimateOpacity(indicator, 1, duration, animate);
    }

    internal void Hide(FrameworkElement indicator, TimeSpan duration, bool animate)
    {
        AnimateOpacity(indicator, 0, duration, animate);
    }

    internal void Stop(FrameworkElement indicator, double opacity)
    {
        ArgumentNullException.ThrowIfNull(indicator);
        StopGeometry(indicator);
        indicator.BeginAnimation(UIElement.OpacityProperty, null);
        indicator.Opacity = opacity;
    }

    private static void AnimateOpacity(
        FrameworkElement indicator,
        double target,
        TimeSpan duration,
        bool animate
    )
    {
        var current = indicator.Opacity;
        indicator.BeginAnimation(UIElement.OpacityProperty, null);
        indicator.Opacity = target;
        if (!animate || duration <= TimeSpan.Zero || Math.Abs(current - target) < 0.001)
        {
            return;
        }

        indicator.BeginAnimation(
            UIElement.OpacityProperty,
            new DoubleAnimation(current, target, new Duration(duration))
            {
                EasingFunction = MovementEasing,
                FillBehavior = FillBehavior.Stop,
            },
            HandoffBehavior.SnapshotAndReplace
        );
    }

    private static void AnimateDouble(
        FrameworkElement indicator,
        DependencyProperty property,
        double from,
        double to,
        TimeSpan duration
    )
    {
        indicator.BeginAnimation(
            property,
            new DoubleAnimation(from, to, new Duration(duration))
            {
                EasingFunction = MovementEasing,
                FillBehavior = FillBehavior.Stop,
            },
            HandoffBehavior.SnapshotAndReplace
        );
    }

    private static void StopGeometry(FrameworkElement indicator)
    {
        indicator.BeginAnimation(Canvas.LeftProperty, null);
        indicator.BeginAnimation(Canvas.TopProperty, null);
        indicator.BeginAnimation(FrameworkElement.WidthProperty, null);
        indicator.BeginAnimation(FrameworkElement.HeightProperty, null);
    }

    private static void ApplyBounds(FrameworkElement indicator, Rect bounds)
    {
        Canvas.SetLeft(indicator, bounds.Left);
        Canvas.SetTop(indicator, bounds.Top);
        indicator.Width = bounds.Width;
        indicator.Height = bounds.Height;
    }

    private static bool AreClose(Rect left, Rect right)
    {
        return Math.Abs(left.Left - right.Left) < GeometryTolerance
            && Math.Abs(left.Top - right.Top) < GeometryTolerance
            && Math.Abs(left.Width - right.Width) < GeometryTolerance
            && Math.Abs(left.Height - right.Height) < GeometryTolerance;
    }

    private static double NormalizeCoordinate(double value)
    {
        return double.IsFinite(value) ? value : 0;
    }

    private static double NormalizeLength(double value)
    {
        return double.IsFinite(value) && value >= 0 ? value : 0;
    }

    private static void ValidateBounds(Rect bounds)
    {
        if (
            bounds.IsEmpty
            || !double.IsFinite(bounds.X)
            || !double.IsFinite(bounds.Y)
            || !double.IsFinite(bounds.Width)
            || !double.IsFinite(bounds.Height)
            || bounds.Width < 0
            || bounds.Height < 0
        )
        {
            throw new ArgumentOutOfRangeException(nameof(bounds));
        }
    }

    private static CubicEase CreateMovementEasing()
    {
        var easing = new CubicEase { EasingMode = EasingMode.EaseOut };
        easing.Freeze();
        return easing;
    }
}
