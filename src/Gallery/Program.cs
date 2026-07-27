using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Abstract.Builder;
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
            .ConfigServices( // Configure services as hosting
                (_, services) =>
                {
                    services.AddSingleton<App>();
                    services.AddCommandParser<GalleryCommandParser>(); // Mapping command key and its executor

                    services.AddNavigable<HomePage>("Overview", "\uE80F"); // Using AddNavigable instead of AddSingleton or other
                    services.AddNavigable<AboutPage>("About", "\uE946");
                    services.AddNavigable<ConfigurationPage>("Configuration", "\uE713");
                    services.AddNavigable<AppearancePage>("Appearance", "\uE790");
                    services.AddNavigable<TitleBarRuntimePage>("Title bar", "\uE8A4");
                    services.AddNavigable<ProjectRuntimePage>("Projects", "\uE8F9");
                    services.AddNavigable<NavigationRuntimePage>("Navigation", "\uE700");
                    services.AddNavigable<ProfileConfigurationPage>("Profile", "\uE77B");
                    services.AddNavigable<StatusBarConfigurationPage>("Status bar", "\uE930");
                    services.AddNavigable<DynamicToolbarConfigurationPage>("Dynamic toolbar", "\uE945");
                    services.AddNavigable<ToolTipsConfigurationPage>("ToolTips", "\uE823");
                    services.AddNavigable<MotionConfigurationPage>("Motion", "\uE768");
                    services.AddNavigable<CustomHandlerConfigurationPage>("Custom handler", "\uE8BA");
                    services.AddNavigable<CommandsPage>("Commands", "\uE756");
                    services.AddNavigable<WindowRuntimePage>("Window", "\uE737");
                    services.AddNavigable<BackgroundTasksPage>("Background", "\uF5EF");
                    services.AddNavigable<ControlLibraryPage>("Controls", "\uE950");
                    services.AddNavigable<HeaderChunkPage>("HeaderChunk", "\uE840");
                    services.AddNavigable<ChunkPage>("Chunk", "\uE81E");
                    services.AddNavigable<ButtonPage>("Button", "\uE815");
                    services.AddNavigable<CardButtonPage>("CardButton", "\uF271");
                    services.AddNavigable<WindowCaptionButtonPage>("WindowCaptionButton", "\uE8BB");
                    services.AddNavigable<CardPage>("Card", "\uE7FB");
                    services.AddNavigable<ActionCardPage>("ActionCard", "\uE7C9");
                    services.AddNavigable<OutputCardPage>("OutputCard", "\uE78B");
                    services.AddNavigable<PresenterPage>("Presenter", "\uE8BA");
                    services.AddNavigable<PageBodyPage>("PageBody", "\uE8A7");
                    services.AddNavigable<DocumentPage>("Document", "\uE8A5");
                    services.AddNavigable<ParagraphPage>("Paragraph", "\uE8D2");
                    services.AddNavigable<CodeSpacePage>("CodeSpace", "\uE943");
                    services.AddNavigable<DataGridPage>("DataGrid", "\uE80A");
                    services.AddNavigable<OverlayPage>("Overlay", "\uE89B");
                    services.AddNavigable<TextBlockPage>("TextBlock", "\uE8D2");
                    services.AddNavigable<ListBoxPage>("ListBox", "\uE8FD");
                    services.AddNavigable<ListBoxItemPage>("ListBoxItem", "\uE8FD");
                    services.AddNavigable<ScrollViewerPage>("ScrollViewer", "\uE896");
                    services.AddNavigable<ScrollBarPage>("ScrollBar", "\uE70E");
                    services.AddNavigable<GridSplitterPage>("GridSplitter", "\uE8A9");
                    services.AddNavigable<ToolTipPage>("ToolTip", "\uE8BD");
                    services.AddNavigable<TextBoxPage>("TextBox", "\uE8D2");
                    services.AddNavigable<PasswordBoxPage>("PasswordBox", "\uE72E");
                    services.AddNavigable<SearchBoxPage>("SearchBox", "\uE721");
                    services.AddNavigable<CheckBoxPage>("CheckBox", "\uE73E");
                    services.AddNavigable<RadioButtonPage>("RadioButton", "\uECCA");
                    services.AddNavigable<ComboBoxPage>("ComboBox", "\uE70D");
                    services.AddNavigable<ComboBoxItemPage>("ComboBoxItem", "\uE8FD");
                    services.AddNavigable<LabelPage>("Label", "\uE8EC");
                }
            )
            .ConfigShell(shell => // Enable functionality at top level
            {
                shell
                    .InitGlobalFont() // Set uniform global font family and sizes
                    .UseCenterContent() // Use centered width restricted content
                    .UseDynamicToolbar() // Able to create toolbar items dynamically
                    .UseMaterialEffect() // Use windows DWM material effect for background
                    .UseMotion() // Use flourish style motion for interactions
                    .UseNavigation() // Able to use navigation panel and its functionality
                    .UseSmoothScroll() // Use smooth scrolling with global scrollviewer
                    .UseStatusBar() // Able to use status bar and its functionality
                    .UseTips() // Use flourish style tooltips instead of WPF one
                    .UseTitleBar(); // Use flourish style titlebar instead of WPF one
            })
            .ConfigTitleBar(t => t.InitApplicationSubTitle("ABC"))
            .ConfigTitleBar(titlebar =>
                titlebar.UseSearch(
                    placeholder: "Type here to search",
                    handler: (_, _) => { }
                )
            ) // search handler TODO
            .ConfigNavigation(nav => // configure navigation panel and its functionality, once UseNavigation is called and enabled (by default)
            {
                nav.InitGroup( // Create basic essential structure for navigation tree
                        null, // The group with ID 0 can create without name, using null instead of String.Empty
                        0, // Unique ID for group, should not repeat
                        group =>
                        {
                            group.InitNavigableViewItem<HomePage>(true); // [isInitial:true] defines the page shown at launch
                        }
                    )
                    .InitGroup( // Create second one
                        "Configuration", // The non ID 0 group must have its name
                        1, // Unique ID, sorting and affect navigation tree order
                        group =>
                        {
                            group.InitNavigableViewItem<ConfigurationPage>();
                            group.InitNavigableViewItem<ProjectRuntimePage>();
                            group.InitNavigableViewItem<CommandsPage>();
                            group.InitNavigableViewItem<BackgroundTasksPage>();
                        }
                    )
                    .InitGroup(
                        "Shell",
                        2,
                        group =>
                        {
                            group.InitNavigableViewItem<AppearancePage>();
                            group.InitNavigableViewItem<TitleBarRuntimePage>();
                            group.InitNavigableViewItem<NavigationRuntimePage>();
                            group.InitNavigableViewItem<ProfileConfigurationPage>();
                            group.InitNavigableViewItem<WindowRuntimePage>();
                            group.InitNavigableViewItem<StatusBarConfigurationPage>();
                            group.InitNavigableViewItem<DynamicToolbarConfigurationPage>();
                            group.InitNavigableViewItem<ToolTipsConfigurationPage>();
                            group.InitNavigableViewItem<MotionConfigurationPage>();
                            group.InitNavigableViewItem<CustomHandlerConfigurationPage>();
                        }
                    )
                    .InitGroup(
                        "Controls",
                        3,
                        group =>
                        {
                            group.InitNavigableViewItem<ControlLibraryPage>(parentId: 1); // Using parentID to start define parent-child tree structure, can only have one tabbed layer
                            group.InitNavigableViewItem<ChunkPage>(childId: 1); // Using childID to define which parent navnode this page should be under located
                            group.InitNavigableViewItem<HeaderChunkPage>(childId: 1);
                            group.InitNavigableViewItem<ButtonPage>(childId: 1);
                            group.InitNavigableViewItem<CardButtonPage>(childId: 1);
                            group.InitNavigableViewItem<WindowCaptionButtonPage>(childId: 1);
                            group.InitNavigableViewItem<CardPage>(childId: 1);
                            group.InitNavigableViewItem<ActionCardPage>(childId: 1);
                            group.InitNavigableViewItem<OutputCardPage>(childId: 1);
                            group.InitNavigableViewItem<PresenterPage>(childId: 1);
                            group.InitNavigableViewItem<PageBodyPage>(childId: 1);
                            group.InitNavigableViewItem<DocumentPage>(childId: 1);
                            group.InitNavigableViewItem<ParagraphPage>(childId: 1);
                            group.InitNavigableViewItem<CodeSpacePage>(childId: 1);
                            group.InitNavigableViewItem<DataGridPage>(childId: 1);
                            group.InitNavigableViewItem<OverlayPage>(childId: 1);
                            group.InitNavigableViewItem<TextBlockPage>(childId: 1);
                            group.InitNavigableViewItem<ListBoxPage>(childId: 1);
                            group.InitNavigableViewItem<ListBoxItemPage>(childId: 1);
                            group.InitNavigableViewItem<ScrollViewerPage>(childId: 1);
                            group.InitNavigableViewItem<ScrollBarPage>(childId: 1);
                            group.InitNavigableViewItem<GridSplitterPage>(childId: 1);
                            group.InitNavigableViewItem<ToolTipPage>(childId: 1);
                            group.InitNavigableViewItem<TextBoxPage>(childId: 1);
                            group.InitNavigableViewItem<PasswordBoxPage>(childId: 1);
                            group.InitNavigableViewItem<SearchBoxPage>(childId: 1);
                            group.InitNavigableViewItem<CheckBoxPage>(childId: 1);
                            group.InitNavigableViewItem<RadioButtonPage>(childId: 1);
                            group.InitNavigableViewItem<ComboBoxPage>(childId: 1);
                            group.InitNavigableViewItem<ComboBoxItemPage>(childId: 1);
                            group.InitNavigableViewItem<LabelPage>(childId: 1);
                        }
                    )
                    .InitGroup(
                        "Actions",
                        4,
                        group =>
                        {
                            group.InitNavigableItem("Message", "\uE8F2", "demo.hello"); // Using InitNavigableItem instead of InitNavigableViewItem if this nav node is NOT a page at same time
                            // When InitNavigableItem is not a parent nav node, its commandkey will be parsed.
                            // It means when it uses parentID, its commandkey will not be parsed anymore, should use null instead of set commandkey string at this case
                            group.InitNavigableItem("Task", "\uE895", "demo.background");
                        }
                    )
                    .InitFixedNavigableViewItem<AboutPage>();
            })
            .ConfigDynamicToolbar(toolbar =>
            {
                toolbar.InitToolbarItems<HomePage>( //Create toolbar items only for HomePage view
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
