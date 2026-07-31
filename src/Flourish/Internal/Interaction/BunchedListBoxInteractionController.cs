using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using ArkheideSystem.Flourish.Controls;
using FlourishScrollViewer = ArkheideSystem.Flourish.Controls.ScrollViewer;
using WpfMouseEventArgs = System.Windows.Input.MouseEventArgs;
using WpfMouseEventHandler = System.Windows.Input.MouseEventHandler;
using WpfPanel = System.Windows.Controls.Panel;
using WpfPoint = System.Windows.Point;
using WpfSelectionMode = System.Windows.Controls.SelectionMode;

namespace ArkheideSystem.Flourish.Internal.Interaction;

/// <summary>
/// Resolves item containers for a <see cref="BunchedListBox" /> and redirects one
/// parent-owned interaction surface between their realized bounds.
/// </summary>
internal sealed class BunchedListBoxInteractionController
{
    private const double BoundsTolerance = 0.1;
    private readonly BunchedListBox owner;
    private readonly BunchedIndicatorAnimator animator = new();
    private readonly List<Border> additionalSelectionChromes = [];
    private FrameworkElement? interactionViewport;
    private Canvas? indicatorLayer;
    private Border? selectionChrome;
    private Border? hoverChrome;
    private Border? pressedChrome;
    private FlourishScrollViewer? scrollViewer;
    private ScrollContentPresenter? scrollContentPresenter;
    private RectangleGeometry? viewportClip;
    private BunchedListBoxItem? hoverTarget;
    private Rect hoverTargetBounds = Rect.Empty;
    private BunchedListBoxItem? singleSelectionTarget;
    private Rect singleSelectionBounds = Rect.Empty;
    private DispatcherOperation? pendingRefresh;
    private DispatcherOperation? pendingHoverExit;
    private bool refreshPointer;
    private bool isPressed;
    private bool isLoaded;
    private bool templateEventsAttached;
    private bool generatorEventsAttached;
    private bool renderingAttached;

    internal BunchedListBoxInteractionController(BunchedListBox owner)
    {
        this.owner = owner ?? throw new ArgumentNullException(nameof(owner));
        owner.AddHandler(
            Mouse.PreviewMouseMoveEvent,
            new WpfMouseEventHandler(Owner_PreviewMouseMove),
            handledEventsToo: true
        );
        owner.AddHandler(
            Mouse.PreviewMouseDownEvent,
            new MouseButtonEventHandler(Owner_PreviewMouseDown),
            handledEventsToo: true
        );
        owner.AddHandler(
            Mouse.PreviewMouseUpEvent,
            new MouseButtonEventHandler(Owner_PreviewMouseUp),
            handledEventsToo: true
        );
        owner.AddHandler(
            Mouse.LostMouseCaptureEvent,
            new WpfMouseEventHandler(Owner_LostMouseCapture),
            handledEventsToo: true
        );
        owner.MouseLeave += Owner_MouseLeave;
        owner.IsEnabledChanged += Owner_IsEnabledChanged;
        owner.Loaded += Owner_Loaded;
        owner.Unloaded += Owner_Unloaded;
    }

    internal BunchedListBoxItem? HoverTarget => hoverTarget;

    internal int SelectionIndicatorCount =>
        owner.SelectionMode == WpfSelectionMode.Single
            ? selectionChrome?.Opacity > 0
                ? 1
                : 0
            : additionalSelectionChromes.Count(chrome => chrome.Opacity > 0);

    internal bool IsAttached => isLoaded && indicatorLayer is not null;

    internal Rect CurrentHoverBounds =>
        hoverChrome is null ? Rect.Empty : animator.GetCurrentBounds(hoverChrome);

    internal void ApplyTemplate(
        FrameworkElement? viewport,
        Canvas? layer,
        Border? selection,
        Border? hover,
        Border? pressed,
        FlourishScrollViewer? viewer
    )
    {
        interactionViewport = viewport;
        indicatorLayer = layer;
        selectionChrome = selection;
        hoverChrome = hover;
        pressedChrome = pressed;
        scrollViewer = viewer;
        scrollContentPresenter = null;
        viewportClip = layer is null ? null : new RectangleGeometry();
        if (layer is not null)
        {
            layer.Clip = viewportClip;
        }

        AttachTemplateEvents();
        ScheduleRefresh(rehitPointer: true);
    }

