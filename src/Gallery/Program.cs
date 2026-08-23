using ArkheideSystem.Flourish.Extension.Culture;
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
            .UseEssentialCulture()
            .ConfigData( // Configure data as hosting
                (data) =>
                {
                    data.InitLocale("en-US"); // Set default locale for application
                }
            )
            .ConfigServices( // Configure services as hosting
                (_, services) =>
                {
                    services.AddSingleton<App>();
                    services.AddCommandParser<GalleryCommandParser>(); // Mapping command key and its executor

                    services.AddNavigable<HomePage>(
                        Key.Application_Overview_D4B1EA57,
                        "\uE80F"
                    ); // Using AddNavigable instead of AddSingleton or other
                    services.AddNavigable<AboutPage>(
                        Key.Application_About_4EFCA0D1,
                        "\uE946"
                    );
                    services.AddNavigable<ConfigurationPage>(
                        Key.Application_Configuration_B332C349,
                        "\uE713"
                    );
                    services.AddNavigable<AppearancePage>(
                        Key.Application_Appearance_3907FA7F,
                        "\uE790"
                    );
                    services.AddNavigable<TitleBarRuntimePage>(
                        Key.Application_TitleBar_11BCA4AC,
                        "\uE8A4"
                    );
                    services.AddNavigable<ProjectRuntimePage>(
                        Key.Application_Projects_04E2A972,
                        "\uE8F9"
                    );
                    services.AddNavigable<NavigationRuntimePage>(
                        Key.Application_Navigation_3DB65F8C,
                        "\uE700"
                    );
                    services.AddNavigable<ProfileConfigurationPage>(
                        Key.Application_Profile_D696A35B,
                        "\uE77B"
                    );
                    services.AddNavigable<StatusBarConfigurationPage>(
                        Key.Application_StatusBar_0BC4C2AF,
                        "\uE930"
                    );
                    services.AddNavigable<DynamicToolbarConfigurationPage>(
                        Key.Application_DynamicToolbar_2B28D7DF,
                        "\uE945"
                    );
                    services.AddNavigable<ToolTipsConfigurationPage>(
                        Key.Application_ToolTips_53998699,
                        "\uE823"
                    );
                    services.AddNavigable<MotionConfigurationPage>(
                        Key.Application_Motion_8CA34424,
                        "\uE768"
                    );
                    services.AddNavigable<CustomHandlerConfigurationPage>(
                        Key.Application_CustomHandler_41F3A17B,
                        "\uE8BA"
                    );
                    services.AddNavigable<CommandsPage>(
                        Key.Application_Commands_B269DC4E,
                        "\uE756"
                    );
                    services.AddNavigable<WindowRuntimePage>(
                        Key.Application_Window_19734A1B,
                        "\uE737"
                    );
                    services.AddNavigable<BackgroundTasksPage>(
                        Key.Application_Background_EA2B8A87,
                        "\uF5EF"
                    );
                    services.AddNavigable<ControlLibraryPage>(
                        Key.Application_Controls_799C2691,
                        "\uE950"
                    );
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
                    services.AddNavigable<CodeSpacePage>("CodeSpace", "\uE943");
                    services.AddNavigable<DataGridPage>("DataGrid", "\uE80A");
                    services.AddNavigable<OverlayPage>("Overlay", "\uE89B");
                    services.AddNavigable<TextBlockPage>("TextBlock", "\uE8D2");
                    services.AddNavigable<BunchedListBoxPage>("BunchedListBox", "\uE8FD");
                    services.AddNavigable<ListBoxPage>("ListBox", "\uE8FD");
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
                    services.AddNavigable<LabelPage>("Label", "\uE8EC");
                }
            )
            .ConfigShell(shell => // Enable functionality at top level
            {
                shell
                    .InitGlobalFont() // Init uniform global font family and sizes
                    .UseCenterContent() // Use centered width restricted content
                    .UseDynamicToolbar() // Able to create toolbar items dynamically
                    .UseMaterialEffect() // Use the current Windows default material backdrop
                    .UseMotion() // Use flourish style motion for interactions
                    .UseNavigation() // Able to use navigation panel and its functionality
                    .UseSmoothScroll() // Use smooth scrolling with global scrollviewer
                    .UseStatusBar() // Able to use status bar and its functionality
                    .UseTips() // Use flourish style tooltips instead of WPF one
                    .UseTitleBar(); // Use flourish style titlebar instead of WPF one
            })
            .ConfigTitleBar(t =>
                t.InitApplicationSubTitle(Key.Application_ComponentReference_661E6097)
            )
            .ConfigTitleBar(titlebar =>
                titlebar.UseSearch(
                    placeholder: Key.Application_TypeHereToSearch_85717255,
                    handler: (_, _) => { }
                )
            ) // search handler TODO
            .ConfigNavigation(nav => // configure navigation panel and its functionality, once UseNavigation is called and enabled (by default)
            {
                nav.AddGroup( // Create basic essential structure for navigation tree
                        null, // The group with ID 0 can create without name, using null instead of String.Empty
                        0, // Unique ID for group, should not repeat
                        group =>
                        {
                            group.AddNavigableViewItem<HomePage>(true); // [isInitial:true] defines the page shown at launch
                        }
                    )
                    .AddGroup( // Create second one
                        // The non ID 0 group must have its name
                        Key.Application_Configuration_B332C349,
                        1, // Unique ID, sorting and affect navigation tree order
                        group =>
                        {
                            group.AddNavigableViewItem<ConfigurationPage>();
                            group.AddNavigableViewItem<ProjectRuntimePage>();
                            group.AddNavigableViewItem<CommandsPage>();
                            group.AddNavigableViewItem<BackgroundTasksPage>();
                        }
                    )
                    .AddGroup(
                        Key.Shell_Shell_A7332854,
                        2,
                        group =>
                        {
                            group.AddNavigableViewItem<AppearancePage>();
                            group.AddNavigableViewItem<TitleBarRuntimePage>();
                            group.AddNavigableViewItem<NavigationRuntimePage>();
                            group.AddNavigableViewItem<ProfileConfigurationPage>();
                            group.AddNavigableViewItem<WindowRuntimePage>();
                            group.AddNavigableViewItem<StatusBarConfigurationPage>();
                            group.AddNavigableViewItem<DynamicToolbarConfigurationPage>();
                            group.AddNavigableViewItem<ToolTipsConfigurationPage>();
                            group.AddNavigableViewItem<MotionConfigurationPage>();
                            group.AddNavigableViewItem<CustomHandlerConfigurationPage>();
                        }
                    )
                    .AddGroup(
                        Key.Application_Controls_799C2691,
                        3,
                        group =>
                        {
                            group.AddNavigableViewItem<ControlLibraryPage>(parentId: 1); // Using parentID to start define parent-child tree structure, can only have one tabbed layer
                            group.AddNavigableViewItem<ChunkPage>(childId: 1); // Using childID to define which parent navnode this page should be under located
                            group.AddNavigableViewItem<HeaderChunkPage>(childId: 1);
                            group.AddNavigableViewItem<ButtonPage>(childId: 1);
                            group.AddNavigableViewItem<CardButtonPage>(childId: 1);
                            group.AddNavigableViewItem<WindowCaptionButtonPage>(childId: 1);
                            group.AddNavigableViewItem<CardPage>(childId: 1);
                            group.AddNavigableViewItem<ActionCardPage>(childId: 1);
                            group.AddNavigableViewItem<OutputCardPage>(childId: 1);
                            group.AddNavigableViewItem<PresenterPage>(childId: 1);
                            group.AddNavigableViewItem<PageBodyPage>(childId: 1);
                            group.AddNavigableViewItem<DocumentPage>(childId: 1);
                            group.AddNavigableViewItem<CodeSpacePage>(childId: 1);
                            group.AddNavigableViewItem<DataGridPage>(childId: 1);
                            group.AddNavigableViewItem<OverlayPage>(childId: 1);
                            group.AddNavigableViewItem<TextBlockPage>(childId: 1);
                            group.AddNavigableViewItem<BunchedListBoxPage>(childId: 1);
                            group.AddNavigableViewItem<ListBoxPage>(childId: 1);
                            group.AddNavigableViewItem<ScrollViewerPage>(childId: 1);
                            group.AddNavigableViewItem<ScrollBarPage>(childId: 1);
                            group.AddNavigableViewItem<GridSplitterPage>(childId: 1);
                            group.AddNavigableViewItem<ToolTipPage>(childId: 1);
                            group.AddNavigableViewItem<TextBoxPage>(childId: 1);
                            group.AddNavigableViewItem<PasswordBoxPage>(childId: 1);
                            group.AddNavigableViewItem<SearchBoxPage>(childId: 1);
                            group.AddNavigableViewItem<CheckBoxPage>(childId: 1);
                            group.AddNavigableViewItem<RadioButtonPage>(childId: 1);
                            group.AddNavigableViewItem<ComboBoxPage>(childId: 1);
                            group.AddNavigableViewItem<LabelPage>(childId: 1);
                        }
                    )
                    .AddGroup(
                        Key.Application_Actions_FF8059DC,
                        4,
                        group =>
                        {
                            group.AddNavigableItem(
                                Key.Application_Message_2F77668A,
                                "\uE8F2",
                                GalleryCommandKeys.DemoHello
                            ); // Using AddNavigableItem instead of AddNavigableViewItem if this nav node is NOT a page at same time
                            // When AddNavigableItem is not a parent nav node, its commandkey will be parsed.
                            // It means when it uses parentID, its commandkey will not be parsed anymore, should use null instead of set commandkey string at this case
                            group.AddNavigableItem(
                                Key.Application_Task_4BC74B21,
                                "\uE895",
                                GalleryCommandKeys.DemoBackground
                            );
                        }
                    )
                    .AddFixedNavigableViewItem<AboutPage>();
            })
            .ConfigDynamicToolbar(toolbar =>
            {
                toolbar.InitToolbarItems<HomePage>( //Create toolbar items only for HomePage view
                    new FlourishToolbarItem(
                        Key.Application_SayHello_6D995DBA,
                        "\uE8F2",
                        GalleryCommandKeys.DemoHello
                    ),
                    new FlourishToolbarItem(
                        Key.Application_QueueTask_229EFD6E,
                        "\uE895",
                        GalleryCommandKeys.DemoBackground
                    )
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
