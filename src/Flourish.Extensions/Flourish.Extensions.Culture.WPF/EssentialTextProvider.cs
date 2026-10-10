using System.Globalization;
using ArkheideSystem.Essential.Culture;
using ArkheideSystem.Flourish.WPF.Abstract;

namespace ArkheideSystem.Flourish.Extensions.Culture.WPF;

/// <summary>Connects one consumer catalog and the bundled Flourish catalog to the neutral text contract.</summary>
/// <remarks>Own and dispose this provider with the desktop window/application that uses it.</remarks>
public sealed class EssentialTextProvider : ITextProvider, IDisposable
{
    private readonly string catalogId;
    private readonly LocalizationContext? context;
    private readonly CultureInfo? formattingCulture;
    private readonly Lazy<LocalizationContext> libraryContext;
    private bool disposed;
    private static readonly Lazy<LocalizationCatalog> libraryCatalog = new(ReadLibraryCatalog);

    /// <summary>Uses the desktop facade without mutating its initial culture selection.</summary>
    /// <remarks>Configure Localizer before constructing this provider when using modules or a startup language policy.</remarks>
    public EssentialTextProvider(string catalogId = "Application", string? formatCulture = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(catalogId);
        this.catalogId = catalogId;
        formattingCulture = formatCulture is null ? null : CultureInfo.GetCultureInfo(formatCulture);
        libraryContext = new(() => new LocalizationContext(libraryCatalog.Value, Culture, FormatCulture.Name));
        Localizer.Current.Changed += SourceChanged;
    }

    /// <summary>Uses an independently owned context for hosts with isolated culture/catalog state.</summary>
    public EssentialTextProvider(LocalizationContext context, string catalogId = "Application")
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentException.ThrowIfNullOrWhiteSpace(catalogId);
        this.context = context;
        this.catalogId = catalogId;
        libraryContext = new(() => new LocalizationContext(libraryCatalog.Value, Culture, FormatCulture.Name));
        context.Changed += SourceChanged;
    }

    /// <summary>Creates isolated selection state over a single or composed Essential catalog.</summary>
    public EssentialTextProvider(LocalizationCatalog catalog, string culture = "en-US",
        string? formatCulture = null, string catalogId = "Application")
        : this(new LocalizationContext(catalog, culture, formatCulture), catalogId) { }

    /// <inheritdoc />
    public string Culture => context?.Culture ?? Localizer.Current.Culture;
    /// <inheritdoc />
    public CultureInfo FormatCulture => context?.FormatCulture ?? formattingCulture ?? CultureInfo.GetCultureInfo(Culture);
    /// <summary>Gets the consumer catalog's selectable cultures, honoring its startup policy.</summary>
    public IReadOnlyList<string> AvailableCultures => context?.AvailableCultures ?? Localizer.Current.AvailableCultures;
    /// <inheritdoc />
    public event EventHandler? Changed;

    /// <summary>Changes the consumer selection using Essential's policy and change-event semantics.</summary>
    /// <remarks>Instance formatting follows this selection. The desktop facade retains this provider's optional format override.</remarks>
    public void SetCulture(string culture)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (context is null) Localizer.Current.SetCulture(culture);
        else context.SetCulture(culture);
    }

    /// <inheritdoc />
    public string Get(TextReference text, params object?[] arguments)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(arguments);
        var template = text.FallbackText ?? text.Token;
        // Library text has its own identity and never resolves through the consumer desktop file.
        if (string.Equals(text.CatalogId, "Flourish", StringComparison.Ordinal))
        {
            var source = libraryContext.Value;
            source.SetCulture(Culture, FormatCulture.Name);
            if (source.TryParse(text.Token, arguments, out var translated)) return translated;
        }
        // All consumer modules form one logical catalog. Other IDs must not resolve a
        // coincidentally identical token from that consumer catalog.
        else if (string.Equals(text.CatalogId, catalogId, StringComparison.Ordinal))
        {
            if (context is not null)
            {
                if (context.TryParse(text.Token, arguments, out var translated)) return translated;
            }
            else if (Localizer.TryParse(text.Token, out var translated)) template = translated;
        }
        return arguments.Length == 0 ? template : string.Format(FormatCulture, template, arguments);
    }

    private static LocalizationCatalog ReadLibraryCatalog()
    {
        using var stream = typeof(ArkheideSystem.Flourish.WPF.FrameworkBuilder).Assembly.GetManifestResourceStream("Flourish.WPF.Texts.json")
            ?? throw new InvalidOperationException("The bundled Flourish text catalog is missing.");
        return LocalizationCatalog.Load(stream, "en-US");
    }

    private void SourceChanged(object? sender, EventArgs args)
    {
        if (!disposed) Changed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Detaches the Essential culture subscription and releases consumer callbacks.</summary>
    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        if (context is null) Localizer.Current.Changed -= SourceChanged;
        else context.Changed -= SourceChanged;
        Changed = null;
    }
}
