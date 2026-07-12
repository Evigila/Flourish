using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using WpfControl = System.Windows.Controls.Control;

namespace ArkheideSystem.Flourish.Internal.Interaction;

/// <summary>
/// Applies hover-reveal visuals to controls that expose the expected template parts.
/// </summary>
internal static class HoverRevealAnimator
{
    private static readonly DependencyProperty TemplatePartsProperty =
        DependencyProperty.RegisterAttached(
            "TemplateParts",
            typeof(TemplateParts),
            typeof(HoverRevealAnimator),
            new PropertyMetadata(null)
        );
    private static readonly CubicEase RevealEasing = CreateRevealEasing();
    private static readonly DoubleAnimation StaticRevealAnimation =
        CreateStaticRevealAnimation();

    internal static void Begin(FrameworkElement element, TimeSpan duration)
    {
        var parts = ResolveTemplateParts(element);
        if (!parts.HasParts)
        {
            return;
        }

        if (duration <= TimeSpan.Zero)
        {
            Show(parts);
            return;
        }

        parts.HoverChrome!.BeginAnimation(
            UIElement.OpacityProperty,
            StaticRevealAnimation,
            HandoffBehavior.SnapshotAndReplace
        );
        var revealAnimation = parts.GetRevealAnimation(duration, RevealEasing);
        parts.HoverRevealScale!.BeginAnimation(
            ScaleTransform.ScaleXProperty,
            revealAnimation,
            HandoffBehavior.SnapshotAndReplace
        );
        parts.HoverRevealScale.BeginAnimation(
            ScaleTransform.ScaleYProperty,
            revealAnimation,
            HandoffBehavior.SnapshotAndReplace
        );
        parts.HasAnimationClocks = true;
    }

    internal static void Show(FrameworkElement element)
    {
        Show(ResolveTemplateParts(element));
    }

    internal static void Reset(FrameworkElement element)
    {
        TryGetTemplateParts(element)?.ClearAnimationClocks();
    }

    internal static void Invalidate(FrameworkElement element)
    {
        var parts = TryGetTemplateParts(element);
        if (parts is null)
        {
            return;
        }

        parts.ClearAnimationClocks();
        element.ClearValue(TemplatePartsProperty);
    }

    internal static TemplateParts ResolveTemplateParts(FrameworkElement element)
    {
        ArgumentNullException.ThrowIfNull(element);
        var cached = TryGetTemplateParts(element);
        if (cached?.Matches(element) == true)
        {
            return cached;
        }

        if (cached is not null)
        {
            cached.ClearAnimationClocks();
            element.ClearValue(TemplatePartsProperty);
        }

        if (element is not WpfControl control)
        {
            var missing = TemplateParts.CreateMissingForNonControl();
            element.SetValue(TemplatePartsProperty, missing);
            return missing;
        }

        control.ApplyTemplate();

        // An OnApplyTemplate override may restore an already-hovered control while
        // ApplyTemplate is running. Reuse the cache created by that path.
        cached = TryGetTemplateParts(element);
        if (cached?.Matches(element) == true)
        {
            return cached;
        }

        var template = control.Template;
        var templateRoot = GetTemplateRoot(control);
        var parts = new TemplateParts(
            template,
            templateRoot,
            template?.FindName("HoverChrome", control) as UIElement,
            template?.FindName("HoverRevealScale", control) as ScaleTransform
        );
        element.SetValue(TemplatePartsProperty, parts);
        return parts;
    }

    internal static TemplateParts? TryGetTemplateParts(FrameworkElement element)
    {
        ArgumentNullException.ThrowIfNull(element);
        return element.ReadLocalValue(TemplatePartsProperty) as TemplateParts;
    }

    private static void Show(TemplateParts parts)
    {
        if (!parts.HasParts)
        {
            return;
        }

        // Keep the static reveal in the animation layer. Reset can then remove it
        // without overwriting the template's hidden base values or visual triggers.
        parts.HoverChrome!.BeginAnimation(
            UIElement.OpacityProperty,
            StaticRevealAnimation,
            HandoffBehavior.SnapshotAndReplace
        );
        parts.HoverRevealScale!.BeginAnimation(
            ScaleTransform.ScaleXProperty,
            StaticRevealAnimation,
            HandoffBehavior.SnapshotAndReplace
        );
        parts.HoverRevealScale.BeginAnimation(
            ScaleTransform.ScaleYProperty,
            StaticRevealAnimation,
            HandoffBehavior.SnapshotAndReplace
        );
        parts.HasAnimationClocks = true;
    }

    private static DependencyObject? GetTemplateRoot(WpfControl control)
    {
        return VisualTreeHelper.GetChildrenCount(control) == 0
            ? null
            : VisualTreeHelper.GetChild(control, 0);
    }

    private static CubicEase CreateRevealEasing()
    {
        var easing = new CubicEase { EasingMode = EasingMode.EaseOut };
        easing.Freeze();
        return easing;
    }

    private static DoubleAnimation CreateStaticRevealAnimation()
    {
        var animation = new DoubleAnimation
        {
            To = 1,
            Duration = new Duration(TimeSpan.Zero),
            FillBehavior = FillBehavior.HoldEnd,
        };
        animation.Freeze();
        return animation;
    }

    internal sealed class TemplateParts(
        ControlTemplate? template,
        DependencyObject? templateRoot,
        UIElement? hoverChrome,
        ScaleTransform? hoverRevealScale,
        bool representsControl = true
    )
    {
        private DoubleAnimation? revealAnimation;
        private TimeSpan revealDuration;

        internal UIElement? HoverChrome { get; } = hoverChrome;

        internal ScaleTransform? HoverRevealScale { get; } = hoverRevealScale;

        internal bool HasParts => HoverChrome is not null && HoverRevealScale is not null;

        internal bool HasAnimationClocks { get; set; }

        internal static TemplateParts CreateMissingForNonControl()
        {
            return new TemplateParts(null, null, null, null, representsControl: false);
        }

        internal bool Matches(FrameworkElement element)
        {
            if (element is not WpfControl control)
            {
                return !representsControl;
            }

            return representsControl
                && ReferenceEquals(template, control.Template)
                && ReferenceEquals(templateRoot, GetTemplateRoot(control));
        }

        internal DoubleAnimation GetRevealAnimation(
            TimeSpan duration,
            CubicEase easing
        )
        {
            if (revealAnimation is not null && revealDuration == duration)
            {
                return revealAnimation;
            }

            revealDuration = duration;
            revealAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = new Duration(duration),
                EasingFunction = easing,
                FillBehavior = FillBehavior.HoldEnd,
            };
            revealAnimation.Freeze();
            return revealAnimation;
        }

        internal void ClearAnimationClocks()
        {
            if (!HasAnimationClocks)
            {
                return;
            }

            HoverChrome?.BeginAnimation(UIElement.OpacityProperty, null);
            HoverRevealScale?.BeginAnimation(ScaleTransform.ScaleXProperty, null);
            HoverRevealScale?.BeginAnimation(ScaleTransform.ScaleYProperty, null);
            HasAnimationClocks = false;
        }
    }
}
