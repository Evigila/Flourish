using System.Globalization;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using ArkheideSystem.Flourish.Controls;
using CustomScrollViewer = ArkheideSystem.Flourish.Controls.ScrollViewer;
using FlourishButton = ArkheideSystem.Flourish.Controls.Button;
using FlourishParagraph = ArkheideSystem.Flourish.Controls.Paragraph;
using WpfBinding = System.Windows.Data.Binding;
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
            "/Flourish;component/Controls/Presenter.xaml",
            "/Flourish;component/Controls/Paragraph.xaml",
            "/Flourish;component/Controls/CodeSpace.xaml",
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
            var iconCard = new IconCard { Title = "Icon card", Icon = "\uE8A5" };
            var listCard = new ListCard { Title = "List card", Icon = "\uE8A5" };
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
                    (iconCard, "IconHost"),
                    (listCard, "IconHost"),
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
    public void CardPresenterChunkAndChunkHero_TitleHostsUseTheirExplicitHeadingTiers()
    {
        RunInSta(() =>
        {
            WpfControl[] controls =
            [
                new Card { Title = "Card" },
                new Presenter { Title = "Presenter" },
                new Chunk { Title = "Section" },
                new ChunkHero { Title = "Hero" },
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
                var presenterTitle = AssertTemplatePart<FlourishTextBlock>(controls[1], "TitleHost");
                Assert.Equal(16d, presenterTitle.FontSize);
                Assert.Equal(FontWeights.Bold, presenterTitle.FontWeight);

                controls[2].ApplyTemplate();
                var chunkTitle = AssertTemplatePart<FlourishTextBlock>(controls[2], "TitleHost");
                Assert.Equal(24d, chunkTitle.FontSize);
                Assert.Equal(FontWeights.Bold, chunkTitle.FontWeight);

                controls[3].ApplyTemplate();
                var heroTitle = AssertTemplatePart<FlourishTextBlock>(controls[3], "TitleHost");
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
    public void GenericTheme_DoesNotOverrideUnrelatedNativeWpfControlStyles()
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
    public void NativeAndFlourishToolTips_UseTheSharedTemporaryOverlayTemplate()
    {
        RunInSta(() =>
        {
            var resources = LoadResourceDictionary(GenericThemeSource);
            var nativeStyle = Assert.IsType<Style>(resources[typeof(ToolTip)]);
            var flourishStyle = Assert.IsType<Style>(
                resources[typeof(FlourishToolTip)]
            );
            Assert.Equal(typeof(ToolTip), nativeStyle.TargetType);
            Assert.Equal(typeof(FlourishToolTip), flourishStyle.TargetType);
            Assert.Same(nativeStyle.BasedOn, flourishStyle.BasedOn);

            var target = new CardButton
            {
                Content = "Reference",
                FontStyle = FontStyles.Italic,
                FontWeight = FontWeights.Bold,
                IsEnabled = false,
                ToolTip = "External navigation is not available in Gallery.",
            };
            var nativeToolTip = new ToolTip
            {
                Content = target.ToolTip,
                PlacementTarget = target,
                Style = nativeStyle,
            };
            target.ToolTip = nativeToolTip;
            var window = CreateWindow(target);

            try
            {
                window.Show();
                window.UpdateLayout();
                target.ApplyTemplate();
                nativeToolTip.IsOpen = true;
                nativeToolTip.ApplyTemplate();

                Assert.True(ToolTipService.GetShowOnDisabled(target));
                Assert.True(FlourishToolTipPlacement.GetIsEnabled(nativeToolTip));
                Assert.Equal(FontStyles.Normal, nativeToolTip.FontStyle);
                Assert.Equal(FontWeights.Regular, nativeToolTip.FontWeight);
                var surface = AssertTemplatePart<Overlay>(
                    nativeToolTip,
                    "SurfaceChrome"
                );
                Assert.Equal(OverlayVariant.Temporary, surface.Variant);
            }
            finally
            {
                nativeToolTip.IsOpen = false;
                window.Close();
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
                    Title = "Section",
                    Description = "Description",
                    Body = new Border(),
                },
                new ChunkHero
                {
                    Title = "Hero",
                    Description = "Description",
                    Body = new FlourishButton { Content = "Action" },
                    Presentation = new Border(),
                },
                new Presenter
                {
                    Title = "Presenter",
                    Description = "Description",
                    Body = new FlourishButton { Content = "Action" },
                    Presentation = new Border(),
                },
                new FlourishParagraph
                {
                    Items =
                    {
                        new FlourishTextBlock { Text = "Paragraph" },
                    },
                },
                new Card { Title = "Card", MainText = "Description" },
                new IconCard { Title = "Icon card", Icon = "\uE8A5" },
                new ListCard { Title = "List card", Icon = "\uE8A5" },
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
    public void ChunkHero_PresenterModesArrangePresentationAndCopyAndNullPresentationUsesFullWidth()
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
                    ScrimVisibility = Visibility.Collapsed,
                },
                new
                {
                    Mode = PresenterMode.Split,
                    Position = PresenterPosition.Left,
                    PresenterColumn = 0,
                    TextColumn = 1,
                    ColumnSpan = 1,
                    ScrimVisibility = Visibility.Collapsed,
                },
                new
                {
                    Mode = PresenterMode.Overlay,
                    Position = PresenterPosition.Left,
                    PresenterColumn = 0,
                    TextColumn = 0,
                    ColumnSpan = 2,
                    ScrimVisibility = Visibility.Visible,
                },
            };

            foreach (var expectation in expectations)
            {
                var hero = new ChunkHero
                {
                    Title = expectation.Mode.ToString(),
                    Body = new Border(),
                    PresenterMode = expectation.Mode,
                    PresenterPosition = expectation.Position,
                    Presentation = new Border(),
                };
                var window = CreateWindow(hero);

                try
                {
                    window.Show();
                    window.UpdateLayout();
                    hero.ApplyTemplate();

                    var presenter = AssertTemplatePart<ContentPresenter>(
                        hero,
                        "PresentationHost"
                    );
                    var text = AssertTemplatePart<Border>(hero, "CopySurface");
                    var scrim = AssertTemplatePart<Border>(hero, "OverlayScrim");
                    var body = AssertTemplatePart<ContentPresenter>(hero, "BodyHost");
                    var clipHost = AssertTemplatePart<Grid>(hero, "PART_ClipHost");
                    var roundedClip = Assert.IsType<RectangleGeometry>(clipHost.Clip);

                    Assert.Equal(expectation.PresenterColumn, Grid.GetColumn(presenter));
                    Assert.Equal(expectation.TextColumn, Grid.GetColumn(text));
                    Assert.Equal(expectation.ColumnSpan, Grid.GetColumnSpan(presenter));
                    Assert.Equal(expectation.ColumnSpan, Grid.GetColumnSpan(text));
                    Assert.Equal(expectation.ScrimVisibility, scrim.Visibility);
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
                Title = "Presenterless",
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
                    "PresentationHost"
                );
                var text = AssertTemplatePart<Border>(
                    presenterlessHero,
                    "CopySurface"
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
    public void Presenter_UsesFullWidthSplitAndOverlayLayoutsAndCollapsesAbsentRegions()
    {
        RunInSta(() =>
        {
            var splitRight = new Presenter
            {
                Title = "Right",
                Description = "Supporting copy",
                Body = new FlourishButton { Content = "Action" },
                Presentation = new Border { Width = 120, Height = 72 },
            };
            var splitLeft = new Presenter
            {
                Title = "Left",
                Presentation = new Border(),
                PresenterPosition = PresenterPosition.Left,
            };
            var overlay = new Presenter
            {
                Title = "Overlay",
                Description = "Supporting copy",
                Body = new Border(),
                Presentation = new Border(),
                PresenterMode = PresenterMode.Overlay,
            };
            var overlayWithoutPresentation = new Presenter
            {
                Title = "No presentation",
                Description = "Foreground remains readable",
                Body = new Border(),
                PresenterMode = PresenterMode.Overlay,
                PresenterPosition = PresenterPosition.Left,
            };
            var empty = new Presenter();
            var panel = new StackPanel
            {
                Children =
                {
                    splitRight,
                    splitLeft,
                    overlay,
                    overlayWithoutPresentation,
                    empty,
                },
            };
            var window = CreateWindow(panel);

            try
            {
                window.Show();
                window.UpdateLayout();

                foreach (var presenter in new[]
                {
                    splitRight,
                    splitLeft,
                    overlay,
                    overlayWithoutPresentation,
                    empty,
                })
                {
                    presenter.ApplyTemplate();
                    Assert.Equal(HorizontalAlignment.Stretch, presenter.HorizontalAlignment);
                    Assert.Same(
                        presenter.TryFindResource("FlourishNeutralBackground2Brush"),
                        presenter.Background
                    );
                    Assert.Null(presenter.BorderBrush);
                    Assert.Equal(new Thickness(), presenter.BorderThickness);
                    Assert.Equal(14d, presenter.FontSize);
                    var surface = AssertTemplatePart<Border>(presenter, "PresenterSurface");
                    var presentationSurface = AssertTemplatePart<Border>(
                        presenter,
                        "PresentationSurface"
                    );
                    Assert.Null(surface.Background);
                    Assert.Same(presenter.Background, presentationSurface.Background);
                    Assert.True(presentationSurface.ClipToBounds);
                    Assert.Equal(
                        presenter.TryFindResource("FlourishSurfaceCornerRadius"),
                        presentationSurface.CornerRadius
                    );
                    Assert.True(surface.ClipToBounds);
                    Assert.Equal(
                        presenter.TryFindResource("FlourishSurfaceCornerRadius"),
                        surface.CornerRadius
                    );
                }

                AssertPresenterLayout(
                    splitRight,
                    presentationColumn: 1,
                    copyColumn: 0,
                    columnSpan: 1,
                    scrimVisibility: Visibility.Collapsed
                );
                Assert.Equal(PresenterMode.Split, splitRight.PresenterMode);
                Assert.Equal(PresenterPosition.Right, splitRight.PresenterPosition);
                var splitCopy = AssertTemplatePart<Border>(splitRight, "CopySurface");
                var splitPresentationSurface = AssertTemplatePart<Border>(
                    splitRight,
                    "PresentationSurface"
                );
                var splitPresentation = AssertTemplatePart<ContentPresenter>(
                    splitRight,
                    "PresentationHost"
                );
                Assert.Equal(
                    HorizontalAlignment.Center,
                    splitPresentation.HorizontalAlignment
                );
                Assert.Equal(
                    VerticalAlignment.Center,
                    splitPresentation.VerticalAlignment
                );
                Assert.Null(splitCopy.Background);
                Assert.Same(splitRight.Background, splitPresentationSurface.Background);
                Assert.True(splitPresentationSurface.ActualWidth > splitPresentation.ActualWidth);
                Assert.True(splitPresentationSurface.ActualHeight > splitPresentation.ActualHeight);
                var presentationOrigin = splitPresentation
                    .TransformToAncestor(splitPresentationSurface)
                    .Transform(new Point());
                Assert.Equal(
                    (splitPresentationSurface.ActualWidth - splitPresentation.ActualWidth) / 2,
                    presentationOrigin.X,
                    3
                );
                Assert.Equal(
                    (splitPresentationSurface.ActualHeight - splitPresentation.ActualHeight) / 2,
                    presentationOrigin.Y,
                    3
                );
                var presentedBorder = Assert.IsType<Border>(splitPresentation.Content);
                Assert.Equal(splitPresentation.ActualWidth, presentedBorder.ActualWidth, 3);
                Assert.Equal(splitPresentation.ActualHeight, presentedBorder.ActualHeight, 3);
                var splitBody = AssertTemplatePart<ContentPresenter>(
                    splitRight,
                    "BodyHost"
                );
                Assert.Equal(HorizontalAlignment.Left, splitBody.HorizontalAlignment);
                Assert.Equal(VerticalAlignment.Center, splitBody.VerticalAlignment);
                var copyAndBodyHost = AssertTemplatePart<StackPanel>(
                    splitRight,
                    "CopyAndBodyHost"
                );
                var copyHost = AssertTemplatePart<StackPanel>(splitRight, "CopyHost");
                var copyOrigin = copyHost
                    .TransformToAncestor(copyAndBodyHost)
                    .Transform(new Point());
                var bodyOrigin = splitBody
                    .TransformToAncestor(copyAndBodyHost)
                    .Transform(new Point());
                Assert.Equal(copyOrigin.X, bodyOrigin.X, 3);
                Assert.Equal(
                    TextAlignment.Left,
                    AssertTemplatePart<FlourishTextBlock>(splitRight, "TitleHost")
                        .TextAlignment
                );
                Assert.Same(
                    splitBody,
                    FindVisualDescendant<ContentPresenter>(splitCopy, "BodyHost")
                );
                AssertPresenterLayout(
                    splitLeft,
                    presentationColumn: 0,
                    copyColumn: 1,
                    columnSpan: 1,
                    scrimVisibility: Visibility.Collapsed
                );
                AssertPresenterLayout(
                    overlay,
                    presentationColumn: 0,
                    copyColumn: 0,
                    columnSpan: 2,
                    scrimVisibility: Visibility.Visible
                );
                var overlayPresentation = AssertTemplatePart<ContentPresenter>(
                    overlay,
                    "PresentationHost"
                );
                Assert.Equal(
                    HorizontalAlignment.Stretch,
                    overlayPresentation.HorizontalAlignment
                );
                Assert.Equal(
                    VerticalAlignment.Stretch,
                    overlayPresentation.VerticalAlignment
                );
                var absentPresentation = AssertTemplatePart<ContentPresenter>(
                    overlayWithoutPresentation,
                    "PresentationHost"
                );
                var absentPresentationSurface = AssertTemplatePart<Border>(
                    overlayWithoutPresentation,
                    "PresentationSurface"
                );
                var readableCopy = AssertTemplatePart<Border>(
                    overlayWithoutPresentation,
                    "CopySurface"
                );
                var readableTitle = AssertTemplatePart<FlourishTextBlock>(
                    overlayWithoutPresentation,
                    "TitleHost"
                );
                var readableDescription = AssertTemplatePart<FlourishTextBlock>(
                    overlayWithoutPresentation,
                    "DescriptionHost"
                );
                var readableBody = AssertTemplatePart<ContentPresenter>(
                    overlayWithoutPresentation,
                    "BodyHost"
                );
                Assert.Equal(Visibility.Collapsed, absentPresentation.Visibility);
                Assert.Equal(Visibility.Collapsed, absentPresentationSurface.Visibility);
                Assert.Equal(0, Grid.GetColumn(readableCopy));
                Assert.Equal(2, Grid.GetColumnSpan(readableCopy));
                Assert.Equal(
                    Visibility.Collapsed,
                    AssertTemplatePart<Border>(overlayWithoutPresentation, "OverlayScrim")
                        .Visibility
                );
                Assert.Same(overlayWithoutPresentation.Foreground, readableTitle.Foreground);
                Assert.Same(
                    overlayWithoutPresentation.Foreground,
                    readableDescription.Foreground
                );
                Assert.Same(
                    overlayWithoutPresentation.Foreground,
                    readableBody.GetValue(TextElement.ForegroundProperty)
                );

                Assert.Equal(
                    Visibility.Collapsed,
                    AssertTemplatePart<StackPanel>(empty, "CopyHost").Visibility
                );
                Assert.Equal(
                    Visibility.Collapsed,
                    AssertTemplatePart<FlourishTextBlock>(empty, "TitleHost").Visibility
                );
                var emptyDescription = AssertTemplatePart<FlourishTextBlock>(
                    empty,
                    "DescriptionHost"
                );
                var emptyBody = AssertTemplatePart<ContentPresenter>(empty, "BodyHost");
                Assert.Equal(Visibility.Collapsed, emptyDescription.Visibility);
                Assert.Equal(new Thickness(), emptyDescription.Margin);
                Assert.Equal(Visibility.Collapsed, emptyBody.Visibility);
                Assert.Equal(new Thickness(), emptyBody.Margin);
                Assert.Equal(
                    Visibility.Collapsed,
                    AssertTemplatePart<ContentPresenter>(empty, "PresentationHost").Visibility
                );
                Assert.Equal(
                    Visibility.Collapsed,
                    AssertTemplatePart<Border>(empty, "PresentationSurface").Visibility
                );
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void Paragraph_RendersItsSubtleSurfaceAndSupportsLocalOverrides()
    {
        RunInSta(() =>
        {
            var paragraph = new FlourishParagraph
            {
                Items = { new FlourishTextBlock { Text = "Paragraph" } },
            };
            var overriddenParagraph = new FlourishParagraph
            {
                Background = Brushes.Red,
                BorderBrush = Brushes.Blue,
                BorderThickness = new Thickness(8),
                Items = { new FlourishTextBlock { Text = "Paragraph" } },
            };
            var panel = new StackPanel
            {
                Children = { paragraph, overriddenParagraph },
            };
            var window = CreateWindow(panel);

            try
            {
                window.Show();
                window.UpdateLayout();
                paragraph.ApplyTemplate();
                overriddenParagraph.ApplyTemplate();

                Assert.Equal(new Thickness(0, 8, 0, 0), paragraph.Margin);
                Assert.Equal(new Thickness(16, 12, 16, 12), paragraph.Padding);
                Assert.Null(paragraph.Background);
                Assert.Same(
                    paragraph.FindResource("FlourishSurfaceStrokeBrush"),
                    paragraph.BorderBrush
                );
                Assert.Equal(new Thickness(1), paragraph.BorderThickness);

                var surface = AssertTemplatePart<Border>(paragraph, "ParagraphSurface");
                Assert.Equal(paragraph.Padding, surface.Padding);
                Assert.Same(paragraph.Background, surface.Background);
                Assert.Same(paragraph.BorderBrush, surface.BorderBrush);
                Assert.Equal(paragraph.BorderThickness, surface.BorderThickness);
                Assert.Equal(new CornerRadius(8), surface.CornerRadius);

                var overriddenSurface = AssertTemplatePart<Border>(
                    overriddenParagraph,
                    "ParagraphSurface"
                );
                Assert.Same(Brushes.Red, overriddenSurface.Background);
                Assert.Same(Brushes.Blue, overriddenSurface.BorderBrush);
                Assert.Equal(new Thickness(8), overriddenSurface.BorderThickness);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void CodeSpace_UsesTheFixedCodeStyleAndCopiesItsExactText()
    {
        RunInSta(() =>
        {
            const string code = "public static void Main()\r\n{\r\n    Run();\r\n}";
            string? copiedText = null;
            var codeSpace = new CodeSpace { Text = code };
            codeSpace.ClipboardWriter = value => copiedText = value;
            var window = CreateWindow(codeSpace);

            try
            {
                window.Show();
                window.UpdateLayout();
                codeSpace.ApplyTemplate();

                Assert.Equal(new Thickness(0, 8, 0, 0), codeSpace.Margin);
                Assert.Equal(new Thickness(16, 12, 16, 12), codeSpace.Padding);
                Assert.Null(codeSpace.Background);
                Assert.Same(
                    codeSpace.FindResource("FlourishSurfaceStrokeBrush"),
                    codeSpace.BorderBrush
                );
                Assert.Equal(new Thickness(1), codeSpace.BorderThickness);

                var surface = AssertTemplatePart<Border>(codeSpace, "CodeSpaceSurface");
                Assert.Equal(codeSpace.Padding, surface.Padding);
                Assert.Same(codeSpace.Background, surface.Background);
                Assert.Same(codeSpace.BorderBrush, surface.BorderBrush);
                Assert.Equal(codeSpace.BorderThickness, surface.BorderThickness);
                Assert.Equal(new CornerRadius(8), surface.CornerRadius);

                var textHost = AssertTemplatePart<System.Windows.Controls.TextBlock>(
                    codeSpace,
                    "CodeTextHost"
                );
                Assert.Equal("Consolas", textHost.FontFamily.Source);
                Assert.Equal(16d, textHost.FontSize);
                Assert.Equal(FontStyles.Normal, textHost.FontStyle);
                Assert.Equal(FontWeights.Bold, textHost.FontWeight);
                Assert.Same(
                    codeSpace.FindResource("FlourishCodeForegroundBrush"),
                    textHost.Foreground
                );
                Assert.Equal(code, textHost.Text);
                Assert.Equal(TextWrapping.NoWrap, textHost.TextWrapping);

                var copyButton = AssertTemplatePart<IconButton>(
                    codeSpace,
                    "PART_CopyButton"
                );
                Assert.Same(ApplicationCommands.Copy, copyButton.Command);
                Assert.Same(codeSpace, copyButton.CommandTarget);
                Assert.Equal("\uE8C8", copyButton.Icon);
                Assert.Equal(HorizontalAlignment.Right, copyButton.HorizontalAlignment);
                Assert.Equal(VerticalAlignment.Top, copyButton.VerticalAlignment);
                Assert.Equal("Copy code", AutomationProperties.GetName(copyButton));
                var copyToolTip = Assert.IsType<FlourishToolTip>(copyButton.ToolTip);
                Assert.Equal("Copy code", copyToolTip.Content);
                Assert.Equal(FontStyles.Normal, copyToolTip.FontStyle);
                Assert.Equal(FontWeights.Regular, copyToolTip.FontWeight);

                Assert.True(ApplicationCommands.Copy.CanExecute(null, codeSpace));
                ApplicationCommands.Copy.Execute(null, codeSpace);
                Assert.Equal(code, copiedText);

                codeSpace.Text = string.Empty;
                CommandManager.InvalidateRequerySuggested();
                window.UpdateLayout();
                Assert.False(ApplicationCommands.Copy.CanExecute(null, codeSpace));
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void Paragraph_UsesFourFontMeasuredSpacesWithoutWholeItemInset()
    {
        RunInSta(() =>
        {
            var bindingSource = new System.Windows.Controls.TextBlock
            {
                Text = "First paragraph",
            };
            var first = new FlourishTextBlock { FontSize = 14 };
            System.Windows.Data.BindingOperations.SetBinding(
                first,
                System.Windows.Controls.TextBlock.TextProperty,
                new WpfBinding(nameof(System.Windows.Controls.TextBlock.Text))
            );
            var second = new FlourishTextBlock { Text = "Second paragraph" };
            var paragraph = new FlourishParagraph
            {
                DataContext = bindingSource,
                Items = { first, second },
            };
            var window = CreateWindow(paragraph);

            try
            {
                window.Show();
                window.UpdateLayout();
                paragraph.ApplyTemplate();

                Assert.Equal(16d, paragraph.FontSize);
                Assert.Equal(HorizontalAlignment.Stretch, paragraph.HorizontalAlignment);
                Assert.Equal(TextWrapping.Wrap, first.TextWrapping);
                Assert.Equal(TextWrapping.Wrap, second.TextWrapping);

                var firstContainer = Assert.IsType<ContentPresenter>(
                    paragraph.ItemContainerGenerator.ContainerFromIndex(0)
                );
                var secondContainer = Assert.IsType<ContentPresenter>(
                    paragraph.ItemContainerGenerator.ContainerFromIndex(1)
                );
                Assert.Equal(new Thickness(), firstContainer.Margin);
                Assert.Equal(new Thickness(0, 12, 0, 0), secondContainer.Margin);

                var firstProxy = Assert.IsType<System.Windows.Controls.TextBlock>(
                    firstContainer.Content
                );
                var secondProxy = Assert.IsType<System.Windows.Controls.TextBlock>(
                    secondContainer.Content
                );
                Assert.Equal("ParagraphTextProxy", firstProxy.Name);
                Assert.Equal("ParagraphTextProxy", secondProxy.Name);
                Assert.Equal("First paragraph", first.Text);
                Assert.Equal("    First paragraph", firstProxy.Text);
                Assert.Equal("    Second paragraph", secondProxy.Text);
                Assert.Equal(16d, firstProxy.FontSize);
                Assert.Equal(16d, secondProxy.FontSize);

                var initialIndentWidth = MeasureParagraphIndent(firstProxy);
                first.FontSize = 28;
                bindingSource.Text = "Updated paragraph";
                window.UpdateLayout();

                Assert.Equal("Updated paragraph", first.Text);
                Assert.Equal("    Updated paragraph", firstProxy.Text);
                Assert.Equal(28d, first.FontSize);
                Assert.Equal(16d, firstProxy.FontSize);
                Assert.Equal(initialIndentWidth, MeasureParagraphIndent(firstProxy));
            }
            finally
            {
                window.Close();
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
            var emptyContentIcon = new IconButton { Icon = "Icon", Content = "" };
            var labeledIcon = new IconButton { Icon = "Icon", Content = "Action" };
            var caption = new WindowCaptionButton { Icon = "Caption" };
            var card = new CardButton
            {
                Icon = "Card",
                Title = "Title",
                Content = "Description",
            };
            var emptyCard = new CardButton { Icon = "", Title = "", Content = "" };
            var panel = new StackPanel
            {
                Children =
                {
                    button,
                    icon,
                    emptyContentIcon,
                    labeledIcon,
                    caption,
                    card,
                    emptyCard,
                },
            };
            var window = CreateWindow(panel);

            try
            {
                window.Show();
                window.UpdateLayout();
                button.ApplyTemplate();
                icon.ApplyTemplate();
                emptyContentIcon.ApplyTemplate();
                labeledIcon.ApplyTemplate();
                caption.ApplyTemplate();
                card.ApplyTemplate();
                emptyCard.ApplyTemplate();

                Assert.Equal(32, button.MinHeight);
                Assert.Equal(72, button.MinWidth);
                Assert.Equal(32, icon.Width);
                Assert.Equal(32, icon.Height);
                Assert.Equal(0, icon.MinWidth);
                Assert.Equal(0, icon.MinHeight);
                Assert.Equal(new Thickness(), icon.Padding);
                Assert.Equal(32, emptyContentIcon.Width);
                Assert.Equal(32, emptyContentIcon.Height);
                Assert.Equal(new Thickness(), emptyContentIcon.Padding);
                Assert.Equal(button.MinWidth, labeledIcon.MinWidth);
                Assert.Equal(button.MinHeight, labeledIcon.MinHeight);
                Assert.Equal(button.Padding, labeledIcon.Padding);
                Assert.Equal(46, caption.Width);
                Assert.Equal(40, caption.Height);
                AssertTemplatePart<ContentPresenter>(icon, "IconHost");
                Assert.Null(icon.Template.FindName("ContentHost", icon));
                AssertTemplatePart<ContentPresenter>(emptyContentIcon, "IconHost");
                Assert.Null(
                    emptyContentIcon.Template.FindName("ContentHost", emptyContentIcon)
                );
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

                var emptyCardIcon = Assert.IsType<ContentPresenter>(
                    FindVisualDescendant<ContentPresenter>(emptyCard, "IconHost")
                );
                var emptyCardTitle = Assert.IsType<FlourishTextBlock>(
                    FindVisualDescendant<FlourishTextBlock>(emptyCard, "TitleHost")
                );
                var emptyCardDescription = Assert.IsType<ContentPresenter>(
                    FindVisualDescendant<ContentPresenter>(emptyCard, "DescriptionHost")
                );
                Assert.Equal(Visibility.Collapsed, emptyCardIcon.Visibility);
                Assert.Equal(Visibility.Collapsed, emptyCardTitle.Visibility);
                Assert.Equal(Visibility.Collapsed, emptyCardDescription.Visibility);
                Assert.Equal(new Thickness(), emptyCardDescription.Margin);
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
            var standard = new Card { Title = "Standard", MainText = "Description" };
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
            var aligned = new Card
            {
                Width = 320,
                Height = 180,
                Title = "Aligned",
                MainText = "Copy",
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
                    AssertTemplatePart<StackPanel>(standard, "CopyHost")
                        .HorizontalAlignment
                );
                Assert.Equal(
                    "Standard",
                    AssertTemplatePart<FlourishTextBlock>(standard, "TitleHost").Text
                );
                Assert.Equal(
                    "Description",
                    AssertTemplatePart<FlourishTextBlock>(standard, "MainTextHost").Text
                );
                aligned.ApplyTemplate();
                var copyHost = AssertTemplatePart<StackPanel>(aligned, "CopyHost");
                Assert.Equal(HorizontalAlignment.Right, copyHost.HorizontalAlignment);
                Assert.Equal(VerticalAlignment.Bottom, copyHost.VerticalAlignment);
                Assert.Equal(
                    TextAlignment.Right,
                    AssertTemplatePart<FlourishTextBlock>(aligned, "TitleHost")
                        .TextAlignment
                );
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void Card_OptionalCopyRegionsCollapseWithoutLeavingSpacing()
    {
        RunInSta(() =>
        {
            var titleOnly = new Card { Title = "Title" };
            var textOnly = new Card { MainText = "Main text" };
            var empty = new Card();
            var window = CreateWindow(
                new StackPanel { Children = { titleOnly, textOnly, empty } }
            );

            try
            {
                window.Show();
                window.UpdateLayout();

                foreach (var card in new[] { titleOnly, textOnly, empty })
                {
                    card.ApplyTemplate();
                }

                Assert.Equal(
                    Visibility.Collapsed,
                    AssertTemplatePart<FlourishTextBlock>(titleOnly, "MainTextHost")
                        .Visibility
                );
                Assert.Equal(
                    Visibility.Collapsed,
                    AssertTemplatePart<FlourishTextBlock>(textOnly, "TitleHost").Visibility
                );
                Assert.Equal(
                    new Thickness(),
                    AssertTemplatePart<FlourishTextBlock>(textOnly, "MainTextHost").Margin
                );
                Assert.Equal(
                    Visibility.Collapsed,
                    AssertTemplatePart<StackPanel>(empty, "CopyHost").Visibility
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
            const string iconContent = "\uE790";
            var actionContent = new FlourishComboBox
            {
                Width = 160,
                ItemsSource = new[] { "System", "Light", "Dark" },
                SelectedIndex = 0,
            };
            var card = new ListCard
            {
                Width = 520,
                Icon = iconContent,
                Title = "Theme",
                MainText = "Choose the application color theme.",
                ActionBody = actionContent,
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

                var icon = AssertTemplatePart<ContentPresenter>(
                    card,
                    "IconHost"
                );
                var copy = AssertTemplatePart<StackPanel>(card, "CopyHost");
                var action = AssertTemplatePart<ContentPresenter>(card, "ActionHost");
                Assert.Equal(0, Grid.GetColumn(icon));
                Assert.Equal(1, Grid.GetColumn(copy));
                Assert.Equal(2, Grid.GetColumn(action));
                Assert.Equal(VerticalAlignment.Center, icon.VerticalAlignment);
                Assert.Equal(VerticalAlignment.Center, copy.VerticalAlignment);
                Assert.Equal(VerticalAlignment.Center, action.VerticalAlignment);
                Assert.Equal(new Thickness(8, 0, 24, 0), icon.Margin);
                Assert.Equal(iconContent, icon.Content);
                Assert.Same(actionContent, action.Content);
                var title = AssertTemplatePart<FlourishTextBlock>(card, "TitleHost");
                var description = AssertTemplatePart<FlourishTextBlock>(
                    card,
                    "MainTextHost"
                );
                Assert.Equal(TextWrapping.NoWrap, title.TextWrapping);
                Assert.Equal(TextWrapping.NoWrap, description.TextWrapping);
                Assert.Equal(TextTrimming.CharacterEllipsis, title.TextTrimming);
                Assert.Equal(
                    TextTrimming.CharacterEllipsis,
                    description.TextTrimming
                );

                var emptyIcon = AssertTemplatePart<ContentPresenter>(
                    empty,
                    "IconHost"
                );
                var emptyAction = AssertTemplatePart<ContentPresenter>(
                    empty,
                    "ActionHost"
                );
                Assert.Equal(Visibility.Collapsed, emptyIcon.Visibility);
                Assert.Equal(new Thickness(), emptyIcon.Margin);
                Assert.Equal(Visibility.Collapsed, emptyAction.Visibility);
                Assert.Equal(new Thickness(), emptyAction.Margin);
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
                Assert.Null(outputCard.Template.FindName("MainTextHost", outputCard));
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
                        Icon = "\uE8A5",
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
    public void IconCard_ArrangesOneIconAtAnyDockAndCollapsesAbsentIcons()
    {
        RunInSta(() =>
        {
            var cards = Enum.GetValues<Dock>()
                .Select(position =>
                    new IconCard
                    {
                        Icon = "\uE8A5",
                        IconPosition = position,
                        Title = "Title",
                        MainText = "Description",
                    }
                )
                .ToArray();
            var nullIcon = new IconCard { Title = "Null icon" };
            var emptyIcon = new IconCard { Title = "Empty icon", Icon = "" };
            var panel = new StackPanel();
            foreach (var card in cards)
            {
                panel.Children.Add(card);
            }

            panel.Children.Add(nullIcon);
            panel.Children.Add(emptyIcon);
            var window = CreateWindow(panel);

            try
            {
                window.Show();
                window.UpdateLayout();

                foreach (var card in cards)
                {
                    card.ApplyTemplate();
                    var icon = AssertTemplatePart<ContentPresenter>(card, "IconHost");
                    Assert.Equal(card.IconPosition, DockPanel.GetDock(icon));
                    Assert.Equal(card.Icon, icon.Content);
                    Assert.Equal(
                        card.IconPosition switch
                        {
                            Dock.Left => new Thickness(0, 0, 16, 0),
                            Dock.Top => new Thickness(0, 0, 0, 16),
                            Dock.Right => new Thickness(16, 0, 0, 0),
                            Dock.Bottom => new Thickness(0, 16, 0, 0),
                            _ => throw new InvalidOperationException(),
                        },
                        icon.Margin
                    );
                    Assert.Equal(
                        Visibility.Visible,
                        AssertTemplatePart<StackPanel>(card, "CopyHost").Visibility
                    );
                }

                foreach (var card in new[] { nullIcon, emptyIcon })
                {
                    card.ApplyTemplate();
                    var icon = AssertTemplatePart<ContentPresenter>(card, "IconHost");
                    Assert.Equal(Visibility.Collapsed, icon.Visibility);
                    Assert.Equal(new Thickness(), icon.Margin);
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

    private static void AssertPresenterLayout(
        Presenter presenter,
        int presentationColumn,
        int copyColumn,
        int columnSpan,
        Visibility scrimVisibility
    )
    {
        var presentation = AssertTemplatePart<Border>(
            presenter,
            "PresentationSurface"
        );
        var copy = AssertTemplatePart<Border>(presenter, "CopySurface");
        var scrim = AssertTemplatePart<Border>(presenter, "OverlayScrim");

        Assert.Equal(presentationColumn, Grid.GetColumn(presentation));
        Assert.Equal(copyColumn, Grid.GetColumn(copy));
        Assert.Equal(columnSpan, Grid.GetColumnSpan(presentation));
        Assert.Equal(columnSpan, Grid.GetColumnSpan(copy));
        Assert.Equal(scrimVisibility, scrim.Visibility);
    }

    private static double MeasureParagraphIndent(
        System.Windows.Controls.TextBlock textBlock
    )
    {
        var typeface = new Typeface(
            textBlock.FontFamily,
            textBlock.FontStyle,
            textBlock.FontWeight,
            textBlock.FontStretch
        );
        var formattedText = new FormattedText(
            "    ",
            CultureInfo.CurrentCulture,
            textBlock.FlowDirection,
            typeface,
            textBlock.FontSize,
            textBlock.Foreground,
            VisualTreeHelper.GetDpi(textBlock).PixelsPerDip
        );

        return formattedText.WidthIncludingTrailingWhitespace;
    }

    private static T? FindVisualDescendant<T>(DependencyObject root, string name)
        where T : FrameworkElement
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is T candidate && candidate.Name == name)
            {
                return candidate;
            }

            var descendant = FindVisualDescendant<T>(child, name);
            if (descendant is not null)
            {
                return descendant;
            }
        }

        return null;
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
