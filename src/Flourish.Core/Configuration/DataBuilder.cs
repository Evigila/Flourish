using System;
using System.IO;

using ArkheideSystem.Flourish.Abstract;

namespace ArkheideSystem.Flourish.Configuration;

internal sealed class DataBuilder(ApplicationDataOptions options)
    : BuilderMutationGuard,
        IDataBuilder
{
    public IDataBuilder SetLocale(
        string locale = "en-US",
        bool usePersistedPreference = true
    )
    {
        ThrowIfFrozen();
        options.Locale = ValidateNotBlank(locale, nameof(locale)).Trim();
        options.UsePersistedLocale = usePersistedPreference;
        return this;
    }

    public IDataBuilder AddCultureFile(string path)
    {
        ThrowIfFrozen();
        options.CulturePaths.Add(ValidateNotBlank(path, nameof(path)).Trim());
        return this;
    }

    public IDataBuilder SetAppSettingsFilePath(string path = "appsettings.Flourish.json")
    {
        ThrowIfFrozen();
        options.AppSettingsFilePath = ResolveFilePath(path, nameof(path));
        return this;
    }

    public IDataBuilder SetProjectCatalogFilePath(string path = "projects.json")
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

        if (
            !string.Equals(Path.GetExtension(fullPath), ".json", StringComparison.OrdinalIgnoreCase)
        )
        {
            throw new ArgumentException(
                "A settings file path must use the .json extension.",
                parameterName
            );
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
