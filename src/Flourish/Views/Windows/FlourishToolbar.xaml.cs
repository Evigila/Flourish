using System.Windows;
using System.Windows.Controls;
using UserControl = System.Windows.Controls.UserControl;

namespace ArkheideSystem.Flourish.Views.Windows;

internal partial class FlourishToolbar : UserControl
{
    public FlourishToolbar()
    {
        InitializeComponent();
    }

    internal StackPanel Items => ItemHost;

    internal StackPanel StartRegion => StartRegionHost;

    internal StackPanel EndRegion => EndRegionHost;

    internal void UpdateVisibility(bool isEnabled)
    {
        ToolbarHostBorder.Visibility =
            isEnabled
            && (
                ItemHost.Children.Count > 0
                || StartRegionHost.Children.Count > 0
                || EndRegionHost.Children.Count > 0
            )
                ? Visibility.Visible
                : Visibility.Collapsed;
    }
}
