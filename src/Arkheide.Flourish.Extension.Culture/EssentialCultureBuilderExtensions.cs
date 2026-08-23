using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace ArkheideSystem.Flourish.Extension.Culture;

/// <summary>Connects a Flourish application to Arkheide Essential Culture.</summary>
public static class EssentialCultureBuilderExtensions
{
    /// <summary>
    /// Uses <see cref="IFlourishLocalization" /> as the application's public culture endpoint and
    /// connects it to Arkheide Essential Culture and Flourish shell text.
    /// </summary>
    /// <param name="builder">The Flourish builder to configure.</param>
    /// <returns>The same builder for chained configuration.</returns>
    public static IFlourishBuilder UseEssentialCulture(this IFlourishBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.ConfigServices(
            (_, services) =>
            {
                services.TryAddSingleton<FlourishShellCultureApplicator>();
                services.TryAddEnumerable(
                    ServiceDescriptor.Singleton<IHostedService, EssentialCultureHostedService>()
                );
            }
        );
    }
}
