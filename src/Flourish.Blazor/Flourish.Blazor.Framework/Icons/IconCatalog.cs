using System.Collections.Frozen;
using System.Globalization;

namespace ArkheideSystem.Flourish.Blazor;

/// <summary>Names from the bundled Google Material Symbols Outlined font.</summary>
public static class IconCatalog
{
    private static readonly FrozenDictionary<string, string> Glyphs = LoadGlyphs();

    /// <summary>All supported official names, ordered ordinally.</summary>
    public static IReadOnlyList<string> Names { get; } = Array.AsReadOnly(Glyphs.Keys.Order(StringComparer.Ordinal).ToArray());

    /// <summary>Checks only official icon names.</summary>
    public static bool Contains(string? name) => name is not null && Glyphs.ContainsKey(name);

    internal static string Glyph(string? name) => Glyphs.TryGetValue(name ?? string.Empty, out var value)
        ? value : Glyphs["help_outline"];

    private static FrozenDictionary<string, string> LoadGlyphs()
    {
        using var stream = typeof(IconCatalog).Assembly.GetManifestResourceStream("Flourish.Blazor.Icons.Codepoints")
            ?? throw new InvalidOperationException("The bundled icon name map is missing.");
        using var reader = new StreamReader(stream);
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        while (reader.ReadLine() is { } line)
        {
            var fields = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (fields.Length != 2 || !int.TryParse(fields[1], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var codepoint))
                throw new InvalidOperationException("The bundled icon name map contains an invalid entry.");
            result.Add(fields[0], char.ConvertFromUtf32(codepoint));
        }
        return result.ToFrozenDictionary(StringComparer.Ordinal);
    }
}
