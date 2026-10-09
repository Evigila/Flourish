using System.Collections;
using System.Globalization;
using System.Reflection;
using Microsoft.AspNetCore.Components;
using Controls = ArkheideSystem.Flourish.Blazor.Components;
using Patterns = ArkheideSystem.Flourish.Blazor.Components.Patterns;
using Primitives = ArkheideSystem.Flourish.Blazor.Components.Primitives;

namespace ArkheideSystem.Gallery.Flourish.Blazor.Models;

public static class ComponentCatalog
{
    public static IReadOnlyList<CatalogGroup> Groups { get; } =
    [
        new("Key.Catalog_Group_Application_Title", "Key.Catalog_Group_Application_Description",
        [
            new(typeof(Controls.ApplicationLayout), "Key.Catalog_ApplicationLayout_Purpose", "Key.Catalog_ApplicationLayout_Variants", "<RouteView RouteData=\"routeData\" DefaultLayout=\"typeof(ApplicationLayout)\" />"),
            new(typeof(Controls.ApplicationShell), "Key.Catalog_ApplicationShell_Purpose", "Key.Catalog_ApplicationShell_Variants", "<ApplicationShell><PageBody><PageHeading Title=\"主页\" /></PageBody></ApplicationShell>")
        ]),
        new("Key.Catalog_Group_General_Title", "Key.Catalog_Group_General_Description",
        [
            new(typeof(Controls.ValidationMessages), "Key.Catalog_ValidationMessages_Purpose", "Key.Catalog_ValidationMessages_Variants", "<ValidationMessages Id=\"token-error\" For=\"@(() => Draft.Token)\" />"),
            new(typeof(Controls.NavigationChoices), "Key.Catalog_NavigationChoices_Purpose", "Key.Catalog_NavigationChoices_Variants", "<NavigationChoices Id=\"methods\" Label=\"访问方式\" Items=\"Methods\" ActiveKey=\"@ActiveMethod\" Compact=\"true\" Variant=\"ButtonVariant.Elevated\" ActiveVariant=\"ButtonVariant.Elevated\"><ChildContent Context=\"method\"><p>@method.Label</p></ChildContent></NavigationChoices>"),
            new(typeof(Controls.Button), "Key.Catalog_Button_Purpose", "Key.Catalog_Button_Variants", "<Button Text=\"示例账号\" Description=\"example@example.test\" TrailingText=\"已连接\" OnClick=\"SelectAsync\" Busy=\"Selecting\" />"),
            new(typeof(Controls.ExpansionIndicator), "Key.Catalog_ExpansionIndicator_Purpose", "Key.Catalog_ExpansionIndicator_Variants", "<Button Variant=\"ButtonVariant.Quiet\" OnClick=\"() => Expanded = !Expanded\" aria-expanded=\"@(Expanded ? \"true\" : \"false\")\" aria-controls=\"details\">详情 <ExpansionIndicator Expanded=\"Expanded\" /></Button>\n<div id=\"details\" hidden=\"@(!Expanded)\">详情内容</div>"),
            new(typeof(Controls.SplitButton), "Key.Catalog_SplitButton_Purpose", "Key.Catalog_SplitButton_Variants", "<SplitButton Text=\"继续\" Type=\"submit\" FullWidth=\"true\"><MenuContent><Button Variant=\"ButtonVariant.Quiet\" Href=\"/examples/display\">返回</Button></MenuContent></SplitButton>"),
            new(typeof(Controls.DisplayBoard), "Key.Catalog_DisplayBoard_Purpose", "Key.Catalog_DisplayBoard_Variants", "<DisplayBoard><Button Icon=\"add\" Text=\"创建\" /></DisplayBoard>\n<DisplayBoard Dotted=\"false\" CopyText=\"@Source\" CopyLabel=\"复制代码\"><CodeBlock Text=\"@Source\" Language=\"razor\" /></DisplayBoard>"),
            new(typeof(Controls.UniformGridButton), "Key.Catalog_UniformGridButton_Purpose", "Key.Catalog_UniformGridButton_Variants", "<UniformGrid Columns=\"2\" NarrowColumns=\"1\"><UniformGridButton Title=\"个人使用\" OnClick=\"ChoosePersonal\" /><UniformGridButton Title=\"工作空间\" OnClick=\"ChooseWorkspace\" /></UniformGrid>"),
            new(typeof(Controls.ActionMenu), "Key.Catalog_ActionMenu_Purpose", "Key.Catalog_ActionMenu_Variants", "<ActionMenu Label=\"更多操作\" Actions=\"Actions\" />\n<ActionMenu Label=\"记录操作\"><Button role=\"menuitem\" OnClick=\"Edit\">编辑</Button></ActionMenu>"),
            new(typeof(Controls.TextBox), "Key.Catalog_TextBox_Purpose", "Key.Catalog_TextBox_Variants", "<Field Label=\"名称\" Id=\"name\"><TextBox Id=\"name\" @bind-Value=\"Draft.Name\" MaxLength=\"120\" /></Field>"),
            new(typeof(Controls.NumberBox<>), "Key.Catalog_NumberBox_Purpose", "Key.Catalog_NumberBox_Variants", "<Field Label=\"金额\" Id=\"amount\"><NumberBox TValue=\"decimal\" Id=\"amount\" @bind-Value=\"Draft.Amount\" Min=\"0\" Step=\"0.01\" /></Field>"),
            new(typeof(Controls.SelectBox<>), "Key.Catalog_SelectBox_Purpose", "Key.Catalog_SelectBox_Variants", "<Field Label=\"状态\" Id=\"state\"><SelectBox TValue=\"RecordState\" Id=\"state\" Options=\"States\" @bind-Value=\"Draft.State\" /></Field>"),
            new(typeof(Controls.MultiSelectBox), "Key.Catalog_MultiSelectBox_Purpose", "Key.Catalog_MultiSelectBox_Variants", "<MultiSelectBox Label=\"活动标签\" Items=\"Tags\" Changed=\"AcceptSelection\" Searchable=\"true\" MaximumSelections=\"3\" />"),
            new(typeof(Controls.CheckBox), "Key.Catalog_CheckBox_Purpose", "Key.Catalog_CheckBox_Variants", "<CheckBox Label=\"启用\" @bind-Value=\"Draft.Active\" />"),
            new(typeof(Controls.ToggleSwitch), "Key.Catalog_ToggleSwitch_Purpose", "Key.Catalog_ToggleSwitch_Variants", "<ToggleSwitch Label=\"显示详情\" @bind-Value=\"ShowDetails\" Controls=\"details\" />"),
            new(typeof(Controls.ToggleSection), "Key.Catalog_ToggleSection_Purpose", "Key.Catalog_ToggleSection_Variants", "<ToggleSection Label=\"高级选项\" @bind-Value=\"Expanded\"><p>选项内容</p></ToggleSection>"),
            new(typeof(Controls.SearchBox), "Key.Catalog_SearchBox_Purpose", "Key.Catalog_SearchBox_Variants", "<SearchBox Label=\"搜索项目\" @bind-Value=\"Query\" SearchChanged=\"FilterItems\" />"),
            new(typeof(Controls.Field), "Key.Catalog_Field_Purpose", "Key.Catalog_Field_Variants", "<Field Label=\"名称\" Id=\"name\" Required=\"true\" For=\"@(() => Draft.Name)\"><ChildContent><TextBox @bind-Value=\"Draft.Name\" /></ChildContent><Actions><InlineActions Alignment=\"HorizontalAlignment.End\"><Button Type=\"submit\" Text=\"保存\" /></InlineActions></Actions></Field>"),
            new(typeof(Controls.FormLayout), "Key.Catalog_FormLayout_Purpose", "Key.Catalog_FormLayout_Variants", "<FormLayout Columns=\"2\"><Field Label=\"名称\" Id=\"name\"><TextBox Id=\"name\" @bind-Value=\"Draft.Name\" /></Field></FormLayout>"),
            new(typeof(Controls.FormActions), "Key.Catalog_FormActions_Purpose", "Key.Catalog_FormActions_Variants", "<FormActions Columns=\"2\"><UniformGridButton Title=\"保存\" Variant=\"UniformGridVariant.Filled\" Type=\"submit\" /><UniformGridButton Title=\"取消\" Variant=\"UniformGridVariant.Outlined\" OnClick=\"Cancel\" /></FormActions>"),
            new(typeof(Controls.UniformGrid), "Key.Catalog_UniformGrid_Purpose", "Key.Catalog_UniformGrid_Variants", "<UniformGrid Columns=\"2\" NarrowColumns=\"1\" CellHeight=\"100\" IconSupport=\"false\"><UniformGridButton Title=\"保存\" OnClick=\"Save\" /><UniformGridButton Title=\"取消\" OnClick=\"Cancel\" /></UniformGrid>"),
            new(typeof(Controls.UniformGridItem), "Key.Catalog_UniformGridItem_Purpose", "Key.Catalog_UniformGridItem_Variants", "<UniformGrid Shape=\"UniformGridShape.Square\"><UniformGridItem Title=\"项目\" Icon=\"folder\" Text=\"12 个演示项目\" Variant=\"UniformGridVariant.Outlined\" /></UniformGrid>"),
            new(typeof(Controls.SectionNavigator), "Key.Catalog_SectionNavigator_Purpose", "Key.Catalog_SectionNavigator_Variants", "<SectionNavigator ContentId=\"page-content\" Label=\"页面板块\" />"),
            new(typeof(Controls.BackToTop), "Key.Catalog_BackToTop_Purpose", "Key.Catalog_BackToTop_Variants", "<BackToTop ContentId=\"page-content\" Label=\"返回顶部\" />"),
            new(typeof(Controls.LineChart), "Key.Catalog_LineChart_Purpose", "Key.Catalog_LineChart_Variants", "<LineChart Id=\"weekly-orders\" Heading=\"每周订单\" Labels=\"Labels\" Series=\"Series\" Title=\"演示数据\" />"),
            new(typeof(Controls.DataTable<>), "Key.Catalog_DataTable_Purpose", "Key.Catalog_DataTable_Variants", "<DataTable TItem=\"Project\" Items=\"Projects\" Columns=\"Columns\" Actions=\"RowActions\" @bind-PageSize=\"PageSize\" />"),
            new(typeof(Controls.DataSearch<>), "Key.Catalog_DataSearch_Purpose", "Key.Catalog_DataSearch_Variants", "<DataSearch TItem=\"Project\" Columns=\"Columns\" Value=\"@Query\" ColumnKey=\"@SearchColumn\" Changed=\"SearchChanged\"><StandaloneCheckBox Label=\"包括归档\" @bind-Value=\"IncludeArchived\" /></DataSearch>"),
            new(typeof(Controls.ListView<>), "Key.Catalog_ListView_Purpose", "Key.Catalog_ListView_Variants", "<ListView TItem=\"Feature\" Items=\"Features\" Columns=\"Columns\" RowHeaderKey=\"name\" Label=\"功能对照\" />"),
            new(typeof(Controls.Dialog), "Key.Catalog_Dialog_Purpose", "Key.Catalog_Dialog_Variants", "<Dialog Title=\"确认操作\" @bind-IsOpen=\"Open\"><ChildContent><p>操作说明</p></ChildContent><Actions><Button OnClick=\"Confirm\">确认</Button></Actions></Dialog>"),
            new(typeof(Controls.Disclosure), "Key.Catalog_Disclosure_Purpose", "Key.Catalog_Disclosure_Variants", "<Disclosure Title=\"调用说明\" InitiallyOpen=\"false\"><p>说明内容</p></Disclosure>"),
            new(typeof(Controls.Notice), "Key.Catalog_Notice_Purpose", "Key.Catalog_Notice_Variants", "<Notice Severity=\"NotificationSeverity.Success\">保存完成。</Notice>"),
            new(typeof(Controls.EmptyState), "Key.Catalog_EmptyState_Purpose", "Key.Catalog_EmptyState_Variants", "<PageBody><PageHeading Title=\"工作空间\" /><EmptyState Variant=\"EmptyStateVariant.Watermark\" Title=\"欢迎使用工作空间\" /></PageBody>"),
            new(typeof(Controls.LoadingState), "Key.Catalog_LoadingState_Purpose", "Key.Catalog_LoadingState_Variants", "<LoadingState Message=\"正在加载项目…\" />"),
            new(typeof(Controls.ProgressBar), "Key.Catalog_ProgressBar_Purpose", "Key.Catalog_ProgressBar_Variants", "<ProgressBar Label=\"导入进度\" Value=\"Progress\" StatusText=\"@Stage\" />"),
            new(typeof(Controls.TutorialBoard), "Key.Catalog_TutorialBoard_Purpose", "Key.Catalog_TutorialBoard_Variants", "<TutorialBoard Steps=\"Steps\" @bind-IsOpen=\"TutorialOpen\" @bind-ActiveStepKey=\"StepKey\" OnAction=\"OpenStep\" OnSkip=\"SkipTutorial\" />"),
            new(typeof(Controls.ProgressRing), "Key.Catalog_ProgressRing_Purpose", "Key.Catalog_ProgressRing_Variants", "<ProgressRing Label=\"任务进度\" Value=\"Progress\" StatusText=\"@Stage\" Stopped=\"Paused\" />"),
            new(typeof(Controls.PageBody), "Key.Catalog_PageBody_Purpose", "Key.Catalog_PageBody_Variants", "<PageBody FullWidth=\"true\" FillHeight=\"true\"><PageHeading Title=\"项目\" Compact=\"true\" /><Primitives.EditingGrid Columns=\"Columns\" Rows=\"Rows\" CellChanged=\"ChangeCell\" /></PageBody>"),
            new(typeof(Controls.PageHeading), "Key.Catalog_PageHeading_Purpose", "Key.Catalog_PageHeading_Variants", "<PageHeading Title=\"项目\"><Actions><Button OnClick=\"Create\">创建</Button></Actions></PageHeading>"),
            new(typeof(Controls.Section), "Key.Catalog_Section_Purpose", "Key.Catalog_Section_Variants", "<Section Title=\"联系方式\"><p>区块内容</p></Section>"),
            new(typeof(Controls.Card), "Key.Catalog_Card_Purpose", "Key.Catalog_Card_Variants", "<Card Prominent=\"true\" Stacked=\"true\" Text=\"介绍与下一步动作\"><Actions><Button Variant=\"ButtonVariant.Elevated\" OnClick=\"Open\">了解方案</Button></Actions></Card>"),
            new(typeof(Controls.IdentityCard), "Key.Catalog_IdentityCard_Purpose", "Key.Catalog_IdentityCard_Variants", "<IdentityCard Title=\"林晓\" HeadingLevel=\"1\" Columns=\"false\"><CopyText Value=\"DEMO-001\" /></IdentityCard>"),
            new(typeof(Controls.Icon), "Key.Catalog_Icon_Purpose", "Key.Catalog_Icon_Variants", "<Icon Name=\"home\" />"),
            new(typeof(Controls.CodeBlock), "Key.Catalog_CodeBlock_Purpose", "Key.Catalog_CodeBlock_Variants", "<CodeBlock Text=\"@ExampleCode\" Language=\"razor\" />"),
            new(typeof(Controls.FormGroup), "Key.Catalog_FormGroup_Purpose", "Key.Catalog_FormGroup_Variants", "<FormGroup Title=\"联系人\" Disabled=\"Saving\"><Field Label=\"名称\" Id=\"name\"><StandaloneTextBox Id=\"name\" @bind-Value=\"Name\" /></Field></FormGroup>"),
            new(typeof(Controls.InlineActions), "Key.Catalog_InlineActions_Purpose", "Key.Catalog_InlineActions_Variants", "<InlineActions Alignment=\"HorizontalAlignment.End\"><Button OnClick=\"Save\">保存</Button><Button Variant=\"ButtonVariant.Secondary\" OnClick=\"Cancel\">取消</Button></InlineActions>"),
            new(typeof(Controls.ImagePreview), "Key.Catalog_ImagePreview_Purpose", "Key.Catalog_ImagePreview_Variants", "<ImagePreview Url=\"logo.svg\" Alt=\"项目标志\"><ChildContent>标志预览</ChildContent></ImagePreview>"),
            new(typeof(Controls.AttributionFooter), "Key.Catalog_AttributionFooter_Purpose", "Key.Catalog_AttributionFooter_Variants", "<AttributionFooter Text=\"© ARKHEIDE SYSTEM\" />"),
            new(typeof(Controls.CopyText), "Key.Catalog_CopyText_Purpose", "Key.Catalog_CopyText_Variants", "<CopyText Value=\"DEMO-001\" />"),
        ]),
        new("Key.Catalog_Group_Presentation_Title", "Key.Catalog_Group_Presentation_Description",
        [
            new(typeof(Controls.ContentContainer), "Key.Catalog_ContentContainer_Purpose", "Key.Catalog_ContentContainer_Variants", "<ContentContainer><Section Title=\"展示内容\"><p>正文</p></Section></ContentContainer>"),
            new(typeof(Controls.PresentationBand), "Key.Catalog_PresentationBand_Purpose", "Key.Catalog_PresentationBand_Variants", "<PresentationBand Title=\"产品能力\" Tone=\"PresentationTone.Surface\"><p>展示正文。</p></PresentationBand>"),
            new(typeof(Controls.PresentationHero), "Key.Catalog_PresentationHero_Purpose", "Key.Catalog_PresentationHero_Variants", "<PresentationHero Title=\"Lumen\" Subtitle=\"Ideas in one place\" Description=\"虚构产品介绍。\"><Actions><Button Href=\"/examples/display/pricing\">查看方案</Button></Actions></PresentationHero>"),
            new(typeof(Controls.LogoDisplayer), "Key.Catalog_LogoDisplayer_Purpose", "Key.Catalog_LogoDisplayer_Variants", "<LogoDisplayer TitleId=\"login-title\" />"),
            new(typeof(Controls.PresentationFooter), "Key.Catalog_PresentationFooter_Purpose", "Key.Catalog_PresentationFooter_Variants", "<PresentationFooter ProjectName=\"示例组织\"><Actions><Button Variant=\"ButtonVariant.Underline\" Href=\"/examples/display\">展示目录</Button></Actions></PresentationFooter>"),
            new(typeof(Controls.OfferStage), "Key.Catalog_OfferStage_Purpose", "Key.Catalog_OfferStage_Variants", "<OfferStage Id=\"sample-offers\" AutoRotate=\"false\"><OfferCard Id=\"sample-basic\" Title=\"Basic\" Description=\"虚构方案。\"><Button Disabled=\"true\">仅作展示</Button></OfferCard></OfferStage>"),
            new(typeof(Controls.OfferCard), "Key.Catalog_OfferCard_Purpose", "Key.Catalog_OfferCard_Variants", "<OfferStage Id=\"sample-stage\" AutoRotate=\"false\"><OfferCard Id=\"sample-plus\" Title=\"Plus\" Description=\"虚构方案。\"><Button Disabled=\"true\">仅作展示</Button></OfferCard></OfferStage>"),
            new(typeof(Controls.AccessPanel), "Key.Catalog_AccessPanel_Purpose", "Key.Catalog_AccessPanel_Variants", "<AccessPanel><Brand><h1>演示入口</h1></Brand><ChildContent><p>访问内容。</p></ChildContent><Actions><Button Href=\"/examples/display\">返回目录</Button></Actions></AccessPanel>"),
            new(typeof(Controls.AccessFormSurface), "Key.Catalog_AccessFormSurface_Purpose", "Key.Catalog_AccessFormSurface_Variants", "<AccessFormSurface><Field Label=\"邮箱\" Id=\"access-email\"><StandaloneTextBox Id=\"access-email\" Type=\"email\" /></Field><Button Disabled=\"true\">演示入口</Button></AccessFormSurface>"),
            new(typeof(Controls.AccessActions), "Key.Catalog_AccessActions_Purpose", "Key.Catalog_AccessActions_Variants", "<AccessActions><Button Variant=\"ButtonVariant.Underline\" Href=\"/examples/display\">返回展示目录</Button></AccessActions>")
        ]),
        new("Key.Catalog_Group_Protocol_Title", "Key.Catalog_Group_Protocol_Description",
        [
            new(typeof(Controls.DateBox<>), "Key.Catalog_DateBox_Purpose", "Key.Catalog_DateBox_Variants", "<Field Label=\"日期\" Id=\"date\"><DateBox TValue=\"DateTime\" Id=\"date\" @bind-Value=\"Draft.Date\" /></Field>"),
            new(typeof(Controls.FilePicker), "Key.Catalog_FilePicker_Purpose", "Key.Catalog_FilePicker_Variants", "<FilePicker Label=\"选择文件\" Accept=\".txt\" OnChange=\"ReadMetadata\" />"),
            new(typeof(Controls.StandaloneTextBox), "Key.Catalog_StandaloneTextBox_Purpose", "Key.Catalog_StandaloneTextBox_Variants", "<Field Label=\"邮箱\" Id=\"email\"><StandaloneTextBox Id=\"email\" Type=\"email\" name=\"email\" @bind-Value=\"Email\" /></Field>"),
            new(typeof(Controls.StandaloneSelectBox<>), "Key.Catalog_StandaloneSelectBox_Purpose", "Key.Catalog_StandaloneSelectBox_Variants", "<Field Label=\"语言\" Id=\"language\"><StandaloneSelectBox TValue=\"string\" Id=\"language\" Options=\"Languages\" @bind-Value=\"Language\" /></Field>"),
            new(typeof(Controls.StandaloneCheckBox), "Key.Catalog_StandaloneCheckBox_Purpose", "Key.Catalog_StandaloneCheckBox_Variants", "<StandaloneCheckBox Label=\"保留演示选择\" name=\"remember\" @bind-Value=\"Remember\" />"),
            new(typeof(Controls.DropdownSurface), "Key.Catalog_DropdownSurface_Purpose", "Key.Catalog_DropdownSurface_Variants", "<!-- 构造示例不代表完整生产菜单；普通命令菜单使用 ActionMenu。 -->\n<DropdownSurface><Trigger>静态入口</Trigger><ChildContent><Button Variant=\"ButtonVariant.Underline\" Href=\"/examples/display\">展示目录</Button></ChildContent></DropdownSurface>"),
            new(typeof(Primitives.SecondaryNavigationItem), "Key.Catalog_Primitives_SecondaryNavigationItem_Purpose", "Key.Catalog_Primitives_SecondaryNavigationItem_Variants", "<Primitives.SecondaryNavigationItem Label=\"展示目录\" Href=\"/examples/display\" />")
        ]),
        new("Key.Catalog_Group_Composition_Title", "Key.Catalog_Group_Composition_Description",
        [
            new(typeof(Patterns.ContentSurface), "Key.Catalog_Patterns_ContentSurface_Purpose", "Key.Catalog_Patterns_ContentSurface_Variants", "<Patterns.ContentSurface DocumentKey=\"@Navigation.Uri\"><TitleBar><p>顶部栏</p></TitleBar><ChildContent><p>正文</p></ChildContent></Patterns.ContentSurface>"),
            new(typeof(Patterns.NavigationSurface), "Key.Catalog_Patterns_NavigationSurface_Purpose", "Key.Catalog_Patterns_NavigationSurface_Variants", "<Patterns.NavigationSurface ShowSecondary=\"false\"><TitleBar><p>顶部栏</p></TitleBar><PrimaryNavigation><Primitives.PrimaryNavigationItem Id=\"home\" Label=\"主页\" Icon=\"home\" Href=\"/\" /></PrimaryNavigation><ChildContent><p>正文</p></ChildContent></Patterns.NavigationSurface>"),
        ]),
        new("Key.Catalog_Group_SpecializedInputs_Title", "Key.Catalog_Group_SpecializedInputs_Description",
        [
            new(typeof(Primitives.MaskedInput), "Key.Catalog_Primitives_MaskedInput_Purpose", "Key.Catalog_Primitives_MaskedInput_Variants", "<Primitives.MaskedInput Mask=\"00000-000\" @bind-Value=\"Draft.PostalCode\" aria-label=\"邮政编码\" />"),
            new(typeof(Primitives.StandaloneMaskedInput), "Key.Catalog_Primitives_StandaloneMaskedInput_Purpose", "Key.Catalog_Primitives_StandaloneMaskedInput_Variants", "<Primitives.StandaloneMaskedInput Mask=\"00000-000\" @bind-Value=\"PostalCode\" aria-label=\"邮政编码\" />"),
            new(typeof(Primitives.SearchAutocomplete<>), "Key.Catalog_Primitives_SearchAutocomplete_Purpose", "Key.Catalog_Primitives_SearchAutocomplete_Variants", "<label for=\"project-search\">选择项目</label>\n<Primitives.SearchAutocomplete TItem=\"Project\" Id=\"project-search\" Items=\"Projects\" @bind-Value=\"ProjectQuery\" ValueSelector=\"ValueOf\" TextSelector=\"NameOf\" ItemSelected=\"SelectProject\" />"),
            new(typeof(Primitives.ReferenceDropdown<>), "Key.Catalog_Primitives_ReferenceDropdown_Purpose", "Key.Catalog_Primitives_ReferenceDropdown_Variants", "<span id=\"project-label\">选择项目</span>\n<Primitives.ReferenceDropdown TValue=\"int\" Id=\"project\" LabelledBy=\"project-label\" Items=\"References\" @bind-Value=\"SelectedId\" />"),
            new(typeof(Primitives.DataPager), "Key.Catalog_Primitives_DataPager_Purpose", "Key.Catalog_Primitives_DataPager_Variants", "<Primitives.DataPager TotalCount=\"Projects.Count\" CurrentPage=\"Page\" PageSize=\"20\" PageChanged=\"ChangePage\" />"),
            new(typeof(Primitives.EditingGrid), "Key.Catalog_Primitives_EditingGrid_Purpose", "Key.Catalog_Primitives_EditingGrid_Variants", "<Primitives.EditingGrid Columns=\"GridColumns\" Rows=\"GridRows\" Interactions=\"GridCommands\" CellChanged=\"CellChanged\" Label=\"编辑项目\" />"),
            new(typeof(Primitives.NavigationGuard), "Key.Catalog_Primitives_NavigationGuard_Purpose", "Key.Catalog_Primitives_NavigationGuard_Variants", "<Primitives.NavigationGuard HasUnsavedChanges=\"Dirty\" Message=\"存在未保存内容，是否离开？\" />"),
            new(typeof(Primitives.InteractionBoundary), "Key.Catalog_Primitives_InteractionBoundary_Purpose", "Key.Catalog_Primitives_InteractionBoundary_Variants", "<Primitives.InteractionBoundary Locked=\"Saving\" Title=\"正在保存\"><Message><p>请等待完成。</p></Message><ChildContent><p>受控区域</p></ChildContent></Primitives.InteractionBoundary>")
        ]),
        new("Key.Catalog_Group_Navigation_Title", "Key.Catalog_Group_Navigation_Description",
        [
            new(typeof(Primitives.AccessBrand), "Key.Catalog_Primitives_AccessBrand_Purpose", "Key.Catalog_Primitives_AccessBrand_Variants", "<Primitives.AccessBrand LogoPath=\"logo.svg\" BrandName=\"应用\" TitleId=\"access-title\" />"),
            new(typeof(Primitives.ShellHeader), "Key.Catalog_Primitives_ShellHeader_Purpose", "Key.Catalog_Primitives_ShellHeader_Variants", "<Primitives.ShellHeader HomeHref=\"/\" HomeAriaLabel=\"返回主页\" LogoPath=\"logo.svg\" BrandName=\"应用\" />"),
            new(typeof(Primitives.ServiceMenu), "Key.Catalog_Primitives_ServiceMenu_Purpose", "Key.Catalog_Primitives_ServiceMenu_Variants", "<Primitives.ServiceMenu Label=\"页面\" Items=\"ServiceLinks\" />"),
            new(typeof(Primitives.PrimaryNavigationItem), "Key.Catalog_Primitives_PrimaryNavigationItem_Purpose", "Key.Catalog_Primitives_PrimaryNavigationItem_Variants", "<Primitives.PrimaryNavigationItem Id=\"home\" Label=\"主页\" Icon=\"home\" Href=\"/\" Match=\"NavLinkMatch.All\" />"),
            new(typeof(Primitives.NoticeTrigger), "Key.Catalog_Primitives_NoticeTrigger_Purpose", "Key.Catalog_Primitives_NoticeTrigger_Variants", "<Primitives.NoticeTrigger Severity=\"NotificationSeverity.Warning\">此字段需要检查。</Primitives.NoticeTrigger>"),
        ]),
    ];

