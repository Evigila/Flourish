using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Controls;
using FlourishButton = ArkheideSystem.Flourish.Controls.Button;

namespace ArkheideSystem.Flourish.Test.Controls;

public sealed class FlourishXamlArchitectureTests
{
    private const string PresentationNamespace =
        "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
    private const string XamlNamespace =
        "http://schemas.microsoft.com/winfx/2006/xaml";
    private static readonly string RepositoryRoot = FindRepositoryRoot();
    private static readonly string FlourishRoot = Path.Combine(
        RepositoryRoot,
        "src",
        "Flourish"
    );
    private static readonly string GalleryRoot = Path.Combine(
        RepositoryRoot,
        "src",
        "Gallery"
    );
    private static readonly string[] CanonicalFontSizeResourceNames =
    [
        "FlourishFontSizeSmall",
        "FlourishFontSizeStandard",
        "FlourishFontSizeIcon",
        "FlourishFontSizeLarge",
        "FlourishFontSizeExtraLarge",
        "FlourishFontSizeHeaderSize",
    ];

    private static readonly string[] CanonicalTextFontSizeResourceNames =
    [
        "FlourishFontSizeSmall",
        "FlourishFontSizeStandard",
        "FlourishFontSizeLarge",
        "FlourishFontSizeExtraLarge",
        "FlourishFontSizeHeaderSize",
    ];
    private static readonly IReadOnlyDictionary<string, double> ContextualIconFontSizes =
        new Dictionary<string, double>(StringComparer.Ordinal)
        {
            ["FlourishIconFontSizeNavigation"] = 18d,
            ["FlourishIconFontSizeTitlebar"] = 16d,
            ["FlourishIconFontSizeWindowCaption"] = 12d,
            ["FlourishIconFontSizeTitlebarSearch"] = 14d,
            ["FlourishIconFontSizeStatusBar"] = 14d,
            ["FlourishIconFontSizeStatusBarBackgroundTask"] = 12d,
            ["FlourishIconFontSizeBackgroundTaskView"] = 16d,
            ["FlourishIconFontSizeSystemStatusView"] = 16d,
        };
    private static readonly Regex FontSizeResourceNamePattern = new(
        @"\bFlourishFontSize[A-Za-z0-9_]*\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant
    );
    private static readonly Regex LiteralFontSizeAssignmentPattern = new(
        @"\bFontSize\s*=\s*[-+]?(?:\d+(?:\.\d*)?|\.\d+)(?:[dDfFmM])?\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant
    );
    private static readonly Regex RetiredFontApiPattern = new(
        @"\b(?:FlourishFontGap|FontGap|SetFontFamily|SetFontSize|SetFontGap)\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant
    );

    [Fact]
    public void ProjectFolders_FollowThePublicInternalThemeAndViewBoundaries()
    {
        string[] requiredDirectories =
        [
            "Abstract",
            "Assets",
            "Controls",
            "Internal",
            "Internal/Composition",
            "Internal/Configuration",
            "Internal/Imaging",
            "Services",
            "Themes",
            "Themes/Colors",
            "Views",
            "Views/Page",
            "Views/Windows",
        ];
        string[] retiredDirectories =
        [
            "Composition",
            "Configuration",
            "Styles",
            "Windows",
            "Controls/Styles",
        ];

        Assert.All(
            requiredDirectories,
            path =>
                Assert.True(
                    Directory.Exists(Path.Combine(FlourishRoot, NormalizePlatformPath(path))),
                    $"Required directory src/Flourish/{path} is missing."
                )
        );
        Assert.All(
            retiredDirectories,
            path =>
                Assert.False(
                    Directory.Exists(Path.Combine(FlourishRoot, NormalizePlatformPath(path))),
                    $"Retired directory src/Flourish/{path} must not exist."
                )
        );
    }

    [Fact]
    public void InternalServiceAndViewNamespaces_DoNotExposeImplementationTypes()
    {
        var exportedImplementations = typeof(FlourishButton)
            .Assembly.GetExportedTypes()
            .Where(type =>
                type.Namespace?.StartsWith(
                    "ArkheideSystem.Flourish.Internal",
                    StringComparison.Ordinal
                ) == true
                || type.Namespace?.StartsWith(
                    "ArkheideSystem.Flourish.Services",
                    StringComparison.Ordinal
                ) == true
                || type.Namespace?.StartsWith(
                    "ArkheideSystem.Flourish.Views",
                    StringComparison.Ordinal
                ) == true
            )
            .Select(type => type.FullName)
            .ToArray();

        Assert.Empty(exportedImplementations);
    }

