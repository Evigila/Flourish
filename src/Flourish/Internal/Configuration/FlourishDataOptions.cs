using System.IO;

namespace ArkheideSystem.Flourish.Internal.Configuration;

internal sealed class FlourishDataOptions
{
    public string Locale { get; set; } = "en-US";

    public List<string> LocalePaths { get; } = [];

    public bool UsePersistedLocale { get; set; } = true;

    public string AppSettingsFilePath { get; set; } = Path.Combine(
        AppContext.BaseDirectory,
        "appsettings.Flourish.json"
    );

    public string ProjectCatalogFilePath { get; set; } = Path.Combine(
        AppContext.BaseDirectory,
        "projects.json"
    );
}
