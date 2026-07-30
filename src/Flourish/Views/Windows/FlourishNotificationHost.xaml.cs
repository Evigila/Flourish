using System.Windows;
using System.Windows.Controls;
using UserControl = System.Windows.Controls.UserControl;

namespace ArkheideSystem.Flourish.Views.Windows;

internal partial class FlourishNotificationHost : UserControl
{
    public FlourishNotificationHost()
    {
        InitializeComponent();
    }

    internal StackPanel Items => ItemHost;

    internal void UpdateVisibility()
    {
        Visibility = ItemHost.Children.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
    }
}
