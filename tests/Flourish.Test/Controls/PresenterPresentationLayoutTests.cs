using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ArkheideSystem.Flourish.Controls;
using ArkheideSystem.Flourish.Internal.Layout;
using FlourishListBox = ArkheideSystem.Flourish.Controls.ListBox;

namespace ArkheideSystem.Flourish.Test.Controls;

public sealed class PresenterPresentationLayoutTests
{
    private const string GenericThemeSource =
        "/Flourish;component/Themes/Generic.xaml";

    [Theory]
    [InlineData(PresenterMode.Split)]
    [InlineData(PresenterMode.TopDown)]
    [InlineData(PresenterMode.Overlay)]
    public void Presentation_UsesTheRealPageLayoutAndHonorsEveryLayoutContract(
        PresenterMode presenterMode
    )
    {
        StaTest.Run(() =>
        {
            var fillingContent = new Border { Background = Brushes.Transparent };
            var fixedContent = new Border
            {
                Width = 120,
                Height = 64,
                Background = Brushes.Transparent,
            };
            var horizontalGroup = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Children =
                {
                    CreateMarker(72, 40),
                    CreateMarker(32, 56, new Thickness(12, 0, 0, 0)),
                    CreateMarker(48, 32, new Thickness(12, 0, 0, 0)),
                },
            };
            var verticalGroup = new StackPanel
            {
                Children =
                {
                    CreateMarker(96, 24),
                    CreateMarker(56, 36, new Thickness(0, 10, 0, 0)),
                },
            };
            var registryList = new FlourishListBox
            {
                MinHeight = 48,
                MaxHeight = 80,
                Margin = new Thickness(0, 10, 0, 0),
                Items =
                {
                    "Registered command",
                    "Registered shortcut",
                },
            };
            verticalGroup.Children.Add(registryList);
            var textContent = new FlourishTextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Role = FlourishTextRole.Icon,
                Text = "\uE950",
            };

            var fillingPresenter = CreatePresenter(fillingContent, presenterMode);
            var fixedPresenter = CreatePresenter(fixedContent, presenterMode);
            var horizontalPresenter = CreatePresenter(horizontalGroup, presenterMode);
            var verticalPresenter = CreatePresenter(verticalGroup, presenterMode);
            var textPresenter = CreatePresenter(textContent, presenterMode);
            var page = CreatePage(
                fillingPresenter,
                fixedPresenter,
                horizontalPresenter,
                verticalPresenter,
                textPresenter
            );
            var window = CreateWindow(page);

            try
            {
                window.Show();
                window.UpdateLayout();

                AssertPresentationHostFillsSurface(fillingPresenter);
                AssertPresentationHostFillsSurface(fixedPresenter);
                AssertPresentationHostFillsSurface(horizontalPresenter);
                AssertPresentationHostFillsSurface(verticalPresenter);
                AssertPresentationHostFillsSurface(textPresenter);

                AssertFillsSurface(fillingContent, fillingPresenter);
                AssertCentered(fixedContent, fixedPresenter);
                AssertVisibleChildrenCentered(horizontalGroup, horizontalPresenter);
                AssertVisibleChildrenCentered(verticalGroup, verticalPresenter);
                AssertVerticalGroupFillsCrossAxis(
                    verticalGroup,
                    registryList,
                    verticalPresenter
                );
                AssertCentered(textContent, textPresenter);
                Assert.True(
                    textContent.ActualHeight
                        < GetTemplatePart<Border>(textPresenter, "PresentationSurface")
                            .ActualHeight
                );
                Assert.Equal(HorizontalAlignment.Stretch, horizontalGroup.HorizontalAlignment);
                Assert.Equal(VerticalAlignment.Stretch, horizontalGroup.VerticalAlignment);

                window.Width = 1040;
                window.Height = 760;
                window.UpdateLayout();

                AssertFillsSurface(fillingContent, fillingPresenter);
                AssertCentered(fixedContent, fixedPresenter);
                AssertVisibleChildrenCentered(horizontalGroup, horizontalPresenter);
                AssertVisibleChildrenCentered(verticalGroup, verticalPresenter);
                AssertVerticalGroupFillsCrossAxis(
                    verticalGroup,
                    registryList,
                    verticalPresenter
                );
                AssertCentered(textContent, textPresenter);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Theory]
    [InlineData(PresenterMode.Split)]
    [InlineData(PresenterMode.TopDown)]
    [InlineData(PresenterMode.Overlay)]
    public void HeaderChunk_PresentationUsesTheSameFillAndGroupCenteringContract(
        PresenterMode presenterMode
    )
    {
        StaTest.Run(() =>
        {
            var first = CreateMarker(52, 36);
            var second = CreateMarker(68, 28, new Thickness(14, 0, 0, 0));
            var group = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Children = { first, second },
            };
            var header = new HeaderChunk
            {
                Title = "Header",
                Content = "The presentation contract is shared by the Presenter family.",
                PresenterMode = presenterMode,
                PresenterPosition = PresenterPosition.Right,
                Presentation = group,
            };
            var pageBody = new PageBody();
            pageBody.Children.Add(header);
            var page = new Page { Content = pageBody };
            CenteredPageContentLayout.Apply(page, 880);
            var window = CreateWindow(page);

            try
            {
                window.Show();
                window.UpdateLayout();

                var surface = GetHeaderPresentationRegion(header);
                AssertVisibleChildrenCentered(group, surface);
            }
            finally
            {
                window.Close();
            }
        });
    }

