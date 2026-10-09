using ArkheideSystem.Flourish.Blazor;
using ArkheideSystem.Flourish.Blazor.Abstract;
using ArkheideSystem.Flourish.Extensions.Culture.Blazor;
using ArkheideSystem.Gallery.Flourish.Blazor.Commands;
using ArkheideSystem.Gallery.Flourish.Blazor.Components;
using ArkheideSystem.Gallery.Flourish.Blazor.Services;
using TextKey = ArkheideSystem.Gallery.Flourish.Blazor.Texts.Key;

var builder = WebApplication.CreateBuilder(args);

static TextReference Text(string key, string fallback) => new("Gallery", key, fallback);
builder.Services.AddFlourishFramework(
    builder.Configuration,
    framework =>
        framework
            .ConfigureCulture(culture =>
                culture
                    .AddCatalog<Program>("Gallery", "Gallery.Texts.json")
                    .SetDefaultCatalog("Gallery")
            )
            .ConfigureProject(project =>
                project
                    .SetProjectName(Text(TextKey.Project_Name, "Gallery"))
                    .SetLogo("gallery.svg")
                    .SetFavicon("gallery-favicon.svg")
            )
            .ConfigureTopBar(top =>
                top.InjectToRight<LanguagePicker>()
                    .AddMenu(
                        Text(TextKey.Menu_Pages, "页面"),
                        menu =>
                            menu.AddMenuItem(
                                    Text(TextKey.Nav_Framework, "框架"),
                                    GalleryCommandParser.OpenFramework
                                )
                                .AddMenuItem(
                                    Text(TextKey.Nav_Controls, "控件"),
                                    GalleryCommandParser.OpenControls
                                )
                                .AddMenuItem(
                                    Text(TextKey.Nav_Foundations, "基础"),
                                    GalleryCommandParser.OpenFoundations
                                )
                                .AddMenuItem(
                                    Text(TextKey.Nav_Examples, "案例"),
                                    GalleryCommandParser.OpenExamples
                                )
                    )
            )
            .ConfigureNavigation(navigation =>
                navigation
                    .AddNav(Text(TextKey.Nav_Home, "主页"), "home", "/", exact: true)
                    .AddNav(
                        Text(TextKey.Nav_ChangeLog, "更新记录"),
                        "history",
                        "/changelog",
                        exact: true
                    )
                    .AddNav(
                        Text(TextKey.Nav_Framework, "框架"),
                        "responsive_layout",
                        "/framework",
                        secondary =>
                            secondary
                                .AddSubNav(
                                    Text(TextKey.Nav_GetStarted, "接入应用"),
                                    "start",
                                    "/framework",
                                    exact: true
                                )
                                .AddSubNav(
                                    Text(TextKey.Nav_TopBar, "顶部栏"),
                                    "web_asset",
                                    "/framework/topbar"
                                )
                                .AddSubNav(
                                    Text(TextKey.Nav_Navigation, "导航"),
                                    "menu",
                                    "/framework/navigation"
                                )
                                .AddSubNav(
                                    Text(TextKey.Nav_Commands, "命令处理"),
                                    "terminal",
                                    "/framework/commands"
                                )
                                .AddSubNav(
                                    Text(TextKey.Nav_PageLayout, "页面布局"),
                                    "view_quilt",
                                    "/framework/layout"
                                )
                                .AddSubNav(
                                    Text(TextKey.Nav_Interactions, "内置交互"),
                                    "touch_app",
                                    "/framework/interactions"
                                )
                                .AddSubNav(
                                    Text(TextKey.Nav_Localization, "语言与本地化"),
                                    "translate",
                                    "/framework/localization"
                                )
                    )
                    .AddNav(
                        Text(TextKey.Nav_Controls, "控件"),
                        "crossword",
                        "/controls",
                        secondary =>
                            secondary
                                .AddSubNav(
                                    Text(TextKey.Nav_Actions, "按钮与菜单"),
                                    "smart_button",
                                    "/controls",
                                    third =>
                                        third
                                            .AddSubNav(
                                                "Button",
                                                "smart_button",
                                                "/controls/actions/buttonsample",
                                                exact: true
                                            )
                                            .AddSubNav(
                                                "UniformGridButton",
                                                "grid_view",
                                                "/controls/actions/uniformgridbuttonsample",
                                                exact: true
                                            )
                                            .AddSubNav(
                                                "SplitButton",
                                                "splitscreen",
                                                "/controls/actions/splitbuttonsample",
                                                exact: true
                                            )
                                            .AddSubNav(
                                                "ActionMenu",
                                                "smart_button",
                                                "/controls/actions/actionmenusample",
                                                exact: true
                                            )
                                            .AddSubNav(
                                                "ExpansionIndicator",
                                                "arrow_drop_down",
                                                "/controls/actions/expansionindicatorsample",
                                                exact: true
                                            ),
                                    exact: true
                                )
                                .AddSubNav(
                                    Text(TextKey.Nav_Inputs, "输入与选择"),
                                    "input",
                                    "/controls/inputs",
                                    third =>
                                        third
                                            .AddSubNav(
                                                "TextBox",
                                                "input",
                                                "/controls/inputs/textboxsample",
                                                exact: true
                                            )
                                            .AddSubNav(
                                                "NumberBox<TValue>",
                                                "input",
                                                "/controls/inputs/numberboxsample",
                                                exact: true
                                            )
                                            .AddSubNav(
                                                "SelectBox<TValue>",
                                                "input",
                                                "/controls/inputs/selectboxsample",
                                                exact: true
                                            )
                                            .AddSubNav(
                                                "MultiSelectBox",
                                                "input",
                                                "/controls/inputs/multiselectboxsample",
                                                exact: true
                                            )
                                            .AddSubNav(
                                                "LanguagePicker",
                                                "translate",
                                                "/controls/inputs/languagepickersample",
                                                exact: true
                                            )
                                            .AddSubNav(
                                                "CheckBox",
                                                "input",
                                                "/controls/inputs/checkboxsample",
                                                exact: true
                                            )
                                            .AddSubNav(
                                                "ToggleSwitch",
                                                "input",
                                                "/controls/inputs/toggleswitchsample",
                                                exact: true
                                            )
                                            .AddSubNav(
                                                "SearchBox",
                                                "input",
                                                "/controls/inputs/searchboxsample",
                                                exact: true
                                            )
                                            .AddSubNav(
                                                "Field",
                                                "input",
                                                "/controls/inputs/fieldsample",
                                                exact: true
                                            )
                                            .AddSubNav(
                                                "FormLayout",
                                                "input",
                                                "/controls/inputs/formlayoutsample",
                                                exact: true
                                            )
                                            .AddSubNav(
                                                "FormGroup",
                                                "input",
                                                "/controls/inputs/formgroupsample",
                                                exact: true
                                            )
                                            .AddSubNav(
                                                "FormActions",
                                                "input",
                                                "/controls/inputs/formactionssample",
                                                exact: true
                                            )
                                            .AddSubNav(
                                                "Primitives.MaskedInput",
                                                "input",
                                                "/controls/inputs/maskedinputsample",
                                                exact: true
                                            )
                                            .AddSubNav(
                                                "Primitives.StandaloneMaskedInput",
                                                "input",
                                                "/controls/inputs/standalonemaskedinputsample",
                                                exact: true
                                            )
                                            .AddSubNav(
                                                "Primitives.SearchAutocomplete<TItem>",
                                                "input",
                                                "/controls/inputs/searchautocompletesample",
                                                exact: true
                                            )
                                            .AddSubNav(
                                                "Primitives.ReferenceDropdown<TValue>",
                                                "input",
                                                "/controls/inputs/referencedropdownsample",
                                                exact: true
                                            ),
                                    exact: true
                                )
                                .AddSubNav(
                                    Text(TextKey.Nav_Data, "数据与列表"),
                                    "table_chart",
                                    "/controls/data",
                                    third =>
                                        third
                                            .AddSubNav(
                                                "LineChart",
                                                "show_chart",
                                                "/controls/data/linechartsample",
                                                exact: true
                                            )
                                            .AddSubNav(
                                                "DataTable<TItem>",
                                                "table_chart",
                                                "/controls/data/datatablesample",
                                                exact: true
                                            )
                                            .AddSubNav(
                                                "ListView<TItem>",
                                                "table_chart",
                                                "/controls/data/listviewsample",
                                                exact: true
                                            )
                                            .AddSubNav(
                                                "Primitives.DataPager",
                                                "table_chart",
                                                "/controls/data/datapagersample",
                                                exact: true
                                            )
                                            .AddSubNav(
                                                "Primitives.EditingGrid",
                                                "table_chart",
                                                "/controls/data/editinggridsample",
                                                exact: true
                                            ),
                                    exact: true
                                )
                                .AddSubNav(
                                    Text(TextKey.Nav_Overlays, "对话框与弹层"),
                                    "web_asset",
                                    "/controls/overlays",
                                    third =>
                                        third.AddSubNav(
                                            "Dialog",
                                            "web_asset",
                                            "/controls/overlays/dialogsample",
                                            exact: true
                                        ),
                                    exact: true
                                )
                                .AddSubNav(
                                    Text(TextKey.Nav_Feedback, "状态反馈"),
                                    "notifications",
                                    "/controls/feedback",
                                    third =>
                                        third
                                            .AddSubNav(
                                                "Notice",
                                                "notifications",
                                                "/controls/feedback/noticesample",
                                                exact: true
                                            )
                                            .AddSubNav(
                                                "EmptyState",
                                                "notifications",
                                                "/controls/feedback/emptystatesample",
                                                exact: true
                                            )
                                            .AddSubNav(
                                                "LoadingState",
                                                "notifications",
                                                "/controls/feedback/loadingstatesample",
                                                exact: true
                                            )
                                            .AddSubNav(
                                                "Primitives.NoticeTrigger",
                                                "notifications",
                                                "/controls/feedback/noticetriggersample",
                                                exact: true
                                            ),
                                    exact: true
                                )
                                .AddSubNav(
                                    Text(TextKey.Nav_Progress, "进度"),
                                    "hourglass_empty",
                                    "/controls/progress",
                                    third =>
                                        third
                                            .AddSubNav(
                                                "ProgressBar",
                                                "hourglass_empty",
                                                "/controls/progress/progressbarsample",
                                                exact: true
                                            )
                                            .AddSubNav(
                                                "ProgressRing",
                                                "hourglass_empty",
                                                "/controls/progress/progressringsample",
                                                exact: true
                                            ),
                                    exact: true
                                )
                                .AddSubNav(
                                    Text(TextKey.Nav_Content, "内容与组合"),
                                    "view_agenda",
                                    "/controls/content",
                                    third =>
                                        third
                                            .AddSubNav(
                                                "ToggleSection",
                                                "view_agenda",
                                                "/controls/content/togglesectionsample",
                                                exact: true
                                            )
                                            .AddSubNav(
                                                "UniformGrid",
                                                "view_agenda",
                                                "/controls/content/uniformgridsample",
                                                exact: true
                                            )
                                            .AddSubNav(
                                                "UniformGridItem",
                                                "grid_view",
                                                "/controls/content/uniformgriditemsample",
                                                exact: true
                                            )
                                            .AddSubNav(
                                                "Disclosure",
                                                "view_agenda",
                                                "/controls/content/disclosuresample",
                                                exact: true
                                            )
                                            .AddSubNav(
                                                "Section",
                                                "view_agenda",
                                                "/controls/content/sectionsample",
                                                exact: true
                                            )
                                            .AddSubNav(
                                                "Card",
                                                "view_agenda",
                                                "/controls/content/cardsample",
                                                exact: true
                                            )
                                            .AddSubNav(
                                                "InlineActions",
                                                "view_agenda",
                                                "/controls/content/inlineactionssample",
                                                exact: true
                                            )
                                            .AddSubNav(
                                                "ImagePreview",
                                                "image",
                                                "/controls/content/imagepreviewsample",
                                                exact: true
                                            )
                                            .AddSubNav(
                                                "Icon",
                                                "view_agenda",
                                                "/controls/content/iconsample",
                                                exact: true
                                            )
                                            .AddSubNav(
                                                "DisplayBoard",
                                                "view_in_ar",
                                                "/controls/content/displayboardsample",
                                                exact: true
                                            )
                                            .AddSubNav(
                                                "CodeBlock",
                                                "view_agenda",
                                                "/controls/content/codeblocksample",
                                                exact: true
                                            )
                                            .AddSubNav(
                                                "CopyText",
                                                "content_copy",
                                                "/controls/content/copytextsample",
                                                exact: true
                                            ),
                                    exact: true
                                )
                                .AddSubNav(
                                    Text(TextKey.Nav_Layout, "外壳与布局"),
                                    "view_quilt",
                                    "/controls/layout",
                                    third =>
                                        third
                                            .AddSubNav(
                                                "ApplicationLayout",
                                                "view_quilt",
                                                "/controls/layout/applicationlayoutsample",
                                                exact: true
                                            )
                                            .AddSubNav(
                                                "ApplicationShell",
                                                "view_quilt",
                                                "/controls/layout/applicationshellsample",
                                                exact: true
                                            )
                                            .AddSubNav(
                                                "PageBody",
                                                "view_quilt",
                                                "/controls/layout/pagebodysample",
                                                exact: true
                                            )
                                            .AddSubNav(
                                                "AttributionFooter",
                                                "copyright",
                                                "/controls/layout/attributionfootersample",
                                                exact: true
                                            )
                                            .AddSubNav(
                                                "SectionNavigator",
                                                "more_vert",
                                                "/controls/layout/sectionnavigatorsample",
                                                exact: true
                                            )
                                            .AddSubNav(
                                                "PageHeading",
                                                "view_quilt",
                                                "/controls/layout/pageheadingsample",
                                                exact: true
                                            )
                                            .AddSubNav(
                                                "Patterns.ContentSurface",
                                                "view_quilt",
                                                "/controls/layout/contentsurfacesample",
                                                exact: true
                                            )
                                            .AddSubNav(
                                                "Patterns.NavigationSurface",
                                                "view_quilt",
                                                "/controls/layout/navigationsurfacesample",
                                                exact: true
                                            )
                                            .AddSubNav(
                                                "Primitives.NavigationGuard",
                                                "view_quilt",
                                                "/controls/layout/navigationguardsample",
                                                exact: true
                                            )
                                            .AddSubNav(
                                                "Primitives.InteractionBoundary",
                                                "view_quilt",
                                                "/controls/layout/interactionboundarysample",
                                                exact: true
                                            )
                                            .AddSubNav(
                                                "Primitives.AccessBrand",
                                                "view_quilt",
                                                "/controls/layout/accessbrandsample",
                                                exact: true
                                            )
                                            .AddSubNav(
                                                "Primitives.ShellHeader",
                                                "view_quilt",
                                                "/controls/layout/shellheadersample",
                                                exact: true
                                            )
                                            .AddSubNav(
                                                "Primitives.ServiceMenu",
                                                "view_quilt",
                                                "/controls/layout/servicemenusample",
                                                exact: true
                                            )
                                            .AddSubNav(
                                                "Primitives.PrimaryNavigationItem",
                                                "view_quilt",
                                                "/controls/layout/primarynavigationitemsample",
                                                exact: true
                                            ),
                                    exact: true
                                )
                    )
                    .AddNav(
                        Text(TextKey.Nav_Foundations, "基础"),
                        "shapes",
                        "/foundations",
                        secondary =>
                            secondary
                                .AddSubNav(
                                    Text(TextKey.Nav_Colors, "颜色与主题"),
                                    "palette",
                                    "/foundations",
                                    exact: true
                                )
                                .AddSubNav(
                                    Text(TextKey.Nav_Typography, "字体"),
                                    "text_fields",
                                    "/foundations/typography"
                                )
                                .AddSubNav(
                                    Text(TextKey.Nav_Spacing, "间距"),
                                    "space_bar",
                                    "/foundations/spacing"
                                )
                                .AddSubNav(
                                    Text(TextKey.Nav_Shape, "圆角与阴影"),
                                    "rounded_corner",
                                    "/foundations/shape"
                                )
                                .AddSubNav(
                                    Text(TextKey.Nav_Dimensions, "布局尺寸"),
                                    "aspect_ratio",
                                    "/foundations/dimensions"
                                )
                                .AddSubNav(
                                    Text(TextKey.Nav_Icons, "图标"),
                                    "emoji_symbols",
                                    "/foundations/icons"
                                )
                    )
                    .AddNav(
                        Text(TextKey.Nav_Examples, "案例"),
                        "explore",
                        "/examples",
                        secondary =>
                            secondary
                                .AddSubNav(
                                    Text(TextKey.Nav_ExampleIndex, "案例目录"),
                                    "view_list",
                                    "/examples",
                                    exact: true
                                )
                                .AddSubNav(
                                    Text(TextKey.Nav_FormPage, "创建表单页面"),
                                    "description",
                                    "/examples/form"
                                )
                                .AddSubNav(Text(TextKey.Nav_Forms, "表单演示"), "edit", "/forms")
                                .AddSubNav(
                                    Text(TextKey.Nav_Records, "记录列表"),
                                    "list",
                                    "/records"
                                )
                                .AddSubNav(
                                    Text(TextKey.Nav_RecordDetails, "记录详情"),
                                    "person",
                                    "/records/sample"
                                )
                                .AddSubNav(
                                    Text(TextKey.Nav_Patterns, "组合布局"),
                                    "view_quilt",
                                    "/patterns"
                                )
                                .AddSubNav(
                                    Text(TextKey.Nav_Tutorial, "教程"),
                                    "school",
                                    "/examples/tutorial",
                                    exact: true
                                )
                                .AddSubNav(
                                    Text(TextKey.Nav_DisplayPages, "展示页面"),
                                    "web",
                                    "/examples/display",
                                    display =>
                                        display
                                            .AddSubNav(
                                                Text(TextKey.Nav_DisplayProduct, "产品横幅"),
                                                "web_asset",
                                                "/examples/display/product",
                                                exact: true
                                            )
                                            .AddSubNav(
                                                Text(TextKey.Nav_DisplayPricing, "报价与比较"),
                                                "table_chart",
                                                "/examples/display/pricing",
                                                exact: true
                                            )
                                            .AddSubNav(
                                                Text(TextKey.Nav_DisplayAccess, "访问案例总览"),
                                                "login",
                                                "/examples/display/access",
                                                exact: true
                                            )
                                            .AddSubNav(
                                                Text(TextKey.Nav_DisplayLogin, "完整登录页"),
                                                "login",
                                                "/examples/display/login",
                                                exact: true
                                            )
                                            .AddSubNav(
                                                Text(TextKey.Nav_DisplayAccounts, "已保存账号选择"),
                                                "person",
                                                "/examples/display/accounts",
                                                exact: true
                                            )
                                            .AddSubNav(
                                                Text(TextKey.Nav_DisplayMethods, "访问方式选择"),
                                                "login",
                                                "/examples/display/access-methods",
                                                exact: true
                                            )
                                            .AddSubNav(
                                                Text(TextKey.Nav_DisplayWizard, "向导选项"),
                                                "grid_view",
                                                "/examples/display/wizard",
                                                exact: true
                                            ),
                                    exact: true
                                )
                                .AddSubNav(
                                    Text(TextKey.Nav_Enums, "枚举下拉框"),
                                    "arrow_drop_down",
                                    "/examples/enum"
                                )
                                .AddSubNav(
                                    Text(TextKey.Nav_Search, "搜索与列表"),
                                    "search",
                                    "/examples/search"
                                )
                    )
                    .AddFixedNavButton(
                        Text(TextKey.Nav_Theme, "切换主题"),
                        "routine",
                        GalleryCommandParser.ToggleTheme
                    )
            )
            .SetCommandParser<GalleryCommandParser>()
);
builder.Services.AddFlourishDesign(builder.Configuration);
builder.Services.AddScoped<RecordStore>();

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error", createScopeForErrors: true);
    app.UseHsts();
    app.UseHttpsRedirection();
}
app.UseStatusCodePagesWithReExecute("/not-found");
app.UseAntiforgery();
app.MapStaticAssets().ShortCircuit();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();
