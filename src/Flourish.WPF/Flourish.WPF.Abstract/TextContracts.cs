using System.Globalization;

namespace ArkheideSystem.Flourish.WPF.Abstract;

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

/// <summary>A provider-neutral, desktop-lifetime text source, matching the Blazor ownership boundary.</summary>
public interface ITextProvider
{
    string Culture { get; }
    CultureInfo FormatCulture { get; }
    event EventHandler? Changed;
    string Get(TextReference text, params object?[] arguments);
}
