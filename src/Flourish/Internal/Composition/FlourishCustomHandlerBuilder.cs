using System.Windows;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;

namespace ArkheideSystem.Flourish.Internal.Composition;

internal sealed class FlourishCustomHandlerBuilder(FlourishShellOptions options)
    : FlourishBuilderMutationGuard,
        IFlourishCustomHandlerBuilder
{
    public IFlourishCustomHandlerBuilder AddRegionContent(
        FlourishRegion region,
        Func<IServiceProvider, FrameworkElement> contentFactory,
        int order = 0
    )
    {
        ThrowIfFrozen();
        ArgumentNullException.ThrowIfNull(contentFactory);
        options.RegionContents.Add(new FlourishRegionContent(region, contentFactory, order));
        return this;
    }

    public IFlourishCustomHandlerBuilder InitProfileContent(
        Func<IServiceProvider, FrameworkElement> contentFactory
    )
    {
        ThrowIfFrozen();
        ArgumentNullException.ThrowIfNull(contentFactory);
        options.RegionContents.RemoveAll(existing =>
            existing.Region == FlourishRegion.TitlebarProfile
        );
        options.RegionContents.Add(
            new FlourishRegionContent(FlourishRegion.TitlebarProfile, contentFactory)
        );
        return this;
    }

    public IFlourishCustomHandlerBuilder AddTitleBarAction(
        string displayName,
        string iconGlyph,
        string? commandKey,
        int order = 0
    )
    {
        ThrowIfFrozen();
        displayName = ValidateNotBlank(displayName, nameof(displayName));
        return AddRegionContent(
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

    public IFlourishCustomHandlerBuilder AddTitleBarActionHandler(
        string displayName,
        string iconGlyph,
        Action<IServiceProvider> action,
        int order = 0
    )
    {
        ThrowIfFrozen();
        displayName = ValidateNotBlank(displayName, nameof(displayName));
        ArgumentNullException.ThrowIfNull(action);
        return AddRegionContent(
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

    public IFlourishCustomHandlerBuilder AddFooterCommand(
        FlourishRegion region,
        string displayText,
        string iconGlyph,
        string? commandKey,
        int order = 0
    )
    {
        ThrowIfFrozen();
        ValidateFooterRegion(region, nameof(region));
        displayText = ValidateNotBlank(displayText, nameof(displayText));
        return AddRegionContent(
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
        FlourishRegion region,
        string displayText,
        string iconGlyph,
        Action<IServiceProvider> action,
        int order = 0
    )
    {
        ThrowIfFrozen();
        ValidateFooterRegion(region, nameof(region));
        displayText = ValidateNotBlank(displayText, nameof(displayText));
        ArgumentNullException.ThrowIfNull(action);
        return AddRegionContent(
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
