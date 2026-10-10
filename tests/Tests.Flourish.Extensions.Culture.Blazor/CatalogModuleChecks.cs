using System.Text;
using ArkheideSystem.Essential.Culture;
using ArkheideSystem.Flourish.Extensions.Culture.Blazor;
using Microsoft.Extensions.DependencyInjection;
using static ArkheideSystem.Tests.Flourish.Extensions.Culture.Blazor.Program;

namespace ArkheideSystem.Tests.Flourish.Extensions.Culture.Blazor;

internal static class CatalogModuleChecks
{
    private const string Common = """
        { "Shared": { "en-US": "Common", "zh-CN": "公共", "pt-BR": "Comum" } }
        """;
    private const string Feature = """
        { "Feature": { "en-US": "Feature: {0:N2}", "zh-CN": "功能：{0:N2}" } }
        """;

    internal static void Register(List<(string Name, Func<Task> Run)> tests)
    {
        tests.Add(("Module catalogs compose deployment paths, optional languages and eager loading", ModuleComposition));
        tests.Add(("Module validation preserves Essential diagnostics and rejects incomplete startup", InvalidModules));
        tests.Add(("Parsed catalogs preserve retained-language policy and host selection defaults", LanguagePolicy));
        tests.Add(("All catalog entries share identity, default selection and frozen configuration", CatalogConfiguration));
        tests.Add(("Catalog stream entry retains caller ownership and current-position semantics", StreamOwnership));
    }

    private static async Task ModuleComposition()
    {
        using var files = new ModuleFiles();
        var common = files.Write("Culture.Common.json", Common);
        var feature = files.Write("Culture.Feature.json", Feature);
        var relativeCommon = Path.GetRelativePath(AppContext.BaseDirectory, common);
        await using var root = Build(Services(culture => culture.AddCatalogFiles("Modules", [relativeCommon, feature]), addApplication: false));
        // Loading is complete before registration returns; rendering and selection do not revisit files.
        File.Delete(common);
        File.Delete(feature);
        await using var scope = root.CreateAsyncScope();
        var session = scope.ServiceProvider.GetRequiredService<CultureSession>();
        Equal("Common", session.Parse("Shared"));
        Equal("Feature: 1,234.50", session.Parse("Feature", 1234.5m));
        Require(session.AvailableCultures.SequenceEqual(new[] { "en-US", "pt-BR", "zh-CN" }), "Module culture union was not used for default choices.");
        Require(await session.SelectAsync("pt-BR"), "Optional module culture was not selectable.");
        Equal("Comum", session.Parse("Shared"));
        Equal("Feature: 1.234,50", session.Parse("Feature", 1234.5m));
        Require(await session.SelectAsync("zh-CN", "en-US"), "Independent configured UI/format choices failed.");
        Equal("功能：1,234.50", session.Parse("Feature", 1234.5m));
    }

    private static Task InvalidModules()
    {
        using var files = new ModuleFiles();
        var common = files.Write("Culture.Common.json", Common);
        var duplicate = files.Write("Culture.Duplicate.json", Common);
        var malformed = files.Write("Culture.Malformed.json", "{");
        var missingFallback = files.Write("Culture.NoFallback.json", """{ "Feature": { "zh-CN": "功能" } }""");
        var missing = Path.Combine(Path.GetDirectoryName(common)!, "Culture.Missing.json");
        var duplicateError = Throws<InvalidDataException>(() => Services(culture => culture.AddCatalogFiles("Modules", [common, duplicate])));
        Require(duplicateError.Message.Contains(common, StringComparison.Ordinal) && duplicateError.Message.Contains(duplicate, StringComparison.Ordinal)
            && duplicateError.Message.Contains("Shared", StringComparison.Ordinal), "Duplicate module diagnostics lost the key or source files.");
        Require(Throws<InvalidDataException>(() => Services(culture => culture.AddCatalogFiles("Modules", [malformed])))
            .Message.Contains(malformed, StringComparison.Ordinal), "Malformed module diagnostics lost its path.");
        Require(Throws<InvalidDataException>(() => Services(culture => culture.AddCatalogFiles("Modules", [missingFallback])))
            .Message.Contains("en-US", StringComparison.Ordinal), "Missing fallback diagnostics lost its culture.");
        Equal(missing, Throws<FileNotFoundException>(() => Services(culture => culture.AddCatalogFiles("Modules", [common, missing]))).FileName);
        Throws<ArgumentException>(() => Services(culture => culture.AddCatalogFiles("Modules", [common, common])));
        Throws<ArgumentException>(() => Services(culture => culture.AddCatalogFiles("Modules", [])));
        Throws<ArgumentException>(() => Services(culture => culture.AddCatalogFiles("Modules", [" "])));
        Throws<ArgumentNullException>(() => Services(culture => culture.AddCatalogFiles("Modules", null!)));
        Throws<ArgumentException>(() => Services(culture => culture.AddCatalogFiles("Modules", [common], "en-US",
            new CatalogLoadOptions(disabledCultures: ["en-US"]))));
        return Task.CompletedTask;
    }

