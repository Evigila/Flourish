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
    NavigationPanelDirection Direction,
    IReadOnlyList<FrameworkElement>? CenteredContentHosts = null
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
        double[] currentCenteredWidths;
        double[] currentCenteredNetScales;
        double[] currentCenteredWorldOffsets;
        var isContinuation = false;
        if (active is { } existing && existing.Matches(target))
        {
            isContinuation = true;
            state = existing;
            currentVisibleWidth = existing.Clip.Rect.Width;
            currentScale = existing.ContentScale.ScaleX;
            currentTranslation = existing.ContentTranslation.X;
            currentCenteredWidths = existing.CenteredContentHosts
                .Select(host => host.GetVisibleWidth(currentScale))
                .ToArray();
            currentCenteredNetScales = existing.CenteredContentHosts
                .Select(host => host.GetNetScale(currentScale))
                .ToArray();
            currentCenteredWorldOffsets = existing.CenteredContentHosts
                .Select(host => host.GetWorldOffset(currentScale))
                .ToArray();
            StopClocks(existing);
        }
        else
        {
            Cancel();
            state = new TransitionState(target);
            currentVisibleWidth = committedWidth;
            currentScale = state.ContentScale.ScaleX;
            currentTranslation = state.ContentTranslation.X;
            currentCenteredWidths = state.CenteredContentHosts
                .Select(host => host.LayoutWidth)
                .ToArray();
            currentCenteredNetScales = state.CenteredContentHosts
                .Select(_ => 1d)
                .ToArray();
            currentCenteredWorldOffsets = state.CenteredContentHosts
                .Select(_ => 0d)
                .ToArray();
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

        var contentWidthDelta = committedWidth - targetWidth;
        var centeredRuns = new List<CenteredContentAnimationRun>(
            state.CenteredContentHosts.Count
        );
        for (var index = 0; index < state.CenteredContentHosts.Count; index++)
        {
            var centeredHost = state.CenteredContentHosts[index];
            var targetVisibleWidth = centeredHost.PredictTargetWidth(contentWidthDelta);
            if (
                centeredHost.IsAtMaximumWidth(currentCenteredWidths[index])
                && centeredHost.IsAtMaximumWidth(targetVisibleWidth)
                && (!isContinuation || centeredHost.IsUsingTransformCompensation)
            )
            {
                // Keep capped content at its natural layout width. The child counter-scale
                // cancels the content-area scale, while this offset corrects any difference
                // between the content-area center and the host's actual viewport center.
                var targetWorldOffset =
                    centeredHost.PredictTargetCenter(contentWidthDelta)
                    - (targetScale * centeredHost.CenterX);
                var scaleClockIndex = timeline.Children.Count;
                timeline.Children.Add(
                    new CenteredContentCompensationAnimation
                    {
                        CompensatedValueFrom = currentCenteredNetScales[index],
                        CompensatedValueTo = 1,
                        Duration = new Duration(effectiveDuration),
                        EasingFunction = easing,
                        FillBehavior = FillBehavior.Stop,
                        OuterScaleFrom = currentScale,
                        OuterScaleTo = targetScale,
                    }
                );
                var translationClockIndex = timeline.Children.Count;
                timeline.Children.Add(
                    new CenteredContentCompensationAnimation
                    {
                        CompensatedValueFrom = currentCenteredWorldOffsets[index],
                        CompensatedValueTo = targetWorldOffset,
                        Duration = new Duration(effectiveDuration),
                        EasingFunction = easing,
                        FillBehavior = FillBehavior.Stop,
                        OuterScaleFrom = currentScale,
                        OuterScaleTo = targetScale,
                    }
                );
                centeredRuns.Add(
                    CenteredContentAnimationRun.CreateTransform(
                        centeredHost,
                        scaleClockIndex,
                        translationClockIndex,
                        1 / targetScale,
                        targetWorldOffset / targetScale
                    )
                );
                continue;
            }

            var widthClockIndex = timeline.Children.Count;
            timeline.Children.Add(
                new CenteredContentCompensationAnimation
                {
                    CompensatedValueFrom = currentCenteredWidths[index],
                    CompensatedValueTo = targetVisibleWidth,
                    Duration = new Duration(effectiveDuration),
                    EasingFunction = easing,
                    FillBehavior = FillBehavior.Stop,
                    OuterScaleFrom = currentScale,
                    OuterScaleTo = targetScale,
                }
            );
            centeredRuns.Add(
                CenteredContentAnimationRun.CreateWidth(
                    centeredHost,
                    widthClockIndex
                )
            );
        }

        var clock = (ClockGroup)timeline.CreateClock(true);
        var runGeneration = ++generation;
        EventHandler completionHandler = (_, _) => Complete(state, runGeneration);
        state.Begin(clock, completionHandler, completed, runGeneration);
        active = state;

        try
        {
            Grid.SetColumn(target.PaneHost, 0);
            Grid.SetColumnSpan(target.PaneHost, 2);
            target.PaneHost.Width = presentationWidth;
            target.PaneHost.HorizontalAlignment =
                target.Direction == NavigationPanelDirection.Left
                    ? HorizontalAlignment.Left
                    : HorizontalAlignment.Right;
            state.Clip.Rect = toClip;
            target.PaneHost.Clip = state.Clip;
            state.ContentScale.ScaleX = targetScale;
            state.ContentTranslation.X = targetTranslation;
            target.ContentHost.RenderTransformOrigin = new System.Windows.Point();
            target.ContentHost.RenderTransform = state.ContentTransform;

            state.Clip.ApplyAnimationClock(
                RectangleGeometry.RectProperty,
                (AnimationClock)clock.Children[0],
                HandoffBehavior.SnapshotAndReplace
            );
            state.ContentScale.ApplyAnimationClock(
                ScaleTransform.ScaleXProperty,
                (AnimationClock)clock.Children[1],
                HandoffBehavior.SnapshotAndReplace
            );
            state.ContentTranslation.ApplyAnimationClock(
                TranslateTransform.XProperty,
                (AnimationClock)clock.Children[2],
                HandoffBehavior.SnapshotAndReplace
            );
            foreach (var run in centeredRuns)
            {
                run.Apply(clock);
            }
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
        state.ContentScale.ApplyAnimationClock(ScaleTransform.ScaleXProperty, null);
        state.ContentTranslation.ApplyAnimationClock(
            TranslateTransform.XProperty,
            null
        );
        foreach (var centeredHost in state.CenteredContentHosts)
        {
            centeredHost.Element.ApplyAnimationClock(
                FrameworkElement.MaxWidthProperty,
                null
            );
            centeredHost.CounterScale.ApplyAnimationClock(
                ScaleTransform.ScaleXProperty,
                null
            );
            centeredHost.CounterTranslation.ApplyAnimationClock(
                TranslateTransform.XProperty,
                null
            );
        }
        state.ClearRun();
    }

    private static void RestorePresentation(TransitionState state)
    {
        var target = state.Target;
        RestoreLocalValue(
            target.ContentHost,
            UIElement.RenderTransformProperty,
            state.OriginalContentTransformLocalValue
        );
        RestoreLocalValue(
            target.ContentHost,
            UIElement.RenderTransformOriginProperty,
            state.OriginalContentTransformOriginLocalValue
        );
        foreach (var centeredHost in state.CenteredContentHosts)
        {
            centeredHost.RestoreTransform();
        }
        target.PaneHost.Clip = state.OriginalClip;
        target.PaneHost.Width = state.OriginalWidth;
        target.PaneHost.HorizontalAlignment = state.OriginalHorizontalAlignment;
        Grid.SetColumn(target.PaneHost, state.OriginalColumn);
        Grid.SetColumnSpan(target.PaneHost, state.OriginalColumnSpan);
    }

    private static void RestoreLocalValue(
        DependencyObject target,
        DependencyProperty property,
        object localValue
    )
    {
        if (localValue == DependencyProperty.UnsetValue)
        {
            target.ClearValue(property);
            return;
        }

        target.SetValue(property, localValue);
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
            OriginalContentTransformLocalValue = target.ContentHost.ReadLocalValue(
                UIElement.RenderTransformProperty
            );
            OriginalContentTransformOriginLocalValue = target.ContentHost.ReadLocalValue(
                UIElement.RenderTransformOriginProperty
            );
            CenteredContentHostReferences = NormalizeCenteredContentHosts(
                target.CenteredContentHosts
            );
            CenteredContentHosts = CenteredContentHostReferences
                .Where(CanAnimateCenteredContentHost)
                .Select(host => new CenteredContentHostState(host, target.ContentHost))
                .ToArray();
            ContentTransform.Children.Add(ContentScale);
            ContentTransform.Children.Add(ContentTranslation);
        }

        internal NavigationPaneTransitionTarget Target { get; }

        internal RectangleGeometry Clip { get; } = new();

        internal TransformGroup ContentTransform { get; } = new();

        internal ScaleTransform ContentScale { get; } = new(1, 1);

        internal TranslateTransform ContentTranslation { get; } = new();

        internal IReadOnlyList<FrameworkElement> CenteredContentHostReferences { get; }

        internal IReadOnlyList<CenteredContentHostState> CenteredContentHosts { get; }

        internal Geometry? OriginalClip { get; }

        internal double OriginalWidth { get; }

        internal HorizontalAlignment OriginalHorizontalAlignment { get; }

        internal int OriginalColumn { get; }

        internal int OriginalColumnSpan { get; }

        internal object OriginalContentTransformLocalValue { get; }

        internal object OriginalContentTransformOriginLocalValue { get; }

        internal ClockGroup? Clock { get; private set; }

        internal EventHandler? CompletionHandler { get; private set; }

        internal Action? Completed { get; private set; }

        internal long Generation { get; private set; }

        internal bool Matches(NavigationPaneTransitionTarget target)
        {
            return ReferenceEquals(Target.WorkArea, target.WorkArea)
                && ReferenceEquals(Target.PaneHost, target.PaneHost)
                && ReferenceEquals(Target.ContentHost, target.ContentHost)
                && Target.Direction == target.Direction
                && HaveSameCenteredContentHosts(
                    CenteredContentHostReferences,
                    target.CenteredContentHosts
                );
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

    private sealed class CenteredContentHostState
    {
        internal CenteredContentHostState(
            FrameworkElement element,
            FrameworkElement contentHost
        )
        {
            Element = element;
            LayoutWidth = element.ActualWidth;
            MaximumWidth = element.MaxWidth;
            AvailableWidth = GetAvailableWidth(element, contentHost);
            CenterX = GetCenterX(element, contentHost);
            OriginalTransformLocalValue = element.ReadLocalValue(
                UIElement.RenderTransformProperty
            );
            OriginalTransformOriginLocalValue = element.ReadLocalValue(
                UIElement.RenderTransformOriginProperty
            );
            CompensationTransform.Children.Add(CounterScale);
            CompensationTransform.Children.Add(CounterTranslation);
        }

        internal FrameworkElement Element { get; }

        internal double LayoutWidth { get; }

        internal double MaximumWidth { get; }

        internal double AvailableWidth { get; }

        internal double CenterX { get; }

        internal ScaleTransform CounterScale { get; } = new(1, 1);

        internal TranslateTransform CounterTranslation { get; } = new();

        internal TransformGroup CompensationTransform { get; } = new();

        internal object OriginalTransformLocalValue { get; }

        internal object OriginalTransformOriginLocalValue { get; }

        internal bool IsUsingTransformCompensation { get; set; }

        internal double GetVisibleWidth(double outerScale)
        {
            return Element.ActualWidth * GetNetScale(outerScale);
        }

        internal double GetNetScale(double outerScale)
        {
            return outerScale
                * (IsUsingTransformCompensation ? CounterScale.ScaleX : 1);
        }

        internal double GetWorldOffset(double outerScale)
        {
            return IsUsingTransformCompensation
                ? outerScale * CounterTranslation.X
                : 0;
        }

        internal bool IsAtMaximumWidth(double width)
        {
            return Math.Abs(width - MaximumWidth) < MinimumDistance;
        }

        internal double PredictTargetCenter(double contentWidthDelta)
        {
            return CenterX + (contentWidthDelta / 2);
        }

        internal double PredictTargetWidth(double contentWidthDelta)
        {
            if (double.IsFinite(Element.Width))
            {
                return Math.Clamp(Element.Width, Element.MinWidth, MaximumWidth);
            }

            if (Element.HorizontalAlignment != HorizontalAlignment.Stretch)
            {
                return LayoutWidth;
            }

            var availableWidth = Math.Max(
                0,
                AvailableWidth
                    + contentWidthDelta
                    - Element.Margin.Left
                    - Element.Margin.Right
            );
            return Math.Clamp(availableWidth, Element.MinWidth, MaximumWidth);
        }

        internal void ApplyTransform(
            double targetCounterScale,
            double targetLocalOffset
        )
        {
            CounterScale.ScaleX = targetCounterScale;
            CounterTranslation.X = targetLocalOffset;
            Element.RenderTransformOrigin = new System.Windows.Point(0.5, 0);
            Element.RenderTransform = CompensationTransform;
            IsUsingTransformCompensation = true;
        }

        internal void RestoreTransform()
        {
            RestoreLocalValue(
                Element,
                UIElement.RenderTransformProperty,
                OriginalTransformLocalValue
            );
            RestoreLocalValue(
                Element,
                UIElement.RenderTransformOriginProperty,
                OriginalTransformOriginLocalValue
            );
            IsUsingTransformCompensation = false;
        }

        private static double GetAvailableWidth(
            FrameworkElement element,
            FrameworkElement contentHost
        )
        {
            for (
                DependencyObject? ancestor = VisualTreeHelper.GetParent(element);
                ancestor is not null && !ReferenceEquals(ancestor, contentHost);
                ancestor = VisualTreeHelper.GetParent(ancestor)
            )
            {
                if (
                    ancestor is ScrollViewer scrollViewer
                    && double.IsFinite(scrollViewer.ViewportWidth)
                    && scrollViewer.ViewportWidth > 0
                )
                {
                    return scrollViewer.ViewportWidth;
                }
            }

            return contentHost.ActualWidth;
        }

        private static double GetCenterX(
            FrameworkElement element,
            FrameworkElement contentHost
        )
        {
            return element
                .TransformToAncestor(contentHost)
                .Transform(new System.Windows.Point(element.ActualWidth / 2, 0))
                .X;
        }
    }

    private sealed class CenteredContentAnimationRun
    {
        private CenteredContentAnimationRun(
            CenteredContentHostState host,
            int widthClockIndex,
            int scaleClockIndex,
            int translationClockIndex,
            double targetCounterScale,
            double targetLocalOffset
        )
        {
            Host = host;
            WidthClockIndex = widthClockIndex;
            ScaleClockIndex = scaleClockIndex;
            TranslationClockIndex = translationClockIndex;
            TargetCounterScale = targetCounterScale;
            TargetLocalOffset = targetLocalOffset;
        }

        private CenteredContentHostState Host { get; }

        private int WidthClockIndex { get; }

        private int ScaleClockIndex { get; }

        private int TranslationClockIndex { get; }

        private double TargetCounterScale { get; }

        private double TargetLocalOffset { get; }

        internal static CenteredContentAnimationRun CreateWidth(
            CenteredContentHostState host,
            int clockIndex
        )
        {
            return new CenteredContentAnimationRun(host, clockIndex, -1, -1, 1, 0);
        }

        internal static CenteredContentAnimationRun CreateTransform(
            CenteredContentHostState host,
            int scaleClockIndex,
            int translationClockIndex,
            double targetCounterScale,
            double targetLocalOffset
        )
        {
            return new CenteredContentAnimationRun(
                host,
                -1,
                scaleClockIndex,
                translationClockIndex,
                targetCounterScale,
                targetLocalOffset
            );
        }

        internal void Apply(ClockGroup clock)
        {
            if (WidthClockIndex >= 0)
            {
                Host.RestoreTransform();
                Host.Element.ApplyAnimationClock(
                    FrameworkElement.MaxWidthProperty,
                    (AnimationClock)clock.Children[WidthClockIndex],
                    HandoffBehavior.SnapshotAndReplace
                );
                return;
            }

            Host.ApplyTransform(TargetCounterScale, TargetLocalOffset);
            Host.CounterScale.ApplyAnimationClock(
                ScaleTransform.ScaleXProperty,
                (AnimationClock)clock.Children[ScaleClockIndex],
                HandoffBehavior.SnapshotAndReplace
            );
            Host.CounterTranslation.ApplyAnimationClock(
                TranslateTransform.XProperty,
                (AnimationClock)clock.Children[TranslationClockIndex],
                HandoffBehavior.SnapshotAndReplace
            );
        }
    }

    /// <summary>
    /// Converts a world-space value into the local coordinate system of the animated content
    /// area. The same calculation drives visible width, net scale, and center offset.
    /// </summary>
    private sealed class CenteredContentCompensationAnimation : DoubleAnimationBase
    {
        internal static readonly DependencyProperty CompensatedValueFromProperty =
            DependencyProperty.Register(
                nameof(CompensatedValueFrom),
                typeof(double),
                typeof(CenteredContentCompensationAnimation)
            );

        internal static readonly DependencyProperty CompensatedValueToProperty =
            DependencyProperty.Register(
                nameof(CompensatedValueTo),
                typeof(double),
                typeof(CenteredContentCompensationAnimation)
            );

        internal static readonly DependencyProperty EasingFunctionProperty =
            DependencyProperty.Register(
                nameof(EasingFunction),
                typeof(IEasingFunction),
                typeof(CenteredContentCompensationAnimation)
            );

        internal static readonly DependencyProperty OuterScaleFromProperty =
            DependencyProperty.Register(
                nameof(OuterScaleFrom),
                typeof(double),
                typeof(CenteredContentCompensationAnimation)
            );

        internal static readonly DependencyProperty OuterScaleToProperty = DependencyProperty.Register(
            nameof(OuterScaleTo),
            typeof(double),
            typeof(CenteredContentCompensationAnimation)
        );

        internal double CompensatedValueFrom
        {
            get => (double)GetValue(CompensatedValueFromProperty);
            set => SetValue(CompensatedValueFromProperty, value);
        }

        internal double CompensatedValueTo
        {
            get => (double)GetValue(CompensatedValueToProperty);
            set => SetValue(CompensatedValueToProperty, value);
        }

        internal IEasingFunction? EasingFunction
        {
            get => (IEasingFunction?)GetValue(EasingFunctionProperty);
            set => SetValue(EasingFunctionProperty, value);
        }

        internal double OuterScaleFrom
        {
            get => (double)GetValue(OuterScaleFromProperty);
            set => SetValue(OuterScaleFromProperty, value);
        }

        internal double OuterScaleTo
        {
            get => (double)GetValue(OuterScaleToProperty);
            set => SetValue(OuterScaleToProperty, value);
        }

        protected override Freezable CreateInstanceCore()
        {
            return new CenteredContentCompensationAnimation();
        }

        protected override double GetCurrentValueCore(
            double defaultOriginValue,
            double defaultDestinationValue,
            AnimationClock animationClock
        )
        {
            var easedProgress = GetEasedProgress(animationClock, EasingFunction);
            var outerScale = Lerp(OuterScaleFrom, OuterScaleTo, easedProgress);
            var compensatedValue = Lerp(
                CompensatedValueFrom,
                CompensatedValueTo,
                easedProgress
            );
            return double.IsFinite(outerScale) && outerScale > 0
                ? compensatedValue / outerScale
                : defaultDestinationValue;
        }
    }

    private static bool CanAnimateCenteredContentHost(FrameworkElement host)
    {
        return double.IsFinite(host.ActualWidth)
            && host.ActualWidth > 0
            && double.IsFinite(host.MaxWidth)
            && host.MaxWidth > 0;
    }

    private static double GetEasedProgress(
        AnimationClock animationClock,
        IEasingFunction? easingFunction
    )
    {
        var progress = animationClock.CurrentProgress ?? 0;
        return easingFunction?.Ease(progress) ?? progress;
    }

    private static double Lerp(double from, double to, double progress)
    {
        return from + ((to - from) * progress);
    }

    private static IReadOnlyList<FrameworkElement> NormalizeCenteredContentHosts(
        IReadOnlyList<FrameworkElement>? hosts
    )
    {
        if (hosts is null || hosts.Count == 0)
        {
            return [];
        }

        var normalized = new List<FrameworkElement>(hosts.Count);
        foreach (var host in hosts)
        {
            ArgumentNullException.ThrowIfNull(host);
            if (!normalized.Any(existing => ReferenceEquals(existing, host)))
            {
                normalized.Add(host);
            }
        }

        return normalized;
    }

    private static bool HaveSameCenteredContentHosts(
        IReadOnlyList<FrameworkElement> existing,
        IReadOnlyList<FrameworkElement>? candidate
    )
    {
        var normalizedCandidate = NormalizeCenteredContentHosts(candidate);
        return existing.Count == normalizedCandidate.Count
            && existing
                .Zip(normalizedCandidate)
                .All(pair => ReferenceEquals(pair.First, pair.Second));
    }
}
