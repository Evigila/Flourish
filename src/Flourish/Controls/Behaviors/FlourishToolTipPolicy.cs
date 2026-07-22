using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using WpfToolTip = System.Windows.Controls.ToolTip;

namespace ArkheideSystem.Flourish.Controls;

/// <summary>
/// Opts a Flourish control into the configured tooltip presentation policy.
/// </summary>
public static class FlourishToolTipPolicy
{
    /// <summary>Identifies whether Flourish tooltip presentation is enabled.</summary>
    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(FlourishToolTipPolicy),
            new FrameworkPropertyMetadata(false, OnIsEnabledChanged)
        );

    private static readonly DependencyPropertyDescriptor ToolTipDescriptor =
        DependencyPropertyDescriptor.FromProperty(
            FrameworkElement.ToolTipProperty,
            typeof(FrameworkElement)
        );

    private static readonly ConditionalWeakTable<FrameworkElement, ToolTipState> States =
        new();

    /// <summary>Gets whether Flourish tooltip presentation is enabled.</summary>
    public static bool GetIsEnabled(DependencyObject element)
    {
        return (bool)element.GetValue(IsEnabledProperty);
    }

    /// <summary>Sets whether Flourish tooltip presentation is enabled.</summary>
    public static void SetIsEnabled(DependencyObject element, bool value)
    {
        element.SetValue(IsEnabledProperty, value);
    }

    private static void OnIsEnabledChanged(
        DependencyObject dependencyObject,
        DependencyPropertyChangedEventArgs eventArgs
    )
    {
        if (dependencyObject is not FrameworkElement owner)
        {
            return;
        }

        if ((bool)eventArgs.NewValue)
        {
            var state = States.GetOrCreateValue(owner);
            ToolTipDescriptor.AddValueChanged(owner, Owner_ToolTipChanged);
            WrapCurrentValue(owner, state);
            return;
        }

        if (!States.TryGetValue(owner, out var existingState))
        {
            return;
        }

        ToolTipDescriptor.RemoveValueChanged(owner, Owner_ToolTipChanged);
        RestoreCurrentValue(owner, existingState);
        States.Remove(owner);
    }

    private static void Owner_ToolTipChanged(object? sender, EventArgs eventArgs)
    {
        if (
            sender is not FrameworkElement owner
            || !States.TryGetValue(owner, out var state)
            || state.IsUpdating
        )
        {
            return;
        }

        ReleaseWrapper(state);
        state.OriginalValue = null;
        WrapCurrentValue(owner, state);
    }

    private static void WrapCurrentValue(FrameworkElement owner, ToolTipState state)
    {
        var current = owner.ToolTip;
        if (current is null || current is WpfToolTip)
        {
            return;
        }

        var wrapper = new FlourishToolTip { Content = current };
        state.OriginalValue = current;
        state.Wrapper = wrapper;
        SetToolTip(owner, state, wrapper);
    }

    private static void RestoreCurrentValue(FrameworkElement owner, ToolTipState state)
    {
        if (ReferenceEquals(owner.ToolTip, state.Wrapper))
        {
            var originalValue = state.OriginalValue;
            ReleaseWrapper(state);
            SetToolTip(owner, state, originalValue);
            return;
        }

        ReleaseWrapper(state);
    }

    private static void ReleaseWrapper(ToolTipState state)
    {
        if (state.Wrapper is { } wrapper)
        {
            wrapper.Content = null;
            state.Wrapper = null;
        }
    }

    private static void SetToolTip(
        FrameworkElement owner,
        ToolTipState state,
        object? value
    )
    {
        state.IsUpdating = true;
        try
        {
            owner.SetCurrentValue(FrameworkElement.ToolTipProperty, value);
        }
        finally
        {
            state.IsUpdating = false;
        }
    }

    private sealed class ToolTipState
    {
        public bool IsUpdating { get; set; }

        public object? OriginalValue { get; set; }

        public FlourishToolTip? Wrapper { get; set; }
    }
}
