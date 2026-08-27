using System;

using ArkheideSystem.Flourish.Abstract;
using System.Windows.Controls;
using ArkheideSystem.Flourish.Appearance;
using ArkheideSystem.Flourish.Configuration;
using ArkheideSystem.Flourish.Profile;
using ArkheideSystem.Flourish.Projects;

namespace ArkheideSystem.Flourish.Shell.TitleBar;

internal sealed class TitleBarBuilder(
    TitleBarOptions options,
    ProjectOptions projectOptions,
    AppearanceOptions appearanceOptions,
    ProfileOptions profileOptions
)
    : BuilderMutationGuard,
        ITitleBarBuilder
{
    public ITitleBarBuilder SetEnabled(bool enabled = true)
    {
        ThrowIfFrozen();
        options.IsTitlebarEnabled = enabled;
        return this;
    }

    public ITitleBarBuilder SetSearch(
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

    public ITitleBarBuilder SetBreadcrumbMode(
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

    public ITitleBarBuilder SetNavigationToggle(bool enabled = true)
    {
        ThrowIfFrozen();
        options.IsTitlebarNavigationToggleEnabled = enabled;
        return this;
    }

    public ITitleBarBuilder SetLogo(
        bool enabled = true,
        string? logoPath = null,
        bool showApplicationTitle = true,
        bool showApplicationSubtitle = true,
        bool showProjectTitle = false
    )
    {
        ThrowIfFrozen();
        options.LogoPath = logoPath is null ? null : ValidateNotBlank(logoPath, nameof(logoPath));
        options.IsTitlebarLogoEnabled = enabled;
        options.ShowApplicationTitleInLogoFlyout = showApplicationTitle;
        options.ShowApplicationSubtitleInLogoFlyout = showApplicationSubtitle;
        options.ShowProjectTitleInLogoFlyout = showProjectTitle;
        return this;
    }

    public ITitleBarBuilder SetApplicationTitle(string title = "MyApp")
    {
        ThrowIfFrozen();
        options.ApplicationTitle = ValidateNotBlank(title, nameof(title));
        options.IsTitlebarTitleEnabled = true;
        return this;
    }

    public ITitleBarBuilder SetApplicationSubtitle(string subtitle = "MyApp")
    {
        ThrowIfFrozen();
        options.ApplicationSubtitle = ValidateNotBlank(subtitle, nameof(subtitle));
        return this;
    }

    public ITitleBarBuilder SetUnnamedProjectPlaceholder(
        string placeholder = "Unnamed project"
    )
    {
        ThrowIfFrozen();
        projectOptions.UnnamedProjectPlaceholder = ValidateNotBlank(placeholder, nameof(placeholder));
        return this;
    }

    public ITitleBarBuilder SetProfile(
        bool enabled = true,
        NameOrder nameOrder = NameOrder.FirstLast,
        bool usePersistedPreference = true
    )
    {
        ThrowIfFrozen();
        ValidateEnum(nameOrder, nameof(nameOrder));
        profileOptions.NameOrder = nameOrder;
        profileOptions.IsProfileEnabled = enabled;
        options.IsTitlebarProfileEnabled = enabled;
        profileOptions.UsePersistedNameOrder = usePersistedPreference;
        return this;
    }

    public ITitleBarBuilder SetProfilePage<TPage>()
        where TPage : Page
    {
        ThrowIfFrozen();
        profileOptions.PageType = typeof(TPage);
        return this;
    }

    public ITitleBarBuilder SetThemeToggle(
        bool enabled = true,
        ApplicationTheme mode = ApplicationTheme.System,
        bool usePersistedPreference = true
    )
    {
        ThrowIfFrozen();
        ValidateEnum(mode, nameof(mode));
        appearanceOptions.DefaultTheme = mode;
        appearanceOptions.IsThemeEnabled = enabled;
        options.IsTitlebarThemeToggleEnabled = enabled;
        appearanceOptions.UsePersistedTheme = usePersistedPreference;
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
