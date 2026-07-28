using System.IO;
using ArkheideSystem.Flourish.Internal.Configuration;

namespace ArkheideSystem.Flourish.Test.Internal.Configuration;

public sealed class FlourishDataOptionsTests
{
    [Fact]
    public void Defaults_UseEnglishLocaleWithoutCustomFiles()
    {
        var options = new FlourishDataOptions();

        Assert.Equal("en-US", options.Locale);
        Assert.Empty(options.LocalePaths);
        Assert.True(options.UsePersistedLocale);
        Assert.Equal(
            Path.Combine(AppContext.BaseDirectory, "appsettings.Flourish.json"),
            options.AppSettingsFilePath
        );
        Assert.Equal(
            Path.Combine(AppContext.BaseDirectory, "projects.json"),
            options.ProjectCatalogFilePath
        );
    }
}
