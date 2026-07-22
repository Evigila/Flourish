using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using ArkheideSystem.Flourish.Controls;
using ArkheideSystem.Flourish.Themes;
using CustomScrollViewer = ArkheideSystem.Flourish.Controls.ScrollViewer;
using FlourishButton = ArkheideSystem.Flourish.Controls.Button;
using WpfButton = System.Windows.Controls.Button;

namespace ArkheideSystem.Flourish.Test.Controls;

public sealed class FlourishPublicControlsTests
{
    private const string GenericThemeSource =
        "/Flourish;component/Themes/Generic.xaml";
    private const string XamlNamespace = "http://schemas.arkheide.system/flourish";

    [Fact]
    public void CanonicalThemeAndControlContracts_ArePublic()
    {
        Type[] publicContractTypes =
        [
            typeof(FlourishThemeResources),
            typeof(ButtonVariant),
            typeof(Variant),
            typeof(OverlayVariant),
            typeof(PresenterMode),
            typeof(PresenterPosition),
            typeof(FlourishGridSplitterVariant),
            typeof(FlourishListBoxAppearance),
            typeof(FlourishTextRole),
            typeof(HoverReveal),
            typeof(FlourishToolTipPlacement),
            .. GetPublicFlourishControlTypes(),
        ];

        Assert.All(publicContractTypes, type => Assert.True(type.IsPublic, type.FullName));
        Assert.All(
            new[]
            {
                nameof(HoverReveal.GetIsEnabled),
                nameof(HoverReveal.SetIsEnabled),
                nameof(HoverReveal.GetIsMotionEnabled),
                nameof(HoverReveal.SetIsMotionEnabled),
                nameof(HoverReveal.GetIsParticipant),
                nameof(HoverReveal.SetIsParticipant),
                nameof(HoverReveal.GetTemplateHandlesInteraction),
                nameof(HoverReveal.SetTemplateHandlesInteraction),
                nameof(HoverReveal.GetAnimationDuration),
                nameof(HoverReveal.SetAnimationDuration),
                nameof(HoverReveal.GetOverrideColor),
                nameof(HoverReveal.SetOverrideColor),
            },
            methodName => AssertPublicStaticMethod(typeof(HoverReveal), methodName)
        );
        Assert.All(
            new[]
            {
                nameof(FlourishToolTipPlacement.GetIsEnabled),
                nameof(FlourishToolTipPlacement.SetIsEnabled),
            },
            methodName =>
                AssertPublicStaticMethod(typeof(FlourishToolTipPlacement), methodName)
        );
    }

    [Fact]
    public void Assembly_MapsStableXamlNamespaceToCanonicalPublicApiSurfaces()
    {
        var assembly = typeof(FlourishButton).Assembly;
        var definitions = assembly
            .GetCustomAttributes<XmlnsDefinitionAttribute>()
            .Where(definition => definition.XmlNamespace == XamlNamespace)
            .Select(definition => definition.ClrNamespace)
            .ToHashSet(StringComparer.Ordinal);
        var prefixes = assembly.GetCustomAttributes<XmlnsPrefixAttribute>();

        Assert.Contains("ArkheideSystem.Flourish.Abstract", definitions);
        Assert.Contains("ArkheideSystem.Flourish.Controls", definitions);
        Assert.Contains("ArkheideSystem.Flourish.Themes", definitions);
        Assert.Contains(
            prefixes,
            prefix => prefix.XmlNamespace == XamlNamespace && prefix.Prefix == "flourish"
        );
    }

    [Fact]
    public void CanonicalThemeResources_LoadTheSingleGenericThemeEntryPoint()
    {
        RunInSta(() =>
        {
            _ = Application.LoadComponent(new Uri(GenericThemeSource, UriKind.Relative));
            var resources = new FlourishThemeResources();

            Assert.Equal(GenericThemeSource, resources.Source.OriginalString);
        });
    }

    [Fact]
    public void CanonicalThemeResources_EnsureMergedIsIdempotent()
    {
        RunInSta(() =>
        {
            var resources = new ResourceDictionary();

            FlourishThemeResources.EnsureMerged(resources);
            FlourishThemeResources.EnsureMerged(resources);

            var merged = Assert.Single(resources.MergedDictionaries);
            Assert.IsType<FlourishThemeResources>(merged);
        });
    }

    [Fact]
    public void CanonicalThemeResources_EnsureMergedAddsCanonicalAfterUnrelatedGraph()
    {
        RunInSta(() =>
        {
            var resources = new ResourceDictionary();
            var wrapper = new ResourceDictionary();
            wrapper.MergedDictionaries.Add(new ResourceDictionary());
            resources.MergedDictionaries.Add(wrapper);

            FlourishThemeResources.EnsureMerged(resources);

            Assert.Equal(2, resources.MergedDictionaries.Count);
            Assert.Same(wrapper, resources.MergedDictionaries[0]);
            Assert.IsType<FlourishThemeResources>(resources.MergedDictionaries[1]);
        });
    }

