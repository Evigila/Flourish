using System.Windows;
using AcksheedSys.Flourish.Abstract;
using AcksheedSys.Gallery.Views;
using Microsoft.Extensions.DependencyInjection;

namespace AcksheedSys.Gallery;

internal static class Program
{
    private static IFlourish? flourish;

    public static IFlourish Flourish =>
        flourish ?? throw new InvalidOperationException("Flourish has not been built.");

    public static T Fetch<T>()
        where T : notnull
    {
        return Flourish.GetRequiredService<T>();
    }

    [STAThread]
    public static int Main(string[] args)
    {
        flourish = FlourishBuilder
            .CreateDefaultBuilder(args)
            .ConfigureServices(
                (_, services) =>
                {
                    services.AddSingleton<App>();

                    services.AddNavigable<HomePage>("首页", "\uE80F", isInitial: true);
                    services.AddNavigable<GalleryPage>("图库", "\uE91B");
                    services.AddNavigable<EditorPage>("编辑", "\uE70F");
                    services.AddNavigable<SettingsPage>("设置", "\uE713");
                }
            )
            .ConfigureShell(
                (_, shell) =>
                {
                    shell
                        .UseTitlebar(
                            EnableSearch: true,
                            EnableBreadcrumb: true,
                            EnableNavToggle: true,
                            EnableLogo: true,
                            EnableTitle: true,
                            EnableSubTitle: true,
                            EnableProfile: true,
                            EnableTrayExit: false
                        )
                        .SetTitle("Gallery")
                        .SetSubtitle("Flourish示例")
                        .SetFont("Microsoft YaHei", 14)
                        .SetSearchPlaceholder("搜索图片")
                        .SetBreadcrumbBehavior(BreadcrumbShowOption.Auto)
                        .UseNavigationPanel(
                            enabled: true,
                            direction: NavigationPanelDirection.Left,
                            isInitiallyOpen: false,
                            title: "导航"
                        )
                        .UseDynamicToolbar()
                        .UseMaterialEffect(MaterialEffect.Mica)
                        .SetWindowSize(1536, 864)
                        .SetWindowMinSize(1280, 720)
                        .SetWindowPosition(WindowStartupLocation.CenterScreen);
                }
            )
            .ConfigureDynamicToolbar(
                (_, tool) =>
                {
                    tool.CreateToolbarItems<HomePage>(
                        new("打开", "\uE8E5", "home.open"),
                        new("保存", "\uE74E", "home.save")
                    );

                    tool.CreateToolbarItems<GalleryPage>(
                        new("打开", "\uE8E5", "gallery.open"),
                        new("保存", "\uE74E", "gallery.save"),
                        new("导入", "\uE898", "gallery.import")
                    );
                }
            )
            .ConfigureStatus(
                (_, status) =>
                {
                    status.SetStatusText("就绪").ShowLANConnectionStatus().ShowPowerStatus();
                }
            )
            .Build();

        try
        {
            flourish.Start();
            var app = flourish.GetRequiredService<App>();
            return app.Run();
        }
        finally
        {
            flourish.StopAsync().GetAwaiter().GetResult();
            flourish.Dispose();
            flourish = null;
        }
    }
}
