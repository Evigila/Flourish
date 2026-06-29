using System.Windows.Media.Imaging;
using AcksheedSys.Flourish.Abstract;
using AcksheedSys.Flourish.Models;

namespace AcksheedSys.Flourish.Composition;

internal sealed class FlourishShellBuilder(FlourishShellOptions options) : IFlourishShellBuilder
{
    public IFlourishShellBuilder SetTitle(string title)
    {
        options.Title = title;
        return this;
    }

    public IFlourishShellBuilder SetSubtitle(string subtitle)
    {
        options.Subtitle = subtitle;
        return this;
    }

    public IFlourishShellBuilder SetLogo(string packUri)
    {
        options.LogoSource = new BitmapImage(new Uri(packUri, UriKind.RelativeOrAbsolute));
        return this;
    }

    public IFlourishShellBuilder UseNavigationPanel(
        bool enabled = true,
        NavigationPanelDirection direction = NavigationPanelDirection.Left,
        string title = "Navigation"
    )
    {
        options.IsNavigationPanelEnabled = enabled;
        options.NavigationPanelDirection = direction;
        options.PaneTitle = title;
        return this;
    }

    public IFlourishShellBuilder UseSearchOnTitlebar(
        bool enabled = true,
        string placeholder = "Search"
    )
    {
        options.IsTitlebarSearchEnabled = enabled;
        options.SearchPlaceholder = placeholder;
        return this;
    }

    public IFlourishShellBuilder UseDynamicToolbar(bool enabled = true)
    {
        options.IsDynamicToolbarEnabled = enabled;
        return this;
    }

    public IFlourishShellBuilder UseBreadcrumb(
        bool enabled = true,
        BreadcrumbShowOption mode = BreadcrumbShowOption.OnlyAvailable
    )
    {
        options.IsBreadcrumbEnabled = enabled;
        options.BreadcrumbShowOption = mode;
        return this;
    }
}
