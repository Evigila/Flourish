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
            .ConfigureData(
                data =>
                {
                    data.SetAppCompany("Arkheide System Team").SetAppName("Flourish Gallery");
                }
            )
            .ConfigureServices(
                (_, services) =>
                {
                    services.AddSingleton<App>();
                    services.AddSingleton<ICommandParser, GalleryCommandParser>();

                    services.AddNavigable<HomePage>(
                        "Home",
                        "\uE80F",
                        navigationKey: GalleryNavigationRoutes.Home
                    );
                    services.AddNavigable<GalleryPage>(
                        "Gallery",
                        "\uE91B",
                        navigationKey: GalleryNavigationRoutes.Gallery
                    );
                    services.AddNavigable<EditorPage>(
                        "Editor",
                        "\uE70F",
                        navigationKey: GalleryNavigationRoutes.Editor
                    );
                    services.AddNavigable<SettingsPage>(
                        "Settings",
                        "\uE713",
                        navigationKey: GalleryNavigationRoutes.Settings
                    );
                    services.AddNavigable<TreeParentPage>(
                        "Tree",
                        "\uE8A5",
                        navigationKey: GalleryNavigationRoutes.TreeParent
                    );
                    services.AddNavigable<Page1>(
                        "Page1",
                        "\uE8A5",
                        navigationKey: GalleryNavigationRoutes.Page1
                    );
                    services.AddNavigable<Page2>(
                        "Page2",
                        "\uE8A5",
                        navigationKey: GalleryNavigationRoutes.Page2
                    );
                }
            )
            .ConfigureShell(
                shell =>
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
                }
            )
            .ConfigureTitleBar(
                titlebar =>
                {
                    titlebar
                        .SetBreadcrumbButton()
                        .SetNavToggle()
                        .SetLogo("pack://application:,,,/Flourish;component/Assets/favicon.ico")
                        .SetTitle("Gallery")
                        .SetSubTitle("Flourish 示例")
                        .SetProfile(NameOrder.FirstLast)
                        .SetThemeToggle(FlourishTheme.System)
                        .SetSearch(
                            "输入 message 测试搜索回调",
                            (services, text) =>
                            {
                                if (
                                    !string.Equals(
                                        text,
                                        "message",
                                        StringComparison.OrdinalIgnoreCase
                                    )
                                )
                                {
                                    return;
                                }

                                services
                                    .GetRequiredService<IMessageService>()
                                    .Show(
                                        "Search handler invoked.",
                                        "Gallery",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Information
                                    );
                            }
                        );
                }
            )
            .ConfigureNavigation(
                nav =>
                {
                    nav.SetDirection()
                        .SetInitiallyOpen()
                        .SetPanelWidth(
                            openWidth: 220,
                            closedWidth: 48,
                            maxWidth: 480,
                            minWidth: 150
                        )
                        .SetGroup(null,
                            groupId: 0,
                            group =>
                            {
                                group.AddNavigableViewItem(
                                    GalleryNavigationRoutes.Home,
                                    isInitial: true
                                );
                                group.AddNavigableViewItem(GalleryNavigationRoutes.Gallery);
                                group.AddNavigableViewItem(GalleryNavigationRoutes.Editor);
                            }
                        )
                        .SetGroup(
                            "按钮",
                            groupId: 1,
                            group =>
                            {
                                group.AddNavigableItem(
                                    "Hello",
                                    "demo.hello",
                                    iconGlyph: "\uE8F2"
                                );
                                group.AddNavigableItem(
                                    "World",
                                    "demo.world",
                                    iconGlyph: "\uE774"
                                );
                            }
                        )
                        .SetGroup(
                            "Tree",
                            groupId: 2,
                            group =>
                            {
                                group.AddNavigableViewItem(
                                    GalleryNavigationRoutes.TreeParent,
                                    parentId: 1
                                );
                                group.AddNavigableItem(
                                    "Button1",
                                    "tree.button1",
                                    childId: 1,
                                    iconGlyph: "\uE8B7"
                                );
                                group.AddNavigableItem(
                                    "Button2",
                                    "tree.button2",
                                    childId: 1,
                                    iconGlyph: "\uE8B7"
                                );

                                group.AddNavigableItem(
                                    "普通父节点",
                                    null,
                                    parentId: 2,
                                    iconGlyph: "\uE8A5"
                                );
                                group.AddNavigableViewItem(
                                    GalleryNavigationRoutes.Page1,
                                    childId: 2
                                );
                                group.AddNavigableViewItem(
                                    GalleryNavigationRoutes.Page2,
                                    childId: 2
                                );
                            }
                        )
                        .AddFixedNavigableViewItem(GalleryNavigationRoutes.Settings)
                        .AddFixedNavigableItem("关于", "app.about", iconGlyph: "\uE946");
                }
            )
            .ConfigureDynamicToolbar(
                tool =>
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
                }
            )
            .ConfigureStatusBar(
                statusBar =>
                {
                    statusBar
                        .SetStatusText("就绪")
                        .ShowLANConnectionStatus()
                        .ShowPowerStatus();
                }
            )
            .ConfigureMotion(
                motion =>
                {
                    motion
                        .EnableHoverRevealAnimation()
                        .EnableNavigationPanelTransition()
                        .EnablePageTransition();
                }
            )
            .ConfigureWindow(window =>
                window
                    .SetWindowSize()
                    .SetWindowMinSize()
                    .SetWindowPosition()
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
