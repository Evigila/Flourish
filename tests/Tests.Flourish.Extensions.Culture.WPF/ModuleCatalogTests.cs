using System.IO;
using ArkheideSystem.Essential.Culture;
using ArkheideSystem.Flourish.Extensions.Culture.WPF;
using ArkheideSystem.Flourish.WPF;
using ArkheideSystem.Tests.Flourish.Extensions.Culture.WPF.Texts;
using Xunit;

namespace ArkheideSystem.Tests.Flourish.Extensions.Culture.WPF;

public sealed class ModuleCatalogTests
{
    private static LocalizationCatalog Catalog(CatalogLoadOptions? options = null) =>
        LocalizationCatalog.FromFiles(
            CultureResources.Files.Select(path => Path.Combine(AppContext.BaseDirectory, path)),
            CultureResources.FallbackCulture, options);

    [Fact]
    public void GeneratedManifest_ComposesCommonAndFeatureKeysThroughTheNeutralProvider()
    {
        Assert.Equal("en-US", CultureResources.FallbackCulture);
        Assert.Equal(["Culture.json", "Records/Culture.Records.json"], CultureResources.Files);
        Assert.All(CultureResources.Files, path => Assert.True(File.Exists(Path.Combine(AppContext.BaseDirectory, path))));
        var builder = new FrameworkBuilder();
        Assert.Same(builder, builder.UseEssentialCulture(Catalog(), out var provider,
            culture: "pt-BR", formatCulture: "pt-BR", catalogId: "Gallery"));
        using (provider)
        {
            Assert.Same(provider, builder.TextProvider);
            Assert.Equal("Application", provider.Get(new("Gallery", Key.ApplicationTitle)));
            Assert.Equal("Registros", provider.Get(new("Gallery", Key.RecordTitle)));
            Assert.Equal("Total: 1.234,50", provider.Get(new("Gallery", Key.RecordTotal), 1234.5));
            Assert.Equal("Other fallback", provider.Get(new("Other", Key.RecordTitle, "Other fallback")));
            provider.SetCulture("zh-CN");
            Assert.Equal("应用", provider.Get(new("Gallery", Key.ApplicationTitle)));
            Assert.Equal("Records", provider.Get(new("Gallery", Key.RecordTitle)));
        }
    }

    [Fact]
    public void StartupPolicy_RejectsDisabledSelectionsAndRetainsIndependentFormatting()
    {
        var catalog = Catalog(new CatalogLoadOptions(
            enabledCultures: ["EN-us", "zh-CN", "pt-BR", "fr-CA"], disabledCultures: ["fr"]));
        Assert.Equal(["en-US", "fr", "pt-BR", "zh-CN"], catalog.DeclaredCultures);
        using var provider = new EssentialTextProvider(catalog, "fr-CA", "de-DE", "Gallery");
        Assert.Equal(["en-US", "fr-CA", "pt-BR", "zh-CN"], provider.AvailableCultures);
        // The disabled French parent cannot supply a translation; the module keeps its fallback.
        Assert.Equal("Records", provider.Get(new("Gallery", Key.RecordTitle)));
        Assert.Equal("Total: 1.234,50", provider.Get(new("Gallery", Key.RecordTotal), 1234.5));
        Assert.Equal("de-DE", provider.FormatCulture.Name);
        var changes = 0;
        provider.Changed += (_, _) => changes++;
        Assert.Throws<ArgumentException>(() => provider.SetCulture("FR"));
        Assert.Equal("fr-CA", provider.Culture);
        Assert.Equal("de-DE", provider.FormatCulture.Name);
        Assert.Equal(0, changes);
        Assert.Throws<ArgumentException>(() => new EssentialTextProvider(catalog, "fr", catalogId: "Gallery"));
        provider.SetCulture("PT-br");
        Assert.Equal(1, changes);
        Assert.Equal("pt-BR", provider.Culture);
        Assert.Equal("pt-BR", provider.FormatCulture.Name);
        Assert.Equal("Registros", provider.Get(new("Gallery", Key.RecordTitle)));
        provider.SetCulture("pt-BR");
        Assert.Equal(1, changes);
    }

    [Fact]
    public void SharedModules_IsolateSelectionAndDisposalAcrossProviders()
    {
        var catalog = Catalog(new CatalogLoadOptions(enabledCultures: ["en-US", "pt-BR"]));
        var first = new EssentialTextProvider(catalog, catalogId: "Gallery");
        using var second = new EssentialTextProvider(catalog, catalogId: "Gallery");
        var changes = 0;
        first.Changed += (_, _) => changes++;
        first.SetCulture("pt-BR");
        Assert.Equal("Registros", first.Get(new("Gallery", Key.RecordTitle)));
        Assert.Equal("Records", second.Get(new("Gallery", Key.RecordTitle)));
        Assert.Equal("en-US", second.Culture);
        first.Dispose();
        first.Dispose();
        second.SetCulture("pt-BR");
        Assert.Equal(1, changes);
        Assert.Throws<ObjectDisposedException>(() => first.SetCulture("en-US"));
        Assert.Throws<ObjectDisposedException>(() => first.Get(new("Gallery", Key.RecordTitle)));
    }

    [Fact]
    public void LibraryLookup_UsesEssentialParentFallbackAndStableTokens()
    {
        using var provider = new EssentialTextProvider(Catalog(), culture: "pt-BR-x-gallery", catalogId: "Gallery");
        Assert.Equal("Todas as colunas", provider.Get(new("Flourish", "Key.Table_AllColumns")));
        Assert.Equal("Registros", provider.Get(new("Gallery", Key.RecordTitle)));
        provider.SetCulture("zh-CN-x-gallery");
        Assert.Equal("所有列", provider.Get(new("Flourish", "Key.Table_AllColumns")));
        Assert.Equal("应用", provider.Get(new("Gallery", Key.ApplicationTitle)));
        provider.SetCulture("fr-CA");
        Assert.Equal("All columns", provider.Get(new("Flourish", "Key.Table_AllColumns")));
        Assert.Equal("Enregistrements", provider.Get(new("Gallery", Key.RecordTitle)));
    }

    [Fact]
    public void DesktopFacade_ConsumesStartupModulesAndRejectsLateConfiguration()
    {
        var previous = Localizer.Current.Culture;
        try
        {
            var builder = new FrameworkBuilder();
            builder.UseEssentialCulture(out var provider, "Gallery", "pt-BR");
            using (provider)
            {
                Assert.Equal(["en-US", "pt-BR", "zh-CN"], provider.AvailableCultures);
                provider.SetCulture("pt-BR");
                Assert.Equal("Registros", provider.Get(new("Gallery", Key.RecordTitle)));
                Assert.Equal("Total: 1.234,50", provider.Get(new("Gallery", Key.RecordTotal), 1234.5));
                Assert.Throws<ArgumentException>(() => provider.SetCulture("fr"));
                Assert.Equal("pt-BR", provider.Culture);
                Assert.Throws<InvalidOperationException>(() => Localizer.Configure(Catalog()));
            }
        }
        finally { Localizer.Current.SetCulture(previous); }
    }
}
