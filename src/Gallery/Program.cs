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
                    services.AddSingleton<ICommandParser, GalleryCommandParser>();

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
                            (_, titlebar) =>
                            {
                                titlebar
                                    .ShowSearch()
                                    .ShowBreadcrumb()
                                    .ShowNavToggle()
                                    .ShowLogo()
                                    .ShowTitle()
                                    .ShowSubTitle()
                                    .ShowProfile()
                                    .SetTrayExit()
                                    .SetBreadcrumbBehavior()
                                    .SetTitle("Gallery")
                                    .SetSubtitle("Flourish 示例")
                                    .SetSearchPlaceholder("搜索图片");
                            }
                        )
                        .UseNavigationPanel(
                            (_, nav) =>
                            {
                                nav.SetDirection().SetInitiallyOpen().SetTitle("导航");
                            }
                        )
                        .UseDynamicToolbar()
                        .UseMotion(
                            (_, motion) =>
                            {
                                motion
                                    .SetDuration(TimeSpan.FromMilliseconds(180))
                                    .SetNavigationPanelTransition()
                                    .SetPageTransition();
                            }
                        )
                        .UseMaterialEffect(MaterialEffect.Mica)
                        .SetGlobalFont("Microsoft YaHei", 14)
                        .SetWindowProperty(
                            (_, window) =>
                            {
                                window
                                    .SetWindowSize(1536, 864)
                                    .SetWindowMinSize(1280, 720)
                                    .SetWindowPosition(WindowStartupLocation.CenterScreen);
                            }
                        );
                }
            )
            .ConfigureDynamicToolbar(
                (_, tool) =>
                {
                    tool.CreateToolbarItems<HomePage>(
                        true,
                        new("打开", "\uE8E5", "home.open"),
                        new("保存", "\uE74E", "home.save")
                    );

                    tool.CreateToolbarItems<GalleryPage>(
                        true,
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
