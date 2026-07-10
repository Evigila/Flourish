using System.Windows.Media.Imaging;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Configuration;

namespace ArkheideSystem.Flourish.Composition;

internal sealed class FlourishTitlebarBuilder(FlourishShellOptions options)
    : IFlourishTitlebarBuilder
{
    private const string ApplicationPackUriPrefix = "pack://application:,,,/";

    public IFlourishTitlebarBuilder SetSearch(
        string placeholder,
        Action<string> handler
    )
    {
        ArgumentNullException.ThrowIfNull(handler);
        return SetSearch(placeholder, (_, text) => handler(text));
    }

    public IFlourishTitlebarBuilder SetSearch(
        string placeholder,
        Action<IServiceProvider, string> handler
    )
    {
        ArgumentNullException.ThrowIfNull(handler);
        options.SearchPlaceholder = ValidateNotBlank(placeholder, nameof(placeholder));
        options.TitlebarSearchTextChanged = handler;
        options.IsTitlebarSearchEnabled = true;
        return this;
    }

    public IFlourishTitlebarBuilder SetBreadcrumbButton(
        BreadcrumbShowOption option = BreadcrumbShowOption.Auto
    )
    {
        ValidateEnum(option, nameof(option));
        options.BreadcrumbShowOption = option;
        options.IsBreadcrumbEnabled = true;
        return this;
    }

    public IFlourishTitlebarBuilder SetNavToggle()
    {
        options.IsTitlebarNavigationToggleEnabled = true;
        return this;
    }

    public IFlourishTitlebarBuilder SetLogo(string logoPath)
    {
        var path = ValidateNotBlank(logoPath, nameof(logoPath));
        var logoUri = path.StartsWith(
            ApplicationPackUriPrefix,
            StringComparison.OrdinalIgnoreCase
        )
            ? new Uri($"/{path[ApplicationPackUriPrefix.Length..]}", UriKind.Relative)
            : new Uri(path, UriKind.RelativeOrAbsolute);
        options.LogoSource = new BitmapImage(logoUri);
        options.IsTitlebarLogoEnabled = true;
        return this;
    }

    public IFlourishTitlebarBuilder SetTitle(string title)
    {
        options.Title = ValidateNotBlank(title, nameof(title));
        options.IsTitlebarTitleEnabled = true;
        return this;
    }

    public IFlourishTitlebarBuilder SetSubTitle(string subTitle)
    {
        options.Subtitle = ValidateNotBlank(subTitle, nameof(subTitle));
        options.IsTitlebarSubtitleEnabled = true;
        return this;
    }

    public IFlourishTitlebarBuilder SetProfile(
        NameOrder nameOrder = NameOrder.FirstLast
    )
    {
        ValidateEnum(nameOrder, nameof(nameOrder));
        options.Profile.NameOrder = nameOrder;
        options.IsProfileEnabled = true;
        options.IsTitlebarProfileEnabled = true;
        return this;
    }

    public IFlourishTitlebarBuilder SetThemeToggle(
        FlourishTheme mode = FlourishTheme.System
    )
    {
        ValidateEnum(mode, nameof(mode));
        options.DefaultTheme = mode;
        options.IsThemeEnabled = true;
        options.IsTitlebarThemeToggleEnabled = true;
        return this;
    }

    private static string ValidateNotBlank(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be empty.", parameterName);
        }

        return value;
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
