using System.Windows.Controls;

namespace ArkheideSystem.Gallery.Views;

public partial class ControlLibraryPage : Page
{
    private readonly INavigationService navigation;

    public ControlLibraryPage(INavigationService navigation)
    {
        this.navigation = navigation;
        InitializeComponent();
    }

    private void ControlCard_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        if (sender is Button { Tag: string route })
        {
            navigation.Navigate(route);
        }
    }
}
