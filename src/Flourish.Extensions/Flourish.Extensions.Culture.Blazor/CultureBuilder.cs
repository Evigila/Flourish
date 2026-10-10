using System.Globalization;
using ArkheideSystem.Essential.Culture;
using Microsoft.Extensions.Configuration;

namespace ArkheideSystem.Flourish.Extensions.Culture.Blazor;

/// <summary>Configures catalogs and browser culture defaults before the framework registration completes.</summary>
public sealed class CultureBuilder
{
    private readonly Dictionary<string, LocalizationCatalog> catalogs = new(StringComparer.Ordinal);
    private readonly List<string> supportedCultures = [];
    private string defaultCatalog = "Flourish";
    private bool selectedDefaultCatalog;
    private string defaultCulture;
    private string defaultFormatCulture;
    private int retentionDays;
    private bool completed;

    internal CultureBuilder(IConfiguration configuration)
    {
        defaultCulture = Normalize(configuration["Flourish:Localization:DefaultCulture"] ?? "en-US");
        defaultFormatCulture = Normalize(configuration["Flourish:Localization:DefaultFormatCulture"] ?? defaultCulture);
        retentionDays = configuration.GetValue("Flourish:Preferences:RetentionDays", 365);
        foreach (var name in configuration.GetSection("Flourish:Localization:SupportedCultures").Get<string[]>() ?? [])
            AddSupportedCultures(name);
    }

    /// <summary>Loads an embedded JSON catalog from the marker type's assembly.</summary>
    public CultureBuilder AddCatalog<TAssemblyMarker>(string catalogId, string resourceName)
    {
        Check();
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceName);
        var assembly = typeof(TAssemblyMarker).Assembly;
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Catalog resource '{resourceName}' was not found in '{assembly.GetName().Name}'.");
        return AddCatalog(catalogId, stream);
    }

    /// <summary>Reads a JSON catalog from the stream's current position; the caller owns the stream.</summary>
    public CultureBuilder AddCatalog(string catalogId, Stream stream)
    {
        CheckCatalog(catalogId);
        ArgumentNullException.ThrowIfNull(stream);
        return AddCatalog(catalogId, LocalizationCatalog.Load(stream));
    }

    /// <summary>Adds an immutable Essential catalog, including its fallback and retained-language policy.</summary>
    public CultureBuilder AddCatalog(string catalogId, LocalizationCatalog catalog)
    {
        CheckCatalog(catalogId);
        ArgumentNullException.ThrowIfNull(catalog);
        catalogs.Add(catalogId, catalog);
        if (!selectedDefaultCatalog && catalogId is not ("Flourish" or "Culture"))
        {
            defaultCatalog = catalogId;
            selectedDefaultCatalog = true;
        }
        return this;
    }

    /// <summary>
    /// Eagerly loads explicitly supplied module files as one catalog. Relative deployment paths resolve
    /// against AppContext.BaseDirectory; callers may also supply absolute paths. Essential validates
    /// the files, globally unique keys, fallback translations and optional retained-language policy.
    /// </summary>
    public CultureBuilder AddCatalogFiles(string catalogId, IEnumerable<string> paths,
        string fallbackCulture = "en-US", CatalogLoadOptions? options = null)
    {
        CheckCatalog(catalogId);
        ArgumentNullException.ThrowIfNull(paths);
        return AddCatalog(catalogId, LocalizationCatalog.FromFiles(paths.Select(ResolveCatalogPath), fallbackCulture, options));
    }

    /// <summary>Overrides the Parse/TryParse catalog. The first application catalog is used by default, or Flourish if none is added.</summary>
    public CultureBuilder SetDefaultCatalog(string catalogId)
    {
        Check();
        ArgumentException.ThrowIfNullOrWhiteSpace(catalogId);
        defaultCatalog = catalogId;
        selectedDefaultCatalog = true;
        return this;
    }

    /// <summary>Overrides default UI and formatting cultures; an omitted formatting culture follows the UI culture.</summary>
    public CultureBuilder SetDefaultCulture(string culture, string? formatCulture = null)
    {
        Check();
        defaultCulture = Normalize(culture);
        defaultFormatCulture = Normalize(formatCulture ?? culture);
        return this;
    }

    /// <summary>Adds allowed UI and formatting selections; when omitted, the default catalog's cultures are used.</summary>
    public CultureBuilder AddSupportedCultures(params string[] cultures)
    {
        Check();
        ArgumentNullException.ThrowIfNull(cultures);
        foreach (var culture in cultures)
        {
            var name = Normalize(culture);
            if (!supportedCultures.Contains(name, StringComparer.OrdinalIgnoreCase)) supportedCultures.Add(name);
        }
        return this;
    }

    /// <summary>Overrides the personal browser cookie lifetime, from one day through ten years.</summary>
    public CultureBuilder SetRetentionDays(int days)
    {
        Check();
        ValidateRetention(days);
        retentionDays = days;
        return this;
    }

    internal CultureOptions Complete()
    {
        Check();
        if (!catalogs.TryGetValue(defaultCatalog, out var catalog))
            throw new InvalidOperationException($"Default catalog '{defaultCatalog}' is not registered.");
        var cultures = supportedCultures.Count == 0 ? catalog.AvailableCultures.Select(Normalize).ToArray() : supportedCultures.ToArray();
        if (!cultures.Contains(defaultCulture, StringComparer.OrdinalIgnoreCase) || !cultures.Contains(defaultFormatCulture, StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException("Default UI and formatting cultures must be included in the supported cultures.");
        foreach (var (catalogId, registered) in catalogs)
        {
            foreach (var culture in cultures)
            {
                if (!registered.IsCultureEnabled(culture))
                    throw new InvalidOperationException($"UI culture '{culture}' is disabled by catalog '{catalogId}'. Align supported cultures with every catalog's language policy.");
            }
        }
        ValidateRetention(retentionDays);
        completed = true;
        return new(new System.Collections.ObjectModel.ReadOnlyDictionary<string, LocalizationCatalog>(catalogs), defaultCatalog,
            defaultCulture, defaultFormatCulture, Array.AsReadOnly(cultures), retentionDays);
    }

    internal static string Normalize(string culture)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(culture);
        var name = CultureInfo.GetCultureInfo(culture).Name;
        if (name.Length == 0) throw new ArgumentException("A named culture is required.", nameof(culture));
        return name;
    }

    private static void ValidateRetention(int days)
    {
        if (days is < 1 or > 3650) throw new ArgumentOutOfRangeException(nameof(days), "Preference retention must be between 1 and 3650 days.");
    }

    private void CheckCatalog(string catalogId)
    {
        Check();
        ArgumentException.ThrowIfNullOrWhiteSpace(catalogId);
        if (catalogs.ContainsKey(catalogId)) throw new InvalidOperationException($"Catalog '{catalogId}' is already configured.");
    }

    private static string ResolveCatalogPath(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return Path.GetFullPath(path, AppContext.BaseDirectory);
    }

    private void Check()
    {
        if (completed) throw new InvalidOperationException("Culture configuration is complete and cannot be modified.");
    }
}

internal sealed record CultureOptions(
    IReadOnlyDictionary<string, LocalizationCatalog> Catalogs,
    string DefaultCatalog,
    string DefaultCulture,
    string DefaultFormatCulture,
    IReadOnlyList<string> SupportedCultures,
    int RetentionDays);
