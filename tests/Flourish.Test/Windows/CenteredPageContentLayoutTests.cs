using System.Runtime.ExceptionServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using ArkheideSystem.Flourish.Controls;
using ArkheideSystem.Flourish.Views.Windows;
using CustomScrollViewer = ArkheideSystem.Flourish.Controls.ScrollViewer;
using WpfScrollViewer = System.Windows.Controls.ScrollViewer;

namespace ArkheideSystem.Flourish.Test.Windows;

public sealed class CenteredPageContentLayoutTests
{
    private const string GenericThemeSource =
        "/Flourish;component/Themes/Generic.xaml";

    [Fact]
    public void Apply_WithoutWidthLimit_KeepsScrollableContentUnconstrained()
    {
        RunInSta(() =>
        {
            var scrollViewer = new WpfScrollViewer { Content = new Grid() };
            var page = new Page { Content = scrollViewer };

            CenteredPageContentLayout.Apply(page, contentWidth: null);

            Assert.Same(scrollViewer, page.Content);
            var presenter = Assert.IsType<CenteredPageContentPresenter>(
                scrollViewer.Content
            );
            Assert.Equal(double.PositiveInfinity, presenter.MaxWidth);
        });
    }

    [Fact]
    public void Apply_WithRootScrollViewer_LimitsOnlyTheScrollableContent()
    {
        RunInSta(() =>
        {
            var content = new Border { Height = 900 };
            var scrollViewer = new CustomScrollViewer
            {
                Content = content,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            };
            var page = new Page { Content = scrollViewer };

            CenteredPageContentLayout.Apply(page, 480);

            Assert.Same(scrollViewer, page.Content);
            var presenter = Assert.IsType<CenteredPageContentPresenter>(
                scrollViewer.Content
            );
            Assert.Same(content, presenter.Content);
            Assert.Equal(480, presenter.MaxWidth);
            Assert.Equal(HorizontalAlignment.Stretch, presenter.HorizontalAlignment);
        });
    }

    [Fact]
    public void Apply_WhenCalledAgain_ReusesTheCenteredPresenter()
    {
        RunInSta(() =>
        {
            var scrollViewer = new WpfScrollViewer { Content = new Grid() };
            var page = new Page { Content = scrollViewer };

            CenteredPageContentLayout.Apply(page, 480);
            var presenter = Assert.IsType<CenteredPageContentPresenter>(
                scrollViewer.Content
            );
            CenteredPageContentLayout.Apply(page, 720);

            Assert.Same(presenter, scrollViewer.Content);
            Assert.Equal(720, presenter.MaxWidth);
        });
    }

    [Fact]
    public void Apply_WithWideViewport_CentersContentAndLeavesScrollbarAtViewportEdge()
    {
        RunInSta(() =>
        {
            var content = new Border { Height = 900 };
            var scrollViewer = new CustomScrollViewer
            {
                Content = content,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            };
            var page = new Page { Content = scrollViewer };
            page.Resources["FlourishContentBodyMargin"] = new Thickness(32, 0, 32, 0);
            CenteredPageContentLayout.Apply(page, 480);

            var window = new Window
            {
                Width = 800,
                Height = 400,
                Left = -10_000,
                Top = -10_000,
                ShowActivated = false,
                ShowInTaskbar = false,
                WindowStartupLocation = WindowStartupLocation.Manual,
                Content = page,
            };
            window.Resources.MergedDictionaries.Add(
                LoadResourceDictionary(GenericThemeSource)
            );
            window.Show();

            try
            {
                PumpDispatcher();
                var presenter = Assert.IsType<CenteredPageContentPresenter>(
                    scrollViewer.Content
                );
                var verticalScrollBar = FindVisualDescendants<ScrollBar>(scrollViewer)
                    .Single(scrollBar => scrollBar.Orientation == Orientation.Vertical);
                var presenterOrigin = presenter.TranslatePoint(new Point(), scrollViewer);
                var scrollBarOrigin = verticalScrollBar.TranslatePoint(new Point(), scrollViewer);

                Assert.InRange(presenter.ActualWidth, 479, 481);
                Assert.InRange(
                    presenterOrigin.X,
                    (scrollViewer.ViewportWidth - presenter.ActualWidth) / 2 - 1,
                    (scrollViewer.ViewportWidth - presenter.ActualWidth) / 2 + 1
                );
                Assert.Equal(Visibility.Visible, verticalScrollBar.Visibility);
                Assert.InRange(
                    scrollBarOrigin.X + verticalScrollBar.ActualWidth,
                    scrollViewer.ActualWidth - 1,
                    scrollViewer.ActualWidth + 1
                );
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void Apply_WithNarrowViewport_UsesTheAvailableContentWidth()
    {
        RunInSta(() =>
        {
            var scrollViewer = new CustomScrollViewer
            {
                Content = new Border { Height = 900 },
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            };
            var page = new Page { Content = scrollViewer };
            page.Resources["FlourishContentBodyMargin"] = new Thickness(32, 0, 32, 0);
            CenteredPageContentLayout.Apply(page, 720);

            var window = new Window
            {
                Width = 360,
                Height = 320,
                Left = -10_000,
                Top = -10_000,
                ShowActivated = false,
                ShowInTaskbar = false,
                WindowStartupLocation = WindowStartupLocation.Manual,
                Content = page,
            };
            window.Resources.MergedDictionaries.Add(
                LoadResourceDictionary(GenericThemeSource)
            );
            window.Show();

            try
            {
                PumpDispatcher();
                var presenter = Assert.IsType<CenteredPageContentPresenter>(
                    scrollViewer.Content
                );

                Assert.InRange(
                    presenter.ActualWidth,
                    scrollViewer.ViewportWidth - 65,
                    scrollViewer.ViewportWidth - 63
                );
            }
            finally
            {
                window.Close();
            }
        });
    }

    private static IEnumerable<T> FindVisualDescendants<T>(DependencyObject root)
        where T : DependencyObject
    {
        for (
            var index = 0;
            index < System.Windows.Media.VisualTreeHelper.GetChildrenCount(root);
            index++
        )
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(root, index);
            if (child is T match)
            {
                yield return match;
            }

            foreach (var descendant in FindVisualDescendants<T>(child))
            {
                yield return descendant;
            }
        }
    }

    private static ResourceDictionary LoadResourceDictionary(string source)
    {
        return Assert.IsType<ResourceDictionary>(
            Application.LoadComponent(new Uri(source, UriKind.Relative))
        );
    }

    private static void PumpDispatcher()
    {
        Dispatcher.CurrentDispatcher.Invoke(
            DispatcherPriority.ApplicationIdle,
            new Action(() => { })
        );
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
            ExceptionDispatchInfo.Capture(error).Throw();
        }
    }
}