    public static int Count => Groups.Sum(group => group.Entries.Count);
}

public sealed record CatalogGroup(string TitleKey, string DescriptionKey, IReadOnlyList<ComponentEntry> Entries);

public sealed class ComponentEntry(Type componentType, string purposeKey, string variantsKey, string example)
{
    public Type ComponentType { get; } = componentType;
    public string Name { get; } = DisplayName(componentType);
    public string Namespace { get; } = componentType.Namespace ?? string.Empty;
    public string PurposeKey { get; } = purposeKey;
    public Controls.ComponentUsageInfo Usage => Controls.ComponentUsageCatalog.For(ComponentType);
    public bool IsProductionEntry => Usage.Kind is Controls.ComponentUseKind.General or Controls.ComponentUseKind.Scenario;
    public string VariantsKey { get; } = variantsKey;
    public string Example { get; } = example
        .Replace("Primitives.", "ArkheideSystem.Flourish.Blazor.Components.Primitives.", StringComparison.Ordinal)
        .Replace("Patterns.", "ArkheideSystem.Flourish.Blazor.Components.Patterns.", StringComparison.Ordinal);
    public IReadOnlyList<ComponentParameter> ApiParameters { get; } = DescribeParameters(componentType);
    public string Parameters => string.Join("；", ApiParameters.Select(parameter => $"{parameter.Name}: {parameter.Type}"));

