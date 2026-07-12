using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using WpfControl = System.Windows.Controls.Control;

namespace ArkheideSystem.Flourish.Internal.Interaction;

/// <summary>
/// Applies hover-reveal visuals to controls that expose the expected template parts.
/// </summary>
internal static class HoverRevealAnimator
{
    internal static void Begin(FrameworkElement element, TimeSpan duration)
    {
        var hoverChrome = FindTemplatePart<UIElement>(element, "HoverChrome");
        var hoverRevealScale = FindTemplatePart<ScaleTransform>(element, "HoverRevealScale");
        if (hoverChrome is null || hoverRevealScale is null)
        {
            return;
        }

        if (duration <= TimeSpan.Zero)
        {
            Show(hoverChrome, hoverRevealScale);
            return;
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

    internal static void Show(FrameworkElement element)
    {
        var hoverChrome = FindTemplatePart<UIElement>(element, "HoverChrome");
        var hoverRevealScale = FindTemplatePart<ScaleTransform>(element, "HoverRevealScale");
        if (hoverChrome is null || hoverRevealScale is null)
        {
            return;
        }

        Show(hoverChrome, hoverRevealScale);
    }

    internal static void Reset(FrameworkElement element)
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

    private static void Show(UIElement hoverChrome, ScaleTransform hoverRevealScale)
    {
        // Keep the static reveal in the animation layer. Reset can then remove it
        // without overwriting the template's hidden base values or pressed-state triggers.
        hoverChrome.BeginAnimation(
            UIElement.OpacityProperty,
            CreateOpacityAnimation(),
            HandoffBehavior.SnapshotAndReplace
        );
        hoverRevealScale.BeginAnimation(
            ScaleTransform.ScaleXProperty,
            CreateOpacityAnimation(),
            HandoffBehavior.SnapshotAndReplace
        );
        hoverRevealScale.BeginAnimation(
            ScaleTransform.ScaleYProperty,
            CreateOpacityAnimation(),
            HandoffBehavior.SnapshotAndReplace
        );
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
