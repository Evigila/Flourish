using System;
using System.Collections.Generic;
using Xunit;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Configuration;
using ArkheideSystem.Flourish.Localization;
using ArkheideSystem.Flourish.Test.Infrastructure;

using System.IO;

namespace ArkheideSystem.Flourish.Test.Services;

public sealed class FlourishLocalizationServiceTests
{
    [Fact]
    public void Constructor_WithoutCultureConfiguration_UsesBuiltInEnglish()
    {
        var sut = CreateService();

        Assert.Equal("en-US", sut.Current.Locale);
        Assert.Equal("Back", sut.Get(FlourishLocaleKeys.TitleBarBack));
        Assert.Equal("User", sut.Get(FlourishLocaleKeys.ProfileDefaultName));
    }

    [Theory]
    [InlineData(" EN ", "en")]
    [InlineData("cn", "cn")]
    [InlineData("ZH_cn", "zh-CN")]
    [InlineData("pt_br", "pt-BR")]
    [InlineData("x_PRIVATE", "x-private")]
    public void Constructor_NormalizesLocaleIdentifiers(string locale, string expected)
    {
        var sut = CreateService(new FlourishDataOptions { Locale = locale });

        Assert.Equal(expected, sut.Current.Locale);
    }

    [Fact]
    public void AvailableLocales_ReturnsCanonicalBuiltInIdentifiers()
    {
        var sut = CreateService();

        Assert.Equal(["en-US", "zh-CN"], sut.Current.AvailableLocales);
    }

    [Fact]
    public void AutomaticCultureFile_OverridesIndividualCellsAndAddsLocales()
    {
        using var directory = new TemporaryDirectory();
        directory.WriteText(
            FlourishLocalizationService.CultureFileName,
            """
            {
              "Tray.Show": {
                "en-US": "Reveal",
                "es_es": "Mostrar"
              }
            }
            """
        );

        var sut = new FlourishLocalizationService(new FlourishDataOptions(), directory.Path);

        Assert.Equal(["en-US", "es-ES", "zh-CN"], sut.Current.AvailableLocales);
        Assert.Equal("Reveal", sut.Get(FlourishLocaleKeys.TrayShow));
        Assert.Equal("Exit", sut.Get(FlourishLocaleKeys.TrayExit));

        sut.SetLocale("es-ES");
        Assert.Equal("Mostrar", sut.Get(FlourishLocaleKeys.TrayShow));
        Assert.Equal("Exit", sut.Get(FlourishLocaleKeys.TrayExit));

        sut.SetLocale("zh-CN");
        Assert.Equal("显示", sut.Get(FlourishLocaleKeys.TrayShow));
    }

    [Fact]
    public void ExplicitCultureFiles_MergeByLocaleAndKeyInRegistrationOrder()
    {
        using var firstDirectory = new TemporaryDirectory();
        using var secondDirectory = new TemporaryDirectory();
        var firstPath = firstDirectory.WriteText(
            FlourishLocalizationService.CultureFileName,
            """
            {
              "Tray.Show": { "en-US": "Reveal", "fr-FR": "Afficher" },
              "Tray.Exit": { "en-US": "Quit" }
            }
            """
        );
        var secondPath = secondDirectory.WriteText(
            FlourishLocalizationService.CultureFileName,
            """
            {
              "Tray.Show": { "en-US": "Open" }
            }
            """
        );
        var options = new FlourishDataOptions();
        options.CulturePaths.Add(firstPath);
        options.CulturePaths.Add(secondPath);

        var sut = CreateService(options);

        Assert.Equal("Open", sut.Get(FlourishLocaleKeys.TrayShow));
        Assert.Equal("Quit", sut.Get(FlourishLocaleKeys.TrayExit));
        sut.SetLocale("fr-FR");
        Assert.Equal("Afficher", sut.Get(FlourishLocaleKeys.TrayShow));
        Assert.Equal("Quit", sut.Get(FlourishLocaleKeys.TrayExit));
    }

    [Fact]
    public void Get_WhenKeyDoesNotExist_ReturnsKey()
    {
        var sut = CreateService();

        Assert.Equal("Missing.Key", sut.Get("Missing.Key"));
    }

    [Fact]
    public void BuiltInCultures_ContainEveryCanonicalKey()
    {
        foreach (var locale in new[] { "zh-CN", "en-US" })
        {
            var sut = CreateService(new FlourishDataOptions { Locale = locale });

            foreach (var key in FlourishLocaleKeys.All)
            {
                Assert.NotEqual(key, sut.Get(key));
                Assert.False(string.IsNullOrWhiteSpace(sut.Get(key)));
            }
        }
    }

    [Fact]
    public void Constructor_WhenCultureFileDoesNotExist_ThrowsClearFileNotFoundException()
    {
        var options = new FlourishDataOptions();
        options.CulturePaths.Add(
            Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid():N}", "FlourishCulture.Json")
        );

        var exception = Assert.Throws<FileNotFoundException>(() => CreateService(options));

