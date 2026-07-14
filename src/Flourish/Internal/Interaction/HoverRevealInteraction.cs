using System.Windows;
using ArkheideSystem.Flourish.Controls;

namespace ArkheideSystem.Flourish.Internal.Interaction;

/// <summary>
/// Owns the pointer lifecycle and runtime policy for the public HoverReveal behavior.
/// </summary>
internal static class HoverRevealInteraction
{
    internal static void SetParticipant(FrameworkElement element, bool isParticipant)
    {
        if (isParticipant)
        {
            Attach(element);
        }
        else
        {
            Detach(element);
        }
    }

    internal static void Refresh(FrameworkElement element)
    {
        if (!HoverReveal.GetIsParticipant(element))
        {
            return;
        }

        HoverRevealAnimator.Invalidate(element);
        RefreshInteractionHandlers(element);
        RestoreIfHovered(element);
    }

    internal static void NotifyTemplateApplied(FrameworkElement element)
    {
        ArgumentNullException.ThrowIfNull(element);
        HoverRevealAnimator.Invalidate(element);
        if (!HoverReveal.GetIsParticipant(element) || !element.IsLoaded)
        {
            return;
        }

        RefreshInteractionHandlers(element);
        RestoreIfHovered(element);
    }

    private static void Attach(FrameworkElement element)
    {
        element.Loaded -= Element_Loaded;
        element.Unloaded -= Element_Unloaded;
        DisconnectInteractionHandlers(element);

        element.Loaded += Element_Loaded;
        element.Unloaded += Element_Unloaded;

        if (element.IsLoaded)
        {
            RefreshInteractionHandlers(element);
            RestoreIfHovered(element);
        }
    }

    private static void Detach(FrameworkElement element)
    {
        element.Loaded -= Element_Loaded;
        element.Unloaded -= Element_Unloaded;
        DisconnectInteractionHandlers(element);
        HoverRevealAnimator.Invalidate(element);
    }

