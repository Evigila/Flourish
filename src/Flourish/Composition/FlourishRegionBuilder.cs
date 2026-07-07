using System.Windows;
using AckSS.Flourish.Abstract;
using AckSS.Flourish.Configuration;

namespace AckSS.Flourish.Composition;

internal sealed class FlourishRegionBuilder(FlourishShellOptions options)
    : IFlourishRegionBuilder
{
    public IFlourishRegionBuilder Add(
        FlourishRegion region,
        Func<IServiceProvider, FrameworkElement> contentFactory,
        int order = 0
    )
    {
        EnableRegionHost(region);
        options.RegionContents.Add(new FlourishRegionContent(region, contentFactory, order));
        return this;
    }

    public IFlourishRegionBuilder Add(
        FlourishRegion region,
        Func<FrameworkElement> contentFactory,
        int order = 0
    )
    {
        ArgumentNullException.ThrowIfNull(contentFactory);
        return Add(region, _ => contentFactory(), order);
    }

    public IFlourishRegionBuilder Add(
        FlourishRegion region,
        FrameworkElement content,
        int order = 0
    )
    {
        ArgumentNullException.ThrowIfNull(content);
        return Add(region, _ => content, order);
    }

    private void EnableRegionHost(FlourishRegion region)
    {
        switch (region)
        {
            case FlourishRegion.TitlebarStart:
            case FlourishRegion.TitlebarCenter:
            case FlourishRegion.TitlebarEnd:
            case FlourishRegion.TitlebarProfile:
                options.IsTitlebarEnabled = true;
                break;
            case FlourishRegion.NavigationHeader:
            case FlourishRegion.NavigationFooter:
                options.IsNavigationPanelEnabled = true;
                break;
            case FlourishRegion.ToolbarStart:
            case FlourishRegion.ToolbarEnd:
                options.IsDynamicToolbarEnabled = true;
                break;
            case FlourishRegion.StatusStart:
            case FlourishRegion.StatusEnd:
                options.IsStatusBarEnabled = true;
                break;
        }
    }
}