        Assert.Contains("does not exist", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Constructor_WhenCultureFilePathIsBlank_ThrowsClearArgumentException()
    {
        var options = new FlourishDataOptions();
        options.CulturePaths.Add("   ");

        var exception = Assert.Throws<ArgumentException>(() => CreateService(options));

        Assert.Contains("cannot be empty", exception.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("translations.json")]
    [InlineData("FlourishCulture.txt")]
    [InlineData("LegacyCulture.json")]
    public void Constructor_WhenCultureFileNameIsInvalid_ThrowsClearArgumentException(
        string fileName
    )
    {
        using var directory = new TemporaryDirectory();
        var path = directory.WriteText(fileName, "{ \"Tray.Show\": { \"en-US\": \"Show\" } }");
        var options = new FlourishDataOptions();
        options.CulturePaths.Add(path);

        var exception = Assert.Throws<ArgumentException>(() => CreateService(options));

        Assert.Contains("FlourishCulture.Json", exception.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("{ invalid json")]
    [InlineData("[]")]
    [InlineData("{}")]
    [InlineData("{ \"Tray.Show\": null }")]
    [InlineData("{ \"Tray.Show\": {} }")]
    [InlineData("{ \"Tray.Show\": { \"en-US\": null } }")]
    [InlineData("{ \"Tray.Show\": { \"en-US\": \"   \" } }")]
    [InlineData("{ \"Tray.Show\": { \"en-US\": 1 } }")]
    [InlineData("{ \"\": { \"en-US\": \"Show\" } }")]
    [InlineData("{ \"Tray.Show\": { \"en-US\": \"Show\", \"EN_us\": \"Open\" } }")]
    [InlineData(
        "{ \"Tray.Show\": { \"en-US\": \"Show\" }, \"Tray.Show\": { \"zh-CN\": \"显示\" } }"
    )]
    public void Constructor_WhenCultureJsonIsInvalid_ThrowsClearInvalidDataException(string json)
    {
        using var directory = new TemporaryDirectory();
        var path = directory.WriteText(FlourishLocalizationService.CultureFileName, json);
        var options = new FlourishDataOptions();
        options.CulturePaths.Add(path);

        var exception = Assert.Throws<InvalidDataException>(() => CreateService(options));

        Assert.Contains("Culture", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void SetLocale_ChangesSelectedLocaleAndRaisesChanged()
    {
        var sut = CreateService();
        FlourishLocalizationChangedEventArgs? change = null;
        sut.Changed += (_, args) => change = args;

        sut.SetLocale(" zh_CN ");

        Assert.Equal("zh-CN", sut.Current.Locale);
        Assert.NotNull(change);
        Assert.Equal(FlourishLocalizationChangeKind.LocaleChanged, change.Kind);
        Assert.Equal("en-US", change.PreviousLocale);
        Assert.Equal("zh-CN", change.CurrentLocale);
        Assert.Equal(["zh-CN"], change.AffectedLocales);
    }

    [Theory]
    [InlineData("-en")]
    [InlineData("en-")]
    [InlineData("en--US")]
    [InlineData("en__US")]
    public void SetLocale_WhenSubtagsAreEmpty_ThrowsClearArgumentException(string locale)
    {
        var sut = CreateService();

        var exception = Assert.Throws<ArgumentException>(() => sut.SetLocale(locale));

        Assert.Contains("non-empty", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void RuntimeCultureFile_CanBeRegisteredReloadedAndUnregistered()
    {
        using var directory = new TemporaryDirectory();
        var path = directory.WriteText(
            FlourishLocalizationService.CultureFileName,
            """
            {
              "Tray.Show": { "fr-FR": "Afficher", "de-DE": "Anzeigen" }
            }
            """
        );
        var sut = CreateService();
        var changes = new List<FlourishLocalizationChangeKind>();
        sut.Changed += (_, args) => changes.Add(args.Kind);

        var registration = sut.RegisterFile(path);
        Assert.Equal(["de-DE", "fr-FR"], registration.Locales);
        sut.SetLocale("fr-FR");
        Assert.Equal("Afficher", sut.Get(FlourishLocaleKeys.TrayShow));

        directory.WriteText(
            FlourishLocalizationService.CultureFileName,
            """
            {
              "Tray.Show": { "fr-FR": "Ouvrir" }
            }
            """
        );
        registration.Reload();
        Assert.Equal(["fr-FR"], registration.Locales);
        Assert.DoesNotContain("de-DE", sut.Current.AvailableLocales);
        Assert.Equal("Ouvrir", sut.Get(FlourishLocaleKeys.TrayShow));

        registration.Dispose();
        Assert.False(registration.IsRegistered);
        registration.Dispose();
        Assert.DoesNotContain("fr-FR", sut.Current.AvailableLocales);
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
    public void Dispose_LaterOverride_RevealsEarlierRegistration()
    {
        using var firstDirectory = new TemporaryDirectory();
        using var secondDirectory = new TemporaryDirectory();
        var first = firstDirectory.WriteText(
            FlourishLocalizationService.CultureFileName,
            "{ \"Tray.Show\": { \"fr-FR\": \"First\" } }"
        );
        var second = secondDirectory.WriteText(
            FlourishLocalizationService.CultureFileName,
            "{ \"Tray.Show\": { \"fr-FR\": \"Second\" } }"
        );
        var sut = CreateService();
        sut.RegisterFile(first);
        var overrideRegistration = sut.RegisterFile(second);
        sut.SetLocale("fr-FR");
        Assert.Equal("Second", sut.Get(FlourishLocaleKeys.TrayShow));

        overrideRegistration.Dispose();

        Assert.Equal("First", sut.Get(FlourishLocaleKeys.TrayShow));
    }

    private static FlourishLocalizationService CreateService(FlourishDataOptions? options = null) =>
        new(
            options ?? new FlourishDataOptions(),
            Path.Combine(Path.GetTempPath(), $"Arkheide.Flourish.Test-{Guid.NewGuid():N}")
        );
}