    private static async Task LanguagePolicy()
    {
        using var files = new ModuleFiles();
        var common = files.Write("Culture.Common.json", Common);
        var feature = files.Write("Culture.Feature.json", Feature);
        var policy = new CatalogLoadOptions(enabledCultures: ["zh-CN", "en-US"], disabledCultures: ["pt-BR"]);
        var parsed = LocalizationCatalog.FromFiles([common, feature], "en-US", policy);
        await using var root = Build(Services(culture => culture.AddCatalog("Modules", parsed).SetDefaultCatalog("Modules")));
        await using var scope = root.CreateAsyncScope();
        var session = scope.ServiceProvider.GetRequiredService<CultureSession>();
        Require(session.AvailableCultures.SequenceEqual(new[] { "en-US", "zh-CN" }), "Parsed catalog policy did not determine implicit host choices.");
        Require(parsed.DeclaredCultures.Contains("pt-BR") && !parsed.IsCultureEnabled("pt-BR"), "Authored and selectable languages were conflated.");
        Equal("en-US", session.Culture);
        Throws<ArgumentException>(() => session.SelectAsync("pt-BR"));
        Equal(0, scope.ServiceProvider.GetRequiredService<JsProbe>().Imports);
        Require(await session.SelectAsync("zh-CN", "en-US"), "Policy-compatible independent format selection failed.");
        Equal("功能：1,234.50", session.Parse("Feature", 1234.5m));
        var conflicting = Throws<InvalidOperationException>(() => Services(culture => culture.AddCatalog("Restricted", parsed)));
        Require(conflicting.Message.Contains("Restricted", StringComparison.Ordinal) && conflicting.Message.Contains("pt-BR", StringComparison.Ordinal),
            "Host policy conflict omitted the catalog or UI culture.");
        Throws<InvalidOperationException>(() => Services(culture => culture.AddCatalog("Modules", parsed).SetDefaultCatalog("Modules").SetDefaultCulture("pt-BR")));
        Throws<InvalidOperationException>(() => Services(culture => culture.AddCatalog("Modules", parsed).SetDefaultCatalog("Modules").SetDefaultCulture("en-US", "pt-BR")));

        await using var fileRoot = Build(Services(culture => culture.AddCatalogFiles("Modules", [common, feature], "en-US",
            new CatalogLoadOptions(enabledCultures: ["zh-CN"]))
            .SetDefaultCulture("zh-CN"), addApplication: false));
        await using var fileScope = fileRoot.CreateAsyncScope();
        var fileSession = fileScope.ServiceProvider.GetRequiredService<CultureSession>();
        Require(fileSession.AvailableCultures.SequenceEqual(new[] { "zh-CN" }), "File load policy was ignored.");
        Equal("公共", fileSession.Parse("Shared"));
        Equal("zh-CN", fileSession.FormatCulture.Name);

        var invalidExcluded = files.Write("Culture.InvalidExcluded.json", """{ "Invalid": { "en-US": "Valid", "pt-BR": null } }""");
        Throws<InvalidDataException>(() => Services(culture => culture.AddCatalogFiles("Modules", [invalidExcluded], "en-US",
            new CatalogLoadOptions(disabledCultures: ["pt-BR"]))));
    }

    private static async Task CatalogConfiguration()
    {
        var parsed = LocalizationCatalog.FromJson(Common);
        CultureBuilder? captured = null;
        await using var root = Build(Services(culture =>
        {
            captured = culture;
            culture.AddCatalog("Parsed", parsed);
        }, addApplication: false));
        await using var scope = root.CreateAsyncScope();
        Equal("Common", scope.ServiceProvider.GetRequiredService<CultureSession>().Parse("Shared"));
        Throws<InvalidOperationException>(() => captured!.AddCatalog("Late", parsed));
        Throws<InvalidOperationException>(() => captured!.AddCatalogFiles("Late", ["Absent.json"]));
        Throws<InvalidOperationException>(() => Services(culture => culture.AddCatalog("App", parsed)));
        Throws<InvalidOperationException>(() => Services(culture => culture.AddCatalogFiles("App", ["Absent.json"])));
        Throws<ArgumentException>(() => Services(culture => culture.AddCatalog(" ", parsed)));
        Throws<ArgumentNullException>(() => Services(culture => culture.AddCatalog("Parsed", (LocalizationCatalog)null!)));
        await using var libraryDefault = Build(Services(addApplication: false));
        await using var libraryScope = libraryDefault.CreateAsyncScope();
        Equal("Getting started", libraryScope.ServiceProvider.GetRequiredService<CultureSession>().Parse("Tutorial_Label"));
    }

    private static async Task StreamOwnership()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("ignored\n" + Common));
        stream.Position = Encoding.UTF8.GetByteCount("ignored\n");
        await using var root = Build(Services(culture => culture.AddCatalog("Stream", stream), addApplication: false));
        Require(stream.CanRead && stream.Position == stream.Length, "Successful load disposed or rewound the caller's stream.");
        await using var scope = root.CreateAsyncScope();
        Equal("Common", scope.ServiceProvider.GetRequiredService<CultureSession>().Parse("Shared"));
        using var malformed = new MemoryStream(Encoding.UTF8.GetBytes("{"));
        Throws<InvalidDataException>(() => Services(culture => culture.AddCatalog("Malformed", malformed)));
        Require(malformed.CanRead, "Failed validation disposed the caller's stream.");
    }

    private sealed class ModuleFiles : IDisposable
    {
        private readonly string directory = Path.Combine(AppContext.BaseDirectory, "culture-modules-" + Guid.NewGuid().ToString("N"));
        private readonly List<string> paths = [];

        internal ModuleFiles() => Directory.CreateDirectory(directory);

        internal string Write(string name, string json)
        {
            var path = Path.Combine(directory, name);
            File.WriteAllText(path, json, new UTF8Encoding(false));
            paths.Add(path);
            return path;
        }

        public void Dispose()
        {
            foreach (var path in paths) File.Delete(path);
            Directory.Delete(directory);
        }
    }
}
