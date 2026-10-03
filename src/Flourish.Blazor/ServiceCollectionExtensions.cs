using ArkheideSystem.Flourish.Blazor.Abstract;
using ArkheideSystem.Flourish.Blazor.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace ArkheideSystem.Flourish.Blazor;

/// <summary>Registers immutable shell configuration and per-user Web runtime state.</summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFlourish(this IServiceCollection services, Action<IApplicationBuilder>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        if (services.Any(service => service.ServiceType == typeof(ApplicationOptions)))
            throw new InvalidOperationException("AddFlourish can only be configured once per host.");
        var builder = new ApplicationBuilder(); configure?.Invoke(builder);
        services.AddSingleton(builder.Complete());
        services.AddScoped<IAppearanceService, AppearanceService>();
        return services;
    }
}