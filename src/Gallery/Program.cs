using System.Windows;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Gallery.Views;
using Microsoft.Extensions.DependencyInjection;

namespace ArkheideSystem.Gallery;

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
            .ConfigureData(data =>
            {
                data.SetLocale("CN");
            })
            .ConfigureServices(
                (_, services) =>
                {
                    services.AddSingleton<App>();
                    services.AddSingleton<ICommandParser, GalleryCommandParser>();

                    services.AddNavigable<HomePage>("Home", "\uE80F");
                    services.AddNavigable<GalleryPage>("Gallery", "\uE91B");
                    services.AddNavigable<EditorPage>("Editor", "\uE70F");
                    services.AddNavigable<SettingsPage>("Settings", "\uE713");
                    services.AddNavigable<TreeParentPage>("Tree", "\uE8A5");
                    services.AddNavigable<Page1>("Page1", "\uE8A5");
                    services.AddNavigable<Page2>("Page2", "\uE8A5");
                }
            )
            .ConfigureShell(shell =>
            {
                shell
                    .UseTitleBar()
                    .UseNavigation()
                    .UseDynamicToolbar()
                    .UseMotion()
                    .UseStatusBar()
                    .UseTips()
                    .UseMaterialEffect()
                    .UseGlobalFont("Microsoft YaHei");
            })
            .ConfigureTitleBar(titlebar =>
            {
                titlebar
                    .SetBreadcrumbButton()
                    .SetNavToggle()
                    .SetLogo()
                    .SetTitle("Gallery")
                    .SetSubTitle("Flourish 示例")
                    .SetProfile(NameOrder.FirstLast)
                    .SetThemeToggle(FlourishTheme.System)
                    .SetSearch("输入 message 测试搜索回调", (_, text) => { });
            })
            .ConfigureNavigation(nav =>
            {
                nav.SetDirection()
                    .SetInitiallyOpen()
                    .SetPanelWidth(openWidth: 220, closedWidth: 48, maxWidth: 480, minWidth: 150)
                    .SetGroup(
                        null,
                        0,
                        group =>
                        {
                            group.AddNavigableViewItem<HomePage>(isInitial: true);
                            group.AddNavigableViewItem<GalleryPage>();
                            group.AddNavigableViewItem<EditorPage>();
                        }
                    )
                    .SetGroup(
                        "按钮",
                        1,
                        group =>
                        {
                            group.AddNavigableItem("Hello", "\uE8F2", "demo.hello");
                            group.AddNavigableItem("World", "\uE774", "demo.world");
                        }
                    )
                    .SetGroup(
                        "Tree",
                        2,
                        group =>
                        {
                            group.AddNavigableViewItem<TreeParentPage>(parentId: 1);
                            group.AddNavigableItem("Button1", "\uE8B7", "tree.button1", 0, 1);
                            group.AddNavigableItem("Button2", "\uE8B7", "tree.button2", 0, 1);
                            group.AddNavigableItem("普通父节点", "\uE8A5", null, 2, 0);
                            group.AddNavigableViewItem<Page1>(childId: 2);
                            group.AddNavigableViewItem<Page2>(childId: 2);
                        }
                    )
                    .AddFixedNavigableViewItem<SettingsPage>()
                    .AddFixedNavigableItem("关于", "\uE946", "app.about");
            })
            .ConfigureDynamicToolbar(tool =>
            {
                tool.CreateToolbarItems<HomePage>(
                    new FlourishToolbarItem("打开", "\uE8E5", "home.open"),
                    new FlourishToolbarItem("保存", "\uE74E", "home.save")
                );

                tool.CreateToolbarItems<GalleryPage>(
                    new FlourishToolbarItem("打开", "\uE8E5", "gallery.open"),
                    new FlourishToolbarItem("保存", "\uE74E", "gallery.save"),
                    new FlourishToolbarItem("导入", "\uE898", "gallery.import")
                );
            })
            .ConfigureStatusBar(statusBar =>
            {
                statusBar.SetStatusText("就绪").ShowLANConnectionStatus().ShowPowerStatus();
            })
            .ConfigureMotion(motion =>
            {
                motion
                    .EnableHoverRevealAnimation()
                    .EnableNavigationPanelTransition()
                    .EnablePageTransition();
            })
            .ConfigureWindow(window =>
                window.SetWindowSize().SetWindowMinSize().SetWindowPosition().SetTrayExit(false)
            )
            .Build();

        try
        {
            return flourish.Run<App>();
        }
        finally
        {
            flourish.Dispose();
            flourish = null;
        }
    }
}
