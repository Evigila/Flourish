using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Blazor.Abstract;
using ArkheideSystem.Flourish.Blazor.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ArkheideSystem.Flourish.Blazor;

/// <summary>Registers the framework shell, commands and per-user browser state.</summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFlourishFramework(
        this IServiceCollection services,
        Action<IFrameworkBuilder>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        return Register(services, builder => configure?.Invoke(builder));
    }

    [Obsolete("Use AddFlourishFramework.")]
    public static IServiceCollection AddFlourish(
        this IServiceCollection services,
        Action<IApplicationBuilder>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        return Register(services, builder => configure?.Invoke(builder));
    }

    private static IServiceCollection Register(IServiceCollection services, Action<ApplicationBuilder> configure)
    {
        if (services.Any(service => service.ServiceType == typeof(ApplicationOptions)))
            throw new InvalidOperationException("AddFlourishFramework can only be configured once per host.");

        var builder = new ApplicationBuilder();
        configure(builder);
        var options = builder.Complete();
        services.AddSingleton(options);

        if (options.CommandParserType is { } parserType)
            services.AddScoped(typeof(ICommandParser), parserType);

        services.TryAddScoped<CommandRuntime>();
        services.TryAddScoped<ICommandDispatcher>(provider => provider.GetRequiredService<CommandRuntime>());
        services.TryAddScoped<ITablePreferences, Components.Primitives.TablePreferences>();
        return services;
    }
}
