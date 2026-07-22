using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Gallery.Views;
using Microsoft.Extensions.DependencyInjection;

namespace ArkheideSystem.Gallery;

internal static class Program
{
    private static IFlourish? flourish;
    public static IFlourish Flourish => flourish ?? throw new InvalidOperationException();

    [STAThread]
    public static int Main(string[] args)
    {
        flourish = FlourishBuilder
            .CreateDefaultBuilder(args) //Create default builder as hosting
            .ConfigureServices( // Configure services as hosting
                (_, services) =>
                {
                    services.AddSingleton<App>();
                    services.AddCommandParser<GalleryCommandParser>(); // Mapping command key and its executor

                    services.AddNavigable<HomePage>("Overview", "\uE80F"); // Using AddNavigable instead of AddSingleton or other
                    services.AddNavigable<ConfigurationPage>("Configuration", "\uE713");
                    services.AddNavigable<AppearancePage>("Appearance", "\uE790");
                    services.AddNavigable<TitleBarRuntimePage>("Title bar", "\uE8A4");
                    services.AddNavigable<ProjectRuntimePage>("Projects", "\uE8F9");
                    services.AddNavigable<NavigationRuntimePage>("Navigation", "\uE700");
                    services.AddNavigable<ToolbarStatusPage>("Regions", "\uE945");
                    services.AddNavigable<CommandsPage>("Commands", "\uE756");
                    services.AddNavigable<WindowRuntimePage>("Window", "\uE737");
                    services.AddNavigable<BackgroundTasksPage>("Background", "\uE895");
                    services.AddNavigable<ControlLibraryPage>("Controls", "\uE8D2");
                    services.AddNavigable<ChunkPage>("Chunk", "\uE7C8");
                    services.AddNavigable<ButtonPage>("Button", "\uE8FB");
                    services.AddNavigable<CardPage>("Card", "\uE8A5");
                    services.AddNavigable<PresenterPage>("Presenter", "\uE8B9");
                    services.AddNavigable<ParagraphPage>("Paragraph", "\uE8D2");
                    services.AddNavigable<DataGridPage>("DataGrid", "\uE80A");
                    services.AddNavigable<OverlayPage>("Overlay", "\uE7B3");
                }
            )
            .ConfigureShell(shell => // Enable functionality at top level
            {
                shell
                    .UseCenterContent() // Use centered width restricted content
                    .UseDynamicToolbar() // Able to create toolbar items dynamically
                    .UseGlobalFont() // Set uniform global font family and sizes
                    .UseMaterialEffect() // Use windows DWM material effect for background
                    .UseMotion() // Use flourish style motion for interactions
                    .UseNavigation() // Able to use navigation panel and its functionality
                    .UseStatusBar() // Able to use status bar and its functionality
                    .UseTips() // Use flourish style tooltips instead of WPF one
                    .UseTitleBar(); // Use flourish style titlebar instead of WPF one
            })
            .ConfigureTitleBar(titlebar => titlebar.SetSearch("Type here to search", (_, _) => { })) // search handler TODO
            .ConfigureNavigation(nav => // configure navigation panel and its functionality, once UseNavigation is called and enabled (by default)
            {
                nav.SetGroup( // Create basic essential structure for navigation tree
                        null, // The group with ID 0 can create without name, using null instead of String.Empty
                        0, // Unique ID for group, should not repeat
                        group => group.AddNavigableViewItem<HomePage>(true) // Adding navigable items to this group, it should be registered by calling AddNavigable method
                        // [isInitial:true] should only be called once, it defines default showed page at application launch
                        // Using AddNavigableViewItem if this nav node is also a page at same time
                    )
                    .SetGroup( // Create second one
                        "Configuration", // The non ID 0 group must have its name
                        1, // Unique ID, sorting and affect navigation tree order
                        group =>
                        {
                            group.AddNavigableViewItem<ConfigurationPage>();
                            group.AddNavigableViewItem<ProjectRuntimePage>();
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
                            group.AddNavigableViewItem<ControlLibraryPage>(parentId: 1); // Using parentID to start define parent-child tree structure, can only have one tabbed layer
                            group.AddNavigableViewItem<ChunkPage>(childId: 1); // Using childID to define which parent navnode this page should be under located
                            group.AddNavigableViewItem<ButtonPage>(childId: 1);
                            group.AddNavigableViewItem<CardPage>(childId: 1);
                            group.AddNavigableViewItem<PresenterPage>(childId: 1);
                            group.AddNavigableViewItem<ParagraphPage>(childId: 1);
                            group.AddNavigableViewItem<DataGridPage>(childId: 1);
                            group.AddNavigableViewItem<OverlayPage>(childId: 1);
                        }
                    )
                    .SetGroup(
                        "Commands",
                        4,
                        group =>
                        {
                            group.AddNavigableItem("Message", "\uE8F2", "demo.hello"); // Using AddNavigableItem instead of AddNavigableViewItem if this nav node is NOT a page at same time
                            // When AddNavigableItem is not a parent nav node, its commandkey will be parsed.
                            // It means when it uses parentID, its commandkey will not be parsed anymore, should use null instead of set commandkey string at this case
                            group.AddNavigableItem("Task", "\uE895", "demo.background");
                        }
                    );
            })
            .ConfigureDynamicToolbar(toolbar =>
            {
                toolbar.CreateToolbarItems<HomePage>( //Create toolbar items only for HomePage view
                    new FlourishToolbarItem("Say hello", "\uE8F2", "demo.hello"),
                    new FlourishToolbarItem("Queue task", "\uE895", "demo.background")
                );
            })
            .Build();

        try
        {
            return flourish.Run<App>(); // Run application as WPF one
        }
        finally
        {
            flourish.Dispose();
            flourish = null;
        }
    }
}
