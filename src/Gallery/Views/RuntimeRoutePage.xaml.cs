using System.Windows;
using System.Windows.Controls;

namespace ArkheideSystem.Gallery.Views;

public partial class RuntimeRoutePage : Page
{
    private readonly Guid instanceId = Guid.NewGuid();
    private readonly DateTimeOffset createdAt = DateTimeOffset.Now;
    private readonly INavigationService navigation;

    public RuntimeRoutePage(INavigationService navigation)
    {
        this.navigation = navigation;
        InitializeComponent();
        Loaded += Page_Loaded;
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        InstanceText.Text = $"{instanceId:N}\nCreated {createdAt:HH:mm:ss.fff}";
        ParameterText.Text = navigation.CurrentParameter switch
        {
            null => "<none>",
            var value => value.ToString() ?? "<null>",
        };
    }
}