    internal void ClearTemplate()
    {
        CancelPendingOperations();
        DetachRendering();
        DetachTemplateEvents();
        ClearAdditionalSelectionChromes();
        if (selectionChrome is not null)
        {
            animator.Stop(selectionChrome, 0);
        }
        if (hoverChrome is not null)
        {
            animator.Stop(hoverChrome, 0);
        }
        if (pressedChrome is not null)
        {
            animator.Stop(pressedChrome, 0);
        }

        hoverTarget = null;
        hoverTargetBounds = Rect.Empty;
        singleSelectionTarget = null;
        singleSelectionBounds = Rect.Empty;
        isPressed = false;
        interactionViewport = null;
        indicatorLayer = null;
        selectionChrome = null;
        hoverChrome = null;
        pressedChrome = null;
        scrollViewer = null;
        scrollContentPresenter = null;
        viewportClip = null;
    }

    internal void ContainerPrepared(DependencyObject element)
    {
        if (element is BunchedListBoxItem)
        {
            ScheduleRefresh(rehitPointer: true);
        }
    }

    internal void ContainerClearing(DependencyObject element)
    {
        if (ReferenceEquals(element, hoverTarget))
        {
            HideHover(immediate: true);
        }
        if (ReferenceEquals(element, singleSelectionTarget))
        {
            singleSelectionTarget = null;
            singleSelectionBounds = Rect.Empty;
            if (selectionChrome is not null)
            {
                animator.Stop(selectionChrome, 0);
            }
        }

        ScheduleRefresh(rehitPointer: true);
    }

    internal void SelectionChanged()
    {
        ScheduleRefresh(rehitPointer: false);
    }

    internal void ItemsChanged()
    {
        ScheduleRefresh(rehitPointer: true);
    }

    internal void PolicyChanged()
    {
        if (!CanAnimate)
        {
            CompleteAnimationsAtCurrentTargets();
        }
        ScheduleRefresh(rehitPointer: true);
        UpdateRenderingSubscription();
    }

    private bool CanAnimate =>
        isLoaded
        && owner.IsEnabled
        && HoverReveal.GetIsEnabled(owner)
        && HoverReveal.GetIsMotionEnabled(owner)
        && GetAnimationDuration() > TimeSpan.Zero;

    private void Owner_Loaded(object sender, RoutedEventArgs e)
    {
        isLoaded = true;
        AttachGeneratorEvents();
        AttachTemplateEvents();
        owner.LayoutUpdated += Owner_LayoutUpdated;
        ScheduleRefresh(rehitPointer: true);
    }

    private void Owner_Unloaded(object sender, RoutedEventArgs e)
    {
        isLoaded = false;
        owner.LayoutUpdated -= Owner_LayoutUpdated;
        DetachGeneratorEvents();
        DetachTemplateEvents();
        DetachRendering();
        CancelPendingOperations();
        HideHover(immediate: true);
        ClearSelectionVisuals();
    }

