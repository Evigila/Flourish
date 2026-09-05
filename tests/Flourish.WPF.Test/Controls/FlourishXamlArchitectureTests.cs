using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Controls;
using ArkheideSystem.Flourish.WPF.Test.Infrastructure;
using Xunit;
using FlourishButton = ArkheideSystem.Flourish.Controls.Button;

namespace ArkheideSystem.Flourish.WPF.Test.Controls;

public sealed class FlourishXamlArchitectureTests
{
    private const string PresentationNamespace =
        "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
    private const string FlourishNamespace = "http://schemas.arkheide.system/flourish";
    private const string XamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";
    private static readonly string RepositoryRoot = TestPaths.RepositoryRoot;
    private static readonly string FlourishRoot = Path.Combine(
        RepositoryRoot,
        "src",
        "Flourish.WPF"
    );
    private static readonly string GalleryRoot = Path.Combine(RepositoryRoot, "src", "Gallery.WPF");
    private static readonly string[] CanonicalFontSizeResourceNames =
    [
        "FlourishFontSizeSmall",
        "FlourishFontSizeStandard",
        "FlourishFontSizeStandardIcon",
        "FlourishFontSizeLarge",
        "FlourishFontSizeExtraLarge",
        "FlourishFontSizeLargeIcon",
        "FlourishFontSizeHeaderSize",
    ];

