using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace ArkheideSystem.Flourish.Test.Controls;

public sealed class GalleryControlPageStructureTests
{
    private static readonly XNamespace XamlNamespace =
        "http://schemas.microsoft.com/winfx/2006/xaml";
    private static readonly string RepositoryRoot = FindRepositoryRoot();
    private static readonly string ViewsRoot = Path.Combine(
        RepositoryRoot,
        "src",
        "Gallery",
        "Views"
    );

    [Fact]
    public void ChunkFamily_UsesDedicatedGalleryPagesAndNavigationRoutes()
    {
        string[] pages = ["ChunkPage.xaml", "HeaderChunkPage.xaml"];

        Assert.All(pages, fileName => Assert.True(File.Exists(Path.Combine(ViewsRoot, fileName))));

        var chunkPage = LoadPage("ChunkPage.xaml");
        var topicChunks = chunkPage
            .Descendants()
            .Where(element => element.Name.LocalName == "Chunk")
            .ToArray();
        Assert.DoesNotContain(
            topicChunks.SelectMany(element => element.Descendants()),
            element => element.Name.LocalName == "HeaderChunk"
        );

        var headerChunkPage = LoadPage("HeaderChunkPage.xaml");
        Assert.Single(
            headerChunkPage.Descendants(),
            element => element.Name.LocalName == "HeaderChunk"
        );

        var program = File.ReadAllText(Path.Combine(RepositoryRoot, "src", "Gallery", "Program.cs"));
        Assert.Contains("AddNavigable<HeaderChunkPage>(\"HeaderChunk\"", program);
        Assert.Contains("AddNavigableViewItem<HeaderChunkPage>(childId: 1)", program);
    }

    [Fact]
    public void ButtonFamily_UsesDedicatedGalleryPagesAndNavigationRoutes()
    {
        string[] pages =
        [
            "ButtonPage.xaml",
            "CardButtonPage.xaml",
            "WindowCaptionButtonPage.xaml",
        ];

        Assert.All(pages, fileName => Assert.True(File.Exists(Path.Combine(ViewsRoot, fileName))));

        var buttonPage = LoadPage("ButtonPage.xaml");
        var topicChunks = buttonPage
            .Descendants()
            .Where(element => element.Name.LocalName == "Chunk")
            .Where(element => (string?)element.Attribute("Title") != "Reference")
            .ToArray();

        Assert.DoesNotContain(
            topicChunks.SelectMany(element => element.Descendants()),
            element => element.Name.LocalName is "CardButton" or "WindowCaptionButton"
        );

        var program = File.ReadAllText(Path.Combine(RepositoryRoot, "src", "Gallery", "Program.cs"));
        Assert.Contains("AddNavigable<CardButtonPage>(\"CardButton\"", program);
        Assert.Contains(
            "AddNavigable<WindowCaptionButtonPage>(\"WindowCaptionButton\"",
            program
        );
        Assert.Contains("AddNavigableViewItem<CardButtonPage>(childId: 1)", program);
        Assert.Contains("AddNavigableViewItem<WindowCaptionButtonPage>(childId: 1)", program);
    }

