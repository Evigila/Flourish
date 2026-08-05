using System.IO;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace ArkheideSystem.Flourish.Test.Controls;

public sealed class GalleryControlPageStructureTests
{
    private static readonly XNamespace XamlNamespace =
        "http://schemas.microsoft.com/winfx/2006/xaml";
    private static readonly string RepositoryRoot = TestPaths.RepositoryRoot;
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

        var actionsChunk = Assert.Single(
            topicChunks,
            element => (string?)element.Attribute("Title") == "Actions"
        );
        var actionPresentations = actionsChunk
            .Descendants()
            .Where(element => element.Name.LocalName == "Presenter.Presentation")
            .ToArray();
        Assert.Equal(2, actionPresentations.Length);
        Assert.All(
            actionPresentations,
            presentation =>
            {
                var fixedSurface = Assert.Single(
                    presentation.Elements(),
                    element => element.Name.LocalName == "Grid"
                );
                AssertCanonicalPresentationRoot(fixedSurface);
                var actionGroup = Assert.Single(
                    fixedSurface.Elements(),
                    element => element.Name.LocalName == "StackPanel"
                );
                Assert.Equal("Horizontal", (string?)actionGroup.Attribute("Orientation"));
                Assert.Equal(
                    "Center",
                    (string?)actionGroup.Attribute("HorizontalAlignment")
                );
                Assert.Equal(
                    "Center",
                    (string?)actionGroup.Attribute("VerticalAlignment")
                );
            }
        );

        var cardButtonPage = LoadPage("CardButtonPage.xaml");
        var cardButtonVariant = cardButtonPage
            .Descendants()
            .Single(element =>
                element.Name.LocalName == "Chunk"
                && (string?)element.Attribute("Title") == "Variant"
            );
        var cardButtonPresenters = cardButtonVariant
            .Descendants()
            .Where(element => element.Name.LocalName == "Presenter")
            .ToArray();

