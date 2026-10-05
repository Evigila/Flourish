using System.Globalization;
using ArkheideSystem.Flourish.Blazor.Abstract;

namespace ArkheideSystem.Flourish.Blazor.Hosting;

internal sealed class LiteralTextProvider : ITextProvider
{
    public string Culture => CultureInfo.CurrentUICulture.Name;
    public CultureInfo FormatCulture => CultureInfo.CurrentCulture;
    public event EventHandler? Changed { add { } remove { } }

    public string Get(TextReference text, params object?[] arguments)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(arguments);
        var value = text.FallbackText ?? text.Token;
        return arguments.Length == 0 ? value : string.Format(FormatCulture, value, arguments);
    }
}
