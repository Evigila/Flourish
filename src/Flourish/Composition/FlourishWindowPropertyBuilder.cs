using System.Windows;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Configuration;

namespace ArkheideSystem.Flourish.Composition;

internal sealed class FlourishWindowPropertyBuilder(FlourishShellOptions options)
    : IFlourishWindowPropertyBuilder
{
    public IFlourishWindowPropertyBuilder SetWindowSize(double width = 1536, double height = 864)
    {
        ValidatePositiveFinite(width, nameof(width));
        ValidatePositiveFinite(height, nameof(height));

        options.WindowWidth = width;
        options.WindowHeight = height;
        return this;
    }

    public IFlourishWindowPropertyBuilder SetWindowMinSize(
        double minWidth = 1280,
        double minHeight = 720
    )
    {
        ValidatePositiveFinite(minWidth, nameof(minWidth));
        ValidatePositiveFinite(minHeight, nameof(minHeight));
        EnsureMinDoesNotExceedMax(minWidth, options.WindowMaxWidth, nameof(minWidth));
        EnsureMinDoesNotExceedMax(minHeight, options.WindowMaxHeight, nameof(minHeight));

        options.WindowMinWidth = minWidth;
        options.WindowMinHeight = minHeight;
        return this;
    }

    public IFlourishWindowPropertyBuilder SetWindowMaxSize(
        double maxWidth = double.PositiveInfinity,
        double maxHeight = double.PositiveInfinity
    )
    {
        ValidatePositiveSize(maxWidth, nameof(maxWidth));
        ValidatePositiveSize(maxHeight, nameof(maxHeight));
        EnsureMaxIsNotBelowMin(maxWidth, options.WindowMinWidth, nameof(maxWidth));
        EnsureMaxIsNotBelowMin(maxHeight, options.WindowMinHeight, nameof(maxHeight));

        options.WindowMaxWidth = maxWidth;
        options.WindowMaxHeight = maxHeight;
        return this;
    }

    public IFlourishWindowPropertyBuilder SetWindowPosition(
        WindowStartupLocation startupLocation = WindowStartupLocation.CenterScreen
    )
    {
        ValidateEnum(startupLocation, nameof(startupLocation));
        options.WindowStartupLocation = startupLocation;
        if (startupLocation != WindowStartupLocation.Manual)
        {
            options.WindowLeft = null;
            options.WindowTop = null;
        }

        return this;
    }

    public IFlourishWindowPropertyBuilder SetManualWindowPosition(double left = 0, double top = 0)
    {
        ValidateFinite(left, nameof(left));
        ValidateFinite(top, nameof(top));

        options.WindowLeft = left;
        options.WindowTop = top;
        options.WindowStartupLocation = WindowStartupLocation.Manual;
        return this;
    }

    public IFlourishWindowPropertyBuilder SetWindowState(
        WindowState windowState = WindowState.Normal
    )
    {
        ValidateEnum(windowState, nameof(windowState));
        options.WindowState = windowState;
        return this;
    }

    public IFlourishWindowPropertyBuilder SetWindowResizeMode(
        ResizeMode resizeMode = ResizeMode.CanResize
    )
    {
        ValidateEnum(resizeMode, nameof(resizeMode));
        options.WindowResizeMode = resizeMode;
        return this;
    }

    public IFlourishWindowPropertyBuilder UseTopmost(bool enabled = true)
    {
        options.WindowTopmost = enabled;
        return this;
    }

    public IFlourishWindowPropertyBuilder ShowInTaskbar(bool enabled = true)
    {
        options.WindowShowInTaskbar = enabled;
        return this;
    }

    public IFlourishWindowPropertyBuilder SetTrayExit(bool enabled = true)
    {
        options.IsTrayExitEnabled = enabled;
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
