using ArkheideSystem.Flourish.Blazor.Abstract;
using ArkheideSystem.Flourish.Blazor.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace ArkheideSystem.Flourish.Blazor;

/// <summary>Explicitly registers the optional design configuration and per-user appearance state.</summary>
public static class DesignServiceCollectionExtensions
{
    public static IServiceCollection AddFlourishDesign(this IServiceCollection services, Action<IAppearanceBuilder>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        if (services.Any(service => service.ServiceType == typeof(DesignOptions)))
            throw new InvalidOperationException("AddFlourishDesign can only be configured once per host.");
        var builder = new AppearanceBuilder();
        configure?.Invoke(builder);
        services.AddSingleton(builder.Complete());
        services.AddScoped<AppearanceService>();
        services.AddScoped<IAppearanceService>(provider => provider.GetRequiredService<AppearanceService>());
        services.AddScoped<IThemeProvider>(provider => provider.GetRequiredService<AppearanceService>());
        return services;
    }
}
