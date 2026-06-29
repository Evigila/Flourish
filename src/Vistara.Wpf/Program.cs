using Flourish.Models;
using Flourish.Services;
using Flourish.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vistara.Wpf.Views;

namespace Vistara.Wpf;

internal static class Program
{
    private static IHost? host;

    public static T Fetch<T>()
        where T : class
    {
        return (host?.Services.GetRequiredService(typeof(T)) as T)
            ?? throw new InvalidOperationException("Cannot find service of specified type.");
    }

    [STAThread]
    public static int Main(string[] args)
    {
        ConfigureHost(args);

        try
        {
            host!.Start();
            var app = Fetch<App>();
            return app.Run();
        }
        finally
        {
            host?.StopAsync().GetAwaiter().GetResult();
            host?.Dispose();
        }
    }

    private static void ConfigureHost(string[] args)
    {
        host = Host.CreateDefaultBuilder(args)
            .ConfigureServices(
                (_, services) =>
                {
                    services.AddSingleton<App>();
                    services.AddSingleton<FlourishShellWindow>();
                    services.AddSingleton(CreateShellOptions());

                    services.AddSingleton<PageHistoryService>();
                    services.AddSingleton<INavigationService, NavigationService>();

                    services.AddTransient<HomePage>();
                    services.AddTransient<GalleryPage>();
                    services.AddTransient<EditorPage>();
                    services.AddTransient<SettingsPage>();
                }
            )
            .Build();
    }

    private static FlourishShellOptions CreateShellOptions()
    {
        return new FlourishShellOptions
        {
            Title = "Vistara",
            Subtitle = "图片管理器",
            PaneTitle = "导航",
            SearchPlaceholder = "搜索图片、文件夹或标签",
            StatusText = "就绪",
            LogoFallbackText = "V",
            InitialNavigationKey = "Home",
            NavigationItems =
            [
                new("Home", "首页", "\uE80F", typeof(HomePage)),
                new("Gallery", "图库", "\uE91B", typeof(GalleryPage)),
                new("Editor", "编辑", "\uE70F", typeof(EditorPage)),
                new("Settings", "设置", "\uE713", typeof(SettingsPage)),
            ],
            ToolbarItems = [new("打开", "\uE8E5"), new("保存", "\uE74E"), new("导入", "\uE898")],
            StatusItems =
            [
                new("本地映射", "\uE823"),
                new("未连接", "\uE701"),
                new("82%", "\uE850"),
            ],
        };
    }
}
