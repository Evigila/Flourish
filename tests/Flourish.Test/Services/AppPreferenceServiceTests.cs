using System.IO;
using System.Text.Json;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Moq;

namespace ArkheideSystem.Flourish.Test.Services;

public sealed class AppPreferenceServiceTests
{
    [Fact]
    public void ReadTheme_WhenConfigurationValueDoesNotExist_ReturnsNull()
    {
        using var directory = new TemporaryDirectory();
        var sut = CreateService(directory.Path);

        var theme = sut.ReadTheme();

        Assert.Null(theme);
    }

    [Theory]
    [InlineData("System", FlourishTheme.System)]
    [InlineData("light", FlourishTheme.Light)]
    [InlineData("DARK", FlourishTheme.Dark)]
    public void ReadTheme_UsesHostConfiguration(string value, FlourishTheme expected)
    {
        using var directory = new TemporaryDirectory();
        WriteAppSettings(
            directory.Path,
            $$"""
            {
              "Flourish": {
                "Preferences": {
                  "Theme": "{{value}}"
                }
              }
            }
            """
        );
        var sut = CreateService(directory.Path);

        var theme = sut.ReadTheme();

        Assert.Equal(expected, theme);
    }

    [Fact]
    public void ReadTheme_WhenConfigurationValueIsInvalid_ReturnsNull()
    {
        using var directory = new TemporaryDirectory();
        WriteAppSettings(
            directory.Path,
            """
            {
              "Flourish": {
                "Preferences": {
                  "Theme": "Sepia"
                }
              }
            }
            """
        );
        var sut = CreateService(directory.Path);

        var theme = sut.ReadTheme();

        Assert.Null(theme);
    }

    [Fact]
    public void SaveTheme_CreatesAppSettingsAndRoundTripsThroughHostConfiguration()
    {
        using var directory = new TemporaryDirectory();
        var sut = CreateService(directory.Path);

        sut.SaveTheme(FlourishTheme.Dark);

        Assert.True(File.Exists(sut.AppSettingsFilePath));
        Assert.Equal(FlourishTheme.Dark, sut.ReadTheme());
        Assert.Empty(
            Directory.EnumerateFiles(directory.Path, ".appsettings.json.*.tmp")
        );

        using var document = JsonDocument.Parse(File.ReadAllText(sut.AppSettingsFilePath));
        Assert.Equal(
            "Dark",
            document.RootElement
                .GetProperty("Flourish")
                .GetProperty("Preferences")
                .GetProperty("Theme")
                .GetString()
        );
    }

    [Fact]
    public void SaveTheme_PreservesUnrelatedAppSettingsAndExistingPropertyCasing()
    {
        using var directory = new TemporaryDirectory();
        WriteAppSettings(
            directory.Path,
            """
            {
              "Logging": {
                "LogLevel": {
                  "Default": "Information"
                }
              },
              "flourish": {
                "FeatureFlag": true,
                "preferences": {
                  "WindowMode": "Compact"
                }
              }
            }
            """
        );
        var sut = CreateService(directory.Path);

        sut.SaveTheme(FlourishTheme.System);

        using var document = JsonDocument.Parse(File.ReadAllText(sut.AppSettingsFilePath));
        Assert.Equal(
            "Information",
            document.RootElement
                .GetProperty("Logging")
                .GetProperty("LogLevel")
                .GetProperty("Default")
                .GetString()
        );
        var flourish = document.RootElement.GetProperty("flourish");
        Assert.True(flourish.GetProperty("FeatureFlag").GetBoolean());
        var preferences = flourish.GetProperty("preferences");
        Assert.Equal("Compact", preferences.GetProperty("WindowMode").GetString());
        Assert.Equal("System", preferences.GetProperty("Theme").GetString());
    }

    [Fact]
    public void SaveTheme_WhenAppSettingsContainsInvalidJson_DoesNotOverwriteFile()
    {
        using var directory = new TemporaryDirectory();
        var sut = CreateService(directory.Path);
        const string invalidJson = "{ invalid json";
        WriteAppSettings(directory.Path, invalidJson);

        var exception = Assert.Throws<InvalidDataException>(() =>
            sut.SaveTheme(FlourishTheme.Light)
        );

        Assert.Contains("invalid JSON", exception.Message);
        Assert.Equal(invalidJson, File.ReadAllText(sut.AppSettingsFilePath));
    }

    [Fact]
    public void SaveTheme_WhenFlourishSectionIsNotAnObject_DoesNotOverwriteFile()
    {
        using var directory = new TemporaryDirectory();
        const string originalJson = """
            {
              "Flourish": "invalid section"
            }
            """;
        WriteAppSettings(directory.Path, originalJson);
        var sut = CreateService(directory.Path);

        var exception = Assert.Throws<InvalidDataException>(() =>
            sut.SaveTheme(FlourishTheme.Light)
        );

        Assert.Contains("Flourish", exception.Message);
        Assert.Equal(originalJson, File.ReadAllText(sut.AppSettingsFilePath));
    }

    [Fact]
    public void SaveTheme_WhenCalledConcurrently_LeavesValidAppSettings()
    {
        using var directory = new TemporaryDirectory();
        var sut = CreateService(directory.Path);
        var themes = new[]
        {
            FlourishTheme.System,
            FlourishTheme.Light,
            FlourishTheme.Dark,
        };

        Parallel.For(0, 24, index => sut.SaveTheme(themes[index % themes.Length]));

        using var document = JsonDocument.Parse(File.ReadAllText(sut.AppSettingsFilePath));
        var persistedTheme = document.RootElement
            .GetProperty("Flourish")
            .GetProperty("Preferences")
            .GetProperty("Theme")
            .GetString();
        Assert.True(Enum.TryParse<FlourishTheme>(persistedTheme, out var parsedTheme));
        Assert.Contains(parsedTheme, themes);
        Assert.Empty(
            Directory.EnumerateFiles(directory.Path, ".appsettings.json.*.tmp")
        );
    }

    private static AppPreferenceService CreateService(string contentRootPath)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(contentRootPath)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .Build();
        var hostEnvironment = new Mock<IHostEnvironment>();
        hostEnvironment.SetupGet(environment => environment.ContentRootPath)
            .Returns(contentRootPath);
        return new AppPreferenceService(configuration, hostEnvironment.Object);
    }

    private static void WriteAppSettings(string directoryPath, string json)
    {
        File.WriteAllText(Path.Combine(directoryPath, "appsettings.json"), json);
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                "Flourish.Test",
                Guid.NewGuid().ToString("N")
            );
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
