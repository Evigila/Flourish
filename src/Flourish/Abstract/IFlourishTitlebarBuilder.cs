namespace AcksheedSys.Flourish.Abstract;

public interface IFlourishTitlebarBuilder
{
    IFlourishTitlebarBuilder ShowSearch(bool enabled = true);

    IFlourishTitlebarBuilder ShowBreadcrumb(bool enabled = true);

    IFlourishTitlebarBuilder ShowNavToggle(bool enabled = true);

    IFlourishTitlebarBuilder ShowLogo(bool enabled = true);

    IFlourishTitlebarBuilder ShowTitle(bool enabled = true);

    IFlourishTitlebarBuilder ShowSubTitle(bool enabled = true);

    IFlourishTitlebarBuilder ShowProfile(bool enabled = true);

    IFlourishTitlebarBuilder SetTrayExit(bool enabled = false);

    IFlourishTitlebarBuilder SetTitle(string title);

    IFlourishTitlebarBuilder SetSubtitle(string subtitle);

    IFlourishTitlebarBuilder SetLogo(string packUri);

    IFlourishTitlebarBuilder SetSearchPlaceholder(string placeholder);

    IFlourishTitlebarBuilder SetBreadcrumbBehavior(
        BreadcrumbShowOption behavior = BreadcrumbShowOption.Auto
    );
}
