using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using ArkheideSystem.Flourish.Controls;
using CustomScrollViewer = ArkheideSystem.Flourish.Controls.ScrollViewer;
using FlourishButton = ArkheideSystem.Flourish.Controls.Button;
using WpfButton = System.Windows.Controls.Button;
using WpfControl = System.Windows.Controls.Control;
using WpfScrollViewer = System.Windows.Controls.ScrollViewer;

namespace ArkheideSystem.Flourish.Test.Controls;

public sealed class FlourishControlStylesTests
{
    private const string GenericThemeSource =
        "/Flourish;component/Themes/Generic.xaml";

    public static TheoryData<string> CanonicalResourceDictionaries =>
        new()
        {
            GenericThemeSource,
            "/Flourish;component/Themes/Layout.xaml",
            "/Flourish;component/Themes/Typography.xaml",
            "/Flourish;component/Themes/Colors/Colors.xaml",
            "/Flourish;component/Themes/Colors/Colors.Light.xaml",
            "/Flourish;component/Themes/Colors/Colors.Dark.xaml",
            "/Flourish;component/Themes/Controls.xaml",
            "/Flourish;component/Controls/Button.xaml",
            "/Flourish;component/Controls/IconButton.xaml",
            "/Flourish;component/Controls/WindowCaptionButton.xaml",
            "/Flourish;component/Controls/CardButton.xaml",
            "/Flourish;component/Controls/Chunk.xaml",
            "/Flourish;component/Controls/ChunkHero.xaml",
            "/Flourish;component/Controls/Card.xaml",
            "/Flourish;component/Controls/IconCard.xaml",
            "/Flourish;component/Controls/ListCard.xaml",
            "/Flourish;component/Controls/OutputCard.xaml",
            "/Flourish;component/Controls/Overlay.xaml",
            "/Flourish;component/Controls/CheckBox.xaml",
            "/Flourish;component/Controls/ComboBox.xaml",
            "/Flourish;component/Controls/ComboBoxItem.xaml",
            "/Flourish;component/Controls/GridSplitter.xaml",
            "/Flourish;component/Controls/Label.xaml",
            "/Flourish;component/Controls/ListBox.xaml",
            "/Flourish;component/Controls/ListBoxItem.xaml",
            "/Flourish;component/Controls/PasswordBox.xaml",
            "/Flourish;component/Controls/RadioButton.xaml",
            "/Flourish;component/Controls/ScrollBar.xaml",
            "/Flourish;component/Controls/ScrollViewer.xaml",
            "/Flourish;component/Controls/SearchBox.xaml",
            "/Flourish;component/Controls/TextBlock.xaml",
            "/Flourish;component/Controls/TextBox.xaml",
            "/Flourish;component/Controls/ToolTip.xaml",
        };

    [Theory]
    [MemberData(nameof(CanonicalResourceDictionaries))]
    public void CanonicalResourceDictionary_Loads(string source)
    {
        RunInSta(() => Assert.IsType<ResourceDictionary>(LoadResourceDictionary(source)));
    }

    [Fact]
    public void GenericTheme_ProvidesAnExactDefaultStyleForEveryFlourishControl()
    {
        RunInSta(() =>
        {
            var resources = LoadResourceDictionary(GenericThemeSource);

            foreach (var controlType in GetPublicFlourishControlTypes())
            {
                var style = Assert.IsType<Style>(resources[controlType]);
                Assert.Equal(controlType, style.TargetType);
            }
        });
    }

