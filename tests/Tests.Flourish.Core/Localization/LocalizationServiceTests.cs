using System;
using System.Collections.Generic;
using Xunit;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Configuration;
using ArkheideSystem.Flourish.Localization;

using System.IO;

namespace ArkheideSystem.Tests.Flourish.Core.Localization;

public sealed class LocalizationServiceTests
{
    [Fact]
    public void Constructor_WithoutCultureConfiguration_UsesBuiltInEnglish()
    {
        var sut = CreateService();

        Assert.Equal("en-US", sut.Current.Locale);
        Assert.Equal("Back", sut.Get(LocaleKeys.TitleBarBack));
        Assert.Equal("User", sut.Get(LocaleKeys.ProfileDefaultName));
    }

    [Theory]
    [InlineData(" EN ", "en")]
    [InlineData("cn", "cn")]
    [InlineData("ZH_cn", "zh-CN")]
    [InlineData("pt_br", "pt-BR")]
    [InlineData("x_PRIVATE", "x-private")]
    public void Constructor_NormalizesLocaleIdentifiers(string locale, string expected)
    {
        var sut = CreateService(new ApplicationDataOptions { Locale = locale });

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
            LocalizationService.CultureFileName,
            """
            {
              "Tray.Show": {
                "en-US": "Reveal",
                "es_es": "Mostrar"
              }
            }
            """
        );

        var sut = new LocalizationService(new ApplicationDataOptions(), directory.Path);

        Assert.Equal(["en-US", "es-ES", "zh-CN"], sut.Current.AvailableLocales);
        Assert.Equal("Reveal", sut.Get(LocaleKeys.TrayShow));
        Assert.Equal("Exit", sut.Get(LocaleKeys.TrayExit));

        sut.SetLocale("es-ES");
        Assert.Equal("Mostrar", sut.Get(LocaleKeys.TrayShow));
        Assert.Equal("Exit", sut.Get(LocaleKeys.TrayExit));

        sut.SetLocale("zh-CN");
        Assert.Equal("显示", sut.Get(LocaleKeys.TrayShow));
    }

    [Fact]
    public void ExplicitCultureFiles_MergeByLocaleAndKeyInRegistrationOrder()
    {
        using var firstDirectory = new TemporaryDirectory();
        using var secondDirectory = new TemporaryDirectory();
        var firstPath = firstDirectory.WriteText(
            LocalizationService.CultureFileName,
            """
            {
              "Tray.Show": { "en-US": "Reveal", "fr-FR": "Afficher" },
              "Tray.Exit": { "en-US": "Quit" }
            }
            """
        );
        var secondPath = secondDirectory.WriteText(
            LocalizationService.CultureFileName,
            """
            {
              "Tray.Show": { "en-US": "Open" }
            }
            """
        );
        var options = new ApplicationDataOptions();
        options.CulturePaths.Add(firstPath);
        options.CulturePaths.Add(secondPath);

        var sut = CreateService(options);

        Assert.Equal("Open", sut.Get(LocaleKeys.TrayShow));
        Assert.Equal("Quit", sut.Get(LocaleKeys.TrayExit));
        sut.SetLocale("fr-FR");
        Assert.Equal("Afficher", sut.Get(LocaleKeys.TrayShow));
        Assert.Equal("Quit", sut.Get(LocaleKeys.TrayExit));
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
            var sut = CreateService(new ApplicationDataOptions { Locale = locale });

            foreach (var key in LocaleKeys.All)
            {
                Assert.NotEqual(key, sut.Get(key));
                Assert.False(string.IsNullOrWhiteSpace(sut.Get(key)));
            }
        }
    }

    [Fact]
    public void Constructor_WhenCultureFileDoesNotExist_ThrowsClearFileNotFoundException()
    {
        var options = new ApplicationDataOptions();
        options.CulturePaths.Add(
            Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid():N}", "FlourishCulture.Json")
        );

        var exception = Assert.Throws<FileNotFoundException>(() => CreateService(options));

        Assert.Contains("does not exist", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Constructor_WhenCultureFilePathIsBlank_ThrowsClearArgumentException()
    {
        var options = new ApplicationDataOptions();
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
        var options = new ApplicationDataOptions();
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
        var path = directory.WriteText(LocalizationService.CultureFileName, json);
        var options = new ApplicationDataOptions();
        options.CulturePaths.Add(path);

        var exception = Assert.Throws<InvalidDataException>(() => CreateService(options));

        Assert.Contains("Culture", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void SetLocale_ChangesSelectedLocaleAndRaisesChanged()
    {
        var sut = CreateService();
        LocalizationChangedEventArgs? change = null;
        sut.Changed += (_, args) => change = args;

        sut.SetLocale(" zh_CN ");

        Assert.Equal("zh-CN", sut.Current.Locale);
        Assert.NotNull(change);
        Assert.Equal(LocalizationChangeKind.LocaleChanged, change.Kind);
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
            LocalizationService.CultureFileName,
            """
            {
              "Tray.Show": { "fr-FR": "Afficher", "de-DE": "Anzeigen" }
            }
            """
        );
        var sut = CreateService();
        var changes = new List<LocalizationChangeKind>();
        sut.Changed += (_, args) => changes.Add(args.Kind);

        var registration = sut.RegisterFile(path);
        Assert.Equal(["de-DE", "fr-FR"], registration.Locales);
        sut.SetLocale("fr-FR");
        Assert.Equal("Afficher", sut.Get(LocaleKeys.TrayShow));

        directory.WriteText(
            LocalizationService.CultureFileName,
            """
            {
              "Tray.Show": { "fr-FR": "Ouvrir" }
            }
            """
        );
        registration.Reload();
        Assert.Equal(["fr-FR"], registration.Locales);
        Assert.DoesNotContain("de-DE", sut.Current.AvailableLocales);
        Assert.Equal("Ouvrir", sut.Get(LocaleKeys.TrayShow));

        registration.Dispose();
        Assert.False(registration.IsRegistered);
        registration.Dispose();
        Assert.DoesNotContain("fr-FR", sut.Current.AvailableLocales);
        Assert.Equal("Show", sut.Get(LocaleKeys.TrayShow));
        Assert.Equal(
            [
                LocalizationChangeKind.FileRegistered,
                LocalizationChangeKind.LocaleChanged,
                LocalizationChangeKind.FileReloaded,
                LocalizationChangeKind.FileUnregistered,
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
            LocalizationService.CultureFileName,
            "{ \"Tray.Show\": { \"fr-FR\": \"First\" } }"
        );
        var second = secondDirectory.WriteText(
            LocalizationService.CultureFileName,
            "{ \"Tray.Show\": { \"fr-FR\": \"Second\" } }"
        );
        var sut = CreateService();
        sut.RegisterFile(first);
        var overrideRegistration = sut.RegisterFile(second);
        sut.SetLocale("fr-FR");
        Assert.Equal("Second", sut.Get(LocaleKeys.TrayShow));

        overrideRegistration.Dispose();

        Assert.Equal("First", sut.Get(LocaleKeys.TrayShow));
    }

    private static LocalizationService CreateService(ApplicationDataOptions? options = null) =>
        new(
            options ?? new ApplicationDataOptions(),
            Path.Combine(Path.GetTempPath(), $"Flourish.Test-{Guid.NewGuid():N}")
        );
}
