using System.IO;
using System.Text;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;
using ArkheideSystem.Flourish.Services;

namespace ArkheideSystem.Flourish.Test.Services;

public sealed class FlourishLocalizationServiceTests
{
    [Fact]
    public void Constructor_WithoutLocaleConfiguration_UsesBuiltInEnglish()
    {
        var sut = new FlourishLocalizationService(new FlourishDataOptions());

        Assert.Equal("EN", sut.CurrentLocale);
        Assert.Equal("Back", sut.Get(FlourishLocaleKeys.TitleBarBack));
        Assert.Equal("User", sut.Get(FlourishLocaleKeys.ProfileDefaultName));
    }

    [Fact]
    public void Constructor_WithEnglishLocale_UsesBuiltInEnglishAndFormatsValues()
    {
        var sut = new FlourishLocalizationService(
            new FlourishDataOptions { Locale = " en " }
        );

        Assert.Equal("EN", sut.CurrentLocale);
        Assert.Equal("Back", sut.Get(FlourishLocaleKeys.TitleBarBack));
        Assert.Equal(
            "Theme: System (Dark)",
            sut.Format(FlourishLocaleKeys.TitleBarThemeSystem, "Dark")
        );
    }

    [Fact]
    public void Get_CustomSelectedLocale_OverridesBuiltInAndFallsBackToBuiltInSelectedLocale()
    {
        using var directory = new TemporaryDirectory();
        var path = directory.WriteLocale(
            "lang_EN.json",
            """
            {
              "Tray.Show": "Reveal"
            }
            """
        );
        var options = new FlourishDataOptions { Locale = "EN" };
        options.LocalePaths.Add(path);

        var sut = new FlourishLocalizationService(options);

        Assert.Equal("Reveal", sut.Get(FlourishLocaleKeys.TrayShow));
        Assert.Equal("Exit", sut.Get(FlourishLocaleKeys.TrayExit));
    }

    [Fact]
    public void Get_UnknownLocale_FallsBackToCustomEnglishBeforeBuiltInEnglish()
    {
        using var directory = new TemporaryDirectory();
        var path = directory.WriteLocale(
            "lang_EN.json",
            """
            {
              "Tray.Show": "Reveal"
            }
            """
        );
        var options = new FlourishDataOptions { Locale = "FR" };
        options.LocalePaths.Add(path);

        var sut = new FlourishLocalizationService(options);

        Assert.Equal("Reveal", sut.Get(FlourishLocaleKeys.TrayShow));
        Assert.Equal("Exit", sut.Get(FlourishLocaleKeys.TrayExit));
    }

    [Fact]
    public void Get_BuiltInSelectedLocale_IsUsedBeforeCustomEnglishFallback()
    {
        using var directory = new TemporaryDirectory();
        var path = directory.WriteLocale(
            "lang_EN.json",
            """
            {
              "Tray.Exit": "Quit"
            }
            """
        );
        var options = new FlourishDataOptions { Locale = "CN" };
        options.LocalePaths.Add(path);

        var sut = new FlourishLocalizationService(options);

        Assert.Equal("退出", sut.Get(FlourishLocaleKeys.TrayExit));
    }

    [Fact]
    public void Get_CustomUnknownSelectedLocale_IsUsedBeforeEnglishFallback()
    {
        using var directory = new TemporaryDirectory();
        var path = directory.WriteLocale(
            "lang_FR.json",
            """
            {
              "Tray.Show": "Afficher"
            }
            """
        );
        var options = new FlourishDataOptions { Locale = "FR" };
        options.LocalePaths.Add(path);

        var sut = new FlourishLocalizationService(options);

        Assert.Equal("Afficher", sut.Get(FlourishLocaleKeys.TrayShow));
        Assert.Equal("Exit", sut.Get(FlourishLocaleKeys.TrayExit));
    }

    [Fact]
    public void Constructor_WithRepeatedLocaleFiles_MergesValuesAndAppliesLaterOverrides()
    {
        using var firstDirectory = new TemporaryDirectory();
        using var secondDirectory = new TemporaryDirectory();
        var firstPath = firstDirectory.WriteLocale(
            "lang_EN.json",
            """
            {
              "Tray.Show": "Reveal",
              "Tray.Exit": "Quit"
            }
            """
        );
        var secondPath = secondDirectory.WriteLocale(
            "lang_en.json",
            """
            {
              "Tray.Show": "Open"
            }
            """
        );
        var options = new FlourishDataOptions { Locale = "EN" };
        options.LocalePaths.Add(firstPath);
        options.LocalePaths.Add(secondPath);

        var sut = new FlourishLocalizationService(options);

        Assert.Equal("Open", sut.Get(FlourishLocaleKeys.TrayShow));
        Assert.Equal("Quit", sut.Get(FlourishLocaleKeys.TrayExit));
    }

    [Fact]
    public void Get_WhenKeyDoesNotExist_ReturnsKey()
    {
        var sut = new FlourishLocalizationService(new FlourishDataOptions());

        Assert.Equal("Missing.Key", sut.Get("Missing.Key"));
    }

    [Fact]
    public void BuiltInLocales_ContainEveryCanonicalKey()
    {
        foreach (var locale in new[] { "CN", "EN" })
        {
            var sut = new FlourishLocalizationService(
                new FlourishDataOptions { Locale = locale }
            );

            foreach (var key in FlourishLocaleKeys.All)
            {
                Assert.NotEqual(key, sut.Get(key));
                Assert.False(string.IsNullOrWhiteSpace(sut.Get(key)));
            }
        }
    }