    [Fact]
    public void CanonicalThemeResources_EnsureMergedDoesNotAddToExistingDuplicates()
    {
        RunInSta(() =>
        {
            var resources = new ResourceDictionary();
            var first = new FlourishThemeResources();
            var second = new FlourishThemeResources();
            resources.MergedDictionaries.Add(first);
            resources.MergedDictionaries.Add(second);

            FlourishThemeResources.EnsureMerged(resources);

            Assert.Equal(2, resources.MergedDictionaries.Count);
            Assert.Same(first, resources.MergedDictionaries[0]);
            Assert.Same(second, resources.MergedDictionaries[1]);
            Assert.Same(second, FlourishThemeResources.FindThemeRoot(resources));
        });
    }

    [Fact]
    public void CanonicalThemeResources_EnsureMergedRecognizesTheRootByType()
    {
        RunInSta(() =>
        {
            var resources = new FlourishThemeResources();
            var originalMergedDictionaries = resources.MergedDictionaries.ToArray();

            FlourishThemeResources.EnsureMerged(resources);

            Assert.Equal(
                originalMergedDictionaries,
                resources.MergedDictionaries.Cast<ResourceDictionary>()
            );
            Assert.Same(resources, FlourishThemeResources.FindThemeRoot(resources));
        });
    }

    [Fact]
    public void CanonicalThemeResources_EnsureMergedRecognizesNestedCanonicalType()
    {
        RunInSta(() =>
        {
            var root = new ResourceDictionary();
            var outerWrapper = new ResourceDictionary();
            var innerWrapper = new ResourceDictionary();
            var theme = new FlourishThemeResources();
            innerWrapper.MergedDictionaries.Add(theme);
            outerWrapper.MergedDictionaries.Add(innerWrapper);
            root.MergedDictionaries.Add(outerWrapper);

            FlourishThemeResources.EnsureMerged(root);
            FlourishThemeResources.EnsureMerged(root);

            Assert.Same(outerWrapper, Assert.Single(root.MergedDictionaries));
            Assert.Same(innerWrapper, Assert.Single(outerWrapper.MergedDictionaries));
            Assert.Same(theme, Assert.Single(innerWrapper.MergedDictionaries));
            Assert.Same(theme, FlourishThemeResources.FindThemeRoot(root));
        });
    }

    [Fact]
    public void CanonicalThemeResources_FindInGraphVisitsSharedDictionariesOnce()
    {
        RunInSta(() =>
        {
            var root = new ResourceDictionary();
            var left = new ResourceDictionary();
            var right = new ResourceDictionary();
            var shared = new ResourceDictionary();
            left.MergedDictionaries.Add(shared);
            right.MergedDictionaries.Add(shared);
            root.MergedDictionaries.Add(left);
            root.MergedDictionaries.Add(right);
            var visits = new Dictionary<ResourceDictionary, int>(
                ReferenceEqualityComparer.Instance
            );

            var result = FlourishThemeResources.FindInGraph(
                root,
                dictionary =>
                {
                    visits[dictionary] = visits.GetValueOrDefault(dictionary) + 1;
                    return false;
                }
            );

            Assert.Null(result);
            Assert.Equal(4, visits.Count);
            Assert.All(visits.Values, count => Assert.Equal(1, count));
            Assert.Equal(1, visits[shared]);
        });
    }

    [Fact]
    public void PublicVisualControls_UseTheirOwnDefaultStyleKeys()
    {
        RunInSta(() =>
        {
            foreach (var controlType in GetPublicFlourishControlTypes())
            {
                var control = Assert.IsAssignableFrom<FrameworkElement>(
                    Activator.CreateInstance(controlType)
                );
                Assert.Equal(controlType, GetDefaultStyleKey(control));
            }
        });
    }

