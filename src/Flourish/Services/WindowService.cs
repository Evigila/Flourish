using System.Windows;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;

namespace ArkheideSystem.Flourish.Services;

internal sealed class WindowService(FlourishShellOptions options) : IWindowService
{
    private readonly Lock gate = new();
    private Window? owner;
    private FlourishWindowState? lastPublishedState;
    private int ownerChangeSuppression;

    public event EventHandler<FlourishWindowStateChangedEventArgs>? StateChanged;

    public FlourishWindowState Current
    {
        get
        {
            Window? current;
            lock (gate)
            {
                current = owner;
                if (current is null)
                {
                    return CreateOptionsSnapshot();
                }
            }

            return ReadOwnerThreadSafe(current);
        }
    }

    public void SetBounds(Rect bounds)
    {
        ValidateFinite(bounds.X, nameof(bounds));
        ValidateFinite(bounds.Y, nameof(bounds));
        ValidatePositive(bounds.Width, nameof(bounds));
        ValidatePositive(bounds.Height, nameof(bounds));
        UpdateOptions(() =>
        {
            options.WindowLeft = bounds.X;
            options.WindowTop = bounds.Y;
            options.WindowWidth = bounds.Width;
            options.WindowHeight = bounds.Height;
            options.WindowStartupLocation = WindowStartupLocation.Manual;
        });
        Apply(window =>
        {
            window.WindowStartupLocation = WindowStartupLocation.Manual;
            window.Left = bounds.X;
            window.Top = bounds.Y;
            window.Width = bounds.Width;
            window.Height = bounds.Height;
        });
    }

    public void SetSize(double width, double height)
    {
        ValidatePositive(width, nameof(width));
        ValidatePositive(height, nameof(height));
        UpdateOptions(() =>
        {
            options.WindowWidth = width;
            options.WindowHeight = height;
        });
        Apply(window =>
        {
            window.Width = width;
            window.Height = height;
        });
    }

    public void SetMinimumSize(double width, double height)
    {
        ValidateNonNegative(width, nameof(width));
        ValidateNonNegative(height, nameof(height));
        lock (gate)
        {
            if (width > options.WindowMaxWidth)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(width),
                    width,
                    "Minimum width cannot exceed maximum width."
                );
            }

