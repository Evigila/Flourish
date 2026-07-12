using System.Windows;
using ArkheideSystem.Flourish.Internal.Interaction;

namespace ArkheideSystem.Flourish.Controls;

/// <summary>
/// Provides the configurable hover-reveal behavior used by Flourish control templates.
/// </summary>
/// <remarks>
/// A participating control template supplies elements named <c>HoverChrome</c> and
/// <c>HoverRevealScale</c>. Templates without those elements safely ignore the behavior.
/// </remarks>
public static class HoverReveal
{
    /// <summary>
    /// Identifies the attached property that enables hover-reveal behavior.
    /// </summary>
    public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
        "IsEnabled",
        typeof(bool),
        typeof(HoverReveal),
        new FrameworkPropertyMetadata(
            true,
            FrameworkPropertyMetadataOptions.Inherits,
            OnIsEnabledChanged
        )
    );

    /// <summary>
    /// Identifies the attached property that opts a control template into hover reveal.
    /// </summary>
    public static readonly DependencyProperty IsParticipantProperty =
        DependencyProperty.RegisterAttached(
            "IsParticipant",
            typeof(bool),
            typeof(HoverReveal),
            new FrameworkPropertyMetadata(false, OnIsParticipantChanged)
        );

    /// <summary>
    /// Identifies the inherited attached property that controls reveal animation duration.
    /// </summary>
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

    /// <summary>
    /// Gets the hover-reveal animation duration inherited by an element.
    /// </summary>
    /// <param name="element">The element from which to read the value.</param>
    /// <returns>The configured animation duration.</returns>
    public static TimeSpan GetAnimationDuration(DependencyObject element)
    {
        return (TimeSpan)element.GetValue(AnimationDurationProperty);
    }

    /// <summary>
    /// Sets the hover-reveal animation duration inherited by an element and its descendants.
    /// </summary>
    /// <param name="element">The element on which to set the value.</param>
    /// <param name="value">The animation duration to use.</param>
    public static void SetAnimationDuration(DependencyObject element, TimeSpan value)
    {
        element.SetValue(AnimationDurationProperty, value);
    }

    /// <summary>
    /// Gets whether hover-reveal behavior is enabled for an element.
    /// </summary>
    /// <param name="element">The element from which to read the value.</param>
    /// <returns><see langword="true" /> when hover-reveal behavior is enabled.</returns>
    public static bool GetIsEnabled(DependencyObject element)
    {
        return (bool)element.GetValue(IsEnabledProperty);
    }

    /// <summary>
    /// Sets whether hover-reveal behavior is enabled for an element and its descendants.
    /// </summary>
    /// <param name="element">The element on which to set the value.</param>
    /// <param name="value"><see langword="true" /> to enable hover-reveal behavior.</param>
    public static void SetIsEnabled(DependencyObject element, bool value)
    {
        element.SetValue(IsEnabledProperty, value);
    }

    /// <summary>
    /// Gets whether a control template participates in hover reveal.
    /// </summary>
    public static bool GetIsParticipant(DependencyObject element)
    {
        return (bool)element.GetValue(IsParticipantProperty);
    }

    /// <summary>
    /// Sets whether a control template participates in hover reveal.
    /// </summary>
    public static void SetIsParticipant(DependencyObject element, bool value)
    {
        element.SetValue(IsParticipantProperty, value);
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

        if (GetIsParticipant(frameworkElement))
        {
            HoverRevealAnimator.Reset(frameworkElement);
        }
    }

    private static void OnIsParticipantChanged(
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
        element.PreviewMouseDown -= Element_PreviewMouseDown;
        element.PreviewMouseUp -= Element_PreviewMouseUp;

        element.Loaded += Element_Loaded;
        element.MouseEnter += Element_MouseEnter;
        element.MouseLeave += Element_MouseLeave;
        element.PreviewMouseDown += Element_PreviewMouseDown;
        element.PreviewMouseUp += Element_PreviewMouseUp;

        if (element.IsLoaded)
        {
            HoverRevealAnimator.Reset(element);
        }
    }

    private static void Detach(FrameworkElement element)
    {
        element.Loaded -= Element_Loaded;
        element.MouseEnter -= Element_MouseEnter;
        element.MouseLeave -= Element_MouseLeave;
        element.PreviewMouseDown -= Element_PreviewMouseDown;
        element.PreviewMouseUp -= Element_PreviewMouseUp;
        HoverRevealAnimator.Reset(element);
    }

    private static void Element_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            HoverRevealAnimator.Reset(element);
        }
    }

    private static void Element_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            Reveal(element);
        }
    }

    private static void Element_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            HoverRevealAnimator.Reset(element);
        }
    }

    private static void Element_PreviewMouseDown(
        object sender,
        System.Windows.Input.MouseButtonEventArgs e
    )
    {
        if (sender is FrameworkElement element)
        {
            // Animations have the highest dependency-property precedence. Clear the
            // reveal while pressed so the template's pressed-state triggers remain visible.
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
            RestoreAfterPointerRelease(element, element.IsMouseOver);
        }
    }

    internal static void RestoreAfterPointerRelease(
        FrameworkElement element,
        bool isMouseOver
    )
    {
        if (!isMouseOver)
        {
            return;
        }

        // MouseEnter owns the animated transition. Releasing a press while the pointer
        // remains inside should restore the already-hovered state without replaying it.
        HoverRevealAnimator.Show(element);
    }

    private static void Reveal(FrameworkElement element)
    {
        if (IsRevealAnimationEnabled(element))
        {
            HoverRevealAnimator.Begin(element, GetEffectiveAnimationDuration(element));
            return;
        }

        HoverRevealAnimator.Show(element);
    }

    private static bool IsRevealAnimationEnabled(FrameworkElement element)
    {
        var source = DependencyPropertyHelper.GetValueSource(element, IsEnabledProperty);
        if (source.BaseValueSource != BaseValueSource.Default)
        {
            return GetIsEnabled(element);
        }

        return element.TryFindResource("FlourishHoverRevealEnabled") is not bool enabled
            || enabled;
    }

    private static TimeSpan GetEffectiveAnimationDuration(FrameworkElement element)
    {
        var source = DependencyPropertyHelper.GetValueSource(
            element,
            AnimationDurationProperty
        );
        if (source.BaseValueSource != BaseValueSource.Default)
        {
            return GetAnimationDuration(element);
        }

        return element.TryFindResource("FlourishHoverRevealDuration") is TimeSpan duration
            ? duration
            : GetAnimationDuration(element);
    }
}
