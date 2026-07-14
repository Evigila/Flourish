using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using ArkheideSystem.Flourish.Controls;
using FlourishButton = ArkheideSystem.Flourish.Controls.Button;
using WpfButton = System.Windows.Controls.Button;
using WpfControl = System.Windows.Controls.Control;

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
            "/Flourish;component/Controls/Card.xaml",
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
    public void TextRoles_UseRegularBodyWeightsAndBoldHeadingWeights()
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
                    var expected = textBlock.Role is
                        FlourishTextRole.CardTitle
                        or FlourishTextRole.SectionTitle
                        or FlourishTextRole.PageTitle
                        ? FontWeights.Bold
                        : FontWeights.Regular;
                    Assert.Equal(expected, textBlock.FontWeight);
                }
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
                typeof(ScrollViewer),
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
                new FlourishCard { Content = "Card" },
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
                new FlourishScrollViewer
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

                AssertTemplatePart<FrameworkElement>(button, "HoverChrome");
                AssertTemplatePart<ScaleTransform>(button, "HoverRevealScale");
                AssertTemplatePart<ScrollViewer>(textBox, "PART_ContentHost");
                AssertTemplatePart<ScrollViewer>(searchBox, "PART_ContentHost");
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

                Assert.Equal(30, icon.Width);
                Assert.Equal(30, icon.Height);
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
    public void ButtonDangerAppearance_UsesRedHoverRevealAndAllowsLocalOverride()
    {
        RunInSta(() =>
        {
            var standard = new FlourishButton { Content = "Standard" };
            var danger = new FlourishButton
            {
                Appearance = ButtonAppearance.Danger,
                Content = "Danger",
            };
            var local = new FlourishButton
            {
                Appearance = ButtonAppearance.Danger,
                Content = "Local",
            };
            var localBrush = new SolidColorBrush(Colors.MediumVioletRed);
            HoverReveal.SetOverrideColor(local, localBrush);
            var window = CreateWindow(
                new StackPanel { Children = { standard, danger, local } }
            );

            try
            {
                window.Show();
                window.UpdateLayout();
                standard.ApplyTemplate();
                danger.ApplyTemplate();
                local.ApplyTemplate();

                var standardBrush = Assert.IsType<SolidColorBrush>(
                    standard.TryFindResource("FlourishHoverRevealBrush")
                );
                var dangerBrush = Assert.IsType<SolidColorBrush>(
                    danger.TryFindResource("FlourishDangerHoverRevealBrush")
                );

                Assert.Equal(
                    standardBrush.Color,
                    Assert.IsType<SolidColorBrush>(HoverReveal.GetOverrideColor(standard))
                        .Color
                );
                Assert.Equal(
                    dangerBrush.Color,
                    Assert.IsType<SolidColorBrush>(HoverReveal.GetOverrideColor(danger))
                        .Color
                );
                Assert.Same(localBrush, HoverReveal.GetOverrideColor(local));
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
    public void CardAppearances_KeepContentSurfacesBorderlessAndElevateShowcasesOnly()
    {
        RunInSta(() =>
        {
            var standard = new FlourishCard { Content = "Standard" };
            var hero = new FlourishCard
            {
                Appearance = FlourishCardAppearance.Hero,
                Content = "Hero",
            };
            var window = CreateWindow(new StackPanel { Children = { standard, hero } });

            try
            {
                window.Show();
                window.UpdateLayout();
                standard.ApplyTemplate();
                hero.ApplyTemplate();

                Assert.Equal(new Thickness(), standard.BorderThickness);
                Assert.Equal(new Thickness(), hero.BorderThickness);
                var standardShadow = AssertTemplatePart<Border>(
                    standard,
                    "ShadowChrome"
                );
                var heroShadow = AssertTemplatePart<Border>(hero, "ShadowChrome");
                Assert.Equal(Visibility.Collapsed, standardShadow.Visibility);
                Assert.Equal(Visibility.Visible, heroShadow.Visibility);
                Assert.NotNull(heroShadow.Effect);
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
