using System.Windows.Controls;
using ArkheideSystem.Flourish.Internal.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ArkheideSystem.Flourish.Abstract;

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
    private const string PageSuffix = "Page";

    internal static string CreateDefaultNavigationKey(Type pageType)
    {
        ArgumentNullException.ThrowIfNull(pageType);

        var typeName = pageType.Name;
        return typeName.Length > PageSuffix.Length
            && typeName.EndsWith(PageSuffix, StringComparison.Ordinal)
            ? typeName[..^PageSuffix.Length]
            : typeName;
    }

    /// <summary>
    /// Registers a WPF page as a navigable Flourish page.
    /// </summary>
    /// <remarks>
    /// This method records the page display name, icon glyph, and cache mode. Use
    /// <see cref="IFlourishNavigationGroupBuilder.AddNavigableViewItem{TPage}" /> or
    /// <see cref="IFlourishNavigationBuilder.AddFixedNavigableViewItem{TPage}" /> inside
    /// <c>ConfigureNavigation</c> to place each page that should be visible. The navigation
    /// key is the page class name with one trailing, case-sensitive <c>Page</c> suffix removed;
    /// for example, <c>SettingsPage</c> becomes <c>Settings</c>. Flourish validates key uniqueness
    /// when the application builder builds.
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
    ///
    /// navigation.Navigate("Home");
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
        ArgumentNullException.ThrowIfNull(services);

        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("A navigable page requires a display name.", nameof(displayName));
        }

        var pageType = typeof(TPage);
        var navigationKey = CreateDefaultNavigationKey(pageType);

        services.AddTransient(pageType);
        GetOrCreateState(services)
            .NavigablePages.Add(
                new NavigablePageRegistration(
                    navigationKey,
                    pageType,
                    displayName,
                    iconGlyph,
                    cacheMode
                )
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
