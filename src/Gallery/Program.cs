using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Gallery.Views;
using Microsoft.Extensions.DependencyInjection;

namespace ArkheideSystem.Gallery;

internal static class Program
{
    private static IFlourish? flourish;
    public static IFlourish Flourish =>
        flourish ?? throw new InvalidOperationException("Flourish has not been built.");

    [STAThread]
    public static int Main(string[] args)
    {
        flourish = FlourishBuilder
            .CreateDefaultBuilder(args)
            .ConfigureData(data => data.SetLocale("EN"))
            .ConfigureServices(
                (_, services) =>
                {
                    services.AddSingleton<App>();
                    services.AddCommandParser<GalleryCommandParser>();

                    services.AddNavigable<HomePage>("Overview", "\uE80F");
                    services.AddNavigable<ConfigurationPage>("Configuration", "\uE713");
                    services.AddNavigable<AppearancePage>("Appearance", "\uE790");
                    services.AddNavigable<TitleBarRuntimePage>("Title bar", "\uE8A4");
                    services.AddNavigable<NavigationRuntimePage>("Navigation", "\uE700");
                    services.AddNavigable<ToolbarStatusPage>("Regions", "\uE945");
                    services.AddNavigable<CommandsPage>("Commands", "\uE756");
                    services.AddNavigable<WindowRuntimePage>("Window", "\uE737");
                    services.AddNavigable<BackgroundTasksPage>("Background", "\uE895");
                    services.AddNavigable<ControlLibraryPage>("Controls", "\uE8D2");
                    services.AddNavigable<ChunkPage>("Chunk", "\uE7C8");
                    services.AddNavigable<ButtonPage>("Button", "\uE8FB");
                    services.AddNavigable<CardPage>("Card", "\uE8A5");
                }
            )
            .ConfigureShell(shell =>
            {
                shell
                    .UseTitleBar()
                    .UseNavigation()
                    .UseCenterContent()
                    .UseDynamicToolbar()
                    .UseMotion()
                    .UseStatusBar()
                    .UseTips()
                    .UseMaterialEffect()
                    .UseGlobalFont("Microsoft YaHei", 14);
            })
            .ConfigureTitleBar(titlebar =>
            {
                titlebar
                    .SetBreadcrumbButton()
                    .SetNavToggle()
                    .SetLogo()
                    .SetTitle("Flourish Gallery")
                    .SetSubTitle("Playground")
                    .SetProfile(NameOrder.FirstLast)
                    .SetThemeToggle(FlourishTheme.System)
                    .SetSearch("Type here to search", (_, _) => { });
            })
            .ConfigureNavigation(nav =>
            {
                nav.SetDirection()
                    .SetInitiallyOpen()
                    .SetPanelWidth(openWidth: 250, closedWidth: 56, maxWidth: 520, minWidth: 180)
                    .SetGroup(
                        null,
                        0,
                        group => group.AddNavigableViewItem<HomePage>(isInitial: true)
                    )
                    .SetGroup(
                        "Runtime",
                        1,
                        group =>
                        {
                            group.AddNavigableViewItem<ConfigurationPage>();
                            group.AddNavigableViewItem<CommandsPage>();
                            group.AddNavigableViewItem<BackgroundTasksPage>();
                        }
                    )
                    .SetGroup(
                        "Surfaces",
                        2,
                        group =>
                        {
                            group.AddNavigableViewItem<AppearancePage>();
                            group.AddNavigableViewItem<TitleBarRuntimePage>();
                            group.AddNavigableViewItem<NavigationRuntimePage>();
                            group.AddNavigableViewItem<ToolbarStatusPage>();
                            group.AddNavigableViewItem<WindowRuntimePage>();
                        }
                    )
                    .SetGroup(
                        "Controls",
                        3,
                        group =>
                        {
                            group.AddNavigableViewItem<ControlLibraryPage>(parentId: 1);
                            group.AddNavigableViewItem<ChunkPage>(childId: 1);
                            group.AddNavigableViewItem<ButtonPage>(childId: 1);
                            group.AddNavigableViewItem<CardPage>(childId: 1);
                        }
                    )
                    .SetGroup(
                        "Commands",
                        4,
                        group =>
                        {
                            group.AddNavigableItem("Message", "\uE8F2", "demo.hello");
                            group.AddNavigableItem("Task", "\uE895", "demo.background");
                        }
                    );
            })
            .ConfigureDynamicToolbar(toolbar =>
            {
                toolbar.CreateToolbarItems<HomePage>(
                    new FlourishToolbarItem("Say hello", "\uE8F2", "demo.hello"),
                    new FlourishToolbarItem("Queue task", "\uE895", "demo.background")
                );
            })
            .ConfigureStatusBar(statusBar =>
            {
                statusBar
                    .AddStatusItem("Online", "\uE930")
                    .ShowLANConnectionStatus()
                    .ShowPowerStatus();
            })
            .ConfigureMotion(motion =>
            {
                motion
                    .EnableHoverRevealAnimation()
                    .EnableNavigationPanelTransition()
                    .EnablePageTransition();
            })
            .ConfigureWindow(window =>
                window.SetWindowSize().SetWindowMinSize().SetWindowPosition()
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
