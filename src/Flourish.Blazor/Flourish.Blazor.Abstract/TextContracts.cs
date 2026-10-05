using System.Globalization;

namespace ArkheideSystem.Flourish.Blazor.Abstract;

/// <summary>Identifies translated text without selecting a localization library.</summary>
public sealed record TextReference
{
    public TextReference(string catalogId, string token, string? fallbackText = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(catalogId);
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        CatalogId = catalogId;
        Token = token;
        FallbackText = fallbackText;
    }

    public string CatalogId { get; }
    public string Token { get; }
    public string? FallbackText { get; }
}

/// <summary>Resolves text and notifies consumers within one request or circuit.</summary>
public interface ITextProvider
{
    string Culture { get; }
    CultureInfo FormatCulture { get; }
    event EventHandler? Changed;
    string Get(TextReference text, params object?[] arguments);
}
