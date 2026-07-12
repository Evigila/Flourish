using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ArkheideSystem.Flourish.Abstract;
using HorizontalAlignment = System.Windows.HorizontalAlignment;

namespace ArkheideSystem.Flourish.Internal.Interaction;

internal readonly record struct NavigationPaneTransitionTarget(
    Grid WorkArea,
    FrameworkElement PaneHost,
    FrameworkElement ContentHost,
    ScaleTransform ContentScale,
    TranslateTransform ContentTranslation,
    NavigationPanelDirection Direction
);

/// <summary>
/// Animates the navigation pane exclusively through render properties.
/// </summary>
internal sealed class NavigationPaneTransitionController
{
    private const double MinimumDistance = 0.5;
    private const double MinimumDurationScale = 0.12;
    private TransitionState? active;
    private long generation;

    internal bool IsActive => active is not null;

    internal double? CurrentVisualWidth => active?.Clip.Rect.Width;

    internal ClockController? ActiveClockController => active?.Clock?.Controller;

    internal bool Start(
        NavigationPaneTransitionTarget target,
        double committedWidth,
        double targetWidth,
        double maximumPaneWidth,
        double referenceDistance,
        TimeSpan duration,
        IEasingFunction easing,
        Action completed
    )
    {
        ValidateTarget(target);
        ArgumentNullException.ThrowIfNull(easing);
        ArgumentNullException.ThrowIfNull(completed);

        if (
            !double.IsFinite(committedWidth)
            || !double.IsFinite(targetWidth)
            || !double.IsFinite(maximumPaneWidth)
            || duration <= TimeSpan.Zero
        )
        {
            Cancel();
            return false;
        }

        var workWidth = target.WorkArea.ActualWidth;
        var clipHeight = Math.Max(target.WorkArea.ActualHeight, target.PaneHost.ActualHeight);
        var committedContentWidth = target.ContentHost.ActualWidth;
        var targetContentWidth = committedContentWidth + committedWidth - targetWidth;
        if (
            !double.IsFinite(workWidth)
            || workWidth <= 0
            || clipHeight <= 0
            || committedContentWidth <= 0
            || targetContentWidth <= 0
        )
        {
            Cancel();
            return false;
        }

        TransitionState state;
        double currentVisibleWidth;
        double currentScale;
        double currentTranslation;
        if (active is { } existing && existing.Matches(target))
        {
            state = existing;
            currentVisibleWidth = existing.Clip.Rect.Width;
            currentScale = target.ContentScale.ScaleX;
            currentTranslation = target.ContentTranslation.X;
            StopClocks(existing);
        }
        else
        {
            Cancel();
            state = new TransitionState(target);
            currentVisibleWidth = committedWidth;
            currentScale = target.ContentScale.ScaleX;
            currentTranslation = target.ContentTranslation.X;
        }

        currentVisibleWidth = Math.Clamp(currentVisibleWidth, 0, workWidth);
        targetWidth = Math.Clamp(targetWidth, 0, workWidth);
        if (Math.Abs(currentVisibleWidth - targetWidth) < MinimumDistance)
        {
            active = state;
            Cancel();
            return false;
        }

        var presentationWidth = Math.Min(
            workWidth,
            Math.Max(maximumPaneWidth, Math.Max(currentVisibleWidth, targetWidth))
        );
        if (presentationWidth + MinimumDistance < Math.Max(currentVisibleWidth, targetWidth))
        {
            active = state;
            Cancel();
            return false;
        }

        var targetScale = targetContentWidth / committedContentWidth;
        var targetTranslation = target.Direction == NavigationPanelDirection.Left
            ? targetWidth - committedWidth
            : 0;
        if (!double.IsFinite(targetScale) || targetScale <= 0)
        {
            active = state;
            Cancel();
            return false;
        }

        var fromClip = CreateClipRect(
            target.Direction,
            presentationWidth,
            currentVisibleWidth,
            clipHeight
        );
        var toClip = CreateClipRect(
            target.Direction,
            presentationWidth,
            targetWidth,
            clipHeight
        );
        var effectiveDuration = ScaleDuration(
            duration,
            Math.Abs(targetWidth - currentVisibleWidth),
            referenceDistance
        );

        Grid.SetColumn(target.PaneHost, 0);
        Grid.SetColumnSpan(target.PaneHost, 2);
        target.PaneHost.Width = presentationWidth;
        target.PaneHost.HorizontalAlignment = target.Direction == NavigationPanelDirection.Left
            ? HorizontalAlignment.Left
            : HorizontalAlignment.Right;
        state.Clip.Rect = toClip;
        target.PaneHost.Clip = state.Clip;
        target.ContentScale.ScaleX = targetScale;
        target.ContentTranslation.X = targetTranslation;

        var timeline = new ParallelTimeline
        {
            Duration = new Duration(effectiveDuration),
            FillBehavior = FillBehavior.Stop,
        };
        timeline.Children.Add(
            new RectAnimation(fromClip, toClip, new Duration(effectiveDuration))
            {
                EasingFunction = easing,
                FillBehavior = FillBehavior.Stop,
            }
        );
        timeline.Children.Add(
            new DoubleAnimation(currentScale, targetScale, new Duration(effectiveDuration))
            {
                EasingFunction = easing,
                FillBehavior = FillBehavior.Stop,
            }
        );
        timeline.Children.Add(
            new DoubleAnimation(
                currentTranslation,
                targetTranslation,
                new Duration(effectiveDuration)
            )
            {
                EasingFunction = easing,
                FillBehavior = FillBehavior.Stop,
            }
        );

        var clock = (ClockGroup)timeline.CreateClock(true);
        var runGeneration = ++generation;
        EventHandler completionHandler = (_, _) => Complete(state, runGeneration);
        state.Begin(clock, completionHandler, completed, runGeneration);
        active = state;

        try
        {
            state.Clip.ApplyAnimationClock(
                RectangleGeometry.RectProperty,
                (AnimationClock)clock.Children[0],
                HandoffBehavior.SnapshotAndReplace
            );
            target.ContentScale.ApplyAnimationClock(
                ScaleTransform.ScaleXProperty,
                (AnimationClock)clock.Children[1],
                HandoffBehavior.SnapshotAndReplace
            );
            target.ContentTranslation.ApplyAnimationClock(
                TranslateTransform.XProperty,
                (AnimationClock)clock.Children[2],
                HandoffBehavior.SnapshotAndReplace
            );
        }
        catch
        {
            active = null;
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
        if (state.Clock is { } clock)
        {
            if (state.CompletionHandler is { } completionHandler)
            {
                clock.Completed -= completionHandler;
            }

            clock.Controller?.Remove();
        }

        state.Clip.ApplyAnimationClock(RectangleGeometry.RectProperty, null);
        state.Target.ContentScale.ApplyAnimationClock(ScaleTransform.ScaleXProperty, null);
        state.Target.ContentTranslation.ApplyAnimationClock(
            TranslateTransform.XProperty,
            null
        );
        state.ClearRun();
    }

    private static void RestorePresentation(TransitionState state)
    {
        var target = state.Target;
        target.ContentScale.ScaleX = state.OriginalScale;
        target.ContentTranslation.X = state.OriginalTranslation;
        target.PaneHost.Clip = state.OriginalClip;
        target.PaneHost.Width = state.OriginalWidth;
        target.PaneHost.HorizontalAlignment = state.OriginalHorizontalAlignment;
        Grid.SetColumn(target.PaneHost, state.OriginalColumn);
        Grid.SetColumnSpan(target.PaneHost, state.OriginalColumnSpan);
    }

    private static Rect CreateClipRect(
        NavigationPanelDirection direction,
        double presentationWidth,
        double visibleWidth,
        double height
    )
    {
        var x = direction == NavigationPanelDirection.Right
            ? presentationWidth - visibleWidth
            : 0;
        return new Rect(Math.Max(0, x), 0, visibleWidth, height);
    }

    private static TimeSpan ScaleDuration(
        TimeSpan duration,
        double distance,
        double referenceDistance
    )
    {
        if (!double.IsFinite(referenceDistance) || referenceDistance < MinimumDistance)
        {
            return duration;
        }

        var scale = Math.Clamp(
            distance / referenceDistance,
            MinimumDurationScale,
            1
        );
        return TimeSpan.FromTicks(Math.Max(1, (long)(duration.Ticks * scale)));
    }

    private static void ValidateTarget(NavigationPaneTransitionTarget target)
    {
        ArgumentNullException.ThrowIfNull(target.WorkArea);
        ArgumentNullException.ThrowIfNull(target.PaneHost);
        ArgumentNullException.ThrowIfNull(target.ContentHost);
        ArgumentNullException.ThrowIfNull(target.ContentScale);
        ArgumentNullException.ThrowIfNull(target.ContentTranslation);
    }

    private sealed class TransitionState
    {
        internal TransitionState(NavigationPaneTransitionTarget target)
        {
            Target = target;
            OriginalClip = target.PaneHost.Clip;
            OriginalWidth = target.PaneHost.Width;
            OriginalHorizontalAlignment = target.PaneHost.HorizontalAlignment;
            OriginalColumn = Grid.GetColumn(target.PaneHost);
            OriginalColumnSpan = Grid.GetColumnSpan(target.PaneHost);
            OriginalScale = target.ContentScale.ScaleX;
            OriginalTranslation = target.ContentTranslation.X;
        }

        internal NavigationPaneTransitionTarget Target { get; }

        internal RectangleGeometry Clip { get; } = new();

        internal Geometry? OriginalClip { get; }

        internal double OriginalWidth { get; }

        internal HorizontalAlignment OriginalHorizontalAlignment { get; }

        internal int OriginalColumn { get; }

        internal int OriginalColumnSpan { get; }

        internal double OriginalScale { get; }

        internal double OriginalTranslation { get; }

        internal ClockGroup? Clock { get; private set; }

        internal EventHandler? CompletionHandler { get; private set; }

        internal Action? Completed { get; private set; }

        internal long Generation { get; private set; }

        internal bool Matches(NavigationPaneTransitionTarget target)
        {
            return ReferenceEquals(Target.WorkArea, target.WorkArea)
                && ReferenceEquals(Target.PaneHost, target.PaneHost)
                && ReferenceEquals(Target.ContentHost, target.ContentHost)
                && ReferenceEquals(Target.ContentScale, target.ContentScale)
                && ReferenceEquals(
                    Target.ContentTranslation,
                    target.ContentTranslation
                )
                && Target.Direction == target.Direction;
        }

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
            Clock = null;
            CompletionHandler = null;
            Completed = null;
        }
    }
}
