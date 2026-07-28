using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;

namespace ArkheideSystem.Flourish.Internal.Composition;

internal sealed class FlourishTitlebarBuilder(FlourishShellOptions options)
    : FlourishBuilderMutationGuard,
        IFlourishTitlebarBuilder
{
    public IFlourishTitlebarBuilder UseSearch(
        bool enabled = true,
        string placeholder = "Search",
        Action<IServiceProvider, string>? handler = null
    )
    {
        ThrowIfFrozen();
        options.SearchPlaceholder = ValidateNotBlank(placeholder, nameof(placeholder));
        options.TitlebarSearchTextChanged = handler;
        options.IsTitlebarSearchEnabled = enabled;
        return this;
    }

    public IFlourishTitlebarBuilder UseBreadcrumb(
        bool enabled = true,
        BreadcrumbShowOption option = BreadcrumbShowOption.Auto
    )
    {
        ThrowIfFrozen();
        ValidateEnum(option, nameof(option));
        options.BreadcrumbShowOption = option;
        options.IsBreadcrumbEnabled = enabled;
        return this;
    }

    public IFlourishTitlebarBuilder UseNavigationToggle(bool enabled = true)
    {
        ThrowIfFrozen();
        options.IsTitlebarNavigationToggleEnabled = enabled;
        return this;
    }

    public IFlourishTitlebarBuilder UseLogo(
        bool enabled = true,
        string? logoPath = null,
        bool showApplicationTitle = true,
        bool showApplicationSubTitle = true,
        bool showProjectTitle = false
    )
    {
        ThrowIfFrozen();
        options.LogoPath = logoPath is null
            ? null
            : ValidateNotBlank(logoPath, nameof(logoPath));
        options.IsTitlebarLogoEnabled = enabled;
        options.ShowApplicationTitleInLogoFlyout = showApplicationTitle;
        options.ShowApplicationSubtitleInLogoFlyout = showApplicationSubTitle;
        options.ShowProjectTitleInLogoFlyout = showProjectTitle;
        return this;
    }

    public IFlourishTitlebarBuilder InitApplicationTitle(string title = "MyApp")
    {
        ThrowIfFrozen();
        options.ApplicationTitle = ValidateNotBlank(title, nameof(title));
        options.IsTitlebarTitleEnabled = true;
        return this;
    }

    public IFlourishTitlebarBuilder InitApplicationSubTitle(string subTitle = "MyApp")
    {
        ThrowIfFrozen();
        options.ApplicationSubtitle = ValidateNotBlank(subTitle, nameof(subTitle));
        return this;
    }

    public IFlourishTitlebarBuilder InitUnnamedProjectPlaceholder(
        string placeholder = "Unnamed project"
    )
    {
        ThrowIfFrozen();
        options.UnnamedProjectPlaceholder = ValidateNotBlank(placeholder, nameof(placeholder));
        return this;
    }

    public IFlourishTitlebarBuilder UseProfile(
        bool enabled = true,
        NameOrder nameOrder = NameOrder.FirstLast,
        bool usePersistedPreference = true
    )
    {
        ThrowIfFrozen();
        ValidateEnum(nameOrder, nameof(nameOrder));
        options.Profile.NameOrder = nameOrder;
        options.IsProfileEnabled = enabled;
        options.IsTitlebarProfileEnabled = enabled;
        options.UsePersistedNameOrder = usePersistedPreference;
        return this;
    }

    public IFlourishTitlebarBuilder UseThemeToggle(
        bool enabled = true,
        FlourishTheme mode = FlourishTheme.System,
        bool usePersistedPreference = true
    )
    {
        ThrowIfFrozen();
        ValidateEnum(mode, nameof(mode));
        options.DefaultTheme = mode;
        options.IsThemeEnabled = enabled;
        options.IsTitlebarThemeToggleEnabled = enabled;
        options.UsePersistedTheme = usePersistedPreference;
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
