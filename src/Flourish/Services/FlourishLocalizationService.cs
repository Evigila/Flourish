using System.Globalization;
using System.IO;
using System.Text.Json;
using ArkheideSystem.Flourish.Configuration;

namespace ArkheideSystem.Flourish.Services;

internal sealed class FlourishLocalizationService
{
    private const string DefaultLocale = "CN";
    private const string EmbeddedResourcePrefix = "ArkheideSystem.Flourish.Assets.lang_";

    private readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> builtInLocales;
    private readonly Dictionary<string, Dictionary<string, string>> customLocales =
        new(StringComparer.OrdinalIgnoreCase);

    public FlourishLocalizationService(FlourishDataOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        Locale = NormalizeLocale(options.Locale);
        builtInLocales = new Dictionary<string, IReadOnlyDictionary<string, string>>(
            StringComparer.OrdinalIgnoreCase
        )
        {
            [DefaultLocale] = LoadEmbeddedLocale(DefaultLocale),
            ["EN"] = LoadEmbeddedLocale("EN"),
        };

        foreach (var path in options.LocalePaths)
        {
            LoadCustomLocale(path);
        }
    }

    public string Locale { get; }

    public string Get(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Locale key cannot be empty.", nameof(key));
        }

        return TryGet(customLocales, Locale, key)
            ?? TryGet(builtInLocales, Locale, key)
            ?? TryGet(customLocales, DefaultLocale, key)
            ?? TryGet(builtInLocales, DefaultLocale, key)
            ?? key;
    }

    public string Format(string key, params object?[] arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        return string.Format(CultureInfo.CurrentCulture, Get(key), arguments);
    }

    private void LoadCustomLocale(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Locale file path cannot be empty.", nameof(path));
        }

        var fullPath = Path.GetFullPath(path);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException(
                $"Locale file '{fullPath}' does not exist.",
                fullPath
            );
        }

        var locale = GetLocaleFromFileName(fullPath);
        var values = LoadLocaleFile(fullPath);
        if (!customLocales.TryGetValue(locale, out var mergedValues))
        {
            mergedValues = new Dictionary<string, string>(StringComparer.Ordinal);
            customLocales.Add(locale, mergedValues);
        }

        foreach (var (key, value) in values)
        {
            mergedValues[key] = value;
        }
    }

    private static string GetLocaleFromFileName(string path)
    {
        var fileName = Path.GetFileName(path);
        const string prefix = "lang_";
        const string extension = ".json";
        if (
            !fileName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            || !fileName.EndsWith(extension, StringComparison.OrdinalIgnoreCase)
        )
        {
            throw new ArgumentException(
                $"Locale file '{fileName}' must be named lang_<locale>.json.",
                nameof(path)
            );
        }

        var locale = fileName[prefix.Length..^extension.Length];
        if (
            locale.Length == 0
            || locale.Any(character =>
                !char.IsLetterOrDigit(character) && character is not '-' and not '_'
            )
        )
        {
            throw new ArgumentException(
                $"Locale file '{fileName}' must be named lang_<locale>.json.",
                nameof(path)
            );
        }

        return NormalizeLocale(locale);
    }

    private static IReadOnlyDictionary<string, string> LoadEmbeddedLocale(string locale)
    {
        var resourceName = $"{EmbeddedResourcePrefix}{locale}.json";
        var assembly = typeof(FlourishLocalizationService).Assembly;
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
        {
            throw new InvalidOperationException(
                $"Built-in locale resource '{resourceName}' could not be found."
            );
        }

        return ParseLocale(stream, resourceName);
    }

    private static IReadOnlyDictionary<string, string> LoadLocaleFile(string path)
    {
        try
        {
            using var stream = File.OpenRead(path);
            return ParseLocale(stream, path);
        }
        catch (InvalidDataException)
        {
            throw;
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            throw new InvalidDataException(
                $"Locale file '{path}' could not be read.",
                error
            );
        }
    }

    private static IReadOnlyDictionary<string, string> ParseLocale(
        Stream stream,
        string sourceName
    )
    {
        try
        {
            using var document = JsonDocument.Parse(stream);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                throw new InvalidDataException(
                    $"Locale source '{sourceName}' must contain a JSON object."
                );
            }

            var values = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var property in document.RootElement.EnumerateObject())
            {
                if (string.IsNullOrWhiteSpace(property.Name))
                {
                    throw new InvalidDataException(
                        $"Locale source '{sourceName}' contains an empty key."
                    );
                }

                if (
                    property.Value.ValueKind != JsonValueKind.String
                    || string.IsNullOrWhiteSpace(property.Value.GetString())
                )
                {
                    throw new InvalidDataException(
                        $"Locale source '{sourceName}' contains an empty or non-string value for key '{property.Name}'."
                    );
                }

                if (!values.TryAdd(property.Name, property.Value.GetString()!))
                {
                    throw new InvalidDataException(
                        $"Locale source '{sourceName}' contains duplicate key '{property.Name}'."
                    );
                }
            }

            if (values.Count == 0)
            {
                throw new InvalidDataException(
                    $"Locale source '{sourceName}' does not contain any translations."
                );
            }

            return values;
        }
        catch (JsonException error)
        {
            throw new InvalidDataException(
                $"Locale source '{sourceName}' contains invalid JSON.",
                error
            );
        }
    }

    private static string NormalizeLocale(string? locale)
    {
        return string.IsNullOrWhiteSpace(locale)
            ? DefaultLocale
            : locale.Trim().ToUpperInvariant();
    }

    private static string? TryGet<TDictionary>(
        IReadOnlyDictionary<string, TDictionary> locales,
        string locale,
        string key
    )
        where TDictionary : IReadOnlyDictionary<string, string>
    {
        return locales.TryGetValue(locale, out var values)
            && values.TryGetValue(key, out var value)
            ? value
            : null;
    }
}
