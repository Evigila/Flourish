using System.Windows.Media;
using System.Windows.Media.Imaging;
using AckSS.Flourish.Abstract;
using AckSS.Flourish.Configuration;

namespace AckSS.Flourish.Composition;

internal sealed class FlourishTitlebarBuilder(FlourishShellOptions options)
    : IFlourishTitlebarBuilder
{
    public IFlourishTitlebarBuilder ShowSearch(bool enabled = true)
    {
        options.IsTitlebarSearchEnabled = enabled;
        return this;
    }

    public IFlourishTitlebarBuilder ShowBreadcrumb(bool enabled = true)
    {
        options.IsBreadcrumbEnabled = enabled;
        return this;
    }

    public IFlourishTitlebarBuilder ShowNavToggle(bool enabled = true)
    {
        options.IsTitlebarNavigationToggleEnabled = enabled;
        return this;
    }

    public IFlourishTitlebarBuilder ShowLogo(bool enabled = true)
    {
        options.IsTitlebarLogoEnabled = enabled;
        return this;
    }

    public IFlourishTitlebarBuilder ShowTitle(bool enabled = true)
    {
        options.IsTitlebarTitleEnabled = enabled;
        return this;
    }

    public IFlourishTitlebarBuilder ShowSubTitle(bool enabled = true)
    {
        options.IsTitlebarSubtitleEnabled = enabled;
        return this;
    }

    public IFlourishTitlebarBuilder ShowProfile(bool enabled = true)
    {
        options.IsTitlebarProfileEnabled = enabled;
        return this;
    }

    public IFlourishTitlebarBuilder ShowThemeToggle(bool enabled = true)
    {
        options.IsTitlebarThemeToggleEnabled = enabled;
        return this;
    }

    public IFlourishTitlebarBuilder SetTrayExit(bool enabled = false)
    {
        options.IsTrayExitEnabled = enabled;
        return this;
    }

    public IFlourishTitlebarBuilder SetTitle(string title)
    {
        options.Title = ValidateNotBlank(title, nameof(title));
        return this;
    }

    public IFlourishTitlebarBuilder SetSubtitle(string subtitle)
    {
        options.Subtitle = subtitle;
        return this;
    }

    public IFlourishTitlebarBuilder SetLogo(string packUri)
    {
        options.IsTitlebarLogoEnabled = true;
        options.LogoSource = new BitmapImage(new Uri(packUri, UriKind.RelativeOrAbsolute));
        return this;
    }

    public IFlourishTitlebarBuilder SetLogo(ImageSource logoSource)
    {
        ArgumentNullException.ThrowIfNull(logoSource);
        options.IsTitlebarLogoEnabled = true;
        options.LogoSource = logoSource;
        return this;
    }

    public IFlourishTitlebarBuilder SetLogoFallbackText(string fallbackText)
    {
        options.IsTitlebarLogoEnabled = true;
        options.LogoFallbackText = ValidateNotBlank(fallbackText, nameof(fallbackText));
        return this;
    }

    public IFlourishTitlebarBuilder SetSearchPlaceholder(string placeholder)
    {
        options.SearchPlaceholder = placeholder;
        return this;
    }

    public IFlourishTitlebarBuilder SetSearchHandler(Action<string> searchTextChanged)
    {
        ArgumentNullException.ThrowIfNull(searchTextChanged);
        return SetSearchHandler((_, text) => searchTextChanged(text));
    }

    public IFlourishTitlebarBuilder SetSearchHandler(
        Action<IServiceProvider, string> searchTextChanged
    )
    {
        ArgumentNullException.ThrowIfNull(searchTextChanged);
        options.IsTitlebarSearchEnabled = true;
        options.TitlebarSearchTextChanged = searchTextChanged;
        return this;
    }

    public IFlourishTitlebarBuilder SetBreadcrumbBehavior(
        BreadcrumbShowOption behavior = BreadcrumbShowOption.Auto
    )
    {
        options.BreadcrumbShowOption = behavior;
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

}
