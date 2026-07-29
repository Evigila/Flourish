using System.IO;
using ArkheideSystem.Flourish.Internal.Configuration;
using ArkheideSystem.Flourish.Internal.Composition;

namespace ArkheideSystem.Flourish.Test.Internal.Composition;

public sealed class FlourishDataBuilderTests
{
    [Fact]
    public void ConfigurationMethods_WithValidValues_UpdateOptionsAndReturnBuilder()
    {
        var options = new FlourishDataOptions();
        var sut = new FlourishDataBuilder(options);

        Assert.Same(sut, sut.InitLocale(" en-US "));
        Assert.Same(sut, sut.AddLocaleFile(" Locales/lang_en-US.json "));

        Assert.Equal("en-US", options.Locale);
        Assert.Equal(["Locales/lang_en-US.json"], options.LocalePaths);
    }

    [Fact]
    public void InitLocale_LastCallControlsPersistencePolicy()
    {
        var options = new FlourishDataOptions();
        var sut = new FlourishDataBuilder(options);

        sut.InitLocale("zh-CN", usePersistedPreference: true);
        Assert.True(options.UsePersistedLocale);

        sut.InitLocale("en-US");
        Assert.True(options.UsePersistedLocale);

        sut.InitLocale("zh-CN", usePersistedPreference: false);
        Assert.False(options.UsePersistedLocale);
    }

    [Fact]
    public void StoragePaths_ResolveRelativeToApplicationDirectory()
    {
        var options = new FlourishDataOptions();
        var sut = new FlourishDataBuilder(options);

        Assert.Same(
            sut,
            sut.InitAppSettingsFilePath("Data/appsettings.Flourish.json")
        );
        Assert.Same(sut, sut.InitProjectCatalogFilePath("Data/projects.catalog.json"));

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
    public void InitAppSettingsFilePath_WithoutPathRestoresFlourishDefault()
    {
        var options = new FlourishDataOptions
        {
            AppSettingsFilePath = Path.Combine(
                AppContext.BaseDirectory,
                "Data",
                "custom.json"
            ),
        };
        var sut = new FlourishDataBuilder(options);

        Assert.Same(sut, sut.InitAppSettingsFilePath());
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
        var options = new FlourishDataOptions();
        var sut = new FlourishDataBuilder(options);

        var exception = Assert.Throws<ArgumentException>(() =>
        {
            switch (parameterName)
            {
                case "locale":
                    sut.InitLocale(value!);
                    break;
                case "localePath":
                    sut.AddLocaleFile(value!);
                    break;
                case "appSettingsPath":
                    sut.InitAppSettingsFilePath(value!);
                    break;
                case "projectCatalogPath":
                    sut.InitProjectCatalogFilePath(value!);
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