    private static IReadOnlyList<ComponentParameter> DescribeParameters(Type type)
    {
        var parameters = OwnParameters(type).ToList();
        if (type == typeof(Controls.UniformGridButton) || type == typeof(Controls.UniformGridItem))
        {
            // Layout belongs to the container, not to a separate shape on each child.
            var names = new HashSet<string>(StringComparer.Ordinal)
                { "Shape", "Columns", "Rows", "NarrowColumns", "MaxCellSize", "MaxCellHeight", "CellHeight", "IconSupport", "Variant", "Centered" };
            parameters.AddRange(OwnParameters(typeof(Controls.UniformGrid)).Where(parameter => names.Contains(parameter.Name))
                .Select(parameter => parameter with
                {
                    Name = $"UniformGrid.{parameter.Name}",
                    DescriptionPrefixKey = "Key.Catalog_Parameter_UniformGridOwner",
                    DescriptionOwner = type.Name
                }));
        }
        if (type == typeof(Controls.UniformGridButton))
            parameters.AddRange(OwnParameters(typeof(Controls.FormActions)).Where(parameter => parameter.Name == "Columns")
                .Select(parameter => parameter with
                {
                    Name = "FormActions.Columns",
                    DescriptionKey = "Key.Catalog_Parameter_FormActionsColumns"
                }));
        return parameters;
    }

