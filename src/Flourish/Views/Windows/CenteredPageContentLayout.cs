using System.Windows;
using System.Windows.Controls;
using WpfPage = System.Windows.Controls.Page;

namespace ArkheideSystem.Flourish.Views.Windows;

internal static class CenteredPageContentLayout
{
    public static void Apply(WpfPage page, double? contentWidth)
    {
        ArgumentNullException.ThrowIfNull(page);

        if (
            contentWidth is { } width
            && (double.IsNaN(width) || double.IsInfinity(width) || width <= 0)
        )
        {
            throw new ArgumentOutOfRangeException(
                nameof(contentWidth),
                contentWidth,
                "Content width must be greater than 0."
            );
        }

        var maximumWidth = contentWidth ?? double.PositiveInfinity;
        if (page.Content is ScrollViewer scrollViewer)
        {
            WrapContent(scrollViewer, maximumWidth);
            return;
        }

        if (page.Content is CenteredPageContentPresenter presenter)
        {
            presenter.MaxWidth = maximumWidth;
            return;
        }

        if (page.Content is null)
        {
            return;
        }

        var content = page.Content;
        page.Content = null;
        page.Content = CreatePresenter(content, maximumWidth);
    }

    private static void WrapContent(ContentControl owner, double contentWidth)
    {
        if (owner.Content is CenteredPageContentPresenter presenter)
        {
            presenter.MaxWidth = contentWidth;
            return;
        }

        if (owner.Content is null)
        {
            return;
        }

        var content = owner.Content;
        owner.Content = null;
        owner.Content = CreatePresenter(content, contentWidth);
    }

    private static CenteredPageContentPresenter CreatePresenter(
        object content,
        double contentWidth
    )
    {
        var presenter = new CenteredPageContentPresenter
        {
            Content = content,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
            MaxWidth = contentWidth,
            SnapsToDevicePixels = true,
            UseLayoutRounding = true,
            VerticalAlignment = System.Windows.VerticalAlignment.Stretch,
        };
        presenter.SetResourceReference(
            FrameworkElement.MarginProperty,
            "FlourishContentBodyMargin"
        );
        return presenter;
    }
}

internal sealed class CenteredPageContentPresenter : ContentPresenter { }
