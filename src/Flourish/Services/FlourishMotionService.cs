using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;
using Application = System.Windows.Application;

namespace ArkheideSystem.Flourish.Services;

internal sealed class FlourishMotionService(FlourishShellOptions options) : IMotionService
{
    private const double PageEntranceOffset = 14;
    private readonly object gate = new();
    private Window? owner;

    public FlourishMotionSettings Current
    {
        get
        {
            lock (gate)
            {
                return CaptureSettings();
            }
        }
    }

    public bool CanAnimate => CanAnimateSettings(Current);

    public event EventHandler<FlourishMotionChangedEventArgs>? Changed;

    public void SetEnabled(bool enabled)
    {
        UpdateOptions(() => options.Motion.IsEnabled = enabled);
    }

    public void SetPageTransition(
        FlourishPageTransition transition,
        TimeSpan? duration = null
    )
    {
        ValidateEnum(transition, nameof(transition));
        if (duration.HasValue)
        {
            ValidateDuration(duration.Value, nameof(duration));
        }

        UpdateOptions(() =>
        {
            options.Motion.PageTransition = transition;
            if (duration.HasValue)
            {
                options.Motion.PageTransitionDuration = duration.Value;
            }
        });
    }

    public void SetNavigationPanelTransition(
        FlourishNavigationPanelTransition transition,
        TimeSpan? duration = null
    )
    {
        ValidateEnum(transition, nameof(transition));
        if (duration.HasValue)
        {
            ValidateDuration(duration.Value, nameof(duration));
        }

        UpdateOptions(() =>
        {
            options.Motion.NavigationPanelTransition = transition;
            if (duration.HasValue)
            {
                options.Motion.NavigationPanelTransitionDuration = duration.Value;
            }
        });
    }

    public void SetHoverReveal(bool enabled, TimeSpan? duration = null)
    {
        if (duration.HasValue)
        {
            ValidateDuration(duration.Value, nameof(duration));
        }

        UpdateOptions(() =>
        {
            options.Motion.IsHoverRevealEnabled = enabled;
            if (duration.HasValue)
            {
                options.Motion.HoverRevealAnimationDuration = duration.Value;
            }
        });
    }

    public void SetRespectSystemReducedMotion(bool enabled)
    {
        UpdateOptions(() => options.Motion.RespectSystemReducedMotion = enabled);
    }

    internal void Attach(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);
        owner = window;
        ApplyResources(window, Current);
    }

    public void AnimateNavigationPane(
        ColumnDefinition column,
        double fromWidth,
        double toWidth,
        Action? completed = null
    )
    {
        var settings = Current;
        column.BeginAnimation(ColumnDefinition.WidthProperty, null);

        if (
            !CanAnimateSettings(settings)
            || settings.NavigationPanelTransition == FlourishNavigationPanelTransition.None
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
            Duration = new Duration(settings.NavigationPanelTransitionDuration),
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
        var settings = Current;
        frame.BeginAnimation(UIElement.OpacityProperty, null);
        var translate = EnsureTranslateTransform(frame);
        translate.BeginAnimation(TranslateTransform.YProperty, null);

        if (
            !CanAnimateSettings(settings)
            || settings.PageTransition == FlourishPageTransition.None
        )
        {
            frame.Opacity = 1;
            translate.Y = 0;
            return;
        }

        var duration = new Duration(settings.PageTransitionDuration);
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
                settings.PageTransition == FlourishPageTransition.EntranceFromBottom
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
            settings.PageTransition == FlourishPageTransition.EntranceFromBottom
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

    private void UpdateOptions(Action update)
    {
        FlourishMotionSettings previous;
        FlourishMotionSettings current;
        lock (gate)
        {
            previous = CaptureSettings();
            update();
            current = CaptureSettings();
            if (previous == current)
            {
                return;
            }
        }

        var attachedOwner = owner;
        if (attachedOwner is not null)
        {
            if (attachedOwner.Dispatcher.CheckAccess())
            {
                var effective = Current;
                ApplyResources(attachedOwner, effective);
                RaiseChanged(previous, effective);
            }
            else
            {
                attachedOwner.Dispatcher.Invoke(() =>
                {
                    var effective = Current;
                    ApplyResources(attachedOwner, effective);
                    RaiseChanged(previous, effective);
                });
            }

            return;
        }

        RaiseChanged(previous, current);
    }

    private void RaiseChanged(
        FlourishMotionSettings previous,
        FlourishMotionSettings current
    )
    {
        Changed?.Invoke(
            this,
            new FlourishMotionChangedEventArgs(
                previous,
                current,
                CanAnimateSettings(current)
            )
        );
    }

    private FlourishMotionSettings CaptureSettings()
    {
        return new FlourishMotionSettings(
            options.Motion.IsEnabled,
            options.Motion.PageTransition,
            options.Motion.PageTransitionDuration,
            options.Motion.NavigationPanelTransition,
            options.Motion.NavigationPanelTransitionDuration,
            options.Motion.IsHoverRevealEnabled,
            options.Motion.HoverRevealAnimationDuration,
            options.Motion.RespectSystemReducedMotion
        );
    }

    private static bool CanAnimateSettings(FlourishMotionSettings settings)
    {
        return settings.IsEnabled
            && (!settings.RespectSystemReducedMotion || SystemParameters.ClientAreaAnimation);
    }

    private static void ApplyResources(Window window, FlourishMotionSettings settings)
    {
        var isHoverRevealEnabled =
            CanAnimateSettings(settings) && settings.IsHoverRevealEnabled;
        window.Resources["FlourishHoverRevealEnabled"] = isHoverRevealEnabled;
        window.Resources["FlourishHoverRevealDuration"] =
            settings.HoverRevealAnimationDuration;
        ArkheideSystem.Flourish.Controls.HoverReveal.SetIsEnabled(
            window,
            isHoverRevealEnabled
        );
        ArkheideSystem.Flourish.Controls.HoverReveal.SetAnimationDuration(
            window,
            settings.HoverRevealAnimationDuration
        );

        var application = Application.Current;
        if (application is null)
        {
            return;
        }

        application.Resources["FlourishHoverRevealEnabled"] = isHoverRevealEnabled;
        application.Resources["FlourishHoverRevealDuration"] =
            settings.HoverRevealAnimationDuration;
    }

    private static void ValidateDuration(TimeSpan duration, string parameterName)
    {
        if (duration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                duration,
                "Duration must be greater than zero."
            );
        }
    }

    private static void ValidateEnum<TEnum>(TEnum value, string parameterName)
        where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(value))
        {
            throw new ArgumentOutOfRangeException(parameterName, value, "Unknown value.");
        }
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