    private static IEnumerable<ComponentParameter> OwnParameters(Type type)
    {
        var defaults = ParameterDefaults.Create(type);
        return type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(property => property.IsDefined(typeof(ParameterAttribute), true))
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .Select(property => new ComponentParameter(property.Name, ParameterType(property.PropertyType),
                property.IsDefined(typeof(EditorRequiredAttribute), true), ParameterMeaning.DescriptionKey(property), defaults.Describe(property)));
    }

    public bool Matches(string query, Func<string, string> localize) => string.IsNullOrWhiteSpace(query)
        || $"{Name} {Namespace} {localize(PurposeKey)} {localize(VariantsKey)} {Parameters} {Usage.Kind}".Contains(query.Trim(), StringComparison.OrdinalIgnoreCase);

    private static string DisplayName(Type type)
    {
        var prefix = type.Namespace?.EndsWith(".Primitives", StringComparison.Ordinal) == true ? "Primitives."
            : type.Namespace?.EndsWith(".Patterns", StringComparison.Ordinal) == true ? "Patterns." : string.Empty;
        return prefix + TypeName(type);
    }

    private static string ParameterType(Type type)
    {
        var valueType = Nullable.GetUnderlyingType(type) ?? type;
        return valueType.IsEnum ? $"{TypeName(type)} ({string.Join(", ", Enum.GetNames(valueType))})" : TypeName(type);
    }

