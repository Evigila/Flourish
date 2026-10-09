using System.Globalization;
using System.Text.Json;
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
    private bool disposed;
    private static readonly Lazy<IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>>> libraryTexts = new(ReadLibraryTexts);

    /// <summary>Uses the desktop Culture.json facade without mutating its culture selection.</summary>
    public EssentialTextProvider(string catalogId = "Application", string? formatCulture = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(catalogId);
        this.catalogId = catalogId;
        formattingCulture = formatCulture is null ? null : CultureInfo.GetCultureInfo(formatCulture);
        Localizer.Current.Changed += SourceChanged;
    }

    /// <summary>Uses an independently owned context for hosts with isolated culture/catalog state.</summary>
    public EssentialTextProvider(LocalizationContext context, string catalogId = "Application")
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentException.ThrowIfNullOrWhiteSpace(catalogId);
        this.context = context;
        this.catalogId = catalogId;
        context.Changed += SourceChanged;
    }

    /// <inheritdoc />
    public string Culture => context?.Culture ?? Localizer.Current.Culture;
    /// <inheritdoc />
    public CultureInfo FormatCulture => context?.FormatCulture ?? formattingCulture ?? CultureInfo.GetCultureInfo(Culture);
    /// <inheritdoc />
    public event EventHandler? Changed;

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
            if (libraryTexts.Value.TryGetValue(text.Token, out var translations)
                && (translations.TryGetValue(Culture, out var translated) || translations.TryGetValue("en-US", out translated))) template = translated;
        }
        // Desktop Culture.json contains one consumer catalog. Other IDs must not resolve a
        // coincidentally identical token from that consumer catalog.
        else if (string.Equals(text.CatalogId, catalogId, StringComparison.Ordinal))
        {
            var found = context is null
                ? Localizer.TryParse(text.Token, out var translated)
                : context.TryParse(text.Token, out translated);
            if (found) template = translated;
        }
        return arguments.Length == 0 ? template : string.Format(FormatCulture, template, arguments);
    }

    private static IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> ReadLibraryTexts()
    {
        using var stream = typeof(ArkheideSystem.Flourish.WPF.FrameworkBuilder).Assembly.GetManifestResourceStream("Flourish.WPF.Texts.json")
            ?? throw new InvalidOperationException("The bundled Flourish text catalog is missing.");
        using var document = JsonDocument.Parse(stream);
        return document.RootElement.EnumerateObject().ToDictionary(
            token => token.Name,
            token => (IReadOnlyDictionary<string, string>)token.Value.EnumerateObject().ToDictionary(
                translation => translation.Name,
                translation => translation.Value.GetString() ?? string.Empty,
                StringComparer.OrdinalIgnoreCase),
            StringComparer.Ordinal);
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
