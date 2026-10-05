using System;

using ArkheideSystem.Flourish.Abstract;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace ArkheideSystem.Flourish.Extensions.Culture.WPF;

/// <summary>Connects a Flourish application to Essential.Culture.</summary>
public static class EssentialCultureBuilderExtensions
{
    /// <summary>
    /// Uses <see cref="ILocalizationService" /> as the application's public culture endpoint and
    /// connects it to Essential.Culture and Flourish shell text.
    /// </summary>
    /// <param name="builder">The Flourish builder to configure.</param>
    /// <returns>The same builder for chained configuration.</returns>
    public static IApplicationBuilder UseEssentialCulture(this IApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.ConfigureServices(
            (_, services) =>
            {
                services.TryAddSingleton<ShellCultureApplicator>();
                services.TryAddEnumerable(
                    ServiceDescriptor.Singleton<IHostedService, EssentialCultureHostedService>()
                );
            }
        );
    }
}