    [Fact]
    public void SemanticControlProperties_ExposeStableDefaults()
    {
        RunInSta(() =>
        {
            var button = new FlourishButton();
            var iconButton = new IconButton();
            var windowCaptionButton = new WindowCaptionButton();
            var cardButton = new CardButton();
            var chunk = new Chunk();
            var chunkHero = new ChunkHero();
            var presenter = new Presenter();
            var paragraph = new Paragraph();
            var card = new Card();
            var iconCard = new IconCard();
            var listCard = new ListCard();
            var outputCard = new OutputCard();
            var overlay = new Overlay();
            var gridSplitter = new FlourishGridSplitter();
            var listBox = new FlourishListBox();
            var scrollViewer = new CustomScrollViewer();
            var search = new FlourishSearchBox();
            var text = new FlourishTextBlock();

            Assert.Equal(ButtonVariant.Outlined, button.Variant);
            Assert.Null(iconButton.Icon);
            Assert.IsAssignableFrom<FlourishButton>(iconButton);
            Assert.IsAssignableFrom<IconButton>(windowCaptionButton);
            Assert.Null(cardButton.Icon);
            Assert.Equal(Dock.Top, cardButton.IconPosition);
            Assert.Equal(string.Empty, cardButton.Title);
            Assert.IsAssignableFrom<FlourishButton>(cardButton);
            Assert.Equal(string.Empty, chunk.Title);
            Assert.Null(chunk.Description);
            Assert.Equal(new Thickness(0, 32, 0, 0), chunk.ChunkMargin);
            Assert.Equal(new Thickness(0, 12, 0, 0), chunk.ChunkSpacing);
            Assert.Null(chunk.Body);
            Assert.Equal(string.Empty, chunkHero.Title);
            Assert.Null(chunkHero.Description);
            Assert.Null(chunkHero.Body);
            Assert.Equal(PresenterMode.Split, chunkHero.PresenterMode);
            Assert.Equal(PresenterPosition.Right, chunkHero.PresenterPosition);
            Assert.Null(chunkHero.Presentation);
            Assert.IsAssignableFrom<Presenter>(chunkHero);
            Assert.Equal(string.Empty, presenter.Title);
            Assert.Null(presenter.Description);
            Assert.Null(presenter.Body);
            Assert.Null(presenter.Presentation);
            Assert.Equal(PresenterMode.Split, presenter.PresenterMode);
            Assert.Equal(PresenterPosition.Right, presenter.PresenterPosition);
            Assert.Empty(paragraph.Items);
            Assert.Equal(Variant.Standard, card.Variant);
            Assert.Equal(string.Empty, card.Title);
            Assert.Equal(string.Empty, card.MainText);
            Assert.Equal(HorizontalAlignment.Stretch, card.ContentHorizontalAlignment);
            Assert.Equal(VerticalAlignment.Stretch, card.ContentVerticalAlignment);
            Assert.Null(iconCard.Icon);
            Assert.Equal(Dock.Left, iconCard.IconPosition);
            Assert.IsAssignableFrom<Card>(iconCard);
            Assert.Null(listCard.Icon);
            Assert.Null(listCard.ActionBody);
            Assert.Equal(Variant.Standard, listCard.Variant);
            Assert.Equal(
                HorizontalAlignment.Stretch,
                listCard.ContentHorizontalAlignment
            );
            Assert.Equal(VerticalAlignment.Center, listCard.ContentVerticalAlignment);
            Assert.IsAssignableFrom<Card>(listCard);
            Assert.Equal(string.Empty, outputCard.Output);
            Assert.Equal(OverlayVariant.Temporary, overlay.Variant);
            Assert.Null(overlay.PlacementTarget);
            Assert.Equal(FlourishGridSplitterVariant.Standard, gridSplitter.Variant);
            Assert.Equal(FlourishListBoxAppearance.Standard, listBox.Appearance);
            Assert.False(listBox.IsCompact);
            Assert.True(scrollViewer.IsSmoothScrollingEnabled);
            Assert.False(scrollViewer.IsCompact);
            Assert.Equal(string.Empty, search.Placeholder);
            Assert.Equal(FlourishTextRole.Body, text.Role);
        });
    }

    [Fact]
    public void ButtonFamily_ExposesSixVariantsWithoutAppearanceOrPrefixedTypes()
    {
        Assert.Equal(
            new[] { "Elevated", "Filled", "Tonal", "Outlined", "Text", "Danger" },
            Enum.GetNames<ButtonVariant>()
        );
        Assert.NotNull(typeof(FlourishButton).GetProperty(nameof(FlourishButton.Variant)));
        Assert.Null(typeof(FlourishButton).GetProperty("Appearance"));

        var assembly = typeof(FlourishButton).Assembly;
        Assert.Null(assembly.GetType("ArkheideSystem.Flourish.Controls.ButtonAppearance"));
        Assert.Null(assembly.GetType("ArkheideSystem.Flourish.Controls.FlourishButton"));
        Assert.Null(assembly.GetType("ArkheideSystem.Flourish.Controls.FlourishIconButton"));
        Assert.Null(assembly.GetType("ArkheideSystem.Flourish.Controls.FlourishCardButton"));
        Assert.Null(
            assembly.GetType("ArkheideSystem.Flourish.Controls.FlourishWindowCaptionButton")
        );
    }

    [Fact]
    public void ButtonFamily_AddedDependencyPropertiesRoundTrip()
    {
        RunInSta(() =>
        {
            var icon = new Border();
            var iconButton = new IconButton
            {
                Icon = icon,
                Content = "Label",
                Variant = ButtonVariant.Text,
            };
            var captionButton = new WindowCaptionButton
            {
                Icon = "Caption",
                Variant = ButtonVariant.Danger,
            };
            var cardButton = new CardButton
            {
                Icon = icon,
                IconPosition = Dock.Right,
                Title = "Title",
                Content = "Description",
                Variant = ButtonVariant.Tonal,
            };

            Assert.Same(icon, iconButton.Icon);
            Assert.Equal("Label", iconButton.Content);
            Assert.Equal(ButtonVariant.Text, iconButton.Variant);
            Assert.Equal("Caption", captionButton.Icon);
            Assert.Equal(ButtonVariant.Danger, captionButton.Variant);
            Assert.Same(icon, cardButton.Icon);
            Assert.Equal(Dock.Right, cardButton.IconPosition);
            Assert.Equal("Title", cardButton.Title);
            Assert.Equal("Description", cardButton.Content);
            Assert.Equal(ButtonVariant.Tonal, cardButton.Variant);
        });
    }

