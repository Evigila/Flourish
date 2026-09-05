using System;
using Xunit;
using ArkheideSystem.Flourish.Configuration;

using System.IO;

namespace ArkheideSystem.Flourish.Core.Test.Configuration;

public sealed class ApplicationDataOptionsTests
{
    [Fact]
    public void Defaults_UseEnglishLocaleWithoutCustomFiles()
    {
        var options = new ApplicationDataOptions();

        Assert.Equal("en-US", options.Locale);
        Assert.Empty(options.CulturePaths);
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