    [Fact]
    public void Constructor_WhenLocaleFileDoesNotExist_ThrowsClearFileNotFoundException()
    {
        var options = new FlourishDataOptions();
        options.LocalePaths.Add(
            Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid():N}", "lang_EN.json")
        );

        var exception = Assert.Throws<FileNotFoundException>(() =>
            new FlourishLocalizationService(options)
        );

        Assert.Contains("does not exist", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Constructor_WhenLocaleFilePathIsBlank_ThrowsClearArgumentException()
    {
        var options = new FlourishDataOptions();
        options.LocalePaths.Add("   ");

        var exception = Assert.Throws<ArgumentException>(() =>
            new FlourishLocalizationService(options)
        );

        Assert.Contains("cannot be empty", exception.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("translations.json")]
    [InlineData("lang_.json")]
    [InlineData("lang_EN.txt")]
    [InlineData("lang_EN!.json")]
    public void Constructor_WhenLocaleFileNameIsInvalid_ThrowsClearArgumentException(
        string fileName
    )
    {
        using var directory = new TemporaryDirectory();
        var path = directory.WriteLocale(fileName, "{ \"Tray.Show\": \"Show\" }");
        var options = new FlourishDataOptions();
        options.LocalePaths.Add(path);

        var exception = Assert.Throws<ArgumentException>(() =>
            new FlourishLocalizationService(options)
        );

        Assert.Contains("lang_<locale>.json", exception.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("{ invalid json")]
    [InlineData("[]")]
    [InlineData("{}")]
    [InlineData("{ \"Tray.Show\": null }")]
    [InlineData("{ \"Tray.Show\": \"   \" }")]
    [InlineData("{ \"Tray.Show\": 1 }")]
    [InlineData("{ \"\": \"Show\" }")]
    [InlineData("{ \"Tray.Show\": \"Show\", \"Tray.Show\": \"Open\" }")]
    public void Constructor_WhenLocaleJsonIsInvalid_ThrowsClearInvalidDataException(
        string json
    )
    {
        using var directory = new TemporaryDirectory();
        var path = directory.WriteLocale("lang_EN.json", json);
        var options = new FlourishDataOptions();
        options.LocalePaths.Add(path);

        var exception = Assert.Throws<InvalidDataException>(() =>
            new FlourishLocalizationService(options)
        );

        Assert.Contains("Locale", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void SetLocale_ChangesSelectedLocaleAndRaisesChanged()
    {
        var sut = new FlourishLocalizationService(new FlourishDataOptions());
        FlourishLocalizationChangedEventArgs? change = null;
        sut.Changed += (_, args) => change = args;

        sut.SetLocale(" cn ");

        Assert.Equal("CN", sut.CurrentLocale);
        Assert.Equal("CN", sut.CurrentLocale);
        Assert.NotNull(change);
        Assert.Equal(FlourishLocalizationChangeKind.LocaleChanged, change.Kind);
        Assert.Equal("EN", change.PreviousLocale);
        Assert.Equal("CN", change.CurrentLocale);
    }

    [Fact]
    public void RuntimeLocaleFile_CanBeRegisteredReloadedAndUnregistered()
    {
        using var directory = new TemporaryDirectory();
        var path = directory.WriteLocale(
            "lang_FR.json",
            """
            {
              "Tray.Show": "Afficher"
            }
            """
        );
        var sut = new FlourishLocalizationService(new FlourishDataOptions());
        var changes = new List<FlourishLocalizationChangeKind>();
        sut.Changed += (_, args) => changes.Add(args.Kind);

        var registration = sut.RegisterFile(path);
        sut.SetLocale("FR");
        Assert.Contains("FR", sut.AvailableLocales);
        Assert.Equal("Afficher", sut.Get(FlourishLocaleKeys.TrayShow));

        directory.WriteLocale(
            "lang_FR.json",
            """
            {
              "Tray.Show": "Ouvrir"
            }
            """
        );
        sut.ReloadFile(registration);
        Assert.Equal("Ouvrir", sut.Get(FlourishLocaleKeys.TrayShow));

        Assert.True(sut.Unregister(registration));
        Assert.False(sut.Unregister(registration));
        Assert.DoesNotContain("FR", sut.AvailableLocales);
        Assert.Equal("Show", sut.Get(FlourishLocaleKeys.TrayShow));
        Assert.Equal(
            [
                FlourishLocalizationChangeKind.FileRegistered,
                FlourishLocalizationChangeKind.LocaleChanged,
                FlourishLocalizationChangeKind.FileReloaded,
                FlourishLocalizationChangeKind.FileUnregistered,
            ],
            changes
        );
    }

    [Fact]
    public void Unregister_LaterOverride_RevealsEarlierRegistration()
    {
        using var firstDirectory = new TemporaryDirectory();
        using var secondDirectory = new TemporaryDirectory();
        var first = firstDirectory.WriteLocale(
            "lang_FR.json",
            "{ \"Tray.Show\": \"First\" }"
        );
        var second = secondDirectory.WriteLocale(
            "lang_FR.json",
            "{ \"Tray.Show\": \"Second\" }"
        );
        var sut = new FlourishLocalizationService(new FlourishDataOptions());
        sut.RegisterFile(first);
        var overrideRegistration = sut.RegisterFile(second);
        sut.SetLocale("FR");
        Assert.Equal("Second", sut.Get(FlourishLocaleKeys.TrayShow));

        sut.Unregister(overrideRegistration);

        Assert.Equal("First", sut.Get(FlourishLocaleKeys.TrayShow));
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

        public string WriteLocale(string fileName, string json)
        {
            var path = System.IO.Path.Combine(Path, fileName);
            File.WriteAllText(path, json, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            return path;
        }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