    [Fact]
    public void CardFamily_ExposesVariantsAndDependencyPropertiesRoundTrip()
    {
        RunInSta(() =>
        {
            var card = new Card
            {
                Variant = Variant.Filled,
                Title = "Title",
                MainText = "Supporting text",
                ContentHorizontalAlignment = HorizontalAlignment.Right,
                ContentVerticalAlignment = VerticalAlignment.Bottom,
            };
            var iconCard = new IconCard
            {
                Icon = "\uE8A5",
                IconPosition = Dock.Bottom,
            };
            var actionBody = new FlourishButton();
            var listCard = new ListCard
            {
                Icon = "\uE790",
                ActionBody = actionBody,
                Variant = Variant.Filled,
                ContentHorizontalAlignment = HorizontalAlignment.Right,
                ContentVerticalAlignment = VerticalAlignment.Bottom,
            };

            Assert.Equal(
                new[] { "Elevated", "Standard", "Tonal", "Filled" },
                Enum.GetNames<Variant>()
            );
            Assert.Equal(Variant.Filled, card.Variant);
            Assert.Equal("Title", card.Title);
            Assert.Equal("Supporting text", card.MainText);
            Assert.Equal(HorizontalAlignment.Right, card.ContentHorizontalAlignment);
            Assert.Equal(VerticalAlignment.Bottom, card.ContentVerticalAlignment);
            Assert.Equal("\uE8A5", iconCard.Icon);
            Assert.Equal(Dock.Bottom, iconCard.IconPosition);
            Assert.Equal("\uE790", listCard.Icon);
            Assert.Same(actionBody, listCard.ActionBody);
            Assert.Equal(Variant.Standard, listCard.Variant);
            Assert.Equal(
                HorizontalAlignment.Stretch,
                listCard.ContentHorizontalAlignment
            );
            Assert.Equal(VerticalAlignment.Center, listCard.ContentVerticalAlignment);
            Assert.Equal(
                nameof(ListCard.ActionBody),
                typeof(ListCard).GetCustomAttribute<ContentPropertyAttribute>()?.Name
            );
            Assert.Null(typeof(Card).GetProperty("Text"));
            Assert.Null(typeof(Card).GetProperty("Body"));
            Assert.Null(typeof(Card).GetProperty("Content"));

            var assembly = typeof(Card).Assembly;
            Assert.Null(assembly.GetType("ArkheideSystem.Flourish.Controls.FlourishCard"));
            Assert.Null(
                assembly.GetType("ArkheideSystem.Flourish.Controls.FlourishCardAppearance")
            );
        });
    }

    [Fact]
    public void OutputCard_ExposesAppendOnlyHistoryWithoutCardCopyOrBodyProperties()
    {
        RunInSta(() =>
        {
            var outputCard = new OutputCard();

            Assert.True(OutputCard.OutputProperty.ReadOnly);
            Assert.Null(typeof(OutputCard).GetProperty(nameof(OutputCard.Output))?.SetMethod);
            Assert.False(typeof(Card).IsAssignableFrom(typeof(OutputCard)));
            Assert.Null(typeof(OutputCard).GetProperty(nameof(Card.Title)));
            Assert.Null(typeof(OutputCard).GetProperty(nameof(Card.MainText)));
            Assert.Null(typeof(OutputCard).GetProperty("Body"));

            outputCard.WriteLine("First message");
            outputCard.WriteLine();
            outputCard.WriteLine(null);
            outputCard.WriteLine("Last message");

            Assert.Equal(
                string.Join(
                    Environment.NewLine,
                    "First message",
                    string.Empty,
                    string.Empty,
                    "Last message"
                ),
                outputCard.Output
            );

            outputCard.Clear();

            Assert.Equal(string.Empty, outputCard.Output);

            outputCard.WriteLine(null);
            outputCard.WriteLine("After clear");

            Assert.Equal(Environment.NewLine + "After clear", outputCard.Output);
        });
    }

    [Fact]
    public void Overlay_ExposesTemporaryAndStrongDismissalVariants()
    {
        RunInSta(() =>
        {
            var target = new Border();
            var overlay = new Overlay
            {
                Content = "Details",
                PlacementTarget = target,
                Variant = OverlayVariant.Strong,
            };

            Assert.Equal(new[] { "Temporary", "Strong" }, Enum.GetNames<OverlayVariant>());
            Assert.Equal("Details", overlay.Content);
            Assert.Same(target, overlay.PlacementTarget);
            Assert.Equal(OverlayVariant.Strong, overlay.Variant);
            Assert.NotNull(Overlay.DismissRequestedEvent);
        });
    }

