using System.Windows;
using System.Windows.Media.Imaging;
using AcksheedSys.Flourish.Abstract;
using AcksheedSys.Flourish.Models;
using Microsoft.Extensions.Hosting;

namespace AcksheedSys.Flourish.Composition;

internal sealed class FlourishShellBuilder(FlourishShellOptions options, HostBuilderContext context)
    : IFlourishShellBuilder
{
    public IFlourishShellBuilder UseTitlebar(
        Action<HostBuilderContext, IFlourishTitlebarBuilder> configureTitlebar
    )
    {
        configureTitlebar(context, new FlourishTitlebarBuilder(options));
        return this;
    }

    public IFlourishShellBuilder UseNavigationPanel(
        Action<HostBuilderContext, IFlourishNavigationPanelBuilder> configureNavigationPanel
    )
    {
        options.IsNavigationPanelEnabled = true;
        configureNavigationPanel(context, new FlourishNavigationPanelBuilder(options));
        return this;
    }

    public IFlourishShellBuilder SetWindowProperty(
        Action<HostBuilderContext, IFlourishWindowPropertyBuilder> configureWindow
    )
    {
        configureWindow(context, new FlourishWindowPropertyBuilder(this));
        return this;
    }

    public IFlourishShellBuilder SetGlobalFont(string fontFamily, double fontSize)
    {
        return SetFont(fontFamily, fontSize);
    }

    public IFlourishShellBuilder SetTitle(string title)
    {
        options.Title = title;
        return this;
    }

    public IFlourishShellBuilder SetSubtitle(string subtitle)
    {
        options.Subtitle = subtitle;
        return this;
    }

    public IFlourishShellBuilder SetLogo(string packUri)
    {
        options.LogoSource = new BitmapImage(new Uri(packUri, UriKind.RelativeOrAbsolute));
        return this;
    }

    public IFlourishShellBuilder SetFont(string fontFamily, double fontSize)
    {
        SetFontFamily(fontFamily);
        SetFontSize(fontSize);
        return this;
    }

    public IFlourishShellBuilder SetFontFamily(string fontFamily)
    {
        options.FontFamily = ValidateNotBlank(fontFamily, nameof(fontFamily));
        return this;
    }

    public IFlourishShellBuilder SetFontSize(double fontSize)
    {
        ValidatePositiveFinite(fontSize, nameof(fontSize));
        options.FontSize = fontSize;
        return this;
    }

    public IFlourishShellBuilder SetIconFontFamily(string fontFamily)
    {
        options.IconFontFamily = ValidateNotBlank(fontFamily, nameof(fontFamily));
        return this;
    }

    public IFlourishShellBuilder UseTitlebar(
        bool EnableSearch = true,
        bool EnableBreadcrumb = true,
        bool EnableNavToggle = true,
        bool EnableLogo = true,
        bool EnableTitle = true,
        bool EnableSubTitle = true,
        bool EnableProfile = true,
        bool EnableTrayExit = false
    )
    {
        options.IsTitlebarSearchEnabled = EnableSearch;
        options.IsBreadcrumbEnabled = EnableBreadcrumb;
        options.IsTitlebarNavigationToggleEnabled = EnableNavToggle;
        options.IsTitlebarLogoEnabled = EnableLogo;
        options.IsTitlebarTitleEnabled = EnableTitle;
        options.IsTitlebarSubtitleEnabled = EnableSubTitle;
        options.IsTitlebarProfileEnabled = EnableProfile;
        options.IsTrayExitEnabled = EnableTrayExit;
        return this;
    }

    public IFlourishShellBuilder SetSearchPlaceholder(string placeholder)
    {
        options.SearchPlaceholder = placeholder;
        return this;
    }

    public IFlourishShellBuilder SetWindowSize(double width, double height)
    {
        ValidatePositiveFinite(width, nameof(width));
        ValidatePositiveFinite(height, nameof(height));

        options.WindowWidth = width;
        options.WindowHeight = height;
        return this;
    }

    public IFlourishShellBuilder SetWindowMinSize(double minWidth, double minHeight)
    {
        ValidatePositiveFinite(minWidth, nameof(minWidth));
        ValidatePositiveFinite(minHeight, nameof(minHeight));
        EnsureMinDoesNotExceedMax(minWidth, options.WindowMaxWidth, nameof(minWidth));
        EnsureMinDoesNotExceedMax(minHeight, options.WindowMaxHeight, nameof(minHeight));

        options.WindowMinWidth = minWidth;
        options.WindowMinHeight = minHeight;
        return this;
    }

    public IFlourishShellBuilder SetWindowMaxSize(double maxWidth, double maxHeight)
    {
        ValidatePositiveSize(maxWidth, nameof(maxWidth));
        ValidatePositiveSize(maxHeight, nameof(maxHeight));
        EnsureMaxIsNotBelowMin(maxWidth, options.WindowMinWidth, nameof(maxWidth));
        EnsureMaxIsNotBelowMin(maxHeight, options.WindowMinHeight, nameof(maxHeight));

        options.WindowMaxWidth = maxWidth;
        options.WindowMaxHeight = maxHeight;
        return this;
    }

    public IFlourishShellBuilder SetWindowPosition(WindowStartupLocation startupLocation)
    {
        options.WindowStartupLocation = startupLocation;
        if (startupLocation != WindowStartupLocation.Manual)
        {
            options.WindowLeft = null;
            options.WindowTop = null;
        }

        return this;
    }

    public IFlourishShellBuilder SetWindowPosition(double left, double top)
    {
        ValidateFinite(left, nameof(left));
        ValidateFinite(top, nameof(top));

        options.WindowLeft = left;
        options.WindowTop = top;
        options.WindowStartupLocation = WindowStartupLocation.Manual;
        return this;
    }

    public IFlourishShellBuilder SetWindowState(WindowState windowState)
    {
        options.WindowState = windowState;
        return this;
    }

    public IFlourishShellBuilder SetWindowResizeMode(ResizeMode resizeMode)
    {
        options.WindowResizeMode = resizeMode;
        return this;
    }

    public IFlourishShellBuilder UseMaterialEffect(MaterialEffect effect = MaterialEffect.Mica)
    {
        options.MaterialEffect = effect;
        return this;
    }

    public IFlourishShellBuilder UseTopmost(bool enabled = true)
    {
        options.WindowTopmost = enabled;
        return this;
    }

    public IFlourishShellBuilder ShowInTaskbar(bool enabled = true)
    {
        options.WindowShowInTaskbar = enabled;
        return this;
    }

    public IFlourishShellBuilder UseNavigationPanel(
        bool enabled = true,
        NavigationPanelDirection direction = NavigationPanelDirection.Left,
        string title = "Navigation",
        bool isInitiallyOpen = true
    )
    {
        options.IsNavigationPanelEnabled = enabled;
        options.NavigationPanelDirection = direction;
        options.PaneTitle = title;
        options.IsNavigationPanelInitiallyOpen = isInitiallyOpen;
        return this;
    }

    public IFlourishShellBuilder UseDynamicToolbar(bool enabled = true)
    {
        options.IsDynamicToolbarEnabled = enabled;
        return this;
    }

    public IFlourishShellBuilder SetBreadcrumbBehavior(
        BreadcrumbShowOption behavior = BreadcrumbShowOption.Auto
    )
    {
        options.BreadcrumbShowOption = behavior;
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

    private static string ValidateNotBlank(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be empty.", parameterName);
        }

        return value;
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
}
