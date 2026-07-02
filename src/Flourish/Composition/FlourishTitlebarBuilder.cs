using System.Windows.Media.Imaging;
using AcksheedSys.Flourish.Abstract;
using AcksheedSys.Flourish.Models;

namespace AcksheedSys.Flourish.Composition;

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

    public IFlourishTitlebarBuilder SetTrayExit(bool enabled = false)
    {
        options.IsTrayExitEnabled = enabled;
        return this;
    }

    public IFlourishTitlebarBuilder SetTitle(string title)
    {
        options.Title = title;
        return this;
    }

    public IFlourishTitlebarBuilder SetSubtitle(string subtitle)
    {
        options.Subtitle = subtitle;
        return this;
    }

    public IFlourishTitlebarBuilder SetLogo(string packUri)
    {
        options.LogoSource = new BitmapImage(new Uri(packUri, UriKind.RelativeOrAbsolute));
        return this;
    }

    public IFlourishTitlebarBuilder SetSearchPlaceholder(string placeholder)
    {
        options.SearchPlaceholder = placeholder;
        return this;
    }

    public IFlourishTitlebarBuilder SetBreadcrumbBehavior(
        BreadcrumbShowOption behavior = BreadcrumbShowOption.Auto
    )
    {
        options.BreadcrumbShowOption = behavior;
        return this;
    }
}
