using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;

namespace ArkheideSystem.Flourish.Internal.Composition;

internal sealed class FlourishTitlebarBuilder(FlourishShellOptions options)
    : IFlourishTitlebarBuilder
{
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

    public IFlourishTitlebarBuilder SetLogo(
        string? logoPath = null,
        bool showApplicationTitle = true,
        bool showApplicationSubTitle = true,
        bool showProjectTitle = false
    )
    {
        options.LogoPath = logoPath is null
            ? null
            : ValidateNotBlank(logoPath, nameof(logoPath));
        options.IsTitlebarLogoEnabled = true;
        options.ShowApplicationTitleInLogoFlyout = showApplicationTitle;
        options.ShowApplicationSubtitleInLogoFlyout = showApplicationSubTitle;
        options.ShowProjectTitleInLogoFlyout = showProjectTitle;
        return this;
    }

    public IFlourishTitlebarBuilder SetApplicationTitle(string title)
    {
        options.ApplicationTitle = ValidateNotBlank(title, nameof(title));
        options.IsTitlebarTitleEnabled = true;
        return this;
    }

    public IFlourishTitlebarBuilder SetApplicationSubTitle(string subTitle)
    {
        options.ApplicationSubtitle = ValidateNotBlank(subTitle, nameof(subTitle));
        return this;
    }

    public IFlourishTitlebarBuilder SetUnnamedProjectPlaceholder(
        string placeholder = "Unnamed project"
    )
    {
        options.UnnamedProjectPlaceholder = ValidateNotBlank(placeholder, nameof(placeholder));
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
