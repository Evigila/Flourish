using System.Windows;
using ArkheideSystem.Gallery.Localization;
using Microsoft.Extensions.DependencyInjection;

namespace ArkheideSystem.Gallery;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        var localization = Program.Flourish.GetRequiredService<GalleryLocalizationService>();
        var shellLocalization =
            Program.Flourish.GetRequiredService<GalleryShellLocalizationCoordinator>();
        localization.Start(Dispatcher);
        shellLocalization.Start();
    }
}
