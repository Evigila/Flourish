using System;

using ArkheideSystem.Flourish.Abstract;
using System.Windows;
using ArkheideSystem.Flourish.Configuration;
using ArkheideSystem.Flourish.Shell.Regions;

namespace ArkheideSystem.Flourish.Views.Windows;

internal sealed class CustomContentBuilder(FlourishRegionOptions options)
    : FlourishBuilderMutationGuard,
        ICustomContentBuilder
{
    public ICustomContentBuilder AddRegionContent(
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

    public ICustomContentBuilder SetProfileContent(
        Func<IServiceProvider, FrameworkElement> contentFactory
    )
    {
        ThrowIfFrozen();
        ArgumentNullException.ThrowIfNull(contentFactory);
        options.RegionContents.RemoveAll(existing =>
            existing.Region == FlourishRegion.TitleBarProfile
        );
        options.RegionContents.Add(
            new FlourishRegionContent(FlourishRegion.TitleBarProfile, contentFactory)
        );
        return this;
    }

    public ICustomContentBuilder AddTitleBarAction(
        string displayName,
        string iconGlyph,
        string? commandKey,
        int order = 0
    )
    {
        ThrowIfFrozen();
        displayName = ValidateNotBlank(displayName, nameof(displayName));
        return AddRegionContent(
            FlourishRegion.TitleBarEnd,
            services =>
                FlourishRegionElementFactory.CreateTitlebarActionButton(
                    services,
                    displayName,
                    iconGlyph,
                    commandKey,
                    action: null
                ),
            order
        );
    }

    public ICustomContentBuilder AddTitleBarActionHandler(
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
            FlourishRegion.TitleBarEnd,
            services =>
                FlourishRegionElementFactory.CreateTitlebarActionButton(
                    services,
                    displayName,
                    iconGlyph,
                    commandKey: null,
                    action
                ),
            order
        );
    }

    public ICustomContentBuilder AddFooterCommand(
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
            services =>
                FlourishRegionElementFactory.CreateFooterCommandButton(
                    services,
                    displayText,
                    iconGlyph,
                    commandKey,
                    action: null
                ),
            order
        );
    }

    public ICustomContentBuilder AddFooterCommandHandler(
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
            services =>
                FlourishRegionElementFactory.CreateFooterCommandButton(
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
