using ArkheideSystem.Flourish.Extensions.Culture.Blazor;
using ArkheideSystem.Flourish.Blazor.Abstract;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>Connects optional Culture localization to the Flourish text contract.</summary>
public static class CultureServiceCollectionExtensions
{
    /// <summary>
    /// Registers a scoped text provider backed by the existing Essential.Culture.Blazor service.
    /// Catalogs, language selection, framework setup and preference storage remain host responsibilities.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddFlourishCulture(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddScoped<CultureTextProvider>();
        services.RemoveAll<ITextProvider>();
        services.AddScoped<ITextProvider>(provider => provider.GetRequiredService<CultureTextProvider>());
        return services;
    }
}
