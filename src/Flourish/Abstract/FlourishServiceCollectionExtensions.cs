using System.Windows.Controls;
using AckSS.Flourish.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AckSS.Flourish.Abstract;

/// <summary>
/// Provides service collection extensions used by Flourish applications.
/// </summary>
/// <example>
/// <code><![CDATA[
/// services.AddNavigable<HomePage>(
///     displayName: "Home",
///     iconGlyph: "\uE80F",
///     cacheMode: FlourishPageCacheMode.Enabled);
/// ]]></code>
/// </example>
public static class FlourishServiceCollectionExtensions
{
    /// <summary>
    /// Registers a WPF page as a navigable Flourish page.
    /// </summary>
    /// <remarks>
    /// This method records the page display name, icon glyph, and cache mode. It does not add
    /// the page to a visible navigation position by itself. Use
    /// <see cref="IFlourishNavigationGroupBuilder.AddNavigableViewItem{TPage}" /> or
    /// <see cref="IFlourishNavigationBuilder.AddFixedNavigableViewItem{TPage}" /> inside
    /// <c>ConfigureNavigation</c> to decide where the registered page is displayed.
    /// </remarks>
    /// <typeparam name="TPage">The page type to register.</typeparam>
    /// <param name="services">The service collection that receives the page registration.</param>
    /// <param name="displayName">The display name used when the page is displayed in navigation UI.</param>
    /// <param name="iconGlyph">The icon glyph used when the page is displayed in navigation UI.</param>
    /// <param name="cacheMode">The page caching mode used for this page.</param>
    /// <returns>The same service collection for chained registration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// services.AddNavigable<HomePage>(
    ///     displayName: "Home",
    ///     iconGlyph: "\uE80F",
    ///     cacheMode: FlourishPageCacheMode.Enabled);
    /// ]]></code>
    /// </example>
    public static IServiceCollection AddNavigable<TPage>(
        this IServiceCollection services,
        string displayName,
        string iconGlyph,
        FlourishPageCacheMode cacheMode = FlourishPageCacheMode.Enabled
    )
        where TPage : Page
    {
        return services.AddNavigable(typeof(TPage), displayName, iconGlyph, cacheMode);
    }

    /// <summary>
    /// Registers a WPF page type as a navigable Flourish page.
    /// </summary>
    /// <remarks>
    /// Use this overload when the page type is discovered at runtime, such as from configuration or
    /// a plug-in. As with the generic overload, this registers page metadata only; the visible
    /// navigation item is created later by the navigation panel builder.
    /// </remarks>
    /// <param name="services">The service collection that receives the page registration.</param>
    /// <param name="pageType">The page type to register.</param>
    /// <param name="displayName">The display name used when the page is displayed in navigation UI.</param>
    /// <param name="iconGlyph">The icon glyph used when the page is displayed in navigation UI.</param>
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

        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("A navigable page requires a display name.", nameof(displayName));
        }

        services.AddTransient(pageType);
        GetOrCreateState(services)
            .NavigablePages.Add(
                new NavigablePageRegistration(pageType, displayName, iconGlyph, cacheMode)
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
