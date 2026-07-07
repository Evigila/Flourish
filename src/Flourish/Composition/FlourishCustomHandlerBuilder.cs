using System.Windows;
using AckSS.Flourish.Abstract;
using AckSS.Flourish.Configuration;

namespace AckSS.Flourish.Composition;

internal sealed class FlourishCustomHandlerBuilder(FlourishShellOptions options)
    : IFlourishCustomHandlerBuilder
{
    public IFlourishCustomHandlerBuilder Add(
        FlourishRegion region,
        Func<IServiceProvider, FrameworkElement> contentFactory,
        int order = 0
    )
    {
        ArgumentNullException.ThrowIfNull(contentFactory);
        options.RegionContents.Add(new FlourishRegionContent(region, contentFactory, order));
        return this;
    }

    public IFlourishCustomHandlerBuilder Add(
        FlourishRegion region,
        Func<FrameworkElement> contentFactory,
        int order = 0
    )
    {
        ArgumentNullException.ThrowIfNull(contentFactory);
        return Add(region, _ => contentFactory(), order);
    }

    public IFlourishCustomHandlerBuilder Add(
        FlourishRegion region,
        FrameworkElement content,
        int order = 0
    )
    {
        ArgumentNullException.ThrowIfNull(content);
        return Add(region, _ => content, order);
    }

    public IFlourishCustomHandlerBuilder SetProfileContent(
        Func<IServiceProvider, FrameworkElement> contentFactory
    )
    {
        ArgumentNullException.ThrowIfNull(contentFactory);
        options.IsTitlebarProfileEnabled = true;
        options.RegionContents.RemoveAll(existing =>
            existing.Region == FlourishRegion.TitlebarProfile
        );
        options.RegionContents.Add(
            new FlourishRegionContent(FlourishRegion.TitlebarProfile, contentFactory)
        );
        return this;
    }

    public IFlourishCustomHandlerBuilder SetProfileContent(
        Func<FrameworkElement> contentFactory
    )
    {
        ArgumentNullException.ThrowIfNull(contentFactory);
        return SetProfileContent(_ => contentFactory());
    }

    public IFlourishCustomHandlerBuilder SetProfileContent(FrameworkElement content)
    {
        ArgumentNullException.ThrowIfNull(content);
        return SetProfileContent(_ => content);
    }

    public IFlourishCustomHandlerBuilder AddTitlebarAction(
        string displayName,
        string iconGlyph,
        string? commandKey,
        int order = 0
    )
    {
        displayName = ValidateNotBlank(displayName, nameof(displayName));
        return Add(
            FlourishRegion.TitlebarEnd,
            services => FlourishRegionElementFactory.CreateTitlebarActionButton(
                services,
                displayName,
                iconGlyph,
                commandKey,
                action: null
            ),
            order
        );
    }

    public IFlourishCustomHandlerBuilder AddTitlebarActionHandler(
        string displayName,
        string iconGlyph,
        Action<IServiceProvider> action,
        int order = 0
    )
    {
        displayName = ValidateNotBlank(displayName, nameof(displayName));
        ArgumentNullException.ThrowIfNull(action);
        return Add(
            FlourishRegion.TitlebarEnd,
            services => FlourishRegionElementFactory.CreateTitlebarActionButton(
                services,
                displayName,
                iconGlyph,
                commandKey: null,
                action
            ),
            order
        );
    }

    public IFlourishCustomHandlerBuilder AddFooterContent(
        Func<IServiceProvider, FrameworkElement> contentFactory,
        int order = 0
    )
    {
        return AddFooterContent(FlourishRegion.FooterEnd, contentFactory, order);
    }

    public IFlourishCustomHandlerBuilder AddFooterContent(
        FlourishRegion region,
        Func<IServiceProvider, FrameworkElement> contentFactory,
        int order = 0
    )
    {
        ValidateFooterRegion(region, nameof(region));
        return Add(region, contentFactory, order);
    }

    public IFlourishCustomHandlerBuilder AddFooterContent(
        Func<FrameworkElement> contentFactory,
        int order = 0
    )
    {
        ArgumentNullException.ThrowIfNull(contentFactory);
        return AddFooterContent(FlourishRegion.FooterEnd, _ => contentFactory(), order);
    }

    public IFlourishCustomHandlerBuilder AddFooterContent(
        FlourishRegion region,
        Func<FrameworkElement> contentFactory,
        int order = 0
    )
    {
        ArgumentNullException.ThrowIfNull(contentFactory);
        return AddFooterContent(region, _ => contentFactory(), order);
    }

    public IFlourishCustomHandlerBuilder AddFooterContent(
        FrameworkElement content,
        int order = 0
    )
    {
        ArgumentNullException.ThrowIfNull(content);
        return AddFooterContent(FlourishRegion.FooterEnd, _ => content, order);
    }

    public IFlourishCustomHandlerBuilder AddFooterContent(
        FlourishRegion region,
        FrameworkElement content,
        int order = 0
    )
    {
        ArgumentNullException.ThrowIfNull(content);
        return AddFooterContent(region, _ => content, order);
    }

    public IFlourishCustomHandlerBuilder AddFooterCommand(
        string displayText,
        string iconGlyph,
        string? commandKey,
        int order = 0
    )
    {
        return AddFooterCommand(FlourishRegion.FooterEnd, displayText, iconGlyph, commandKey, order);
    }

    public IFlourishCustomHandlerBuilder AddFooterCommand(
        FlourishRegion region,
        string displayText,
        string iconGlyph,
        string? commandKey,
        int order = 0
    )
    {
        displayText = ValidateNotBlank(displayText, nameof(displayText));
        return AddFooterContent(
            region,
            services => FlourishRegionElementFactory.CreateFooterCommandButton(
                services,
                displayText,
                iconGlyph,
                commandKey,
                action: null
            ),
            order
        );
    }

    public IFlourishCustomHandlerBuilder AddFooterCommandHandler(
        string displayText,
        string iconGlyph,
        Action<IServiceProvider> action,
        int order = 0
    )
    {
        return AddFooterCommandHandler(
            FlourishRegion.FooterEnd,
            displayText,
            iconGlyph,
            action,
            order
        );
    }

    public IFlourishCustomHandlerBuilder AddFooterCommandHandler(
        FlourishRegion region,
        string displayText,
        string iconGlyph,
        Action<IServiceProvider> action,
        int order = 0
    )
    {
        displayText = ValidateNotBlank(displayText, nameof(displayText));
        ArgumentNullException.ThrowIfNull(action);
        return AddFooterContent(
            region,
            services => FlourishRegionElementFactory.CreateFooterCommandButton(
                services,
                displayText,
                iconGlyph,
                commandKey: null,
                action
            ),
            order
        );
    }

    private static void ValidateFooterRegion(FlourishRegion region, string parameterName)
    {
        if (region is FlourishRegion.FooterStart or FlourishRegion.FooterEnd)
        {
            return;
        }

        throw new ArgumentOutOfRangeException(
            parameterName,
            region,
            "Footer content must use FlourishRegion.FooterStart or FlourishRegion.FooterEnd."
        );
    }

    private static string ValidateNotBlank(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be empty.", parameterName);
        }

        return value;
    }
}
