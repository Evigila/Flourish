using AcksheedSys.Flourish.Abstract;
using AcksheedSys.Flourish.Models;
using Application = System.Windows.Application;
using FontFamily = System.Windows.Media.FontFamily;
using Window = System.Windows.Window;

namespace AcksheedSys.Flourish.Services;

internal sealed class FontService(FlourishShellOptions options) : IFlourishFontService
{
    private Window? owner;

    public string FontFamily => options.FontFamily;

    public string IconFontFamily => options.IconFontFamily;

    public double FontSize => options.FontSize;

    public void Apply(Window window)
    {
        owner = window;
        ApplyCore(window);
    }

    public void SetFont(string fontFamily, double fontSize)
    {
        options.FontFamily = ValidateNotBlank(fontFamily, nameof(fontFamily));
        ValidatePositiveFinite(fontSize, nameof(fontSize));
        options.FontSize = fontSize;
        ApplyToOwner();
    }

    public void SetFontFamily(string fontFamily)
    {
        options.FontFamily = ValidateNotBlank(fontFamily, nameof(fontFamily));
        ApplyToOwner();
    }

    public void SetFontSize(double fontSize)
    {
        ValidatePositiveFinite(fontSize, nameof(fontSize));
        options.FontSize = fontSize;
        ApplyToOwner();
    }

    public void SetIconFontFamily(string fontFamily)
    {
        options.IconFontFamily = ValidateNotBlank(fontFamily, nameof(fontFamily));
        ApplyToOwner();
    }

    private void ApplyToOwner()
    {
        if (owner is null)
        {
            return;
        }

        if (owner.CheckAccess())
        {
            ApplyCore(owner);
            return;
        }

        owner.Dispatcher.Invoke(() => ApplyCore(owner));
    }

    private void ApplyCore(Window window)
    {
        var fontFamily = new FontFamily(options.FontFamily);
        var iconFontFamily = new FontFamily(options.IconFontFamily);
        var baseSize = options.FontSize;

        window.FontFamily = fontFamily;
        window.FontSize = baseSize;

        SetResource(window, "FlourishFontFamily", fontFamily);
        SetResource(window, "FlourishIconFontFamily", iconFontFamily);
        SetResource(window, "FlourishFontSizeSmall", ClampFontSize(baseSize - 3));
        SetResource(window, "FlourishFontSizeCaption", ClampFontSize(baseSize - 1));
        SetResource(window, "FlourishFontSizeBase", baseSize);
        SetResource(window, "FlourishFontSizeTitle", baseSize);
        SetResource(window, "FlourishFontSizeTitlebarIcon", ClampFontSize(baseSize + 1));
        SetResource(window, "FlourishFontSizeNavigationIcon", ClampFontSize(baseSize + 3));
        SetResource(window, "FlourishFontSizeWindowButtonIcon", ClampFontSize(baseSize - 2));
    }

    private static void SetResource(Window window, string key, object value)
    {
        window.Resources[key] = value;
        Application.Current?.Resources[key] = value;
    }

    private static double ClampFontSize(double size)
    {
        return Math.Max(1, size);
    }

    private static string ValidateNotBlank(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be empty.", parameterName);
        }

        return value;
    }

    private static void ValidatePositiveFinite(double value, string parameterName)
    {
        if (double.IsNaN(value) || double.IsInfinity(value) || value <= 0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "Value must be a positive finite number."
            );
        }
    }
}