    [Fact]
    public void Overlay_TemporaryRequestsDismissalButStrongDoesNot()
    {
        RunInSta(() =>
        {
            var target = new Border();
            var overlay = new Overlay { PlacementTarget = target };
            var panel = new StackPanel { Children = { target, overlay } };
            var window = new Window { Content = panel };
            var dismissCount = 0;
            overlay.DismissRequested += (_, _) => dismissCount++;

            try
            {
                window.Show();
                window.UpdateLayout();
                RaiseMouseLeave(overlay);
                PumpDispatcher(TimeSpan.FromMilliseconds(180));
                Assert.Equal(1, dismissCount);

                overlay.Variant = OverlayVariant.Strong;
                RaiseMouseLeave(overlay);
                PumpDispatcher(TimeSpan.FromMilliseconds(180));
                Assert.Equal(1, dismissCount);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void OutputCard_CoalescesDependencyPropertyRefreshWhileKeepingOutputImmediatelyReadable()
    {
        RunInSta(() =>
        {
            var outputCard = new OutputCard();
            var outputDescriptor = DependencyPropertyDescriptor.FromProperty(
                OutputCard.OutputProperty,
                typeof(OutputCard)
            );
            Assert.NotNull(outputDescriptor);

            var outputChanges = 0;
            EventHandler handler = (_, _) => outputChanges++;
            outputDescriptor.AddValueChanged(outputCard, handler);

            try
            {
                var expectedLines = Enumerable.Range(0, 100)
                    .Select(index => $"Line {index:000}")
                    .ToArray();

                foreach (var line in expectedLines)
                {
                    outputCard.WriteLine(line);
                }

                var expected = string.Join(Environment.NewLine, expectedLines);
                Assert.Equal(expected, outputCard.Output);
                Assert.Equal(string.Empty, outputCard.GetValue(OutputCard.OutputProperty));
                Assert.Equal(0, outputChanges);

                outputCard.Dispatcher.Invoke(
                    DispatcherPriority.ApplicationIdle,
                    static () => { }
                );

                Assert.Equal(expected, outputCard.GetValue(OutputCard.OutputProperty));
                Assert.Equal(1, outputChanges);
            }
            finally
            {
                outputDescriptor.RemoveValueChanged(outputCard, handler);
            }
        });
    }

    [Fact]
    public void OutputCard_ClearInvalidatesAnOlderQueuedRefresh()
    {
        RunInSta(() =>
        {
            var outputCard = new OutputCard();

            outputCard.WriteLine("Discarded");
            outputCard.Clear();
            outputCard.WriteLine(null);
            outputCard.WriteLine("After clear");

            var expected = Environment.NewLine + "After clear";
            Assert.Equal(expected, outputCard.Output);

            outputCard.Dispatcher.Invoke(
                DispatcherPriority.ApplicationIdle,
                static () => { }
            );

            Assert.Equal(expected, outputCard.GetValue(OutputCard.OutputProperty));
        });
    }

    [Fact]
    public void CardFamily_DoesNotExposeNestedBodyContent()
    {
        Assert.All(
            new[] { typeof(Card), typeof(IconCard), typeof(ListCard) },
            type => Assert.Null(type.GetProperty("Body"))
        );
        Assert.Null(typeof(Card).GetCustomAttribute<ContentPropertyAttribute>());
        Assert.Null(typeof(IconCard).GetCustomAttribute<ContentPropertyAttribute>());
    }

    [Fact]
    public void CardIcons_AcceptOnlyOneTextGlyphAndCannotHostImagesOrComposedContent()
    {
        RunInSta(() =>
        {
            var iconCard = new IconCard { Icon = "\uE8A5" };
            var listCard = new ListCard { Icon = "\uE790" };

            Assert.Equal(typeof(string), IconCard.IconProperty.PropertyType);
            Assert.Equal(typeof(string), ListCard.IconProperty.PropertyType);
            Assert.Equal("\uE8A5", iconCard.Icon);
            Assert.Equal("\uE790", listCard.Icon);
            Assert.Throws<ArgumentException>(() => iconCard.Icon = "\uE8A5\uE790");
            Assert.Throws<ArgumentException>(() => listCard.Icon = "AB");
            Assert.Throws<ArgumentException>(() =>
                iconCard.SetValue(IconCard.IconProperty, new Image())
            );
            Assert.Throws<ArgumentException>(() =>
                listCard.SetValue(ListCard.IconProperty, new StackPanel())
            );
        });
    }

    [Fact]
    public void ListCard_OwnsIconAndActionLogicalContentBeforeAndAfterReplacement()
    {
        RunInSta(() =>
        {
            var dataContext = new object();
            var firstAction = new FlourishButton();
            var replacementAction = new FlourishButton();
            var card = new ListCard
            {
                DataContext = dataContext,
                Icon = "\uE8A5",
                ActionBody = firstAction,
            };
            card.Resources["CardResource"] = "Available";

            Assert.Same(card, LogicalTreeHelper.GetParent(firstAction));
            Assert.Same(dataContext, firstAction.DataContext);
            Assert.Equal("Available", firstAction.FindResource("CardResource"));
            Assert.Equal(
                new object[] { firstAction },
                LogicalTreeHelper.GetChildren(card).Cast<object>()
            );

            card.ActionBody = replacementAction;

            Assert.Null(LogicalTreeHelper.GetParent(firstAction));
            Assert.Same(card, LogicalTreeHelper.GetParent(replacementAction));

            card.ClearValue(ListCard.IconProperty);
            card.ClearValue(ListCard.ActionBodyProperty);

            Assert.Null(LogicalTreeHelper.GetParent(replacementAction));
            Assert.Empty(LogicalTreeHelper.GetChildren(card).Cast<object>());
        });
    }

    [Fact]
    public void ChunkFamily_DependencyPropertiesRoundTripAndChunkOwnsImplicitContent()
    {
        RunInSta(() =>
        {
            var chunkBody = new Border();
            var heroBody = new StackPanel();
            var presentation = new Border();
            var chunk = new Chunk
            {
                Title = "Section",
                Description = "Supporting copy",
                ChunkMargin = new Thickness(1, 2, 3, 4),
                ChunkSpacing = new Thickness(5, 6, 7, 8),
                Body = chunkBody,
            };
            var hero = new ChunkHero
            {
                Title = "Hero",
                Description = "Leading copy",
                Body = heroBody,
                PresenterMode = PresenterMode.Overlay,
                PresenterPosition = PresenterPosition.Left,
                Presentation = presentation,
            };

            Assert.Equal("Section", chunk.Title);
            Assert.Equal("Supporting copy", chunk.Description);
            Assert.Equal(new Thickness(1, 2, 3, 4), chunk.ChunkMargin);
            Assert.Equal(new Thickness(5, 6, 7, 8), chunk.ChunkSpacing);
            Assert.Same(chunkBody, chunk.Body);
            Assert.Equal(
                nameof(Chunk.Body),
                typeof(Chunk).GetCustomAttribute<ContentPropertyAttribute>()?.Name
            );
            Assert.Equal("Hero", hero.Title);
            Assert.Equal("Leading copy", hero.Description);
            Assert.Same(heroBody, hero.Body);
            Assert.Equal(PresenterMode.Overlay, hero.PresenterMode);
            Assert.Equal(PresenterPosition.Left, hero.PresenterPosition);
            Assert.Same(presentation, hero.Presentation);
            Assert.Equal(
                nameof(ChunkHero.Body),
                typeof(ChunkHero).GetCustomAttribute<ContentPropertyAttribute>()?.Name
            );
        });
    }

    [Fact]
    public void ChunkFamily_OwnsLogicalContentBeforeAndAfterReplacement()
    {
        RunInSta(() =>
        {
            var dataContext = new object();
            var firstBody = new Border();
            var replacementBody = new Border();
            var chunk = new Chunk { DataContext = dataContext };
            chunk.Resources["ChunkResource"] = "Available";

            chunk.Body = firstBody;

            Assert.Same(chunk, LogicalTreeHelper.GetParent(firstBody));
            Assert.Same(dataContext, firstBody.DataContext);
            Assert.Equal("Available", firstBody.FindResource("ChunkResource"));

            chunk.Body = replacementBody;

            Assert.Null(LogicalTreeHelper.GetParent(firstBody));
            Assert.Same(chunk, LogicalTreeHelper.GetParent(replacementBody));

            chunk.ClearValue(Chunk.BodyProperty);

            Assert.Null(LogicalTreeHelper.GetParent(replacementBody));

            var presenterBody = new Border();
            var presentation = new Border();
            var presenter = new Presenter
            {
                DataContext = dataContext,
                Body = presenterBody,
                Presentation = presentation,
            };

            Assert.Same(presenter, LogicalTreeHelper.GetParent(presenterBody));
            Assert.Same(presenter, LogicalTreeHelper.GetParent(presentation));
            Assert.Same(dataContext, presenterBody.DataContext);
            Assert.Same(dataContext, presentation.DataContext);
            Assert.Equal(
                new object[] { presenterBody, presentation },
                LogicalTreeHelper.GetChildren(presenter).Cast<object>()
            );

            presenter.Body = null;
            presenter.Presentation = null;

            Assert.Null(LogicalTreeHelper.GetParent(presenterBody));
            Assert.Null(LogicalTreeHelper.GetParent(presentation));
            Assert.Empty(LogicalTreeHelper.GetChildren(presenter).Cast<object>());
        });
    }

    [Fact]
    public void PresenterAndParagraph_ExposeContentContractsAndParagraphOwnsItsItems()
    {
        RunInSta(() =>
        {
            Assert.Equal(
                nameof(Presenter.Presentation),
                typeof(Presenter).GetCustomAttribute<ContentPropertyAttribute>()?.Name
            );
            Assert.Equal(
                nameof(ChunkHero.Body),
                typeof(ChunkHero).GetCustomAttribute<ContentPropertyAttribute>()?.Name
            );
            Assert.Equal(
                nameof(ItemsControl.Items),
                typeof(Paragraph).GetCustomAttribute<ContentPropertyAttribute>()?.Name
            );

            var dataContext = new object();
            var firstParagraph = new FlourishTextBlock { Text = "First" };
            var secondParagraph = new FlourishTextBlock { Text = "Second" };
            var paragraph = new Paragraph { DataContext = dataContext };
            paragraph.Resources["ParagraphResource"] = "Available";

            paragraph.Items.Add(firstParagraph);
            paragraph.Items.Add(secondParagraph);

            Assert.Same(paragraph, LogicalTreeHelper.GetParent(firstParagraph));
            Assert.Same(dataContext, firstParagraph.DataContext);
            Assert.Equal("Available", firstParagraph.FindResource("ParagraphResource"));
            Assert.Equal(
                new object[] { firstParagraph, secondParagraph },
                LogicalTreeHelper.GetChildren(paragraph).Cast<object>()
            );

            paragraph.Items.Remove(firstParagraph);

            Assert.Null(LogicalTreeHelper.GetParent(firstParagraph));
            Assert.Equal(
                new object[] { secondParagraph },
                LogicalTreeHelper.GetChildren(paragraph).Cast<object>()
            );
        });
    }

    [Fact]
    public void Presenter_ImplicitXamlContentTargetsPresentation()
    {
        RunInSta(() =>
        {
            const string xaml = """
                <flourish:Presenter
                  xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                  xmlns:flourish="http://schemas.arkheide.system/flourish"
                  Description="Supporting copy"
                  PresenterMode="Split"
                  PresenterPosition="Right"
                  Title="Example">
                  <Border />
                </flourish:Presenter>
                """;

            var presenter = Assert.IsType<Presenter>(XamlReader.Parse(xaml));

            Assert.Null(presenter.Body);
            Assert.IsType<Border>(presenter.Presentation);
            Assert.Equal(PresenterMode.Split, presenter.PresenterMode);
            Assert.Equal(PresenterPosition.Right, presenter.PresenterPosition);
        });
    }

    [Fact]
    public void IconButton_SimpleToolTipContentUsesFlourishToolTip()
    {
        RunInSta(() =>
        {
            var explicitToolTip = new FlourishToolTip { Content = "Explicit" };
            var nativeToolTip = new ToolTip { Content = "Native" };
            var iconButton = new IconButton { ToolTip = "Refresh" };
            var captionButton = new WindowCaptionButton { ToolTip = "Close" };
            var explicitButton = new IconButton { ToolTip = explicitToolTip };
            var nativeButton = new IconButton { ToolTip = nativeToolTip };

            var iconToolTip = Assert.IsType<FlourishToolTip>(iconButton.ToolTip);
            Assert.Equal("Refresh", iconToolTip.Content);
            Assert.Equal(
                "Close",
                Assert.IsType<FlourishToolTip>(captionButton.ToolTip).Content
            );
            Assert.Same(explicitToolTip, explicitButton.ToolTip);
            Assert.Same(nativeToolTip, nativeButton.ToolTip);
        });
    }

    [Fact]
    public void SemanticEnumProperties_RejectUndefinedValues()
    {
        RunInSta(() =>
        {
            var button = new FlourishButton();

            Assert.Equal(new[] { "Left", "Right" }, Enum.GetNames<PresenterPosition>());
            Assert.Throws<ArgumentException>(() => button.Variant = (ButtonVariant)(-1));
            Assert.Throws<ArgumentException>(() =>
                new Presenter().PresenterMode = (PresenterMode)(-1)
            );
            Assert.Throws<ArgumentException>(() =>
                new Presenter().PresenterPosition = (PresenterPosition)(-1)
            );
            Assert.Throws<ArgumentException>(() =>
                new ChunkHero().PresenterPosition = (PresenterPosition)2
            );
            Assert.Throws<ArgumentException>(() =>
                new CardButton().IconPosition = (Dock)(-1)
            );
            Assert.Throws<ArgumentException>(() =>
                new Card().Variant = (Variant)(-1)
            );
            Assert.Throws<ArgumentException>(() =>
                new Card().ContentHorizontalAlignment = (HorizontalAlignment)(-1)
            );
            Assert.Throws<ArgumentException>(() =>
                new Card().ContentVerticalAlignment = (VerticalAlignment)(-1)
            );
            Assert.Throws<ArgumentException>(() =>
                new IconCard().IconPosition = (Dock)(-1)
            );
            Assert.Throws<ArgumentException>(() =>
                new FlourishGridSplitter().Variant = (FlourishGridSplitterVariant)(-1)
            );
            Assert.Throws<ArgumentException>(() =>
                new FlourishListBox().Appearance = (FlourishListBoxAppearance)(-1)
            );
            Assert.Throws<ArgumentException>(() =>
                new FlourishTextBlock().Role = (FlourishTextRole)(-1)
            );
        });
    }

    [Fact]
    public void HoverReveal_AttachedPropertiesExposeStableDefaultsAndRoundTrip()
    {
        RunInSta(() =>
        {
            var element = new Border();
            var duration = TimeSpan.FromMilliseconds(215);
            var overrideColor = new SolidColorBrush(Colors.Red);

            Assert.True(HoverReveal.GetIsEnabled(element));
            Assert.True(HoverReveal.GetIsMotionEnabled(element));
            Assert.False(HoverReveal.GetIsParticipant(element));
            Assert.False(HoverReveal.GetTemplateHandlesInteraction(element));
            Assert.Equal(
                TimeSpan.FromMilliseconds(140),
                HoverReveal.GetAnimationDuration(element)
            );
            Assert.Null(HoverReveal.GetOverrideColor(element));

            HoverReveal.SetIsEnabled(element, false);
            HoverReveal.SetIsMotionEnabled(element, false);
            HoverReveal.SetIsParticipant(element, true);
            HoverReveal.SetTemplateHandlesInteraction(element, true);
            HoverReveal.SetAnimationDuration(element, duration);
            HoverReveal.SetOverrideColor(element, overrideColor);

            Assert.False(HoverReveal.GetIsEnabled(element));
            Assert.False(HoverReveal.GetIsMotionEnabled(element));
            Assert.True(HoverReveal.GetIsParticipant(element));
            Assert.True(HoverReveal.GetTemplateHandlesInteraction(element));
            Assert.Equal(duration, HoverReveal.GetAnimationDuration(element));
            Assert.Same(overrideColor, HoverReveal.GetOverrideColor(element));
        });
    }

    [Fact]
    public void HoverReveal_MotionPolicyCoercesTheEffectiveStateWithoutInheriting()
    {
        RunInSta(() =>
        {
            var parent = new Grid();
            var child = new Border();
            parent.Children.Add(child);

            HoverReveal.SetIsMotionEnabled(parent, false);

            Assert.False(HoverReveal.GetIsEnabled(parent));
            Assert.True(HoverReveal.GetIsMotionEnabled(child));

            HoverReveal.SetIsMotionEnabled(parent, true);
            Assert.True(HoverReveal.GetIsEnabled(parent));

            HoverReveal.SetIsEnabled(parent, false);
            Assert.False(HoverReveal.GetIsEnabled(parent));
            HoverReveal.SetIsMotionEnabled(parent, true);
            Assert.False(HoverReveal.GetIsEnabled(parent));
        });
    }

    [Fact]
    public void HoverReveal_PolicyInheritsWithoutOptingDescendantsIntoTheBehavior()
    {
        RunInSta(() =>
        {
            var parent = new Grid();
            var child = new WpfButton();
            parent.Children.Add(child);

            HoverReveal.SetIsEnabled(parent, false);
            HoverReveal.SetAnimationDuration(parent, TimeSpan.FromMilliseconds(90));
            HoverReveal.SetIsParticipant(parent, true);

            Assert.False(HoverReveal.GetIsEnabled(child));
            Assert.Equal(
                TimeSpan.FromMilliseconds(90),
                HoverReveal.GetAnimationDuration(child)
            );
            Assert.False(HoverReveal.GetIsParticipant(child));
        });
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

    private static void AssertPublicStaticMethod(Type declaringType, string methodName)
    {
        Assert.True(
            declaringType
                .GetMethod(methodName, BindingFlags.Public | BindingFlags.Static)
                ?.IsPublic,
            methodName
        );
    }

    private static void RaiseMouseLeave(UIElement element)
    {
        element.RaiseEvent(
            new System.Windows.Input.MouseEventArgs(
                System.Windows.Input.Mouse.PrimaryDevice,
                Environment.TickCount
            )
            {
                RoutedEvent = System.Windows.Input.Mouse.MouseLeaveEvent,
            }
        );
    }

    private static void PumpDispatcher(TimeSpan duration)
    {
        var frame = new DispatcherFrame();
        var timer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = duration,
        };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            frame.Continue = false;
        };
        timer.Start();
        Dispatcher.PushFrame(frame);
    }

    private static object? GetDefaultStyleKey(FrameworkElement element)
    {
        var property = typeof(FrameworkElement).GetProperty(
            "DefaultStyleKey",
            BindingFlags.Instance | BindingFlags.NonPublic
        );

        return Assert.IsAssignableFrom<PropertyInfo>(property).GetValue(element);
    }

    private static void RunInSta(Action action)
    {
        Exception? error = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception exception)
            {
                error = exception;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (error is not null)
        {
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(error).Throw();
        }
    }
}
