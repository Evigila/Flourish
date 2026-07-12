using System.Windows;
using ArkheideSystem.Flourish.Internal.Interaction;

namespace ArkheideSystem.Flourish.Controls;

/// <summary>
/// Provides the configurable hover-reveal behavior used by Flourish control templates.
/// </summary>
/// <remarks>
/// A participating control template supplies elements named <c>HoverChrome</c> and
/// <c>HoverRevealScale</c>. Templates without those elements safely ignore the behavior.
/// Templates that also own their static hover and pressed visuals can set
/// <see cref="TemplateHandlesInteractionProperty" /> to avoid redundant pointer work.
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
    /// Identifies whether a participating template owns its static hover and pressed states.
    /// </summary>
    public static readonly DependencyProperty TemplateHandlesInteractionProperty =
        DependencyProperty.RegisterAttached(
            "TemplateHandlesInteraction",
            typeof(bool),
            typeof(HoverReveal),
            new FrameworkPropertyMetadata(false, OnTemplateHandlesInteractionChanged)
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

    /// <summary>
    /// Gets whether a participating template renders its own static hover and pressed states.
    /// </summary>
    public static bool GetTemplateHandlesInteraction(DependencyObject element)
    {
        return (bool)element.GetValue(TemplateHandlesInteractionProperty);
    }

    /// <summary>
    /// Sets whether a participating template renders its own static hover and pressed states.
    /// </summary>
    /// <remarks>
    /// A template that enables this option must provide a non-animated mouse-over fallback
    /// for <see cref="IsEnabledProperty" /> set to <see langword="false" />.
    /// A locally assigned <see cref="System.Windows.Controls.Control.Template" /> falls
    /// back to behavior-managed interaction unless this property is also assigned locally.
    /// </remarks>
    public static void SetTemplateHandlesInteraction(
        DependencyObject element,
        bool value
    )
    {
        var previousValue = GetTemplateHandlesInteraction(element);
        var previousSource = DependencyPropertyHelper.GetValueSource(
            element,
            TemplateHandlesInteractionProperty
        );
        element.SetValue(TemplateHandlesInteractionProperty, value);
        if (
            previousValue == value
            && previousSource.BaseValueSource != BaseValueSource.Local
            && element is FrameworkElement frameworkElement
        )
        {
            HoverRevealInteraction.Refresh(frameworkElement);
        }
    }

    internal static void NotifyTemplateApplied(FrameworkElement element)
    {
        HoverRevealInteraction.NotifyTemplateApplied(element);
    }

    private static void OnIsEnabledChanged(
        DependencyObject element,
        DependencyPropertyChangedEventArgs e
    )
    {
        if (element is FrameworkElement frameworkElement)
        {
            HoverRevealInteraction.Refresh(frameworkElement);
        }
    }

    private static void OnIsParticipantChanged(
        DependencyObject element,
        DependencyPropertyChangedEventArgs e
    )
    {
        if (element is FrameworkElement frameworkElement)
        {
            HoverRevealInteraction.SetParticipant(
                frameworkElement,
                (bool)e.NewValue
            );
        }
    }

    private static void OnTemplateHandlesInteractionChanged(
        DependencyObject element,
        DependencyPropertyChangedEventArgs e
    )
    {
        if (element is FrameworkElement frameworkElement)
        {
            HoverRevealInteraction.Refresh(frameworkElement);
        }
    }
}