    [Fact]
    public void CardFamily_UsesDedicatedGalleryPagesAndTopDownVariantPresenters()
    {
        string[] pages =
        [
            "CardPage.xaml",
            "ActionCardPage.xaml",
            "OutputCardPage.xaml",
        ];

        Assert.All(pages, fileName => Assert.True(File.Exists(Path.Combine(ViewsRoot, fileName))));

        var disallowedControlsByPage = new Dictionary<string, string[]>
        {
            ["CardPage.xaml"] = ["ActionCard", "OutputCard"],
            ["ActionCardPage.xaml"] = ["Card", "OutputCard"],
            ["OutputCardPage.xaml"] = ["Card"],
        };

        foreach (var item in disallowedControlsByPage)
        {
            var page = LoadPage(item.Key);
            var topicChunks = page
                .Descendants()
                .Where(element => element.Name.LocalName == "Chunk")
                .Where(element => (string?)element.Attribute("Title") != "Reference")
                .ToArray();
            var demonstratedCardTypes = topicChunks
                .SelectMany(element => element.Descendants())
                .Where(element =>
                    element.Name.LocalName is "Card" or "ActionCard" or "OutputCard"
                )
                .Select(element => element.Name.LocalName)
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            Assert.Contains(
                Path.GetFileNameWithoutExtension(item.Key).Replace("Page", string.Empty),
                demonstratedCardTypes
            );
            Assert.DoesNotContain(
                demonstratedCardTypes,
                controlName => item.Value.Contains(controlName, StringComparer.Ordinal)
            );
        }

        foreach (var fileName in new[] { "CardPage.xaml", "ActionCardPage.xaml" })
        {
            var page = LoadPage(fileName);
            var variantChunk = page
                .Descendants()
                .Single(element =>
                    element.Name.LocalName == "Chunk"
                    && (string?)element.Attribute("Title") == "Variant"
                );
            var presenters = variantChunk
                .Descendants()
                .Where(element => element.Name.LocalName == "Presenter")
                .ToArray();

            Assert.NotEmpty(presenters);
            Assert.All(
                presenters,
                presenter =>
                {
                    Assert.Equal("TopDown", (string?)presenter.Attribute("PresenterMode"));
                    Assert.DoesNotContain(
                        presenter.Elements(),
                        element => element.Name.LocalName == "Presenter.Body"
                    );
                }
            );
        }

        var program = File.ReadAllText(Path.Combine(RepositoryRoot, "src", "Gallery", "Program.cs"));
        Assert.Contains("AddNavigable<ActionCardPage>(\"ActionCard\"", program);
        Assert.Contains("AddNavigable<OutputCardPage>(\"OutputCard\"", program);
        Assert.Contains("AddNavigableViewItem<ActionCardPage>(childId: 1)", program);
        Assert.Contains("AddNavigableViewItem<OutputCardPage>(childId: 1)", program);
    }

    [Fact]
    public void FoundationalAndCollectionControls_HaveDedicatedGalleryRoutes()
    {
        string[] pageTypes =
        [
            "PageBody",
            "Paragraph",
            "TextBlock",
            "ListBox",
            "ListBoxItem",
            "ScrollViewer",
            "ScrollBar",
            "GridSplitter",
            "ToolTip",
        ];

        var program = File.ReadAllText(Path.Combine(RepositoryRoot, "src", "Gallery", "Program.cs"));

        foreach (var pageType in pageTypes)
        {
            var fileName = $"{pageType}Page.xaml";
            var page = LoadPage(fileName);

            Assert.Single(
                page.Descendants(),
                element => element.Name.LocalName == "HeaderChunk"
            );
            Assert.Contains(
                page.Descendants(),
                element =>
                    element.Name.LocalName == "Chunk"
                    && (string?)element.Attribute("Title") == "Usage"
                    && element.Descendants().Any(descendant =>
                        descendant.Name.LocalName == "CodeSpace"
                    )
            );
            Assert.Contains($"AddNavigable<{pageType}Page>(\"{pageType}\"", program);
            Assert.Contains($"AddNavigableViewItem<{pageType}Page>(childId: 1)", program);
        }

        var paragraphPage = LoadPage("ParagraphPage.xaml");
        Assert.Contains(
            paragraphPage.Descendants(),
            element => element.Name.LocalName == "Document"
        );
        var documentPage = LoadPage("DocumentPage.xaml");
        Assert.Contains(
            documentPage.Descendants(),
            element => element.Name.LocalName == "Paragraph"
        );
    }

