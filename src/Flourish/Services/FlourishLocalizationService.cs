using System.Globalization;
using System.IO;
using System.Text.Json;
using ArkheideSystem.Flourish.Internal.Configuration;

namespace ArkheideSystem.Flourish.Services;

internal sealed class FlourishLocalizationService : IFlourishLocalization
{
    internal const string DefaultLocale = "en-US";
    internal const string CultureFileName = "FlourishCulture.Json";
    private const string EmbeddedResourceName =
        "ArkheideSystem.Flourish.Assets.FlourishCulture.Json";

    private readonly IReadOnlyDictionary<
        string,
        IReadOnlyDictionary<string, string>
    > builtInLocales;
    private readonly Dictionary<string, Dictionary<string, string>> customLocales = new(
        StringComparer.OrdinalIgnoreCase
    );
    private readonly List<CultureRegistrationState> registrations = [];
    private readonly FlourishDataOptions options;
    private readonly Lock gate = new();
    private string locale;

    public FlourishLocalizationService(FlourishDataOptions options)
        : this(options, AppContext.BaseDirectory) { }

    internal FlourishLocalizationService(FlourishDataOptions options, string baseDirectory)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(baseDirectory);
        this.options = options;

        locale = NormalizeLocale(options.Locale);
        builtInLocales = LoadEmbeddedCatalog();

        var automaticCulturePath = Path.Combine(baseDirectory, CultureFileName);
        if (File.Exists(automaticCulturePath))
        {
            RegisterFileCore(automaticCulturePath);
        }

