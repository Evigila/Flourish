using ArkheideSystem.Essential.Culture;
using ArkheideSystem.Flourish.WPF;

namespace ArkheideSystem.Flourish.Extensions.Culture.WPF;

/// <summary>Configures the same provider-neutral boundary used by Flourish.Blazor.</summary>
public static class EssentialCultureBuilderExtensions
{
    /// <summary>Installs a provider whose lifetime remains owned by the caller.</summary>
    public static FrameworkBuilder UseEssentialCulture(this FrameworkBuilder builder, EssentialTextProvider provider)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(provider);
        builder.TextProvider = provider;
        return builder;
    }

    /// <summary>Creates a desktop provider and exposes its disposal ownership explicitly.</summary>
    public static FrameworkBuilder UseEssentialCulture(this FrameworkBuilder builder,
        out EssentialTextProvider provider, string catalogId = "Application", string? formatCulture = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        provider = new EssentialTextProvider(catalogId, formatCulture);
        return builder.UseEssentialCulture(provider);
    }

    /// <summary>Connects an independent Essential localization context.</summary>
    public static FrameworkBuilder UseEssentialCulture(this FrameworkBuilder builder,
        LocalizationContext context, out EssentialTextProvider provider, string catalogId = "Application")
    {
        ArgumentNullException.ThrowIfNull(builder);
        provider = new EssentialTextProvider(context, catalogId);
        return builder.UseEssentialCulture(provider);
    }
}