        Assert.Equal(4, cardButtonPresenters.Length);
        Assert.All(
            cardButtonPresenters,
            presenter =>
            {
                Assert.Null((string?)presenter.Attribute("Height"));
                Assert.Equal("220", (string?)presenter.Attribute("PresentationMinHeight"));
                var presentation = Assert.Single(
                    presenter.Elements(),
                    element => element.Name.LocalName == "Presenter.Presentation"
                );
                var cardButton = Assert.Single(
                    presentation.Elements(),
                    element => element.Name.LocalName == "CardButton"
                );
                Assert.Equal("200", (string?)cardButton.Attribute("Width"));
                Assert.Equal("120", (string?)cardButton.Attribute("Height"));
                AssertCanonicalPresentationRoot(cardButton);
            }
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
            "TextBlock",
            "ListBox",
            "BunchedListBox",
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

        var documentPage = LoadPage("DocumentPage.xaml");
        Assert.Contains(
            documentPage.Descendants(),
            element => element.Name.LocalName == "Paragraph"
        );
        Assert.Contains(documentPage.Descendants(), element => element.Name.LocalName == "Chunk" && (string?)element.Attribute("Title") == "Paragraph");
        Assert.Contains(LoadPage("ListBoxPage.xaml").Descendants(), element => element.Name.LocalName == "Chunk" && (string?)element.Attribute("Title") == "ListBoxItem");
        Assert.Contains(LoadPage("BunchedListBoxPage.xaml").Descendants(), element => element.Name.LocalName == "Chunk" && (string?)element.Attribute("Title") == "BunchedListBoxItem");
        Assert.Contains(LoadPage("ComboBoxPage.xaml").Descendants(), element => element.Name.LocalName == "Chunk" && (string?)element.Attribute("Title") == "ComboBoxItem");
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

        Assert.Equal(28, registeredRoutes.Count);
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
            .Where(name => name is not ("Paragraph" or "ListBoxItem" or "BunchedListBoxItem" or "ComboBoxItem"))
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
            if (Path.GetFileName(path) == "AboutPage.xaml")
            {
                continue;
            }

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
                {
                    var isRightPositionExample =
                        Path.GetFileName(path) == "PresenterPage.xaml"
                        && (string?)presenter.Attribute("Title") == "Right presentation"
                        && presenter
                            .Ancestors()
                            .Any(element =>
                                element.Name.LocalName == "Chunk"
                                && (string?)element.Attribute("Title") == "Split"
                            );
                    Assert.Equal(
                        isRightPositionExample ? "Right" : "Left",
                        (string?)presenter.Attribute("PresenterPosition")
                    );
                }
            );
        }
    }

    [Fact]
    public void PresenterPage_ShowsBothSplitDirections()
    {
        var page = LoadPage("PresenterPage.xaml");
        var splitChunk = page
            .Descendants()
            .Single(element =>
                element.Name.LocalName == "Chunk"
                && (string?)element.Attribute("Title") == "Split"
            );
        var positions = splitChunk
            .Descendants()
            .Where(element =>
                element.Name.LocalName == "Presenter"
                && (string?)element.Attribute("PresenterMode") == "Split"
            )
            .Select(element => (string?)element.Attribute("PresenterPosition"))
            .ToArray();

        Assert.Equal(2, positions.Length);
        Assert.Contains("Left", positions);
        Assert.Contains("Right", positions);
    }

    [Fact]
    public void About_ContainsOnlyAContentFreeProjectChunk()
    {
        var page = LoadPage("AboutPage.xaml");
        var pageBody = Assert.Single(
            page.Descendants(),
            element => element.Name.LocalName == "PageBody"
        );
        var children = pageBody.Elements().ToArray();
        Assert.Equal(2, children.Length);
        Assert.Equal("HeaderChunk", children[0].Name.LocalName);
        var project = children[1];
        Assert.Equal("Chunk", project.Name.LocalName);
        Assert.Equal("Project", (string?)project.Attribute("Title"));
        Assert.Null((string?)project.Attribute("Content"));
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
    public void ControlDisplayPages_TeachUsageWithCodePresenters()
    {
        var program = File.ReadAllText(Path.Combine(RepositoryRoot, "src", "Gallery", "Program.cs"));
        var displayPages = Regex.Matches(
                program,
                @"group\.AddNavigableViewItem<(?<page>\w+Page)>\(childId:\s*1\)"
            )
            .Select(match => match.Groups["page"].Value)
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
            var layout = Assert.Single(usage.Elements());
            Assert.Equal("StackPanel", layout.Name.LocalName);
            var presenters = layout
                .Elements()
                .Where(element => element.Name.LocalName == "Presenter")
                .ToArray();
            Assert.NotEmpty(presenters);
            Assert.Equal(
                presenters.Length,
                usage.Descendants().Count(element => element.Name.LocalName == "CodeSpace")
            );

            for (var index = 0; index < presenters.Length; index++)
            {
                var presenter = presenters[index];
                Assert.Equal("Split", (string?)presenter.Attribute("PresenterMode"));
                Assert.Equal("Left", (string?)presenter.Attribute("PresenterPosition"));
                Assert.False(string.IsNullOrWhiteSpace((string?)presenter.Attribute("Title")));
                Assert.False(string.IsNullOrWhiteSpace((string?)presenter.Attribute("Content")));
                Assert.DoesNotContain(
                    presenter.Elements(),
                    element => element.Name.LocalName == "Presenter.Body"
                );
                if (index > 0)
                {
                    Assert.Equal(
                        "{DynamicResource FlourishPresenterPeerMargin}",
                        (string?)presenter.Attribute("Margin")
                    );
                }

                var presentation = Assert.Single(
                    presenter.Elements(),
                    element => element.Name.LocalName == "Presenter.Presentation"
                );
                var codeSpace = Assert.Single(presentation.Elements());
                Assert.Equal("CodeSpace", codeSpace.Name.LocalName);
                Assert.False(
                    string.IsNullOrWhiteSpace((string?)codeSpace.Attribute("Text")),
                    $"{pageType} must provide concrete usage."
                );
                Assert.Equal("True", (string?)codeSpace.Attribute("IsExpanded"));
                Assert.Equal("False", (string?)codeSpace.Attribute("CanCollapse"));
            }

            var usageCodeSpaces = usage
                .Descendants()
                .Where(element => element.Name.LocalName == "CodeSpace")
                .ToHashSet();
            Assert.DoesNotContain(
                page.Descendants().Where(element =>
                    element.Name.LocalName == "CodeSpace"
                    && !usageCodeSpaces.Contains(element)
                ),
                codeSpace =>
                    codeSpace.Attribute("IsExpanded") is not null
                    || codeSpace.Attribute("CanCollapse") is not null
            );
        });
    }

    [Fact]
    public void PresenterFillExamples_DoNotConstrainTheirAutoSizedPresentationRoots()
    {
        var commandsPage = LoadPage("CommandsPage.xaml");
        var registryPresenter = commandsPage
            .Descendants()
            .Single(element =>
                element.Name.LocalName == "Presenter"
                && (string?)element.Attribute("Title") == "Registry state"
            );
        var registryRoot = Assert.Single(
            registryPresenter
                .Elements()
                .Single(element => element.Name.LocalName == "Presenter.Presentation")
                .Elements()
        );

        Assert.Equal("StackPanel", registryRoot.Name.LocalName);
        Assert.Null(registryRoot.Attribute("HorizontalAlignment"));
        Assert.Equal("Center", (string?)registryRoot.Attribute("VerticalAlignment"));
        Assert.Null(registryRoot.Attribute("Width"));
        Assert.Null(registryRoot.Attribute("Height"));
        Assert.Contains(
            registryRoot.Elements(),
            element => element.Name.LocalName == "ListBox"
        );

        var presenterPage = LoadPage("PresenterPage.xaml");
        var overlayPresenter = presenterPage
            .Descendants()
            .Single(element =>
                element.Name.LocalName == "Presenter"
                && (string?)element.Attribute("PresenterMode") == "Overlay"
            );
        var overlayRoot = Assert.Single(
            overlayPresenter
                .Elements()
                .Single(element => element.Name.LocalName == "Presenter.Presentation")
                .Elements()
        );

        Assert.Equal("Grid", overlayRoot.Name.LocalName);
        Assert.Null(overlayRoot.Attribute("HorizontalAlignment"));
        Assert.Null(overlayRoot.Attribute("VerticalAlignment"));
        Assert.Null(overlayRoot.Attribute("Width"));
        Assert.Null(overlayRoot.Attribute("Height"));
        var centeredOverlayGroup = Assert.Single(
            overlayRoot.Elements(),
            element => element.Name.LocalName == "StackPanel"
        );
        Assert.Equal(
            "Center",
            (string?)centeredOverlayGroup.Attribute("HorizontalAlignment")
        );
        Assert.Equal(
            "Center",
            (string?)centeredOverlayGroup.Attribute("VerticalAlignment")
        );
    }

    [Fact]
    public void AppearanceMaterialPickerDocumentsAutoAndDisablesUnsupportedOptions()
    {
        var page = LoadPage("AppearancePage.xaml");
        var materialBox = page
            .Descendants()
            .Single(element =>
                (string?)element.Attribute(XamlNamespace + "Name") == "MaterialBox"
            );

        Assert.Null(materialBox.Attribute("DisplayMemberPath"));
        Assert.DoesNotContain(
            materialBox.Descendants(),
            element => element.Name.LocalName == "Style"
        );

        var materialChunk = page
            .Descendants()
            .Single(element =>
                element.Name.LocalName == "Chunk"
                && (string?)element.Attribute("Title") == "Window material"
            );
        var usage = Assert.Single(
            materialChunk.Descendants(),
            element => element.Name.LocalName == "CodeSpace"
        );
        var code = Assert.IsType<string>((string?)usage.Attribute("Text"));
        Assert.Contains("MaterialEffect.Auto", code, StringComparison.Ordinal);
        Assert.Contains("MaterialEffect.MicaAlt", code, StringComparison.Ordinal);
        Assert.Contains("PlatformNotSupportedException", code, StringComparison.Ordinal);
    }

    [Fact]
    public void UsageSeparatesMarkupRuntimeAndAccessibilityContracts()
    {
        var outputUsage = LoadPage("OutputCardPage.xaml")
            .Descendants()
            .Single(element =>
                element.Name.LocalName == "Chunk"
                && (string?)element.Attribute("Title") == "Usage"
            );
        var outputExamples = outputUsage
            .Descendants()
            .Where(element => element.Name.LocalName == "CodeSpace")
            .Select(element => (string?)element.Attribute("Text") ?? string.Empty)
            .ToArray();
        Assert.True(outputExamples.Length >= 2);
        Assert.Contains(outputExamples, code => code.Contains("<flourish:OutputCard", StringComparison.Ordinal));
        Assert.Contains(
            outputExamples,
            code =>
                code.Contains("WriteLine", StringComparison.Ordinal)
                && code.Contains("Clear", StringComparison.Ordinal)
        );

        var buttonUsage = LoadPage("ButtonPage.xaml")
            .Descendants()
            .Single(element =>
                element.Name.LocalName == "Chunk"
                && (string?)element.Attribute("Title") == "Usage"
            );
        var buttonExamples = buttonUsage
            .Descendants()
            .Where(element => element.Name.LocalName == "CodeSpace")
            .Select(element => (string?)element.Attribute("Text") ?? string.Empty)
            .ToArray();
        Assert.True(buttonExamples.Length >= 2);
        Assert.Contains(
            buttonExamples,
            code =>
                code.Contains("AutomationProperties.Name", StringComparison.Ordinal)
                && code.Contains("ToolTip", StringComparison.Ordinal)
        );
    }

    [Fact]
    public void InteractionContracts_UseLiveStatePresenters()
    {
        var expectedControls = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["ButtonPage.xaml"] = "Button",
            ["CardButtonPage.xaml"] = "CardButton",
        };

        foreach (var expected in expectedControls)
        {
            var page = LoadPage(expected.Key);
            var contract = page
                .Descendants()
                .Single(element =>
                    element.Name.LocalName == "Chunk"
                    && (string?)element.Attribute("Title") == "Interaction contract"
                );
            Assert.DoesNotContain(
                contract.Descendants(),
                element => element.Name.LocalName == "Card"
            );

            var layout = Assert.Single(contract.Elements());
            Assert.Equal("UniformGrid", layout.Name.LocalName);
            Assert.True(int.Parse((string?)layout.Attribute("Columns") ?? "0") >= 2);
            var presenters = layout
                .Elements()
                .Where(element => element.Name.LocalName == "Presenter")
                .ToArray();
            Assert.True(presenters.Length >= 2);
            Assert.All(presenters, presenter =>
            {
                Assert.Equal("TopDown", (string?)presenter.Attribute("PresenterMode"));
                Assert.DoesNotContain(
                    presenter.Elements(),
                    element => element.Name.LocalName == "Presenter.Body"
                );
                var presentation = Assert.Single(
                    presenter.Elements(),
                    element => element.Name.LocalName == "Presenter.Presentation"
                );
                Assert.Contains(
                    presentation.DescendantsAndSelf(),
                    element => element.Name.LocalName == expected.Value
                );
            });
            Assert.Contains(
                contract.Descendants(),
                element =>
                    element.Name.LocalName == expected.Value
                    && (string?)element.Attribute("IsEnabled") == "False"
            );
        }
    }

    [Fact]
    public void WindowCaptionInteraction_DistinguishesRoutineAndCloseActions()
    {
        var page = LoadPage("WindowCaptionButtonPage.xaml");
        var contract = page
            .Descendants()
            .Single(element =>
                element.Name.LocalName == "Chunk"
                && (string?)element.Attribute("Title") == "Interaction contract"
            );
        var buttons = contract
            .Descendants()
            .Where(element => element.Name.LocalName == "WindowCaptionButton")
            .ToArray();

        Assert.Equal(4, buttons.Length);
        Assert.Equal(
            3,
            buttons.Count(element => (string?)element.Attribute("Variant") == "Text")
        );
        var close = Assert.Single(
            buttons,
            element => (string?)element.Attribute("Variant") == "Danger"
        );
        Assert.Equal("Close", (string?)close.Attribute("AutomationProperties.Name"));
        Assert.DoesNotContain(
            buttons,
            element => element.Attribute("IsEnabled") is not null
        );
    }

    [Fact]
    public void NonControlPages_UseDetailedTopicsAndColocatedCode()
    {
        var program = File.ReadAllText(Path.Combine(RepositoryRoot, "src", "Gallery", "Program.cs"));
        var controlPages = Regex.Matches(
                program,
                @"group\.AddNavigableViewItem<(?<page>\w+Page)>\(childId:\s*1\)"
            )
            .Select(match => match.Groups["page"].Value)
            .Append("ControlLibraryPage")
            .ToHashSet(StringComparer.Ordinal);
        var informationalPages = new HashSet<string>(StringComparer.Ordinal)
        {
            "HomePage",
        };
        var nonControlPages = Directory.EnumerateFiles(ViewsRoot, "*Page.xaml")
            .Select(Path.GetFileNameWithoutExtension)
            .Where(page =>
                page is not null
                && page != "AboutPage"
                && !controlPages.Contains(page)
            )
            .Cast<string>()
            .ToArray();

        Assert.NotEmpty(nonControlPages);
        Assert.All(nonControlPages, pageType =>
        {
            var page = LoadPage($"{pageType}.xaml");
            var topics = page
                .Descendants()
                .Where(element =>
                    element.Name.LocalName == "Chunk"
                    && (string?)element.Attribute("Title") != "Reference"
                )
                .ToArray();

            Assert.NotEmpty(topics);
            Assert.DoesNotContain(
                topics,
                topic => (string?)topic.Attribute("Title") == "Usage"
            );
            Assert.All(topics, topic =>
            {
                var title = Assert.IsType<string>((object?)topic.Attribute("Title")?.Value);
                var content = Assert.IsType<string>((object?)topic.Attribute("Content")?.Value);
                Assert.DoesNotContain("Runtime", title, StringComparison.OrdinalIgnoreCase);
                Assert.True(
                    content.Length >= 200,
                    $"{pageType}/{title} should explain purpose, usage, recommended scenarios, and code intent."
                );

                if (informationalPages.Contains(pageType))
                {
                    Assert.DoesNotContain(
                        topic.Descendants(),
                        element => element.Name.LocalName == "CodeSpace"
                    );
                    return;
                }

                var codeSpace = Assert.Single(
                    topic.Descendants(),
                    element => element.Name.LocalName == "CodeSpace"
                );
                Assert.False(string.IsNullOrWhiteSpace((string?)codeSpace.Attribute("Text")));
                Assert.Same(codeSpace.Parent?.Elements().Last(), codeSpace);
            });
        });
    }

    [Fact]
    public void ConfigurationPage_ColocatesCompleteApiUsageWithEachTopic()
    {
        var page = LoadPage("ConfigurationPage.xaml");
        Assert.DoesNotContain(
            page.Descendants(),
            element =>
                element.Name.LocalName == "Chunk"
                && (string?)element.Attribute("Title") == "Usage"
        );

        var expectedApis = new Dictionary<string, string[]>
        {
            ["Configuration values"] =
            [
                "configuration.Changed",
                "configuration.Reload",
                "configuration.Current",
                "configuration[",
                "configuration.Get<int>",
                "configuration.GetSection<ReportOptions>",
            ],
            ["App settings"] =
            [
                "settings.FilePath",
                "settings.SetAsync",
                "settings.MergeAsync",
                "settings.AppendAsync",
                "settings.RemoveAsync",
                "settings.UpdateAsync",
                "editor.Set",
                "editor.Merge",
                "editor.Append",
                "editor.Remove",
                "result.Changed",
                "result.FilePath",
                "result.ConfigurationReloaded",
            ],
            ["Localization"] =
            [
                "ConfigData",
                "InitLocale",
                "localization.Changed",
                "localization.CurrentLocale",
                "localization.AvailableLocales",
                "localization.Get",
                "localization.Format",
            ],
            ["Locale files"] =
            [
                "AddLocaleFile",
                "localization.RegisterFile",
                "registration.Id",
                "registration.Locale",
                "registration.FilePath",
                "localization.ReloadFile",
                "localization.Unregister",
            ],
        };

        foreach (var expected in expectedApis)
        {
            var chunk = page
                .Descendants()
                .Single(element =>
                    element.Name.LocalName == "Chunk"
                    && (string?)element.Attribute("Title") == expected.Key
                );
            var body = Assert.Single(chunk.Elements());
            Assert.Equal("StackPanel", body.Name.LocalName);
            var codeSpace = Assert.Single(
                body.Elements(),
                element => element.Name.LocalName == "CodeSpace"
            );
            Assert.Same(body.Elements().Last(), codeSpace);
            var code = Assert.IsType<string>((object?)codeSpace.Attribute("Text")?.Value);
            var content = Assert.IsType<string>((object?)chunk.Attribute("Content")?.Value);
            Assert.DoesNotContain("Runtime", expected.Key, StringComparison.OrdinalIgnoreCase);
            Assert.True(content.Length >= 200);
            Assert.Contains("Use", content, StringComparison.Ordinal);
            Assert.Contains("code below", content, StringComparison.OrdinalIgnoreCase);
            Assert.InRange(
                Regex.Matches(code, @"(?m)^// ").Count,
                low: 1,
                high: 3
            );
            Assert.All(
                expected.Value,
                api => Assert.Contains(api, code, StringComparison.Ordinal)
            );
        }
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
                        AssertCanonicalPresentationRoot(presentationRoot);
                        Assert.DoesNotContain(
                            presenter.Elements(),
                            element => element.Name.LocalName == "Presenter.Body"
                        );
                    }
                );
            }
        }
    }

    [Fact]
    public void Presentations_UseFillOrNaturalCenteredContentWithoutNestedBordersExceptCodeSpace()
    {
        foreach (
            var path in Directory.EnumerateFiles(
                ViewsRoot,
                "*.xaml",
                SearchOption.AllDirectories
            )
        )
        {
            var page = XDocument.Load(path);
            var presentations = page
                .Descendants()
                .Where(element => element.Name.LocalName == "Presenter.Presentation");

            foreach (var presentation in presentations)
            {
                var presentationRoot = Assert.Single(presentation.Elements());
                if (presentationRoot.Name.LocalName == "CodeSpace")
                {
                    continue;
                }

                AssertCanonicalPresentationRoot(presentationRoot);
                Assert.DoesNotContain(
                    presentationRoot.DescendantsAndSelf(),
                    element => element.Name.LocalName == "Border"
                );
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

        var navigationPane = XDocument.Load(
            Path.Combine(
                RepositoryRoot,
                "src",
                "Flourish",
                "Views",
                "Windows",
                "FlourishNavigationPane.xaml"
            )
        );
        var navigationIcon = navigationPane
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
            messageChunk.Descendants(),
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

    [Fact]
    public void WindowPositionAction_UsesAGlyphFromTheDefaultIconFont()
    {
        var document = LoadPage("WindowRuntimePage.xaml");
        var actionCard = document
            .Descendants()
            .Single(element =>
                element.Name.LocalName == "ActionCard"
                && (string?)element.Attribute("Title") == "Window position"
            );

        Assert.Equal("\uE707", (string?)actionCard.Attribute("Icon"));
        Assert.DoesNotContain("\uF2D9", document.ToString());
    }

    private static XDocument LoadPage(string fileName) =>
        XDocument.Load(Path.Combine(ViewsRoot, fileName));

    private static void AssertCanonicalPresentationRoot(XElement presentationRoot)
    {
        if (presentationRoot.Name.LocalName == "StackPanel")
        {
            var horizontalAlignment = (string?)presentationRoot.Attribute(
                "HorizontalAlignment"
            );
            if ((string?)presentationRoot.Attribute("Orientation") == "Horizontal")
            {
                Assert.Equal("Center", horizontalAlignment);
            }
            else
            {
                Assert.True(horizontalAlignment is null or "Center");
            }
            Assert.Equal(
                "Center",
                (string?)presentationRoot.Attribute("VerticalAlignment")
            );
            Assert.Null(presentationRoot.Attribute("Width"));
            Assert.Null(presentationRoot.Attribute("Height"));
            return;
        }

        if (presentationRoot.Name.LocalName is "UniformGrid" or "FlourishTextBlock")
        {
            Assert.Equal("Center", (string?)presentationRoot.Attribute("HorizontalAlignment"));
            Assert.Equal("Center", (string?)presentationRoot.Attribute("VerticalAlignment"));
            Assert.Null(presentationRoot.Attribute("Width"));
            Assert.Null(presentationRoot.Attribute("Height"));
            return;
        }

        AssertOptionalPositiveDimension(presentationRoot, "Width");
        AssertOptionalPositiveDimension(presentationRoot, "Height");
    }

    private static void AssertOptionalPositiveDimension(
        XElement presentationRoot,
        string attributeName
    )
    {
        if (presentationRoot.Attribute(attributeName) is not { } dimension)
        {
            return;
        }

        Assert.True(
            double.TryParse(
                dimension.Value,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out var value
            ) && double.IsFinite(value) && value > 0
        );
    }
}
