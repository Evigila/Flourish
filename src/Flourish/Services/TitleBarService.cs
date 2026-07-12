using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;

namespace ArkheideSystem.Flourish.Services;

internal sealed class TitleBarService(FlourishShellOptions options) : ITitleBarService
{
    private readonly object gate = new();

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

    public void SetTitle(string title)
    {
        title = ValidateRequired(title, nameof(title));
        Update(() =>
        {
            options.Title = title;
            options.IsTitlebarTitleEnabled = true;
        });
    }

    public void SetSubtitle(string? subtitle)
    {
        var normalized = subtitle?.Trim() ?? string.Empty;
        Update(() =>
        {
            options.Subtitle = normalized;
            options.IsTitlebarSubtitleEnabled = normalized.Length > 0;
        });
    }

    public void SetIdentity(string title, string? subtitle = null)
    {
        title = ValidateRequired(title, nameof(title));
        var normalizedSubtitle = subtitle?.Trim() ?? string.Empty;
        Update(() =>
        {
            options.Title = title;
            options.Subtitle = normalizedSubtitle;
            options.IsTitlebarTitleEnabled = true;
            options.IsTitlebarSubtitleEnabled = normalizedSubtitle.Length > 0;
        });
    }

    public void SetLogo(string? logoPath, string? fallbackText = null)
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
        });
    }

    public void SetSearchPlaceholder(string placeholder)
    {
        placeholder = ValidateRequired(placeholder, nameof(placeholder));
        Update(() => options.SearchPlaceholder = placeholder);
    }

    public void SetElementVisible(TitleBarElement element, bool visible)
    {
        if (!Enum.IsDefined(element))
        {
            throw new ArgumentOutOfRangeException(nameof(element), element, "Unknown title bar element.");
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
                case TitleBarElement.Subtitle:
                    options.IsTitlebarSubtitleEnabled = visible;
                    break;
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
            options.Title,
            options.Subtitle,
            options.SearchPlaceholder,
            options.LogoPath,
            options.LogoFallbackText,
            options.IsTitlebarSearchEnabled,
            options.IsBreadcrumbEnabled,
            options.IsTitlebarNavigationToggleEnabled,
            options.IsTitlebarLogoEnabled,
            options.IsTitlebarTitleEnabled,
            options.IsTitlebarSubtitleEnabled,
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
