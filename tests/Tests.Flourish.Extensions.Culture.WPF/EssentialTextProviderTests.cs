using System.Globalization;
using ArkheideSystem.Essential.Culture;
using ArkheideSystem.Flourish.Extensions.Culture.WPF;
using ArkheideSystem.Flourish.WPF;
using ArkheideSystem.Flourish.WPF.Abstract;
using Xunit;

namespace ArkheideSystem.Tests.Flourish.Extensions.Culture.WPF;

public sealed class EssentialTextProviderTests
{
    private static LocalizationContext Context(string culture = "en-US", string? formatCulture = null) =>
        new(LocalizationCatalog.FromJson("""
            {"Title":{"en-US":"Application","zh-CN":"应用"},
             "Total":{"en-US":"Total: {0:N2}","zh-CN":"合计：{0:N2}"}}
            """, "en-US"), culture, formatCulture);

    [Fact]
    public void Builder_UsesSingleNeutralProviderWithoutCoreOrShellMutation()
    {
        var builder = new FrameworkBuilder();
        builder.UseEssentialCulture(Context(), out var provider, "Gallery");
        using (provider)
        {
            Assert.Same(provider, builder.TextProvider);
            Assert.Equal("Application", provider.Get(new("Gallery", "Title", "Fallback")));
        }
        Assert.DoesNotContain(typeof(EssentialTextProvider).Assembly.GetReferencedAssemblies(),
            name => name.Name is "Flourish.Core" or "Flourish.Blazor.Framework");
    }

    [Fact]
    public void Lookup_HonorsCatalogFallbackAndExplicitFormatCulture()
    {
        using var provider = new EssentialTextProvider(Context(formatCulture: "pt-BR"), "Gallery");
        Assert.Equal("Other catalog", provider.Get(new("Other", "Title", "Other catalog")));
        Assert.Equal("Unknown", provider.Get(new("Gallery", "Unknown")));
        Assert.Equal("Total: 1.234,50", provider.Get(new("Gallery", "Total"), 1234.5));
        Assert.Equal("Fallback: 1.234,50", provider.Get(new("Gallery", "Missing", "Fallback: {0:N2}"), 1234.5));
        Assert.Equal(CultureInfo.GetCultureInfo("pt-BR"), provider.FormatCulture);
    }

    [Fact]
    public void ContextChanges_AreIsolatedAndDisposeRemovesLifetimeSubscription()
    {
        var first = Context();
        var second = Context();
        var provider = new EssentialTextProvider(first);
        using var other = new EssentialTextProvider(second);
        var changes = 0;
        provider.Changed += (_, _) => changes++;
        first.SetCulture("zh-CN");
        Assert.Equal(1, changes);
        Assert.Equal("应用", provider.Get(new("Application", "Title")));
        Assert.Equal("en-US", other.Culture);
        provider.Dispose();
        provider.Dispose();
        first.SetCulture("en-US");
        Assert.Equal(1, changes);
        Assert.Throws<ObjectDisposedException>(() => provider.Get(new("Application", "Title")));
    }

    [Fact]
    public void DesktopFacade_RefreshesAndDetachesOnDispose()
    {
        var previous = Localizer.Current.Culture;
        try
        {
            Localizer.Current.SetCulture("en-US");
            var provider = new EssentialTextProvider("Tests", "pt-BR");
            var changes = 0;
            provider.Changed += (_, _) => changes++;
            Assert.Equal("Application", provider.Get(new("Tests", "ApplicationTitle")));
            Localizer.Current.SetCulture("zh-CN");
            Assert.Equal(1, changes);
            Assert.Equal("应用", provider.Get(new("Tests", "ApplicationTitle")));
            Assert.Equal("pt-BR", provider.FormatCulture.Name);
            provider.Dispose();
            Localizer.Current.SetCulture("en-US");
            Assert.Equal(1, changes);
        }
        finally { Localizer.Current.SetCulture(previous); }
    }

    [Fact]
    public void LibraryCatalog_UsesTheCurrentThreeLanguagesAndEnglishFallback()
    {
        var context = new LocalizationContext(LocalizationCatalog.FromJson("""
            {"Title":{"en-US":"Application","zh-CN":"应用","pt-BR":"Aplicativo","fr-FR":"Application"}}
            """, "en-US"), "en-US");
        using var provider = new EssentialTextProvider(context, "Gallery");
        foreach (var (culture, expected) in new[] { ("en-US", "All columns"), ("zh-CN", "所有列"), ("pt-BR", "Todas as colunas"), ("fr-FR", "All columns") })
        {
            context.SetCulture(culture);
            Assert.Equal(expected, provider.Get(new("Flourish", "Table_AllColumns", "Fallback")));
        }
        Assert.Equal("Missing fallback", provider.Get(new("Flourish", "Unknown", "Missing fallback")));
    }

    [Fact]
    public void LibraryAndConsumerCatalogs_CannotResolveEachOthersCollidingTokens()
    {
        var context = new LocalizationContext(LocalizationCatalog.FromJson("""
            {"Table_AllColumns":{"en-US":"Consumer columns","zh-CN":"业务字段"},
             "ConsumerOnly":{"en-US":"Consumer value","zh-CN":"业务值"}}
            """, "en-US"), "zh-CN", "pt-BR");
        using var provider = new EssentialTextProvider(context, "Gallery");
        Assert.Equal("业务字段", provider.Get(new("Gallery", "Table_AllColumns")));
        Assert.Equal("所有列", provider.Get(new("Flourish", "Table_AllColumns")));
        Assert.Equal("Library fallback", provider.Get(new("Flourish", "ConsumerOnly", "Library fallback")));
        Assert.Equal("Other fallback", provider.Get(new("Other", "ConsumerOnly", "Other fallback")));
        Assert.Equal("1.234,50", provider.Get(new("Flourish", "Missing", "{0:N2}"), 1234.5));
    }
}
