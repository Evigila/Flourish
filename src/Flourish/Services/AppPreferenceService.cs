using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using ArkheideSystem.Flourish.Abstract;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace ArkheideSystem.Flourish.Services;

internal sealed class AppPreferenceService(
    IConfiguration configuration,
    IHostEnvironment hostEnvironment
)
{
    private const string AppSettingsFileName = "appsettings.json";
    private const string ThemeConfigurationKey = "Flourish:Preferences:Theme";
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
    };
    private readonly object gate = new();

    public string AppSettingsFilePath =>
        Path.Combine(hostEnvironment.ContentRootPath, AppSettingsFileName);

    public FlourishTheme? ReadTheme()
    {
        var value = configuration[ThemeConfigurationKey];
        if (
            string.IsNullOrWhiteSpace(value)
            || !Enum.TryParse(value, ignoreCase: true, out FlourishTheme theme)
            || !Enum.IsDefined(theme)
        )
        {
            return null;
        }

        return theme;
    }

    public void SaveTheme(FlourishTheme theme)
    {
        lock (gate)
        {
            var root = ReadAppSettings();
            var flourish = GetOrCreateObject(root, "Flourish", "Flourish");
            var preferences = GetOrCreateObject(
                flourish,
                "Preferences",
                "Flourish:Preferences"
            );
            SetProperty(preferences, "Theme", JsonValue.Create(theme.ToString()));
            WriteAppSettings(root);

            if (configuration is IConfigurationRoot configurationRoot)
            {
                configurationRoot.Reload();
            }
        }
    }

    private JsonObject ReadAppSettings()
    {
        if (!File.Exists(AppSettingsFilePath))
        {
            return [];
        }

        try
        {
            using var stream = new FileStream(
                AppSettingsFilePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite | FileShare.Delete
            );
            var node = JsonNode.Parse(
                stream,
                documentOptions: new JsonDocumentOptions
                {
                    AllowTrailingCommas = true,
                    CommentHandling = JsonCommentHandling.Skip,
                }
            );
            return node as JsonObject
                ?? throw new InvalidDataException(
                    $"{AppSettingsFileName} must contain a JSON object."
                );
        }
        catch (JsonException error)
        {
            throw new InvalidDataException(
                $"{AppSettingsFileName} contains invalid JSON and was not changed.",
                error
            );
        }
    }

    private void WriteAppSettings(JsonObject root)
    {
        var directory = Path.GetDirectoryName(AppSettingsFilePath)
            ?? throw new InvalidOperationException(
                $"{AppSettingsFileName} has no parent directory."
            );
        Directory.CreateDirectory(directory);
        var temporaryPath = Path.Combine(
            directory,
            $".{AppSettingsFileName}.{Guid.NewGuid():N}.tmp"
        );

        try
        {
            File.WriteAllText(temporaryPath, root.ToJsonString(SerializerOptions));
            File.Move(temporaryPath, AppSettingsFilePath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }

    private static JsonObject GetOrCreateObject(
        JsonObject parent,
        string propertyName,
        string configurationPath
    )
    {
        var existingName = FindPropertyName(parent, propertyName);
        if (existingName is null)
        {
            var created = new JsonObject();
            parent[propertyName] = created;
            return created;
        }

        return parent[existingName] as JsonObject
            ?? throw new InvalidDataException(
                $"The {AppSettingsFileName} value '{configurationPath}' must be a JSON object."
            );
    }

    private static void SetProperty(
        JsonObject parent,
        string propertyName,
        JsonNode? value
    )
    {
        parent[FindPropertyName(parent, propertyName) ?? propertyName] = value;
    }

    private static string? FindPropertyName(JsonObject parent, string propertyName)
    {
        return parent
            .Select(property => property.Key)
            .FirstOrDefault(existingName =>
                string.Equals(existingName, propertyName, StringComparison.OrdinalIgnoreCase)
            );
    }
}
