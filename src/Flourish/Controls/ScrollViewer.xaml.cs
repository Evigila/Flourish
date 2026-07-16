using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WpfScrollViewer = System.Windows.Controls.ScrollViewer;

namespace ArkheideSystem.Flourish.Controls;

/// <summary>
/// Hosts scrollable content with a Flourish appearance and render-only smooth scrolling.
/// </summary>
[TemplatePart(Name = ScrollContentPresenterPartName, Type = typeof(ScrollContentPresenter))]
public class ScrollViewer : WpfScrollViewer
{
    private const string ScrollContentPresenterPartName = "PART_ScrollContentPresenter";
    private const double LogicalSyncIntervalSeconds = 1d / 24d;
    private const double OffsetTolerance = 0.1d;
    private const double ScrollResponse = 18d;
    private const double WheelLineHeight = 16d;
    private const double MinimumWheelVelocityFactor = 1.2d;
    private const double MaximumWheelVelocityFactor = 2.5d;

    private ScrollContentPresenter? _scrollContentPresenter;
    private TranslateTransform? _smoothScrollTransform;
    private bool _isRendering;
    private double _visualVerticalOffset;
    private double _targetVerticalOffset;
    private double _pendingLogicalOffset = double.NaN;
    private double _logicalSyncElapsed;
    private long _lastFrameTimestamp;
    private long _lastWheelTimestamp;

    /// <summary>Identifies the <see cref="IsCompact" /> dependency property.</summary>
    public static readonly DependencyProperty IsCompactProperty = DependencyProperty.Register(
        nameof(IsCompact),
        typeof(bool),
        typeof(ScrollViewer),
        new FrameworkPropertyMetadata(false)
    );

    /// <summary>
    /// Identifies the <see cref="IsSmoothScrollingEnabled" /> dependency property.
    /// </summary>
    public static readonly DependencyProperty IsSmoothScrollingEnabledProperty =
        DependencyProperty.Register(
            nameof(IsSmoothScrollingEnabled),
            typeof(bool),
            typeof(ScrollViewer),
            new FrameworkPropertyMetadata(true, OnIsSmoothScrollingEnabledChanged)
        );