        foreach (var path in options.CulturePaths)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                RegisterFileCore(path);
                continue;
            }

            var fullPath = Path.GetFullPath(path);
            if (!IsRegisteredPath(fullPath))
            {
                RegisterFileCore(fullPath);
            }
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
                    .Keys.Concat(customLocales.Keys)
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
                [normalizedLocale],
                null
            )
        );
    }

    public FlourishCultureRegistration RegisterFile(string path)
    {
        var registration = RegisterFileCore(path);
        var currentLocale = CurrentLocale;
        Changed?.Invoke(
            this,
            new FlourishLocalizationChangedEventArgs(
                FlourishLocalizationChangeKind.FileRegistered,
                currentLocale,
                currentLocale,
                registration.Locales,
                registration
            )
        );
        return registration;
    }

    public void ReloadFile(FlourishCultureRegistration registration)
    {
        ArgumentNullException.ThrowIfNull(registration);
        var values = LoadCultureFile(registration.FilePath);
        lock (gate)
        {
            var state = registrations.FirstOrDefault(candidate =>
                candidate.Handle.Id == registration.Id
            );
            if (state is null)
            {
                throw new InvalidOperationException("The culture-file registration is not active.");
            }

            state.Values = values;
            state.Handle.UpdateLocales(GetSortedLocales(values));
            RebuildCustomLocales();
        }

        var currentLocale = CurrentLocale;
        Changed?.Invoke(
            this,
            new FlourishLocalizationChangedEventArgs(
                FlourishLocalizationChangeKind.FileReloaded,
                currentLocale,
                currentLocale,
                registration.Locales,
                registration
            )
        );
    }

    public bool Unregister(FlourishCultureRegistration registration)
    {
        ArgumentNullException.ThrowIfNull(registration);
        FlourishCultureRegistration? removedRegistration;
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
            RebuildCustomLocales();
        }

        var currentLocale = CurrentLocale;
        Changed?.Invoke(
            this,
            new FlourishLocalizationChangedEventArgs(
                FlourishLocalizationChangeKind.FileUnregistered,
                currentLocale,
                currentLocale,
                removedRegistration.Locales,
                removedRegistration
            )
        );
        return true;
    }

    private FlourishCultureRegistration RegisterFileCore(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Culture file path cannot be empty.", nameof(path));
        }

        var fullPath = Path.GetFullPath(path);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"Culture file '{fullPath}' does not exist.", fullPath);
        }

        ValidateCultureFileName(fullPath);
        var values = LoadCultureFile(fullPath);
        var handle = new FlourishCultureRegistration(
            Guid.NewGuid(),
            GetSortedLocales(values),
            fullPath
        );
        lock (gate)
        {
            registrations.Add(new CultureRegistrationState(handle, values));
            RebuildCustomLocales();
        }

        return handle;
    }

    private bool IsRegisteredPath(string fullPath)
    {
        lock (gate)
        {
            return registrations.Any(registration =>
                string.Equals(
                    registration.Handle.FilePath,
                    fullPath,
                    StringComparison.OrdinalIgnoreCase
                )
            );
        }
    }

    private void RebuildCustomLocales()
    {
        customLocales.Clear();
        foreach (var registration in registrations)
        {
            foreach (var (locale, values) in registration.Values)
            {
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
        }
    }

    private static void ValidateCultureFileName(string path)
    {
        var fileName = Path.GetFileName(path);
        if (!string.Equals(fileName, CultureFileName, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                $"Culture file '{fileName}' must be named {CultureFileName}.",
                nameof(path)
            );
        }
    }

    private static IReadOnlyDictionary<
        string,
        IReadOnlyDictionary<string, string>
    > LoadEmbeddedCatalog()
    {
        var assembly = typeof(FlourishLocalizationService).Assembly;
        using var stream = assembly.GetManifestResourceStream(EmbeddedResourceName);
        if (stream is null)
        {
            throw new InvalidOperationException(
                $"Built-in culture resource '{EmbeddedResourceName}' could not be found."
            );
        }

        return ParseCatalog(stream, EmbeddedResourceName);
    }

    private static IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> LoadCultureFile(
        string path
    )
    {
        try
        {
            using var stream = File.OpenRead(path);
            return ParseCatalog(stream, path);
        }
        catch (InvalidDataException)
        {
            throw;
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            throw new InvalidDataException($"Culture file '{path}' could not be read.", error);
        }
    }

    private static IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> ParseCatalog(
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
                    $"Culture source '{sourceName}' must contain a JSON object."
                );
            }

            var keys = new HashSet<string>(StringComparer.Ordinal);
            var locales = new Dictionary<string, Dictionary<string, string>>(
                StringComparer.OrdinalIgnoreCase
            );
            foreach (var keyProperty in document.RootElement.EnumerateObject())
            {
                if (string.IsNullOrWhiteSpace(keyProperty.Name))
                {
                    throw new InvalidDataException(
                        $"Culture source '{sourceName}' contains an empty key."
                    );
                }

                if (!keys.Add(keyProperty.Name))
                {
                    throw new InvalidDataException(
                        $"Culture source '{sourceName}' contains duplicate key '{keyProperty.Name}'."
                    );
                }

                if (keyProperty.Value.ValueKind != JsonValueKind.Object)
                {
                    throw new InvalidDataException(
                        $"Culture source '{sourceName}' must define an object of locale values for key '{keyProperty.Name}'."
                    );
                }

                var keyLocales = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var localeProperty in keyProperty.Value.EnumerateObject())
                {
                    string normalizedLocale;
                    try
                    {
                        normalizedLocale = NormalizeRuntimeLocale(localeProperty.Name);
                    }
                    catch (ArgumentException error)
                    {
                        throw new InvalidDataException(
                            $"Culture source '{sourceName}' contains invalid locale '{localeProperty.Name}' for key '{keyProperty.Name}'.",
                            error
                        );
                    }

                    if (!keyLocales.Add(normalizedLocale))
                    {
                        throw new InvalidDataException(
                            $"Culture source '{sourceName}' contains duplicate locale '{normalizedLocale}' for key '{keyProperty.Name}'."
                        );
                    }

                    if (
                        localeProperty.Value.ValueKind != JsonValueKind.String
                        || string.IsNullOrWhiteSpace(localeProperty.Value.GetString())
                    )
                    {
                        throw new InvalidDataException(
                            $"Culture source '{sourceName}' contains an empty or non-string value for key '{keyProperty.Name}' and locale '{normalizedLocale}'."
                        );
                    }

                    if (!locales.TryGetValue(normalizedLocale, out var values))
                    {
                        values = new Dictionary<string, string>(StringComparer.Ordinal);
                        locales.Add(normalizedLocale, values);
                    }

                    values.Add(keyProperty.Name, localeProperty.Value.GetString()!);
                }

                if (keyLocales.Count == 0)
                {
                    throw new InvalidDataException(
                        $"Culture source '{sourceName}' does not define any locales for key '{keyProperty.Name}'."
                    );
                }
            }

            if (keys.Count == 0)
            {
                throw new InvalidDataException(
                    $"Culture source '{sourceName}' does not contain any translations."
                );
            }

            return locales.ToDictionary(
                pair => pair.Key,
                pair => (IReadOnlyDictionary<string, string>)pair.Value,
                StringComparer.OrdinalIgnoreCase
            );
        }
        catch (JsonException error)
        {
            throw new InvalidDataException(
                $"Culture source '{sourceName}' contains invalid JSON.",
                error
            );
        }
    }

    private static string[] GetSortedLocales(
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> values
    ) => [.. values.Keys.OrderBy(value => value, StringComparer.OrdinalIgnoreCase)];

    private static string NormalizeLocale(string? locale)
    {
        return string.IsNullOrWhiteSpace(locale) ? DefaultLocale : NormalizeRuntimeLocale(locale);
    }

    internal static bool TryNormalizeLocale(string? locale, out string normalizedLocale)
    {
        normalizedLocale = DefaultLocale;
        if (string.IsNullOrWhiteSpace(locale))
        {
            return false;
        }

        try
        {
            normalizedLocale = NormalizeRuntimeLocale(locale);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    private static string NormalizeRuntimeLocale(string locale)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(locale);
        var normalized = locale.Trim().Replace('_', '-');
        if (
            normalized.Any(character => !char.IsLetterOrDigit(character) && character is not '-')
            || normalized.Split('-').Any(string.IsNullOrEmpty)
        )
        {
            throw new ArgumentException(
                "A locale must contain non-empty letter or digit subtags separated by '-' or '_'.",
                nameof(locale)
            );
        }

        try
        {
            var canonical = CultureInfo.GetCultureInfo(normalized).Name;
            return canonical.Length == 0 ? CanonicalizeCustomLocale(normalized) : canonical;
        }
        catch (CultureNotFoundException)
        {
            return CanonicalizeCustomLocale(normalized);
        }
    }

    private static string CanonicalizeCustomLocale(string locale)
    {
        var subtags = locale.Split('-');
        for (var index = 0; index < subtags.Length; index++)
        {
            var subtag = subtags[index];
            subtags[index] = index switch
            {
                0 => subtag.ToLowerInvariant(),
                _ when subtag.Length == 4 && subtag.All(char.IsLetter) => char.ToUpperInvariant(
                    subtag[0]
                ) + subtag[1..].ToLowerInvariant(),
                _ when (subtag.Length == 2 && subtag.All(char.IsLetter))
                        || (subtag.Length == 3 && subtag.All(char.IsDigit)) =>
                    subtag.ToUpperInvariant(),
                _ => subtag.ToLowerInvariant(),
            };
        }

        return string.Join('-', subtags);
    }

    private static string? TryGet<TDictionary>(
        IReadOnlyDictionary<string, TDictionary> locales,
        string locale,
        string key
    )
        where TDictionary : IReadOnlyDictionary<string, string>
    {
        return locales.TryGetValue(locale, out var values) && values.TryGetValue(key, out var value)
            ? value
            : null;
    }

    private sealed class CultureRegistrationState(
        FlourishCultureRegistration handle,
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> values
    )
    {
        public FlourishCultureRegistration Handle { get; } = handle;

        public IReadOnlyDictionary<
            string,
            IReadOnlyDictionary<string, string>
        > Values { get; set; } = values;
    }
}
