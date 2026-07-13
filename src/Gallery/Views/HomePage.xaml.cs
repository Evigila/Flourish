using System.Windows.Controls;
using ArkheideSystem.Flourish.Abstract;

namespace ArkheideSystem.Gallery.Views;

public partial class HomePage : Page
{
    private readonly INavigationService navigation;

    public HomePage(INavigationService navigation)
    {
        this.navigation = navigation;
        InitializeComponent();
    }

    private void DemoCard_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        if (sender is Button { Tag: string route })
        {
            navigation.Navigate(route);
        }
    }

    private void DemoGrid_SizeChanged(
        object sender,
        System.Windows.SizeChangedEventArgs e
    )
    {
        DemoGrid.Columns = e.NewSize.Width switch
        {
            >= 1240 => 4,
            >= 760 => 3,
            >= 500 => 2,
            _ => 1,
        };
    }
}
