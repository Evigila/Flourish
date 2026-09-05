using System;
using System.Threading;

using ArkheideSystem.Flourish.Abstract;

namespace ArkheideSystem.Flourish.Navigation;

internal sealed class NavigationPanelService(NavigationOptions options)
{
    private readonly Lock gate = new();
    private readonly NavigationOptions options =
        options ?? throw new ArgumentNullException(nameof(options));
    private bool isOpen = options.IsNavigationPanelInitiallyOpen;
    private NavigationPanelState current = new(
        options.IsNavigationPanelEnabled,
        options.IsNavigationPanelInitiallyOpen,
        options.NavigationPanelDirection,
        options.OpenPaneWidth,
        options.ClosedPaneWidth,
        options.NavigationPaneMinWidth,
        options.NavigationPaneMaxWidth,
        0
    );
    private long version;

    public event EventHandler<NavigationPanelChangedEventArgs>? Changed;

    public NavigationPanelState Current => Volatile.Read(ref current);

    public void SetEnabled(bool enabled)
    {
        Mutate(() => options.IsNavigationPanelEnabled = enabled, animate: false);
    }

    public void SetDirection(NavigationPanelDirection direction)
    {
        if (!Enum.IsDefined(direction))
        {
            throw new ArgumentOutOfRangeException(nameof(direction), direction, "Unknown value.");
        }

        Mutate(() => options.NavigationPanelDirection = direction, animate: false);
    }

    public void SetPanelWidth(
        double openWidth,
        double closedWidth,
        double maxWidth,
        double minWidth
    )
    {
        ValidateDimensions(openWidth, closedWidth, maxWidth, minWidth);
        Mutate(
            () =>
            {
                options.OpenPaneWidth = openWidth;
                options.ClosedPaneWidth = closedWidth;
                options.NavigationPaneMaxWidth = maxWidth;
                options.NavigationPaneMinWidth = minWidth;
            },
            animate: false
        );
    }

    public void Open(bool animate = true)
    {
        Mutate(() => isOpen = true, animate);
    }

    public void Close(bool animate = true)
    {
        Mutate(() => isOpen = false, animate);
    }

    public void Toggle(bool animate = true)
    {
        Mutate(() => isOpen = !isOpen, animate);
    }

    internal void RecordOpenWidth(double openWidth)
    {
        lock (gate)
        {
            var clamped = Math.Clamp(
                openWidth,
                options.NavigationPaneMinWidth,
                options.NavigationPaneMaxWidth
            );
            if (options.OpenPaneWidth.Equals(clamped))
            {
                return;
            }

            options.OpenPaneWidth = clamped;
            Volatile.Write(ref current, CreateSnapshot());
        }
    }

    internal void RecordOpenState(bool open)
    {
        lock (gate)
        {
            if (isOpen == open)
            {
                return;
            }

            isOpen = open;
            Volatile.Write(ref current, CreateSnapshot());
        }
    }

    private void Mutate(Action mutation, bool animate)
    {
        NavigationPanelState previous;
        NavigationPanelState current;
        lock (gate)
        {
            previous = Volatile.Read(ref this.current);
            mutation();
            current = CreateSnapshot();
            if (previous with { Version = current.Version } == current)
            {
                return;
            }

            version++;
            current = CreateSnapshot();
            Volatile.Write(ref this.current, current);
        }

        Changed?.Invoke(
            this,
            new NavigationPanelChangedEventArgs(previous, current, animate)
        );
    }

    private NavigationPanelState CreateSnapshot()
    {
        return new NavigationPanelState(
            options.IsNavigationPanelEnabled,
            isOpen,
            options.NavigationPanelDirection,
            options.OpenPaneWidth,
            options.ClosedPaneWidth,
            options.NavigationPaneMinWidth,
            options.NavigationPaneMaxWidth,
            version
        );
    }

    private static void ValidateDimensions(
        double openWidth,
        double closedWidth,
        double maxWidth,
        double minWidth
    )
    {
        ValidatePositiveFinite(openWidth, nameof(openWidth));
        NavigationPanelDimensions.ValidateCollapsedWidth(closedWidth, nameof(closedWidth));
        ValidatePositiveFinite(maxWidth, nameof(maxWidth));
        ValidatePositiveFinite(minWidth, nameof(minWidth));

        if (closedWidth > openWidth)
        {
            throw new ArgumentOutOfRangeException(
                nameof(closedWidth),
                closedWidth,
                "Closed width cannot exceed open width."
            );
        }

        if (minWidth > maxWidth)
        {
            throw new ArgumentOutOfRangeException(
                nameof(minWidth),
                minWidth,
                "Minimum width cannot exceed maximum width."
            );
        }

        if (openWidth < minWidth || openWidth > maxWidth)
        {
            throw new ArgumentOutOfRangeException(
                nameof(openWidth),
                openWidth,
                "Open width must be within the minimum and maximum range."
            );
        }
    }

    private static void ValidatePositiveFinite(double value, string parameterName)
    {
        if (!double.IsFinite(value) || value <= 0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "Value must be positive and finite."
            );
        }
    }
}
