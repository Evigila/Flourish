using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;

namespace ArkheideSystem.Flourish.Services;

internal sealed class NavigationPanelService : INavigationPanelService
{
    private readonly object gate = new();
    private readonly FlourishShellOptions options;
    private bool isOpen;
    private long version;

    public NavigationPanelService(FlourishShellOptions options)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        isOpen = options.IsNavigationPanelInitiallyOpen;
    }

    public event EventHandler<FlourishNavigationPanelChangedEventArgs>? Changed;

    public FlourishNavigationPanelState Current
    {
        get
        {
            lock (gate)
            {
                return CreateSnapshot();
            }
        }
    }

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
            options.OpenPaneWidth = Math.Clamp(
                openWidth,
                options.NavigationPaneMinWidth,
                options.NavigationPaneMaxWidth
            );
        }
    }

    internal void RecordOpenState(bool open)
    {
        lock (gate)
        {
            isOpen = open;
        }
    }

    private void Mutate(Action mutation, bool animate)
    {
        FlourishNavigationPanelState previous;
        FlourishNavigationPanelState current;
        lock (gate)
        {
            previous = CreateSnapshot();
            mutation();
            current = CreateSnapshot();
            if (previous with { Version = current.Version } == current)
            {
                return;
            }

            version++;
            current = CreateSnapshot();
        }

        Changed?.Invoke(
            this,
            new FlourishNavigationPanelChangedEventArgs(previous, current, animate)
        );
    }

    private FlourishNavigationPanelState CreateSnapshot()
    {
        return new FlourishNavigationPanelState(
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
        NavigationPanelDimensions.ValidateCollapsedWidth(
            closedWidth,
            nameof(closedWidth)
        );
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
            throw new ArgumentOutOfRangeException(parameterName, value, "Value must be positive and finite.");
        }
    }

}