    private void Owner_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is false)
        {
            HideHover(immediate: true);
            ClearSelectionVisuals();
        }
        else
        {
            ScheduleRefresh(rehitPointer: true);
        }
    }

    private void Owner_LayoutUpdated(object? sender, EventArgs e)
    {
        ScheduleRefresh(rehitPointer: false);
    }

    private void Owner_PreviewMouseMove(object sender, WpfMouseEventArgs e)
    {
        if (!owner.IsEnabled || !isLoaded)
        {
            return;
        }

        RetargetHover(ResolveTarget(e.OriginalSource as DependencyObject), deferExit: true);
    }

    private void Owner_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left)
        {
            return;
        }

        RetargetHover(ResolveTarget(e.OriginalSource as DependencyObject), deferExit: false);
        if (hoverTarget is null || hoverChrome is null || pressedChrome is null)
        {
            return;
        }

        isPressed = true;
        TransferVisibleBounds(hoverChrome, pressedChrome);
        animator.Stop(hoverChrome, 0);
        animator.Stop(pressedChrome, 1);
    }

    private void Owner_PreviewMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left)
        {
            return;
        }

        RestoreHoverAfterPress();
        RetargetHover(ResolveTarget(e.OriginalSource as DependencyObject), deferExit: true);
        ScheduleRefresh(rehitPointer: false);
    }

    private void Owner_LostMouseCapture(object sender, WpfMouseEventArgs e)
    {
        RestoreHoverAfterPress();
        ScheduleRefresh(rehitPointer: true);
    }

    private void Owner_MouseLeave(object sender, WpfMouseEventArgs e)
    {
        HideHover(immediate: false);
    }

    private void Generator_StatusChanged(object? sender, EventArgs e)
    {
        if (owner.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
        {
            ScheduleRefresh(rehitPointer: true);
        }
    }

    private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        ScheduleRefresh(rehitPointer: true);
        UpdateRenderingSubscription();
    }

    private void OnRendering(object? sender, EventArgs e)
    {
        if (
            !isLoaded
            || scrollViewer is null
            || scrollViewer.CanContentScroll
            || !HasVisibleIndicator
        )
        {
            UpdateRenderingSubscription();
            return;
        }

        UpdateViewportClip();
        RefreshHoverFromPointer();
        RefreshSelection();
    }

    private bool HasVisibleIndicator =>
        hoverTarget is not null
        || singleSelectionTarget is not null
        || additionalSelectionChromes.Any(chrome => chrome.Opacity > 0);

    private void RetargetHover(BunchedListBoxItem? target, bool deferExit)
    {
        CancelPendingHoverExit();
        if (target is null)
        {
            if (deferExit && owner.IsMouseOver)
            {
                pendingHoverExit = owner.Dispatcher.BeginInvoke(
                    DispatcherPriority.Render,
                    () =>
                    {
                        pendingHoverExit = null;
                        HideHover(immediate: false);
                    }
                );
            }
            else
            {
                HideHover(immediate: false);
            }
            return;
        }

        var bounds = TryGetContainerBounds(target);
        if (bounds is null)
        {
            HideHover(immediate: false);
            return;
        }

        var targetChanged = !ReferenceEquals(hoverTarget, target);
        hoverTarget = target;
        MoveHoverVisuals(bounds.Value, targetChanged);
        UpdateRenderingSubscription();
    }

    private void MoveHoverVisuals(Rect bounds, bool targetChanged)
    {
        if (hoverChrome is null || pressedChrome is null)
        {
            return;
        }

        var geometryChanged = !AreClose(hoverTargetBounds, bounds);
        hoverTargetBounds = bounds;
        if (targetChanged || geometryChanged)
        {
            var duration = GetAnimationDuration();
            animator.Move(hoverChrome, bounds, duration, CanAnimate);
            animator.Move(pressedChrome, bounds, duration, CanAnimate);
        }

        if (isPressed)
        {
            animator.Stop(hoverChrome, 0);
            animator.Show(pressedChrome, TimeSpan.Zero, animate: false);
        }
        else
        {
            animator.Stop(pressedChrome, 0);
            animator.Show(hoverChrome, GetAnimationDuration(), CanAnimate);
        }
    }

    private void HideHover(bool immediate)
    {
        CancelPendingHoverExit();
        hoverTarget = null;
        hoverTargetBounds = Rect.Empty;
        isPressed = false;
        if (hoverChrome is not null)
        {
            animator.Hide(hoverChrome, GetAnimationDuration(), CanAnimate && !immediate);
        }
        if (pressedChrome is not null)
        {
            animator.Hide(pressedChrome, GetAnimationDuration(), CanAnimate && !immediate);
        }
        UpdateRenderingSubscription();
    }

    private void RestoreHoverAfterPress()
    {
        if (!isPressed)
        {
            return;
        }

        isPressed = false;
        if (hoverChrome is null || pressedChrome is null)
        {
            return;
        }

        TransferVisibleBounds(pressedChrome, hoverChrome);
        animator.Stop(pressedChrome, 0);
        if (hoverTarget is not null)
        {
            animator.Stop(hoverChrome, 1);
            if (!hoverTargetBounds.IsEmpty)
            {
                animator.Move(hoverChrome, hoverTargetBounds, GetAnimationDuration(), CanAnimate);
            }
        }
    }

    private void TransferVisibleBounds(FrameworkElement source, FrameworkElement destination)
    {
        var current = animator.GetCurrentBounds(source);
        if (!current.IsEmpty && current.Width >= 0 && current.Height >= 0)
        {
            animator.SetBounds(destination, current);
        }
        if (!hoverTargetBounds.IsEmpty)
        {
            animator.Move(destination, hoverTargetBounds, GetAnimationDuration(), CanAnimate);
        }
    }

    private void RefreshHoverFromPointer()
    {
        if (!owner.IsMouseOver || !owner.IsEnabled)
        {
            HideHover(immediate: false);
            return;
        }

        var point = Mouse.GetPosition(owner);
        RetargetHover(
            ResolveTarget(owner.InputHitTest(point) as DependencyObject),
            deferExit: true
        );
    }

    private void RefreshHoverGeometry()
    {
        if (hoverTarget is null)
        {
            return;
        }

        var bounds = TryGetContainerBounds(hoverTarget);
        if (bounds is null)
        {
            HideHover(immediate: true);
            return;
        }

        MoveHoverVisuals(bounds.Value, targetChanged: false);
    }

    private BunchedListBoxItem? ResolveTarget(DependencyObject? source)
    {
        if (source is null)
        {
            return null;
        }

        var container = ItemsControl.ContainerFromElement(owner, source) as BunchedListBoxItem;
        return IsHoverTargetEligible(container) ? container : null;
    }

    private bool IsHoverTargetEligible(BunchedListBoxItem? container)
    {
        return container is not null
            && ReferenceEquals(ItemsControl.ItemsControlFromItemContainer(container), owner)
            && container.IsEnabled
            && container.IsVisible
            && container.IsHitTestVisible
            && container.IsItemVisible
            && !container.IsGroupHeader;
    }

    private void RefreshSelection()
    {
        if (!owner.IsEnabled || indicatorLayer is null || selectionChrome is null)
        {
            ClearSelectionVisuals();
            return;
        }

        if (owner.SelectionMode == WpfSelectionMode.Single)
        {
            HideAdditionalSelectionChromes();
            var target =
                owner.SelectedIndex < 0
                    ? null
                    : owner.ItemContainerGenerator.ContainerFromIndex(owner.SelectedIndex)
                        as BunchedListBoxItem;
            if (!IsSelectionTargetEligible(target))
            {
                singleSelectionTarget = null;
                singleSelectionBounds = Rect.Empty;
                animator.Hide(selectionChrome, GetAnimationDuration(), CanAnimate);
                return;
            }

            var bounds = TryGetContainerBounds(target!);
            if (bounds is null)
            {
                singleSelectionTarget = null;
                singleSelectionBounds = Rect.Empty;
                animator.Hide(selectionChrome, GetAnimationDuration(), CanAnimate);
                return;
            }

            var targetChanged = !ReferenceEquals(singleSelectionTarget, target);
            var geometryChanged = !AreClose(singleSelectionBounds, bounds.Value);
            singleSelectionTarget = target;
            singleSelectionBounds = bounds.Value;
            if (targetChanged || geometryChanged)
            {
                animator.Move(selectionChrome, bounds.Value, GetAnimationDuration(), CanAnimate);
            }
            animator.Show(selectionChrome, GetAnimationDuration(), CanAnimate);
            return;
        }

        singleSelectionTarget = null;
        singleSelectionBounds = Rect.Empty;
        animator.Stop(selectionChrome, 0);
        var realizedBounds = EnumerateVisualDescendants<BunchedListBoxItem>(owner)
            .Where(container => container.IsSelected)
            .Where(IsSelectionTargetEligible)
            .Select(container => TryGetContainerBounds(container!))
            .Where(bounds => bounds is not null)
            .Select(bounds => bounds!.Value)
            .ToArray();

        EnsureAdditionalSelectionChromeCount(realizedBounds.Length);
        for (var index = 0; index < additionalSelectionChromes.Count; index++)
        {
            var chrome = additionalSelectionChromes[index];
            if (index < realizedBounds.Length)
            {
                animator.SetBounds(chrome, realizedBounds[index]);
                animator.Stop(chrome, 1);
            }
            else
            {
                animator.Stop(chrome, 0);
            }
        }
    }

    private bool IsSelectionTargetEligible(BunchedListBoxItem? container)
    {
        return container is not null
            && ReferenceEquals(ItemsControl.ItemsControlFromItemContainer(container), owner)
            && container.IsVisible
            && container.IsItemVisible
            && !container.IsGroupHeader
            && !container.IsCommandItem;
    }

    private void EnsureAdditionalSelectionChromeCount(int count)
    {
        if (indicatorLayer is null)
        {
            return;
        }

        while (additionalSelectionChromes.Count < count)
        {
            var chrome = new Border
            {
                Width = 0,
                Height = 0,
                IsHitTestVisible = false,
                Opacity = 0,
            };
            chrome.SetResourceReference(
                Border.BackgroundProperty,
                "FlourishSelectionBackgroundBrush"
            );
            chrome.SetResourceReference(Border.CornerRadiusProperty, "FlourishControlCornerRadius");
            WpfPanel.SetZIndex(chrome, 0);
            indicatorLayer.Children.Add(chrome);
            additionalSelectionChromes.Add(chrome);
        }
    }

    private void ClearSelectionVisuals()
    {
        singleSelectionTarget = null;
        singleSelectionBounds = Rect.Empty;
        if (selectionChrome is not null)
        {
            animator.Stop(selectionChrome, 0);
        }
        HideAdditionalSelectionChromes();
    }

    private void HideAdditionalSelectionChromes()
    {
        foreach (var chrome in additionalSelectionChromes)
        {
            animator.Stop(chrome, 0);
        }
    }

    private void ClearAdditionalSelectionChromes()
    {
        if (indicatorLayer is not null)
        {
            foreach (var chrome in additionalSelectionChromes)
            {
                animator.Stop(chrome, 0);
                indicatorLayer.Children.Remove(chrome);
            }
        }
        additionalSelectionChromes.Clear();
    }

    private Rect? TryGetContainerBounds(BunchedListBoxItem container)
    {
        if (
            indicatorLayer is null
            || viewportClip is null
            || !container.IsArrangeValid
            || container.ActualWidth <= 0
            || container.ActualHeight <= 0
        )
        {
            return null;
        }

        try
        {
            var transform = container.TransformToVisual(indicatorLayer);
            var bounds = transform.TransformBounds(new Rect(new WpfPoint(), container.RenderSize));
            if (
                bounds.IsEmpty
                || bounds.Width <= 0
                || bounds.Height <= 0
                || !bounds.IntersectsWith(viewportClip.Rect)
            )
            {
                return null;
            }
            return bounds;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    private void UpdateViewportClip()
    {
        if (indicatorLayer is null || scrollViewer is null || viewportClip is null)
        {
            return;
        }

        scrollContentPresenter ??= FindVisualDescendant<ScrollContentPresenter>(
            scrollViewer,
            "PART_ScrollContentPresenter"
        );
        var source = (FrameworkElement?)scrollContentPresenter ?? scrollViewer;
        if (!source.IsArrangeValid || source.ActualWidth <= 0 || source.ActualHeight <= 0)
        {
            viewportClip.Rect = Rect.Empty;
            return;
        }

        try
        {
            var bounds = source
                .TransformToVisual(indicatorLayer)
                .TransformBounds(new Rect(new WpfPoint(), source.RenderSize));
            var layerBounds = new Rect(new WpfPoint(), indicatorLayer.RenderSize);
            bounds.Intersect(layerBounds);
            viewportClip.Rect = bounds;
        }
        catch (InvalidOperationException)
        {
            viewportClip.Rect = Rect.Empty;
        }
    }

    private void ScheduleRefresh(bool rehitPointer)
    {
        refreshPointer |= rehitPointer;
        if (!isLoaded || pendingRefresh is not null)
        {
            return;
        }

        pendingRefresh = owner.Dispatcher.BeginInvoke(
            DispatcherPriority.Render,
            () =>
            {
                pendingRefresh = null;
                var shouldRehit = refreshPointer;
                refreshPointer = false;
                UpdateViewportClip();
                if (shouldRehit)
                {
                    RefreshHoverFromPointer();
                }
                else
                {
                    RefreshHoverGeometry();
                }
                RefreshSelection();
                UpdateRenderingSubscription();
            }
        );
    }

    private void AttachTemplateEvents()
    {
        if (!isLoaded || templateEventsAttached || scrollViewer is null)
        {
            return;
        }

        scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
        templateEventsAttached = true;
    }

    private void DetachTemplateEvents()
    {
        if (!templateEventsAttached || scrollViewer is null)
        {
            templateEventsAttached = false;
            return;
        }

        scrollViewer.ScrollChanged -= ScrollViewer_ScrollChanged;
        templateEventsAttached = false;
    }

    private void AttachGeneratorEvents()
    {
        if (generatorEventsAttached)
        {
            return;
        }

        owner.ItemContainerGenerator.StatusChanged += Generator_StatusChanged;
        generatorEventsAttached = true;
    }

    private void DetachGeneratorEvents()
    {
        if (!generatorEventsAttached)
        {
            return;
        }

        owner.ItemContainerGenerator.StatusChanged -= Generator_StatusChanged;
        generatorEventsAttached = false;
    }

    private void UpdateRenderingSubscription()
    {
        var shouldAttach =
            isLoaded && scrollViewer is { CanContentScroll: false } && HasVisibleIndicator;
        if (shouldAttach && !renderingAttached)
        {
            CompositionTarget.Rendering += OnRendering;
            renderingAttached = true;
        }
        else if (!shouldAttach)
        {
            DetachRendering();
        }
    }

    private void DetachRendering()
    {
        if (!renderingAttached)
        {
            return;
        }

        CompositionTarget.Rendering -= OnRendering;
        renderingAttached = false;
    }

    private void CompleteAnimationsAtCurrentTargets()
    {
        if (hoverChrome is not null)
        {
            animator.Stop(hoverChrome, hoverTarget is not null && !isPressed ? 1 : 0);
            if (!hoverTargetBounds.IsEmpty)
            {
                animator.SetBounds(hoverChrome, hoverTargetBounds);
            }
        }
        if (pressedChrome is not null)
        {
            animator.Stop(pressedChrome, hoverTarget is not null && isPressed ? 1 : 0);
            if (!hoverTargetBounds.IsEmpty)
            {
                animator.SetBounds(pressedChrome, hoverTargetBounds);
            }
        }
        if (selectionChrome is not null)
        {
            animator.Stop(selectionChrome, singleSelectionTarget is null ? 0 : 1);
            if (!singleSelectionBounds.IsEmpty)
            {
                animator.SetBounds(selectionChrome, singleSelectionBounds);
            }
        }
    }

    private TimeSpan GetAnimationDuration()
    {
        var source = DependencyPropertyHelper.GetValueSource(
            owner,
            HoverReveal.AnimationDurationProperty
        );
        if (source.BaseValueSource != BaseValueSource.Default)
        {
            return HoverReveal.GetAnimationDuration(owner);
        }

        return owner.TryFindResource("FlourishHoverRevealDuration") is TimeSpan duration
            ? duration
            : HoverReveal.GetAnimationDuration(owner);
    }

    private void CancelPendingOperations()
    {
        if (pendingRefresh is { Status: DispatcherOperationStatus.Pending })
        {
            pendingRefresh.Abort();
        }
        pendingRefresh = null;
        refreshPointer = false;
        CancelPendingHoverExit();
    }

    private void CancelPendingHoverExit()
    {
        if (pendingHoverExit is { Status: DispatcherOperationStatus.Pending })
        {
            pendingHoverExit.Abort();
        }
        pendingHoverExit = null;
    }

    private static bool AreClose(Rect left, Rect right)
    {
        if (left.IsEmpty || right.IsEmpty)
        {
            return left.IsEmpty && right.IsEmpty;
        }

        return Math.Abs(left.X - right.X) < BoundsTolerance
            && Math.Abs(left.Y - right.Y) < BoundsTolerance
            && Math.Abs(left.Width - right.Width) < BoundsTolerance
            && Math.Abs(left.Height - right.Height) < BoundsTolerance;
    }

    private static T? FindVisualDescendant<T>(DependencyObject root, string name)
        where T : FrameworkElement
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is T match && match.Name == name)
            {
                return match;
            }

            var descendant = FindVisualDescendant<T>(child, name);
            if (descendant is not null)
            {
                return descendant;
            }
        }

        return null;
    }

    private static IEnumerable<T> EnumerateVisualDescendants<T>(DependencyObject root)
        where T : DependencyObject
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is T match)
            {
                yield return match;
            }

            foreach (var descendant in EnumerateVisualDescendants<T>(child))
            {
                yield return descendant;
            }
        }
    }
}
