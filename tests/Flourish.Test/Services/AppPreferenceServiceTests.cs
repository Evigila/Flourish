using System.IO;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Configuration;
using ArkheideSystem.Flourish.Services;

namespace ArkheideSystem.Flourish.Test.Services;

public sealed class AppPreferenceServiceTests
{
    [Fact]
    public void ReadTheme_WhenPreferenceFileDoesNotExist_ReturnsNull()
    {
        using var directory = new TemporaryDirectory();
        var sut = CreateService(directory.Path);

        var theme = sut.ReadTheme();

        Assert.Null(theme);
    }

    [Fact]
    public void SaveTheme_CreatesDirectoryAndRoundTripsTheme()
    {
        using var directory = new TemporaryDirectory(create: false);
        var sut = CreateService(directory.Path);

        sut.SaveTheme(FlourishTheme.Dark);

        Assert.True(Directory.Exists(directory.Path));
        Assert.True(File.Exists(sut.PreferenceFilePath));
        Assert.Equal(FlourishTheme.Dark, sut.ReadTheme());
    }

    [Fact]
    public void SaveTheme_WritesReadableStringEnumJson()
    {
        using var directory = new TemporaryDirectory();
        var sut = CreateService(directory.Path);

        sut.SaveTheme(FlourishTheme.Light);

        var json = File.ReadAllText(sut.PreferenceFilePath);
        Assert.Contains("\"theme\": \"Light\"", json);
    }

    [Fact]
    public void ReadTheme_WhenPreferenceFileContainsInvalidJson_ReturnsNull()
    {
        using var directory = new TemporaryDirectory();
        var sut = CreateService(directory.Path);
        File.WriteAllText(sut.PreferenceFilePath, "{ invalid json");

        var theme = sut.ReadTheme();

        Assert.Null(theme);
    }

    [Fact]
    public void SaveTheme_WhenPreferenceFileIsCorrupt_ReplacesItWithValidData()
    {
        using var directory = new TemporaryDirectory();
        var sut = CreateService(directory.Path);
        File.WriteAllText(sut.PreferenceFilePath, "{ invalid json");

        sut.SaveTheme(FlourishTheme.System);

        Assert.Equal(FlourishTheme.System, sut.ReadTheme());
    }

    private static AppPreferenceService CreateService(string preferencePath)
    {
        return new AppPreferenceService(
            new FlourishDataOptions { AppPreferenceDataPath = preferencePath },
            new FlourishShellOptions()
        );
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory(bool create = true)
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                "Flourish.Test",
                Guid.NewGuid().ToString("N")
            );

            if (create)
            {
                Directory.CreateDirectory(Path);
            }
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
