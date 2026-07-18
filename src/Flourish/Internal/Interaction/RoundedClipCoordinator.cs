using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

namespace ArkheideSystem.Flourish.Internal.Interaction;

/// <summary>
/// Keeps a rounded clip synchronized with a template host while coalescing layout churn.
/// </summary>
internal sealed class RoundedClipCoordinator
{
    private static readonly DependencyPropertyDescriptor CornerRadiusDescriptor =
        DependencyPropertyDescriptor.FromProperty(
            Border.CornerRadiusProperty,
            typeof(Border)
        );

    private FrameworkElement? clipHost;
    private Border? cornerSource;
    private DispatcherOperation? pendingUpdate;
    private Size lastSize = new(double.NaN, double.NaN);
    private CornerRadius lastCornerRadius;
    private bool hasAppliedClip;
    private bool isActive;
    private bool isCornerListenerAttached;

    internal int GeometryBuildCount { get; private set; }

    internal void Attach(FrameworkElement? host, Border? source)
    {
        Detach();
        if (host is null || source is null)
        {
            return;
        }

        clipHost = host;
        cornerSource = source;
        clipHost.SizeChanged += ClipHost_SizeChanged;
        clipHost.Loaded += ClipHost_Loaded;
        clipHost.Unloaded += ClipHost_Unloaded;
        isActive = clipHost.IsLoaded;
        if (isActive)
        {
            AttachCornerListener();
        }

        // Preserve the existing no-delay behavior when a template is first applied.
        ApplyClip();
    }

    internal void Detach()
    {
        if (clipHost is not null)
        {
            clipHost.SizeChanged -= ClipHost_SizeChanged;
            clipHost.Loaded -= ClipHost_Loaded;
            clipHost.Unloaded -= ClipHost_Unloaded;
        }

        DetachCornerListener();

        AbortPendingUpdate();
        clipHost = null;
        cornerSource = null;
        lastSize = new Size(double.NaN, double.NaN);
        lastCornerRadius = default;
        hasAppliedClip = false;
        isActive = false;
    }

    private void ClipHost_SizeChanged(object sender, SizeChangedEventArgs eventArgs)
    {
        if (
            (!double.IsFinite(lastSize.Width) || lastSize.Width <= 0 || lastSize.Height <= 0)
            && eventArgs.NewSize.Width > 0
            && eventArgs.NewSize.Height > 0
        )
        {
            ApplyClip();
            return;
        }

        ScheduleUpdate();
    }

    private void CornerSource_CornerRadiusChanged(object? sender, EventArgs eventArgs)
    {
        ScheduleUpdate();
    }

    private void ClipHost_Loaded(object sender, RoutedEventArgs eventArgs)
    {
        isActive = true;
        AttachCornerListener();
        ApplyClip();
    }

    private void ClipHost_Unloaded(object sender, RoutedEventArgs eventArgs)
    {
        isActive = false;
        DetachCornerListener();
        AbortPendingUpdate();
    }

    private void ScheduleUpdate()
    {
        if (
            !isActive
            || clipHost is null
            || pendingUpdate?.Status == DispatcherOperationStatus.Pending
        )
        {
            return;
        }

        pendingUpdate = clipHost.Dispatcher.BeginInvoke(
            DispatcherPriority.Render,
            new Action(ApplyScheduledClip)
        );
    }

    private void ApplyScheduledClip()
    {
        pendingUpdate = null;
        ApplyClip();
    }

    private void ApplyClip()
    {
        if (clipHost is null || cornerSource is null)
        {
            return;
        }

        var size = clipHost.RenderSize;
        var cornerRadius = cornerSource.CornerRadius;
        if (hasAppliedClip && size == lastSize && cornerRadius == lastCornerRadius)
        {
            return;
        }

        clipHost.Clip = RoundedClipGeometry.Create(size, cornerRadius);
        lastSize = size;
        lastCornerRadius = cornerRadius;
        hasAppliedClip = true;
        GeometryBuildCount++;
    }

    private void AttachCornerListener()
    {
        if (cornerSource is null || isCornerListenerAttached)
        {
            return;
        }

        CornerRadiusDescriptor.AddValueChanged(cornerSource, CornerSource_CornerRadiusChanged);
        isCornerListenerAttached = true;
    }

    private void DetachCornerListener()
    {
        if (cornerSource is null || !isCornerListenerAttached)
        {
            return;
        }

        CornerRadiusDescriptor.RemoveValueChanged(
            cornerSource,
            CornerSource_CornerRadiusChanged
        );
        isCornerListenerAttached = false;
    }

    private void AbortPendingUpdate()
    {
        if (pendingUpdate?.Status == DispatcherOperationStatus.Pending)
        {
            pendingUpdate.Abort();
        }

        pendingUpdate = null;
    }
}

internal static class RoundedClipGeometry
{
    internal static Geometry Create(Size size, CornerRadius cornerRadius)
    {
        var width = Math.Max(0, size.Width);
        var height = Math.Max(0, size.Height);
        var topLeft = Math.Max(0, cornerRadius.TopLeft);
        var topRight = Math.Max(0, cornerRadius.TopRight);
        var bottomRight = Math.Max(0, cornerRadius.BottomRight);
        var bottomLeft = Math.Max(0, cornerRadius.BottomLeft);
        var scale = 1d;

        scale = LimitCornerScale(scale, width, topLeft + topRight);
        scale = LimitCornerScale(scale, width, bottomLeft + bottomRight);
        scale = LimitCornerScale(scale, height, topLeft + bottomLeft);
        scale = LimitCornerScale(scale, height, topRight + bottomRight);

        topLeft *= scale;
        topRight *= scale;
        bottomRight *= scale;
        bottomLeft *= scale;

        if (
            topLeft == topRight
            && topLeft == bottomRight
            && topLeft == bottomLeft
        )
        {
            var rectangle = new RectangleGeometry(
                new Rect(0, 0, width, height),
                topLeft,
                topLeft
            );
            rectangle.Freeze();
            return rectangle;
        }

        var geometry = new StreamGeometry();
        using (var context = geometry.Open())
        {
            context.BeginFigure(new Point(topLeft, 0), isFilled: true, isClosed: true);
            context.LineTo(new Point(width - topRight, 0), true, false);
            AppendCorner(context, new Point(width, topRight), topRight);
            context.LineTo(new Point(width, height - bottomRight), true, false);
            AppendCorner(context, new Point(width - bottomRight, height), bottomRight);
            context.LineTo(new Point(bottomLeft, height), true, false);
            AppendCorner(context, new Point(0, height - bottomLeft), bottomLeft);
            context.LineTo(new Point(0, topLeft), true, false);
            AppendCorner(context, new Point(topLeft, 0), topLeft);
        }

        geometry.Freeze();
        return geometry;
    }

    private static void AppendCorner(
        StreamGeometryContext context,
        Point endPoint,
        double radius
    )
    {
        if (radius <= 0)
        {
            context.LineTo(endPoint, true, false);
            return;
        }

        context.ArcTo(
            endPoint,
            new Size(radius, radius),
            0,
            false,
            SweepDirection.Clockwise,
            true,
            false
        );
    }

    private static double LimitCornerScale(
        double currentScale,
        double availableLength,
        double requestedLength
    )
    {
        return requestedLength > availableLength && requestedLength > 0
            ? Math.Min(currentScale, availableLength / requestedLength)
            : currentScale;
    }
}