    [Fact]
    public void TextRoles_ResolveCanonicalSizeLineHeightBottomSpaceAndWeightMetrics()
    {
        RunInSta(() =>
        {
            var textBlocks = Enum.GetValues<FlourishTextRole>()
                .Select(role => new FlourishTextBlock { Role = role, Text = role.ToString() })
                .ToArray();
            var panel = new StackPanel();
            foreach (var textBlock in textBlocks)
            {
                panel.Children.Add(textBlock);
            }

            var window = CreateWindow(panel);
            try
            {
                window.Show();
                window.UpdateLayout();

                foreach (var textBlock in textBlocks)
                {
                    var (expectedSize, expectedLineHeight, expectedBottomSpace, expectedWeight) =
                        textBlock.Role switch
                        {
                            FlourishTextRole.Caption or FlourishTextRole.Status =>
                                (12d, 14d, 1d, FontWeights.Regular),
                            FlourishTextRole.Icon =>
                                (16d, 16d, 0d, FontWeights.Regular),
                            FlourishTextRole.CardTitle =>
                                (16d, 20d, 2d, FontWeights.Bold),
                            FlourishTextRole.SectionTitle =>
                                (24d, 29d, 3d, FontWeights.Bold),
                            FlourishTextRole.PageTitle =>
                                (32d, 37d, 4d, FontWeights.Bold),
                            _ => (14d, 16d, 1d, FontWeights.Regular),
                        };
                    Assert.Equal(expectedSize, textBlock.FontSize);
                    Assert.Equal(expectedLineHeight, textBlock.LineHeight);
                    Assert.Equal(new Thickness(0, 0, 0, expectedBottomSpace), textBlock.Padding);
                    Assert.Equal(expectedWeight, textBlock.FontWeight);
                    Assert.Equal(LineStackingStrategy.BlockLineHeight, textBlock.LineStackingStrategy);
                    Assert.Equal(
                        textBlock.Role == FlourishTextRole.Icon
                            ? "Segoe MDL2 Assets"
                            : "Segoe UI",
                        textBlock.FontFamily.Source
                    );
                    Assert.Equal(
                        expectedLineHeight + expectedBottomSpace,
                        textBlock.ActualHeight,
                        precision: 3
                    );
                }
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void IconGlyphHosts_ResolveTheDedicatedIconFamilyAndSizeAtRuntime()
    {
        RunInSta(() =>
        {
            var iconText = new FlourishTextBlock
            {
                Role = FlourishTextRole.Icon,
                Text = "\uE10F",
            };
            var iconButton = new IconButton { Icon = "\uE10F" };
            var iconCard = new IconCard { Title = "Icon card", Presenter = "\uE8A5" };
            var listCard = new ListCard { Title = "List card", Presenter = "\uE8A5" };
            var panel = new StackPanel();
            panel.Children.Add(iconText);
            panel.Children.Add(iconButton);
            panel.Children.Add(iconCard);
            panel.Children.Add(listCard);

            var window = CreateWindow(panel);
            try
            {
                window.Show();
                window.UpdateLayout();

                Assert.Equal("Segoe MDL2 Assets", iconText.FontFamily.Source);
                Assert.Equal(16d, iconText.FontSize);
                Assert.Equal(16d, iconText.LineHeight);
                Assert.Equal(new Thickness(), iconText.Padding);

                foreach (var (control, partName) in new (WpfControl Control, string PartName)[]
                {
                    (iconButton, "IconHost"),
                    (iconCard, "PresenterHost"),
                    (listCard, "PresenterHost"),
                })
                {
                    control.ApplyTemplate();
                    var host = AssertTemplatePart<ContentPresenter>(control, partName);
                    var fontFamily = Assert.IsType<FontFamily>(
                        host.GetValue(TextElement.FontFamilyProperty)
                    );
                    var fontSize = Assert.IsType<double>(
                        host.GetValue(TextElement.FontSizeProperty)
                    );
                    Assert.Equal("Segoe MDL2 Assets", fontFamily.Source);
                    Assert.Equal(16d, fontSize);
                }
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void CardChunkAndChunkHero_TitleHostsUseTheirExplicitHeadingTiers()
    {
        RunInSta(() =>
        {
            WpfControl[] controls =
            [
                new Card { Title = "Card" },
                new Chunk { ChunkTitle = "Section" },
                new ChunkHero { ChunkHeroTitle = "Hero" },
            ];
            var panel = new StackPanel();
            foreach (var control in controls)
            {
                panel.Children.Add(control);
            }

            var window = CreateWindow(panel);
            try
            {
                window.Show();
                window.UpdateLayout();

                controls[0].ApplyTemplate();
                var cardTitle = AssertTemplatePart<FlourishTextBlock>(controls[0], "TitleHost");
                Assert.Equal(16d, cardTitle.FontSize);
                Assert.Equal(FontWeights.Bold, cardTitle.FontWeight);

                controls[1].ApplyTemplate();
                var chunkTitle = AssertTemplatePart<FlourishTextBlock>(controls[1], "TitleHost");
                Assert.Equal(24d, chunkTitle.FontSize);
                Assert.Equal(FontWeights.Bold, chunkTitle.FontWeight);

                controls[2].ApplyTemplate();
                var heroTitle = AssertTemplatePart<FlourishTextBlock>(controls[2], "TitleHost");
                Assert.Equal(32d, heroTitle.FontSize);
                Assert.Equal(FontWeights.Bold, heroTitle.FontWeight);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void GenericTheme_DoesNotOverrideNativeWpfControlStyles()
    {
        RunInSta(() =>
        {
            var resources = LoadResourceDictionary(GenericThemeSource);
            Type[] nativeTypes =
            [
                typeof(WpfButton),
                typeof(RepeatButton),
                typeof(ToggleButton),
                typeof(CheckBox),
                typeof(RadioButton),
                typeof(TextBox),
                typeof(PasswordBox),
                typeof(WpfScrollViewer),
                typeof(ScrollBar),
                typeof(TextBlock),
                typeof(Label),
                typeof(ComboBox),
                typeof(ComboBoxItem),
                typeof(ListBox),
                typeof(ListBoxItem),
                typeof(GridSplitter),
                typeof(ToolTip),
            ];

            foreach (var nativeType in nativeTypes)
            {
                Assert.False(
                    resources.Contains(nativeType),
                    $"Generic.xaml unexpectedly supplies an implicit Style for {nativeType.FullName}."
                );
            }
        });
    }

    [Fact]
    public void LightAndDarkPalettes_HaveMatchingResourceKeysAndValueTypes()
    {
        RunInSta(() =>
        {
            var light = LoadResourceDictionary(
                "/Flourish;component/Themes/Colors/Colors.Light.xaml"
            );
            var dark = LoadResourceDictionary(
                "/Flourish;component/Themes/Colors/Colors.Dark.xaml"
            );
            var lightKeys = light.Keys.Cast<object>().ToHashSet();
            var darkKeys = dark.Keys.Cast<object>().ToHashSet();

            Assert.True(
                lightKeys.SetEquals(darkKeys),
                $"Light-only: {FormatKeys(lightKeys.Except(darkKeys))}; "
                    + $"Dark-only: {FormatKeys(darkKeys.Except(lightKeys))}"
            );

            foreach (var key in lightKeys)
            {
                var lightValue = light[key];
                var darkValue = dark[key];
                Assert.NotNull(lightValue);
                Assert.NotNull(darkValue);
                Assert.Equal(lightValue.GetType(), darkValue.GetType());
            }
        });
    }

    [Fact]
    public void FlourishControlTemplates_InstantiateAndExposeRequiredParts()
    {
        RunInSta(() =>
        {
            var button = new FlourishButton { Content = "Action" };
            var textBox = new FlourishTextBox { Text = "Text" };
            var passwordBox = new FlourishPasswordBox { Password = "before-template" };
            var comboBox = new FlourishComboBox { ItemsSource = new[] { "One", "Two" } };
            var listBox = new FlourishListBox
            {
                ItemsSource = new[] { "First", "Second" },
            };
            var scrollBar = new FlourishScrollBar();
            var searchBox = new FlourishSearchBox { Placeholder = "Search" };
            var toolTip = new FlourishToolTip { Content = "Tip" };
            WpfControl[] templatedControls =
            [
                button,
                new Chunk
                {
                    ChunkTitle = "Section",
                    ChunkDescription = "Description",
                    ChunkBody = new Border(),
                },
                new ChunkHero
                {
                    ChunkHeroTitle = "Hero",
                    ChunkHeroDescription = "Description",
                    ChunkHeroBody = new FlourishButton { Content = "Action" },
                    Presenter = new Border(),
                },
                new Card { Title = "Card", Text = "Description" },
                new IconCard { Title = "Icon card", Presenter = "Icon" },
                new ListCard { Title = "List card", Presenter = "Icon" },
                new OutputCard(),
                new Overlay { Content = "Overlay" },
                new FlourishCheckBox { Content = "Check" },
                comboBox,
                new FlourishComboBoxItem { Content = "Choice" },
                new FlourishGridSplitter(),
                new FlourishLabel { Content = "Label" },
                listBox,
                new FlourishListBoxItem { Content = "Item" },
                passwordBox,
                new FlourishRadioButton { Content = "Radio" },
                scrollBar,
                new CustomScrollViewer
                {
                    Content = new FlourishTextBlock { Text = "Scrollable" },
                },
                searchBox,
                textBox,
            ];
            var panel = new StackPanel();
            panel.Children.Add(new FlourishTextBlock { Text = "Semantic text" });
            foreach (var control in templatedControls)
            {
                panel.Children.Add(control);
            }

            var window = CreateWindow(panel);
            try
            {
                window.Show();
                button.ToolTip = toolTip;
                toolTip.PlacementTarget = button;
                toolTip.IsOpen = true;
                comboBox.IsDropDownOpen = true;
                window.UpdateLayout();

                foreach (var control in templatedControls)
                {
                    control.ApplyTemplate();
                    Assert.NotNull(control.Template);
                }
                toolTip.ApplyTemplate();
                Assert.NotNull(toolTip.Template);
                var toolTipSurface = AssertTemplatePart<Overlay>(toolTip, "SurfaceChrome");
                Assert.Same(toolTip.Background, toolTipSurface.Background);
                Assert.Equal(OverlayVariant.Temporary, toolTipSurface.Variant);

                AssertTemplatePart<FrameworkElement>(button, "HoverChrome");
                AssertTemplatePart<ScaleTransform>(button, "HoverRevealScale");
                AssertTemplatePart<WpfScrollViewer>(textBox, "PART_ContentHost");
                AssertTemplatePart<WpfScrollViewer>(searchBox, "PART_ContentHost");
                AssertTemplatePart<Popup>(comboBox, "PART_Popup");
                AssertTemplatePart<Track>(scrollBar, "PART_Track");

                var editor = AssertTemplatePart<PasswordBox>(
                    passwordBox,
                    "PART_PasswordBox"
                );
                Assert.Equal("before-template", editor.Password);
                editor.Password = "after-template";
                Assert.Equal("after-template", passwordBox.Password);

                Assert.IsType<FlourishComboBoxItem>(
                    comboBox.ItemContainerGenerator.ContainerFromIndex(0)
                );
                Assert.IsType<FlourishListBoxItem>(
                    listBox.ItemContainerGenerator.ContainerFromIndex(0)
                );
            }
            finally
            {
                toolTip.IsOpen = false;
                comboBox.IsDropDownOpen = false;
                window.Close();
            }
        });
    }

    [Fact]
    public void ChunkHero_PresenterModesArrangePresenterAndTextAndNullPresenterUsesFullWidth()
    {
        RunInSta(() =>
        {
            var expectations = new[]
            {
                new
                {
                    Mode = PresenterMode.Split,
                    Position = PresenterPosition.Right,
                    PresenterColumn = 1,
                    TextColumn = 0,
                    ColumnSpan = 1,
                    ScrimOpacity = 0d,
                },
                new
                {
                    Mode = PresenterMode.Split,
                    Position = PresenterPosition.Left,
                    PresenterColumn = 0,
                    TextColumn = 1,
                    ColumnSpan = 1,
                    ScrimOpacity = 0d,
                },
                new
                {
                    Mode = PresenterMode.Overlay,
                    Position = PresenterPosition.Left,
                    PresenterColumn = 0,
                    TextColumn = 0,
                    ColumnSpan = 2,
                    ScrimOpacity = 1d,
                },
            };

            foreach (var expectation in expectations)
            {
                var hero = new ChunkHero
                {
                    ChunkHeroTitle = expectation.Mode.ToString(),
                    ChunkHeroBody = new Border(),
                    PresenterMode = expectation.Mode,
                    PresenterPosition = expectation.Position,
                    Presenter = new Border(),
                };
                var window = CreateWindow(hero);

                try
                {
                    window.Show();
                    window.UpdateLayout();
                    hero.ApplyTemplate();

                    var presenter = AssertTemplatePart<ContentPresenter>(
                        hero,
                        "PresenterHost"
                    );
                    var text = AssertTemplatePart<Border>(hero, "TextSurface");
                    var scrim = AssertTemplatePart<Border>(hero, "OverlayScrim");
                    var body = AssertTemplatePart<ContentPresenter>(hero, "BodyHost");
                    var clipHost = AssertTemplatePart<Grid>(hero, "PART_ClipHost");
                    var roundedClip = Assert.IsType<RectangleGeometry>(clipHost.Clip);

                    Assert.Equal(expectation.PresenterColumn, Grid.GetColumn(presenter));
                    Assert.Equal(expectation.TextColumn, Grid.GetColumn(text));
                    Assert.Equal(expectation.ColumnSpan, Grid.GetColumnSpan(presenter));
                    Assert.Equal(expectation.ColumnSpan, Grid.GetColumnSpan(text));
                    Assert.Equal(expectation.ScrimOpacity, scrim.Opacity);
                    Assert.Equal(HorizontalAlignment.Stretch, body.HorizontalAlignment);
                    Assert.Equal(new Thickness(0, 32, 0, 0), hero.Margin);
                    Assert.True(roundedClip.IsFrozen);
                    Assert.Equal(0, roundedClip.Bounds.X, precision: 3);
                    Assert.Equal(0, roundedClip.Bounds.Y, precision: 3);
                    Assert.Equal(
                        clipHost.RenderSize.Width,
                        roundedClip.Bounds.Width,
                        precision: 3
                    );
                    Assert.Equal(
                        clipHost.RenderSize.Height,
                        roundedClip.Bounds.Height,
                        precision: 3
                    );
                    Assert.True(
                        roundedClip.FillContains(
                            new System.Windows.Point(
                                clipHost.RenderSize.Width / 2,
                                clipHost.RenderSize.Height / 2
                            )
                        )
                    );
                    Assert.False(
                        roundedClip.FillContains(new System.Windows.Point(1, 1))
                    );
                    Assert.False(
                        roundedClip.FillContains(
                            new System.Windows.Point(clipHost.RenderSize.Width - 1, 1)
                        )
                    );
                    Assert.False(
                        roundedClip.FillContains(
                            new System.Windows.Point(
                                1,
                                clipHost.RenderSize.Height - 1
                            )
                        )
                    );
                    Assert.False(
                        roundedClip.FillContains(
                            new System.Windows.Point(
                                clipHost.RenderSize.Width - 1,
                                clipHost.RenderSize.Height - 1
                            )
                        )
                    );

                    hero.Width = 420;
                    window.UpdateLayout();
                    window.Dispatcher.Invoke(
                        System.Windows.Threading.DispatcherPriority.ApplicationIdle,
                        static () => { }
                    );

                    var resizedClip = Assert.IsType<RectangleGeometry>(clipHost.Clip);
                    Assert.NotSame(roundedClip, resizedClip);
                    Assert.True(resizedClip.IsFrozen);
                    Assert.Equal(
                        clipHost.RenderSize.Width,
                        resizedClip.Bounds.Width,
                        precision: 3
                    );
                    Assert.Equal(
                        clipHost.RenderSize.Height,
                        resizedClip.Bounds.Height,
                        precision: 3
                    );
                }
                finally
                {
                    window.Close();
                }
            }

            var presenterlessHero = new ChunkHero
            {
                ChunkHeroTitle = "Presenterless",
                PresenterMode = PresenterMode.Split,
                PresenterPosition = PresenterPosition.Left,
            };
            var presenterlessWindow = CreateWindow(presenterlessHero);

            try
            {
                presenterlessWindow.Show();
                presenterlessWindow.UpdateLayout();
                presenterlessHero.ApplyTemplate();

                var presenter = AssertTemplatePart<ContentPresenter>(
                    presenterlessHero,
                    "PresenterHost"
                );
                var text = AssertTemplatePart<Border>(
                    presenterlessHero,
                    "TextSurface"
                );

                Assert.Equal(Visibility.Collapsed, presenter.Visibility);
                Assert.Equal(0, Grid.GetColumn(text));
                Assert.Equal(2, Grid.GetColumnSpan(text));
            }
            finally
            {
                presenterlessWindow.Close();
            }
        });
    }

    [Fact]
    public void ButtonFamily_UsesCompactIconOnlyGeometryAndKeepsCaptionGeometrySpecialized()
    {
        RunInSta(() =>
        {
            var button = new FlourishButton { Content = "Action" };
            var icon = new IconButton { Icon = "Icon" };
            var labeledIcon = new IconButton { Icon = "Icon", Content = "Action" };
            var caption = new WindowCaptionButton { Icon = "Caption" };
            var card = new CardButton
            {
                Icon = "Card",
                Title = "Title",
                Content = "Description",
            };
            var panel = new StackPanel
            {
                Children = { button, icon, labeledIcon, caption, card },
            };
            var window = CreateWindow(panel);

            try
            {
                window.Show();
                window.UpdateLayout();
                button.ApplyTemplate();
                icon.ApplyTemplate();
                labeledIcon.ApplyTemplate();
                caption.ApplyTemplate();
                card.ApplyTemplate();

                Assert.Equal(32, button.MinHeight);
                Assert.Equal(72, button.MinWidth);
                Assert.Equal(32, icon.Width);
                Assert.Equal(32, icon.Height);
                Assert.Equal(0, icon.MinWidth);
                Assert.Equal(0, icon.MinHeight);
                Assert.Equal(new Thickness(), icon.Padding);
                Assert.Equal(button.MinWidth, labeledIcon.MinWidth);
                Assert.Equal(button.MinHeight, labeledIcon.MinHeight);
                Assert.Equal(button.Padding, labeledIcon.Padding);
                Assert.Equal(46, caption.Width);
                Assert.Equal(40, caption.Height);
                AssertTemplatePart<ContentPresenter>(icon, "IconHost");
                Assert.Null(icon.Template.FindName("ContentHost", icon));
                AssertTemplatePart<ContentPresenter>(labeledIcon, "ContentHost");
                Assert.Null(caption.Template.FindName("IconHost", caption));
                Assert.NotNull(button.FocusVisualStyle);
                Assert.NotNull(icon.FocusVisualStyle);
                Assert.NotNull(caption.FocusVisualStyle);
                Assert.NotNull(card.FocusVisualStyle);
                Assert.True(HoverReveal.GetIsParticipant(button));
                Assert.True(HoverReveal.GetIsParticipant(icon));
                Assert.True(HoverReveal.GetIsParticipant(labeledIcon));
                Assert.True(HoverReveal.GetIsParticipant(caption));
                Assert.True(HoverReveal.GetIsParticipant(card));
                Assert.Null(button.Template.FindName("FocusChrome", button));
                Assert.Null(icon.Template.FindName("FocusChrome", icon));
                Assert.Null(labeledIcon.Template.FindName("FocusChrome", labeledIcon));
                Assert.Null(caption.Template.FindName("FocusChrome", caption));
                Assert.Null(card.Template.FindName("FocusChrome", card));
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void ButtonVariants_ApplyFlourishSurfacesAndBackgroundOnlyElevatedShadow()
    {
        RunInSta(() =>
        {
            var elevated = new FlourishButton
            {
                Content = "Elevated",
                Variant = ButtonVariant.Elevated,
            };
            var filled = new FlourishButton
            {
                Content = "Filled",
                Variant = ButtonVariant.Filled,
            };
            var tonal = new FlourishButton
            {
                Content = "Tonal",
                Variant = ButtonVariant.Tonal,
            };
            var outlined = new FlourishButton
            {
                Content = "Outlined",
                Variant = ButtonVariant.Outlined,
            };
            var textButton = new FlourishButton
            {
                Content = "Text",
                Variant = ButtonVariant.Text,
            };
            var window = CreateWindow(
                new StackPanel
                {
                    Children = { elevated, filled, tonal, outlined, textButton },
                }
            );

            try
            {
                window.Show();
                window.UpdateLayout();
                elevated.ApplyTemplate();

                Assert.Null(elevated.Effect);
                var shadowChrome = AssertTemplatePart<Border>(elevated, "ShadowChrome");
                Assert.IsType<DropShadowEffect>(shadowChrome.Effect);
                Assert.Equal(Visibility.Visible, shadowChrome.Visibility);
                Assert.False(elevated.ClipToBounds);
                Assert.Equal(0, elevated.BorderThickness.Left);
                Assert.Same(
                    filled.TryFindResource("FlourishPrimaryBackgroundBrush"),
                    filled.Background
                );
                Assert.Equal(0, filled.BorderThickness.Left);
                Assert.Same(
                    tonal.TryFindResource("FlourishTonalButtonBackgroundBrush"),
                    tonal.Background
                );
                Assert.Same(
                    tonal.TryFindResource("FlourishTonalButtonForegroundBrush"),
                    tonal.Foreground
                );
                Assert.Equal(Colors.Transparent, ((SolidColorBrush)outlined.Background).Color);
                Assert.Equal(1, outlined.BorderThickness.Left);
                Assert.Equal(Colors.Transparent, ((SolidColorBrush)textButton.Background).Color);
                Assert.Equal(0, textButton.BorderThickness.Left);

                var revealBrush = elevated.TryFindResource("FlourishHoverRevealBrush");
                foreach (var button in new[] { elevated, filled, tonal, outlined, textButton })
                {
                    Assert.Same(revealBrush, HoverReveal.GetOverrideColor(button));
                }
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void ButtonDangerVariant_UsesRedHoverRevealAndAllowsLocalOverride()
    {
        RunInSta(() =>
        {
            var outlined = new FlourishButton { Content = "Outlined" };
            var danger = new FlourishButton
            {
                Variant = ButtonVariant.Danger,
                Content = "Danger",
            };
            var local = new FlourishButton
            {
                Variant = ButtonVariant.Danger,
                Content = "Local",
            };
            var localBrush = new SolidColorBrush(Colors.MediumVioletRed);
            HoverReveal.SetOverrideColor(local, localBrush);
            var captionDanger = new WindowCaptionButton
            {
                Icon = "Close",
                Variant = ButtonVariant.Danger,
            };
            var window = CreateWindow(
                new StackPanel { Children = { outlined, danger, local, captionDanger } }
            );

            try
            {
                window.Show();
                window.UpdateLayout();
                outlined.ApplyTemplate();
                danger.ApplyTemplate();
                local.ApplyTemplate();
                captionDanger.ApplyTemplate();

                var outlinedBrush = Assert.IsType<SolidColorBrush>(
                    outlined.TryFindResource("FlourishHoverRevealBrush")
                );
                var dangerBrush = Assert.IsType<SolidColorBrush>(
                    danger.TryFindResource("FlourishDangerHoverRevealBrush")
                );

                Assert.Equal(
                    outlinedBrush.Color,
                    Assert.IsType<SolidColorBrush>(HoverReveal.GetOverrideColor(outlined))
                        .Color
                );
                Assert.Equal(
                    dangerBrush.Color,
                    Assert.IsType<SolidColorBrush>(HoverReveal.GetOverrideColor(danger))
                        .Color
                );
                Assert.Same(localBrush, HoverReveal.GetOverrideColor(local));
                Assert.True(HoverReveal.GetIsMotionEnabled(captionDanger));
                Assert.True(HoverReveal.GetIsEnabled(captionDanger));
                Assert.Equal(
                    dangerBrush.Color,
                    AssertTemplatePart<Border>(danger, "HoverChrome").Background
                        is SolidColorBrush dangerHoverBrush
                        ? dangerHoverBrush.Color
                        : throw new InvalidOperationException("Danger hover chrome must use a brush.")
                );
                Assert.Same(
                    localBrush,
                    AssertTemplatePart<Border>(local, "HoverChrome").Background
                );
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void CardVariants_UseUnifiedSurfacesAndAllowFilledBackgroundOverride()
    {
        RunInSta(() =>
        {
            var standard = new Card { Title = "Standard", Text = "Description" };
            var elevated = new Card
            {
                Variant = Variant.Elevated,
                Title = "Elevated",
            };
            var tonal = new Card { Variant = Variant.Tonal, Title = "Tonal" };
            var filled = new Card { Variant = Variant.Filled, Title = "Filled" };
            var customBrush = new SolidColorBrush(Colors.MediumPurple);
            var customFilled = new Card
            {
                Variant = Variant.Filled,
                Background = customBrush,
                Title = "Custom",
            };
            var alignedContent = new Border();
            var aligned = new Card
            {
                Width = 320,
                Height = 180,
                Title = "Aligned",
                Text = "Copy",
                Body = alignedContent,
                ContentHorizontalAlignment = HorizontalAlignment.Right,
                ContentVerticalAlignment = VerticalAlignment.Bottom,
            };
            var window = CreateWindow(
                new StackPanel
                {
                    Children =
                    {
                        standard,
                        elevated,
                        tonal,
                        filled,
                        customFilled,
                        aligned,
                    },
                }
            );

            try
            {
                window.Show();
                window.UpdateLayout();
                standard.ApplyTemplate();
                elevated.ApplyTemplate();

                Assert.Equal(new Thickness(), standard.BorderThickness);
                Assert.Equal(new Thickness(), elevated.BorderThickness);
                var standardShadow = AssertTemplatePart<Border>(
                    standard,
                    "ShadowChrome"
                );
                var elevatedShadow = AssertTemplatePart<Border>(
                    elevated,
                    "ShadowChrome"
                );
                Assert.Equal(Visibility.Collapsed, standardShadow.Visibility);
                Assert.Equal(Visibility.Visible, elevatedShadow.Visibility);
                Assert.NotNull(elevatedShadow.Effect);
                Assert.Same(
                    tonal.TryFindResource("FlourishCardTonalBackgroundBrush"),
                    tonal.Background
                );
                Assert.Same(
                    filled.TryFindResource("FlourishPrimaryBackgroundBrush"),
                    filled.Background
                );
                Assert.Same(
                    filled.TryFindResource("FlourishForegroundOnPrimaryBrush"),
                    filled.Foreground
                );
                Assert.Same(customBrush, customFilled.Background);
                Assert.Equal(
                    HorizontalAlignment.Stretch,
                    AssertTemplatePart<Grid>(standard, "BuiltInContentHost")
                        .HorizontalAlignment
                );
                Assert.Equal(
                    "Standard",
                    AssertTemplatePart<FlourishTextBlock>(standard, "TitleHost").Text
                );
                Assert.Equal(
                    "Description",
                    AssertTemplatePart<FlourishTextBlock>(standard, "TextHost").Text
                );
                aligned.ApplyTemplate();
                var copyHost = AssertTemplatePart<StackPanel>(aligned, "CopyHost");
                var bodyHost = AssertTemplatePart<ContentPresenter>(
                    aligned,
                    "BodyHost"
                );
                var contentGroup = AssertTemplatePart<Grid>(aligned, "BuiltInContentHost");
                Assert.Equal(HorizontalAlignment.Right, contentGroup.HorizontalAlignment);
                Assert.Equal(VerticalAlignment.Bottom, contentGroup.VerticalAlignment);
                Assert.Equal(1, Grid.GetRow(copyHost));
                Assert.Equal(0, Grid.GetRow(bodyHost));
                Assert.Same(alignedContent, bodyHost.Content);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void Card_ArrangesBodyAroundCopyAndCentersTheCombinedGroup()
    {
        RunInSta(() =>
        {
            var top = new Card
            {
                Width = 320,
                Height = 180,
                Title = "Top copy",
                Text = "Description",
                Body = new Border { Width = 80, Height = 24 },
                ContentHorizontalAlignment = HorizontalAlignment.Left,
                ContentVerticalAlignment = VerticalAlignment.Top,
            };
            var bottom = new Card
            {
                Width = 320,
                Height = 180,
                Title = "Bottom copy",
                Body = new Border { Width = 80, Height = 24 },
                ContentHorizontalAlignment = HorizontalAlignment.Right,
                ContentVerticalAlignment = VerticalAlignment.Bottom,
            };
            var centered = new Card
            {
                Width = 320,
                Height = 180,
                Title = "Centered copy",
                Body = new Border { Width = 80, Height = 24 },
                ContentHorizontalAlignment = HorizontalAlignment.Center,
                ContentVerticalAlignment = VerticalAlignment.Center,
            };
            var emptyBody = new Card { Title = "No body" };
            var window = CreateWindow(
                new StackPanel { Children = { top, bottom, centered, emptyBody } }
            );

            try
            {
                window.Show();
                window.UpdateLayout();

                foreach (var card in new[] { top, bottom, centered, emptyBody })
                {
                    card.ApplyTemplate();
                }

                var topCopy = AssertTemplatePart<StackPanel>(top, "CopyHost");
                var topBody = AssertTemplatePart<ContentPresenter>(top, "BodyHost");
                Assert.Equal(0, Grid.GetRow(topCopy));
                Assert.Equal(1, Grid.GetRow(topBody));

                var bottomCopy = AssertTemplatePart<StackPanel>(bottom, "CopyHost");
                var bottomBody = AssertTemplatePart<ContentPresenter>(bottom, "BodyHost");
                Assert.Equal(1, Grid.GetRow(bottomCopy));
                Assert.Equal(0, Grid.GetRow(bottomBody));

                var centeredGroup = AssertTemplatePart<Grid>(
                    centered,
                    "BuiltInContentHost"
                );
                Assert.Equal(HorizontalAlignment.Center, centeredGroup.HorizontalAlignment);
                Assert.Equal(VerticalAlignment.Center, centeredGroup.VerticalAlignment);
                Assert.Equal(
                    centered.ActualWidth / 2,
                    centeredGroup.TranslatePoint(
                        new Point(centeredGroup.ActualWidth / 2, 0),
                        centered
                    ).X,
                    3
                );
                Assert.Equal(
                    centered.ActualHeight / 2,
                    centeredGroup.TranslatePoint(
                        new Point(0, centeredGroup.ActualHeight / 2),
                        centered
                    ).Y,
                    3
                );
                Assert.Equal(
                    Visibility.Collapsed,
                    AssertTemplatePart<ContentPresenter>(emptyBody, "BodyHost")
                        .Visibility
                );
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void ListCard_FixesStandardLeftCopyRightLayoutAndCollapsesEmptyRegions()
    {
        RunInSta(() =>
        {
            var presenterContent = new Border { Width = 24, Height = 24 };
            var bodyContent = new FlourishComboBox
            {
                Width = 160,
                ItemsSource = new[] { "System", "Light", "Dark" },
                SelectedIndex = 0,
            };
            var card = new ListCard
            {
                Width = 520,
                Presenter = presenterContent,
                Title = "Theme",
                Text = "Choose the application color theme.",
                Body = bodyContent,
                Variant = Variant.Filled,
                ContentHorizontalAlignment = HorizontalAlignment.Right,
                ContentVerticalAlignment = VerticalAlignment.Bottom,
            };
            var empty = new ListCard { Width = 520, Title = "Presenterless" };
            var window = CreateWindow(
                new StackPanel { Children = { card, empty } }
            );

            try
            {
                window.Show();
                window.UpdateLayout();
                card.ApplyTemplate();
                empty.ApplyTemplate();

                Assert.Equal(Variant.Standard, card.Variant);
                Assert.Equal(HorizontalAlignment.Stretch, card.ContentHorizontalAlignment);
                Assert.Equal(VerticalAlignment.Center, card.ContentVerticalAlignment);
                Assert.Equal(new Thickness(16, 12, 16, 12), card.Padding);
                Assert.Equal(72, card.MinHeight);
                Assert.Equal(
                    new Thickness(0, 4, 0, 0),
                    Assert.IsType<Thickness>(
                        card.TryFindResource("FlourishListCardPeerMargin")
                    )
                );
                Assert.Same(
                    card.TryFindResource("FlourishCardBackgroundBrush"),
                    card.Background
                );

                var surface = AssertTemplatePart<Border>(card, "SurfaceChrome");
                Assert.Equal(new CornerRadius(8), surface.CornerRadius);

                var presenter = AssertTemplatePart<ContentPresenter>(
                    card,
                    "PresenterHost"
                );
                var copy = AssertTemplatePart<StackPanel>(card, "CopyHost");
                var body = AssertTemplatePart<ContentPresenter>(card, "BodyHost");
                Assert.Equal(0, Grid.GetColumn(presenter));
                Assert.Equal(1, Grid.GetColumn(copy));
                Assert.Equal(2, Grid.GetColumn(body));
                Assert.Equal(VerticalAlignment.Center, presenter.VerticalAlignment);
                Assert.Equal(VerticalAlignment.Center, copy.VerticalAlignment);
                Assert.Equal(VerticalAlignment.Center, body.VerticalAlignment);
                Assert.Equal(new Thickness(8, 0, 24, 0), presenter.Margin);
                Assert.Same(presenterContent, presenter.Content);
                Assert.Same(bodyContent, body.Content);
                var title = AssertTemplatePart<FlourishTextBlock>(card, "TitleHost");
                var description = AssertTemplatePart<FlourishTextBlock>(
                    card,
                    "TextHost"
                );
                Assert.Equal(TextWrapping.NoWrap, title.TextWrapping);
                Assert.Equal(TextWrapping.NoWrap, description.TextWrapping);
                Assert.Equal(TextTrimming.CharacterEllipsis, title.TextTrimming);
                Assert.Equal(
                    TextTrimming.CharacterEllipsis,
                    description.TextTrimming
                );

                var emptyPresenter = AssertTemplatePart<ContentPresenter>(
                    empty,
                    "PresenterHost"
                );
                var emptyBody = AssertTemplatePart<ContentPresenter>(
                    empty,
                    "BodyHost"
                );
                Assert.Equal(Visibility.Collapsed, emptyPresenter.Visibility);
                Assert.Equal(new Thickness(), emptyPresenter.Margin);
                Assert.Equal(Visibility.Collapsed, emptyBody.Visibility);
                Assert.Equal(new Thickness(), emptyBody.Margin);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void OutputCard_UsesCompactScrollableCaptionViewportInsideCardPadding()
    {
        RunInSta(() =>
        {
            var outputCard = new OutputCard
            {
                Width = 420,
                Height = 72,
            };
            for (var index = 1; index <= 48; index++)
            {
                outputCard.WriteLine($"Output message {index:00} with a stable long value.");
            }

            var window = CreateWindow(outputCard);
            try
            {
                window.Show();
                window.UpdateLayout();
                outputCard.ApplyTemplate();
                window.Dispatcher.Invoke(
                    System.Windows.Threading.DispatcherPriority.ApplicationIdle,
                    static () => { }
                );
                window.UpdateLayout();

                Assert.Equal(72d, outputCard.MinHeight);
                Assert.Equal(new Thickness(12), outputCard.Padding);
                Assert.Equal("Consolas", outputCard.FontFamily.Source);
                Assert.Same(
                    outputCard.TryFindResource("FlourishCardBackgroundBrush"),
                    outputCard.Background
                );
                Assert.Same(
                    outputCard.TryFindResource("FlourishOutputViewportForegroundBrush"),
                    outputCard.Foreground
                );

                var surface = AssertTemplatePart<Border>(outputCard, "SurfaceChrome");
                var viewport = AssertTemplatePart<Border>(outputCard, "OutputViewport");
                var scrollViewer = AssertTemplatePart<CustomScrollViewer>(
                    outputCard,
                    "PART_OutputScrollViewer"
                );
                var outputHost = AssertTemplatePart<FlourishTextBlock>(
                    outputCard,
                    "OutputHost"
                );

                Assert.Equal(new CornerRadius(8), surface.CornerRadius);
                Assert.Equal(outputCard.Padding, surface.Padding);
                Assert.Equal(new CornerRadius(6), viewport.CornerRadius);
                Assert.Same(
                    outputCard.TryFindResource("FlourishOutputViewportBackgroundBrush"),
                    viewport.Background
                );
                Assert.True(viewport.ClipToBounds);
                Assert.Equal(new Thickness(12, 8, 12, 8), scrollViewer.Padding);
                Assert.True(scrollViewer.IsCompact);
                Assert.False(scrollViewer.Focusable);
                Assert.Equal(
                    ScrollBarVisibility.Auto,
                    scrollViewer.HorizontalScrollBarVisibility
                );
                Assert.Equal(
                    ScrollBarVisibility.Auto,
                    scrollViewer.VerticalScrollBarVisibility
                );
                Assert.Equal(FlourishTextRole.Caption, outputHost.Role);
                Assert.Equal("Consolas", outputHost.FontFamily.Source);
                Assert.Same(outputCard.Foreground, outputHost.Foreground);
                Assert.Equal(12d, outputHost.FontSize);
                Assert.Equal(14d, outputHost.LineHeight);
                Assert.True(scrollViewer.ViewportHeight >= outputHost.LineHeight);
                Assert.Equal(TextWrapping.NoWrap, outputHost.TextWrapping);
                Assert.Equal(outputCard.Output, outputHost.Text);
                Assert.Null(outputCard.Template.FindName("TitleHost", outputCard));
                Assert.Null(outputCard.Template.FindName("TextHost", outputCard));
                Assert.Null(outputCard.Template.FindName("BodyHost", outputCard));
                Assert.True(scrollViewer.ScrollableHeight > 0);
                Assert.Equal(
                    scrollViewer.ScrollableHeight,
                    scrollViewer.VerticalOffset,
                    precision: 3
                );

                outputCard.Clear();
                window.UpdateLayout();

                Assert.Equal(string.Empty, outputHost.Text);
                Assert.Equal(0, scrollViewer.VerticalOffset);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void OutputCard_HistoryDoesNotDriveAnAutoSizedPeerRowHeight()
    {
        RunInSta(() =>
        {
            var listColumn = new StackPanel();
            for (var index = 0; index < 3; index++)
            {
                listColumn.Children.Add(
                    new ListCard
                    {
                        Margin = index == 0
                            ? new Thickness()
                            : new Thickness(0, 4, 0, 0),
                        Presenter = "\uE8A5",
                        Title = $"Setting {index + 1}",
                    }
                );
            }

            var outputCard = new OutputCard();
            for (var index = 1; index <= 80; index++)
            {
                outputCard.WriteLine($"Historical output {index:00}");
            }

            var row = new UniformGrid
            {
                Columns = 2,
                Width = 600,
                VerticalAlignment = VerticalAlignment.Stretch,
            };
            row.Children.Add(listColumn);
            row.Children.Add(outputCard);

            var window = CreateWindow(new StackPanel { Children = { row } });
            try
            {
                window.Show();
                window.UpdateLayout();
                outputCard.ApplyTemplate();
                window.Dispatcher.Invoke(
                    System.Windows.Threading.DispatcherPriority.ApplicationIdle,
                    static () => { }
                );
                window.UpdateLayout();

                var scrollViewer = AssertTemplatePart<CustomScrollViewer>(
                    outputCard,
                    "PART_OutputScrollViewer"
                );

                Assert.Equal(outputCard.MinHeight, outputCard.DesiredSize.Height, precision: 3);
                Assert.Equal(listColumn.ActualHeight, outputCard.ActualHeight, precision: 3);
                Assert.True(outputCard.ActualHeight > outputCard.DesiredSize.Height);
                Assert.True(scrollViewer.ScrollableHeight > 0);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void IconCard_SplitPositionsPresenterOppositeTextAndOverlayIgnoresPosition()
    {
        RunInSta(() =>
        {
            var expectedPositions = new Dictionary<
                PresenterPosition,
                (int PresenterRow, int PresenterColumn, int TextRow, int TextColumn)
            >
            {
                [PresenterPosition.Left] = (1, 0, 1, 2),
                [PresenterPosition.LeftTop] = (0, 0, 2, 2),
                [PresenterPosition.LeftBottom] = (2, 0, 0, 2),
                [PresenterPosition.Top] = (0, 1, 2, 1),
                [PresenterPosition.Bottom] = (2, 1, 0, 1),
                [PresenterPosition.Right] = (1, 2, 1, 0),
                [PresenterPosition.RightTop] = (0, 2, 2, 0),
                [PresenterPosition.RightBottom] = (2, 2, 0, 0),
            };
            var cards = expectedPositions.Keys
                .Select(position =>
                    new IconCard
                    {
                        Width = 320,
                        Height = 180,
                        Presenter = position.ToString(),
                        PresenterPosition = position,
                        Title = "Title",
                        Text = "Description",
                        Body = new Border(),
                    }
                )
                .ToArray();
            var overlayLeft = new IconCard
            {
                Width = 320,
                Height = 180,
                Presenter = new Border(),
                PresenterMode = PresenterMode.Overlay,
                PresenterPosition = PresenterPosition.Left,
                Title = "Overlay",
                Body = new Border(),
            };
            var overlayRightBottom = new IconCard
            {
                Width = 320,
                Height = 180,
                Presenter = new Border(),
                PresenterMode = PresenterMode.Overlay,
                PresenterPosition = PresenterPosition.RightBottom,
                Title = "Overlay",
                Body = new Border(),
            };
            var presenterlessSplit = new IconCard
            {
                Width = 320,
                Height = 180,
                PresenterPosition = PresenterPosition.RightBottom,
                Title = "Presenterless split",
            };
            var presenterlessOverlay = new IconCard
            {
                Width = 320,
                Height = 180,
                PresenterMode = PresenterMode.Overlay,
                PresenterPosition = PresenterPosition.RightBottom,
                Title = "Presenterless overlay",
            };
            var panel = new StackPanel();
            foreach (var card in cards)
            {
                panel.Children.Add(card);
            }

            panel.Children.Add(overlayLeft);
            panel.Children.Add(overlayRightBottom);
            panel.Children.Add(presenterlessSplit);
            panel.Children.Add(presenterlessOverlay);
            var window = CreateWindow(panel);

            try
            {
                window.Show();
                window.UpdateLayout();

                foreach (var card in cards)
                {
                    card.ApplyTemplate();
                    var expectation = expectedPositions[card.PresenterPosition];
                    var presenter = AssertTemplatePart<ContentPresenter>(
                        card,
                        "PresenterHost"
                    );
                    var text = AssertTemplatePart<Grid>(
                        card,
                        "BuiltInContentHost"
                    );

                    Assert.Equal(expectation.PresenterRow, Grid.GetRow(presenter));
                    Assert.Equal(expectation.PresenterColumn, Grid.GetColumn(presenter));
                    Assert.Equal(expectation.TextRow, Grid.GetRow(text));
                    Assert.Equal(expectation.TextColumn, Grid.GetColumn(text));

                    var copy = AssertTemplatePart<StackPanel>(card, "CopyHost");
                    var body = AssertTemplatePart<ContentPresenter>(card, "BodyHost");
                    if (
                        card.PresenterPosition
                        is PresenterPosition.Top or PresenterPosition.Bottom
                    )
                    {
                        Assert.Equal(0, Grid.GetRow(copy));
                        Assert.Equal(0, Grid.GetRow(body));
                        Assert.Equal(0, Grid.GetColumn(copy));
                        Assert.Equal(1, Grid.GetColumn(body));
                    }
                    else
                    {
                        Assert.Equal(0, Grid.GetRow(copy));
                        Assert.Equal(1, Grid.GetRow(body));
                        Assert.Equal(0, Grid.GetColumn(copy));
                        Assert.Equal(0, Grid.GetColumn(body));
                    }
                }

                foreach (var overlay in new[] { overlayLeft, overlayRightBottom })
                {
                    overlay.ApplyTemplate();
                    var presenter = AssertTemplatePart<ContentPresenter>(
                        overlay,
                        "PresenterHost"
                    );
                    var text = AssertTemplatePart<Grid>(
                        overlay,
                        "BuiltInContentHost"
                    );
                    var scrim = AssertTemplatePart<Border>(overlay, "OverlayScrim");

                    Assert.Equal(0, Grid.GetRow(presenter));
                    Assert.Equal(3, Grid.GetRowSpan(presenter));
                    Assert.Equal(0, Grid.GetColumn(presenter));
                    Assert.Equal(3, Grid.GetColumnSpan(presenter));
                    Assert.Equal(0, Grid.GetRow(text));
                    Assert.Equal(3, Grid.GetRowSpan(text));
                    Assert.Equal(0, Grid.GetColumn(text));
                    Assert.Equal(3, Grid.GetColumnSpan(text));
                    Assert.Equal(Visibility.Visible, scrim.Visibility);
                    Assert.Equal(
                        0,
                        Grid.GetRow(AssertTemplatePart<StackPanel>(overlay, "CopyHost"))
                    );
                    Assert.Equal(
                        1,
                        Grid.GetRow(
                            AssertTemplatePart<ContentPresenter>(overlay, "BodyHost")
                        )
                    );

                    var clipHost = AssertTemplatePart<Grid>(overlay, "PART_ClipHost");
                    var surface = AssertTemplatePart<Border>(
                        overlay,
                        "PART_SurfaceChrome"
                    );
                    var clip = Assert.IsType<RectangleGeometry>(clipHost.Clip);
                    Assert.True(surface.CornerRadius.TopLeft > 0);
                    Assert.Equal(clipHost.RenderSize.Width, clip.Bounds.Width, 3);
                    Assert.Equal(clipHost.RenderSize.Height, clip.Bounds.Height, 3);
                    Assert.False(clip.FillContains(new Point(0, 0)));
                    Assert.True(
                        clip.FillContains(
                            new Point(
                                surface.CornerRadius.TopLeft,
                                surface.CornerRadius.TopLeft
                            )
                        )
                    );
                }

                foreach (
                    var presenterless in new[] { presenterlessSplit, presenterlessOverlay }
                )
                {
                    presenterless.ApplyTemplate();
                    var presenter = AssertTemplatePart<ContentPresenter>(
                        presenterless,
                        "PresenterHost"
                    );
                    var text = AssertTemplatePart<Grid>(
                        presenterless,
                        "BuiltInContentHost"
                    );
                    var title = AssertTemplatePart<FlourishTextBlock>(
                        presenterless,
                        "TitleHost"
                    );
                    var scrim = AssertTemplatePart<Border>(
                        presenterless,
                        "OverlayScrim"
                    );

                    Assert.Equal(Visibility.Collapsed, presenter.Visibility);
                    Assert.Equal(Visibility.Collapsed, scrim.Visibility);
                    Assert.Equal(0, Grid.GetRow(text));
                    Assert.Equal(3, Grid.GetRowSpan(text));
                    Assert.Equal(0, Grid.GetColumn(text));
                    Assert.Equal(3, Grid.GetColumnSpan(text));
                    Assert.Same(presenterless.Foreground, title.Foreground);
                    Assert.Equal(
                        presenterless.Padding,
                        AssertTemplatePart<Grid>(presenterless, "PART_ClipHost").Margin
                    );
                }
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void NavigationListBox_PreservesAnExplicitItemContainerContract()
    {
        RunInSta(() =>
        {
            var toolTip = new FlourishToolTip { Content = "Explicit item" };
            var item = new FlourishListBoxItem
            {
                Content = "Item",
                IsCommandItem = true,
                IsEnabled = false,
                ToolTip = toolTip,
            };
            var listBox = new FlourishListBox
            {
                Appearance = FlourishListBoxAppearance.Navigation,
                Items = { item },
            };
            var window = CreateWindow(listBox);

            try
            {
                window.Show();
                window.UpdateLayout();

                Assert.Same(item, listBox.ItemContainerGenerator.ContainerFromIndex(0));
                Assert.True(item.IsCommandItem);
                Assert.False(item.IsEnabled);
                Assert.Same(toolTip, item.ToolTip);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void NavigationListBoxItem_FollowsTheSharedToolTipDelayPolicy()
    {
        RunInSta(() =>
        {
            var item = new FlourishListBoxItem { Content = "Item" };
            var listBox = new FlourishListBox
            {
                Appearance = FlourishListBoxAppearance.Navigation,
                Items = { item },
            };
            var resources = new ResourceDictionary
            {
                ["FlourishToolTipInitialShowDelay"] = int.MaxValue,
            };
            var window = CreateWindow(listBox);
            window.Resources.MergedDictionaries.Add(resources);

            try
            {
                window.Show();
                window.UpdateLayout();

                Assert.Equal(
                    int.MaxValue,
                    ToolTipService.GetInitialShowDelay(item)
                );

                resources["FlourishToolTipInitialShowDelay"] = 275;

                Assert.Equal(275, ToolTipService.GetInitialShowDelay(item));
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void PasswordBox_OuterFocusAndPublicFocusMethodReachTheEditor()
    {
        RunInSta(() =>
        {
            var passwordBox = new FlourishPasswordBox();
            var window = CreateWindow(passwordBox);
            window.ShowActivated = true;

            try
            {
                window.Show();
                window.Activate();
                window.UpdateLayout();
                passwordBox.ApplyTemplate();
                var editor = AssertTemplatePart<PasswordBox>(
                    passwordBox,
                    "PART_PasswordBox"
                );

                Assert.True(passwordBox.Focusable);
                Assert.False(editor.IsTabStop);
                Keyboard.Focus(passwordBox);
                Assert.Same(editor, Keyboard.FocusedElement);
                Assert.True(passwordBox.FocusEditor());
                Assert.Same(editor, Keyboard.FocusedElement);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void GridSplitter_RowsDirectionUsesHorizontalGeometry()
    {
        RunInSta(() =>
        {
            var splitter = new FlourishGridSplitter
            {
                ResizeDirection = GridResizeDirection.Rows,
            };
            var window = CreateWindow(splitter);

            try
            {
                window.Show();
                window.UpdateLayout();

                Assert.True(double.IsNaN(splitter.Width));
                Assert.Equal(4, splitter.Height);
                Assert.Equal(HorizontalAlignment.Stretch, splitter.HorizontalAlignment);
                Assert.Equal(Cursors.SizeNS, splitter.Cursor);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void GridSplitter_NavigationPaneProvidesAVisibleDragPreview()
    {
        RunInSta(() =>
        {
            var host = new StackPanel();
            var splitter = new FlourishGridSplitter
            {
                Height = 80,
                Variant = FlourishGridSplitterVariant.NavigationPane,
            };
            host.Children.Add(splitter);
            var window = CreateWindow(host);

            try
            {
                window.Show();
                window.UpdateLayout();

                Assert.True(splitter.ShowsPreview);
                var previewStyle = Assert.IsType<Style>(splitter.PreviewStyle);
                Assert.Equal(typeof(WpfControl), previewStyle.TargetType);

                var preview = new WpfControl
                {
                    Width = splitter.ActualWidth,
                    Height = splitter.ActualHeight,
                    Style = previewStyle,
                };
                host.Children.Add(preview);
                window.UpdateLayout();
                preview.ApplyTemplate();

                var indicator = Assert.IsType<Border>(
                    preview.Template.FindName("PreviewIndicator", preview)
                );
                Assert.NotNull(indicator.Background);
                Assert.True(indicator.Opacity > 0);
                Assert.True(indicator.ActualWidth > 0);
                Assert.True(indicator.ActualHeight > 0);
            }
            finally
            {
                window.Close();
            }
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

    private static Window CreateWindow(UIElement content)
    {
        var window = new Window
        {
            Width = 640,
            Height = 1000,
            Left = -10000,
            Top = -10000,
            ShowActivated = false,
            ShowInTaskbar = false,
            Content = content,
        };
        window.Resources.MergedDictionaries.Add(
            LoadResourceDictionary(GenericThemeSource)
        );
        return window;
    }

    private static T AssertTemplatePart<T>(WpfControl control, string partName)
        where T : class
    {
        var part = control.Template.FindName(partName, control);
        Assert.True(
            part is T,
            $"{control.GetType().FullName} template part '{partName}' was "
                + $"{part?.GetType().FullName ?? "missing"}; expected {typeof(T).FullName}."
        );
        return (T)part!;
    }

    private static ResourceDictionary LoadResourceDictionary(string source)
    {
        return Assert.IsType<ResourceDictionary>(
            Application.LoadComponent(new Uri(source, UriKind.Relative))
        );
    }

    private static string FormatKeys(IEnumerable<object> keys)
    {
        return string.Join(", ", keys.Select(key => key.ToString()).Order());
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
