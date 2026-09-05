using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.WPF.Test.Infrastructure;
using Xunit;
using GalleryKey = ArkheideSystem.Essential.Culture.Key;

namespace ArkheideSystem.Flourish.WPF.Test.Gallery;

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
            Assert.All(
                translations.Values,
                value => Assert.False(string.IsNullOrWhiteSpace(value))
            );
            Assert.DoesNotContain('\uFFFD', translations["en-US"]);
            Assert.DoesNotContain('\uFFFD', translations["zh-CN"]);
            Assert.Equal(
                GetPlaceholderIndexes(translations["en-US"]),
                GetPlaceholderIndexes(translations["zh-CN"])
            );
        }
    }

    [Fact]
    public void CultureDocument_KeepsInterfaceCopyConcise()
    {
        var document = ReadCultureDocument(GetCulturePath());

        Assert.True(
            document.Values.Average(translations => translations["en-US"].Length) <= 38,
            "English Gallery copy is too verbose."
        );
        Assert.True(
            document.Values.Average(translations => translations["zh-CN"].Length) <= 15,
            "Chinese Gallery copy is too verbose."
        );

        foreach (var (key, translations) in document)
        {
            Assert.True(translations["en-US"].Length <= 130, $"English copy is too long: {key}");
            Assert.True(translations["zh-CN"].Length <= 105, $"Chinese copy is too long: {key}");
            Assert.DoesNotContain(
                "the code below",
                translations["en-US"],
                StringComparison.OrdinalIgnoreCase
            );
            Assert.DoesNotContain(
                "covers every",
                translations["en-US"],
                StringComparison.OrdinalIgnoreCase
            );
            Assert.DoesNotContain("下方代码", translations["zh-CN"], StringComparison.Ordinal);
            Assert.DoesNotContain("涵盖所有", translations["zh-CN"], StringComparison.Ordinal);
        }
    }

    [Fact]
    public void GalleryXaml_UsesOnlyRegisteredTypedLocalizeKeys()
    {
        var document = ReadCultureDocument(GetCulturePath());
        var viewDirectory = Path.Combine(FindRepositoryRoot(), "src", "Gallery.WPF", "Views");
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
            path =>
                Assert.Contains("xmlns:culture=", File.ReadAllText(path), StringComparison.Ordinal)
        );
        Assert.DoesNotContain("LangKey.", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void GalleryProject_UsesEssentialCultureWithoutCrossRepositorySourceCoupling()
    {
        var repository = FindRepositoryRoot();
        var project = File.ReadAllText(
            Path.Combine(repository, "src", "Gallery.WPF", "Gallery.WPF.csproj")
        );
        var program = File.ReadAllText(
            Path.Combine(repository, "src", "Gallery.WPF", "Program.cs")
        );
        var flourishProject = File.ReadAllText(
            Path.Combine(repository, "src", "Flourish.WPF", "Flourish.WPF.csproj")
        );
        var coreProject = File.ReadAllText(
            Path.Combine(repository, "src", "Flourish.Core", "Flourish.Core.csproj")
        );
        var solution = File.ReadAllText(Path.Combine(repository, "Flourish.slnx"));

        Assert.DoesNotContain("Extension.Culture", project, StringComparison.Ordinal);
        Assert.DoesNotContain("../Extension", solution, StringComparison.Ordinal);
        Assert.Contains(
            "PackageReference Include=\"Arkheide.Essential.Culture.Wpf\" Version=\"1.2.0\"",
            project,
            StringComparison.Ordinal
        );
        Assert.Contains("<IsPackable>false</IsPackable>", project, StringComparison.Ordinal);
        Assert.DoesNotContain("UseLocalEssentialCulture", project, StringComparison.Ordinal);
        Assert.DoesNotContain(
            "Essential.Culture.Generator.csproj",
            project,
            StringComparison.Ordinal
        );
        Assert.Contains(
            "<AdditionalFiles Include=\"Localization\\Culture.json\" />",
            project,
            StringComparison.Ordinal
        );
        Assert.DoesNotContain("ArkheideSystem.LangKey", project, StringComparison.Ordinal);
        Assert.DoesNotContain("UseEssentialCulture", program, StringComparison.Ordinal);
        Assert.DoesNotContain("ArkheideSystem.Extension", program, StringComparison.Ordinal);
        Assert.Contains(
            "Localizer.Current.SetCulture(\"en-US\")",
            program,
            StringComparison.Ordinal
        );
        Assert.DoesNotContain("AddGalleryLocalization", program, StringComparison.Ordinal);
        Assert.DoesNotContain("appsettings.json", project, StringComparison.OrdinalIgnoreCase);
        Assert.False(
            File.Exists(Path.Combine(repository, "src", "Gallery.WPF", "appsettings.json"))
        );
        Assert.False(
            File.Exists(Path.Combine(repository, "src", "Gallery.WPF", "GalleryCommandKeys.cs"))
        );
        Assert.Contains("GalleryCommandParser.DemoHello", program, StringComparison.Ordinal);
        Assert.Contains("GalleryCommandParser.DemoBackground", program, StringComparison.Ordinal);
        Assert.DoesNotContain(
            "ArkheideSystem.Essential.Culture",
            flourishProject,
            StringComparison.Ordinal
        );
        Assert.DoesNotContain("FlourishCulture.Json", flourishProject, StringComparison.Ordinal);
        Assert.Contains("FlourishCulture.Json", coreProject, StringComparison.Ordinal);
    }

    [Fact]
    public void RepositoryBuild_DoesNotCarryAnExtensionSpecificAssemblyWorkaround()
    {
        var repository = FindRepositoryRoot();
        var buildTargets = XDocument.Load(Path.Combine(repository, "Directory.Build.targets"));
        Assert.DoesNotContain(
            buildTargets.Descendants(),
            element =>
                element.Name.LocalName == "Target"
                && (string?)element.Attribute("Name") == "RemoveTransitiveLegacyFlourishReference"
        );
    }

    [Fact]
    public void RepositoryPack_RejectsArchitectureSpecificLibraryAssets()
    {
        var repository = FindRepositoryRoot();
        var buildTargets = XDocument.Load(Path.Combine(repository, "Directory.Build.targets"));
        var target = Assert.Single(
            buildTargets.Descendants(),
            element =>
                element.Name.LocalName == "Target"
                && (string?)element.Attribute("Name") == "RequireArchitectureNeutralLibraryPackage"
        );

        Assert.Equal("GenerateNuspec", (string?)target.Attribute("BeforeTargets"));
        string condition = (string?)target.Attribute("Condition") ?? string.Empty;
        Assert.Contains("'$(IsPackable)' == 'true'", condition, StringComparison.Ordinal);
        Assert.Contains("'$(IncludeBuildOutput)' != 'false'", condition, StringComparison.Ordinal);
        Assert.Contains("'$(Platform)' != 'AnyCPU'", condition, StringComparison.Ordinal);
        Assert.Single(target.Descendants(), element => element.Name.LocalName == "Error");
    }

    [Fact]
    public void PublicIdentities_UseConciseCultureKeyAndFinalFlourishAssemblyNames()
    {
        var repository = FindRepositoryRoot();
        var gallerySources = Directory
            .GetFiles(
                Path.Combine(repository, "src", "Gallery.WPF"),
                "*.cs",
                SearchOption.AllDirectories
            )
            .Where(path =>
                !path.Contains(
                    $"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}",
                    StringComparison.OrdinalIgnoreCase
                )
                && !path.Contains(
                    $"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            .Select(File.ReadAllText)
            .ToArray();
        var flourishProject = File.ReadAllText(
            Path.Combine(repository, "src", "Flourish.WPF", "Flourish.WPF.csproj")
        );

        Assert.Contains(
            gallerySources,
            source =>
                source.Contains(
                    "using CKey = ArkheideSystem.Essential.Culture.Key;",
                    StringComparison.Ordinal
                )
        );
        Assert.DoesNotContain(
            gallerySources,
            source => source.Contains("global using", StringComparison.Ordinal)
        );
        Assert.DoesNotContain(
            gallerySources,
            source => source.Contains("CultureKeyToken", StringComparison.Ordinal)
        );
        Assert.Contains(
            "<AssemblyName>Flourish.WPF</AssemblyName>",
            flourishProject,
            StringComparison.Ordinal
        );
        Assert.Contains(
            "<PackageId>Arkheide.Flourish.WPF</PackageId>",
            flourishProject,
            StringComparison.Ordinal
        );
        Assert.Equal("Flourish.WPF", typeof(IApplicationRuntime).Assembly.GetName().Name);
        Assert.StartsWith("ArkheideSystem.Flourish", typeof(IApplicationRuntime).Namespace);
    }

    [Fact]
    public void Repository_DoesNotContainUnpublishedCompatibilityArtifacts()
    {
        var repository = FindRepositoryRoot();
        var solution = File.ReadAllText(Path.Combine(repository, "Flourish.slnx"));
        var assemblyInfo = File.ReadAllText(
            Path.Combine(repository, "src", "Flourish.WPF", "AssemblyInfo.cs")
        );
        var wpfSources = Directory
            .GetFiles(
                Path.Combine(repository, "src", "Flourish.WPF"),
                "*.*",
                SearchOption.AllDirectories
            )
            .Where(path =>
                Path.GetExtension(path) is ".cs" or ".xaml"
                && !path.Contains(
                    $"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}",
                    StringComparison.OrdinalIgnoreCase
                )
                && !path.Contains(
                    $"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            .Select(File.ReadAllText)
            .ToArray();
        var packageIds = Directory
            .GetFiles(Path.Combine(repository, "src"), "*.csproj", SearchOption.AllDirectories)
            .Select(XDocument.Load)
            .Where(project =>
                project.Descendants().Any(element => element.Name.LocalName == "PackageId")
                && !project
                    .Descendants()
                    .Any(element =>
                        element.Name.LocalName == "IsPackable" && element.Value == "false"
                    )
            )
            .Select(project =>
                project.Descendants().Single(element => element.Name.LocalName == "PackageId").Value
            )
            .OrderBy(packageId => packageId, StringComparer.Ordinal)
            .ToArray();

        Assert.False(
            Directory.Exists(Path.Combine(repository, "src", "Flourish.WPF.Compatibility"))
        );
        Assert.DoesNotContain("Flourish.WPF.Compatibility", solution, StringComparison.Ordinal);
        Assert.DoesNotContain("TypeForwardedTo", assemblyInfo, StringComparison.Ordinal);
        Assert.Empty(typeof(ArkheideSystem.Flourish.Controls.Button).Assembly.GetForwardedTypes());
        Assert.DoesNotContain(
            wpfSources,
            source =>
                source.Contains("FlourishFontSizeIcon", StringComparison.Ordinal)
                || source.Contains("FlourishLineHeightIcon", StringComparison.Ordinal)
                || source.Contains("FlourishTypographyBottomSpaceIcon", StringComparison.Ordinal)
                || source.Contains("FlourishParagraphMargin", StringComparison.Ordinal)
        );
        Assert.Equal(new[] { "Arkheide.Flourish.Core", "Arkheide.Flourish.WPF" }, packageIds);
    }

    [Fact]
    public void MemberRows_DelegateDynamicLocalizationToEssentialCulture()
    {
        var repository = FindRepositoryRoot();
        var galleryRoot = Path.Combine(repository, "src", "Gallery.WPF");
        var rowSource = File.ReadAllText(Path.Combine(galleryRoot, "MemberRow.cs"));
        var viewDirectory = Path.Combine(galleryRoot, "Views");
        var xaml = string.Join(
            Environment.NewLine,
            Directory.GetFiles(viewDirectory, "*.xaml").Select(File.ReadAllText)
        );
        var code = string.Join(
            Environment.NewLine,
            Directory.GetFiles(viewDirectory, "*.xaml.cs").Select(File.ReadAllText)
        );

        Assert.False(File.Exists(Path.Combine(galleryRoot, "ControlMemberRow.cs")));
        Assert.Contains(
            "record MemberRow(string Name, string DescriptionKey)",
            rowSource,
            StringComparison.Ordinal
        );
        Assert.DoesNotContain("Localizer", rowSource, StringComparison.Ordinal);
        Assert.DoesNotContain("INotifyPropertyChanged", rowSource, StringComparison.Ordinal);
        Assert.DoesNotContain("ControlMemberRow", code, StringComparison.Ordinal);
        Assert.Contains(
            "{culture:Localize KeyBinding={Binding DescriptionKey}}",
            xaml,
            StringComparison.Ordinal
        );
        Assert.DoesNotContain("Binding=\"{Binding Description}\"", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void FlourishAssets_UseOneKeyFirstCultureCatalog()
    {
        var repository = FindRepositoryRoot();
        var assetDirectory = Path.Combine(repository, "src", "Flourish.Core", "Assets");
        var files = Directory
            .GetFiles(assetDirectory, "*Culture.Json")
            .Select(path => Path.GetFileName(path)!)
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(["FlourishCulture.Json"], files);

        using var document = JsonDocument.Parse(
            File.ReadAllText(Path.Combine(assetDirectory, "FlourishCulture.Json"))
        );
        Assert.All(
            document.RootElement.EnumerateObject(),
            key =>
            {
                Assert.Equal(JsonValueKind.Object, key.Value.ValueKind);
                Assert.True(key.Value.TryGetProperty("en-US", out var english), key.Name);
                Assert.True(key.Value.TryGetProperty("zh-CN", out var chinese), key.Name);
                Assert.True(english.GetString()!.Length <= 60, key.Name);
                Assert.True(chinese.GetString()!.Length <= 32, key.Name);
            }
        );

        var galleryCatalogPath = Path.Combine(
            repository,
            "src",
            "Gallery.WPF",
            "FlourishCulture.Json"
        );
        using var galleryDocument = JsonDocument.Parse(File.ReadAllText(galleryCatalogPath));
        Assert.True(
            galleryDocument.RootElement.EnumerateObject().Count()
                < document.RootElement.EnumerateObject().Count()
        );
        Assert.All(
            galleryDocument.RootElement.EnumerateObject(),
            key =>
            {
                Assert.Equal(
                    ["es-ES"],
                    key.Value.EnumerateObject().Select(locale => locale.Name).ToArray()
                );
                Assert.True(key.Value.GetProperty("es-ES").GetString()!.Length <= 70, key.Name);
            }
        );
        Assert.Empty(
            Directory.GetFiles(
                assetDirectory,
                "Flourish.LangKey_*.Json",
                SearchOption.TopDirectoryOnly
            )
        );
        Assert.Empty(
            Directory.GetFiles(
                Path.Combine(repository, "src", "Gallery.WPF"),
                "Flourish.LangKey_*.Json",
                SearchOption.TopDirectoryOnly
            )
        );
    }

    private static string GetCulturePath() =>
        Path.Combine(FindRepositoryRoot(), "src", "Gallery.WPF", "Localization", "Culture.json");

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
