using System;

using ArkheideSystem.Flourish.Abstract;
using System.Windows;
using ArkheideSystem.Flourish.Configuration;

namespace ArkheideSystem.Flourish.Windowing;

internal sealed class WindowBuilder(FlourishWindowOptions options)
    : FlourishBuilderMutationGuard,
        IWindowBuilder
{
    public IWindowBuilder SetSize(
        double width = 1536,
        double height = 864,
        bool usePersistedPreference = true
    )
    {
        ThrowIfFrozen();
        ValidatePositiveFinite(width, nameof(width));
        ValidatePositiveFinite(height, nameof(height));

        options.WindowWidth = width;
        options.WindowHeight = height;
        options.UsePersistedWindowSize = usePersistedPreference;
        return this;
    }

    public IWindowBuilder SetMinimumSize(
        double minWidth = 1280,
        double minHeight = 720
    )
    {
        ThrowIfFrozen();
        ValidatePositiveFinite(minWidth, nameof(minWidth));
        ValidatePositiveFinite(minHeight, nameof(minHeight));
        EnsureMinDoesNotExceedMax(minWidth, options.WindowMaxWidth, nameof(minWidth));
        EnsureMinDoesNotExceedMax(minHeight, options.WindowMaxHeight, nameof(minHeight));

        options.WindowMinWidth = minWidth;
        options.WindowMinHeight = minHeight;
        return this;
    }

    public IWindowBuilder SetMaximumSize(
        double maxWidth = double.PositiveInfinity,
        double maxHeight = double.PositiveInfinity
    )
    {
        ThrowIfFrozen();
        ValidatePositiveSize(maxWidth, nameof(maxWidth));
        ValidatePositiveSize(maxHeight, nameof(maxHeight));
        EnsureMaxIsNotBelowMin(maxWidth, options.WindowMinWidth, nameof(maxWidth));
        EnsureMaxIsNotBelowMin(maxHeight, options.WindowMinHeight, nameof(maxHeight));

        options.WindowMaxWidth = maxWidth;
        options.WindowMaxHeight = maxHeight;
        return this;
    }

    public IWindowBuilder SetStartupLocation(
        WindowStartupLocation startupLocation = WindowStartupLocation.CenterScreen,
        bool usePersistedPreference = true
    )
    {
        ThrowIfFrozen();
        ValidateEnum(startupLocation, nameof(startupLocation));
        options.WindowStartupLocation = startupLocation;
        options.UsePersistedWindowPosition = usePersistedPreference;
        if (startupLocation != WindowStartupLocation.Manual)
        {
            options.WindowLeft = null;
            options.WindowTop = null;
        }

        return this;
    }

    public IWindowBuilder SetManualPosition(
        double left = 0,
        double top = 0,
        bool usePersistedPreference = true
    )
    {
        ThrowIfFrozen();
        ValidateFinite(left, nameof(left));
        ValidateFinite(top, nameof(top));

        options.WindowLeft = left;
        options.WindowTop = top;
        options.WindowStartupLocation = WindowStartupLocation.Manual;
        options.UsePersistedWindowPosition = usePersistedPreference;
        return this;
    }

    public IWindowBuilder SetState(
        WindowState windowState = WindowState.Normal,
        bool usePersistedPreference = true
    )
    {
        ThrowIfFrozen();
        ValidateEnum(windowState, nameof(windowState));
        options.WindowState = windowState;
        options.UsePersistedWindowState = usePersistedPreference;
        return this;
    }

    public IWindowBuilder SetResizeMode(
        ResizeMode resizeMode = ResizeMode.CanResize
    )
    {
        ThrowIfFrozen();
        ValidateEnum(resizeMode, nameof(resizeMode));
        options.WindowResizeMode = resizeMode;
        return this;
    }

    public IWindowBuilder SetTopmost(
        bool enabled = true,
        bool usePersistedPreference = true
    )
    {
        ThrowIfFrozen();
        options.WindowTopmost = enabled;
        options.UsePersistedWindowTopmost = usePersistedPreference;
        return this;
    }

    public IWindowBuilder SetShownInTaskbar(bool enabled = true)
    {
        ThrowIfFrozen();
        options.WindowShowInTaskbar = enabled;
        return this;
    }

    public IWindowBuilder SetTrayExit(
        bool enabled = true,
        bool usePersistedPreference = true
    )
    {
        ThrowIfFrozen();
        options.IsTrayExitEnabled = enabled;
        options.UsePersistedTrayExit = usePersistedPreference;
        return this;
    }

    private static void ValidatePositiveFinite(double value, string parameterName)
    {
        ValidateFinite(value, parameterName);
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "Value must be greater than 0."
            );
        }
    }

    private static void ValidatePositiveSize(double value, string parameterName)
    {
        if (double.IsNaN(value) || value <= 0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "Value must be greater than 0."
            );
        }
    }

    private static void ValidateFinite(double value, string parameterName)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            throw new ArgumentOutOfRangeException(parameterName, value, "Value must be finite.");
        }
    }

    private static void EnsureMinDoesNotExceedMax(
        double minValue,
        double maxValue,
        string parameterName
    )
    {
        if (minValue > maxValue)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                minValue,
                "Minimum size cannot exceed maximum size."
            );
        }
    }

    private static void EnsureMaxIsNotBelowMin(
        double maxValue,
        double minValue,
        string parameterName
    )
    {
        if (maxValue < minValue)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                maxValue,
                "Maximum size cannot be below minimum size."
            );
        }
    }

    private static void ValidateEnum<TEnum>(TEnum value, string parameterName)
        where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(value))
        {
            throw new ArgumentOutOfRangeException(parameterName, value, "Unknown value.");
        }
    }
}
