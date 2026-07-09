using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using AckSS.Flourish.Abstract;
using AckSS.Flourish.Configuration;

namespace AckSS.Flourish.Services;

internal sealed class FlourishMotionService(FlourishShellOptions options)
{
    private const double PageEntranceOffset = 14;

    private bool CanAnimate =>
        options.Motion.IsEnabled
        && (!options.Motion.RespectSystemReducedMotion || SystemParameters.ClientAreaAnimation);

    public void AnimateNavigationPane(
        ColumnDefinition column,
        double fromWidth,
        double toWidth,
        Action? completed = null
    )
    {
        column.BeginAnimation(ColumnDefinition.WidthProperty, null);

        if (
            !CanAnimate
            || options.Motion.NavigationPanelTransition == FlourishNavigationPanelTransition.None
            || AreClose(fromWidth, toWidth)
        )
        {
            column.Width = new GridLength(toWidth);
            completed?.Invoke();
            return;
        }

        var animation = new GridLengthAnimation
        {
            From = new GridLength(fromWidth),
            To = new GridLength(toWidth),
            Duration = new Duration(options.Motion.NavigationPanelTransitionDuration),
            EasingFunction = CreateEase(),
            FillBehavior = FillBehavior.Stop,
        };

        animation.Completed += (_, _) =>
        {
            column.Width = new GridLength(toWidth);
            completed?.Invoke();
        };

        column.BeginAnimation(
            ColumnDefinition.WidthProperty,
            animation,
            HandoffBehavior.SnapshotAndReplace
        );
    }

    public void AnimatePageEntrance(FrameworkElement frame)
    {
        frame.BeginAnimation(UIElement.OpacityProperty, null);
        var translate = EnsureTranslateTransform(frame);
        translate.BeginAnimation(TranslateTransform.YProperty, null);

        if (!CanAnimate || options.Motion.PageTransition == FlourishPageTransition.None)
        {
            frame.Opacity = 1;
            translate.Y = 0;
            return;
        }

        var duration = new Duration(options.Motion.PageTransitionDuration);
        var opacityAnimation = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = duration,
            EasingFunction = CreateEase(),
            FillBehavior = FillBehavior.Stop,
        };

        var yAnimation = new DoubleAnimation
        {
            From =
                options.Motion.PageTransition == FlourishPageTransition.EntranceFromBottom
                    ? PageEntranceOffset
                    : 0,
            To = 0,
            Duration = duration,
            EasingFunction = CreateEase(),
            FillBehavior = FillBehavior.Stop,
        };

        opacityAnimation.Completed += (_, _) =>
        {
            frame.Opacity = 1;
            translate.Y = 0;
        };

        frame.Opacity = 0;
        translate.Y =
            options.Motion.PageTransition == FlourishPageTransition.EntranceFromBottom
                ? PageEntranceOffset
                : 0;

        frame.BeginAnimation(
            UIElement.OpacityProperty,
            opacityAnimation,
            HandoffBehavior.SnapshotAndReplace
        );
        translate.BeginAnimation(
            TranslateTransform.YProperty,
            yAnimation,
            HandoffBehavior.SnapshotAndReplace
        );
    }

    private static CubicEase CreateEase()
    {
        return new CubicEase { EasingMode = EasingMode.EaseOut };
    }

    private static TranslateTransform EnsureTranslateTransform(FrameworkElement element)
    {
        if (element.RenderTransform is TranslateTransform translate)
        {
            return translate;
        }

        translate = new TranslateTransform();
        element.RenderTransform = translate;
        return translate;
    }

    private static bool AreClose(double first, double second)
    {
        return Math.Abs(first - second) < 0.5;
    }
}

internal sealed class GridLengthAnimation : AnimationTimeline
{
    public static readonly DependencyProperty FromProperty = DependencyProperty.Register(
        nameof(From),
        typeof(GridLength),
        typeof(GridLengthAnimation)
    );

    public static readonly DependencyProperty ToProperty = DependencyProperty.Register(
        nameof(To),
        typeof(GridLength),
        typeof(GridLengthAnimation)
    );

    public static readonly DependencyProperty EasingFunctionProperty = DependencyProperty.Register(
        nameof(EasingFunction),
        typeof(IEasingFunction),
        typeof(GridLengthAnimation)
    );

    public GridLength From
    {
        get => (GridLength)GetValue(FromProperty);
        set => SetValue(FromProperty, value);
    }

    public GridLength To
    {
        get => (GridLength)GetValue(ToProperty);
        set => SetValue(ToProperty, value);
    }

    public IEasingFunction? EasingFunction
    {
        get => (IEasingFunction?)GetValue(EasingFunctionProperty);
        set => SetValue(EasingFunctionProperty, value);
    }

    public override Type TargetPropertyType => typeof(GridLength);

    protected override Freezable CreateInstanceCore()
    {
        return new GridLengthAnimation();
    }

    public override object GetCurrentValue(
        object defaultOriginValue,
        object defaultDestinationValue,
        AnimationClock animationClock
    )
    {
        var progress = animationClock.CurrentProgress ?? 1;
        var easedProgress = EasingFunction?.Ease(progress) ?? progress;
        var value = From.Value + ((To.Value - From.Value) * easedProgress);

        return new GridLength(Math.Max(0, value));
    }
}