    [Fact]
    public void GenericTheme_IsTheSingleCompositionRoot()
    {
        var generic = LoadXaml(Path.Combine(FlourishRoot, "Themes", "Generic.xaml"));
        string[] expectedSources =
        [
            "/Flourish;component/Themes/Layout.xaml",
            "/Flourish;component/Themes/Typography.xaml",
            "/Flourish;component/Themes/Colors/Colors.xaml",
            "/Flourish;component/Themes/Controls.xaml",
        ];

        Assert.Equal(expectedSources, GetMergedDictionarySources(generic));

        var rootThemeFiles = Directory
            .EnumerateFiles(Path.Combine(FlourishRoot, "Themes"), "*.xaml")
            .Select(Path.GetFileName)
            .Order(StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(
            new[] { "Controls.xaml", "Generic.xaml", "Layout.xaml", "Typography.xaml" },
            rootThemeFiles
        );

        var colorFiles = Directory
            .EnumerateFiles(Path.Combine(FlourishRoot, "Themes", "Colors"), "*.xaml")
            .Select(Path.GetFileName)
            .Order(StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(
            new[] { "Colors.Dark.xaml", "Colors.Light.xaml", "Colors.xaml" },
            colorFiles
        );
    }

    [Fact]
    public void ControlsTheme_ComposesEveryControlDictionaryExactlyOnce()
    {
        var controlsRoot = Path.Combine(FlourishRoot, "Controls");
        var theme = LoadXaml(Path.Combine(FlourishRoot, "Themes", "Controls.xaml"));
        var actualSources = GetMergedDictionarySources(theme);
        var familyDependencyFiles = new HashSet<string>(StringComparer.Ordinal)
        {
            "Button.xaml",
            "Card.xaml",
            "IconCard.xaml",
            "IconButton.xaml",
            "ListCard.xaml",
            "WindowCaptionButton.xaml",
        };
        var expectedSources = Directory
            .EnumerateFiles(controlsRoot, "*.xaml", SearchOption.TopDirectoryOnly)
            .Where(path => !familyDependencyFiles.Contains(Path.GetFileName(path)))
            .Select(path =>
                $"/Flourish;component/Controls/{Path.GetFileName(path)}"
            )
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(expectedSources, actualSources.Order(StringComparer.Ordinal));
        Assert.Equal(actualSources.Length, actualSources.Distinct(StringComparer.Ordinal).Count());
        Assert.All(
            actualSources,
            source => Assert.StartsWith("/Flourish;component/Controls/", source)
        );

        Assert.Equal(
            ["IconCard.xaml"],
            GetMergedDictionarySources(
                LoadXaml(Path.Combine(controlsRoot, "OutputCard.xaml"))
            )
        );
        Assert.Equal(
            ["ListCard.xaml"],
            GetMergedDictionarySources(
                LoadXaml(Path.Combine(controlsRoot, "IconCard.xaml"))
            )
        );
        Assert.Equal(
            ["Card.xaml"],
            GetMergedDictionarySources(
                LoadXaml(Path.Combine(controlsRoot, "ListCard.xaml"))
            )
        );
        Assert.Equal(
            ["WindowCaptionButton.xaml"],
            GetMergedDictionarySources(
                LoadXaml(Path.Combine(controlsRoot, "CardButton.xaml"))
            )
        );
        Assert.Equal(
            ["IconButton.xaml"],
            GetMergedDictionarySources(
                LoadXaml(Path.Combine(controlsRoot, "WindowCaptionButton.xaml"))
            )
        );
        Assert.Equal(
            ["Button.xaml"],
            GetMergedDictionarySources(
                LoadXaml(Path.Combine(controlsRoot, "IconButton.xaml"))
            )
        );
        Assert.Empty(
            GetMergedDictionarySources(
                LoadXaml(Path.Combine(controlsRoot, "Button.xaml"))
            )
        );
    }

    [Fact]
    public void EveryPublicVisualControl_HasAMatchingDictionaryCodePairAndProjectNesting()
    {
        var controlsRoot = Path.Combine(FlourishRoot, "Controls");
        var project = LoadXaml(Path.Combine(FlourishRoot, "Flourish.csproj"));
        var dependentUpon = project
            .Descendants()
            .Where(element => element.Name.LocalName == "Compile")
            .Select(element => new
            {
                Path = NormalizePath(
                    element.Attribute("Update")?.Value
                        ?? element.Attribute("Include")?.Value
                        ?? string.Empty
                ),
                Parent = element
                    .Elements()
                    .FirstOrDefault(child => child.Name.LocalName == "DependentUpon")
                    ?.Value,
            })
            .ToDictionary(item => item.Path, item => item.Parent, StringComparer.OrdinalIgnoreCase);
        var violations = new List<string>();

        foreach (var type in GetPublicFlourishControlTypes())
        {
            var fileName = GetControlFileName(type);
            var xamlPath = Path.Combine(controlsRoot, $"{fileName}.xaml");
            var codePath = Path.Combine(controlsRoot, $"{fileName}.xaml.cs");
            var projectPath = $"Controls/{fileName}.xaml.cs";

            if (!File.Exists(xamlPath))
            {
                violations.Add($"{RelativePath(xamlPath)} is missing");
            }
            else
            {
                var dictionary = LoadXaml(xamlPath);
                if (dictionary.Root?.Name.LocalName != "ResourceDictionary")
                {
                    violations.Add($"{RelativePath(xamlPath)} is not a ResourceDictionary");
                }

                var implicitStyle = dictionary
                    .Descendants()
                    .Where(element => element.Name.LocalName == "Style")
                    .Where(element => element.Attribute(XName.Get("Key", XamlNamespace)) is null)
                    .SingleOrDefault(element =>
                        ((string?)element.Attribute("TargetType"))?.Contains(
                            $"controls:{type.Name}",
                            StringComparison.Ordinal
                        ) == true
                    );
                if (implicitStyle is null)
                {
                    violations.Add(
                        $"{RelativePath(xamlPath)} has no implicit Style for {type.Name}"
                    );
                }
            }

            if (!File.Exists(codePath))
            {
                violations.Add($"{RelativePath(codePath)} is missing");
            }
            else if (!File.ReadAllText(codePath).Contains($"class {type.Name}", StringComparison.Ordinal))
            {
                violations.Add($"{RelativePath(codePath)} does not declare {type.Name}");
            }

            if (!dependentUpon.TryGetValue(projectPath, out var parent))
            {
                violations.Add($"{projectPath} has no Compile metadata");
            }
            else if (!string.Equals(parent, $"{fileName}.xaml", StringComparison.Ordinal))
            {
                violations.Add(
                    $"{projectPath} must depend on {fileName}.xaml, but depends on {parent ?? "<missing>"}"
                );
            }
        }

        AssertNoArchitectureViolations(
            violations,
            "Every public Flourish visual control must own one same-named XAML/XAML.cs pair."
        );
    }

    [Fact]
    public void ControlsRoot_HasNoOrphanedDictionaryOrCodeBehindFiles()
    {
        var controlsRoot = Path.Combine(FlourishRoot, "Controls");
        var violations = new List<string>();

        foreach (
            var xamlPath in Directory.EnumerateFiles(
                controlsRoot,
                "*.xaml",
                SearchOption.TopDirectoryOnly
            )
        )
        {
            var codePath = xamlPath + ".cs";
            if (!File.Exists(codePath))
            {
                violations.Add($"{RelativePath(xamlPath)} has no matching XAML.cs file");
            }
        }

        foreach (
            var codePath in Directory.EnumerateFiles(
                controlsRoot,
                "*.xaml.cs",
                SearchOption.TopDirectoryOnly
            )
        )
        {
            var xamlPath = codePath[..^3];
            if (!File.Exists(xamlPath))
            {
                violations.Add($"{RelativePath(codePath)} has no matching XAML file");
            }
        }

        AssertNoArchitectureViolations(
            violations,
            "Controls must remain independent XAML/XAML.cs pairs."
        );
    }

    [Fact]
    public void NativeWpfControls_DoNotReceiveImplicitFlourishStyles()
    {
        var violations = new List<string>();

        foreach (var file in EnumerateXamlFiles(FlourishRoot))
        {
            var document = LoadXaml(file);
            foreach (
                var style in document
                    .Descendants()
                    .Where(element => element.Name.LocalName == "Style")
                    .Where(element => element.Attribute(XName.Get("Key", XamlNamespace)) is null)
            )
            {
                var targetType = (string?)style.Attribute("TargetType") ?? string.Empty;
                if (!targetType.Contains("controls:", StringComparison.Ordinal))
                {
                    violations.Add(
                        $"{FormatViolation(file, style)} implicitly styles {targetType}"
                    );
                }
            }
        }

        AssertNoArchitectureViolations(
            violations,
            "Native WPF controls must retain their native theme; Generic-reachable dictionaries may not publish unkeyed native Styles."
        );
    }

    [Fact]
    public void StyleAndTemplateDeclarations_AreConfinedToControlDictionaries()
    {
        var controlsRoot = Path.Combine(FlourishRoot, "Controls");
        var violations = new List<string>();

        foreach (var file in EnumerateXamlFiles(FlourishRoot))
        {
            if (IsUnderDirectory(file, controlsRoot))
            {
                continue;
            }

            var document = LoadXaml(file);
            foreach (
                var declaration in document
                    .Descendants()
                    .Where(element =>
                        element.Name.LocalName is "Style" or "ControlTemplate"
                    )
            )
            {
                violations.Add(FormatViolation(file, declaration));
            }
        }

        AssertNoArchitectureViolations(
            violations,
            "Style and ControlTemplate declarations may only live in Controls/*.xaml."
        );
    }

    [Fact]
    public void GalleryXaml_DoesNotDeclareOrSelectStylesAndTemplates()
    {
        var galleryRoot = Path.Combine(RepositoryRoot, "src", "Gallery");
        var violations = new List<string>();

        foreach (var file in EnumerateXamlFiles(galleryRoot))
        {
            var document = LoadXaml(file);
            foreach (
                var element in document
                    .Descendants()
                    .Where(element =>
                        element.Name.LocalName is "Style" or "ControlTemplate"
                    )
            )
            {
                violations.Add(FormatViolation(file, element));
            }

            foreach (
                var styleAttribute in document
                    .Root!
                    .DescendantsAndSelf()
                    .SelectMany(element => element.Attributes())
                    .Where(attribute => attribute.Name.LocalName == "Style")
            )
            {
                violations.Add(FormatViolation(file, styleAttribute));
            }
        }

        AssertNoArchitectureViolations(
            violations,
            "Gallery pages must demonstrate reusable Flourish controls without local styles."
        );
    }

    [Fact]
    public void EveryGalleryPage_UsesOneLeadingHeroFollowedOnlyByChunks()
    {
        var viewsRoot = Path.Combine(RepositoryRoot, "src", "Gallery", "Views");
        var violations = new List<string>();

        foreach (var path in Directory.EnumerateFiles(viewsRoot, "*.xaml", SearchOption.AllDirectories))
        {
            var document = LoadXaml(path);
            if (document.Root?.Name.LocalName != "Page")
            {
                continue;
            }

            var heroes = document
                .Descendants()
                .Where(element => element.Name.LocalName == nameof(ChunkHero))
                .Where(element => !IsInsidePopup(element))
                .ToArray();

            if (heroes.Length != 1)
            {
                violations.Add(
                    $"{RelativePath(path)} declares {heroes.Length} main-content ChunkHero elements"
                );
                continue;
            }

            var hero = heroes[0];
            var flow = hero.Parent;
            if (flow is null)
            {
                violations.Add($"{FormatViolation(path, hero)} has no main content container");
                continue;
            }

            var visibleSections = flow
                .Elements()
                .Where(element => !IsPropertyElement(element))
                .Where(element => element.Name.LocalName != "Popup")
                .ToArray();
            if (!ReferenceEquals(visibleSections.FirstOrDefault(), hero))
            {
                violations.Add($"{FormatViolation(path, hero)} is not the leading main section");
            }

            foreach (var element in visibleSections.Skip(1))
            {
                if (element.Name.LocalName != nameof(Chunk))
                {
                    violations.Add(
                        $"{FormatViolation(path, element)} follows ChunkHero outside a Chunk"
                    );
                }
            }
        }

        AssertNoArchitectureViolations(
            violations,
            "Every Gallery page must have exactly one leading ChunkHero and place each subsequent visible main section in a full-width Chunk; Popup infrastructure is excluded."
        );
    }

    [Fact]
    public void ProfilePage_IsAnInternalShellFlyoutRatherThanAMainContentPage()
    {
        var path = Path.Combine(FlourishRoot, "Views", "Page", "ProfilePage.xaml");
        var document = LoadXaml(path);
        var root = Assert.IsType<XElement>(document.Root);
        var xamlNamespace = XNamespace.Get(
            "http://schemas.microsoft.com/winfx/2006/xaml"
        );

        Assert.Equal("Page", root.Name.LocalName);
        Assert.Equal("internal", (string?)root.Attribute(xamlNamespace + "ClassModifier"));
        Assert.DoesNotContain(
            root.Descendants(),
            element => element.Name.LocalName is nameof(ChunkHero) or nameof(Chunk)
        );
    }

    [Fact]
    public void GalleryChunks_DeclareRequiredTitleAndBody()
    {
        var viewsRoot = Path.Combine(RepositoryRoot, "src", "Gallery", "Views");
        var violations = new List<string>();

        foreach (var path in Directory.EnumerateFiles(viewsRoot, "*.xaml", SearchOption.AllDirectories))
        {
            var document = LoadXaml(path);
            if (document.Root?.Name.LocalName != "Page")
            {
                continue;
            }

            foreach (
                var chunk in document
                    .Descendants()
                    .Where(element => element.Name.LocalName == nameof(Chunk))
                    .Where(element => !IsInsidePopup(element))
            )
            {
                var title =
                    (string?)chunk.Attribute(nameof(Chunk.Title))
                    ?? chunk
                        .Elements()
                        .FirstOrDefault(element =>
                            element.Name.LocalName == $"{nameof(Chunk)}.{nameof(Chunk.Title)}"
                        )
                        ?.Value;
                if (string.IsNullOrWhiteSpace(title))
                {
                    violations.Add($"{FormatViolation(path, chunk)} has no required Title");
                }

                if (!HasChunkBody(chunk))
                {
                    violations.Add($"{FormatViolation(path, chunk)} has no required Body");
                }
            }
        }

        AssertNoArchitectureViolations(
            violations,
            "Every Gallery Chunk must declare a non-empty Title and a Body."
        );
    }

    [Fact]
    public void GalleryPresenters_DeclareTheCompleteCompositionContract()
    {
        var viewsRoot = Path.Combine(RepositoryRoot, "src", "Gallery", "Views");
        var violations = new List<string>();
        string[] requiredProperties =
        [
            nameof(Presenter.Title),
            nameof(Presenter.Description),
            nameof(Presenter.PresenterMode),
            nameof(Presenter.PresenterPosition),
        ];

        foreach (var path in Directory.EnumerateFiles(viewsRoot, "*.xaml", SearchOption.AllDirectories))
        {
            var document = LoadXaml(path);
            foreach (
                var presenter in document
                    .Descendants()
                    .Where(element =>
                        element.Name.LocalName is nameof(Presenter) or nameof(ChunkHero)
                    )
            )
            {
                foreach (var property in requiredProperties)
                {
                    if (string.IsNullOrWhiteSpace((string?)presenter.Attribute(property)))
                    {
                        violations.Add(
                            $"{FormatViolation(path, presenter)} has no explicit {property}"
                        );
                    }
                }

                if (presenter.Name.LocalName != nameof(Presenter))
                {
                    continue;
                }

                if (
                    !presenter.Elements().Any(element =>
                        element.Name.LocalName
                            == $"{nameof(Presenter)}.{nameof(Presenter.Presentation)}"
                    )
                )
                {
                    violations.Add(
                        $"{FormatViolation(path, presenter)} has no explicit Presentation"
                    );
                }

                foreach (var directContent in presenter.Elements().Where(element =>
                    !IsPropertyElement(element)
                ))
                {
                    violations.Add(
                        $"{FormatViolation(path, directContent)} is implicit Presenter content"
                    );
                }
            }
        }

        AssertNoArchitectureViolations(
            violations,
            "Gallery Presenter and ChunkHero declarations must explicitly set Title, Description, PresenterMode, and PresenterPosition; ordinary Presenter visuals must use Presenter.Presentation."
        );
    }

    [Fact]
    public void PresenterTemplate_FixesCopyAndPresentationIntoTwoEqualColumns()
    {
        var document = LoadXaml(
            Path.Combine(FlourishRoot, "Controls", "Presenter.xaml")
        );
        var template = document
            .Descendants()
            .Single(element =>
                element.Name.LocalName == "ControlTemplate"
                && (string?)element.Attribute(XNamespace.Get(XamlNamespace) + "Key")
                    == "PresenterTemplate"
            );
        var layoutGrid = template
            .Descendants()
            .First(element =>
                element.Name.LocalName == "Grid"
                && element.Elements().Any(child =>
                    child.Name.LocalName == "Grid.ColumnDefinitions"
                )
            );
        var columns = layoutGrid
            .Elements()
            .Single(element => element.Name.LocalName == "Grid.ColumnDefinitions")
            .Elements()
            .ToArray();

        Assert.Equal(2, columns.Length);
        Assert.All(columns, column => Assert.Equal("*", (string?)column.Attribute("Width")));

        var presentationSurface = layoutGrid
            .Elements()
            .Single(element =>
                (string?)element.Attribute(XNamespace.Get(XamlNamespace) + "Name")
                    == "PresentationSurface"
            );
        var presentationHost = presentationSurface
            .Descendants()
            .Single(element =>
                (string?)element.Attribute(XNamespace.Get(XamlNamespace) + "Name")
                    == "PresentationHost"
            );
        var copySurface = layoutGrid
            .Elements()
            .Single(element =>
                (string?)element.Attribute(XNamespace.Get(XamlNamespace) + "Name")
                    == "CopySurface"
            );

        var presenterSurface = template
            .Descendants()
            .Single(element =>
                (string?)element.Attribute(XNamespace.Get(XamlNamespace) + "Name")
                    == "PresenterSurface"
            );
        Assert.Equal("True", (string?)presenterSurface.Attribute("ClipToBounds"));
        Assert.Equal(
            "{DynamicResource FlourishSurfaceCornerRadius}",
            (string?)presenterSurface.Attribute("CornerRadius")
        );
        Assert.Null((string?)presenterSurface.Attribute("Background"));

        Assert.Equal("1", (string?)presentationSurface.Attribute("Grid.Column"));
        Assert.Equal(
            "{TemplateBinding Background}",
            (string?)presentationSurface.Attribute("Background")
        );
        Assert.Equal("True", (string?)presentationSurface.Attribute("ClipToBounds"));
        Assert.Equal(
            "{DynamicResource FlourishSurfaceCornerRadius}",
            (string?)presentationSurface.Attribute("CornerRadius")
        );
        Assert.Equal("0", (string?)copySurface.Attribute("Grid.Column"));
        Assert.Null((string?)copySurface.Attribute("Background"));
        Assert.Equal("Center", (string?)presentationHost.Attribute("HorizontalAlignment"));
        Assert.Equal("Center", (string?)presentationHost.Attribute("VerticalAlignment"));

        var bodyHost = copySurface
            .Descendants()
            .Single(element =>
                (string?)element.Attribute(XNamespace.Get(XamlNamespace) + "Name")
                    == "BodyHost"
            );
        Assert.Equal("Left", (string?)bodyHost.Attribute("HorizontalAlignment"));
        Assert.Equal("Center", (string?)bodyHost.Attribute("VerticalAlignment"));

        foreach (var hostName in new[] { "TitleHost", "DescriptionHost" })
        {
            var textHost = copySurface
                .Descendants()
                .Single(element =>
                    (string?)element.Attribute(XNamespace.Get(XamlNamespace) + "Name")
                        == hostName
                );
            Assert.Equal("Left", (string?)textHost.Attribute("TextAlignment"));
        }
    }

    [Fact]
    public void ControlsGalleryPages_FollowTheCanonicalLearningSequence()
    {
        string[] pages =
        [
            "ControlLibraryPage.xaml",
            "ChunkPage.xaml",
            "ButtonPage.xaml",
            "CardPage.xaml",
            "ParagraphPage.xaml",
            "CodeSpacePage.xaml",
            "DataGridPage.xaml",
            "OverlayPage.xaml",
        ];

        foreach (var fileName in pages)
        {
            var document = LoadXaml(Path.Combine(GalleryRoot, "Views", fileName));
            var chunks = document
                .Descendants()
                .Where(element => element.Name.LocalName == nameof(Chunk))
                .ToArray();
            var actualTitles = chunks
                .Select(element =>
                    (string?)element.Attribute(nameof(Chunk.Title)) ?? string.Empty
                )
                .ToArray();

            var variantIndex = Array.IndexOf(actualTitles, "Variant");
            var tableIndex = Array.IndexOf(actualTitles, "Table");
            var usageIndex = Array.IndexOf(actualTitles, "Usage");
            var referenceIndex = Array.IndexOf(actualTitles, "Reference");

            Assert.True(variantIndex is -1 or 0, $"{fileName}: Variant must be first when present.");
            Assert.Equal(variantIndex + 1, tableIndex);
            Assert.True(
                usageIndex > tableIndex + 1,
                $"{fileName}: Table and Usage must be separated by topic-specific examples."
            );
            Assert.Equal(actualTitles.Length - 2, usageIndex);
            Assert.Equal(actualTitles.Length - 1, referenceIndex);
            Assert.Equal(actualTitles.Length, actualTitles.Distinct(StringComparer.Ordinal).Count());

            var table = chunks[tableIndex];
            var dataGrid = Assert.Single(
                table.Descendants(),
                element => element.Name.LocalName == "DataGrid"
            );
            Assert.Equal("False", (string?)dataGrid.Attribute("AutoGenerateColumns"));
            Assert.Equal("True", (string?)dataGrid.Attribute("IsReadOnly"));
            Assert.Equal(
                2,
                dataGrid.Descendants().Count(element =>
                    element.Name.LocalName == "DataGridTextColumn"
                )
            );
            Assert.DoesNotContain(
                table.Descendants(),
                element => element.Name.LocalName == "Border"
            );

            var reference = chunks[^1];
            var referenceButtons = reference
                .Descendants()
                .Where(element => element.Name.LocalName == nameof(CardButton))
                .ToArray();
            Assert.Equal(2, referenceButtons.Length);
            Assert.All(referenceButtons, button =>
            {
                Assert.Equal("False", (string?)button.Attribute("IsEnabled"));
                Assert.False(string.IsNullOrWhiteSpace((string?)button.Attribute("ToolTip")));
            });
        }
    }

    [Fact]
    public void GalleryChunks_DoNotShareAHorizontalRow()
    {
        var viewsRoot = Path.Combine(RepositoryRoot, "src", "Gallery", "Views");
        var violations = new List<string>();

        foreach (
            var path in Directory.EnumerateFiles(
                viewsRoot,
                "*.xaml",
                SearchOption.AllDirectories
            )
        )
        {
            var document = LoadXaml(path);
            if (document.Root?.Name.LocalName != "Page")
            {
                continue;
            }

            foreach (var parent in document.Descendants())
            {
                var chunks = parent
                    .Elements()
                    .Where(element => element.Name.LocalName == nameof(Chunk))
                    .ToArray();
                if (chunks.Length < 2)
                {
                    continue;
                }

                var isVerticalStack =
                    parent.Name.LocalName == "StackPanel"
                    && (string?)parent.Attribute("Orientation") is null or "Vertical";
                if (!isVerticalStack)
                {
                    violations.Add(
                        $"{RelativePath(path)} places {chunks.Length} Chunk elements in a {parent.Name.LocalName}"
                    );
                }
            }
        }

        AssertNoArchitectureViolations(
            violations,
            "Gallery chunks must span the available row; arrange related cards inside one chunk instead."
        );
    }

    [Fact]
    public void GalleryListCardColumns_StayPureAndUseTheFixedLayoutContract()
    {
        var viewsRoot = Path.Combine(RepositoryRoot, "src", "Gallery", "Views");
        var violations = new List<string>();

        foreach (
            var path in Directory.EnumerateFiles(
                viewsRoot,
                "*.xaml",
                SearchOption.AllDirectories
            )
        )
        {
            var document = LoadXaml(path);
            if (document.Root?.Name.LocalName != "Page")
            {
                continue;
            }

            foreach (
                var listCard in document.Descendants().Where(element =>
                    element.Name.LocalName == nameof(ListCard)
                )
            )
            {
                string[] fixedProperties =
                [
                    nameof(Card.Variant),
                    nameof(Card.ContentHorizontalAlignment),
                    nameof(Card.ContentVerticalAlignment),
                ];
                foreach (var property in fixedProperties)
                {
                    if (listCard.Attribute(property) is not null)
                    {
                        violations.Add(
                            $"{FormatViolation(path, listCard)} sets fixed property {property}"
                        );
                    }
                }
            }

            foreach (
                var parent in document.Descendants().Where(element =>
                    element.Name.LocalName == "StackPanel"
                )
            )
            {
                var directCards = parent
                    .Elements()
                    .Where(element =>
                        element.Name.LocalName
                            is nameof(Card)
                                or nameof(IconCard)
                                or nameof(ListCard)
                                or nameof(OutputCard)
                    )
                    .ToArray();
                if (
                    directCards.Any(element => element.Name.LocalName == nameof(ListCard))
                    && directCards.Any(element => element.Name.LocalName != nameof(ListCard))
                )
                {
                    violations.Add(
                        $"{FormatViolation(path, parent)} mixes ListCard with another card type"
                    );
                }
            }
        }

        AssertNoArchitectureViolations(
            violations,
            "ListCard columns must stay pure and must use their fixed Standard left-copy-right contract."
        );
    }

    [Fact]
    public void GalleryListCardActionBodies_ContainAtMostOneInteractiveControl()
    {
        var viewsRoot = Path.Combine(RepositoryRoot, "src", "Gallery", "Views");
        var interactiveControlNames = new HashSet<string>(StringComparer.Ordinal)
        {
            "Button",
            "IconButton",
            "FlourishTextBox",
            "FlourishPasswordBox",
            "FlourishSearchBox",
            "FlourishCheckBox",
            "FlourishRadioButton",
            "FlourishComboBox",
            "TextBox",
            "PasswordBox",
            "CheckBox",
            "RadioButton",
            "ComboBox",
        };
        var violations = new List<string>();

        foreach (
            var path in Directory.EnumerateFiles(
                viewsRoot,
                "*.xaml",
                SearchOption.AllDirectories
            )
        )
        {
            var document = LoadXaml(path);
            foreach (
                var body in document.Descendants().Where(element =>
                    element.Name.LocalName == "ListCard.ActionBody"
                )
            )
            {
                var interactiveControls = body
                    .Descendants()
                    .Where(element => interactiveControlNames.Contains(element.Name.LocalName))
                    .ToArray();
                if (interactiveControls.Length > 1)
                {
                    violations.Add(
                        $"{FormatViolation(path, body)} contains {interactiveControls.Length} interactive controls: "
                            + string.Join(
                                ", ",
                                interactiveControls.Select(element => element.Name.LocalName)
                            )
                    );
                }
            }
        }

        AssertNoArchitectureViolations(
            violations,
            "ListCard.ActionBody must contain at most one interactive control; split independent inputs and actions into separate rows."
        );
    }

    [Fact]
    public void GalleryListCardPeers_UseTheCompactSpacingToken()
    {
        const string compactMargin = "{DynamicResource FlourishListCardPeerMargin}";
        var viewsRoot = Path.Combine(RepositoryRoot, "src", "Gallery", "Views");
        var violations = new List<string>();

        foreach (
            var path in Directory.EnumerateFiles(
                viewsRoot,
                "*.xaml",
                SearchOption.AllDirectories
            )
        )
        {
            var document = LoadXaml(path);
            foreach (
                var listCard in document.Descendants().Where(element =>
                    element.Name.LocalName == nameof(ListCard)
                )
            )
            {
                var previousPeer = listCard.ElementsBeforeSelf().LastOrDefault();
                var followsListCard = previousPeer?.Name.LocalName == nameof(ListCard);
                var margin = (string?)listCard.Attribute("Margin");

                if (followsListCard && margin != compactMargin)
                {
                    violations.Add(
                        $"{FormatViolation(path, listCard)} follows another ListCard without the compact peer margin"
                    );
                }
                else if (!followsListCard && margin == compactMargin)
                {
                    violations.Add(
                        $"{FormatViolation(path, listCard)} adds peer spacing before the first ListCard in its group"
                    );
                }
            }
        }

        AssertNoArchitectureViolations(
            violations,
            "Consecutive ListCards must use the compact ListCard-specific margin between rows only."
        );
    }

    [Fact]
    public void GalleryListCards_DoNotAddApplyRows()
    {
        var viewsRoot = Path.Combine(RepositoryRoot, "src", "Gallery", "Views");
        var violations = new List<string>();

        foreach (
            var path in Directory.EnumerateFiles(
                viewsRoot,
                "*.xaml",
                SearchOption.AllDirectories
            )
        )
        {
            var document = LoadXaml(path);
            foreach (
                var listCard in document.Descendants().Where(element =>
                    element.Name.LocalName == nameof(ListCard)
                )
            )
            {
                var title = (string?)listCard.Attribute(nameof(Card.Title)) ?? string.Empty;
                var hasApplyHandler = listCard
                    .Descendants()
                    .Where(element => element.Name.LocalName == "Button")
                    .Select(element => (string?)element.Attribute("Click"))
                    .Any(handler =>
                        handler?.StartsWith("Apply", StringComparison.OrdinalIgnoreCase)
                        == true
                    );
                if (
                    title.StartsWith("Apply", StringComparison.OrdinalIgnoreCase)
                    || hasApplyHandler
                )
                {
                    violations.Add(FormatViolation(path, listCard));
                }
            }
        }

        AssertNoArchitectureViolations(
            violations,
            "ListCard settings must apply as their value is committed; do not add a separate Apply row."
        );
    }

    [Fact]
    public void GalleryComboBoxes_UseSelectionOnlyInteraction()
    {
        var viewsRoot = Path.Combine(RepositoryRoot, "src", "Gallery", "Views");
        var violations = new List<string>();

        foreach (
            var path in Directory.EnumerateFiles(
                viewsRoot,
                "*.xaml",
                SearchOption.AllDirectories
            )
        )
        {
            var document = LoadXaml(path);
            foreach (
                var comboBox in document.Descendants().Where(element =>
                    element.Name.LocalName is "ComboBox" or "FlourishComboBox"
                )
            )
            {
                if (
                    string.Equals(
                        (string?)comboBox.Attribute("IsEditable"),
                        "True",
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    violations.Add(FormatViolation(path, comboBox));
                }
            }
        }

        AssertNoArchitectureViolations(
            violations,
            "Gallery selection controls must open their choices directly instead of accepting free-form text. Use a separate text input when custom values are supported."
        );
    }

    [Fact]
    public void GalleryOutputSurfaces_UseTheDedicatedOutputCardControl()
    {
        var viewsRoot = Path.Combine(RepositoryRoot, "src", "Gallery", "Views");
        var violations = new List<string>();
        var outputCardCount = 0;

        foreach (
            var path in Directory.EnumerateFiles(
                viewsRoot,
                "*.xaml",
                SearchOption.AllDirectories
            )
        )
        {
            var document = LoadXaml(path);
            outputCardCount += document.Descendants().Count(element =>
                element.Name.LocalName == nameof(OutputCard)
            );
            foreach (
                var card in document.Descendants().Where(element =>
                    element.Name.LocalName is nameof(Card) or nameof(IconCard)
                )
            )
            {
                var title = (string?)card.Attribute(nameof(Card.Title)) ?? string.Empty;
                var hasOutputSemantics = title.Contains(
                        "Output",
                        StringComparison.OrdinalIgnoreCase
                    )
                    || title.Contains("Result", StringComparison.OrdinalIgnoreCase);
                if (hasOutputSemantics)
                {
                    violations.Add(
                        $"{FormatViolation(path, card)} uses {card.Name.LocalName} "
                            + $"with output title '{title}'"
                    );
                }
            }
        }

        Assert.True(outputCardCount > 0, "The Gallery must demonstrate OutputCard.");
        AssertNoArchitectureViolations(
            violations,
            "Gallery output and result histories must use OutputCard instead of a titled Card or IconCard."
        );
    }

    [Fact]
    public void GalleryTwoColumnSingleListCard_IsNotWrappedInAStackPanel()
    {
        var viewsRoot = Path.Combine(RepositoryRoot, "src", "Gallery", "Views");
        var violations = new List<string>();

        foreach (
            var path in Directory.EnumerateFiles(
                viewsRoot,
                "*.xaml",
                SearchOption.AllDirectories
            )
        )
        {
            var document = LoadXaml(path);
            foreach (
                var uniformGrid in document.Descendants().Where(element =>
                    element.Name.LocalName == "UniformGrid"
                        && (string?)element.Attribute("Columns") == "2"
                )
            )
            {
                foreach (
                    var stack in uniformGrid.Elements().Where(element =>
                        element.Name.LocalName == "StackPanel"
                    )
                )
                {
                    var listCards = stack
                        .Elements()
                        .Where(element => element.Name.LocalName == nameof(ListCard))
                        .ToArray();
                    if (listCards.Length == 1)
                    {
                        violations.Add(FormatViolation(path, stack));
                    }
                }
            }
        }

        AssertNoArchitectureViolations(
            violations,
            "A single ListCard must be a direct stretched column peer so its visible surface matches the OutputCard height."
        );
    }

    [Fact]
    public void GalleryCards_DoNotDeclareRetiredBodySlots()
    {
        var viewsRoot = Path.Combine(RepositoryRoot, "src", "Gallery", "Views");
        var violations = new List<string>();

        foreach (
            var path in Directory.EnumerateFiles(
                viewsRoot,
                "*.xaml",
                SearchOption.AllDirectories
            )
        )
        {
            var document = LoadXaml(path);
            if (document.Root?.Name.LocalName != "Page")
            {
                continue;
            }

            var retiredBodies = document
                .Descendants()
                .Where(element =>
                    element.Name.LocalName
                        is "Card.Body"
                            or "IconCard.Body"
                            or "ListCard.Body"
                );
            violations.AddRange(
                retiredBodies.Select(element => FormatViolation(path, element))
            );

            violations.AddRange(
                document
                    .Descendants()
                    .Where(element =>
                        (
                            element.Name.LocalName
                                is nameof(Card) or nameof(IconCard) or nameof(ListCard)
                        )
                        && element.Attribute("Body") is not null
                    )
                    .Select(element => FormatViolation(path, element))
            );
        }

        AssertNoArchitectureViolations(
            violations,
            "Card, IconCard, and ListCard do not expose Body; use MainText, Icon, or ListCard.ActionBody instead."
        );
    }

    [Fact]
    public void GalleryActionCards_KeepDynamicOutputInAPeerCard()
    {
        var viewsRoot = Path.Combine(RepositoryRoot, "src", "Gallery", "Views");
        var violations = new List<string>();

        foreach (
            var path in Directory.EnumerateFiles(
                viewsRoot,
                "*.xaml",
                SearchOption.AllDirectories
            )
        )
        {
            var document = LoadXaml(path);
            if (document.Root?.Name.LocalName != "Page")
            {
                continue;
            }

            foreach (
                var body in document
                    .Descendants()
                    .Where(element =>
                        element.Name.LocalName == "ListCard.ActionBody"
                    )
            )
            {
                var hasLocalAction = body.Descendants().Any(element =>
                    element.Name.LocalName
                        is "Button"
                            or nameof(IconButton)
                            or nameof(WindowCaptionButton)
                );
                if (!hasLocalAction)
                {
                    continue;
                }

                var namedStatus = body
                    .Descendants()
                    .Where(element => element.Name.LocalName == "FlourishTextBlock")
                    .Where(element => (string?)element.Attribute("Role") == "Status")
                    .Where(element =>
                        element.Attribute(XName.Get("Name", XamlNamespace)) is not null
                    );
                violations.AddRange(
                    namedStatus.Select(element => FormatViolation(path, element))
                );
            }
        }

        AssertNoArchitectureViolations(
            violations,
            "Dynamic response text must live in an adjacent OutputCard, not in the action card."
        );
    }

    [Fact]
    public void GalleryOutputCards_AreNamedContentlessStretchingPeers()
    {
        var outputCards = EnumerateXamlFiles(Path.Combine(GalleryRoot, "Views"))
            .SelectMany(path =>
            {
                var document = LoadXaml(path);
                return document
                    .Descendants()
                    .Where(element => element.Name.LocalName == nameof(OutputCard))
                    .Select(element => (Path: path, OutputCard: element));
            })
            .ToArray();
        var violations = new List<string>();

        foreach (var item in outputCards)
        {
            var outputCard = item.OutputCard;
            var location = FormatViolation(item.Path, outputCard);
            if (outputCard.Attribute(XName.Get("Name", XamlNamespace)) is null)
            {
                violations.Add($"{location} has no x:Name for WriteLine calls");
            }

            if ((string?)outputCard.Attribute("VerticalAlignment") != "Stretch")
            {
                violations.Add($"{location} does not stretch to its peer column");
            }

            if (outputCard.Elements().Any())
            {
                violations.Add($"{location} declares inline content");
            }

            string[] forbiddenProperties =
            [
                nameof(Card.Title),
                nameof(Card.MainText),
                "Body",
                nameof(OutputCard.Output),
            ];
            foreach (var property in forbiddenProperties)
            {
                if (outputCard.Attribute(property) is not null)
                {
                    violations.Add($"{location} sets forbidden property {property}");
                }
            }
        }

        Assert.NotEmpty(outputCards);
        AssertNoArchitectureViolations(
            violations,
            "Gallery OutputCards must be named, contentless, append-only stretching peers; typography and scrolling belong to the control template."
        );
    }

    [Fact]
    public void ProductTypography_DeclaresOnlyTheCanonicalIncreasingDefaultFontScale()
    {
        var typography = LoadXaml(
            Path.Combine(FlourishRoot, "Themes", "Typography.xaml")
        );
        var typographySizeResources = typography
            .Descendants()
            .Select(element => new
            {
                Element = element,
                Name = (string?)element.Attribute(XName.Get("Key", XamlNamespace)),
            })
            .Where(resource =>
                resource.Name?.StartsWith("FlourishFontSize", StringComparison.Ordinal)
                == true
            )
            .ToArray();

        Assert.Equal(
            CanonicalFontSizeResourceNames,
            typographySizeResources.Select(resource => resource.Name)
        );

        var actualSizes = typographySizeResources.ToDictionary(
            resource => resource.Name!,
            resource => XmlConvert.ToDouble(resource.Element.Value.Trim()),
            StringComparer.Ordinal
        );
        Assert.Equal(12d, actualSizes["FlourishFontSizeSmall"]);
        Assert.Equal(14d, actualSizes["FlourishFontSizeStandard"]);
        Assert.Equal(16d, actualSizes["FlourishFontSizeIcon"]);
        Assert.Equal(16d, actualSizes["FlourishFontSizeLarge"]);
        Assert.Equal(24d, actualSizes["FlourishFontSizeExtraLarge"]);
        Assert.Equal(32d, actualSizes["FlourishFontSizeHeaderSize"]);

        var allowedNames = CanonicalFontSizeResourceNames.ToHashSet(
            StringComparer.Ordinal
        );
        var violations = new List<string>();

        foreach (var root in new[] { FlourishRoot, GalleryRoot })
        {
            foreach (var file in EnumerateProductSourceFiles(root))
            {
                var lineNumber = 0;
                foreach (var line in File.ReadLines(file))
                {
                    lineNumber++;
                    foreach (Match match in FontSizeResourceNamePattern.Matches(line))
                    {
                        if (!allowedNames.Contains(match.Value))
                        {
                            violations.Add(
                                $"{RelativePath(file)}:{lineNumber} ({match.Value})"
                            );
                        }
                    }
                }
            }
        }

        AssertNoArchitectureViolations(
            violations,
            "Product source may only reference the four text tiers and the dedicated Icon font-size resource."
        );

        var contextualIconSizes = typographySizeResources.First().Element.Parent!
            .Elements()
            .Where(element =>
                ((string?)element.Attribute(XName.Get("Key", XamlNamespace)))?.StartsWith(
                    "FlourishIconFontSize",
                    StringComparison.Ordinal
                ) == true
            )
            .ToDictionary(
                element => (string)element.Attribute(XName.Get("Key", XamlNamespace))!,
                element => XmlConvert.ToDouble(element.Value.Trim()),
                StringComparer.Ordinal
            );
        Assert.Equal(ContextualIconFontSizes.Count, contextualIconSizes.Count);
        foreach (var expected in ContextualIconFontSizes)
        {
            Assert.Equal(expected.Value, contextualIconSizes[expected.Key]);
        }
    }

    [Fact]
    public void ProductTypography_DeclaresTieredLineHeightsAndBottomSpaces()
    {
        var typography = LoadXaml(
            Path.Combine(FlourishRoot, "Themes", "Typography.xaml")
        );
        var keyName = XName.Get("Key", XamlNamespace);
        var resources = typography
            .Descendants()
            .Where(element => element.Attribute(keyName) is not null)
            .ToDictionary(
                element => (string)element.Attribute(keyName)!,
                StringComparer.Ordinal
            );

        (string Tier, double FontSize, double LineHeight, Thickness BottomSpace)[] tiers =
        [
            ("Small", 12, 14, new Thickness(0, 0, 0, 1)),
            ("Standard", 14, 16, new Thickness(0, 0, 0, 1)),
            ("Large", 16, 20, new Thickness(0, 0, 0, 2)),
            ("ExtraLarge", 24, 29, new Thickness(0, 0, 0, 3)),
            ("HeaderSize", 32, 37, new Thickness(0, 0, 0, 4)),
        ];

        foreach (var (tier, fontSize, lineHeight, bottomSpace) in tiers)
        {
            Assert.Equal(
                fontSize,
                XmlConvert.ToDouble(resources[$"FlourishFontSize{tier}"].Value.Trim())
            );
            Assert.Equal(
                lineHeight,
                XmlConvert.ToDouble(resources[$"FlourishLineHeight{tier}"].Value.Trim())
            );
            Assert.Equal(
                bottomSpace,
                ParseThickness(
                    resources[$"FlourishTypographyBottomSpace{tier}"].Value.Trim()
                )
            );
            Assert.True(lineHeight >= fontSize);
        }

        Assert.Equal(
            16d,
            XmlConvert.ToDouble(resources["FlourishFontSizeIcon"].Value.Trim())
        );
        Assert.Equal(
            16d,
            XmlConvert.ToDouble(resources["FlourishLineHeightIcon"].Value.Trim())
        );
        Assert.Equal(
            new Thickness(),
            ParseThickness(resources["FlourishTypographyBottomSpaceIcon"].Value.Trim())
        );
    }

    [Fact]
    public void FontApis_ExposeOnlyTheExplicitTextAndIconScaleContract()
    {
        Type[] explicitScaleTypes =
        [
            typeof(string),
            typeof(double),
            typeof(double),
            typeof(double),
            typeof(double),
            typeof(double),
            typeof(double),
        ];
        string[] explicitScaleNames =
        [
            "fontFamily",
            "smallFontSize",
            "standardFontSize",
            "iconFontSize",
            "largeFontSize",
            "extraLargeFontSize",
            "headerSizeFontSize",
        ];
        Type[] nullableScaleTypes =
        [
            typeof(string),
            typeof(double?),
            typeof(double?),
            typeof(double?),
            typeof(double?),
            typeof(double?),
            typeof(double?),
        ];

        var builderGlobalFont = Assert.Single(
            typeof(IFlourishShellBuilder).GetMethods(),
            method => method.Name == nameof(IFlourishShellBuilder.UseGlobalFont)
        );
        AssertOptionalParameterContract(
            builderGlobalFont,
            explicitScaleTypes,
            explicitScaleNames,
            ["Microsoft Yahei", 12d, 14d, 16d, 16d, 24d, 32d]
        );

        var serviceSetFont = Assert.Single(
            typeof(IFontService).GetMethods(),
            method => method.Name == nameof(IFontService.SetFont)
        );
        AssertParameterContract(serviceSetFont, explicitScaleTypes, explicitScaleNames);

        var builderPageOverride = Assert.Single(
            typeof(IFlourishShellBuilder).GetMethods(),
            method => method.Name == nameof(IFlourishShellBuilder.SetOverrideFont)
        );
        Assert.True(builderPageOverride.IsGenericMethodDefinition);
        AssertParameterContract(builderPageOverride, nullableScaleTypes, explicitScaleNames);

        var servicePageOverrides = typeof(IFontService)
            .GetMethods()
            .Where(method => method.Name == nameof(IFontService.SetOverrideFont))
            .ToArray();
        Assert.Equal(2, servicePageOverrides.Length);
        var genericServicePageOverride = Assert.Single(
            servicePageOverrides,
            method => method.IsGenericMethodDefinition
        );
        AssertParameterContract(
            genericServicePageOverride,
            nullableScaleTypes,
            explicitScaleNames
        );
        var runtimeServicePageOverride = Assert.Single(
            servicePageOverrides,
            method => !method.IsGenericMethod
        );
        AssertParameterContract(
            runtimeServicePageOverride,
            [typeof(Type), .. nullableScaleTypes],
            ["pageType", .. explicitScaleNames]
        );

        var pageOverrideConstructor = Assert.Single(
            typeof(FlourishPageFontOverride).GetConstructors()
        );
        AssertParameterContract(
            pageOverrideConstructor,
            nullableScaleTypes,
            explicitScaleNames
        );

        var fontAssemblyApiMethods = typeof(IFontService)
            .Assembly
            .GetTypes()
            .SelectMany(type =>
                type.GetMethods(
                    BindingFlags.Public
                        | BindingFlags.NonPublic
                        | BindingFlags.Instance
                        | BindingFlags.Static
                        | BindingFlags.DeclaredOnly
                )
            )
            .Where(method =>
                method.Name
                    is nameof(IFlourishShellBuilder.UseGlobalFont)
                        or nameof(IFontService.SetFont)
                        or nameof(IFontService.SetOverrideFont)
            )
            .ToArray();

        Assert.Equal(
            2,
            fontAssemblyApiMethods.Count(method =>
                method.Name == nameof(IFlourishShellBuilder.UseGlobalFont)
            )
        );
        Assert.Equal(
            2,
            fontAssemblyApiMethods.Count(method => method.Name == nameof(IFontService.SetFont))
        );
        Assert.Equal(
            6,
            fontAssemblyApiMethods.Count(method =>
                method.Name == nameof(IFontService.SetOverrideFont)
            )
        );
        Assert.All(
            fontAssemblyApiMethods.Where(method =>
                method.Name == nameof(IFlourishShellBuilder.UseGlobalFont)
            ),
            method =>
                AssertOptionalParameterContract(
                    method,
                    explicitScaleTypes,
                    explicitScaleNames,
                    ["Microsoft Yahei", 12d, 14d, 16d, 16d, 24d, 32d]
                )
        );
        Assert.All(
            fontAssemblyApiMethods.Where(method =>
                method.Name == nameof(IFontService.SetFont)
            ),
            method => AssertParameterContract(method, explicitScaleTypes, explicitScaleNames)
        );
        Assert.All(
            fontAssemblyApiMethods.Where(method =>
                method.Name == nameof(IFontService.SetOverrideFont)
            ),
            method =>
            {
                if (method.IsGenericMethodDefinition)
                {
                    AssertParameterContract(
                        method,
                        nullableScaleTypes,
                        explicitScaleNames
                    );
                    return;
                }

                AssertParameterContract(
                    method,
                    [typeof(Type), .. nullableScaleTypes],
                    ["pageType", .. explicitScaleNames]
                );
            }
        );

        var violations = new List<string>();
        foreach (var root in new[] { FlourishRoot, GalleryRoot })
        {
            foreach (var file in EnumerateProductSourceFiles(root))
            {
                var lineNumber = 0;
                foreach (var line in File.ReadLines(file))
                {
                    lineNumber++;
                    foreach (Match match in RetiredFontApiPattern.Matches(line))
                    {
                        violations.Add(
                            $"{RelativePath(file)}:{lineNumber} ({match.Value})"
                        );
                    }
                }
            }
        }

        AssertNoArchitectureViolations(
            violations,
            "The explicit text and icon font contract must not retain gap-based resources, mutators, or compatibility APIs."
        );
    }

    [Fact]
    public void ProductXaml_FontSizesUseCanonicalResourcesOrTemplateBindings()
    {
        var allowedValues = CanonicalFontSizeResourceNames
            .Concat(ContextualIconFontSizes.Keys)
            .Select(name => $"{{DynamicResource {name}}}")
            .Append("{TemplateBinding FontSize}")
            .ToHashSet(StringComparer.Ordinal);
        var violations = new List<string>();

        foreach (var root in new[] { FlourishRoot, GalleryRoot })
        {
            foreach (
                var file in EnumerateProductSourceFiles(root).Where(file =>
                    Path.GetExtension(file) == ".xaml"
                )
            )
            {
                var document = LoadXaml(file);

                foreach (
                    var attribute in document
                        .Root!
                        .DescendantsAndSelf()
                        .SelectMany(element => element.Attributes())
                        .Where(attribute => IsFontSizePropertyName(attribute.Name.LocalName))
                )
                {
                    if (!allowedValues.Contains(attribute.Value))
                    {
                        violations.Add(
                            $"{FormatViolation(file, attribute)} ({attribute.Value})"
                        );
                    }
                }

                foreach (
                    var setter in document.Descendants().Where(element =>
                        element.Name.LocalName == "Setter"
                        && IsFontSizePropertyName(
                            (string?)element.Attribute("Property") ?? string.Empty
                        )
                    )
                )
                {
                    var value = (string?)setter.Attribute("Value") ?? setter.Value.Trim();
                    if (!allowedValues.Contains(value))
                    {
                        violations.Add(
                            $"{FormatViolation(file, setter)} ({value})"
                        );
                    }
                }

                foreach (
                    var propertyElement in document
                        .Descendants()
                        .Where(element => IsFontSizePropertyName(element.Name.LocalName))
                )
                {
                    var value = propertyElement.Value.Trim();
                    if (!allowedValues.Contains(value))
                    {
                        violations.Add(
                            $"{FormatViolation(file, propertyElement)} ({value})"
                        );
                    }
                }
            }
        }

        AssertNoArchitectureViolations(
            violations,
            "FontSize values must use a canonical text or icon dynamic resource; control templates may forward FontSize with TemplateBinding."
        );
    }

    [Fact]
    public void ProductXaml_AllFontGlyphEntryPointsUseTheIconFamilyAndIconSize()
    {
        const string iconFamily = "{DynamicResource FlourishIconFontFamily}";
        var allowedIconSizes = ContextualIconFontSizes.Keys
            .Prepend("FlourishFontSizeIcon")
            .Select(name => $"{{DynamicResource {name}}}")
            .ToHashSet(StringComparer.Ordinal);
        var violations = new List<string>();
        var iconEntryCount = 0;

        foreach (var root in new[] { FlourishRoot, GalleryRoot })
        {
            foreach (var file in EnumerateXamlFiles(root))
            {
                var document = LoadXaml(file);
                foreach (var element in document.Root!.DescendantsAndSelf())
                {
                    foreach (
                        var familyAttribute in element.Attributes().Where(attribute =>
                            attribute.Name.LocalName
                                is "FontFamily" or "TextElement.FontFamily"
                            && attribute.Value == iconFamily
                        )
                    )
                    {
                        iconEntryCount++;
                        var sizeProperty = familyAttribute.Name.LocalName.Replace(
                            "FontFamily",
                            "FontSize",
                            StringComparison.Ordinal
                        );
                        var sizeValue = element
                            .Attributes()
                            .SingleOrDefault(attribute =>
                                attribute.Name.LocalName == sizeProperty
                            )
                            ?.Value;
                        if (sizeValue is null || !allowedIconSizes.Contains(sizeValue))
                        {
                            violations.Add(
                                $"{FormatViolation(file, element)} ({sizeProperty}={sizeValue ?? "<missing>"})"
                            );
                        }

                        if (
                            element.Name.LocalName == "FlourishTextBlock"
                            && (string?)element.Attribute("Role") != "Icon"
                        )
                        {
                            violations.Add(
                                $"{FormatViolation(file, element)} (FlourishTextBlock icon glyph is missing Role=Icon)"
                            );
                        }
                    }

                    if ((string?)element.Attribute("Role") == "Icon")
                    {
                        iconEntryCount++;
                        var explicitSize = (string?)element.Attribute("FontSize");
                        var explicitPadding = (string?)element.Attribute("Padding");
                        if (explicitSize is not null && !allowedIconSizes.Contains(explicitSize))
                        {
                            violations.Add(
                                $"{FormatViolation(file, element)} (FontSize={explicitSize})"
                            );
                        }
                        if (
                            explicitPadding is not null
                            && explicitPadding
                                != "{DynamicResource FlourishTypographyBottomSpaceIcon}"
                            && explicitPadding != "0"
                        )
                        {
                            violations.Add(
                                $"{FormatViolation(file, element)} (Padding={explicitPadding})"
                            );
                        }
                    }
                }
            }
        }

        Assert.True(iconEntryCount > 0);
        AssertNoArchitectureViolations(
            violations,
            "Every font-glyph entry point must resolve the icon family, an approved default or contextual icon size, and zero-bottom-space Icon role."
        );

        var shellSource = File.ReadAllText(
            Path.Combine(FlourishRoot, "Views", "Windows", "FlourishShellWindow.xaml.cs")
        );
        var iconBindingCalls = Regex.Matches(
            shellSource,
            "BindIconTypography\\(\\s*[^,\\r\\n]+\\s*,\\s*\"(?<key>[^\"]+)\"\\s*\\)",
            RegexOptions.CultureInvariant
        );
        Assert.NotEmpty(iconBindingCalls.Cast<Match>());
        var allowedBindingKeys = ContextualIconFontSizes.Keys
            .Prepend("FlourishFontSizeIcon")
            .ToHashSet(StringComparer.Ordinal);
        Assert.All(iconBindingCalls.Cast<Match>(), match =>
            Assert.Contains(match.Groups["key"].Value, allowedBindingKeys)
        );

        Assert.Contains(
            "textBlock.TextAlignment = System.Windows.TextAlignment.Center;",
            shellSource,
            StringComparison.Ordinal
        );
        Assert.Contains(
            "textBlock.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;",
            shellSource,
            StringComparison.Ordinal
        );
        Assert.Contains(
            "textBlock.SetResourceReference(TextBlock.LineHeightProperty, sizeResourceKey);",
            shellSource,
            StringComparison.Ordinal
        );
    }

    [Fact]
    public void ProductXaml_ReservesTheDedicatedIconSizeForFontGlyphEntryPoints()
    {
        const string iconFamily = "{DynamicResource FlourishIconFontFamily}";
        var iconSizes = ContextualIconFontSizes.Keys
            .Prepend("FlourishFontSizeIcon")
            .Select(name => $"{{DynamicResource {name}}}")
            .ToHashSet(StringComparer.Ordinal);
        var violations = new List<string>();

        foreach (var root in new[] { FlourishRoot, GalleryRoot })
        {
            foreach (var file in EnumerateXamlFiles(root))
            {
                var document = LoadXaml(file);
                foreach (var element in document.Root!.DescendantsAndSelf())
                {
                    foreach (
                        var sizeAttribute in element.Attributes().Where(attribute =>
                            IsFontSizePropertyName(attribute.Name.LocalName)
                            && iconSizes.Contains(attribute.Value)
                        )
                    )
                    {
                        var matchingFamilyProperty = sizeAttribute.Name.LocalName.Replace(
                            "FontSize",
                            "FontFamily",
                            StringComparison.Ordinal
                        );
                        var isDirectIconEntry =
                            (string?)element.Attribute("Role") == "Icon"
                            || element.Attributes().Any(attribute =>
                                attribute.Name.LocalName == matchingFamilyProperty
                                && attribute.Value == iconFamily
                            );
                        var isIconRoleSetter =
                            element.Name.LocalName == "Setter"
                            && (string?)element.Attribute("Property") == "FontSize"
                            && element.Parent is { } trigger
                            && trigger.Name.LocalName == "Trigger"
                            && (string?)trigger.Attribute("Property") == "Role"
                            && (string?)trigger.Attribute("Value") == "Icon"
                            && trigger.Elements().Any(sibling =>
                                sibling.Name.LocalName == "Setter"
                                && (string?)sibling.Attribute("Property") == "FontFamily"
                                && (string?)sibling.Attribute("Value") == iconFamily
                            );

                        if (!isDirectIconEntry && !isIconRoleSetter)
                        {
                            violations.Add(FormatViolation(file, sizeAttribute));
                        }
                    }
                }
            }
        }

        AssertNoArchitectureViolations(
            violations,
            "Default and contextual icon font-size resources are reserved for glyph entry points; text must use one of the four text tiers."
        );
    }

    [Fact]
    public void ProductCode_DoesNotAssignLiteralFontSizes()
    {
        var violations = new List<string>();

        foreach (var root in new[] { FlourishRoot, GalleryRoot })
        {
            foreach (
                var file in EnumerateProductSourceFiles(root).Where(file =>
                    Path.GetExtension(file) == ".cs"
                )
            )
            {
                var lineNumber = 0;
                foreach (var line in File.ReadLines(file))
                {
                    lineNumber++;
                    if (LiteralFontSizeAssignmentPattern.IsMatch(line))
                    {
                        violations.Add($"{RelativePath(file)}:{lineNumber} ({line.Trim()})");
                    }
                }
            }
        }

        AssertNoArchitectureViolations(
            violations,
            "Product code must bind UI font sizes to the canonical typography resources instead of assigning numeric literals."
        );
    }

    [Fact]
    public void TextRoles_MapToCanonicalSizeLineHeightBottomSpaceAndWeightMetrics()
    {
        var document = LoadXaml(
            Path.Combine(FlourishRoot, "Controls", "TextBlock.xaml")
        );
        var style = document
            .Descendants()
            .Single(element =>
                element.Name.LocalName == "Style"
                && (string?)element.Attribute("TargetType")
                    == "{x:Type controls:FlourishTextBlock}"
            );
        static string? SetterValue(XElement owner, string property)
        {
            return owner
                .Elements()
                .SingleOrDefault(element =>
                    element.Name.LocalName == "Setter"
                    && (string?)element.Attribute("Property") == property
                )
                ?.Attribute("Value")
                ?.Value;
        }

        var baseFontFamily = SetterValue(style, "FontFamily");
        var baseFontSize = SetterValue(style, "FontSize");
        var baseFontWeight = SetterValue(style, "FontWeight");
        var baseLineHeight = SetterValue(style, "LineHeight");
        var baseBottomSpace = SetterValue(style, "Padding");

        Assert.Equal("{DynamicResource FlourishFontFamily}", baseFontFamily);
        Assert.Equal("{DynamicResource FlourishFontSizeStandard}", baseFontSize);
        Assert.Equal("Regular", baseFontWeight);
        Assert.Equal("{DynamicResource FlourishLineHeightStandard}", baseLineHeight);
        Assert.Equal(
            "{DynamicResource FlourishTypographyBottomSpaceStandard}",
            baseBottomSpace
        );
        Assert.Equal("BlockLineHeight", SetterValue(style, "LineStackingStrategy"));

        var roleTriggers = style
            .Descendants()
            .Where(element =>
                element.Name.LocalName == "Trigger"
                && (string?)element.Attribute("Property") == "Role"
            )
            .ToDictionary(
                element => (string)element.Attribute("Value")!,
                StringComparer.Ordinal
            );

        foreach (var role in Enum.GetValues<FlourishTextRole>())
        {
            var expectedTier = role switch
            {
                FlourishTextRole.Caption or FlourishTextRole.Status => "Small",
                FlourishTextRole.Icon => "Icon",
                FlourishTextRole.CardTitle => "Large",
                FlourishTextRole.SectionTitle => "ExtraLarge",
                FlourishTextRole.PageTitle => "HeaderSize",
                _ => "Standard",
            };
            var actualFamily = baseFontFamily;
            var actualSize = baseFontSize;
            var actualWeight = baseFontWeight;
            var actualLineHeight = baseLineHeight;
            var actualBottomSpace = baseBottomSpace;

            if (roleTriggers.TryGetValue(role.ToString(), out var trigger))
            {
                actualFamily = SetterValue(trigger, "FontFamily") ?? actualFamily;
                actualSize = SetterValue(trigger, "FontSize") ?? actualSize;
                actualWeight = SetterValue(trigger, "FontWeight") ?? actualWeight;
                actualLineHeight = SetterValue(trigger, "LineHeight") ?? actualLineHeight;
                actualBottomSpace = SetterValue(trigger, "Padding") ?? actualBottomSpace;
            }

            Assert.Equal(
                $"{{DynamicResource FlourishFontSize{expectedTier}}}",
                actualSize
            );
            Assert.Equal(
                $"{{DynamicResource FlourishLineHeight{expectedTier}}}",
                actualLineHeight
            );
            Assert.Equal(
                $"{{DynamicResource FlourishTypographyBottomSpace{expectedTier}}}",
                actualBottomSpace
            );
            Assert.Equal(
                expectedTier is "Large" or "ExtraLarge" or "HeaderSize" ? "Bold" : "Regular",
                actualWeight
            );
            Assert.Equal(
                role == FlourishTextRole.Icon
                    ? "{DynamicResource FlourishIconFontFamily}"
                    : "{DynamicResource FlourishFontFamily}",
                actualFamily
            );
        }
    }

    [Fact]
    public void CardPresenterAndChunkTitleHosts_UseTheirCanonicalHeadingRoles()
    {
        (string FileName, string ExpectedRole)[] expectations =
        [
            ("Card.xaml", "CardTitle"),
            ("CardButton.xaml", "CardTitle"),
            ("IconCard.xaml", "CardTitle"),
            ("ListCard.xaml", "CardTitle"),
            ("Presenter.xaml", "CardTitle"),
            ("Chunk.xaml", "SectionTitle"),
            ("ChunkHero.xaml", "PageTitle"),
        ];

        foreach (var (fileName, expectedRole) in expectations)
        {
            var document = LoadXaml(
                Path.Combine(FlourishRoot, "Controls", fileName)
            );
            var titleHost = document
                .Descendants()
                .Single(element =>
                    (string?)element.Attribute(XName.Get("Name", XamlNamespace))
                    == "TitleHost"
                );

            Assert.Equal(expectedRole, (string?)titleHost.Attribute("Role"));
        }
    }

    [Fact]
    public void ShellApplicationLogoFallback_UsesTheStandardFontSize()
    {
        var shell = LoadXaml(
            Path.Combine(FlourishRoot, "Views", "Windows", "FlourishShellWindow.xaml")
        );
        var fallback = shell
            .Descendants()
            .Single(element =>
                (string?)element.Attribute(XName.Get("Name", XamlNamespace))
                == "ApplicationInfoLogoFallback"
            );

        Assert.Equal(
            "{DynamicResource FlourishFontSizeStandard}",
            (string?)fallback.Attribute("FontSize")
        );
    }

    [Fact]
    public void ShellTypography_UsesRootPixelAlignmentAndWpfTextRenderingDefaults()
    {
        var shell = LoadXaml(
            Path.Combine(FlourishRoot, "Views", "Windows", "FlourishShellWindow.xaml")
        );
        Assert.Equal("True", (string?)shell.Root?.Attribute("SnapsToDevicePixels"));
        Assert.Equal("True", (string?)shell.Root?.Attribute("UseLayoutRounding"));

        string[] textOptionNames =
        [
            "TextOptions.TextFormattingMode",
            "TextOptions.TextRenderingMode",
            "TextOptions.TextHintingMode",
        ];
        var textOptionOverrides = EnumerateXamlFiles(FlourishRoot)
            .SelectMany(file =>
            {
                var document = LoadXaml(file);
                return document
                    .Root!
                    .DescendantsAndSelf()
                    .SelectMany(element => element.Attributes())
                    .Where(attribute => textOptionNames.Contains(attribute.Name.LocalName))
                    .Select(attribute => FormatViolation(file, attribute));
            })
            .ToArray();

        AssertNoArchitectureViolations(
            textOptionOverrides,
            "Flourish XAML must preserve WPF's default text rendering options."
        );
    }

    [Fact]
    public void ProductTypography_UsesOnlyRegularAndBoldFontWeights()
    {
        var violations = new List<string>();

        foreach (var file in EnumerateXamlFiles(FlourishRoot))
        {
            var document = LoadXaml(file);
            var fontWeights = document
                .Root!
                .DescendantsAndSelf()
                .SelectMany(element => element.Attributes())
                .Where(attribute =>
                    attribute.Name.LocalName == "FontWeight"
                    || (
                        attribute.Name.LocalName == "Value"
                        && (string?)attribute.Parent?.Attribute("Property") == "FontWeight"
                    )
                );
            foreach (var fontWeight in fontWeights)
            {
                if (fontWeight.Value is not ("Regular" or "Bold"))
                {
                    violations.Add(
                        $"{FormatViolation(file, fontWeight)} ({fontWeight.Value})"
                    );
                }
            }
        }

        string[] forbiddenCodeWeights =
        [
            "FontWeights.Thin",
            "FontWeights.ExtraLight",
            "FontWeights.UltraLight",
            "FontWeights.Light",
            "FontWeights.SemiLight",
            "FontWeights.DemiLight",
            "FontWeights.Medium",
            "FontWeights.SemiBold",
            "FontWeights.DemiBold",
        ];
        foreach (
            var file in Directory.EnumerateFiles(FlourishRoot, "*.cs", SearchOption.AllDirectories)
        )
        {
            var lineNumber = 0;
            foreach (var line in File.ReadLines(file))
            {
                lineNumber++;
                foreach (var forbiddenWeight in forbiddenCodeWeights)
                {
                    if (line.Contains(forbiddenWeight, StringComparison.Ordinal))
                    {
                        violations.Add(
                            $"{RelativePath(file)}:{lineNumber} ({forbiddenWeight})"
                        );
                    }
                }
            }
        }

        AssertNoArchitectureViolations(
            violations,
            "Product typography must use Regular body text and Bold headings."
        );

        (string File, string ElementName)[] namedHeadings =
        [
            ("Views/Windows/FlourishShellWindow.xaml", "NavigationGroupHeader"),
            ("Views/Windows/TitleBar.xaml", "TitleComboBox"),
            ("Views/Windows/FlourishMessageBoxWindow.xaml", "CaptionText"),
            ("Views/Page/ProfilePage.xaml", "DisplayNameText"),
        ];
        foreach (var (file, elementName) in namedHeadings)
        {
            var document = LoadXaml(Path.Combine(FlourishRoot, NormalizePlatformPath(file)));
            var heading = document
                .Descendants()
                .Single(element =>
                    (string?)element.Attribute(XName.Get("Name", XamlNamespace)) == elementName
                );
            Assert.Equal("Bold", (string?)heading.Attribute("FontWeight"));
        }
    }

    [Fact]
    public void HomeDemoCards_ExposeAccessibleAutomationNames()
    {
        var document = LoadXaml(
            Path.Combine(RepositoryRoot, "src", "Gallery", "Views", "HomePage.xaml")
        );
        var demoCards = document
            .Descendants()
            .Where(element =>
                element.Name.LocalName == "CardButton"
                && element.Attribute("Tag") is not null
            )
            .ToArray();

        Assert.Equal(9, demoCards.Length);
        Assert.All(
            demoCards,
            card => Assert.False(
                string.IsNullOrWhiteSpace(
                    (string?)card.Attribute("AutomationProperties.Name")
                )
            )
        );
    }

    [Theory]
    [InlineData("Colors.Light.xaml")]
    [InlineData("Colors.Dark.xaml")]
    public void ColorPalettes_UseCanonicalResourceNames(string fileName)
    {
        var document = LoadXaml(
            Path.Combine(FlourishRoot, "Themes", "Colors", fileName)
        );
        var keys = document
            .Descendants()
            .Select(element =>
                (string?)element.Attribute(XName.Get("Key", XamlNamespace))
            )
            .OfType<string>()
            .ToArray();

        Assert.NotEmpty(keys);
        Assert.All(keys, key => Assert.StartsWith("Flourish", key));
        Assert.Equal(keys.Length, keys.Distinct(StringComparer.Ordinal).Count());
    }

    private static Type[] GetPublicFlourishControlTypes()
    {
        return typeof(FlourishButton)
            .Assembly.GetExportedTypes()
            .Where(type =>
                type.Namespace == "ArkheideSystem.Flourish.Controls"
                && typeof(FrameworkElement).IsAssignableFrom(type)
                && !type.IsAbstract
            )
            .OrderBy(type => type.FullName, StringComparer.Ordinal)
            .ToArray();
    }

    private static string GetControlFileName(Type type)
    {
        return type.Name.StartsWith("Flourish", StringComparison.Ordinal)
            ? type.Name["Flourish".Length..]
            : type.Name;
    }

    private static string[] GetMergedDictionarySources(XDocument document)
    {
        var mergedDictionaries = document.Root?.Element(
            XName.Get("ResourceDictionary.MergedDictionaries", PresentationNamespace)
        );
        return mergedDictionaries
                ?.Elements()
                .Where(element => element.Name.LocalName == "ResourceDictionary")
                .Select(element => (string?)element.Attribute("Source"))
                .Where(source => source is not null)
                .Cast<string>()
                .ToArray()
            ?? [];
    }

    private static bool IsInsidePopup(XElement element)
    {
        return element.Ancestors().Any(ancestor => ancestor.Name.LocalName == "Popup");
    }

    private static bool IsPropertyElement(XElement element)
    {
        return element.Name.LocalName.Contains('.', StringComparison.Ordinal);
    }

    private static bool HasChunkBody(XElement chunk)
    {
        var explicitBody = chunk
            .Elements()
            .FirstOrDefault(element =>
                element.Name.LocalName == $"{nameof(Chunk)}.{nameof(Chunk.Body)}"
            );
        if (explicitBody is not null)
        {
            return explicitBody.Nodes().Any(IsMeaningfulContentNode);
        }

        return chunk.Nodes().Any(node =>
            node is XElement element && !IsPropertyElement(element)
            || IsMeaningfulTextNode(node)
        );
    }

    private static bool IsMeaningfulContentNode(XNode node)
    {
        return node is XElement || IsMeaningfulTextNode(node);
    }

    private static bool IsMeaningfulTextNode(XNode node)
    {
        return node is XText text && !string.IsNullOrWhiteSpace(text.Value);
    }

    private static void AssertNoArchitectureViolations(
        IReadOnlyCollection<string> violations,
        string message
    )
    {
        Assert.True(
            violations.Count == 0,
            message
                + Environment.NewLine
                + string.Join(Environment.NewLine, violations)
        );
    }

    private static XDocument LoadXaml(string file)
    {
        return XDocument.Load(file, LoadOptions.SetLineInfo);
    }

    private static IEnumerable<string> EnumerateXamlFiles(string directory)
    {
        return Directory.EnumerateFiles(directory, "*.xaml", SearchOption.AllDirectories);
    }

    private static IEnumerable<string> EnumerateProductSourceFiles(string directory)
    {
        return Directory
            .EnumerateFiles(directory, "*", SearchOption.AllDirectories)
            .Where(file =>
                Path.GetExtension(file) is ".cs" or ".xaml"
                && !NormalizePath(Path.GetRelativePath(directory, file))
                    .Split('/')
                    .Any(segment => segment is "bin" or "obj")
            );
    }

    private static bool IsFontSizePropertyName(string propertyName)
    {
        return propertyName == "FontSize"
            || propertyName.EndsWith(".FontSize", StringComparison.Ordinal);
    }

    private static bool IsFontSizeSetter(XElement element)
    {
        return element.Name.LocalName == "Setter"
            && IsFontSizePropertyName(
                (string?)element.Attribute("Property") ?? string.Empty
            );
    }

    private static bool IsFontWeightSetter(XElement element)
    {
        return element.Name.LocalName == "Setter"
            && (string?)element.Attribute("Property") == "FontWeight";
    }

    private static Thickness ParseThickness(string value)
    {
        var values = value
            .Split(',')
            .Select(part => XmlConvert.ToDouble(part.Trim()))
            .ToArray();
        return values.Length switch
        {
            1 => new Thickness(values[0]),
            2 => new Thickness(values[0], values[1], values[0], values[1]),
            4 => new Thickness(values[0], values[1], values[2], values[3]),
            _ => throw new InvalidDataException($"'{value}' is not a WPF Thickness."),
        };
    }

    private static void AssertParameterContract(
        MethodBase method,
        IReadOnlyList<Type> expectedTypes,
        IReadOnlyList<string> expectedNames
    )
    {
        var parameters = method.GetParameters();
        Assert.Equal(expectedTypes.Count, parameters.Length);
        Assert.Equal(expectedTypes, parameters.Select(parameter => parameter.ParameterType));
        Assert.Equal(expectedNames, parameters.Select(parameter => parameter.Name));
        Assert.All(parameters, parameter => Assert.False(parameter.IsOptional));
    }

    private static void AssertOptionalParameterContract(
        MethodBase method,
        IReadOnlyList<Type> expectedTypes,
        IReadOnlyList<string> expectedNames,
        IReadOnlyList<object> expectedDefaultValues
    )
    {
        var parameters = method.GetParameters();
        Assert.Equal(expectedTypes.Count, parameters.Length);
        Assert.Equal(expectedTypes, parameters.Select(parameter => parameter.ParameterType));
        Assert.Equal(expectedNames, parameters.Select(parameter => parameter.Name));
        Assert.Equal(expectedDefaultValues, parameters.Select(parameter => parameter.DefaultValue));
        Assert.All(parameters, parameter => Assert.True(parameter.IsOptional));
    }

    private static string FormatViolation(string file, XObject node)
    {
        var lineInfo = (IXmlLineInfo)node;
        return $"{RelativePath(file)}:{lineInfo.LineNumber}";
    }

    private static string RelativePath(string path)
    {
        return Path.GetRelativePath(RepositoryRoot, path).Replace('\\', '/');
    }

    private static string NormalizePath(string path)
    {
        return path.Replace('\\', '/');
    }

    private static string NormalizePlatformPath(string path)
    {
        return path.Replace('/', Path.DirectorySeparatorChar);
    }

    private static bool IsUnderDirectory(string file, string directory)
    {
        var relative = Path.GetRelativePath(directory, file);
        return relative != ".."
            && !relative.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
            && !Path.IsPathRooted(relative);
    }

    private static string FindRepositoryRoot()
    {
        for (
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            directory is not null;
            directory = directory.Parent
        )
        {
            if (
                File.Exists(Path.Combine(directory.FullName, "Flourish.slnx"))
                && Directory.Exists(Path.Combine(directory.FullName, "src", "Flourish"))
                && Directory.Exists(Path.Combine(directory.FullName, "src", "Gallery"))
            )
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException(
            $"Could not locate the Flourish repository above {AppContext.BaseDirectory}."
        );
    }
}