    private static readonly string[] CanonicalIconFontSizeResourceNames =
    [
        "FlourishFontSizeStandardIcon",
        "FlourishFontSizeLargeIcon",
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
            ["FlourishIconFontSizeToolbar"] = 13d,
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
    public void ProjectFolders_FollowFeatureModuleThemeAndViewBoundaries()
    {
        string[] requiredDirectories =
        [
            "Abstract",
            "Appearance",
            "Assets",
            "BackgroundTasks",
            "Commands",
            "Configuration",
            "Controls",
            "Hosting",
            "Layout",
            "Localization",
            "Messaging",
            "Motion",
            "Navigation",
            "Profile",
            "Projects",
            "Shell",
            "Shell/Regions",
            "Shell/StatusBar",
            "Shell/TitleBar",
            "Shell/Toolbar",
            "Themes",
            "Themes/Colors",
            "ToolTips",
            "Views",
            "Views/Page",
            "Views/Windows",
            "Windowing",
        ];
        string[] retiredDirectories =
        [
            "Composition",
            "Abstract/Builder",
            "Abstract/Essential",
            "Abstract/Runtime",
            "Internal",
            "Services",
            "Styles",
            "Windows",
            "Controls/Behaviors",
            "Controls/Styles",
        ];

        Assert.All(
            requiredDirectories,
            path =>
                Assert.True(
                    Directory.Exists(Path.Combine(FlourishRoot, NormalizePlatformPath(path))),
                    $"Required directory src/Flourish.WPF/{path} is missing."
                )
        );
        Assert.All(
            retiredDirectories,
            path =>
            {
                var retiredPath = Path.Combine(FlourishRoot, NormalizePlatformPath(path));
                Assert.False(
                    Directory.Exists(retiredPath)
                        && Directory
                            .EnumerateFiles(retiredPath, "*", SearchOption.AllDirectories)
                            .Any(),
                    $"Retired directory src/Flourish.WPF/{path} must contain no product files."
                );
            }
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
            "/Flourish.WPF;component/Themes/Layout.xaml",
            "/Flourish.WPF;component/Themes/Typography.xaml",
            "/Flourish.WPF;component/Themes/Colors/Colors.xaml",
            "/Flourish.WPF;component/Themes/Controls.xaml",
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
        Assert.Equal(new[] { "Colors.Dark.xaml", "Colors.Light.xaml", "Colors.xaml" }, colorFiles);
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
            "ActionCard.xaml",
            "ScrollViewer.xaml",
            "WindowCaptionButton.xaml",
            "ListBox.xaml",
            "ListBoxItem.xaml",
            "BunchedListBoxItem.xaml",
        };
        var expectedSources = Directory
            .EnumerateFiles(controlsRoot, "*.xaml", SearchOption.TopDirectoryOnly)
            .Where(path => !familyDependencyFiles.Contains(Path.GetFileName(path)))
            .Select(path => $"/Flourish.WPF;component/Controls/{Path.GetFileName(path)}")
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(expectedSources, actualSources.Order(StringComparer.Ordinal));
        Assert.Equal(actualSources.Length, actualSources.Distinct(StringComparer.Ordinal).Count());
        Assert.All(
            actualSources,
            source => Assert.StartsWith("/Flourish.WPF;component/Controls/", source)
        );

        Assert.Equal(
            ["ActionCard.xaml"],
            GetMergedDictionarySources(LoadXaml(Path.Combine(controlsRoot, "OutputCard.xaml")))
        );
        Assert.Equal(
            ["Card.xaml"],
            GetMergedDictionarySources(LoadXaml(Path.Combine(controlsRoot, "ActionCard.xaml")))
        );
        Assert.Equal(
            ["WindowCaptionButton.xaml"],
            GetMergedDictionarySources(LoadXaml(Path.Combine(controlsRoot, "CardButton.xaml")))
        );
        Assert.Equal(
            ["Button.xaml"],
            GetMergedDictionarySources(
                LoadXaml(Path.Combine(controlsRoot, "WindowCaptionButton.xaml"))
            )
        );
        Assert.Equal(
            ["ListBox.xaml", "BunchedListBoxItem.xaml"],
            GetMergedDictionarySources(LoadXaml(Path.Combine(controlsRoot, "BunchedListBox.xaml")))
        );
        Assert.Equal(
            ["ListBoxItem.xaml"],
            GetMergedDictionarySources(
                LoadXaml(Path.Combine(controlsRoot, "BunchedListBoxItem.xaml"))
            )
        );
        Assert.Empty(GetMergedDictionarySources(LoadXaml(Path.Combine(controlsRoot, "Card.xaml"))));
        Assert.Empty(
            GetMergedDictionarySources(LoadXaml(Path.Combine(controlsRoot, "Button.xaml")))
        );
    }

    [Fact]
    public void DisabledControlSurfaces_UseTheSharedNeutralPalette()
    {
        var controlsRoot = Path.Combine(FlourishRoot, "Controls");
        string[] surfaceControlFiles =
        [
            "ActionCard.xaml",
            "Button.xaml",
            "Card.xaml",
            "CardButton.xaml",
            "CheckBox.xaml",
            "ComboBox.xaml",
            "ComboBoxItem.xaml",
            "DataGrid.xaml",
            "ListBox.xaml",
            "ListBoxItem.xaml",
            "OutputCard.xaml",
            "PasswordBox.xaml",
            "Presenter.xaml",
            "RadioButton.xaml",
            "SearchBox.xaml",
            "TextBox.xaml",
            "WindowCaptionButton.xaml",
        ];
        string[] expectedResources =
        [
            "FlourishNeutralBackgroundDisabledBrush",
            "FlourishNeutralStrokeDisabledBrush",
            "FlourishNeutralForegroundDisabledBrush",
        ];

        foreach (var fileName in surfaceControlFiles)
        {
            var document = LoadXaml(Path.Combine(controlsRoot, fileName));
            var disabledStates = document.Descendants().Where(IsDisabledState).ToArray();

            Assert.NotEmpty(disabledStates);
            Assert.Contains(
                disabledStates,
                state =>
                {
                    var values = state
                        .Descendants()
                        .Where(element => element.Name.LocalName == "Setter")
                        .Select(setter => setter.Attribute("Value")?.Value ?? string.Empty)
                        .ToArray();

                    return expectedResources.All(resource =>
                        values.Any(value => value.Contains(resource, StringComparison.Ordinal))
                    );
                }
            );
        }
    }

    [Fact]
    public void DisabledControlStates_DoNotRestoreVariantColorsOrUseOpacity()
    {
        var controlsRoot = Path.Combine(FlourishRoot, "Controls");
        var violations = Directory
            .EnumerateFiles(controlsRoot, "*.xaml", SearchOption.TopDirectoryOnly)
            .SelectMany(file =>
                LoadXaml(file)
                    .Descendants()
                    .Where(IsDisabledState)
                    .SelectMany(state =>
                        state
                            .Descendants()
                            .Where(element => element.Name.LocalName == "Setter")
                            .Where(setter =>
                                setter.Attribute("Property")?.Value == "Opacity"
                                || setter.Attribute("Value")?.Value == "Transparent"
                            )
                            .Select(setter => $"{Path.GetFileName(file)}: {setter}")
                    )
            )
            .ToArray();

        Assert.Empty(violations);
    }

    [Fact]
    public void EveryPublicVisualControl_HasAMatchingDictionaryCodePairAndProjectNesting()
    {
        var controlsRoot = Path.Combine(FlourishRoot, "Controls");
        var project = LoadXaml(Path.Combine(FlourishRoot, "Flourish.WPF.csproj"));
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
            else if (
                !File.ReadAllText(codePath).Contains($"class {type.Name}", StringComparison.Ordinal)
            )
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
                    .Where(element => element.Name.LocalName is "Style" or "ControlTemplate")
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
        var galleryRoot = Path.Combine(RepositoryRoot, "src", "Gallery.WPF");
        var violations = new List<string>();

        foreach (var file in EnumerateXamlFiles(galleryRoot))
        {
            var document = LoadXaml(file);
            foreach (
                var element in document
                    .Descendants()
                    .Where(element => element.Name.LocalName is "Style" or "ControlTemplate")
            )
            {
                violations.Add(FormatViolation(file, element));
            }

            foreach (
                var styleAttribute in document
                    .Root!.DescendantsAndSelf()
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
    public void EveryGalleryPage_UsesPageBodyAsItsRootContent()
    {
        var viewsRoot = Path.Combine(RepositoryRoot, "src", "Gallery.WPF", "Views");
        var violations = new List<string>();

        foreach (
            var path in Directory.EnumerateFiles(viewsRoot, "*.xaml", SearchOption.AllDirectories)
        )
        {
            var document = LoadXaml(path);
            if (document.Root?.Name.LocalName != "Page")
            {
                continue;
            }

            var rootContent = document
                .Root.Elements()
                .Where(element => !IsPropertyElement(element))
                .ToArray();
            if (rootContent.Length != 1 || rootContent[0].Name.LocalName != nameof(PageBody))
            {
                violations.Add(
                    $"{RelativePath(path)} must contain exactly one root {nameof(PageBody)}"
                );
            }
        }

        AssertNoArchitectureViolations(
            violations,
            "Gallery pages must use PageBody for the canonical scrolling page layout."
        );
    }

    [Fact]
    public void EveryGalleryPage_UsesOneLeadingHeaderFollowedOnlyByChunks()
    {
        var viewsRoot = Path.Combine(RepositoryRoot, "src", "Gallery.WPF", "Views");
        var violations = new List<string>();

        foreach (
            var path in Directory.EnumerateFiles(viewsRoot, "*.xaml", SearchOption.AllDirectories)
        )
        {
            var document = LoadXaml(path);
            if (document.Root?.Name.LocalName != "Page")
            {
                continue;
            }

            var heroes = document
                .Descendants()
                .Where(element => element.Name.LocalName == nameof(HeaderChunk))
                .Where(element => !IsInsidePopup(element))
                .ToArray();

            if (heroes.Length != 1)
            {
                violations.Add(
                    $"{RelativePath(path)} declares {heroes.Length} main-content HeaderChunk elements"
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

            var visibleSections = flow.Elements()
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
                        $"{FormatViolation(path, element)} follows HeaderChunk outside a Chunk"
                    );
                }
            }
        }

        AssertNoArchitectureViolations(
            violations,
            "Every Gallery page must have exactly one leading HeaderChunk and place each subsequent visible main section in a full-width Chunk; Popup infrastructure is excluded."
        );
    }

    [Fact]
    public void ProfilePage_IsAnInternalShellFlyoutRatherThanAMainContentPage()
    {
        var path = Path.Combine(FlourishRoot, "Views", "Page", "ProfilePage.xaml");
        var document = LoadXaml(path);
        var root = Assert.IsType<XElement>(document.Root);
        var xamlNamespace = XNamespace.Get("http://schemas.microsoft.com/winfx/2006/xaml");

        Assert.Equal("Page", root.Name.LocalName);
        Assert.Equal("internal", (string?)root.Attribute(xamlNamespace + "ClassModifier"));
        Assert.DoesNotContain(
            root.Descendants(),
            element => element.Name.LocalName is nameof(HeaderChunk) or nameof(Chunk)
        );
    }

    [Fact]
    public void GalleryChunks_DeclareRequiredTitleAndBody()
    {
        var viewsRoot = Path.Combine(RepositoryRoot, "src", "Gallery.WPF", "Views");
        var violations = new List<string>();

        foreach (
            var path in Directory.EnumerateFiles(viewsRoot, "*.xaml", SearchOption.AllDirectories)
        )
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
        var viewsRoot = Path.Combine(RepositoryRoot, "src", "Gallery.WPF", "Views");
        var violations = new List<string>();
        string[] requiredProperties =
        [
            nameof(Presenter.Title),
            nameof(Presenter.Content),
            nameof(Presenter.PresenterMode),
            nameof(Presenter.PresenterPosition),
        ];

        foreach (
            var path in Directory.EnumerateFiles(viewsRoot, "*.xaml", SearchOption.AllDirectories)
        )
        {
            var document = LoadXaml(path);
            foreach (
                var presenter in document
                    .Descendants()
                    .Where(element =>
                        element.Name.LocalName is nameof(Presenter) or nameof(HeaderChunk)
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
                    !presenter
                        .Elements()
                        .Any(element =>
                            element.Name.LocalName
                            == $"{nameof(Presenter)}.{nameof(Presenter.Presentation)}"
                        )
                )
                {
                    violations.Add(
                        $"{FormatViolation(path, presenter)} has no explicit Presentation"
                    );
                }

                foreach (
                    var directContent in presenter
                        .Elements()
                        .Where(element => !IsPropertyElement(element))
                )
                {
                    violations.Add(
                        $"{FormatViolation(path, directContent)} is implicit Presenter content"
                    );
                }
            }
        }

        AssertNoArchitectureViolations(
            violations,
            "Gallery Presenter and HeaderChunk declarations must explicitly set Title, Content, PresenterMode, and PresenterPosition; ordinary Presenter visuals must use Presenter.Presentation."
        );
    }

    [Fact]
    public void PresenterTemplate_DefinesSplitColumnsAndTopDownRows()
    {
        var document = LoadXaml(Path.Combine(FlourishRoot, "Controls", "Presenter.xaml"));
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
                && element.Elements().Any(child => child.Name.LocalName == "Grid.ColumnDefinitions")
            );
        var columns = layoutGrid
            .Elements()
            .Single(element => element.Name.LocalName == "Grid.ColumnDefinitions")
            .Elements()
            .ToArray();
        var rows = layoutGrid
            .Elements()
            .Single(element => element.Name.LocalName == "Grid.RowDefinitions")
            .Elements()
            .ToArray();

        Assert.Equal(2, columns.Length);
        Assert.All(columns, column => Assert.Equal("*", (string?)column.Attribute("Width")));
        Assert.Equal(2, rows.Length);
        Assert.All(rows, row => Assert.Equal("*", (string?)row.Attribute("Height")));

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
                (string?)element.Attribute(XNamespace.Get(XamlNamespace) + "Name") == "CopySurface"
            );

        var presenterSurface = template
            .Descendants()
            .Single(element =>
                (string?)element.Attribute(XNamespace.Get(XamlNamespace) + "Name")
                == "PresenterSurface"
            );
        Assert.Equal(
            "PART_ClipHost",
            (string?)layoutGrid.Attribute(XNamespace.Get(XamlNamespace) + "Name")
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
        Assert.Equal("PresenterPresentationHost", presentationHost.Name.LocalName);
        Assert.Equal("Stretch", (string?)presentationHost.Attribute("HorizontalAlignment"));
        Assert.Equal("Stretch", (string?)presentationHost.Attribute("VerticalAlignment"));
        Assert.Equal(
            "{TemplateBinding Presentation}",
            (string?)presentationHost.Attribute("Content")
        );

        var bodyHost = copySurface
            .Descendants()
            .Single(element =>
                (string?)element.Attribute(XNamespace.Get(XamlNamespace) + "Name") == "BodyHost"
            );
        Assert.Equal("Left", (string?)bodyHost.Attribute("HorizontalAlignment"));
        Assert.Equal("Center", (string?)bodyHost.Attribute("VerticalAlignment"));

        foreach (var hostName in new[] { "TitleHost", "ContentHost" })
        {
            var textHost = copySurface
                .Descendants()
                .Single(element =>
                    (string?)element.Attribute(XNamespace.Get(XamlNamespace) + "Name") == hostName
                );
            Assert.Equal("Left", (string?)textHost.Attribute("TextAlignment"));
        }

        var topDownSetters = template
            .Descendants()
            .Single(element =>
                element.Name.LocalName == "Trigger"
                && (string?)element.Attribute("Property") == nameof(Presenter.PresenterMode)
                && (string?)element.Attribute("Value") == nameof(PresenterMode.TopDown)
            )
            .Elements()
            .Where(element => element.Name.LocalName == "Setter")
            .ToDictionary(
                element =>
                    $"{(string?)element.Attribute("TargetName")}.{(string?)element.Attribute("Property")}",
                element => (string?)element.Attribute("Value")
            );
        Assert.Equal("0", topDownSetters["PresentationSurface.Grid.Column"]);
        Assert.Equal("2", topDownSetters["PresentationSurface.Grid.ColumnSpan"]);
        Assert.Equal("0", topDownSetters["PresentationSurface.Grid.Row"]);
        Assert.Equal("1", topDownSetters["PresentationSurface.Grid.RowSpan"]);
        Assert.Equal("0", topDownSetters["CopySurface.Grid.Column"]);
        Assert.Equal("2", topDownSetters["CopySurface.Grid.ColumnSpan"]);
        Assert.Equal("1", topDownSetters["CopySurface.Grid.Row"]);
        Assert.Equal("1", topDownSetters["CopySurface.Grid.RowSpan"]);
    }

    [Fact]
    public void ControlsGalleryPages_FollowTheCanonicalLearningSequence()
    {
        string[] pages =
        [
            "ChunkPage.xaml",
            "HeaderChunkPage.xaml",
            "ButtonPage.xaml",
            "CardButtonPage.xaml",
            "WindowCaptionButtonPage.xaml",
            "CardPage.xaml",
            "ActionCardPage.xaml",
            "OutputCardPage.xaml",
            "PresenterPage.xaml",
            "PageBodyPage.xaml",
            "DocumentPage.xaml",
            "CodeSpacePage.xaml",
            "DataGridPage.xaml",
            "OverlayPage.xaml",
            "TextBlockPage.xaml",
            "ListBoxPage.xaml",
            "BunchedListBoxPage.xaml",
            "ScrollViewerPage.xaml",
            "ScrollBarPage.xaml",
            "GridSplitterPage.xaml",
            "ToolTipPage.xaml",
            "TextBoxPage.xaml",
            "PasswordBoxPage.xaml",
            "SearchBoxPage.xaml",
            "CheckBoxPage.xaml",
            "RadioButtonPage.xaml",
            "ComboBoxPage.xaml",
            "LabelPage.xaml",
        ];

        foreach (var fileName in pages)
        {
            var document = LoadXaml(Path.Combine(GalleryRoot, "Views", fileName));
            var chunks = document
                .Descendants()
                .Where(element => element.Name.LocalName == nameof(Chunk))
                .ToArray();
            var actualTitles = chunks
                .Select(element => (string?)element.Attribute(nameof(Chunk.Title)) ?? string.Empty)
                .ToArray();

            var variantIndex = Array.IndexOf(actualTitles, "Variant");
            var tableIndex = Array.IndexOf(actualTitles, "Table");
            var usageIndex = Array.IndexOf(actualTitles, "Usage");
            var referenceIndex = Array.IndexOf(actualTitles, "Reference");

            Assert.True(
                variantIndex is -1 or 0,
                $"{fileName}: Variant must be first when present."
            );
            Assert.Equal(variantIndex + 1, tableIndex);
            if (fileName == "ChunkPage.xaml")
            {
                Assert.Equal(tableIndex + 1, usageIndex);
            }
            else
            {
                Assert.True(
                    usageIndex > tableIndex + 1,
                    $"{fileName}: Table and Usage must be separated by topic-specific examples."
                );
            }
            Assert.Equal(actualTitles.Length - 2, usageIndex);
            Assert.Equal(actualTitles.Length - 1, referenceIndex);
            Assert.Equal(
                actualTitles.Length,
                actualTitles.Distinct(StringComparer.Ordinal).Count()
            );

            var table = chunks[tableIndex];
            var dataGrid = Assert.Single(
                table.Descendants(),
                element => element.Name.LocalName == "DataGrid"
            );
            Assert.Equal("False", (string?)dataGrid.Attribute("AutoGenerateColumns"));
            Assert.Equal("True", (string?)dataGrid.Attribute("IsReadOnly"));
            Assert.Equal(
                2,
                dataGrid
                    .Descendants()
                    .Count(element => element.Name.LocalName == "DataGridTextColumn")
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
            Assert.All(
                referenceButtons,
                button =>
                {
                    Assert.Equal("False", (string?)button.Attribute("IsEnabled"));
                    Assert.False(string.IsNullOrWhiteSpace((string?)button.Attribute("ToolTip")));
                }
            );
        }
    }

    [Fact]
    public void GalleryChunks_DoNotShareAHorizontalRow()
    {
        var viewsRoot = Path.Combine(RepositoryRoot, "src", "Gallery.WPF", "Views");
        var violations = new List<string>();

        foreach (
            var path in Directory.EnumerateFiles(viewsRoot, "*.xaml", SearchOption.AllDirectories)
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
                    parent.Name.LocalName == nameof(PageBody)
                    || parent.Name.LocalName == "StackPanel"
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
    public void GalleryActionCardGroups_StayPureAndUseTheFixedLayoutContract()
    {
        var viewsRoot = Path.Combine(RepositoryRoot, "src", "Gallery.WPF", "Views");
        var violations = new List<string>();

        foreach (
            var path in Directory.EnumerateFiles(viewsRoot, "*.xaml", SearchOption.AllDirectories)
        )
        {
            var document = LoadXaml(path);
            if (document.Root?.Name.LocalName != "Page")
            {
                continue;
            }

            foreach (
                var listCard in document
                    .Descendants()
                    .Where(element => element.Name.LocalName == nameof(ActionCard))
            )
            {
                string[] fixedProperties =
                [
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
                var parent in document
                    .Descendants()
                    .Where(element => element.Name.LocalName == "StackPanel")
            )
            {
                var directCards = parent
                    .Elements()
                    .Where(element =>
                        element.Name.LocalName
                            is nameof(Card)
                                or nameof(ActionCard)
                                or nameof(OutputCard)
                    )
                    .ToArray();
                if (
                    directCards.Any(element => element.Name.LocalName == nameof(ActionCard))
                    && directCards.Any(element => element.Name.LocalName != nameof(ActionCard))
                )
                {
                    violations.Add(
                        $"{FormatViolation(path, parent)} mixes ActionCard with another card type"
                    );
                }
            }
        }

        AssertNoArchitectureViolations(
            violations,
            "ActionCard groups must stay pure and use only the fixed Horizontal or Vertical layout contract."
        );
    }

    [Fact]
    public void GalleryActionCardActionBodies_ContainAtMostOneInteractiveControl()
    {
        var viewsRoot = Path.Combine(RepositoryRoot, "src", "Gallery.WPF", "Views");
        var interactiveControlNames = new HashSet<string>(StringComparer.Ordinal)
        {
            "Button",
            "TextBox",
            "PasswordBox",
            "SearchBox",
            "RadioButton",
            "ComboBox",
            "TextBox",
            "PasswordBox",
            "CheckBox",
            "RadioButton",
            "ComboBox",
        };
        var violations = new List<string>();

        foreach (
            var path in Directory.EnumerateFiles(viewsRoot, "*.xaml", SearchOption.AllDirectories)
        )
        {
            var document = LoadXaml(path);
            foreach (
                var body in document
                    .Descendants()
                    .Where(element => element.Name.LocalName == "ActionCard.Body")
            )
            {
                var interactiveControls = body.Descendants()
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
            "ActionCard.Body must contain at most one interactive control; split independent inputs and actions into separate rows."
        );
    }

    [Fact]
    public void GalleryActionCardPeers_UseTheCompactSpacingToken()
    {
        const string compactMargin = "{DynamicResource FlourishActionCardPeerMargin}";
        var viewsRoot = Path.Combine(RepositoryRoot, "src", "Gallery.WPF", "Views");
        var violations = new List<string>();

        foreach (
            var path in Directory.EnumerateFiles(viewsRoot, "*.xaml", SearchOption.AllDirectories)
        )
        {
            var document = LoadXaml(path);
            foreach (
                var listCard in document
                    .Descendants()
                    .Where(element => element.Name.LocalName == nameof(ActionCard))
            )
            {
                var previousPeer = listCard.ElementsBeforeSelf().LastOrDefault();
                var followsActionCard = previousPeer?.Name.LocalName == nameof(ActionCard);
                var margin = (string?)listCard.Attribute("Margin");

                if (followsActionCard && margin != compactMargin)
                {
                    violations.Add(
                        $"{FormatViolation(path, listCard)} follows another ActionCard without the compact peer margin"
                    );
                }
                else if (!followsActionCard && margin == compactMargin)
                {
                    violations.Add(
                        $"{FormatViolation(path, listCard)} adds peer spacing before the first ActionCard in its group"
                    );
                }
            }
        }

        AssertNoArchitectureViolations(
            violations,
            "Consecutive ActionCards must use the compact ActionCard-specific margin between rows only."
        );
    }

    [Fact]
    public void GalleryCodeSpacePeers_UseTheSharedSectionSpacingToken()
    {
        const string peerMargin = "{DynamicResource FlourishCodeSpacePeerMargin}";
        var viewsRoot = Path.Combine(RepositoryRoot, "src", "Gallery.WPF", "Views");
        var violations = new List<string>();

        foreach (
            var path in Directory.EnumerateFiles(viewsRoot, "*.xaml", SearchOption.AllDirectories)
        )
        {
            var document = LoadXaml(path);
            foreach (
                var codeSpace in document
                    .Descendants()
                    .Where(element => element.Name.LocalName == nameof(CodeSpace))
            )
            {
                var isStackPanelPeer =
                    codeSpace.Parent?.Name.LocalName == "StackPanel"
                    && codeSpace.ElementsBeforeSelf().Any();
                var margin = (string?)codeSpace.Attribute("Margin");

                if (isStackPanelPeer && margin != peerMargin)
                {
                    violations.Add(
                        $"{FormatViolation(path, codeSpace)} follows another section element without the shared CodeSpace peer margin"
                    );
                }
                else if (!isStackPanelPeer && margin == peerMargin)
                {
                    violations.Add(
                        $"{FormatViolation(path, codeSpace)} applies section peer spacing outside a StackPanel peer position"
                    );
                }
            }
        }

        AssertNoArchitectureViolations(
            violations,
            "Gallery CodeSpace sections must use the shared peer margin only when following another element in a StackPanel."
        );
    }

    [Fact]
    public void GalleryActionCards_DoNotAddApplyRows()
    {
        var viewsRoot = Path.Combine(RepositoryRoot, "src", "Gallery.WPF", "Views");
        var violations = new List<string>();

        foreach (
            var path in Directory.EnumerateFiles(viewsRoot, "*.xaml", SearchOption.AllDirectories)
        )
        {
            var document = LoadXaml(path);
            foreach (
                var listCard in document
                    .Descendants()
                    .Where(element => element.Name.LocalName == nameof(ActionCard))
            )
            {
                var title = (string?)listCard.Attribute(nameof(ActionCard.Title)) ?? string.Empty;
                var hasApplyHandler = listCard
                    .Descendants()
                    .Where(element => element.Name.LocalName == "Button")
                    .Select(element => (string?)element.Attribute("Click"))
                    .Any(handler =>
                        handler?.StartsWith("Apply", StringComparison.OrdinalIgnoreCase) == true
                    );
                if (
                    title.StartsWith("Apply", StringComparison.OrdinalIgnoreCase) || hasApplyHandler
                )
                {
                    violations.Add(FormatViolation(path, listCard));
                }
            }
        }

        AssertNoArchitectureViolations(
            violations,
            "ActionCard settings must apply as their value is committed; do not add a separate Apply row."
        );
    }

    [Fact]
    public void GalleryComboBoxes_UseSelectionOnlyInteraction()
    {
        var viewsRoot = Path.Combine(RepositoryRoot, "src", "Gallery.WPF", "Views");
        var violations = new List<string>();

        foreach (
            var path in Directory.EnumerateFiles(viewsRoot, "*.xaml", SearchOption.AllDirectories)
        )
        {
            var document = LoadXaml(path);
            foreach (
                var comboBox in document
                    .Descendants()
                    .Where(element => element.Name.LocalName is "ComboBox" or "ComboBox")
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
        var viewsRoot = Path.Combine(RepositoryRoot, "src", "Gallery.WPF", "Views");
        var violations = new List<string>();
        var outputCardCount = 0;

        foreach (
            var path in Directory.EnumerateFiles(viewsRoot, "*.xaml", SearchOption.AllDirectories)
        )
        {
            var document = LoadXaml(path);
            outputCardCount += document
                .Descendants()
                .Count(element => element.Name.LocalName == nameof(OutputCard));
            foreach (
                var card in document
                    .Descendants()
                    .Where(element => element.Name.LocalName == nameof(Card))
            )
            {
                var title = (string?)card.Attribute(nameof(Card.Title)) ?? string.Empty;
                var hasOutputSemantics =
                    title.Contains("Output", StringComparison.OrdinalIgnoreCase)
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
            "Gallery output and result histories must use OutputCard instead of a titled Card."
        );
    }

    [Fact]
    public void GalleryTwoColumnSingleActionCard_IsNotWrappedInAStackPanel()
    {
        var viewsRoot = Path.Combine(RepositoryRoot, "src", "Gallery.WPF", "Views");
        var violations = new List<string>();

        foreach (
            var path in Directory.EnumerateFiles(viewsRoot, "*.xaml", SearchOption.AllDirectories)
        )
        {
            var document = LoadXaml(path);
            foreach (
                var uniformGrid in document
                    .Descendants()
                    .Where(element =>
                        element.Name.LocalName == "UniformGrid"
                        && (string?)element.Attribute("Columns") == "2"
                    )
            )
            {
                foreach (
                    var stack in uniformGrid
                        .Elements()
                        .Where(element => element.Name.LocalName == "StackPanel")
                )
                {
                    var listCards = stack
                        .Elements()
                        .Where(element => element.Name.LocalName == nameof(ActionCard))
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
            "A single ActionCard must be a direct stretched column peer so its visible surface matches the OutputCard height."
        );
    }

    [Fact]
    public void GalleryCards_DoNotDeclareRetiredBodySlots()
    {
        var viewsRoot = Path.Combine(RepositoryRoot, "src", "Gallery.WPF", "Views");
        var violations = new List<string>();

        foreach (
            var path in Directory.EnumerateFiles(viewsRoot, "*.xaml", SearchOption.AllDirectories)
        )
        {
            var document = LoadXaml(path);
            if (document.Root?.Name.LocalName != "Page")
            {
                continue;
            }

            var retiredBodies = document
                .Descendants()
                .Where(element => element.Name.LocalName == "Card.Body");
            violations.AddRange(retiredBodies.Select(element => FormatViolation(path, element)));

            violations.AddRange(
                document
                    .Descendants()
                    .Where(element =>
                        element.Name.LocalName == nameof(Card)
                        && element.Attribute("Body") is not null
                    )
                    .Select(element => FormatViolation(path, element))
            );
        }

        AssertNoArchitectureViolations(
            violations,
            "Card does not expose Body; use Content or Icon. Use ActionCard when one interactive Body is required."
        );
    }

    [Fact]
    public void GalleryActionCards_KeepDynamicOutputInAPeerCard()
    {
        var viewsRoot = Path.Combine(RepositoryRoot, "src", "Gallery.WPF", "Views");
        var violations = new List<string>();

        foreach (
            var path in Directory.EnumerateFiles(viewsRoot, "*.xaml", SearchOption.AllDirectories)
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
                    .Where(element => element.Name.LocalName == "ActionCard.Body")
            )
            {
                var hasLocalAction = body.Descendants()
                    .Any(element =>
                        element.Name.LocalName is nameof(Button) or nameof(WindowCaptionButton)
                    );
                if (!hasLocalAction)
                {
                    continue;
                }

                var namedStatus = body.Descendants()
                    .Where(element => element.Name.LocalName == "TextBlock")
                    .Where(element => (string?)element.Attribute("Role") == "Status")
                    .Where(element =>
                        element.Attribute(XName.Get("Name", XamlNamespace)) is not null
                    );
                violations.AddRange(namedStatus.Select(element => FormatViolation(path, element)));
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
                nameof(Card.Content),
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
        var typography = LoadXaml(Path.Combine(FlourishRoot, "Themes", "Typography.xaml"));
        var typographySizeResources = typography
            .Descendants()
            .Select(element => new
            {
                Element = element,
                Name = (string?)element.Attribute(XName.Get("Key", XamlNamespace)),
            })
            .Where(resource =>
                resource.Name is not null
                && CanonicalFontSizeResourceNames.Contains(resource.Name, StringComparer.Ordinal)
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
        Assert.Equal(11d, actualSizes["FlourishFontSizeSmall"]);
        Assert.Equal(13d, actualSizes["FlourishFontSizeStandard"]);
        Assert.Equal(14d, actualSizes["FlourishFontSizeStandardIcon"]);
        Assert.Equal(14d, actualSizes["FlourishFontSizeLarge"]);
        Assert.Equal(18d, actualSizes["FlourishFontSizeExtraLarge"]);
        Assert.Equal(22d, actualSizes["FlourishFontSizeLargeIcon"]);
        Assert.Equal(25d, actualSizes["FlourishFontSizeHeaderSize"]);

        var allowedNames = CanonicalFontSizeResourceNames.ToHashSet(StringComparer.Ordinal);
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
                            violations.Add($"{RelativePath(file)}:{lineNumber} ({match.Value})");
                        }
                    }
                }
            }
        }

        AssertNoArchitectureViolations(
            violations,
            "Product source may only reference the canonical text and icon font-size resources."
        );

        var contextualIconSizes = typographySizeResources
            .First()
            .Element.Parent!.Elements()
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
        var typography = LoadXaml(Path.Combine(FlourishRoot, "Themes", "Typography.xaml"));
        var keyName = XName.Get("Key", XamlNamespace);
        var resources = typography
            .Descendants()
            .Where(element => element.Attribute(keyName) is not null)
            .ToDictionary(element => (string)element.Attribute(keyName)!, StringComparer.Ordinal);

        (string Tier, double FontSize, double LineHeight, Thickness BottomSpace)[] tiers =
        [
            ("Small", 11, 13, new Thickness(0, 0, 0, 1)),
            ("Standard", 13, 15, new Thickness(0, 0, 0, 1)),
            ("Large", 14, 18, new Thickness(0, 0, 0, 2)),
            ("ExtraLarge", 18, 23, new Thickness(0, 0, 0, 3)),
            ("HeaderSize", 25, 30, new Thickness(0, 0, 0, 4)),
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
                ParseThickness(resources[$"FlourishTypographyBottomSpace{tier}"].Value.Trim())
            );
            Assert.True(lineHeight >= fontSize);
        }

        foreach (var (tier, size) in new[] { ("StandardIcon", 14d), ("LargeIcon", 22d) })
        {
            Assert.Equal(
                size,
                XmlConvert.ToDouble(resources[$"FlourishFontSize{tier}"].Value.Trim())
            );
            Assert.Equal(
                size,
                XmlConvert.ToDouble(resources[$"FlourishLineHeight{tier}"].Value.Trim())
            );
            Assert.Equal(
                new Thickness(),
                ParseThickness(resources[$"FlourishTypographyBottomSpace{tier}"].Value.Trim())
            );
        }
        Assert.DoesNotContain("FlourishFontSizeIcon", resources.Keys);
        Assert.DoesNotContain("FlourishLineHeightIcon", resources.Keys);
        Assert.DoesNotContain("FlourishTypographyBottomSpaceIcon", resources.Keys);
    }

    [Fact]
    public void ControlRowTextHosts_UseSharedLineBoxesWithoutLocalVerticalOffsets()
    {
        var xName = XNamespace.Get(XamlNamespace) + "Name";
        var controlHosts = new (string File, string Name)[]
        {
            ("Button.xaml", "ContentHost"),
            ("ListBoxItem.xaml", "ContentHost"),
            ("BunchedListBoxItem.xaml", "ContentHost"),
            ("ComboBoxItem.xaml", "ContentHost"),
            ("ComboBox.xaml", "SelectionContentSite"),
            ("CheckBox.xaml", "ContentHost"),
            ("RadioButton.xaml", "ContentHost"),
            ("Label.xaml", "ContentHost"),
        };

        foreach (var (file, name) in controlHosts)
        {
            var document = LoadXaml(Path.Combine(FlourishRoot, "Controls", file));
            var host = document
                .Descendants()
                .Single(element => (string?)element.Attribute(xName) == name);

            Assert.Equal(
                "{DynamicResource FlourishControlContentPresenterStyle}",
                (string?)host.Attribute("Style")
            );
            Assert.Null(host.Attribute("Padding"));
            Assert.Null(host.Attribute("RenderTransform"));
            Assert.DoesNotContain(
                host.Elements(),
                element =>
                    element.Name.LocalName.EndsWith("RenderTransform", StringComparison.Ordinal)
            );
        }

        var navigation = LoadXaml(
            Path.Combine(FlourishRoot, "Views", "Windows", "NavigationPaneView.xaml")
        );
        var navigationLabel = navigation
            .Descendants()
            .Single(element => (string?)element.Attribute(xName) == "NavigationItemLabel");
        Assert.Equal(
            "{DynamicResource FlourishControlTextBlockStyle}",
            (string?)navigationLabel.Attribute("Style")
        );
        Assert.Null(navigationLabel.Attribute("Padding"));

        var searchBox = LoadXaml(Path.Combine(FlourishRoot, "Controls", "SearchBox.xaml"));
        foreach (var name in new[] { "SearchIcon", "PlaceholderText" })
        {
            var text = searchBox
                .Descendants()
                .Single(element => (string?)element.Attribute(xName) == name);
            Assert.Equal("BlockLineHeight", (string?)text.Attribute("LineStackingStrategy"));
        }
    }

    [Fact]
    public void CodeSpace_CanCollapseOwnsOnlyTheExpandedCollapseAffordance()
    {
        var document = LoadXaml(Path.Combine(FlourishRoot, "Controls", "CodeSpace.xaml"));
        var collapseTrigger = Assert.Single(
            document.Descendants(),
            element =>
                element.Name.LocalName == "Trigger"
                && (string?)element.Attribute("Property") == "CanCollapse"
                && (string?)element.Attribute("Value") == "False"
        );
        Assert.Contains(
            collapseTrigger.Elements(),
            element =>
                element.Name.LocalName == "Setter"
                && (string?)element.Attribute("TargetName") == "PART_CollapseButton"
                && (string?)element.Attribute("Property") == "Visibility"
                && (string?)element.Attribute("Value") == "Collapsed"
        );

        var actions = document
            .Descendants()
            .Single(element =>
                (string?)element.Attribute(XName.Get("Name", XamlNamespace)) == "ExpandedActions"
            );
        Assert.Equal(
            new[] { "PART_CollapseButton", "PART_CopyButton" },
            actions
                .Elements()
                .Select(element => (string?)element.Attribute(XName.Get("Name", XamlNamespace)))
        );
        Assert.All(
            actions.Elements(),
            button =>
                Assert.Equal(
                    "{DynamicResource FlourishFontSizeStandardIcon}",
                    (string?)button.Attribute("IconSize")
                )
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
            typeof(bool),
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
            "usePersistedPreference",
        ];
        Type[] runtimeScaleTypes = explicitScaleTypes[..^1];
        string[] runtimeScaleNames = explicitScaleNames[..^1];
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
            typeof(IFontBuilder).GetMethods(),
            method => method.Name == nameof(IFontBuilder.SetFont)
        );
        AssertOptionalParameterContract(
            builderGlobalFont,
            explicitScaleTypes,
            explicitScaleNames,
            ["Microsoft Yahei", 11d, 13d, 14d, 14d, 18d, 25d, true]
        );

        var serviceSetFont = Assert.Single(
            typeof(IFontService).GetMethods(),
            method => method.Name == nameof(IFontService.SetFont)
        );
        AssertParameterContract(serviceSetFont, runtimeScaleTypes, runtimeScaleNames);

        var builderPageOverride = Assert.Single(
            typeof(IFontBuilder).GetMethods(),
            method => method.Name == nameof(IFontBuilder.SetOverrideFont)
        );
        Assert.True(builderPageOverride.IsGenericMethodDefinition);
        AssertTrailingOptionalParameterContract(
            builderPageOverride,
            nullableScaleTypes,
            runtimeScaleNames
        );

        var servicePageOverrides = typeof(IFontService)
            .GetMethods()
            .Where(method => method.Name == nameof(IFontService.SetOverrideFont))
            .ToArray();
        Assert.Equal(2, servicePageOverrides.Length);
        var genericServicePageOverride = Assert.Single(
            servicePageOverrides,
            method => method.IsGenericMethodDefinition
        );
        AssertParameterContract(genericServicePageOverride, nullableScaleTypes, runtimeScaleNames);
        var runtimeServicePageOverride = Assert.Single(
            servicePageOverrides,
            method => !method.IsGenericMethod
        );
        AssertParameterContract(
            runtimeServicePageOverride,
            [typeof(Type), .. nullableScaleTypes],
            ["pageType", .. runtimeScaleNames]
        );

        var pageOverrideConstructor = Assert.Single(typeof(PageFontOverride).GetConstructors());
        AssertParameterContract(pageOverrideConstructor, nullableScaleTypes, runtimeScaleNames);

        var fontAssemblyApiMethods = typeof(IFontService)
            .Assembly.GetTypes()
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
                    is nameof(IFontBuilder.SetFont)
                        or nameof(IFontBuilder.SetOverrideFont)
                        or nameof(IFontService.SetFont)
                        or nameof(IFontService.SetOverrideFont)
            )
            .ToArray();

        var setFontMethods = fontAssemblyApiMethods
            .Where(method => method.Name == nameof(IFontBuilder.SetFont))
            .ToArray();
        Assert.Equal(4, setFontMethods.Length);
        Assert.Equal(2, setFontMethods.Count(method => method.GetParameters().Length == 8));
        Assert.Equal(2, setFontMethods.Count(method => method.GetParameters().Length == 7));
        Assert.All(
            setFontMethods,
            method =>
            {
                if (method.GetParameters().Length == 8)
                {
                    AssertOptionalParameterContract(
                        method,
                        explicitScaleTypes,
                        explicitScaleNames,
                        ["Microsoft Yahei", 11d, 13d, 14d, 14d, 18d, 25d, true]
                    );
                    return;
                }

                AssertParameterContract(method, runtimeScaleTypes, runtimeScaleNames);
            }
        );
        var setOverrideFontMethods = fontAssemblyApiMethods
            .Where(method => method.Name == nameof(IFontBuilder.SetOverrideFont))
            .ToArray();
        Assert.Equal(5, setOverrideFontMethods.Length);
        Assert.All(
            setOverrideFontMethods,
            method =>
            {
                if (method.GetParameters().Skip(1).Any(parameter => parameter.IsOptional))
                {
                    AssertTrailingOptionalParameterContract(
                        method,
                        nullableScaleTypes,
                        runtimeScaleNames
                    );
                    return;
                }

                if (method.IsGenericMethodDefinition)
                {
                    AssertParameterContract(method, nullableScaleTypes, runtimeScaleNames);
                    return;
                }

                AssertParameterContract(
                    method,
                    [typeof(Type), .. nullableScaleTypes],
                    ["pageType", .. runtimeScaleNames]
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
                        violations.Add($"{RelativePath(file)}:{lineNumber} ({match.Value})");
                    }
                }
            }
        }

        AssertNoArchitectureViolations(
            violations,
            "The explicit text and icon font contract must not retain gap-based resources, mutators, or retired aliases."
        );
    }

    [Fact]
    public void ProductXaml_FontSizesUseCanonicalResourcesOrTemplateBindings()
    {
        var allowedValues = CanonicalFontSizeResourceNames
            .Concat(ContextualIconFontSizes.Keys)
            .Select(name => $"{{DynamicResource {name}}}")
            .Append("{TemplateBinding FontSize}")
            .Append("{TemplateBinding IconSize}")
            .ToHashSet(StringComparer.Ordinal);
        var violations = new List<string>();

        foreach (var root in new[] { FlourishRoot, GalleryRoot })
        {
            foreach (
                var file in EnumerateProductSourceFiles(root)
                    .Where(file => Path.GetExtension(file) == ".xaml")
            )
            {
                var document = LoadXaml(file);

                foreach (
                    var attribute in document
                        .Root!.DescendantsAndSelf()
                        .SelectMany(element => element.Attributes())
                        .Where(attribute => IsFontSizePropertyName(attribute.Name.LocalName))
                )
                {
                    if (!allowedValues.Contains(attribute.Value))
                    {
                        violations.Add($"{FormatViolation(file, attribute)} ({attribute.Value})");
                    }
                }

                foreach (
                    var setter in document
                        .Descendants()
                        .Where(element =>
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
                        violations.Add($"{FormatViolation(file, setter)} ({value})");
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
                        violations.Add($"{FormatViolation(file, propertyElement)} ({value})");
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
        var allowedIconSizes = ContextualIconFontSizes
            .Keys.Concat(CanonicalIconFontSizeResourceNames)
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
                        var familyAttribute in element
                            .Attributes()
                            .Where(attribute =>
                                attribute.Name.LocalName is "FontFamily" or "TextElement.FontFamily"
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
                            .SingleOrDefault(attribute => attribute.Name.LocalName == sizeProperty)
                            ?.Value;
                        if (sizeValue is null || !allowedIconSizes.Contains(sizeValue))
                        {
                            violations.Add(
                                $"{FormatViolation(file, element)} ({sizeProperty}={sizeValue ?? "<missing>"})"
                            );
                        }

                        if (
                            element.Name.NamespaceName == FlourishNamespace
                            && element.Name.LocalName == "TextBlock"
                            && (string?)element.Attribute("Role") != "Icon"
                        )
                        {
                            violations.Add(
                                $"{FormatViolation(file, element)} (TextBlock icon glyph is missing Role=Icon)"
                            );
                        }
                    }

                    if ((string?)element.Attribute("Role") == "Icon")
                    {
                        iconEntryCount++;
                        var explicitSize = (string?)element.Attribute("FontSize");
                        var explicitLineHeight = (string?)element.Attribute("LineHeight");
                        var explicitPadding = (string?)element.Attribute("Padding");
                        if (explicitSize is not null && !allowedIconSizes.Contains(explicitSize))
                        {
                            violations.Add(
                                $"{FormatViolation(file, element)} (FontSize={explicitSize})"
                            );
                        }
                        if (
                            explicitSize == "{DynamicResource FlourishFontSizeLargeIcon}"
                            && explicitLineHeight != "{DynamicResource FlourishLineHeightLargeIcon}"
                        )
                        {
                            violations.Add(
                                $"{FormatViolation(file, element)} (LineHeight={explicitLineHeight ?? "<missing>"})"
                            );
                        }
                        if (
                            explicitPadding is not null
                            && explicitPadding
                                != "{DynamicResource FlourishTypographyBottomSpaceStandardIcon}"
                            && explicitPadding
                                != "{DynamicResource FlourishTypographyBottomSpaceLargeIcon}"
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
            Path.Combine(FlourishRoot, "Views", "Windows", "ShellWindow.xaml.cs")
        );
        var iconBindingCalls = Regex.Matches(
            shellSource,
            "BindIconTypography\\(\\s*[^,\\r\\n]+\\s*,\\s*\"(?<key>[^\"]+)\"\\s*\\)",
            RegexOptions.CultureInvariant
        );
        Assert.NotEmpty(iconBindingCalls.Cast<Match>());
        var allowedBindingKeys = ContextualIconFontSizes
            .Keys.Concat(CanonicalIconFontSizeResourceNames)
            .ToHashSet(StringComparer.Ordinal);
        Assert.All(
            iconBindingCalls.Cast<Match>(),
            match => Assert.Contains(match.Groups["key"].Value, allowedBindingKeys)
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
        var iconSizes = ContextualIconFontSizes
            .Keys.Concat(CanonicalIconFontSizeResourceNames)
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
                        var sizeAttribute in element
                            .Attributes()
                            .Where(attribute =>
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
                            || element
                                .Attributes()
                                .Any(attribute =>
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
                            && trigger
                                .Elements()
                                .Any(sibling =>
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
                var file in EnumerateProductSourceFiles(root)
                    .Where(file => Path.GetExtension(file) == ".cs")
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
        var document = LoadXaml(Path.Combine(FlourishRoot, "Controls", "TextBlock.xaml"));
        var style = document
            .Descendants()
            .Single(element =>
                element.Name.LocalName == "Style"
                && (string?)element.Attribute("TargetType") == "{x:Type controls:TextBlock}"
                && element.Attribute(XName.Get("Key", XamlNamespace)) is null
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
        Assert.Equal("{DynamicResource FlourishTypographyBottomSpaceStandard}", baseBottomSpace);
        Assert.Equal("BlockLineHeight", SetterValue(style, "LineStackingStrategy"));

        var roleTriggers = style
            .Descendants()
            .Where(element =>
                element.Name.LocalName == "Trigger"
                && (string?)element.Attribute("Property") == "Role"
            )
            .ToDictionary(element => (string)element.Attribute("Value")!, StringComparer.Ordinal);

        foreach (var role in Enum.GetValues<TextRole>())
        {
            var expectedTier = role switch
            {
                TextRole.Caption or TextRole.Status => "Small",
                TextRole.Icon => "StandardIcon",
                TextRole.CardTitle => "Large",
                TextRole.SectionTitle => "ExtraLarge",
                TextRole.PageTitle => "HeaderSize",
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

            Assert.Equal($"{{DynamicResource FlourishFontSize{expectedTier}}}", actualSize);
            Assert.Equal($"{{DynamicResource FlourishLineHeight{expectedTier}}}", actualLineHeight);
            Assert.Equal(
                $"{{DynamicResource FlourishTypographyBottomSpace{expectedTier}}}",
                actualBottomSpace
            );
            Assert.Equal(
                expectedTier is "Large" or "ExtraLarge" or "HeaderSize" ? "Bold" : "Regular",
                actualWeight
            );
            Assert.Equal(
                role == TextRole.Icon
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
            ("ActionCard.xaml", "CardTitle"),
            ("Presenter.xaml", "CardTitle"),
            ("Chunk.xaml", "SectionTitle"),
            ("HeaderChunk.xaml", "PageTitle"),
        ];

        foreach (var (fileName, expectedRole) in expectations)
        {
            var document = LoadXaml(Path.Combine(FlourishRoot, "Controls", fileName));
            var titleHost = document
                .Descendants()
                .Single(element =>
                    (string?)element.Attribute(XName.Get("Name", XamlNamespace)) == "TitleHost"
                );

            Assert.Equal(expectedRole, (string?)titleHost.Attribute("Role"));
        }
    }

    [Fact]
    public void ShellApplicationLogoFallback_UsesTheStandardFontSize()
    {
        var shell = LoadXaml(
            Path.Combine(FlourishRoot, "Views", "Windows", "ApplicationInfoOverlay.xaml")
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
        var shell = LoadXaml(Path.Combine(FlourishRoot, "Views", "Windows", "ShellWindow.xaml"));
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
                    .Root!.DescendantsAndSelf()
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
                .Root!.DescendantsAndSelf()
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
                    violations.Add($"{FormatViolation(file, fontWeight)} ({fontWeight.Value})");
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
                        violations.Add($"{RelativePath(file)}:{lineNumber} ({forbiddenWeight})");
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
            ("Views/Windows/NavigationPaneView.xaml", "NavigationGroupHeader"),
            ("Views/Windows/TitleBar.xaml", "TitleComboBox"),
            ("Views/Windows/MessageBoxWindow.xaml", "CaptionText"),
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
            Path.Combine(RepositoryRoot, "src", "Gallery.WPF", "Views", "HomePage.xaml")
        );
        var demoCards = document
            .Descendants()
            .Where(element =>
                element.Name.LocalName == "CardButton" && element.Attribute("Tag") is not null
            )
            .ToArray();

        Assert.Equal(9, demoCards.Length);
        Assert.All(
            demoCards,
            card =>
                Assert.False(
                    string.IsNullOrWhiteSpace((string?)card.Attribute("AutomationProperties.Name"))
                )
        );
    }

    [Theory]
    [InlineData("Colors.Light.xaml")]
    [InlineData("Colors.Dark.xaml")]
    public void ColorPalettes_UseCanonicalResourceNames(string fileName)
    {
        var document = LoadXaml(Path.Combine(FlourishRoot, "Themes", "Colors", fileName));
        var keys = document
            .Descendants()
            .Select(element => (string?)element.Attribute(XName.Get("Key", XamlNamespace)))
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

    private static bool IsDisabledState(XElement element)
    {
        if (element.Name.LocalName == "Trigger")
        {
            return element.Attribute("Property")?.Value == "IsEnabled"
                && element.Attribute("Value")?.Value == "False";
        }

        return element.Name.LocalName is "MultiTrigger" or "MultiDataTrigger"
            && element
                .Descendants()
                .Any(condition =>
                    condition.Name.LocalName == "Condition"
                    && condition.Attribute("Property")?.Value == "IsEnabled"
                    && condition.Attribute("Value")?.Value == "False"
                );
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

        return chunk
            .Nodes()
            .Any(node =>
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
            message + Environment.NewLine + string.Join(Environment.NewLine, violations)
        );
    }

    private static XDocument LoadXaml(string file)
    {
        return GalleryLocalizationTestResolver.LoadXaml(file, LoadOptions.SetLineInfo);
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
            && IsFontSizePropertyName((string?)element.Attribute("Property") ?? string.Empty);
    }

    private static bool IsFontWeightSetter(XElement element)
    {
        return element.Name.LocalName == "Setter"
            && (string?)element.Attribute("Property") == "FontWeight";
    }

    private static Thickness ParseThickness(string value)
    {
        var values = value.Split(',').Select(part => XmlConvert.ToDouble(part.Trim())).ToArray();
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

    private static void AssertTrailingOptionalParameterContract(
        MethodBase method,
        IReadOnlyList<Type> expectedTypes,
        IReadOnlyList<string> expectedNames
    )
    {
        var parameters = method.GetParameters();
        Assert.Equal(expectedTypes.Count, parameters.Length);
        Assert.Equal(expectedTypes, parameters.Select(parameter => parameter.ParameterType));
        Assert.Equal(expectedNames, parameters.Select(parameter => parameter.Name));
        Assert.False(parameters[0].IsOptional);
        Assert.All(
            parameters.Skip(1),
            parameter =>
            {
                Assert.True(parameter.IsOptional);
                Assert.Null(parameter.DefaultValue);
            }
        );
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
}
