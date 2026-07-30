using System.Windows;
using System.Windows.Controls;
using ArkheideSystem.Flourish.Abstract;
using UserControl = System.Windows.Controls.UserControl;
using WpfPage = System.Windows.Controls.Page;
using WpfPanel = System.Windows.Controls.Panel;

namespace ArkheideSystem.Flourish.Views.Windows;

internal partial class FlourishShellContentHost : UserControl
{
    private readonly IReadOnlyList<FrameworkElement> centeredHosts;

    public FlourishShellContentHost()
    {
        InitializeComponent();
        centeredHosts =
        [
            ContentHeaderRegionHost,
            ToolbarView,
            BreadcrumbLayoutHost,
            ContentFooterRegionHost,
        ];
    }

    internal FlourishToolbar Toolbar => ToolbarView;

    internal Frame NavigationFrame => RootFrame;

    internal WpfPage? CurrentPage => RootFrame.Content as WpfPage;

    internal FrameworkElement TransitionHost => PageTransitionContentHost;

    internal FrameworkElement LayoutHost => this;

    internal IReadOnlyList<FrameworkElement> CenteredHosts => centeredHosts;

    internal void SetBreadcrumbVisibility(bool isVisible)
    {
        BreadcrumbHost.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
    }

    internal void SetBreadcrumb(string text)
    {
        BreadcrumbText.Text = text;
        SetBreadcrumbVisibility(isVisible: true);
    }

    internal void ApplyCenteredLayout(double maximumWidth)
    {
        foreach (var host in centeredHosts)
        {
            host.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            host.MaxWidth = maximumWidth;
        }
    }

    internal void SetRegionContent(
        FlourishRegion region,
        IReadOnlyList<FrameworkElement> elements
    )
    {
        WpfPanel host = region switch
        {
            FlourishRegion.ContentHeader => ContentHeaderRegionHost,
            FlourishRegion.ContentFooter => ContentFooterRegionHost,
            FlourishRegion.ContentOverlay => (WpfPanel)ContentOverlayRegionHost,
            FlourishRegion.ToolbarStart => ToolbarView.StartRegion,
            FlourishRegion.ToolbarEnd => ToolbarView.EndRegion,
            _ => throw new ArgumentOutOfRangeException(
                nameof(region),
                region,
                "The region is not hosted by the shell content area."
            ),
        };

        SynchronizePanelChildren(host, elements);
        host.Visibility = elements.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private static void SynchronizePanelChildren(
        WpfPanel panel,
        IReadOnlyList<FrameworkElement> desiredChildren
    )
    {
        for (var index = 0; index < desiredChildren.Count; index++)
        {
            var desired = desiredChildren[index];
            if (index < panel.Children.Count && ReferenceEquals(panel.Children[index], desired))
            {
                continue;
            }

            var existingIndex = panel.Children.IndexOf(desired);
            if (existingIndex >= 0)
            {
                panel.Children.RemoveAt(existingIndex);
            }

            panel.Children.Insert(index, desired);
        }

        while (panel.Children.Count > desiredChildren.Count)
        {
            panel.Children.RemoveAt(panel.Children.Count - 1);
        }
    }
}
