using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ArkheideSystem.Flourish.Controls;
using FlourishButton = ArkheideSystem.Flourish.Controls.Button;
using FlourishCheckBox = ArkheideSystem.Flourish.Controls.CheckBox;
using FlourishLabel = ArkheideSystem.Flourish.Controls.FlourishLabel;
using FlourishRadioButton = ArkheideSystem.Flourish.Controls.FlourishRadioButton;
using WpfControl = System.Windows.Controls.Control;

namespace ArkheideSystem.Flourish.Test.Controls;

public sealed class FlourishControlTextPresenterTests
{
    private const string GenericThemeSource = "/Flourish;component/Themes/Generic.xaml";
    private const string ControlPresenterStyleKey = "FlourishControlContentPresenterStyle";

    [Fact]
    public void TextLayoutModes_KeepFlowSpacingSeparateFromControlLineBoxes()
    {
        StaTest.Run(() =>
        {
            var flowText = new FlourishTextBlock { Text = "Ag" };
            var controlText = new FlourishTextBlock { Text = "Ag" };
            var statusControlText = new FlourishTextBlock
            {
                Role = FlourishTextRole.Status,
                Text = "12",
            };
            controlText.SetResourceReference(
                FrameworkElement.StyleProperty,
                "FlourishControlTextBlockStyle"
            );
            statusControlText.SetResourceReference(
                FrameworkElement.StyleProperty,
                "FlourishControlTextBlockStyle"
            );
            var panel = new StackPanel
            {
                Children = { flowText, controlText, statusControlText },
            };
            var window = CreateWindow(panel);

            try
            {
                window.Show();
                window.UpdateLayout();

                Assert.Equal(FlourishTextLayoutMode.Flow, flowText.LayoutMode);
                Assert.Equal(new Thickness(0, 0, 0, 1), flowText.Padding);
                Assert.Equal(17, flowText.ActualHeight, precision: 3);

                Assert.Equal(FlourishTextLayoutMode.Control, controlText.LayoutMode);
                Assert.Equal(new Thickness(), controlText.Padding);
                Assert.Equal(16, controlText.ActualHeight, precision: 3);
                Assert.Equal(LineStackingStrategy.BlockLineHeight, controlText.LineStackingStrategy);

                Assert.Equal(FlourishTextLayoutMode.Control, statusControlText.LayoutMode);
                Assert.Equal(new Thickness(), statusControlText.Padding);
                Assert.Equal(14, statusControlText.ActualHeight, precision: 3);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void SingleLineControlHosts_UseOneSharedTextLineBox()
    {
        StaTest.Run(() =>
        {
            var button = new FlourishButton { Content = "Ag", Icon = "\uE8A5" };
            var checkBox = new FlourishCheckBox { Content = "Ag" };
            var radioButton = new FlourishRadioButton { Content = "Ag" };
            var listItem = new FlourishListBoxItem { Content = "Ag" };
            var bunchedItem = new BunchedListBoxItem { Content = "Ag" };
            var comboItem = new FlourishComboBoxItem { Content = "Ag" };
            var comboBox = new FlourishComboBox { Items = { "Ag" }, SelectedIndex = 0 };
            var label = new FlourishLabel { Content = "Ag" };
            var searchBox = new FlourishSearchBox { Placeholder = "Ag" };
            var textBox = new FlourishTextBox { Text = "Ag" };
            var passwordBox = new FlourishPasswordBox { Password = "Ag" };
            var panel = new StackPanel
            {
                Children =
                {
                    button,
                    checkBox,
                    radioButton,
                    listItem,
                    bunchedItem,
                    comboItem,
                    comboBox,
                    label,
                    searchBox,
                    textBox,
                    passwordBox,
                },
            };
            var window = CreateWindow(panel);

            try
            {
                window.Show();
                window.UpdateLayout();

                var sharedStyle = Assert.IsType<Style>(window.FindResource(ControlPresenterStyleKey));
                foreach (var host in new[]
                {
                    FindTemplatePart<ContentPresenter>(button, "ContentHost"),
                    FindTemplatePart<ContentPresenter>(checkBox, "ContentHost"),
                    FindTemplatePart<ContentPresenter>(radioButton, "ContentHost"),
                    FindTemplatePart<ContentPresenter>(listItem, "ContentHost"),
                    FindTemplatePart<ContentPresenter>(bunchedItem, "ContentHost"),
                    FindTemplatePart<ContentPresenter>(comboItem, "ContentHost"),
                    FindTemplatePart<ContentPresenter>(comboBox, "SelectionContentSite"),
                    Assert.IsType<ContentPresenter>(FindVisualDescendant<ContentPresenter>(label)),
                })
                {
                    Assert.Same(sharedStyle, host.Style);
                    Assert.Equal(16, TextBlock.GetLineHeight(host));
                    Assert.Equal(
                        LineStackingStrategy.BlockLineHeight,
                        TextBlock.GetLineStackingStrategy(host)
                    );

                    var text = Assert.IsType<TextBlock>(FindVisualDescendant<TextBlock>(host));
                    Assert.Equal(16, text.ActualHeight, precision: 3);
                    var textBounds = text
                        .TransformToAncestor(host)
                        .TransformBounds(new Rect(text.RenderSize));
                    Assert.InRange(
                        Math.Abs((textBounds.Top + (textBounds.Height / 2)) - (host.ActualHeight / 2)),
                        0,
                        0.5
                    );
                }

                var iconStyle = Assert.IsType<Style>(
                    window.FindResource("FlourishIconContentPresenterStyle")
                );
                var iconHost = FindTemplatePart<ContentPresenter>(button, "IconHost");
                Assert.Same(iconStyle, iconHost.Style);
                Assert.Equal(22, TextBlock.GetLineHeight(iconHost));
                Assert.Equal(
                    LineStackingStrategy.BlockLineHeight,
                    TextBlock.GetLineStackingStrategy(iconHost)
                );

                var placeholder = FindTemplatePart<TextBlock>(searchBox, "PlaceholderText");
                var searchIcon = FindTemplatePart<TextBlock>(searchBox, "SearchIcon");
                Assert.Equal(16, placeholder.LineHeight);
                Assert.Equal(LineStackingStrategy.BlockLineHeight, placeholder.LineStackingStrategy);
                Assert.Equal(LineStackingStrategy.BlockLineHeight, searchIcon.LineStackingStrategy);

                foreach (var editor in new DependencyObject[]
                {
                    textBox,
                    searchBox,
                    FindTemplatePart<TextBox>(comboBox, "PART_EditableTextBox"),
                    FindTemplatePart<PasswordBox>(passwordBox, "PART_PasswordBox"),
                })
                {
                    Assert.Equal(16, TextBlock.GetLineHeight(editor));
                    Assert.Equal(
                        LineStackingStrategy.BlockLineHeight,
                        TextBlock.GetLineStackingStrategy(editor)
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
    public void SharedControlTextStyle_PreservesCustomContentAndTemplates()
    {
        StaTest.Run(() =>
        {
            var content = new Border { Width = 23, Height = 11 };
            var contentTemplate = new DataTemplate();
            var button = new FlourishButton { Content = content };
            var listItem = new FlourishListBoxItem
            {
                Content = "Templated",
                ContentTemplate = contentTemplate,
            };
            var panel = new StackPanel { Children = { button, listItem } };
            var window = CreateWindow(panel);

            try
            {
                window.Show();
                window.UpdateLayout();

                var buttonHost = FindTemplatePart<ContentPresenter>(button, "ContentHost");
                Assert.Same(content, buttonHost.Content);
                Assert.Same(content, FindVisualDescendant<Border>(buttonHost));
                Assert.Equal(23, content.ActualWidth, precision: 3);
                Assert.Equal(11, content.ActualHeight, precision: 3);

                var itemHost = FindTemplatePart<ContentPresenter>(listItem, "ContentHost");
                Assert.Same(contentTemplate, itemHost.ContentTemplate);
            }
            finally
            {
                window.Close();
            }
        });
    }

    private static T FindTemplatePart<T>(WpfControl control, string name)
        where T : DependencyObject
    {
        control.ApplyTemplate();
        return Assert.IsType<T>(control.Template.FindName(name, control));
    }

    private static T? FindVisualDescendant<T>(DependencyObject root)
        where T : DependencyObject
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is T match)
            {
                return match;
            }

            if (FindVisualDescendant<T>(child) is { } descendant)
            {
                return descendant;
            }
        }

        return null;
    }

    private static Window CreateWindow(UIElement content)
    {
        var window = new Window
        {
            Width = 640,
            Height = 720,
            Left = -10000,
            Top = -10000,
            ShowActivated = false,
            ShowInTaskbar = false,
            Content = content,
        };
        window.Resources.MergedDictionaries.Add(
            Assert.IsType<ResourceDictionary>(
                Application.LoadComponent(new Uri(GenericThemeSource, UriKind.Relative))
            )
        );
        return window;
    }
}