    private static Border CreateMarker(
        double width,
        double height,
        Thickness margin = default
    )
    {
        return new Border
        {
            Width = width,
            Height = height,
            Margin = margin,
            Background = Brushes.Transparent,
        };
    }

    private static Presenter CreatePresenter(
        UIElement presentation,
        PresenterMode presenterMode
    )
    {
        return new Presenter
        {
            Title = "Presentation",
            Content = "Supporting copy follows the presentation.",
            Presentation = presentation,
            PresenterMode = presenterMode,
            PresenterPosition = PresenterPosition.Left,
            PresentationMinHeight = 160,
        };
    }

    private static Page CreatePage(params Presenter[] presenters)
    {
        var presenterStack = new StackPanel();
        foreach (var presenter in presenters)
        {
            presenterStack.Children.Add(presenter);
        }

        var pageBody = new PageBody();
        pageBody.Children.Add(
            new Chunk
            {
                Title = "Presenter layout",
                Body = presenterStack,
            }
        );
        var page = new Page { Content = pageBody };
        CenteredPageContentLayout.Apply(page, 880);
        return page;
    }

    private static void AssertPresentationHostFillsSurface(Presenter presenter)
    {
        var surface = GetTemplatePart<Border>(presenter, "PresentationSurface");
        var host = GetTemplatePart<FrameworkElement>(presenter, "PresentationHost");
        var origin = GetOrigin(host, surface);

        Assert.Equal(0, origin.X, 3);
        Assert.Equal(0, origin.Y, 3);
        Assert.Equal(surface.ActualWidth, host.ActualWidth, 3);
        Assert.Equal(surface.ActualHeight, host.ActualHeight, 3);
    }

    private static void AssertFillsSurface(FrameworkElement content, Presenter presenter)
    {
        var surface = GetTemplatePart<Border>(presenter, "PresentationSurface");
        var origin = GetOrigin(content, surface);

        Assert.Equal(0, origin.X, 3);
        Assert.Equal(0, origin.Y, 3);
        Assert.Equal(surface.ActualWidth, content.ActualWidth, 3);
        Assert.Equal(surface.ActualHeight, content.ActualHeight, 3);
    }

    private static void AssertCentered(FrameworkElement content, Presenter presenter)
    {
        AssertCentered(content, GetTemplatePart<Border>(presenter, "PresentationSurface"));
    }

    private static void AssertCentered(FrameworkElement content, FrameworkElement surface)
    {
        var origin = GetOrigin(content, surface);

        AssertClose((surface.ActualWidth - content.ActualWidth) / 2, origin.X);
        AssertClose((surface.ActualHeight - content.ActualHeight) / 2, origin.Y);
    }

    private static void AssertVisibleChildrenCentered(
        Panel group,
        Presenter presenter
    )
    {
        AssertVisibleChildrenCentered(
            group,
            GetTemplatePart<Border>(presenter, "PresentationSurface")
        );
    }

    private static void AssertVerticalGroupFillsCrossAxis(
        StackPanel group,
        FrameworkElement stretchableChild,
        Presenter presenter
    )
    {
        var surface = GetTemplatePart<Border>(presenter, "PresentationSurface");
        var groupOrigin = GetOrigin(group, surface);

        Assert.Equal(Orientation.Vertical, group.Orientation);
        Assert.Equal(0, groupOrigin.X, 3);
        Assert.Equal(surface.ActualWidth, group.ActualWidth, 3);
        Assert.Equal(group.ActualWidth, stretchableChild.ActualWidth, 3);
        AssertClose((surface.ActualHeight - group.ActualHeight) / 2, groupOrigin.Y);
    }

    private static void AssertVisibleChildrenCentered(
        Panel group,
        FrameworkElement surface
    )
    {
        Rect? union = null;
        foreach (FrameworkElement child in group.Children)
        {
            var origin = GetOrigin(child, surface);
            var bounds = new Rect(origin, child.RenderSize);
            union = union is null ? bounds : Rect.Union(union.Value, bounds);
        }

        var visibleBounds = Assert.IsType<Rect>(union);
        AssertClose(
            surface.ActualWidth / 2,
            visibleBounds.Left + visibleBounds.Width / 2
        );
        AssertClose(
            surface.ActualHeight / 2,
            visibleBounds.Top + visibleBounds.Height / 2
        );
    }

    private static FrameworkElement GetHeaderPresentationRegion(HeaderChunk header)
    {
        return GetTemplatePart<FrameworkElement>(header, "PresentationHost");
    }

    private static void AssertClose(double expected, double actual)
    {
        Assert.InRange(actual, expected - 0.51, expected + 0.51);
    }

    private static Point GetOrigin(Visual element, Visual ancestor)
    {
        return element.TransformToAncestor(ancestor).Transform(new Point());
    }

    private static T GetTemplatePart<T>(Control control, string name)
        where T : DependencyObject
    {
        control.ApplyTemplate();
        return Assert.IsAssignableFrom<T>(control.Template.FindName(name, control));
    }

    private static Window CreateWindow(UIElement content)
    {
        var window = new Window
        {
            Width = 960,
            Height = 640,
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
