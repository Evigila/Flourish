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

                    services.AddNavigable<HomePage>("首页", "\uE80F");
                    services.AddNavigable<GalleryPage>("图库", "\uE91B");
                    services.AddNavigable<EditorPage>("编辑", "\uE70F");
                    services.AddNavigable<SettingsPage>("设置", "\uE713");
                    services.AddNavigable<TreeParentPage>("页面父节点", "\uE8A5");
                    services.AddNavigable<Page1>("Page1", "\uE8A5");
                    services.AddNavigable<Page2>("Page2", "\uE8A5");
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
                                nav.SetDirection()
                                    .SetInitiallyOpen()
                                    .SetGroup(
                                        "导航",
                                        GroupID: 0,
                                        group =>
                                        {
                                            group.AddNavigableViewItem<HomePage>(isInitial: true);
                                            group.AddNavigableViewItem<GalleryPage>();
                                            group.AddNavigableViewItem<EditorPage>();
                                        }
                                    )
                                    .SetGroup(
                                        "按钮",
                                        GroupID: 1,
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
                                        "树",
                                        GroupID: 2,
                                        group =>
                                        {
                                            group.AddNavigableViewItem<TreeParentPage>(parentID: 1);
                                            group.AddNavigableItem(
                                                "Button1",
                                                "tree.button1",
                                                childID: 1,
                                                iconGlyph: "\uE8B7"
                                            );
                                            group.AddNavigableItem(
                                                "Button2",
                                                "tree.button2",
                                                childID: 1,
                                                iconGlyph: "\uE8B7"
                                            );

                                            group.AddNavigableItem(
                                                "普通父节点",
                                                null,
                                                parentID: 2,
                                                iconGlyph: "\uE8A5"
                                            );
                                            group.AddNavigableViewItem<Page1>(childID: 2);
                                            group.AddNavigableViewItem<Page2>(childID: 2);
                                        }
                                    )
                                    .AddFixedNavigableViewItem<SettingsPage>()
                                    .AddFixedNavigableItem(
                                        "关于",
                                        "app.about",
                                        iconGlyph: "\uE946"
                                    );
                            }
                        )
                        .UseDynamicToolbar()
                        .UseMotion(
                            (_, motion) =>
                            {
                                motion
                                    .SetDuration()
                                    .SetHoverReveal()
                                    .SetNavigationPanelTransition()
                                    .SetPageTransition();
                            }
                        )
                        .UseMaterialEffect()
                        .SetGlobalFont("Microsoft YaHei")
                        .SetWindowProperty(
                            (_, window) =>
                            {
                                window.SetWindowSize().SetWindowMinSize().SetWindowPosition();
                            }
                        );
                }
            )
            .ConfigureDynamicToolbar(
                (_, tool) =>
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
