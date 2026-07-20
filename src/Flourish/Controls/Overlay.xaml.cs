using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace ArkheideSystem.Flourish.Controls;

/// <summary>Describes how long an <see cref="Overlay" /> remains open.</summary>
public enum OverlayVariant
{
    /// <summary>The overlay closes when the pointer leaves its trigger and surface.</summary>
    Temporary,

    /// <summary>The overlay remains open until it is explicitly dismissed.</summary>
    Strong,
}

/// <summary>A themed floating content surface with temporary or strong dismissal semantics.</summary>
public class Overlay : ContentControl
{
    private static readonly TimeSpan TemporaryDismissDelay = TimeSpan.FromMilliseconds(120);
    private readonly DispatcherTimer dismissTimer;
    private bool isPlacementTargetAttached;

    /// <summary>Identifies the <see cref="Variant" /> dependency property.</summary>
    public static readonly DependencyProperty VariantProperty = DependencyProperty.Register(
        nameof(Variant),
        typeof(OverlayVariant),
        typeof(Overlay),
        new FrameworkPropertyMetadata(OverlayVariant.Temporary, OnVariantChanged),
        IsVariantValid
    );

    /// <summary>Identifies the <see cref="PlacementTarget" /> dependency property.</summary>
    public static readonly DependencyProperty PlacementTargetProperty = DependencyProperty.Register(
        nameof(PlacementTarget),
        typeof(FrameworkElement),
        typeof(Overlay),
        new FrameworkPropertyMetadata(null, OnPlacementTargetChanged)
    );

    /// <summary>Identifies the <see cref="DismissRequested" /> routed event.</summary>
    public static readonly RoutedEvent DismissRequestedEvent = EventManager.RegisterRoutedEvent(
        nameof(DismissRequested),
        RoutingStrategy.Bubble,
        typeof(RoutedEventHandler),
        typeof(Overlay)
    );

    static Overlay()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(Overlay),
            new FrameworkPropertyMetadata(typeof(Overlay))
        );
    }

    /// <summary>Initializes an overlay.</summary>
    public Overlay()
    {
        dismissTimer = new DispatcherTimer(DispatcherPriority.Input, Dispatcher)
        {
            Interval = TemporaryDismissDelay,
        };
        dismissTimer.Tick += DismissTimer_Tick;
        MouseEnter += Overlay_MouseEnter;
        MouseLeave += Overlay_MouseLeave;
        Loaded += Overlay_Loaded;
        Unloaded += Overlay_Unloaded;
    }

    /// <summary>Occurs when a temporary overlay asks its host to close it.</summary>
    public event RoutedEventHandler DismissRequested
    {
        add => AddHandler(DismissRequestedEvent, value);
        remove => RemoveHandler(DismissRequestedEvent, value);
    }

    /// <summary>Gets or sets the overlay's dismissal variant.</summary>
    public OverlayVariant Variant
    {
        get => (OverlayVariant)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    /// <summary>Gets or sets the element whose hover state keeps a temporary overlay open.</summary>
    public FrameworkElement? PlacementTarget
    {
        get => (FrameworkElement?)GetValue(PlacementTargetProperty);
        set => SetValue(PlacementTargetProperty, value);
    }

    private static void OnVariantChanged(
        DependencyObject dependencyObject,
        DependencyPropertyChangedEventArgs eventArgs
    )
    {
        ((Overlay)dependencyObject).dismissTimer.Stop();
    }

    private static void OnPlacementTargetChanged(
        DependencyObject dependencyObject,
        DependencyPropertyChangedEventArgs eventArgs
    )
    {
        var overlay = (Overlay)dependencyObject;
        overlay.DetachPlacementTarget(eventArgs.OldValue as FrameworkElement);
        if (overlay.IsLoaded)
        {
            overlay.AttachPlacementTarget(eventArgs.NewValue as FrameworkElement);
        }
        overlay.dismissTimer.Stop();
    }

    private void AttachPlacementTarget(FrameworkElement? target)
    {
        if (target is null || isPlacementTargetAttached)
        {
            return;
        }

        target.MouseEnter += PlacementTarget_MouseEnter;
        target.MouseLeave += PlacementTarget_MouseLeave;
        isPlacementTargetAttached = true;
    }

    private void DetachPlacementTarget(FrameworkElement? target)
    {
        if (target is null || !isPlacementTargetAttached)
        {
            return;
        }

        target.MouseEnter -= PlacementTarget_MouseEnter;
        target.MouseLeave -= PlacementTarget_MouseLeave;
        isPlacementTargetAttached = false;
    }

    private void PlacementTarget_MouseEnter(object sender, MouseEventArgs e) =>
        dismissTimer.Stop();

    private void PlacementTarget_MouseLeave(object sender, MouseEventArgs e) =>
        ScheduleTemporaryDismiss();

    private void Overlay_MouseEnter(object sender, MouseEventArgs e) => dismissTimer.Stop();

    private void Overlay_MouseLeave(object sender, MouseEventArgs e) => ScheduleTemporaryDismiss();

    private void ScheduleTemporaryDismiss()
    {
        if (Variant != OverlayVariant.Temporary || PlacementTarget is null)
        {
            return;
        }

        dismissTimer.Stop();
        dismissTimer.Start();
    }

    private void DismissTimer_Tick(object? sender, EventArgs e)
    {
        dismissTimer.Stop();
        if (
            Variant == OverlayVariant.Temporary
            && !IsMouseOver
            && PlacementTarget?.IsMouseOver != true
        )
        {
            RaiseEvent(new RoutedEventArgs(DismissRequestedEvent, this));
        }
    }

    private void Overlay_Loaded(object sender, RoutedEventArgs e)
    {
        AttachPlacementTarget(PlacementTarget);
    }

    private void Overlay_Unloaded(object sender, RoutedEventArgs e)
    {
        dismissTimer.Stop();
        DetachPlacementTarget(PlacementTarget);
    }

    private static bool IsVariantValid(object value)
    {
        return value is OverlayVariant variant && Enum.IsDefined(variant);
    }
}
