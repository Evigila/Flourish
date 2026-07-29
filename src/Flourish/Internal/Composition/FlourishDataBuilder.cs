using System.IO;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;

namespace ArkheideSystem.Flourish.Internal.Composition;

internal sealed class FlourishDataBuilder(FlourishDataOptions options)
    : FlourishBuilderMutationGuard,
        IFlourishDataBuilder
{
    public IFlourishDataBuilder InitLocale(
        string locale = "en-US",
        bool usePersistedPreference = true
    )
    {
        ThrowIfFrozen();
        options.Locale = ValidateNotBlank(locale, nameof(locale)).Trim();
        options.UsePersistedLocale = usePersistedPreference;
        return this;
    }

    public IFlourishDataBuilder AddLocaleFile(string path)
    {
        ThrowIfFrozen();
        options.LocalePaths.Add(ValidateNotBlank(path, nameof(path)).Trim());
        return this;
    }

    public IFlourishDataBuilder InitAppSettingsFilePath(
        string path = "appsettings.Flourish.json"
    )
    {
        ThrowIfFrozen();
        options.AppSettingsFilePath = ResolveFilePath(path, nameof(path));
        return this;
    }

    public IFlourishDataBuilder InitProjectCatalogFilePath(string path = "projects.json")
    {
        ThrowIfFrozen();
        options.ProjectCatalogFilePath = ResolveFilePath(path, nameof(path));
        return this;
    }

    internal static string ResolveFilePath(string path, string parameterName)
    {
        var value = ValidateNotBlank(path, parameterName).Trim();
        var fullPath = Path.GetFullPath(value, AppContext.BaseDirectory);
        if (string.IsNullOrWhiteSpace(Path.GetFileName(fullPath)))
        {
            throw new ArgumentException("A file path must include a file name.", parameterName);
        }

        if (Directory.Exists(fullPath))
        {
            throw new ArgumentException("A file path cannot identify a directory.", parameterName);
        }

        if (!string.Equals(Path.GetExtension(fullPath), ".json", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("A settings file path must use the .json extension.", parameterName);
        }

        return fullPath;
    }

    private static string ValidateNotBlank(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be empty.", parameterName);
        }

        return value;
    }
}
