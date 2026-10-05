using ArkheideSystem.Flourish.Blazor.Abstract;
using LocalizationService = ArkheideSystem.Essential.Culture.Blazor.ILocalizationService;
using System.Globalization;

namespace ArkheideSystem.Flourish.Extensions.Culture.Blazor;

internal sealed class CultureTextProvider(LocalizationService localization) : ITextProvider
{
    public string Culture => localization.Culture;
    public CultureInfo FormatCulture => localization.FormatCulture;

    public event EventHandler? Changed
    {
        add => localization.Changed += value;
        remove => localization.Changed -= value;
    }

    public string Get(TextReference text, params object?[] arguments)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(arguments);
        if (localization.TryParseFrom(text.CatalogId, text.Token, arguments, out var value)) return value;
        var fallback = text.FallbackText ?? text.Token;
        return arguments.Length == 0 ? fallback : string.Format(FormatCulture, fallback, arguments);
    }
}
