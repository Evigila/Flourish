using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using WpfControl = System.Windows.Controls.Control;

namespace AckSS.Flourish.Controls;

internal static class HoverReveal
{
    public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
        "IsEnabled",
        typeof(bool),
        typeof(HoverReveal),
        new FrameworkPropertyMetadata(
            false,
            FrameworkPropertyMetadataOptions.Inherits,
            OnIsEnabledChanged
        )
    );

    public static readonly DependencyProperty AnimationDurationProperty =
        DependencyProperty.RegisterAttached(
            "AnimationDuration",
            typeof(TimeSpan),
            typeof(HoverReveal),
            new FrameworkPropertyMetadata(
                TimeSpan.FromMilliseconds(140),
                FrameworkPropertyMetadataOptions.Inherits
            )
        );

    public static TimeSpan GetAnimationDuration(DependencyObject element)
    {
        return (TimeSpan)element.GetValue(AnimationDurationProperty);
    }

    public static void SetAnimationDuration(DependencyObject element, TimeSpan value)
    {
        element.SetValue(AnimationDurationProperty, value);
    }

    public static bool GetIsEnabled(DependencyObject element)
    {
        return (bool)element.GetValue(IsEnabledProperty);
    }

    public static void SetIsEnabled(DependencyObject element, bool value)
    {
        element.SetValue(IsEnabledProperty, value);
    }

    private static void OnIsEnabledChanged(
        DependencyObject element,
        DependencyPropertyChangedEventArgs e
    )
    {
        if (element is not FrameworkElement frameworkElement)
        {
            return;
        }

        if ((bool)e.NewValue)
        {
            Attach(frameworkElement);
            return;
        }

        Detach(frameworkElement);
    }

    private static void Attach(FrameworkElement element)
    {
        element.Loaded -= Element_Loaded;
        element.MouseEnter -= Element_MouseEnter;
        element.MouseLeave -= Element_MouseLeave;

        element.Loaded += Element_Loaded;
        element.MouseEnter += Element_MouseEnter;
        element.MouseLeave += Element_MouseLeave;

        if (element.IsLoaded)
        {
            ResetReveal(element);
        }
    }

    private static void Detach(FrameworkElement element)
    {
        element.Loaded -= Element_Loaded;
        element.MouseEnter -= Element_MouseEnter;
        element.MouseLeave -= Element_MouseLeave;
        ResetReveal(element);
    }

    private static void Element_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            ResetReveal(element);
        }
    }

    private static void Element_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (sender is FrameworkElement element && GetIsEnabled(element))
        {
            BeginReveal(element);
        }
    }

    private static void Element_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            ResetReveal(element);
        }
    }

    private static void BeginReveal(FrameworkElement element)
    {
        var hoverChrome = FindTemplatePart<UIElement>(element, "HoverChrome");
        var hoverRevealScale = FindTemplatePart<ScaleTransform>(element, "HoverRevealScale");
        if (hoverChrome is null || hoverRevealScale is null)
        {
            return;
        }

        var duration = GetAnimationDuration(element);
        if (duration <= TimeSpan.Zero)
        {
            duration = TimeSpan.FromMilliseconds(140);
        }

        hoverChrome.BeginAnimation(
            UIElement.OpacityProperty,
            CreateOpacityAnimation(),
            HandoffBehavior.SnapshotAndReplace
        );
        hoverRevealScale.BeginAnimation(
            ScaleTransform.ScaleXProperty,
            CreateRevealAnimation(duration),
            HandoffBehavior.SnapshotAndReplace
        );
        hoverRevealScale.BeginAnimation(
            ScaleTransform.ScaleYProperty,
            CreateRevealAnimation(duration),
            HandoffBehavior.SnapshotAndReplace
        );
    }

    private static void ResetReveal(FrameworkElement element)
    {
        var hoverChrome = FindTemplatePart<UIElement>(element, "HoverChrome");
        var hoverRevealScale = FindTemplatePart<ScaleTransform>(element, "HoverRevealScale");

        if (hoverChrome is not null)
        {
            hoverChrome.BeginAnimation(UIElement.OpacityProperty, null);
        }

        if (hoverRevealScale is not null)
        {
            hoverRevealScale.BeginAnimation(ScaleTransform.ScaleXProperty, null);
            hoverRevealScale.BeginAnimation(ScaleTransform.ScaleYProperty, null);
        }
    }

    private static DoubleAnimation CreateOpacityAnimation()
    {
        return new DoubleAnimation
        {
            To = 1,
            Duration = new Duration(TimeSpan.Zero),
            FillBehavior = FillBehavior.HoldEnd,
        };
    }

    private static DoubleAnimation CreateRevealAnimation(TimeSpan duration)
    {
        return new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = new Duration(duration),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
            FillBehavior = FillBehavior.HoldEnd,
        };
    }

    private static T? FindTemplatePart<T>(FrameworkElement element, string name)
        where T : class
    {
        if (element is WpfControl control)
        {
            control.ApplyTemplate();
            return control.Template?.FindName(name, control) as T;
        }

        return null;
    }
}
