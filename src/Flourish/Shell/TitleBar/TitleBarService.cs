using System;
using System.Threading;

using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Projects;

namespace ArkheideSystem.Flourish.Shell.TitleBar;

internal sealed class TitleBarService
{
    private readonly Lock gate = new();
    private readonly FlourishTitleBarOptions options;
    private readonly FlourishProjectOptions projectOptions;
    private FlourishTitleBarState current;
    private long version;

    public TitleBarService(
        FlourishTitleBarOptions options,
        FlourishProjectOptions projectOptions
    )
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        this.projectOptions =
            projectOptions ?? throw new ArgumentNullException(nameof(projectOptions));
        current = CreateSnapshot();
    }

    public event EventHandler<FlourishStateChangedEventArgs<FlourishTitleBarState>>? Changed;

    internal long CurrentVersion => Volatile.Read(ref current).Version;

    public FlourishTitleBarState Current => Volatile.Read(ref current);

    public void SetEnabled(bool enabled)
    {
        Update(() => options.IsTitlebarEnabled = enabled);
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

    public void SetApplicationSubtitle(string? subtitle)
    {
        var normalized = subtitle?.Trim() ?? string.Empty;
        Update(() => options.ApplicationSubtitle = normalized);
    }

    public void SetApplicationIdentity(string title, string? subtitle = null)
    {
        title = ValidateRequired(title, nameof(title));
        var normalizedSubtitle = subtitle?.Trim() ?? string.Empty;
        Update(() =>
        {
            options.ApplicationTitle = title;
            options.ApplicationSubtitle = normalizedSubtitle;
            options.IsTitlebarTitleEnabled = true;
        });
    }

    public void SetUnnamedProjectPlaceholder(string placeholder)
    {
        placeholder = ValidateRequired(placeholder, nameof(placeholder));
        Update(() => projectOptions.UnnamedProjectPlaceholder = placeholder);
    }

    public void SetLogo(
        string? logoPath,
        string? fallbackText = null,
        bool showApplicationTitle = true,
        bool showApplicationSubtitle = true,
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
            options.ShowApplicationSubtitleInLogoFlyout = showApplicationSubtitle;
            options.ShowProjectTitleInLogoFlyout = showProjectTitle;
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
            var previous = current;
            update();
            if (MatchesOptions(previous))
            {
                return;
            }

            version++;
            state = CreateSnapshot();
            Volatile.Write(ref current, state);
        }

        Changed?.Invoke(this, new FlourishStateChangedEventArgs<FlourishTitleBarState>(state));
    }

    private FlourishTitleBarState CreateSnapshot()
    {
        return new FlourishTitleBarState(
            options.ApplicationTitle,
            options.ApplicationSubtitle,
            projectOptions.UnnamedProjectPlaceholder,
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
            options.IsTitlebarTitleEnabled || projectOptions.IsMultiProjectEnabled,
            options.IsTitlebarThemeToggleEnabled,
            options.IsTitlebarProfileEnabled,
            options.BreadcrumbShowOption
        )
        {
            IsEnabled = options.IsTitlebarEnabled,
            Version = version,
        };
    }

    private bool MatchesOptions(FlourishTitleBarState state)
    {
        return state.ApplicationTitle == options.ApplicationTitle
            && state.ApplicationSubtitle == options.ApplicationSubtitle
            && state.UnnamedProjectPlaceholder == projectOptions.UnnamedProjectPlaceholder
            && state.SearchPlaceholder == options.SearchPlaceholder
            && state.LogoPath == options.LogoPath
            && state.LogoFallbackText == options.LogoFallbackText
            && state.ShowApplicationTitle == options.ShowApplicationTitleInLogoFlyout
            && state.ShowApplicationSubtitle == options.ShowApplicationSubtitleInLogoFlyout
            && state.ShowProjectTitle == options.ShowProjectTitleInLogoFlyout
            && state.IsSearchVisible == options.IsTitlebarSearchEnabled
            && state.IsBreadcrumbVisible == options.IsBreadcrumbEnabled
            && state.IsNavigationToggleVisible == options.IsTitlebarNavigationToggleEnabled
            && state.IsLogoVisible == options.IsTitlebarLogoEnabled
            && state.IsTitleVisible
                == (options.IsTitlebarTitleEnabled || projectOptions.IsMultiProjectEnabled)
            && state.IsThemeToggleVisible == options.IsTitlebarThemeToggleEnabled
            && state.IsProfileVisible == options.IsTitlebarProfileEnabled
            && state.BreadcrumbMode == options.BreadcrumbShowOption
            && state.IsEnabled == options.IsTitlebarEnabled;
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
