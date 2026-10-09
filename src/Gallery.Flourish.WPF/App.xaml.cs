using System.Windows;
using System.Windows.Threading;
using ArkheideSystem.Flourish.WPF.Abstract;
using F = ArkheideSystem.Flourish.WPF.Controls;

namespace ArkheideSystem.Gallery.Flourish.WPF;

public partial class App : Application
{
    private bool reporting;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        DispatcherUnhandledException += ReportExampleFailure;
        MainWindow = new MainWindow();
        MainWindow.Show();
    }

    private async void ReportExampleFailure(object sender, DispatcherUnhandledExceptionEventArgs args)
    {
        Console.Error.WriteLine(args.Exception);
        if (reporting || MainWindow is not Window owner || !owner.IsVisible) return;
        args.Handled = true;
        reporting = true;
        try
        {
            using var dialog = new F.Dialog
            {
                Title = "Gallery example failed",
                Content = new F.Notice { Kind = NoticeKind.Error, Title = args.Exception.Message, Content = new F.CodeBlock { Text = args.Exception.ToString() } }
            };
            var close = new F.Button { Content = "Close" };
            close.Click += async (_, _) => await dialog.CloseAsync();
            dialog.Actions = close;
            await dialog.ShowAsync(owner);
        }
        finally { reporting = false; }
    }
}
