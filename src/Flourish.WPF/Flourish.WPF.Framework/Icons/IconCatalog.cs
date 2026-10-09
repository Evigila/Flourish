using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Windows.Media;

namespace ArkheideSystem.Flourish.WPF;

/// <summary>Official names and native outlines from the bundled Material Symbols Outlined artwork.</summary>
public static class IconCatalog
{
    private static readonly FrozenDictionary<string, int> Codepoints = LoadCodepoints();
    private static readonly Lazy<GlyphTypeface> Typeface = new(() => new GlyphTypeface(
        new Uri("pack://application:,,,/Flourish.WPF.Framework;component/Icons/MaterialSymbolsOutlined.ttf", UriKind.Absolute)));
    private static readonly ConcurrentDictionary<ushort, Geometry> Outlines = new();
    private static readonly Lazy<FrozenDictionary<int, string>> FilledPaths = new(LoadFilledPaths);
    private static readonly ConcurrentDictionary<int, Geometry> FilledOutlines = new();

    public static IReadOnlyList<string> Names { get; } = Array.AsReadOnly(Codepoints.Keys.Order(StringComparer.Ordinal).ToArray());
    public static bool Contains(string? name) => name is not null && Codepoints.ContainsKey(name);

    internal static Geometry Outline(string? name, bool filled = false)
    {
        var codepoint = Codepoints.TryGetValue(name ?? "", out var known) ? known : Codepoints["help_outline"];
        if (filled)
            return FilledOutlines.GetOrAdd(codepoint, key =>
            {
                var outline = Geometry.Parse(FilledPaths.Value[key]);
                outline.Freeze();
                return outline;
            });
        var typeface = Typeface.Value;
        if (!typeface.CharacterToGlyphMap.TryGetValue(codepoint, out var glyph))
            throw new InvalidOperationException("The bundled Material Symbols font does not match its official name map.");
        return Outlines.GetOrAdd(glyph, key =>
        {
            var outline = typeface.GetGlyphOutline(key, 1, 1);
            outline.Freeze();
            return outline;
        });
    }

    private static FrozenDictionary<int, string> LoadFilledPaths()
    {
        using var stream = typeof(IconCatalog).Assembly.GetManifestResourceStream("Flourish.WPF.Icons.Filled")
            ?? throw new InvalidOperationException("The bundled Material Symbols filled artwork is missing.");
        using var decompressed = new BrotliStream(stream, CompressionMode.Decompress);
        var paths = JsonSerializer.Deserialize<Dictionary<int, string>>(decompressed)
            ?? throw new InvalidOperationException("The bundled Material Symbols filled artwork is invalid.");
        if (Codepoints.Values.Any(codepoint => !paths.ContainsKey(codepoint)))
            throw new InvalidOperationException("The bundled Material Symbols filled artwork does not match its official name map.");
        return paths.ToFrozenDictionary();
    }

    private static FrozenDictionary<string, int> LoadCodepoints()
    {
        using var stream = typeof(IconCatalog).Assembly.GetManifestResourceStream("Flourish.WPF.Icons.Codepoints")
            ?? throw new InvalidOperationException("The bundled Material Symbols name map is missing.");
        using var reader = new StreamReader(stream);
        var names = new Dictionary<string, int>(StringComparer.Ordinal);
        while (reader.ReadLine() is { } line)
        {
            var fields = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (fields.Length != 2 || !int.TryParse(fields[1], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var codepoint))
                throw new InvalidOperationException("The bundled Material Symbols name map is invalid.");
            names.Add(fields[0], codepoint);
        }
        return names.ToFrozenDictionary(StringComparer.Ordinal);
    }
}
