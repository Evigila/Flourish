using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using ArkheideSystem.Flourish.Controls;
using ArkheideSystem.Flourish.Views.Windows;

namespace ArkheideSystem.Flourish.Test.Controls;

public sealed class PageBodyTests
{
    [Fact]
    public void Constructor_UsesAStackPanelForPageElements()
    {
        RunInSta(() =>
        {
            var first = new HeaderChunk();
            var second = new Chunk();
            var body = new PageBody();

            body.Children.Add(first);
            body.Children.Add(second);

            var panel = Assert.IsType<StackPanel>(body.Content);
            Assert.Equal(new UIElement[] { first, second }, panel.Children.Cast<UIElement>());
            Assert.Equal(
                nameof(PageBody.Children),
                typeof(PageBody).GetCustomAttribute<ContentPropertyAttribute>()?.Name
            );
        });
    }

    [Fact]
    public void ImplicitXamlContent_AcceptsHeaderAndChunks()
    {
        RunInSta(() =>
        {
            const string xaml = """
                <flourish:PageBody
                  xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                  xmlns:flourish="http://schemas.arkheide.system/flourish">
                  <flourish:HeaderChunk />
                  <flourish:Chunk />
                </flourish:PageBody>
                """;

            var body = Assert.IsType<PageBody>(XamlReader.Parse(xaml));

            Assert.Collection(
                body.Children.Cast<UIElement>(),
                child => Assert.IsType<HeaderChunk>(child),
                child => Assert.IsType<Chunk>(child)
            );
        });
    }

    [Fact]
    public void Children_RejectUnsupportedElementsAndInvalidHeaderPlacement()
    {
        RunInSta(() =>
        {
            var body = new PageBody();

            Assert.Throws<ArgumentException>(() => body.Children.Add(new Border()));

            body.Children.Add(new HeaderChunk());
            body.Children.Add(new Chunk());

            Assert.Throws<InvalidOperationException>(() =>
                body.Children.Add(new HeaderChunk())
            );
            Assert.Throws<InvalidOperationException>(() =>
                body.Children.Insert(0, new Chunk())
            );
        });
    }

    [Fact]
    public void CenteredLayout_WrapsThePanelAndPreservesTheChildrenCollection()
    {
        RunInSta(() =>
        {
            var initialChild = new HeaderChunk();
            var addedAfterLayout = new Chunk();
            var body = new PageBody();
            body.Children.Add(initialChild);
            var panel = Assert.IsType<StackPanel>(body.Content);
            var page = new Page { Content = body };

            CenteredPageContentLayout.Apply(page, 480);

            Assert.Same(body, page.Content);
            var presenter = Assert.IsType<CenteredPageContentPresenter>(body.Content);
            Assert.Same(panel, presenter.Content);
            Assert.Equal(480, presenter.MaxWidth);

            body.Children.Add(addedAfterLayout);
            Assert.Equal(
                new UIElement[] { initialChild, addedAfterLayout },
                panel.Children.Cast<UIElement>()
            );

            CenteredPageContentLayout.Apply(page, 720);

            Assert.Same(presenter, body.Content);
            Assert.Equal(720, presenter.MaxWidth);
        });
    }

    [Fact]
    public void PageMargin_TracksTheDynamicResource()
    {
        RunInSta(() =>
        {
            var body = new PageBody();
            var panel = Assert.IsType<StackPanel>(body.Content);
            body.Resources["FlourishPageContentMargin"] = new Thickness(0, 0, 0, 24);

            Assert.Equal(new Thickness(0, 0, 0, 24), panel.Margin);

            body.Resources["FlourishPageContentMargin"] = new Thickness(0, 0, 0, 48);

            Assert.Equal(new Thickness(0, 0, 0, 48), panel.Margin);
        });
    }

    private static void RunInSta(Action action)
    {
        Exception? exception = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception caught)
            {
                exception = caught;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (exception is not null)
        {
            ExceptionDispatchInfo.Capture(exception).Throw();
        }
    }
}
