using System.IO;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
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
            "IconButton.xaml",
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
            "Native WPF controls must retain their native theme unless a Flourish control is used."
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
            ("Views/Windows/TitleBar.xaml", "TitleText"),
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

        Assert.Equal(8, demoCards.Length);
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