    private static string TypeName(Type type)
    {
        if (type == typeof(string)) return "string";
        if (type == typeof(bool)) return "bool";
        if (type == typeof(int)) return "int";
        if (type == typeof(double)) return "double";
        if (type == typeof(object)) return "object";
        if (Nullable.GetUnderlyingType(type) is { } valueType) return TypeName(valueType) + "?";
        if (!type.IsGenericType) return type.Name;
        return $"{type.Name.Split('`')[0]}<{string.Join(", ", type.GetGenericArguments().Select(TypeName))}>";
    }
}

public sealed record ComponentParameter(string Name, string Type, bool Required, string DescriptionKey, string DefaultValue, string? DescriptionPrefixKey = null, string? DescriptionOwner = null);

internal sealed class ParameterDefaults
{
    private readonly object? instance;
    private readonly Type? closedType;

    private ParameterDefaults(object? instance, Type? closedType)
        => (this.instance, this.closedType) = (instance, closedType);

    public static ParameterDefaults Create(Type type)
    {
        // These reviewed Razor components have no custom constructors. Do not attach them
        // to a renderer, inject services, or run lifecycle methods just to document defaults.
        if (type.Assembly != typeof(Controls.UniformGrid).Assembly) return new(null, null);
        try
        {
            var concrete = type.IsGenericTypeDefinition
                ? type.MakeGenericType(type.GetGenericArguments().Select(_ => typeof(int)).ToArray()) : type;
            return new(Activator.CreateInstance(concrete), concrete);
        }
        catch (ArgumentException) { return new(null, null); }
        catch (MemberAccessException) { return new(null, null); }
        catch (TargetInvocationException) { return new(null, null); }
    }

