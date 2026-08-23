using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using Arkheide.Essential.Culture;
using GalleryKey = Arkheide.Essential.Culture.Key;

namespace ArkheideSystem.Flourish.Test.Gallery;

[Collection("Gallery localization")]
public sealed class GalleryLocalizationTests
{
    [Fact]
    public void CultureDocument_MatchesGeneratedKeysAndTranslationContract()
    {
        var document = ReadCultureDocument(GetCulturePath());
        var generatedTokens = typeof(GalleryKey)
            .GetProperties(BindingFlags.Public | BindingFlags.Static)
            .Where(property => property.PropertyType == typeof(string))
            .Select(property => Assert.IsType<string>(property.GetValue(null)))
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray();
        var expectedTokens = document
            .Keys.Select(key => $"Key.{key}")
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray();

        Assert.True(
            document.Count >= 1400,
            $"Expected broad Gallery coverage; found {document.Count} keys."
        );
        Assert.Equal(expectedTokens, generatedTokens);

        foreach (var (key, translations) in document)
        {
            Assert.Matches("^[A-Za-z_][A-Za-z0-9_]*$", key);
            Assert.Equal(
                ["en-US", "zh-CN"],
                translations.Keys.OrderBy(value => value, StringComparer.Ordinal).ToArray()
            );
            Assert.All(translations.Values, value => Assert.False(string.IsNullOrWhiteSpace(value)));
            Assert.DoesNotContain('\uFFFD', translations["en-US"]);
            Assert.DoesNotContain('\uFFFD', translations["zh-CN"]);
            Assert.Equal(
                GetPlaceholderIndexes(translations["en-US"]),
                GetPlaceholderIndexes(translations["zh-CN"])
            );
        }
    }

    [Fact]
    public void GalleryXaml_UsesOnlyRegisteredTypedLocalizeKeys()
    {
        var document = ReadCultureDocument(GetCulturePath());
        var viewDirectory = Path.Combine(FindRepositoryRoot(), "src", "Gallery", "Views");
        var files = Directory.GetFiles(viewDirectory, "*.xaml");
        var xaml = string.Join(Environment.NewLine, files.Select(File.ReadAllText));
        var keys = Regex
            .Matches(xaml, @"\{culture:Localize\s+Key=(?<key>[A-Za-z_][A-Za-z0-9_]*)")
            .Select(match => match.Groups["key"].Value)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        Assert.NotEmpty(keys);
        Assert.All(keys, key => Assert.True(document.ContainsKey(key), key));
        Assert.All(
            files,
            path => Assert.Contains("xmlns:culture=", File.ReadAllText(path), StringComparison.Ordinal)
        );
        Assert.DoesNotContain("LangKey.", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void GalleryProject_UsesEssentialCultureWithoutChangingFlourishResources()
    {
        var repository = FindRepositoryRoot();
        var project = File.ReadAllText(Path.Combine(repository, "src", "Gallery", "Gallery.csproj"));
        var program = File.ReadAllText(Path.Combine(repository, "src", "Gallery", "Program.cs"));
        var flourishProject = File.ReadAllText(
            Path.Combine(repository, "src", "Flourish", "Arkheide.Flourish.csproj")
        );

        Assert.Contains("Arkheide.Flourish.Extension.Culture.csproj", project, StringComparison.Ordinal);
        Assert.DoesNotContain("PackageReference Include=\"Arkheide.Essential.Culture", project, StringComparison.Ordinal);
        Assert.Contains(
            "<AdditionalFiles Include=\"Localization\\Culture.json\" />",
            project,
            StringComparison.Ordinal
        );
        Assert.DoesNotContain("ArkheideSystem.LangKey", project, StringComparison.Ordinal);
        Assert.Contains(".UseEssentialCulture()", program, StringComparison.Ordinal);
        Assert.DoesNotContain("AddGalleryLocalization", program, StringComparison.Ordinal);
        Assert.DoesNotContain("Arkheide.Essential.Culture", flourishProject, StringComparison.Ordinal);
        Assert.Contains("Flourish.LangKey_en-US.Json", flourishProject, StringComparison.Ordinal);
    }

    [Fact]
    public void PublicIdentities_UseConciseCultureKeyAndArkheideFlourishAssemblyNames()
    {
        var repository = FindRepositoryRoot();
        var globalUsings = File.ReadAllText(
            Path.Combine(repository, "src", "Gallery", "GlobalUsings.cs")
        );
        var flourishProject = File.ReadAllText(
            Path.Combine(repository, "src", "Flourish", "Arkheide.Flourish.csproj")
        );

        Assert.Contains(
            "global using Key = Arkheide.Essential.Culture.Key;",
            globalUsings,
            StringComparison.Ordinal
        );
        Assert.DoesNotContain("CultureKeyToken", globalUsings, StringComparison.Ordinal);
        Assert.Contains(
            "<AssemblyName>Arkheide.Flourish</AssemblyName>",
            flourishProject,
            StringComparison.Ordinal
        );
        Assert.Contains(
            "<PackageId>Arkheide.Flourish</PackageId>",
            flourishProject,
            StringComparison.Ordinal
        );
        Assert.Equal("Arkheide.Flourish", typeof(IFlourish).Assembly.GetName().Name);
        Assert.StartsWith("ArkheideSystem.Flourish", typeof(IFlourish).Namespace);
    }

    [Fact]
    public void FlourishAssets_RemainOwnedByFlourish()
    {
        var assetDirectory = Path.Combine(FindRepositoryRoot(), "src", "Flourish", "Assets");
        var files = Directory
            .GetFiles(assetDirectory, "Flourish.LangKey_*.Json")
            .Select(path => Path.GetFileName(path)!)
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(["Flourish.LangKey_en-US.Json", "Flourish.LangKey_zh-CN.Json"], files);
    }

    private static string GetCulturePath() =>
        Path.Combine(FindRepositoryRoot(), "src", "Gallery", "Localization", "Culture.json");

    private static Dictionary<string, Dictionary<string, string>> ReadCultureDocument(string path)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return document
            .RootElement.EnumerateObject()
            .ToDictionary(
                entry => entry.Name,
                entry =>
                    entry
                        .Value.EnumerateObject()
                        .ToDictionary(
                            translation => translation.Name,
                            translation => Assert.IsType<string>(translation.Value.GetString()),
                            StringComparer.Ordinal
                        ),
                StringComparer.Ordinal
            );
    }

    private static string[] GetPlaceholderIndexes(string value) =>
        Regex
            .Matches(value, @"(?<!\{)\{(?<index>[0-9]+)(?:[^}]*)\}(?!\})")
            .Select(match => match.Groups["index"].Value)
            .OrderBy(index => index, StringComparer.Ordinal)
            .ToArray();

    private static string FindRepositoryRoot() => TestPaths.RepositoryRoot;
}

[CollectionDefinition("Gallery localization", DisableParallelization = true)]
public sealed class GalleryLocalizationCollection;
