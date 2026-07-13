using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ArkheideSystem.Flourish.Abstract;

namespace ArkheideSystem.Flourish.Internal.Interaction;

internal readonly record struct PageTransitionTarget(
    FrameworkElement ContentHost,
    FrameworkElement Chrome,
    ScaleTransform ChromeScale
);

/// <summary>
/// Reveals page content by animating a text-free chrome while leaving the content visual unchanged.
/// </summary>
internal sealed class PageTransitionController
{
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
        target.Chrome.BeginAnimation(UIElement.OpacityProperty, null);
        target.ChromeScale.BeginAnimation(ScaleTransform.ScaleYProperty, null);
        target.Chrome.Opacity = 1;
        target.ChromeScale.ScaleY = 1;

        var timeline = new DoubleAnimation(1, 0, new Duration(duration))
        {
            EasingFunction = easing,
            FillBehavior = FillBehavior.Stop,
        };
        var clock = (AnimationClock)timeline.CreateClock(true);
        var runGeneration = ++generation;
        EventHandler completionHandler = (_, _) => Complete(state, runGeneration);
        state.Begin(clock, completionHandler, completed, runGeneration);
        active = state;

        try
        {
            if (transition == FlourishPageTransition.Fade)
            {
                target.Chrome.ApplyAnimationClock(
                    UIElement.OpacityProperty,
                    clock,
                    HandoffBehavior.SnapshotAndReplace
                );
            }
            else
            {
                target.ChromeScale.ApplyAnimationClock(
                    ScaleTransform.ScaleYProperty,
                    clock,
                    HandoffBehavior.SnapshotAndReplace
                );
            }
        }
        catch
        {
            active = null;
            StopClock(state);
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
        StopClock(state);
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
        StopClock(state);
        RestorePresentation(state);
        completed?.Invoke();
    }

    private static void StopClock(TransitionState state)
    {
        if (state.CompletionHandler is { } completionHandler)
        {
            state.Clock.Completed -= completionHandler;
        }

        state.Clock.Controller?.Remove();
        if (state.Transition == FlourishPageTransition.Fade)
        {
            state.Target.Chrome.ApplyAnimationClock(UIElement.OpacityProperty, null);
        }
        else
        {
            state.Target.ChromeScale.ApplyAnimationClock(
                ScaleTransform.ScaleYProperty,
                null
            );
        }

        state.ClearRun();
    }

    private static void RestorePresentation(TransitionState state)
    {
        state.Target.Chrome.Opacity = state.OriginalOpacity;
        state.Target.ChromeScale.ScaleY = state.OriginalScaleY;
    }

    private static void ValidateTarget(PageTransitionTarget target)
    {
        ArgumentNullException.ThrowIfNull(target.ContentHost);
        ArgumentNullException.ThrowIfNull(target.Chrome);
        ArgumentNullException.ThrowIfNull(target.ChromeScale);
        if (ReferenceEquals(target.ContentHost, target.Chrome))
        {
            throw new ArgumentException(
                "The page content and transition chrome must be different elements.",
                nameof(target)
            );
        }
    }

    private sealed class TransitionState(
        PageTransitionTarget target,
        FlourishPageTransition transition
    )
    {
        internal PageTransitionTarget Target { get; } = target;

        internal FlourishPageTransition Transition { get; } = transition;

        internal double OriginalOpacity { get; } = target.Chrome.Opacity;

        internal double OriginalScaleY { get; } = target.ChromeScale.ScaleY;

        internal AnimationClock Clock { get; private set; } = null!;

        internal EventHandler? CompletionHandler { get; private set; }

        internal Action? Completed { get; private set; }

        internal long Generation { get; private set; }

        internal void Begin(
            AnimationClock clock,
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
