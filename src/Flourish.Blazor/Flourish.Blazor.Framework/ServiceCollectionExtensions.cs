using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Blazor.Abstract;
using ArkheideSystem.Flourish.Blazor.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;

namespace ArkheideSystem.Flourish.Blazor;

/// <summary>Registers the framework shell, commands and per-user browser state.</summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFlourishFramework(
        this IServiceCollection services,
        Action<IFrameworkBuilder>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        return Register(services, new ConfigurationManager(), builder => configure?.Invoke(builder));
    }

    /// <summary>Loads appsettings.Flourish.json below host overrides and registers the application framework.</summary>
    public static IServiceCollection AddFlourishFramework(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IFrameworkBuilder>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        return Register(services, configuration, builder => configure?.Invoke(builder));
    }

    private static IServiceCollection Register(IServiceCollection services, IConfiguration configuration, Action<ApplicationBuilder> configure)
    {
        if (services.Any(service => service.ServiceType == typeof(ApplicationOptions)))
            throw new InvalidOperationException("AddFlourishFramework can only be configured once per host.");

        var settings = LoadSettings(configuration);
        var builder = new ApplicationBuilder();
        configure(builder);
        var options = builder.Complete();
        services.AddSingleton(options);

        if (options.CommandParserType is { } parserType)
            services.AddScoped(typeof(ICommandParser), parserType);

        services.TryAddScoped<ITextProvider, LiteralTextProvider>();
        services.TryAddScoped<CommandRuntime>();
        services.TryAddScoped<ICommandDispatcher>(provider => provider.GetRequiredService<CommandRuntime>());
        services.TryAddScoped<ITablePreferences, Components.TablePreferences>();
        // ASP.NET hosts receive the standard interactive component services through this entry.
        // Service-only consumers can still register the framework without creating a web host.
        if (services.Any(service => service.ServiceType == typeof(IWebHostEnvironment)))
            services.AddRazorComponents().AddInteractiveServerComponents();
        builder.RegisterServices(services, settings);
        return services;
    }

    private static IConfiguration LoadSettings(IConfiguration configuration)
    {
        const string path = "appsettings.Flourish.json";
        if (configuration is ConfigurationManager manager)
        {
            if (!manager.Sources.OfType<Microsoft.Extensions.Configuration.Json.JsonConfigurationSource>()
                .Any(source => string.Equals(source.Path, path, StringComparison.OrdinalIgnoreCase)))
            {
                var source = new Microsoft.Extensions.Configuration.Json.JsonConfigurationSource
                {
                    Path = path,
                    Optional = true,
                    ReloadOnChange = false,
                    FileProvider = ((IConfigurationBuilder)manager).GetFileProvider()
                };
                // Business settings, environment and command line retain their existing precedence.
                manager.Sources.Insert(0, source);
            }
            return manager;
        }

        using var defaults = (ConfigurationRoot)new ConfigurationBuilder().AddJsonFile(path, optional: true, reloadOnChange: false).Build();
        return new ConfigurationBuilder().AddInMemoryCollection(defaults.AsEnumerable())
            .AddInMemoryCollection(configuration.AsEnumerable()).Build();
    }
}
