using System.Windows;
using Flourish.Windows;

namespace Vistara.Wpf;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var mainWindow = Program.Fetch<FlourishShellWindow>();
        MainWindow = mainWindow;
        mainWindow.Show();
    }
}
