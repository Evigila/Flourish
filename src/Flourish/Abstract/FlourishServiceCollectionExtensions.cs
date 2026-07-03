using System.Windows.Controls;
using AcksheedSys.Flourish.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AcksheedSys.Flourish.Abstract;

/// <summary>
/// Provides service collection extensions used by Flourish applications.
/// </summary>
/// <example>
/// <code><![CDATA[
/// services.AddNavigable<HomePage>("Home", "\uE80F", isInitial: true);
/// ]]></code>
/// </example>
public static class FlourishServiceCollectionExtensions
{
    /// <summary>
    /// Registers a WPF page as a navigable Flourish page.
    /// </summary>
    /// <typeparam name="TPage">The page type to register.</typeparam>
    /// <param name="services">The service collection that receives the page registration.</param>
    /// <param name="displayName">The display name shown in navigation UI.</param>
    /// <param name="iconGlyph">The icon glyph shown in navigation UI.</param>
    /// <param name="isInitial">A value indicating whether this page should be used as the initial page.</param>
    /// <param name="cacheMode">The page caching mode used for this page.</param>
    /// <returns>The same service collection for chained registration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// services.AddNavigable<HomePage>(
    ///     displayName: "Home",
    ///     iconGlyph: "\uE80F",
    ///     isInitial: true,
    ///     cacheMode: FlourishPageCacheMode.Enabled);
    /// ]]></code>
    /// </example>
    public static IServiceCollection AddNavigable<TPage>(
        this IServiceCollection services,
        string displayName,
        string iconGlyph,
        bool isInitial = false,
        FlourishPageCacheMode cacheMode = FlourishPageCacheMode.Enabled
    )
        where TPage : Page
    {
        services.AddTransient<TPage>();
        GetOrCreateState(services)
            .NavigablePages.Add(
                new NavigablePageRegistration(
                    typeof(TPage),
                    displayName,
                    iconGlyph,
                    isInitial,
                    cacheMode
                )
            );

        return services;
    }

    /// <summary>
    /// Registers a WPF page type as a navigable Flourish page.
    /// </summary>
    /// <param name="services">The service collection that receives the page registration.</param>
    /// <param name="pageType">The page type to register.</param>
    /// <param name="displayName">The display name shown in navigation UI.</param>
    /// <param name="iconGlyph">The icon glyph shown in navigation UI.</param>
    /// <param name="isInitial">A value indicating whether this page should be used as the initial page.</param>
    /// <param name="cacheMode">The page caching mode used for this page.</param>
    /// <returns>The same service collection for chained registration.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="pageType" /> does not derive from <see cref="Page" />.</exception>
    /// <example>
    /// <code><![CDATA[
    /// services.AddNavigable(
    ///     typeof(SettingsPage),
    ///     displayName: "Settings",
    ///     iconGlyph: "\uE713");
    /// ]]></code>
    /// </example>
    public static IServiceCollection AddNavigable(
        this IServiceCollection services,
        Type pageType,
        string displayName,
        string iconGlyph,
        bool isInitial = false,
        FlourishPageCacheMode cacheMode = FlourishPageCacheMode.Enabled
    )
    {
        if (!typeof(Page).IsAssignableFrom(pageType))
        {
            throw new ArgumentException(
                $"{pageType.FullName} must derive from System.Windows.Controls.Page.",
                nameof(pageType)
            );
        }

        services.AddTransient(pageType);
        GetOrCreateState(services)
            .NavigablePages.Add(
                new NavigablePageRegistration(pageType, displayName, iconGlyph, isInitial, cacheMode)
            );

        return services;
    }

    private static FlourishServiceCollectionState GetOrCreateState(IServiceCollection services)
    {
        var descriptor = services.FirstOrDefault(descriptor =>
            descriptor.ServiceType == typeof(FlourishServiceCollectionState)
            && descriptor.ImplementationInstance is FlourishServiceCollectionState
        );

        if (descriptor?.ImplementationInstance is FlourishServiceCollectionState state)
        {
            return state;
        }

        state = new FlourishServiceCollectionState();
        services.AddSingleton(state);
        return state;
    }
}