    [Fact]
    public void ControlsOverview_UsesOneNavigationChunkAndMirrorsEveryChildRouteIcon()
    {
        var page = LoadPage("ControlLibraryPage.xaml");
        var pageBody = Assert.Single(
            page.Descendants(),
            element => element.Name.LocalName == "PageBody"
        );
        Assert.Collection(
            pageBody.Elements(),
            header => Assert.Equal("HeaderChunk", header.Name.LocalName),
            chunk =>
            {
                Assert.Equal("Chunk", chunk.Name.LocalName);
                Assert.Equal("Control library", (string?)chunk.Attribute("Title"));
            },
            reference =>
            {
                Assert.Equal("Chunk", reference.Name.LocalName);
                Assert.Equal("Reference", (string?)reference.Attribute("Title"));
            }
        );

        var chunk = Assert.Single(
            pageBody.Elements(),
            element =>
                element.Name.LocalName == "Chunk"
                && (string?)element.Attribute("Title") == "Control library"
        );
        var cards = chunk
            .Descendants()
            .Where(element => element.Name.LocalName == "CardButton")
            .ToArray();

        var program = File.ReadAllText(Path.Combine(RepositoryRoot, "src", "Gallery", "Program.cs"));
        var childPageTypes = Regex.Matches(
                program,
                @"AddNavigableViewItem<(?<page>\w+Page)>\(childId: 1\)"
            )
            .Select(match => match.Groups["page"].Value)
            .ToHashSet(StringComparer.Ordinal);
        var registeredRoutes = Regex.Matches(
                program,
                @"services\.AddNavigable<(?<page>\w+Page)>\(""(?<title>[^""]+)"", ""\\u(?<icon>[0-9A-Fa-f]{4})""\);"
            )
            .Where(match => childPageTypes.Contains(match.Groups["page"].Value))
            .ToDictionary(
                match => match.Groups["title"].Value,
                match => char.ConvertFromUtf32(
                    Convert.ToInt32(match.Groups["icon"].Value, 16)
                ),
                StringComparer.Ordinal
            );

        Assert.Equal(30, registeredRoutes.Count);
        Assert.Equal(registeredRoutes.Count, cards.Length);
        var publicControlNames = Directory
            .EnumerateFiles(
                Path.Combine(RepositoryRoot, "src", "Flourish", "Controls"),
                "*.xaml.cs",
                SearchOption.TopDirectoryOnly
            )
            .Select(path =>
                Regex.Match(
                    File.ReadAllText(path),
                    @"public\s+(?:sealed\s+)?class\s+(?<name>\w+)"
                )
            )
            .Where(match => match.Success)
            .Select(match => match.Groups["name"].Value)
            .Select(name =>
                name.StartsWith("Flourish", StringComparison.Ordinal)
                    ? name["Flourish".Length..]
                    : name
            )
            .Order(StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(registeredRoutes.Keys.Order(StringComparer.Ordinal), publicControlNames);
        Assert.Equal(
            registeredRoutes.Keys.Order(StringComparer.Ordinal),
            cards
                .Select(card => (string?)card.Attribute("Tag"))
                .OfType<string>()
                .Order(StringComparer.Ordinal)
        );

        Assert.All(cards, card =>
        {
            var route = Assert.IsType<string>((object?)card.Attribute("Tag")?.Value);
            Assert.Equal(route, (string?)card.Attribute("Title"));
            Assert.Equal(registeredRoutes[route], (string?)card.Attribute("Icon"));
            Assert.Equal("150", (string?)card.Attribute("MinHeight"));
        });
    }

    [Fact]
    public void EveryGalleryPage_EndsWithTwoDisabledReferenceButtons()
    {
        foreach (
            var path in Directory.EnumerateFiles(
                ViewsRoot,
                "*Page.xaml",
                SearchOption.AllDirectories
            )
        )
        {
            var page = XDocument.Load(path);
            if (page.Root?.Name.LocalName != "Page")
            {
                continue;
            }

            var pageBody = Assert.Single(
                page.Descendants(),
                element => element.Name.LocalName == "PageBody"
            );
            var chunks = pageBody
                .Elements()
                .Where(element => element.Name.LocalName == "Chunk")
                .ToArray();
            var reference = Assert.Single(
                chunks,
                element => (string?)element.Attribute("Title") == "Reference"
            );
            Assert.Same(chunks[^1], reference);

            var buttons = reference
                .Descendants()
                .Where(element => element.Name.LocalName == "CardButton")
                .ToArray();
            Assert.Equal(2, buttons.Length);
            Assert.All(
                buttons,
                button => Assert.Equal("False", (string?)button.Attribute("IsEnabled"))
            );
        }
    }

    [Fact]
    public void SplitPresenters_UseTheDefaultLeftPresentationPosition()
    {
        foreach (
            var path in Directory.EnumerateFiles(
                ViewsRoot,
                "*Page.xaml",
                SearchOption.AllDirectories
            )
        )
        {
            var page = XDocument.Load(path);
            var splitPresenters = page
                .Descendants()
                .Where(element =>
                    element.Name.LocalName == "Presenter"
                    && (string?)element.Attribute("PresenterMode") == "Split"
                )
                .ToArray();

            Assert.All(
                splitPresenters,
                presenter =>
                    Assert.Equal(
                        "Left",
                        (string?)presenter.Attribute("PresenterPosition")
                    )
            );
        }
    }

    [Fact]
    public void About_RemovesAddressesAndUsesDisabledProjectCardButtons()
    {
        var page = LoadPage("AboutPage.xaml");
        Assert.DoesNotContain(
            page.Descendants(),
            element =>
                element.Name.LocalName == "Chunk"
                && (string?)element.Attribute("Title") == "Addresses"
        );

        var project = page
            .Descendants()
            .Single(element =>
                element.Name.LocalName == "Chunk"
                && (string?)element.Attribute("Title") == "Project"
            );
        Assert.DoesNotContain(
            project.Descendants(),
            element => element.Name.LocalName == "Card"
        );
        var buttons = project
            .Descendants()
            .Where(element => element.Name.LocalName == "CardButton")
            .ToArray();
        Assert.Equal(2, buttons.Length);
        Assert.All(
            buttons,
            button => Assert.Equal("False", (string?)button.Attribute("IsEnabled"))
        );
    }

    [Fact]
    public void RegisteredDisplayPages_UseOneCodeSpaceOnlyForUsage()
    {
        var program = File.ReadAllText(Path.Combine(RepositoryRoot, "src", "Gallery", "Program.cs"));
        var excludedPages = new HashSet<string>(StringComparer.Ordinal)
        {
            "AboutPage",
            "ControlLibraryPage",
            "HomePage",
        };
        var displayPages = Regex.Matches(
                program,
                @"services\.AddNavigable<(?<page>\w+Page)>"
            )
            .Select(match => match.Groups["page"].Value)
            .Where(page => !excludedPages.Contains(page))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        Assert.NotEmpty(displayPages);
        Assert.All(displayPages, pageType =>
        {
            var page = LoadPage($"{pageType}.xaml");
            var usage = Assert.Single(
                page.Descendants(),
                element =>
                    element.Name.LocalName == "Chunk"
                    && (string?)element.Attribute("Title") == "Usage"
            );
            Assert.Null((string?)usage.Attribute("Content"));
            var codeSpace = Assert.Single(usage.Elements());
            Assert.Equal("CodeSpace", codeSpace.Name.LocalName);
            Assert.False(
                string.IsNullOrWhiteSpace((string?)codeSpace.Attribute("Text")),
                $"{pageType} must provide concrete XAML or C# usage."
            );
        });
    }

    [Fact]
    public void VariantChunks_UseBodyFreeTopDownPresenterCellsExceptWindowCaptionButton()
    {
        foreach (var path in Directory.EnumerateFiles(ViewsRoot, "*Page.xaml"))
        {
            var page = XDocument.Load(path);
            var variantChunks = page
                .Descendants()
                .Where(element =>
                    element.Name.LocalName == "Chunk"
                    && (string?)element.Attribute("Title") == "Variant"
                )
                .ToArray();

            foreach (var variantChunk in variantChunks)
            {
                if (Path.GetFileName(path) == "WindowCaptionButtonPage.xaml")
                {
                    var presenter = Assert.Single(
                        variantChunk.Descendants(),
                        element => element.Name.LocalName == "Presenter"
                    );
                    Assert.Equal("Split", (string?)presenter.Attribute("PresenterMode"));
                    Assert.Equal("Left", (string?)presenter.Attribute("PresenterPosition"));
                    continue;
                }

                var layout = Assert.Single(
                    variantChunk.Elements(),
                    element => element.Name.LocalName == "UniformGrid"
                );
                Assert.True(int.Parse((string?)layout.Attribute("Columns") ?? "0") > 1);

                var presenters = layout
                    .Elements()
                    .Where(element => element.Name.LocalName == "Presenter")
                    .ToArray();
                Assert.NotEmpty(presenters);
                Assert.Equal(layout.Elements().Count(), presenters.Length);
                var presentationHeights = new List<double>();

                Assert.All(
                    presenters,
                    presenter =>
                    {
                        Assert.Equal("TopDown", (string?)presenter.Attribute("PresenterMode"));
                        Assert.False(
                            string.IsNullOrWhiteSpace((string?)presenter.Attribute("Title"))
                        );
                        Assert.False(
                            string.IsNullOrWhiteSpace((string?)presenter.Attribute("Content"))
                        );
                        var presentation = Assert.Single(
                            presenter.Elements(),
                            element => element.Name.LocalName == "Presenter.Presentation"
                        );
                        var presentationRoot = Assert.Single(presentation.Elements());
                        var minHeight = Assert.IsType<XAttribute>(
                            presentationRoot.Attribute("MinHeight")
                        );
                        Assert.True(
                            double.Parse(
                                minHeight.Value,
                                System.Globalization.CultureInfo.InvariantCulture
                            ) >= 136
                        );
                        presentationHeights.Add(
                            double.Parse(
                                minHeight.Value,
                                System.Globalization.CultureInfo.InvariantCulture
                            )
                        );
                        Assert.DoesNotContain(
                            presenter.Elements(),
                            element => element.Name.LocalName == "Presenter.Body"
                        );
                    }
                );
                Assert.Single(presentationHeights.Distinct());
            }
        }
    }

    [Fact]
    public void MultiColumnLayouts_ContainOnlyTopDownPresentersAndNeverHeaderChunks()
    {
        foreach (var path in Directory.EnumerateFiles(ViewsRoot, "*Page.xaml"))
        {
            var page = XDocument.Load(path);
            var multiColumnLayouts = page
                .Descendants()
                .Where(element =>
                    element.Name.LocalName == "UniformGrid"
                    && int.TryParse((string?)element.Attribute("Columns"), out var columns)
                    && columns > 1
                );

            foreach (var layout in multiColumnLayouts)
            {
                Assert.True(
                    (string?)layout.Attribute("VerticalAlignment") is null or "Stretch"
                );
                Assert.DoesNotContain(
                    layout.Descendants(),
                    element => element.Name.LocalName == "HeaderChunk"
                );
                Assert.DoesNotContain(
                    layout.Elements(),
                    element => (string?)element.Attribute("VerticalAlignment") == "Top"
                );
                Assert.All(
                    layout
                        .Elements()
                        .Where(element => element.Name.LocalName == "Presenter"),
                    presenter =>
                        Assert.Equal(
                            "TopDown",
                            (string?)presenter.Attribute("PresenterMode")
                        )
                );
            }
        }
    }

    [Fact]
    public void NavigationAndCardIcons_UseAdaptiveThemeColors()
    {
        var primary = "{DynamicResource FlourishPrimaryForegroundBrush}";
        var onPrimary = "{DynamicResource FlourishForegroundOnPrimaryBrush}";
        var disabled = "{DynamicResource FlourishNeutralForegroundDisabledBrush}";

        var shell = XDocument.Load(
            Path.Combine(
                RepositoryRoot,
                "src",
                "Flourish",
                "Views",
                "Windows",
                "FlourishShellWindow.xaml"
            )
        );
        var navigationIcon = shell
            .Descendants()
            .Single(element =>
                (string?)element.Attribute(XamlNamespace + "Name") == "NavigationItemIcon"
            );
        Assert.Equal(primary, (string?)navigationIcon.Attribute("Foreground"));

        foreach (var fileName in new[] { "Card.xaml", "ActionCard.xaml", "CardButton.xaml" })
        {
            var template = XDocument.Load(
                Path.Combine(
                    RepositoryRoot,
                    "src",
                    "Flourish",
                    "Controls",
                    fileName
                )
            );
            var iconHost = template
                .Descendants()
                .Single(element =>
                    (string?)element.Attribute(XamlNamespace + "Name") == "IconHost"
                );
            Assert.Equal(
                primary,
                (string?)iconHost.Attribute("TextElement.Foreground")
            );

            if (fileName == "ActionCard.xaml")
            {
                continue;
            }

            var filledTrigger = template
                .Descendants()
                .Single(element =>
                    element.Name.LocalName == "Trigger"
                    && (string?)element.Attribute("Property") == "Variant"
                    && (string?)element.Attribute("Value") == "Filled"
                    && element
                        .Elements()
                        .Any(setter =>
                            (string?)setter.Attribute("TargetName") == "IconHost"
                        )
                );
            Assert.Contains(
                filledTrigger.Elements(),
                element =>
                    element.Name.LocalName == "Setter"
                    && (string?)element.Attribute("TargetName") == "IconHost"
                    && (string?)element.Attribute("Property") == "TextElement.Foreground"
                    && (string?)element.Attribute("Value") == onPrimary
            );

            if (fileName == "CardButton.xaml")
            {
                var disabledTrigger = template
                    .Descendants()
                    .Single(element =>
                        element.Name.LocalName == "Trigger"
                        && (string?)element.Attribute("Property") == "IsEnabled"
                        && (string?)element.Attribute("Value") == "False"
                        && element
                            .Elements()
                            .Any(setter =>
                                (string?)setter.Attribute("TargetName") == "IconHost"
                            )
                    );
                Assert.Contains(
                    disabledTrigger.Elements(),
                    element =>
                        (string?)element.Attribute("TargetName") == "IconHost"
                        && (string?)element.Attribute("Value") == disabled
                );
            }
        }
    }

    [Fact]
    public void WindowMessages_UseOneContinuousHorizontalActionCardColumn()
    {
        var document = LoadPage("WindowRuntimePage.xaml");
        var messageChunk = document
            .Descendants()
            .Single(element =>
                element.Name.LocalName == "Chunk"
                && (string?)element.Attribute("Title") == "Messages"
            );
        var layout = Assert.Single(
            messageChunk.Elements(),
            element => element.Name.LocalName == "UniformGrid"
        );
        Assert.Equal("2", (string?)layout.Attribute("Columns"));

        var actionColumn = Assert.Single(
            layout.Elements(),
            element => element.Name.LocalName == "StackPanel"
        );

        var actionCards = actionColumn
            .Elements()
            .Where(element => element.Name.LocalName == "ActionCard")
            .ToArray();
        Assert.Equal(8, actionCards.Length);
        Assert.All(
            actionCards,
            actionCard =>
                Assert.True(
                    (string?)actionCard.Attribute("Variant") is null or "Horizontal"
                )
        );
        Assert.DoesNotContain(
            actionColumn.Elements(),
            element => element.Name.LocalName != "ActionCard"
        );
        Assert.Single(
            layout.Elements(),
            element =>
                element.Name.LocalName == "OutputCard"
                && (string?)element.Attribute(XNamespace.Get("http://schemas.microsoft.com/winfx/2006/xaml") + "Name")
                    == "MessageActivityOutput"
        );
    }

    private static XDocument LoadPage(string fileName) =>
        XDocument.Load(Path.Combine(ViewsRoot, fileName));

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (
                Directory.Exists(Path.Combine(current.FullName, "src", "Flourish"))
                && Directory.Exists(Path.Combine(current.FullName, "src", "Gallery"))
            )
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the Flourish repository root.");
    }
}