    public string Describe(PropertyInfo parameter)
    {
        if (instance is null || closedType is null) return string.Empty;
        // Read only auto-property backing fields. A parameter getter may depend on runtime
        // state; generic Value defaults must not be mistaken for the chosen probe type's zero.
        var property = closedType.GetProperty(parameter.Name, BindingFlags.Instance | BindingFlags.Public);
        var field = property?.DeclaringType?.GetField($"<{parameter.Name}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
        if (field is null || parameter.PropertyType.IsGenericParameter
            || Nullable.GetUnderlyingType(parameter.PropertyType)?.ContainsGenericParameters == true) return string.Empty;
        var value = field.GetValue(instance);
        if (value is null) return string.Empty;
        if (value is CultureInfo && parameter.DeclaringType?.IsGenericType == true
            && (parameter.DeclaringType.GetGenericTypeDefinition() == typeof(Controls.DataTable<>)
                || parameter.DeclaringType.GetGenericTypeDefinition() == typeof(Controls.ListView<>)))
            return "CultureInfo.CurrentCulture";
        if (value is string text) return System.Text.Json.JsonSerializer.Serialize(text);
        if (value is bool boolean) return boolean ? "true" : "false";
        if (value is Enum enumeration) return $"{enumeration.GetType().Name}.{enumeration}";
        if (value is byte or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal)
            return Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
        if (value is ICollection { Count: 0 }) return "[]";
        if (value is IEnumerable && value.GetType().IsGenericType
            && value.GetType().GetGenericTypeDefinition() == typeof(HashSet<>)
            && value.GetType().GetProperty("Count")?.GetValue(value) is 0) return "[]";
        if (value.GetType().Name is "TableText" or "GridText" or "GridInteractions") return $"new {value.GetType().Name}()";
        return string.Empty;
    }
}