    private static void Element_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            RefreshInteractionHandlers(element);
            RestoreIfHovered(element);
        }
    }

    private static void Element_Unloaded(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            DisconnectInteractionHandlers(element);
            HoverRevealAnimator.Invalidate(element);
        }
    }

    private static void Element_MouseEnter(
        object sender,
        System.Windows.Input.MouseEventArgs e
    )
    {
        if (sender is FrameworkElement { IsEnabled: true } element)
        {
            Reveal(element);
        }
    }

    private static void Element_MouseLeave(
        object sender,
        System.Windows.Input.MouseEventArgs e
    )
    {
        if (sender is FrameworkElement element)
        {
            HoverRevealAnimator.Reset(element);
        }
    }

    private static void Element_IsEnabledChanged(
        object sender,
        DependencyPropertyChangedEventArgs e
    )
    {
        if (sender is FrameworkElement element)
        {
            HoverRevealAnimator.Invalidate(element);
            RefreshInteractionHandlers(element);
            if ((bool)e.NewValue)
            {
                RestoreIfHovered(element);
            }
        }
    }

    private static void Element_PreviewMouseDown(
        object sender,
        System.Windows.Input.MouseButtonEventArgs e
    )
    {
        if (sender is FrameworkElement element)
        {
            HoverRevealAnimator.Reset(element);
        }
    }

    private static void Element_PreviewMouseUp(
        object sender,
        System.Windows.Input.MouseButtonEventArgs e
    )
    {
        if (sender is FrameworkElement element)
        {
            RestoreIfHovered(element);
        }
    }

    private static void Element_LostMouseCapture(
        object sender,
        System.Windows.Input.MouseEventArgs e
    )
    {
        if (sender is FrameworkElement element)
        {
            // A capture can be canceled without a matching mouse-up. Clear the
            // pressed reveal clock before restoring a legitimate hover state.
            HoverRevealAnimator.Reset(element);
            RestoreIfHovered(element);
        }
    }

    private static void Reveal(FrameworkElement element)
    {
        switch (GetRevealPolicy(element))
        {
            case RevealPolicy.Animated:
                HoverRevealAnimator.Begin(
                    element,
                    GetEffectiveAnimationDuration(element)
                );
                break;
            case RevealPolicy.BehaviorStaticFallback:
                HoverRevealAnimator.Show(element);
                break;
            case RevealPolicy.TemplateStaticFallback:
                break;
        }
    }

    private static void RestoreIfHovered(FrameworkElement element)
    {
        if (!element.IsEnabled || !element.IsMouseOver)
        {
            return;
        }

        if (GetRevealPolicy(element) is not RevealPolicy.TemplateStaticFallback)
        {
            HoverRevealAnimator.Show(element);
        }
    }

    private static RevealPolicy GetRevealPolicy(FrameworkElement element)
    {
        if (!HoverReveal.GetIsEnabled(element))
        {
            return DoesTemplateHandleInteraction(element)
                ? RevealPolicy.TemplateStaticFallback
                : RevealPolicy.BehaviorStaticFallback;
        }

        return RevealPolicy.Animated;
    }

    private static TimeSpan GetEffectiveAnimationDuration(FrameworkElement element)
    {
        var source = DependencyPropertyHelper.GetValueSource(
            element,
            HoverReveal.AnimationDurationProperty
        );
        if (source.BaseValueSource != BaseValueSource.Default)
        {
            return HoverReveal.GetAnimationDuration(element);
        }

        return element.TryFindResource("FlourishHoverRevealDuration") is TimeSpan duration
            ? duration
            : HoverReveal.GetAnimationDuration(element);
    }

    private static void RefreshInteractionHandlers(FrameworkElement element)
    {
        DisconnectInteractionHandlers(element);
        if (!element.IsLoaded || !HoverReveal.GetIsParticipant(element))
        {
            return;
        }

        if (GetRevealPolicy(element) is RevealPolicy.TemplateStaticFallback)
        {
            return;
        }

        element.IsEnabledChanged += Element_IsEnabledChanged;
        if (!element.IsEnabled)
        {
            return;
        }

        element.MouseEnter += Element_MouseEnter;
        element.MouseLeave += Element_MouseLeave;
        // Template-owned pressed visuals still need the animation layer cleared:
        // an active hover clock otherwise has precedence over an Opacity trigger.
        element.PreviewMouseDown += Element_PreviewMouseDown;
        element.PreviewMouseUp += Element_PreviewMouseUp;
        element.LostMouseCapture += Element_LostMouseCapture;
    }

    private static void DisconnectInteractionHandlers(FrameworkElement element)
    {
        element.MouseEnter -= Element_MouseEnter;
        element.MouseLeave -= Element_MouseLeave;
        element.IsEnabledChanged -= Element_IsEnabledChanged;
        element.PreviewMouseDown -= Element_PreviewMouseDown;
        element.PreviewMouseUp -= Element_PreviewMouseUp;
        element.LostMouseCapture -= Element_LostMouseCapture;
    }

    private static bool DoesTemplateHandleInteraction(FrameworkElement element)
    {
        if (!HoverReveal.GetTemplateHandlesInteraction(element))
        {
            return false;
        }

        if (element is not System.Windows.Controls.Control control)
        {
            return true;
        }

        var templateSource = DependencyPropertyHelper.GetValueSource(
            control,
            System.Windows.Controls.Control.TemplateProperty
        );
        if (templateSource.BaseValueSource != BaseValueSource.Local)
        {
            return true;
        }

        var contractSource = DependencyPropertyHelper.GetValueSource(
            element,
            HoverReveal.TemplateHandlesInteractionProperty
        );
        return contractSource.BaseValueSource == BaseValueSource.Local;
    }

    private enum RevealPolicy
    {
        Animated,
        TemplateStaticFallback,
        BehaviorStaticFallback,
    }
}
