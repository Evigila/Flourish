using System.Globalization;
using ArkheideSystem.Essential.Culture.Blazor;
using ArkheideSystem.Flourish.Blazor.Abstract;
using ArkheideSystem.Flourish.Blazor.Components;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.JSInterop;

namespace ArkheideSystem.Flourish.Extensions.Culture.Blazor;

/// <summary>Adds optional localization through the framework's registration entry.</summary>
public static class CultureFrameworkExtensions
{
    /// <summary>Configures catalogs, request negotiation, persistent personal selection and the scoped text provider.</summary>
    public static IFrameworkBuilder ConfigureCulture(this IFrameworkBuilder framework, Action<CultureBuilder>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(framework);
        return framework.ConfigureServices((services, configuration) =>
        {
            if (services.Any(service => service.ServiceType == typeof(CultureOptions)))
                throw new InvalidOperationException("ConfigureCulture can only be configured once per host.");
            var builder = new CultureBuilder(configuration);
            builder.AddCatalog<ApplicationShell>("Flourish", "Flourish.Blazor.Texts.json");
            builder.AddCatalog<LanguagePicker>("Culture", "Culture.Texts.json");
            configure?.Invoke(builder);
            var options = builder.Complete();
            services.AddSingleton(options);
            services.AddHttpContextAccessor();
            services.AddCultureBlazor(culture =>
            {
                foreach (var catalog in options.Catalogs) culture.AddCatalog(catalog.Key, catalog.Value);
                culture.SetDefaultCatalog(options.DefaultCatalog)
                    .SetDefaultCulture(options.DefaultCulture, options.DefaultFormatCulture)
                    .AddSupportedCultures(options.SupportedCultures.ToArray())
                    .InitializeWith(provider => InitialSelection(provider, options));
            });
            services.AddScoped(provider => new CultureSession(
                provider.GetRequiredService<ILocalizationService>(), provider.GetRequiredService<IJSRuntime>(),
                provider.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>(), options));
            services.RemoveAll<ITextProvider>();
            services.AddScoped<ITextProvider>(provider => provider.GetRequiredService<CultureSession>());
            services.Configure<RequestLocalizationOptions>(request =>
            {
                request.DefaultRequestCulture = new RequestCulture(options.DefaultFormatCulture, options.DefaultCulture);
                request.SupportedCultures = options.SupportedCultures.Select(CultureInfo.GetCultureInfo).ToList();
                request.SupportedUICultures = options.SupportedCultures.Select(CultureInfo.GetCultureInfo).ToList();
                request.RequestCultureProviders = [new CookieRequestCultureProvider(), new AcceptLanguageHeaderRequestCultureProvider()];
            });
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IStartupFilter, CultureStartupFilter>());
        });
    }

    private static LocalizationSelection InitialSelection(IServiceProvider provider, CultureOptions options)
    {
        var feature = provider.GetRequiredService<IHttpContextAccessor>().HttpContext?.Features.Get<IRequestCultureFeature>();
        return feature is null ? new(options.DefaultCulture, options.DefaultFormatCulture)
            : new(feature.RequestCulture.UICulture.Name, feature.RequestCulture.Culture.Name);
    }
}

internal sealed class CultureStartupFilter : IStartupFilter
{
    public CultureStartupFilter() { }

    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => application =>
    {
        application.UseRequestLocalization();
        next(application);
    };
}
