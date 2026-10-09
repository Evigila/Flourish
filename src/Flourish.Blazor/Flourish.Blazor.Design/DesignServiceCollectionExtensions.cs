using ArkheideSystem.Flourish.Blazor.Abstract;
using ArkheideSystem.Flourish.Blazor.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using ArkheideSystem.Flourish.Abstract;

namespace ArkheideSystem.Flourish.Blazor;

/// <summary>Explicitly registers the optional design configuration and per-user appearance state.</summary>
public static class DesignServiceCollectionExtensions
{
    /// <summary>Uses the framework appearance defaults, with explicit configuration applied last.</summary>
    public static IServiceCollection AddFlourishDesign(this IServiceCollection services, IConfiguration configuration,
        Action<IAppearanceBuilder>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        return services.AddFlourishDesign(appearance =>
        {
            var section = configuration.GetSection("Flourish:Appearance");
            if (section["Primary"] is not null || section["Accent"] is not null)
                appearance.SetColors(section["Primary"] ?? "#153A32", section["Accent"] ?? "#16745F");
            if (section["Theme"] is not null)
                appearance.SetTheme(section.GetValue<ApplicationTheme>("Theme"));
            if (section["FontFamily"] is { } font) appearance.SetFont(font);
            configure?.Invoke(appearance);
        });
    }

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