            if (height > options.WindowMaxHeight)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(height),
                    height,
                    "Minimum height cannot exceed maximum height."
                );
            }

            options.WindowMinWidth = width;
            options.WindowMinHeight = height;
        }
        Apply(window =>
        {
            window.MinWidth = width;
            window.MinHeight = height;
        });
    }

    public void SetMaximumSize(double width, double height)
    {
        ValidatePositiveOrInfinity(width, nameof(width));
        ValidatePositiveOrInfinity(height, nameof(height));
        lock (gate)
        {
            if (width < options.WindowMinWidth)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(width),
                    width,
                    "Maximum width cannot be below minimum width."
                );
            }

            if (height < options.WindowMinHeight)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(height),
                    height,
                    "Maximum height cannot be below minimum height."
                );
            }

            options.WindowMaxWidth = width;
            options.WindowMaxHeight = height;
        }
        Apply(window =>
        {
            window.MaxWidth = width;
            window.MaxHeight = height;
        });
    }

    public void SetResizeMode(ResizeMode resizeMode)
    {
        if (!Enum.IsDefined(resizeMode))
        {
            throw new ArgumentOutOfRangeException(
                nameof(resizeMode),
                resizeMode,
                "Unknown resize mode."
            );
        }

        UpdateOptions(() => options.WindowResizeMode = resizeMode);
        Apply(window => window.ResizeMode = resizeMode);
    }

    public void SetTopmost(bool topmost)
    {
        UpdateOptions(() => options.WindowTopmost = topmost);
        Apply(window => window.Topmost = topmost);
    }

    public void SetShownInTaskbar(bool shown)
    {
        UpdateOptions(() => options.WindowShowInTaskbar = shown);
        Apply(window => window.ShowInTaskbar = shown);
    }

    public void CenterOnScreen()
    {
        UpdateOptions(() =>
        {
            options.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            options.WindowLeft = null;
            options.WindowTop = null;
        });
        Apply(window =>
        {
            var workArea = SystemParameters.WorkArea;
            var width = double.IsNaN(window.Width) ? window.ActualWidth : window.Width;
            var height = double.IsNaN(window.Height) ? window.ActualHeight : window.Height;
            window.WindowStartupLocation = WindowStartupLocation.Manual;
            window.Left = workArea.Left + Math.Max(0, (workArea.Width - width) / 2);
            window.Top = workArea.Top + Math.Max(0, (workArea.Height - height) / 2);
        });
    }

    public void Show() => Apply(window => window.Show());

    public void Hide() => Apply(window => window.Hide());

    public void Activate() => Apply(window => window.Activate());

    public void Minimize()
    {
        UpdateOptions(() => options.WindowState = WindowState.Minimized);
        Apply(window => window.WindowState = WindowState.Minimized);
    }

    public void Maximize()
    {
        UpdateOptions(() => options.WindowState = WindowState.Maximized);
        Apply(window => window.WindowState = WindowState.Maximized);
    }

    public void Restore()
    {
        UpdateOptions(() => options.WindowState = WindowState.Normal);
        Apply(window => window.WindowState = WindowState.Normal);
    }

    internal void Attach(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);
        lock (gate)
        {
            if (ReferenceEquals(owner, window))
            {
                return;
            }

            DetachCore();
            owner = window;
            lastPublishedState = null;
            owner.StateChanged += OwnerChanged;
            owner.LocationChanged += OwnerChanged;
            owner.SizeChanged += OwnerSizeChanged;
            owner.IsVisibleChanged += OwnerVisibleChanged;
            owner.Activated += OwnerChanged;
            owner.Deactivated += OwnerChanged;
            owner.Closed += OwnerClosed;
        }

        ProcessOwnerChange(window);
    }

    private void Apply(Action<Window> action)
    {
        Window? current;
        FlourishWindowState? unattachedState = null;
        lock (gate)
        {
            current = owner;
            if (current is null)
            {
                unattachedState = CreateOptionsSnapshot();
            }
        }

        if (current is null)
        {
            Publish(unattachedState!);
            return;
        }

        if (current.Dispatcher.CheckAccess())
        {
            ApplyCore(current, action);
            return;
        }

        current.Dispatcher.Invoke(() => ApplyCore(current, action));
    }

    private void ApplyCore(Window window, Action<Window> action)
    {
        ownerChangeSuppression++;
        try
        {
            action(window);
        }
        finally
        {
            ownerChangeSuppression--;
        }

        ProcessOwnerChange(window);
    }

    private void OwnerChanged(object? sender, EventArgs e)
    {
        if (ownerChangeSuppression == 0 && sender is Window window)
        {
            ProcessOwnerChange(window);
        }
    }

    private void OwnerSizeChanged(object sender, SizeChangedEventArgs e) => OwnerChanged(sender, e);

    private void OwnerVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (ownerChangeSuppression == 0 && sender is Window window)
        {
            ProcessOwnerChange(window);
        }
    }

    private void OwnerClosed(object? sender, EventArgs e)
    {
        FlourishWindowState state;
        lock (gate)
        {
            DetachCore();
            state = CreateOptionsSnapshot();
        }

        Publish(state);
    }

    private void DetachCore()
    {
        if (owner is null)
        {
            return;
        }

        owner.StateChanged -= OwnerChanged;
        owner.LocationChanged -= OwnerChanged;
        owner.SizeChanged -= OwnerSizeChanged;
        owner.IsVisibleChanged -= OwnerVisibleChanged;
        owner.Activated -= OwnerChanged;
        owner.Deactivated -= OwnerChanged;
        owner.Closed -= OwnerClosed;
        owner = null;
    }

    private void ProcessOwnerChange(Window window)
    {
        var state = ReadOwner(window);
        lock (gate)
        {
            if (!ReferenceEquals(owner, window))
            {
                return;
            }

            SynchronizeOptionsLocked(state);
        }

        Publish(state);
    }

    private void SynchronizeOptionsLocked(FlourishWindowState state)
    {
        options.WindowState = state.WindowState;
        options.WindowResizeMode = state.ResizeMode;
        options.WindowTopmost = state.IsTopmost;
        options.WindowShowInTaskbar = state.IsShownInTaskbar;
        if (state.WindowState == WindowState.Normal)
        {
            options.WindowLeft = state.Bounds.X;
            options.WindowTop = state.Bounds.Y;
            options.WindowWidth = state.Bounds.Width;
            options.WindowHeight = state.Bounds.Height;
        }
    }

    private void Publish(FlourishWindowState state)
    {
        lock (gate)
        {
            if (state == lastPublishedState)
            {
                return;
            }

            lastPublishedState = state;
        }

        StateChanged?.Invoke(this, new FlourishWindowStateChangedEventArgs(state));
    }

    private static FlourishWindowState ReadOwnerThreadSafe(Window window)
    {
        return window.Dispatcher.CheckAccess()
            ? ReadOwner(window)
            : window.Dispatcher.Invoke(() => ReadOwner(window));
    }

    private static FlourishWindowState ReadOwner(Window window)
    {
        var width = window.ActualWidth > 0 ? window.ActualWidth : window.Width;
        var height = window.ActualHeight > 0 ? window.ActualHeight : window.Height;
        return new FlourishWindowState(
            new Rect(window.Left, window.Top, Math.Max(0, width), Math.Max(0, height)),
            new System.Windows.Size(window.MinWidth, window.MinHeight),
            new System.Windows.Size(window.MaxWidth, window.MaxHeight),
            window.WindowState,
            window.ResizeMode,
            window.Topmost,
            window.ShowInTaskbar,
            window.IsVisible,
            window.IsActive
        );
    }

    private FlourishWindowState CreateOptionsSnapshot()
    {
        return new FlourishWindowState(
            new Rect(
                options.WindowLeft ?? 0,
                options.WindowTop ?? 0,
                options.WindowWidth,
                options.WindowHeight
            ),
            new System.Windows.Size(options.WindowMinWidth, options.WindowMinHeight),
            new System.Windows.Size(options.WindowMaxWidth, options.WindowMaxHeight),
            options.WindowState,
            options.WindowResizeMode,
            options.WindowTopmost,
            options.WindowShowInTaskbar,
            false,
            false
        );
    }

    private void UpdateOptions(Action update)
    {
        lock (gate)
        {
            update();
        }
    }

    private static void ValidateFinite(double value, string parameterName)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            throw new ArgumentOutOfRangeException(parameterName, value, "Value must be finite.");
        }
    }

    private static void ValidatePositive(double value, string parameterName)
    {
        ValidateFinite(value, parameterName);
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "Value must be greater than zero."
            );
        }
    }

    private static void ValidateNonNegative(double value, string parameterName)
    {
        ValidateFinite(value, parameterName);
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "Value cannot be negative."
            );
        }
    }

    private static void ValidatePositiveOrInfinity(double value, string parameterName)
    {
        if (double.IsPositiveInfinity(value))
        {
            return;
        }

        ValidatePositive(value, parameterName);
    }
}
