using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ArkheideSystem.Flourish.Abstract;

namespace ArkheideSystem.Flourish.Internal.Interaction;

internal readonly record struct PageTransitionTarget(FrameworkElement Presenter);

/// <summary>
/// Animates a short-lived bitmap cache of the page presenter so its visual tree does not need to
/// be rasterized again for every transition frame.
/// </summary>
internal sealed class PageTransitionController
{
    private const double PageEntranceOffset = 14;
    private TransitionState? active;
    private long generation;

    internal bool IsActive => active is not null;

    internal ClockController? ActiveClockController => active?.Clock.Controller;

    internal bool Start(
        PageTransitionTarget target,
        FlourishPageTransition transition,
        TimeSpan duration,
        IEasingFunction easing,
        Action completed
    )
    {
        ValidateTarget(target);
        ArgumentNullException.ThrowIfNull(easing);
        ArgumentNullException.ThrowIfNull(completed);

        if (
            transition is not (
                FlourishPageTransition.Fade
                or FlourishPageTransition.EntranceFromBottom
            )
            || duration <= TimeSpan.Zero
        )
        {
            Cancel();
            return false;
        }

        Cancel();
        var state = new TransitionState(target, transition);
        var timeline = new ParallelTimeline
        {
            Duration = new Duration(duration),
            FillBehavior = FillBehavior.Stop,
        };
        timeline.Children.Add(
            new DoubleAnimation(0, state.OriginalOpacity, new Duration(duration))
            {
                EasingFunction = easing,
                FillBehavior = FillBehavior.Stop,
            }
        );
        if (transition == FlourishPageTransition.EntranceFromBottom)
        {
            timeline.Children.Add(
                new DoubleAnimation(PageEntranceOffset, 0, new Duration(duration))
                {
                    EasingFunction = easing,
                    FillBehavior = FillBehavior.Stop,
                }
            );
        }

        var clock = (ClockGroup)timeline.CreateClock(true);
        var runGeneration = ++generation;
        EventHandler completionHandler = (_, _) => Complete(state, runGeneration);
        state.Begin(clock, completionHandler, completed, runGeneration);
        active = state;

        try
        {
            target.Presenter.SetCurrentValue(
                UIElement.CacheModeProperty,
                state.TransitionCache
            );
            if (transition == FlourishPageTransition.EntranceFromBottom)
            {
                target.Presenter.SetCurrentValue(
                    UIElement.RenderTransformProperty,
                    state.Translation
                );
            }

            target.Presenter.ApplyAnimationClock(
                UIElement.OpacityProperty,
                (AnimationClock)clock.Children[0],
                HandoffBehavior.SnapshotAndReplace
            );
            if (transition == FlourishPageTransition.EntranceFromBottom)
            {
                state.Translation.ApplyAnimationClock(
                    TranslateTransform.YProperty,
                    (AnimationClock)clock.Children[1],
                    HandoffBehavior.SnapshotAndReplace
                );
            }
        }
        catch
        {
            active = null;
            generation++;
            StopClocks(state);
            RestorePresentation(state);
            throw;
        }

        return true;
    }

    internal void Cancel()
    {
        if (active is not { } state)
        {
            return;
        }

        active = null;
        generation++;
        StopClocks(state);
        RestorePresentation(state);
    }

    private void Complete(TransitionState state, long runGeneration)
    {
        if (
            !ReferenceEquals(active, state)
            || state.Generation != runGeneration
            || generation != runGeneration
        )
        {
            return;
        }

        var completed = state.Completed;
        active = null;
        generation++;
        StopClocks(state);
        RestorePresentation(state);
        completed?.Invoke();
    }

    private static void StopClocks(TransitionState state)
    {
        if (state.CompletionHandler is { } completionHandler)
        {
            state.Clock.Completed -= completionHandler;
        }

        state.Clock.Controller?.Remove();
        try
        {
            state.Target.Presenter.ApplyAnimationClock(UIElement.OpacityProperty, null);
            if (
                state.Transition == FlourishPageTransition.EntranceFromBottom
            )
            {
                state.Translation.ApplyAnimationClock(
                    TranslateTransform.YProperty,
                    null
                );
            }
        }
        finally
        {
            state.ClearRun();
        }
    }

    private static void RestorePresentation(TransitionState state)
    {
        var target = state.Target;
        target.Presenter.SetCurrentValue(UIElement.OpacityProperty, state.OriginalOpacity);
        RestoreLocalValue(
            target.Presenter,
            UIElement.OpacityProperty,
            state.OriginalOpacityLocalValue
        );
        if (state.Transition == FlourishPageTransition.EntranceFromBottom)
        {
            target.Presenter.SetCurrentValue(
                UIElement.RenderTransformProperty,
                state.OriginalRenderTransform
            );
            RestoreLocalValue(
                target.Presenter,
                UIElement.RenderTransformProperty,
                state.OriginalRenderTransformLocalValue
            );
        }

        target.Presenter.SetCurrentValue(
            UIElement.CacheModeProperty,
            state.OriginalCacheMode
        );
        RestoreLocalValue(
            target.Presenter,
            UIElement.CacheModeProperty,
            state.OriginalCacheModeLocalValue
        );
    }

    private static void ValidateTarget(PageTransitionTarget target)
    {
        ArgumentNullException.ThrowIfNull(target.Presenter);
    }

    private static void RestoreLocalValue(
        DependencyObject target,
        DependencyProperty property,
        object originalLocalValue
    )
    {
        if (ReferenceEquals(target.ReadLocalValue(property), originalLocalValue))
        {
            return;
        }

        if (originalLocalValue == DependencyProperty.UnsetValue)
        {
            target.ClearValue(property);
            return;
        }

        target.SetValue(property, originalLocalValue);
    }

    private sealed class TransitionState(
        PageTransitionTarget target,
        FlourishPageTransition transition
    )
    {
        internal PageTransitionTarget Target { get; } = target;

        internal FlourishPageTransition Transition { get; } = transition;

        internal double OriginalOpacity { get; } = target.Presenter.Opacity;

        internal object OriginalOpacityLocalValue { get; } =
            target.Presenter.ReadLocalValue(UIElement.OpacityProperty);

        internal CacheMode? OriginalCacheMode { get; } = target.Presenter.CacheMode;

        internal object OriginalCacheModeLocalValue { get; } =
            target.Presenter.ReadLocalValue(UIElement.CacheModeProperty);

        internal Transform OriginalRenderTransform { get; } =
            target.Presenter.RenderTransform;

        internal object OriginalRenderTransformLocalValue { get; } =
            target.Presenter.ReadLocalValue(UIElement.RenderTransformProperty);

        internal TranslateTransform Translation { get; } = new();

        internal BitmapCache TransitionCache { get; } = new()
        {
            SnapsToDevicePixels = false,
        };

        internal ClockGroup Clock { get; private set; } = null!;

        internal EventHandler? CompletionHandler { get; private set; }

        internal Action? Completed { get; private set; }

        internal long Generation { get; private set; }

        internal void Begin(
            ClockGroup clock,
            EventHandler completionHandler,
            Action completed,
            long generation
        )
        {
            Clock = clock;
            CompletionHandler = completionHandler;
            Completed = completed;
            Generation = generation;
            clock.Completed += completionHandler;
        }

        internal void ClearRun()
        {
            CompletionHandler = null;
            Completed = null;
        }
    }
}
