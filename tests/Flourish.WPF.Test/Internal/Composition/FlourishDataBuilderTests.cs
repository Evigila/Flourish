using System;
using Xunit;
using ArkheideSystem.Flourish.Configuration;

using System.IO;

namespace ArkheideSystem.Flourish.WPF.Test.Internal.Composition;

public sealed class DataBuilderTests
{
    [Fact]
    public void ConfigurationMethods_WithValidValues_UpdateOptionsAndReturnBuilder()
    {
        var options = new ApplicationDataOptions();
        var sut = new DataBuilder(options);

        Assert.Same(sut, sut.SetLocale(" en-US "));
        Assert.Same(sut, sut.AddCultureFile(" Locales/FlourishCulture.Json "));

        Assert.Equal("en-US", options.Locale);
        Assert.Equal(["Locales/FlourishCulture.Json"], options.CulturePaths);
    }

    [Fact]
    public void SetLocale_LastCallControlsPersistencePolicy()
    {
        var options = new ApplicationDataOptions();
        var sut = new DataBuilder(options);

        sut.SetLocale("zh-CN", usePersistedPreference: true);
        Assert.True(options.UsePersistedLocale);

        sut.SetLocale("en-US");
        Assert.True(options.UsePersistedLocale);

        sut.SetLocale("zh-CN", usePersistedPreference: false);
        Assert.False(options.UsePersistedLocale);
    }

    [Fact]
    public void StoragePaths_ResolveRelativeToApplicationDirectory()
    {
        var options = new ApplicationDataOptions();
        var sut = new DataBuilder(options);

        Assert.Same(
            sut,
            sut.SetAppSettingsFilePath("Data/appsettings.Flourish.json")
        );
        Assert.Same(sut, sut.SetProjectCatalogFilePath("Data/projects.catalog.json"));

        Assert.Equal(
            Path.GetFullPath(
                "Data/appsettings.Flourish.json",
                AppContext.BaseDirectory
            ),
            options.AppSettingsFilePath
        );
        Assert.Equal(
            Path.GetFullPath("Data/projects.catalog.json", AppContext.BaseDirectory),
            options.ProjectCatalogFilePath
        );
    }

    [Fact]
    public void SetAppSettingsFilePath_WithoutPathRestoresFlourishDefault()
    {
        var options = new ApplicationDataOptions
        {
            AppSettingsFilePath = Path.Combine(
                AppContext.BaseDirectory,
                "Data",
                "custom.json"
            ),
        };
        var sut = new DataBuilder(options);

        Assert.Same(sut, sut.SetAppSettingsFilePath());
        Assert.Equal(
            Path.Combine(AppContext.BaseDirectory, "appsettings.Flourish.json"),
            options.AppSettingsFilePath
        );
    }

    [Theory]
    [InlineData("locale", null)]
    [InlineData("locale", "")]
    [InlineData("locale", "   ")]
    [InlineData("localePath", null)]
    [InlineData("localePath", "")]
    [InlineData("localePath", "   ")]
    [InlineData("appSettingsPath", null)]
    [InlineData("projectCatalogPath", "   ")]
    public void ConfigurationMethods_WithBlankValue_ThrowArgumentException(
        string parameterName,
        string? value
    )
    {
        var options = new ApplicationDataOptions();
        var sut = new DataBuilder(options);

        var exception = Assert.Throws<ArgumentException>(() =>
        {
            switch (parameterName)
            {
                case "locale":
                    sut.SetLocale(value!);
                    break;
                case "localePath":
                    sut.AddCultureFile(value!);
                    break;
                case "appSettingsPath":
                    sut.SetAppSettingsFilePath(value!);
                    break;
                case "projectCatalogPath":
                    sut.SetProjectCatalogFilePath(value!);
                    break;
            }
        });

        Assert.Equal(
            parameterName is "localePath" or "appSettingsPath" or "projectCatalogPath"
                ? "path"
                : parameterName,
            exception.ParamName
        );
    }
}
