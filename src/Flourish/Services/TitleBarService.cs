using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;

namespace ArkheideSystem.Flourish.Services;

internal sealed class TitleBarService(FlourishShellOptions options) : ITitleBarService
{
    private readonly Lock gate = new();

    public event EventHandler<FlourishTitleBarChangedEventArgs>? Changed;

    public FlourishTitleBarState Current
    {
        get
        {
            lock (gate)
            {
                return CreateSnapshot();
            }
        }
    }

    public void SetApplicationTitle(string title)
    {
        title = ValidateRequired(title, nameof(title));
        Update(() =>
        {
            options.ApplicationTitle = title;
            options.IsTitlebarTitleEnabled = true;
        });
    }

    public void SetApplicationSubTitle(string? subTitle)
    {
        var normalized = subTitle?.Trim() ?? string.Empty;
        Update(() => options.ApplicationSubtitle = normalized);
    }

    public void SetApplicationIdentity(string title, string? subTitle = null)
    {
        title = ValidateRequired(title, nameof(title));
        var normalizedSubTitle = subTitle?.Trim() ?? string.Empty;
        Update(() =>
        {
            options.ApplicationTitle = title;
            options.ApplicationSubtitle = normalizedSubTitle;
            options.IsTitlebarTitleEnabled = true;
        });
    }

    public void SetUnnamedProjectPlaceholder(string placeholder)
    {
        placeholder = ValidateRequired(placeholder, nameof(placeholder));
        Update(() => options.UnnamedProjectPlaceholder = placeholder);
    }

    public void SetLogo(
        string? logoPath,
        string? fallbackText = null,
        bool showApplicationTitle = true,
        bool showApplicationSubTitle = true,
        bool showProjectTitle = false
    )
    {
        var normalizedPath = string.IsNullOrWhiteSpace(logoPath) ? null : logoPath.Trim();
        var normalizedFallback = string.IsNullOrWhiteSpace(fallbackText)
            ? options.LogoFallbackText
            : fallbackText.Trim();
        Update(() =>
        {
            options.LogoPath = normalizedPath;
            options.LogoFallbackText = normalizedFallback;
            options.IsTitlebarLogoEnabled = true;
            options.ShowApplicationTitleInLogoFlyout = showApplicationTitle;
            options.ShowApplicationSubtitleInLogoFlyout = showApplicationSubTitle;
            options.ShowProjectTitleInLogoFlyout = showProjectTitle;
        });
    }

    [Obsolete("Use SetApplicationTitle.")]
    public void SetTitle(string title) => SetApplicationTitle(title);

    [Obsolete("Use SetApplicationSubTitle.")]
    public void SetSubtitle(string? subtitle) => SetApplicationSubTitle(subtitle);

    [Obsolete("Use SetApplicationIdentity.")]
    public void SetIdentity(string title, string? subtitle = null) =>
        SetApplicationIdentity(title, subtitle);

    public void SetSearchPlaceholder(string placeholder)
    {
        placeholder = ValidateRequired(placeholder, nameof(placeholder));
        Update(() => options.SearchPlaceholder = placeholder);
    }

    public void SetElementVisible(TitleBarElement element, bool visible)
    {
        if (!Enum.IsDefined(element))
        {
            throw new ArgumentOutOfRangeException(
                nameof(element),
                element,
                "Unknown title bar element."
            );
        }

        Update(() =>
        {
            switch (element)
            {
                case TitleBarElement.Search:
                    options.IsTitlebarSearchEnabled = visible;
                    break;
                case TitleBarElement.Breadcrumb:
                    options.IsBreadcrumbEnabled = visible;
                    break;
                case TitleBarElement.NavigationToggle:
                    options.IsTitlebarNavigationToggleEnabled = visible;
                    break;
                case TitleBarElement.Logo:
                    options.IsTitlebarLogoEnabled = visible;
                    break;
                case TitleBarElement.Title:
                    options.IsTitlebarTitleEnabled = visible;
                    break;
#pragma warning disable CS0618
                case TitleBarElement.Subtitle:
                    options.ShowApplicationSubtitleInLogoFlyout = visible;
                    break;
#pragma warning restore CS0618
                case TitleBarElement.ThemeToggle:
                    options.IsTitlebarThemeToggleEnabled = visible;
                    break;
                case TitleBarElement.Profile:
                    options.IsTitlebarProfileEnabled = visible;
                    break;
            }
        });
    }

    public void SetBreadcrumbMode(BreadcrumbShowOption mode)
    {
        if (!Enum.IsDefined(mode))
        {
            throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unknown breadcrumb mode.");
        }

        Update(() =>
        {
            options.BreadcrumbShowOption = mode;
            options.IsBreadcrumbEnabled = mode != BreadcrumbShowOption.Hidden;
        });
    }

    private void Update(Action update)
    {
        FlourishTitleBarState state;
        lock (gate)
        {
            var previous = CreateSnapshot();
            update();
            state = CreateSnapshot();
            if (state == previous)
            {
                return;
            }
        }

        Changed?.Invoke(this, new FlourishTitleBarChangedEventArgs(state));
    }

    private FlourishTitleBarState CreateSnapshot()
    {
        return new FlourishTitleBarState(
            options.ApplicationTitle,
            options.ApplicationSubtitle,
            options.UnnamedProjectPlaceholder,
            options.SearchPlaceholder,
            options.LogoPath,
            options.LogoFallbackText,
            options.ShowApplicationTitleInLogoFlyout,
            options.ShowApplicationSubtitleInLogoFlyout,
            options.ShowProjectTitleInLogoFlyout,
            options.IsTitlebarSearchEnabled,
            options.IsBreadcrumbEnabled,
            options.IsTitlebarNavigationToggleEnabled,
            options.IsTitlebarLogoEnabled,
            options.IsTitlebarTitleEnabled || options.IsMultiProjectEnabled,
            options.IsTitlebarThemeToggleEnabled,
            options.IsTitlebarProfileEnabled,
            options.BreadcrumbShowOption
        );
    }

    private static string ValidateRequired(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be empty.", parameterName);
        }

        return value.Trim();
    }
}
