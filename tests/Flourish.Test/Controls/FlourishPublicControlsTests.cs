using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
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
            var card = new Card();
            var iconCard = new IconCard();
            var listCard = new ListCard();
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
            Assert.Equal(string.Empty, chunk.ChunkTitle);
            Assert.Null(chunk.ChunkDescription);
            Assert.Equal(new Thickness(0, 32, 0, 0), chunk.ChunkMargin);
            Assert.Equal(new Thickness(0, 12, 0, 0), chunk.ChunkSpacing);
            Assert.Null(chunk.ChunkBody);
            Assert.Equal(string.Empty, chunkHero.ChunkHeroTitle);
            Assert.Null(chunkHero.ChunkHeroDescription);
            Assert.Null(chunkHero.ChunkHeroBody);
            Assert.Equal(PresenterMode.Split, chunkHero.PresenterMode);
            Assert.Equal(PresenterPosition.Right, chunkHero.PresenterPosition);
            Assert.Null(chunkHero.Presenter);
            Assert.Equal(Variant.Standard, card.Variant);
            Assert.Equal(string.Empty, card.Title);
            Assert.Equal(string.Empty, card.Text);
            Assert.Null(card.Body);
            Assert.Equal(HorizontalAlignment.Stretch, card.ContentHorizontalAlignment);
            Assert.Equal(VerticalAlignment.Stretch, card.ContentVerticalAlignment);
            Assert.Null(iconCard.Presenter);
            Assert.Equal(PresenterMode.Split, iconCard.PresenterMode);
            Assert.Equal(PresenterPosition.Left, iconCard.PresenterPosition);
            Assert.IsAssignableFrom<Card>(iconCard);
            Assert.Null(listCard.Presenter);
            Assert.Equal(Variant.Standard, listCard.Variant);
            Assert.Equal(
                HorizontalAlignment.Stretch,
                listCard.ContentHorizontalAlignment
            );
            Assert.Equal(VerticalAlignment.Center, listCard.ContentVerticalAlignment);
            Assert.IsAssignableFrom<Card>(listCard);
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
            var content = new Border();
            var presenter = new Border();
            var card = new Card
            {
                Variant = Variant.Filled,
                Title = "Title",
                Text = "Supporting text",
                Body = content,
                ContentHorizontalAlignment = HorizontalAlignment.Right,
                ContentVerticalAlignment = VerticalAlignment.Bottom,
            };
            var iconCard = new IconCard
            {
                Presenter = presenter,
                PresenterMode = PresenterMode.Overlay,
                PresenterPosition = PresenterPosition.RightBottom,
            };
            var listPresenter = new Border();
            var listCard = new ListCard
            {
                Presenter = listPresenter,
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
            Assert.Equal("Supporting text", card.Text);
            Assert.Same(content, card.Body);
            Assert.Equal(HorizontalAlignment.Right, card.ContentHorizontalAlignment);
            Assert.Equal(VerticalAlignment.Bottom, card.ContentVerticalAlignment);
            Assert.Same(presenter, iconCard.Presenter);
            Assert.Equal(PresenterMode.Overlay, iconCard.PresenterMode);
            Assert.Equal(PresenterPosition.RightBottom, iconCard.PresenterPosition);
            Assert.Same(listPresenter, listCard.Presenter);
            Assert.Equal(Variant.Standard, listCard.Variant);
            Assert.Equal(
                HorizontalAlignment.Stretch,
                listCard.ContentHorizontalAlignment
            );
            Assert.Equal(VerticalAlignment.Center, listCard.ContentVerticalAlignment);
            Assert.Null(typeof(ListCard).GetProperty(nameof(IconCard.PresenterMode)));
            Assert.Null(typeof(ListCard).GetProperty(nameof(IconCard.PresenterPosition)));
            Assert.Equal(
                nameof(Card.Body),
                typeof(Card).GetCustomAttribute<ContentPropertyAttribute>()?.Name
            );
            Assert.Null(typeof(Card).GetProperty("Content"));

            var assembly = typeof(Card).Assembly;
            Assert.Null(assembly.GetType("ArkheideSystem.Flourish.Controls.FlourishCard"));
            Assert.Null(
                assembly.GetType("ArkheideSystem.Flourish.Controls.FlourishCardAppearance")
            );
        });
    }

    [Fact]
    public void Card_OwnsBodyLogicalContentBeforeAndAfterReplacement()
    {
        RunInSta(() =>
        {
            var dataContext = new object();
            var firstBody = new Border();
            var replacementBody = new Border();
            var card = new Card { DataContext = dataContext, Body = firstBody };
            card.Resources["CardResource"] = "Available";

            Assert.Same(card, LogicalTreeHelper.GetParent(firstBody));
            Assert.Same(dataContext, firstBody.DataContext);
            Assert.Equal("Available", firstBody.FindResource("CardResource"));
            Assert.Equal(
                new object[] { firstBody },
                LogicalTreeHelper.GetChildren(card).Cast<object>()
            );

            card.Body = replacementBody;

            Assert.Null(LogicalTreeHelper.GetParent(firstBody));
            Assert.Same(card, LogicalTreeHelper.GetParent(replacementBody));

            card.ClearValue(Card.BodyProperty);

            Assert.Null(LogicalTreeHelper.GetParent(replacementBody));
            Assert.Empty(LogicalTreeHelper.GetChildren(card).Cast<object>());
        });
    }

    [Fact]
    public void IconCard_OwnsPresenterLogicalContentBeforeAndAfterReplacement()
    {
        RunInSta(() =>
        {
            var dataContext = new object();
            var body = new Border();
            var firstPresenter = new Border();
            var replacementPresenter = new Border();
            var card = new IconCard
            {
                DataContext = dataContext,
                Body = body,
                Presenter = firstPresenter,
            };
            card.Resources["CardResource"] = "Available";

            Assert.Same(card, LogicalTreeHelper.GetParent(body));
            Assert.Same(card, LogicalTreeHelper.GetParent(firstPresenter));
            Assert.Same(dataContext, firstPresenter.DataContext);
            Assert.Equal("Available", firstPresenter.FindResource("CardResource"));
            Assert.Equal(
                new object[] { body, firstPresenter },
                LogicalTreeHelper.GetChildren(card).Cast<object>()
            );

            card.Presenter = replacementPresenter;

            Assert.Null(LogicalTreeHelper.GetParent(firstPresenter));
            Assert.Same(card, LogicalTreeHelper.GetParent(replacementPresenter));

            card.ClearValue(IconCard.PresenterProperty);

            Assert.Null(LogicalTreeHelper.GetParent(replacementPresenter));
            Assert.Equal(
                new object[] { body },
                LogicalTreeHelper.GetChildren(card).Cast<object>()
            );
        });
    }

    [Fact]
    public void ListCard_OwnsPresenterLogicalContentBeforeAndAfterReplacement()
    {
        RunInSta(() =>
        {
            var dataContext = new object();
            var body = new Border();
            var firstPresenter = new Border();
            var replacementPresenter = new Border();
            var card = new ListCard
            {
                DataContext = dataContext,
                Body = body,
                Presenter = firstPresenter,
            };
            card.Resources["CardResource"] = "Available";

            Assert.Same(card, LogicalTreeHelper.GetParent(body));
            Assert.Same(card, LogicalTreeHelper.GetParent(firstPresenter));
            Assert.Same(dataContext, firstPresenter.DataContext);
            Assert.Equal("Available", firstPresenter.FindResource("CardResource"));
            Assert.Equal(
                new object[] { body, firstPresenter },
                LogicalTreeHelper.GetChildren(card).Cast<object>()
            );

            card.Presenter = replacementPresenter;

            Assert.Null(LogicalTreeHelper.GetParent(firstPresenter));
            Assert.Same(card, LogicalTreeHelper.GetParent(replacementPresenter));

            card.ClearValue(ListCard.PresenterProperty);

            Assert.Null(LogicalTreeHelper.GetParent(replacementPresenter));
            Assert.Equal(
                new object[] { body },
                LogicalTreeHelper.GetChildren(card).Cast<object>()
            );
        });
    }

    [Fact]
    public void ChunkFamily_DependencyPropertiesRoundTripAndChunkOwnsImplicitContent()
    {
        RunInSta(() =>
        {
            var chunkBody = new Border();
            var heroBody = new StackPanel();
            var presenter = new Border();
            var chunk = new Chunk
            {
                ChunkTitle = "Section",
                ChunkDescription = "Supporting copy",
                ChunkMargin = new Thickness(1, 2, 3, 4),
                ChunkSpacing = new Thickness(5, 6, 7, 8),
                ChunkBody = chunkBody,
            };
            var hero = new ChunkHero
            {
                ChunkHeroTitle = "Hero",
                ChunkHeroDescription = "Leading copy",
                ChunkHeroBody = heroBody,
                PresenterMode = PresenterMode.Overlay,
                PresenterPosition = PresenterPosition.Left,
                Presenter = presenter,
            };

            Assert.Equal("Section", chunk.ChunkTitle);
            Assert.Equal("Supporting copy", chunk.ChunkDescription);
            Assert.Equal(new Thickness(1, 2, 3, 4), chunk.ChunkMargin);
            Assert.Equal(new Thickness(5, 6, 7, 8), chunk.ChunkSpacing);
            Assert.Same(chunkBody, chunk.ChunkBody);
            Assert.Equal(
                nameof(Chunk.ChunkBody),
                typeof(Chunk).GetCustomAttribute<ContentPropertyAttribute>()?.Name
            );
            Assert.Equal("Hero", hero.ChunkHeroTitle);
            Assert.Equal("Leading copy", hero.ChunkHeroDescription);
            Assert.Same(heroBody, hero.ChunkHeroBody);
            Assert.Equal(PresenterMode.Overlay, hero.PresenterMode);
            Assert.Equal(PresenterPosition.Left, hero.PresenterPosition);
            Assert.Same(presenter, hero.Presenter);
            Assert.Equal(
                nameof(ChunkHero.ChunkHeroBody),
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

            chunk.ChunkBody = firstBody;

            Assert.Same(chunk, LogicalTreeHelper.GetParent(firstBody));
            Assert.Same(dataContext, firstBody.DataContext);
            Assert.Equal("Available", firstBody.FindResource("ChunkResource"));

            chunk.ChunkBody = replacementBody;

            Assert.Null(LogicalTreeHelper.GetParent(firstBody));
            Assert.Same(chunk, LogicalTreeHelper.GetParent(replacementBody));

            chunk.ClearValue(Chunk.ChunkBodyProperty);

            Assert.Null(LogicalTreeHelper.GetParent(replacementBody));

            var heroBody = new Border();
            var heroPresenter = new Border();
            var hero = new ChunkHero
            {
                DataContext = dataContext,
                ChunkHeroBody = heroBody,
                Presenter = heroPresenter,
            };

            Assert.Same(hero, LogicalTreeHelper.GetParent(heroBody));
            Assert.Same(hero, LogicalTreeHelper.GetParent(heroPresenter));
            Assert.Same(dataContext, heroBody.DataContext);
            Assert.Same(dataContext, heroPresenter.DataContext);
            Assert.Equal(
                new object[] { heroBody, heroPresenter },
                LogicalTreeHelper.GetChildren(hero).Cast<object>()
            );

            hero.ChunkHeroBody = null;
            hero.Presenter = null;

            Assert.Null(LogicalTreeHelper.GetParent(heroBody));
            Assert.Null(LogicalTreeHelper.GetParent(heroPresenter));
            Assert.Empty(LogicalTreeHelper.GetChildren(hero).Cast<object>());
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

            Assert.Throws<ArgumentException>(() => button.Variant = (ButtonVariant)(-1));
            Assert.Throws<ArgumentException>(() =>
                new ChunkHero().PresenterMode = (PresenterMode)(-1)
            );
            Assert.Throws<ArgumentException>(() =>
                new ChunkHero().PresenterPosition = PresenterPosition.Top
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
                new IconCard().PresenterMode = (PresenterMode)(-1)
            );
            Assert.Throws<ArgumentException>(() =>
                new IconCard().PresenterPosition = (PresenterPosition)(-1)
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
