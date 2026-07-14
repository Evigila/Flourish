using System.Globalization;
using System.IO;
using System.Text.Json;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;

namespace ArkheideSystem.Flourish.Services;

internal sealed class FlourishLocalizationService : IFlourishLocalization
{
    private const string DefaultLocale = "EN";
    private const string EmbeddedResourcePrefix = "ArkheideSystem.Flourish.Assets.lang_";

    private readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> builtInLocales;
    private readonly Dictionary<string, Dictionary<string, string>> customLocales =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly List<LocaleRegistrationState> registrations = [];
    private readonly FlourishDataOptions options;
    private readonly object gate = new();
    private string locale;

    public FlourishLocalizationService(FlourishDataOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        this.options = options;

        locale = NormalizeLocale(options.Locale);
        builtInLocales = new Dictionary<string, IReadOnlyDictionary<string, string>>(
            StringComparer.OrdinalIgnoreCase
        )
        {
            ["CN"] = LoadEmbeddedLocale("CN"),
            ["EN"] = LoadEmbeddedLocale("EN"),
        };

        foreach (var path in options.LocalePaths)
        {
            RegisterFileCore(path);
        }
    }

    public string CurrentLocale
    {
        get
        {
            lock (gate)
            {
                return locale;
            }
        }
    }

    public IReadOnlyList<string> AvailableLocales
    {
        get
        {
            lock (gate)
            {
                return builtInLocales
                    .Keys.Concat(registrations.Select(registration => registration.Handle.Locale))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                    .ToArray();
            }
        }
    }

    public event EventHandler<FlourishLocalizationChangedEventArgs>? Changed;

    public string Get(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Locale key cannot be empty.", nameof(key));
        }

        lock (gate)
        {
            return TryGet(customLocales, locale, key)
                ?? TryGet(builtInLocales, locale, key)
                ?? TryGet(customLocales, DefaultLocale, key)
                ?? TryGet(builtInLocales, DefaultLocale, key)
                ?? key;
        }
    }

    public string Format(string key, params object?[] arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        return string.Format(CultureInfo.CurrentCulture, Get(key), arguments);
    }

    public void SetLocale(string locale)
    {
        var normalizedLocale = NormalizeRuntimeLocale(locale);
        string previousLocale;
        lock (gate)
        {
            previousLocale = this.locale;
            if (string.Equals(previousLocale, normalizedLocale, StringComparison.Ordinal))
            {
                return;
            }

            this.locale = normalizedLocale;
            options.Locale = normalizedLocale;
        }

        Changed?.Invoke(
            this,
            new FlourishLocalizationChangedEventArgs(
                FlourishLocalizationChangeKind.LocaleChanged,
                previousLocale,
                normalizedLocale,
                normalizedLocale,
                null
            )
        );
    }

    public FlourishLocaleRegistration RegisterFile(string path)
    {
        var registration = RegisterFileCore(path);
        var currentLocale = CurrentLocale;
        Changed?.Invoke(
            this,
            new FlourishLocalizationChangedEventArgs(
                FlourishLocalizationChangeKind.FileRegistered,
                currentLocale,
                currentLocale,
                registration.Locale,
                registration
            )
        );
        return registration;
    }

    public void ReloadFile(FlourishLocaleRegistration registration)
    {
        ArgumentNullException.ThrowIfNull(registration);
        var values = LoadLocaleFile(registration.FilePath);
        lock (gate)
        {
            var state = registrations.FirstOrDefault(candidate =>
                candidate.Handle.Id == registration.Id
            );
            if (state is null)
            {
                throw new InvalidOperationException(
                    "The locale-file registration is not active."
                );
            }

            state.Values = values;
            RebuildCustomLocale(state.Handle.Locale);
        }

        var currentLocale = CurrentLocale;
        Changed?.Invoke(
            this,
            new FlourishLocalizationChangedEventArgs(
                FlourishLocalizationChangeKind.FileReloaded,
                currentLocale,
                currentLocale,
                registration.Locale,
                registration
            )
        );
    }

    public bool Unregister(FlourishLocaleRegistration registration)
    {
        ArgumentNullException.ThrowIfNull(registration);
        FlourishLocaleRegistration? removedRegistration;
        lock (gate)
        {
            var index = registrations.FindIndex(candidate =>
                candidate.Handle.Id == registration.Id
            );
            if (index < 0)
            {
                return false;
            }

            removedRegistration = registrations[index].Handle;
            registrations.RemoveAt(index);
            RebuildCustomLocale(removedRegistration.Locale);
        }

        var currentLocale = CurrentLocale;
        Changed?.Invoke(
            this,
            new FlourishLocalizationChangedEventArgs(
                FlourishLocalizationChangeKind.FileUnregistered,
                currentLocale,
                currentLocale,
                removedRegistration.Locale,
                removedRegistration
            )
        );
        return true;
    }

    private FlourishLocaleRegistration RegisterFileCore(string path)
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
        var handle = new FlourishLocaleRegistration(Guid.NewGuid(), locale, fullPath);
        lock (gate)
        {
            registrations.Add(new LocaleRegistrationState(handle, values));
            RebuildCustomLocale(locale);
        }

        return handle;
    }

    private void RebuildCustomLocale(string locale)
    {
        customLocales.Remove(locale);
        Dictionary<string, string>? mergedValues = null;
        foreach (
            var registration in registrations.Where(candidate =>
                string.Equals(
                    candidate.Handle.Locale,
                    locale,
                    StringComparison.OrdinalIgnoreCase
                )
            )
        )
        {
            mergedValues ??= new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var (key, value) in registration.Values)
            {
                mergedValues[key] = value;
            }
        }

        if (mergedValues is not null)
        {
            customLocales[locale] = mergedValues;
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

    private static string NormalizeRuntimeLocale(string locale)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(locale);
        var normalized = locale.Trim().ToUpperInvariant();
        if (
            normalized.Any(character =>
                !char.IsLetterOrDigit(character) && character is not '-' and not '_'
            )
        )
        {
            throw new ArgumentException(
                "A locale can only contain letters, digits, '-' and '_'.",
                nameof(locale)
            );
        }

        return normalized;
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

    private sealed class LocaleRegistrationState(
        FlourishLocaleRegistration handle,
        IReadOnlyDictionary<string, string> values
    )
    {
        public FlourishLocaleRegistration Handle { get; } = handle;

        public IReadOnlyDictionary<string, string> Values { get; set; } = values;
    }
}
