using System;

using System.Collections.Generic;

using ArkheideSystem.Flourish.Abstract;
using System.IO;

namespace ArkheideSystem.Flourish.Configuration;

internal sealed class ApplicationDataOptions
{
    public string Locale { get; set; } = "en-US";

    public List<string> CulturePaths { get; } = [];

    public bool UsePersistedLocale { get; set; } = true;

    public string AppSettingsFilePath { get; set; } =
        Path.Combine(AppContext.BaseDirectory, "appsettings.Flourish.json");

    public string? ProjectCatalogFilePath { get; set; } =
        Path.Combine(AppContext.BaseDirectory, "projects.json");
}