    static ScrollViewer()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ScrollViewer),
            new FrameworkPropertyMetadata(typeof(ScrollViewer))
        );
    }

    /// <summary>Initializes a new instance of the <see cref="ScrollViewer" /> class.</summary>
    public ScrollViewer()
    {
        Unloaded += OnUnloaded;
    }

    /// <summary>Gets or sets whether the viewer uses compact scroll bars.</summary>
    public bool IsCompact
    {
        get => (bool)GetValue(IsCompactProperty);
        set => SetValue(IsCompactProperty, value);
    }

    /// <summary>
    /// Gets or sets whether mouse-wheel input uses render-only smooth scrolling.
    /// </summary>
    /// <remarks>
    /// Smooth scrolling is bypassed when <see cref="WpfScrollViewer.CanContentScroll" /> is
    /// enabled so item-based virtualizing panels retain their native logical scrolling.
    /// </remarks>
    public bool IsSmoothScrollingEnabled
    {
        get => (bool)GetValue(IsSmoothScrollingEnabledProperty);
        set => SetValue(IsSmoothScrollingEnabledProperty, value);
    }

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
        StopRendering(resetTransform: true);
        base.OnApplyTemplate();

        _scrollContentPresenter =
            GetTemplateChild(ScrollContentPresenterPartName) as ScrollContentPresenter;

        if (_scrollContentPresenter is null)
        {
            _smoothScrollTransform = null;
            return;
        }

        // Template Freezables can be frozen by WPF. Own one mutable transform per instance.
        _smoothScrollTransform = new TranslateTransform();
        _scrollContentPresenter.RenderTransform = _smoothScrollTransform;

        RebaseAnimationState();
    }

    /// <inheritdoc />
    protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
    {
        base.OnPreviewMouseWheel(e);

        if (
            e.Handled
            || !CanUseSmoothScrolling()
            || (Keyboard.Modifiers & ModifierKeys.Shift) != 0
        )
        {
            return;
        }

        if (!_isRendering)
        {
            RebaseAnimationState();
        }

        var wheelDistance = GetWheelDistance(e.Delta);
        var nextTarget = ClampVerticalOffset(_targetVerticalOffset - wheelDistance);

        // Leave an outward wheel event unhandled so an ancestor viewer can consume it.
        if (Math.Abs(nextTarget - _targetVerticalOffset) <= OffsetTolerance)
        {
            return;
        }

        _targetVerticalOffset = nextTarget;
        StartRendering();
        e.Handled = true;
    }

    /// <inheritdoc />
    protected override void OnScrollChanged(ScrollChangedEventArgs e)
    {
        base.OnScrollChanged(e);

        if (!_isRendering)
        {
            RebaseAnimationState();
            return;
        }

        _visualVerticalOffset = ClampVerticalOffset(_visualVerticalOffset);
        _targetVerticalOffset = ClampVerticalOffset(_targetVerticalOffset);

        if (Math.Abs(e.VerticalChange) <= OffsetTolerance)
        {
            ApplyVisualOffset();
            return;
        }

        if (
            !double.IsNaN(_pendingLogicalOffset)
            && Math.Abs(e.VerticalOffset - _pendingLogicalOffset) <= OffsetTolerance
        )
        {
            _pendingLogicalOffset = double.NaN;
            ApplyVisualOffset();
            return;
        }

        // Thumb dragging, keyboard navigation, and programmatic scrolling are authoritative.
        StopRendering(resetTransform: true);
        RebaseAnimationState();
    }

    private static void OnIsSmoothScrollingEnabledChanged(
        DependencyObject dependencyObject,
        DependencyPropertyChangedEventArgs eventArgs
    )
    {
        if (dependencyObject is ScrollViewer viewer && eventArgs.NewValue is false)
        {
            viewer.CompleteAnimation();
        }
    }

    private bool CanUseSmoothScrolling()
    {
        return IsSmoothScrollingEnabled
            && IsLoaded
            && !CanContentScroll
            && _scrollContentPresenter is not null
            && _smoothScrollTransform is not null
            && ScrollableHeight > OffsetTolerance;
    }

    private double GetWheelDistance(int delta)
    {
        var wheelLines = Math.Max(1, SystemParameters.WheelScrollLines);
        var isPreciseInput = Math.Abs(delta) < Mouse.MouseWheelDeltaForOneLine
            || delta % Mouse.MouseWheelDeltaForOneLine != 0;
        var velocityFactor = 1d;
        var now = Stopwatch.GetTimestamp();

        if (!isPreciseInput && _lastWheelTimestamp != 0)
        {
            var elapsedMilliseconds =
                (now - _lastWheelTimestamp) * 1000d / Stopwatch.Frequency;
            velocityFactor =
                (MaximumWheelVelocityFactor - MinimumWheelVelocityFactor)
                    * Math.Exp(-elapsedMilliseconds / 20d)
                + MinimumWheelVelocityFactor;
        }
        else if (!isPreciseInput)
        {
            velocityFactor = MinimumWheelVelocityFactor;
        }

        _lastWheelTimestamp = now;
        return delta
            / (double)Mouse.MouseWheelDeltaForOneLine
            * wheelLines
            * WheelLineHeight
            * velocityFactor;
    }

    private void StartRendering()
    {
        if (_isRendering)
        {
            return;
        }

        _isRendering = true;
        _logicalSyncElapsed = 0d;
        _lastFrameTimestamp = Stopwatch.GetTimestamp();
        CompositionTarget.Rendering += OnRendering;
    }

    private void OnRendering(object? sender, EventArgs eventArgs)
    {
        try
        {
            var now = Stopwatch.GetTimestamp();
            var elapsedSeconds = Math.Clamp(
                (now - _lastFrameTimestamp) / (double)Stopwatch.Frequency,
                0d,
                0.05d
            );
            _lastFrameTimestamp = now;
            _logicalSyncElapsed += elapsedSeconds;

            var interpolation = 1d - Math.Exp(-ScrollResponse * elapsedSeconds);
            _visualVerticalOffset +=
                (_targetVerticalOffset - _visualVerticalOffset) * interpolation;

            if (
                Math.Abs(_targetVerticalOffset - _visualVerticalOffset)
                <= OffsetTolerance
            )
            {
                _visualVerticalOffset = _targetVerticalOffset;
            }

            ApplyVisualOffset();

            if (
                _logicalSyncElapsed >= LogicalSyncIntervalSeconds
                || _visualVerticalOffset == _targetVerticalOffset
            )
            {
                SynchronizeLogicalOffset();
                _logicalSyncElapsed = 0d;
            }

            if (
                _visualVerticalOffset == _targetVerticalOffset
                && Math.Abs(VerticalOffset - _targetVerticalOffset) <= OffsetTolerance
            )
            {
                StopRendering(resetTransform: true);
                RebaseAnimationState();
            }
        }
        catch
        {
            // Rendering is a static event. Always release the control before propagating.
            StopRendering(resetTransform: false);
            throw;
        }
    }

    private void SynchronizeLogicalOffset()
    {
        var offset = ClampVerticalOffset(_visualVerticalOffset);

        if (Math.Abs(VerticalOffset - offset) <= OffsetTolerance)
        {
            _pendingLogicalOffset = double.NaN;
            return;
        }

        _pendingLogicalOffset = offset;
        ScrollToVerticalOffset(offset);
        ApplyVisualOffset();
    }

    private void ApplyVisualOffset()
    {
        if (_smoothScrollTransform is { IsFrozen: false })
        {
            _smoothScrollTransform.Y = VerticalOffset - _visualVerticalOffset;
        }
    }

    private void CompleteAnimation()
    {
        if (!_isRendering)
        {
            return;
        }

        ScrollToVerticalOffset(ClampVerticalOffset(_targetVerticalOffset));
        StopRendering(resetTransform: true);
        RebaseAnimationState();
    }

    private void StopRendering(bool resetTransform)
    {
        if (_isRendering)
        {
            CompositionTarget.Rendering -= OnRendering;
            _isRendering = false;
        }

        _pendingLogicalOffset = double.NaN;
        _logicalSyncElapsed = 0d;
        _lastFrameTimestamp = 0;

        if (resetTransform && _smoothScrollTransform is { IsFrozen: false })
        {
            _smoothScrollTransform.Y = 0d;
        }
    }

    private void RebaseAnimationState()
    {
        _visualVerticalOffset = VerticalOffset;
        _targetVerticalOffset = VerticalOffset;
    }

    private double ClampVerticalOffset(double offset)
    {
        return Math.Clamp(offset, 0d, Math.Max(0d, ScrollableHeight));
    }

    private void OnUnloaded(object sender, RoutedEventArgs eventArgs)
    {
        StopRendering(resetTransform: true);
        RebaseAnimationState();
    }
}
