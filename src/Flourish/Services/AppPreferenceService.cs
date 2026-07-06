using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using AcksheedSys.Flourish.Abstract;
using AcksheedSys.Flourish.Configuration;

namespace AcksheedSys.Flourish.Services;

internal sealed class AppPreferenceService(
    FlourishDataOptions dataOptions,
    FlourishShellOptions shellOptions
)
{
    private const string PreferenceFileName = "preferences.json";
    private static readonly JsonSerializerOptions SerializerOptions = CreateSerializerOptions();

    public string PreferenceDirectory => dataOptions.GetRequiredAppPreferenceDataPath(shellOptions);

    public string PreferenceFilePath => Path.Combine(PreferenceDirectory, PreferenceFileName);

    public FlourishTheme? ReadTheme()
    {
        if (!File.Exists(PreferenceFilePath))
        {
            return null;
        }

        try
        {
            var json = File.ReadAllText(PreferenceFilePath);
            return JsonSerializer
                .Deserialize<FlourishPreferenceData>(json, SerializerOptions)
                ?.Theme;
        }
        catch (IOException)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
    }

    public void SaveTheme(FlourishTheme theme)
    {
        Directory.CreateDirectory(PreferenceDirectory);
        var data = ReadPreferences() ?? new FlourishPreferenceData();
        data.Theme = theme;
        var json = JsonSerializer.Serialize(data, SerializerOptions);
        File.WriteAllText(PreferenceFilePath, json);
    }

    private FlourishPreferenceData? ReadPreferences()
    {
        if (!File.Exists(PreferenceFilePath))
        {
            return null;
        }

        try
        {
            var json = File.ReadAllText(PreferenceFilePath);
            return JsonSerializer.Deserialize<FlourishPreferenceData>(json, SerializerOptions);
        }
        catch (IOException)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
    }

    private static JsonSerializerOptions CreateSerializerOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            WriteIndented = true,
        };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }

    private sealed class FlourishPreferenceData
    {
        public FlourishTheme? Theme { get; set; }
    }
}
