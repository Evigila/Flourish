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
        new("应用外壳与主题", "ApplicationLayout 从 Program 的配置生成外壳；页面通常无需自行拼装导航和顶部栏。Design 统一管理整个应用的颜色角色。",
        [
            new(typeof(Controls.ApplicationLayout), "路由默认布局，加载框架与可选主题资源。", "Router 的 DefaultLayout 指定一次即可；页面只提供正文。", "<RouteView RouteData=\"routeData\" DefaultLayout=\"typeof(ApplicationLayout)\" />"),
            new(typeof(Controls.ApplicationShell), "承载顶部栏、两级导航、固定导航与页面正文。", "默认使用 Program 配置；TitleBarStart / Brand / End 可用于自定义宿主组合。", "<ApplicationShell><PageBody><PageHeading Title=\"主页\" /></PageBody></ApplicationShell>")
        ]),
        new("常用控件与页面布局", "使用 Components 命名空间。输入示例中的 Draft 由页面定义；InputBase 控件需要 ValueExpression，使用 @bind-Value 会自动提供。",
        [
            new(typeof(Controls.ValidationMessages), "显示隐藏或跨字段验证错误。", "普通字段通过 Field.For 共用此呈现，不重复放入消息；独立使用 For 绑定 EditContext 字段，Error 可提供显式错误。订阅随表单切换或释放清理，Id 关联可访问错误说明。", "<ValidationMessages Id=\"token-error\" For=\"@(() => Draft.Token)\" />"),
            new(typeof(Controls.NavigationChoices), "使用原生 GET 链接选择访问方式。", "Id/Label/Items/ActiveKey必填；NavigationChoiceItem包含Key/Label/Href/Disabled。ChildContent按项提供业务内容；链接完整导航、无JS，非活动面板保持挂载但隐藏。案例 /examples/display/access-methods。", "<NavigationChoices Id=\"methods\" Label=\"访问方式\" Items=\"Methods\" ActiveKey=\"@ActiveMethod\"><ChildContent Context=\"method\"><p>@method.Label</p></ChildContent></NavigationChoices>"),
            new(typeof(Controls.Button), "在页面标题、工具区或弹窗中触发操作，也可提供导航链接。", "Primary、Secondary、Danger、Quiet、Underline、Elevated 六种外观；Disabled / Busy 是状态属性。Icon 可选；Text 与 ChildContent 都为空时自动只显示图标，需提供 aria-label。Href 提供导航目标。Primary 与 Secondary 是填充和描边的唯一入口。", "<Button Variant=\"ButtonVariant.Secondary\" Icon=\"save\" Text=\"保存\" OnClick=\"SaveAsync\" Busy=\"Saving\" />"),
            new(typeof(Controls.ExpansionIndicator), "显示展开状态的装饰性三角标记，放在控制区域展开的按钮中。", "与 SelectBox 的原生三角标记保持一致；Expanded 控制展开状态，Design 使用 140ms 旋转动画并支持减少动态效果。按钮负责点击、名称和 aria-expanded / aria-controls；标记本身不可交互。SplitButton、导航和菜单已复用。", "<Button Variant=\"ButtonVariant.Quiet\" OnClick=\"() => Expanded = !Expanded\" aria-expanded=\"@(Expanded ? \"true\" : \"false\")\" aria-controls=\"details\">详情 <ExpansionIndicator Expanded=\"Expanded\" /></Button>\n<div id=\"details\" hidden=\"@(!Expanded)\">详情内容</div>"),
            new(typeof(Controls.SplitButton), "将主动作和方形次入口组合，支持原生下拉内容。", "MenuContent 提供静态 SSR 可用的原生 details 导航/动作，与受控 Expanded/OnSecondaryClick 互斥；不设置 MenuContent 时使用明确的双动作回调。Type=submit 只提交主按钮。FullWidth 优先于像素 Width，次入口保持 48px 方形；Busy/Disabled 不挂载可用菜单。", "<SplitButton Text=\"继续\" Type=\"submit\" FullWidth=\"true\"><MenuContent><Button Variant=\"ButtonVariant.Quiet\" Href=\"/examples/display\">返回</Button></MenuContent></SplitButton>"),
            new(typeof(Controls.DisplayBoard), "为控件预览、演示场景和可复制内容提供统一展示区域。", "Dotted 默认 true；Centered 默认 true，控制普通子内容的位置。CodeBlock 始终从左上开始；代码背景板可禁用点阵。CopyText 提供完整复制内容；右上角 Elevated 图标按钮复制成功后暂时显示勾号。", "<DisplayBoard><Button Icon=\"add\" Text=\"创建\" /></DisplayBoard>\n<DisplayBoard Dotted=\"false\" CopyText=\"@Source\" CopyLabel=\"复制代码\"><CodeBlock Text=\"@Source\" Language=\"razor\" /></DisplayBoard>"),
            new(typeof(Controls.UniformGridButton), "为等宽网格提供可点击的单元格，用于表单操作和多步向导的有限选项。", "配合 UniformGrid 或 FormActions 使用。纯向导选择步骤只有网格和按钮文字，不使用独立标题、说明或下拉框；案例 /examples/display/wizard。长方形铺满容器宽度后按列均分，正方形保留边长上限。Variant 支持 Filled、Outlined、Danger、Elevated；省略时继承网格外观，独立使用时为 Elevated。主要操作显式设为 Filled，其余操作设为 Outlined。Title 使用 H1 的 34px，Text 与 ChildContent 使用正文 17px。Icon 可选，有图标默认自动启用上下等分布局，IconSupport=false 保留连续布局；Href、Type、Busy、Disabled 与 OnClick 复用 Button 行为。", "<UniformGrid Columns=\"2\" NarrowColumns=\"1\"><UniformGridButton Title=\"个人使用\" OnClick=\"ChoosePersonal\" /><UniformGridButton Title=\"工作空间\" OnClick=\"ChooseWorkspace\" /></UniformGrid>"),
            new(typeof(Controls.ActionMenu), "统一命令和记录原生操作菜单。", "Actions 使用 MenuAction；ChildContent 使用真实 Button 或业务 GET/POST 表单，两者互斥。共享可用性、键盘、异步关闭与弹窗焦点恢复；OpenOnHover 仅适用于 Actions。", "<ActionMenu Label=\"更多操作\" Actions=\"Actions\" />\n<ActionMenu Label=\"记录操作\"><Button role=\"menuitem\" OnClick=\"Edit\">编辑</Button></ActionMenu>"),
            new(typeof(Controls.TextBox), "绑定文字并参与表单验证。", "Type 支持原生输入类型；Multiline 与 Rows 切换多行；MaxLength 限制输入长度。", "<Field Label=\"名称\" Id=\"name\"><TextBox Id=\"name\" @bind-Value=\"Draft.Name\" MaxLength=\"120\" /></Field>"),
            new(typeof(Controls.NumberBox<>), "绑定数值并处理解析错误。", "TValue 支持 int、long、short、decimal、double、float 及其可空类型；Min / Max / Step 控制原生输入范围。", "<Field Label=\"金额\" Id=\"amount\"><NumberBox TValue=\"decimal\" Id=\"amount\" @bind-Value=\"Draft.Amount\" Min=\"0\" Step=\"0.01\" /></Field>"),
            new(typeof(Controls.SelectBox<>), "绑定一个选项，包括枚举值。", "Options 使用 SelectOption<TValue>；也可用 ChildContent 提供 option；不可用项由选项的 Disabled 控制。支持可定制原生选择器的浏览器中，下拉面板始终右对齐，并使用共享的三角旋转效果。", "<Field Label=\"状态\" Id=\"state\"><SelectBox TValue=\"RecordState\" Id=\"state\" Options=\"States\" @bind-Value=\"Draft.State\" /></Field>"),
            new(typeof(Controls.MultiSelectBox), "选择多个选项，可搜索、新增并按需排序。", "Items 使用 MultiSelectOption(Key, Label, Selected=false, CanDeselect=true, CanReorder=true, Disabled=false)，Changed 返回完整 MultiSelectChange(OrderedKeys, SelectedKeys) 快照。默认无选择、不排序、不搜索；MinimumSelected / MaximumSelections 约束数量，Searchable / CreateRequested 可启用搜索和宿主新增。DataTable 与 LineChart 复用同一控件并显式开启排序。", "<MultiSelectBox Label=\"活动标签\" Items=\"Tags\" Changed=\"AcceptSelection\" Searchable=\"true\" MaximumSelections=\"3\" />"),
            new(typeof(Controls.CheckBox), "绑定布尔字段并参与表单验证。", "Label 或 ChildContent 提供说明；Disabled 禁用交互。Design 中选中框边长等于单行输入框高度，默认为 48px。", "<CheckBox Label=\"启用\" @bind-Value=\"Draft.Active\" />"),
            new(typeof(Controls.ToggleSwitch), "切换一个布尔状态。", "OnLabel / OffLabel 提供状态文字；Controls 指向受控区域；可独立于 EditForm 使用。", "<ToggleSwitch Label=\"显示详情\" @bind-Value=\"ShowDetails\" Controls=\"details\" />"),
            new(typeof(Controls.ToggleSection), "用开关显示或隐藏一段内容。", "Value 绑定展开状态；隐藏内容仍保留其状态；Disabled 禁用切换。", "<ToggleSection Label=\"高级选项\" @bind-Value=\"Expanded\"><p>选项内容</p></ToggleSection>"),
            new(typeof(Controls.SearchBox), "接收搜索文字并发送查询事件。", "ValueChanged 立即反映输入；防抖后 SearchRequested 接收文字与 CancellationToken，适合可取消的远程查询；SearchChanged 在查询未取消时通知；DebounceMilliseconds 控制延迟。", "<SearchBox Label=\"搜索项目\" @bind-Value=\"Query\" SearchChanged=\"FilterItems\" />"),
            new(typeof(Controls.Field), "关联标签、输入、必填状态与验证错误。", "For 连接表单验证；Error 传入外部错误；FullWidth 占用整行。Id 应与输入相同。", "<Field Label=\"名称\" Id=\"name\" For=\"@(() => Draft.Name)\"><TextBox Id=\"name\" @bind-Value=\"Draft.Name\" /></Field>"),
            new(typeof(Controls.FormLayout), "排列字段并将焦点移到无效输入。", "Columns 指定列数；FocusInvalidFields 可关闭自动聚焦。需要外层 EditForm 负责提交与验证。", "<FormLayout Columns=\"2\"><Field Label=\"名称\" Id=\"name\"><TextBox Id=\"name\" @bind-Value=\"Draft.Name\" /></Field></FormLayout>"),
            new(typeof(Controls.FormActions), "统一表单操作区域的列数与尺寸。", "Columns 指定操作列数；子项使用 UniformGridButton；窄窗口通过主题样式适配。建议主要操作显式使用 Variant=Filled，其余操作使用 Variant=Outlined；按操作语义配置，不依赖子项顺序。", "<FormActions Columns=\"2\"><UniformGridButton Title=\"保存\" Variant=\"UniformGridVariant.Filled\" Type=\"submit\" /><UniformGridButton Title=\"取消\" Variant=\"UniformGridVariant.Outlined\" OnClick=\"Cancel\" /></FormActions>"),
            new(typeof(Controls.UniformGrid), "将多个单元格排列成等宽网格，适合入口目录、向导选择与信息概览。", "Shape 默认 Rectangle，Square 为正方形。Variant 控制 Filled、Outlined、Danger 或默认 Elevated 外观。容器透明，不为未占用位置绘制背景或阴影。长方形默认 width:100%，先铺满容器再按列均分；MaxCellHeight 默认 260px，仅限制高度，不形成宽度上限。自动列数使用两倍高度作为换列阈值，轨道仍可继续拉宽。正方形保留 fit-content 与 MaxCellSize 默认 280px 的等量宽高上限。Columns / Rows 可显式指定，NarrowColumns 仅在设置后覆盖窄窗口列数；Centered 只控制定位，不收缩长方形。IconSupport 可配置上下等分布局，null 时有图标自动启用，false 保留连续布局；单格可覆盖。单元格使用 UniformGridItem 或 UniformGridButton，可各自覆盖 Variant。", "<UniformGrid Columns=\"2\" NarrowColumns=\"1\"><UniformGridButton Title=\"个人使用\" OnClick=\"ChoosePersonal\" /><UniformGridButton Title=\"工作空间\" OnClick=\"ChooseWorkspace\" /></UniformGrid>"),
            new(typeof(Controls.UniformGridItem), "在等宽网格中展示没有点击行为的信息。", "与 UniformGridButton 共用 Filled、Outlined、Danger、Elevated 外观。Variant 省略时继承网格外观，独立使用时为 Elevated。Title 使用 H1 的 34px，Text 与 ChildContent 使用正文 17px；Icon 可选。没有 Href 或 OnClick。", "<UniformGrid Shape=\"UniformGridShape.Square\"><UniformGridItem Title=\"项目\" Icon=\"folder\" Text=\"12 个演示项目\" Variant=\"UniformGridVariant.Outlined\" /></UniformGrid>"),
            new(typeof(Controls.SectionNavigator), "在正文右侧显示页面板块入口，不占用正文宽度。", "默认跟随 ApplicationShell 中当前页面的 H2 标题。独立使用时指定 ContentId；Items 可显式提供标题与目标 ID。悬浮或聚焦显示标题，当前板块的圆点放大并显示主题色外圈。", "<SectionNavigator ContentId=\"page-content\" Label=\"页面板块\" />"),
            new(typeof(Controls.BackToTop), "使用标准居中图标返回指定正文的顶部。", "ContentId 指定真实滚动区；Threshold 决定增强后的显示阈值。静态 SSR 保留可用的片段链接；增强尊重减少动态效果并将焦点交还正文。", "<BackToTop ContentId=\"page-content\" Label=\"返回顶部\" />"),
            new(typeof(Controls.LineChart), "以标准主题展示真实数值的多序列折线图。", "Labels 使用 PointLabel，Series 使用 DataSeries；序列可独立启用、格式化并声明最大值。默认共享刻度；IndependentScales 显式采用独立刻度且显示各序列最大值。包含精确值的辅助技术表格，不实现业务计算。", "<LineChart Labels=\"Labels\" Series=\"Series\" Title=\"演示数据\" />"),
            new(typeof(Controls.DataTable<>), "显示带搜索、排序、分页和行操作的数据。", "View 在 Table / Cards 间切换；Columns 使用 TableColumn<TItem>；Actions 使用 RowAction<TItem>。每页默认 10 项，可选择 20 / 50 / 100；PageSizeChanged 支持 @bind-PageSize。Loading、Error、EmptyMessage 显示真实状态。", "<DataTable TItem=\"Project\" Items=\"Projects\" Columns=\"Columns\" Actions=\"RowActions\" @bind-PageSize=\"PageSize\" />"),
            new(typeof(Controls.DataSearch<>), "使用标准字段按共享列合同发出受控搜索请求。", "Columns 使用 TableColumn<TItem>，Changed 使用 TableSearchRequest；Value / ColumnKey 由宿主管理。默认第一个 Searchable 列；IncludeAllColumns=true 允许空键代表全部列。Disabled 阻止回调，MaximumLength 限制输入，ChildContent 与查询框同行。与 DataTable 共用同一搜索呈现，不拥有业务查询或防抖服务。", "<DataSearch TItem=\"Project\" Columns=\"Columns\" Value=\"@Query\" ColumnKey=\"@SearchColumn\" Changed=\"SearchChanged\"><StandaloneCheckBox Label=\"包括归档\" @bind-Value=\"IncludeArchived\" /></DataSearch>"),
            new(typeof(Controls.ListView<>), "以与 DataTable 相同的表格样式展示静态只读内容。", "Items 按传入顺序完整展示，无搜索、排序、分页、加载状态、选中、行操作或 JavaScript 控制器。Columns 复用 TableColumn<TItem> 的标题、值和 Format；Sortable / Searchable / CanHide 不启用动态能力。RowHeaderKey 提供行标题，CellTemplate 使用 TableCellContext<TItem> 定制只读内容。Caption 提供辅助技术表格说明；省略时使用 Label。", "<ListView TItem=\"Feature\" Items=\"Features\" Columns=\"Columns\" RowHeaderKey=\"name\" Label=\"功能对照\" />"),
            new(typeof(Controls.Dialog), "打开模态对话框并管理焦点与关闭，也用于操作确认。", "Presentation 支持居中或底部面板；Busy 禁用关闭；CanClose 可异步确认；Actions 使用普通 Button 执行确认与取消。ShowAsync 等待 CloseAsync 的结果；BrowserControlled / Views 接入浏览器协议状态，而不用另一个弹窗控件。", "<Dialog Title=\"确认操作\" @bind-IsOpen=\"Open\"><ChildContent><p>操作说明</p></ChildContent><Actions><Button OnClick=\"Confirm\">确认</Button></Actions></Dialog>"),
            new(typeof(Controls.Disclosure), "提供原生 details 展开区域。", "InitiallyOpen 指定初始展开；Title 是可访问触发器文字。", "<Disclosure Title=\"调用说明\" InitiallyOpen=\"false\"><p>说明内容</p></Disclosure>"),
            new(typeof(Controls.Notice), "显示操作结果或需要注意的状态。", "Severity 使用 NotificationSeverity；Announce 控制播报；Subtle 提供轻量外观；Title 可增加标题。", "<Notice Severity=\"NotificationSeverity.Success\">保存完成。</Notice>"),
            new(typeof(Controls.EmptyState), "显示没有数据时的真实状态与可选操作。", "Title 描述当前空结果；Actions 提供下一步操作。", "<EmptyState Title=\"没有项目\"><Actions><Button OnClick=\"Create\">创建项目</Button></Actions></EmptyState>"),
            new(typeof(Controls.LoadingState), "显示内容加载状态。", "Message 提供播报内容；ChildContent 可替换文字。", "<LoadingState Message=\"正在加载项目…\" />"),
            new(typeof(Controls.ProgressBar), "显示完整任务的进度条。", "Value 使用 0–100；null 表示未知进度；StatusText 说明当前阶段。", "<ProgressBar Label=\"导入进度\" Value=\"Progress\" StatusText=\"@Stage\" />"),
            new(typeof(Controls.ProgressRing), "在紧凑位置显示进度环。", "Value 使用 0–100 或 null；Stopped 暂停旋转；StatusText 为阶段提示。", "<ProgressRing Label=\"任务进度\" Value=\"Progress\" StatusText=\"@Stage\" Stopped=\"Paused\" />"),
            new(typeof(Controls.PageBody), "为页面正文提供统一宽度与边距。", "Fluid 省略时继承 Program 的布局配置；true 使用流动宽度，false 使用居中最大宽度；FullWidth 使用完整正文区域。", "<PageBody><PageHeading Title=\"项目\" /><Section Title=\"列表\"><p>页面内容</p></Section></PageBody>"),
            new(typeof(Controls.PageHeading), "显示可收缩页面标题与操作。", "Design 中展开标题为 50px、坍缩标题为 38px；38px 仅用于坍缩大标题。Compact 指定紧凑状态；ParentHref / ParentLabel 显示真正的上级链接；Actions 放置操作。", "<PageHeading Title=\"项目\"><Actions><Button OnClick=\"Create\">创建</Button></Actions></PageHeading>"),
            new(typeof(Controls.Section), "组织有标题的页面内容区。", "Title 使用真实内容标题；Actions 提供区级操作；正文通过 ChildContent 提供。", "<Section Title=\"联系方式\"><p>区块内容</p></Section>"),
            new(typeof(Controls.Card), "承载常规摘要或展示页大卡片。", "默认 Title / ChildContent 保持原 h3 与正文结构。Prominent 默认将 Actions 放左侧、Title 和 Text 放右侧正文；Stacked=true 始终正文在上、操作在下。Text 使用 H1 字号的 p，不新增页面 h1。窄屏纵排、无动作占满宽度、长文自然增长；使用现有卡片角色色与标准 Elevated Button，不是新控件或宿主皮肤。", "<Card Prominent=\"true\" Stacked=\"true\" Text=\"介绍与下一步动作\"><Actions><Button Variant=\"ButtonVariant.Elevated\" OnClick=\"Open\">了解方案</Button></Actions></Card>"),
            new(typeof(Controls.IdentityCard), "显示身份主值及关联信息。", "Columns 控制分栏；SideContent 提供侧栏；内容使用 dl / dt / dd 等语义结构。", "<IdentityCard Title=\"项目负责人\"><dl><dt>名称</dt><dd class=\"f-identity-primary\">林晓</dd></dl></IdentityCard>"),
            new(typeof(Controls.Icon), "显示自托管 Material Symbols Outlined 图标。", "Name 只接受官方图标名称；Class 可调整宿主组合。颜色继承父控件，纯图标操作需要可访问名称。", "<Icon Name=\"home\" />"),
            new(typeof(Controls.CodeBlock), "将代码以文本方式显示。", "Text 自动转义；Language 标识语言，不执行代码。", "<CodeBlock Text=\"@ExampleCode\" Language=\"razor\" />"),
            new(typeof(Controls.FormGroup), "按原生字段组组织标准输入。", "Title 提供 legend；Columns 使用 FormLayout；Disabled 整体禁用字段。AdditionalAttributes 保留原生属性。", "<FormGroup Title=\"联系人\" Disabled=\"Saving\"><Field Label=\"名称\" Id=\"name\"><StandaloneTextBox Id=\"name\" @bind-Value=\"Name\" /></Field></FormGroup>"),
            new(typeof(Controls.InlineActions), "将小型操作排列为可换行的标准动作行。", "ChildContent 直接包含标准 Button；大型等宽表单操作使用 FormActions。", "<InlineActions><Button OnClick=\"Save\">保存</Button><Button Variant=\"ButtonVariant.Secondary\" OnClick=\"Cancel\">取消</Button></InlineActions>"),
            new(typeof(Controls.ImagePreview), "显示图片、可选说明和标准操作。", "Url 与 Alt 必填；ChildContent 放说明，Actions 通过 InlineActions 排列标准按钮。", "<ImagePreview Url=\"logo.svg\" Alt=\"项目标志\"><ChildContent>标志预览</ChildContent></ImagePreview>"),
            new(typeof(Controls.AttributionFooter), "显示紧凑版权或来源说明。", "Text 必填，版权主体和文案由宿主提供；不创建项目水印。", "<AttributionFooter Text=\"© ARKHEIDE SYSTEM\" />"),
            new(typeof(Controls.CopyText), "以统一可选择的代码文本显示需要复制的原值。", "Value 必填，只做编码后的只读文本呈现；不改写文本、不自动调用剪贴板。完整复制动作使用 DisplayBoard.CopyText。", "<CopyText Value=\"DEMO-001\" />"),
        ]),
        new("展示与访问页面", "展示页面使用全宽背景与居中内容轨道，不套用业务页的粘性标题；认证、价格与品牌文案仍由宿主负责。",
        [
            new(typeof(Controls.ContentContainer), "为展示内容提供统一的居中限宽轨道。", "外部横幅负责全宽背景，本容器只约束内部控件与正文，不接管业务数据。", "<ContentContainer><Section Title=\"展示内容\"><p>正文</p></Section></ContentContainer>"),
            new(typeof(Controls.PresentationBand), "将全宽背景和居中内容组合为展示页章节。", "MinHeight 默认 800px，内容允许自然增高；Dotted 默认关闭。Tone 使用 Canvas、Surface 或 Primary，推荐相邻横幅不同色；内容使用同一限宽轨道。", "<PresentationBand Title=\"产品能力\" Tone=\"PresentationTone.Surface\"><p>展示正文。</p></PresentationBand>"),
            new(typeof(Controls.PresentationHero), "提供产品介绍页的艺术标题、副标题和正文。", "只适用于展示页首屏；Actions 使用标准 Button，不用于业务页面标题。", "<PresentationHero Title=\"Lumen\" Subtitle=\"Ideas in one place\" Description=\"虚构产品介绍。\"><Actions><Button Href=\"/examples/display/pricing\">查看方案</Button></Actions></PresentationHero>"),
            new(typeof(Controls.LogoDisplayer), "在 Logo 下展示项目艺术字。", "默认读取 ConfigureProject 的 Logo 和项目名称；ProjectName/LogoPath 可显式覆盖。仅用于访问和展示场景，不扩大业务正文；HeadingLevel 选择语义标题级别。", "<LogoDisplayer TitleId=\"login-title\" />"),
            new(typeof(Controls.PresentationFooter), "提供展示页品牌、版权与大字背景页脚。", "标题与水印自动跟随 ConfigureProject 的项目名称和语言。Copyright 由宿主独立提供；Actions 放标准 Underline 导航按钮。", "<PresentationFooter Copyright=\"虚构展示\"><Actions><Button Variant=\"ButtonVariant.Underline\" Href=\"/examples/display\">展示目录</Button></Actions></PresentationFooter>"),
            new(typeof(Controls.OfferStage), "排列并渐进增强展示类报价卡片。", "搭配 OfferCard；AutoRotate 可关闭，减少动态效果和焦点交互暂停轮播。LinkScopeId 隔离当前页面中的报价锚点。无许可与价格计算。", "<OfferStage Id=\"sample-offers\" AutoRotate=\"false\"><OfferCard Id=\"sample-basic\" Title=\"Basic\" Description=\"虚构方案。\"><Button Disabled=\"true\">仅作展示</Button></OfferCard></OfferStage>"),
            new(typeof(Controls.OfferCard), "展示一个方案的标题、说明和标准动作。", "仅作为 OfferStage 子项；Id 为真实锚点，价格与可用动作由宿主提供。", "<OfferStage Id=\"sample-stage\" AutoRotate=\"false\"><OfferCard Id=\"sample-plus\" Title=\"Plus\" Description=\"虚构方案。\"><Button Disabled=\"true\">仅作展示</Button></OfferCard></OfferStage>"),
            new(typeof(Controls.AccessPanel), "为访问或登录页面提供标准面板与插槽。", "Brand、ChildContent、Actions 分别承载品牌、协议表单和辅助导航；Wide、Emphasized 是布局与表面变种。面板本身不进行认证。", "<AccessPanel><Brand><h1>演示入口</h1></Brand><ChildContent><p>访问内容。</p></ChildContent><Actions><Button Href=\"/examples/display\">返回目录</Button></Actions></AccessPanel>"),
            new(typeof(Controls.AccessFormSurface), "排列访问协议表单中的字段和提交动作。", "外层原生 GET/POST 或 EditForm 由宿主提供；使用 Field 与标准输入，表面不创建协议或提交处理器。", "<AccessFormSurface><Field Label=\"邮箱\" Id=\"access-email\"><StandaloneTextBox Id=\"access-email\" Type=\"email\" /></Field><Button Disabled=\"true\">演示入口</Button></AccessFormSurface>"),
            new(typeof(Controls.AccessActions), "排列访问页的辅助导航与动作。", "使用标准 Button 子项；不创建账号、不认证，也不推断权限。", "<AccessActions><Button Variant=\"ButtonVariant.Underline\" Href=\"/examples/display\">返回展示目录</Button></AccessActions>")
        ]),
        new("协议边界与特定宿主", "这些入口按源码用途元数据区分生产场景与构造边界，不是业务表单控件的另一套替代规范。",
        [
            new(typeof(Controls.DateBox<>), "绑定日期并接入 EditForm 验证。", "TValue 采用 InputDate 支持的日期类型；Type 控制日期、时间或日期时间输入。", "<Field Label=\"日期\" Id=\"date\"><DateBox TValue=\"DateTime\" Id=\"date\" @bind-Value=\"Draft.Date\" /></Field>"),
            new(typeof(Controls.FilePicker), "选择文件并将文件信息交给宿主。", "Accept、Multiple、OnChange 仅配置文件选择；上传、大小限制和内容验证由宿主负责。", "<FilePicker Label=\"选择文件\" Accept=\".txt\" OnChange=\"ReadMetadata\" />"),
            new(typeof(Controls.StandaloneTextBox), "在原生协议表单或无 EditContext 场景绑定输入字符串。", "与 TextBox 共用标准样式；name 属性保留 GET/POST 字段，Type 可使用 email、number 等原生输入类型；数字约束通过 min/max/step 提供。ChangeEvent 支持 input 或 change，不执行模型校验。", "<Field Label=\"邮箱\" Id=\"email\"><StandaloneTextBox Id=\"email\" Type=\"email\" name=\"email\" @bind-Value=\"Email\" /></Field>"),
            new(typeof(Controls.StandaloneSelectBox<>), "在协议边界或无 EditContext 场景选择一个值。", "Options 复用 SelectOption<TValue>；name 属性参与原生提交；业务表单使用 SelectBox。", "<Field Label=\"语言\" Id=\"language\"><StandaloneSelectBox TValue=\"string\" Id=\"language\" Options=\"Languages\" @bind-Value=\"Language\" /></Field>"),
            new(typeof(Controls.StandaloneCheckBox), "在协议边界提供布尔选择与原生提交值。", "SubmissionValue 指定选中提交值；WrapLabel 配合宿主协议标记；业务模型验证使用 CheckBox。", "<StandaloneCheckBox Label=\"保留演示选择\" name=\"remember\" @bind-Value=\"Remember\" />"),
            new(typeof(Controls.DropdownSurface), "为协议宿主提供原生 details 构造表面。", "不包含选择、菜单焦点或外部点击关闭，不能独立替代完整生产菜单；普通命令菜单使用 ActionMenu。", "<!-- 构造示例不代表完整生产菜单；普通命令菜单使用 ActionMenu。 -->\n<DropdownSurface><Trigger>静态入口</Trigger><ChildContent><Button Variant=\"ButtonVariant.Underline\" Href=\"/examples/display\">展示目录</Button></ChildContent></DropdownSurface>"),
            new(typeof(Primitives.SecondaryNavigationItem), "在自定义导航宿主中呈现二级链接。", "常规应用由 Program 的 ConfigureNavigation 定义导航，不自行拼装外壳。", "<Primitives.SecondaryNavigationItem Label=\"展示目录\" Href=\"/examples/display\" />")
        ]),
        new("页面组合", "Patterns 命名空间提供较低层的页面组合。通常优先使用 ApplicationLayout；以下组件适用于自定义宿主。",
        [
            new(typeof(Patterns.ContentSurface), "提供固定顶部栏与正文滚动区，也支持文档流布局。", "TitleBar / ChildContent / Footer 提供区域；DocumentKey 在页面切换时识别正文；ContentId 与 SkipLabel 提供跳过链接。DocumentFlow=true 使用文档滚动，可与 FullHeight PresentationBand 组合独立展示或访问页面。", "<Patterns.ContentSurface DocumentKey=\"@Navigation.Uri\"><TitleBar><p>顶部栏</p></TitleBar><ChildContent><p>正文</p></ChildContent></Patterns.ContentSurface>"),
            new(typeof(Patterns.NavigationSurface), "组合顶部栏、一级与二级导航和正文。", "ShowSecondary 控制二级栏；区域由命名片段提供；DocumentKey 识别页面。一级导航使用 PrimaryNavigationItem，插槽中的页面操作使用标准 Button；Underline 适合轻量操作或链接。", "<Patterns.NavigationSurface ShowSecondary=\"false\"><TitleBar><p>顶部栏</p></TitleBar><PrimaryNavigation><Primitives.PrimaryNavigationItem Id=\"home\" Label=\"主页\" Icon=\"home\" Href=\"/\" /></PrimaryNavigation><ChildContent><p>正文</p></ChildContent></Patterns.NavigationSurface>"),
        ]),
        new("专用输入与编辑场景", "掩码、引用选择、搜索候选和电子表格编辑各有明确场景。数据模型与回调由应用定义。",
        [
            new(typeof(Primitives.MaskedInput), "将输入掩码接入 EditForm。", "继承 InputBase<string?>；Mask 指定格式，InputMode 指定原生键盘类型。", "<Primitives.MaskedInput Mask=\"00000-000\" @bind-Value=\"Draft.PostalCode\" aria-label=\"邮政编码\" />"),
            new(typeof(Primitives.StandaloneMaskedInput), "在表单上下文之外绑定掩码输入。", "Value / ValueChanged 提供双向绑定；原生属性通过 AdditionalAttributes 传递。", "<Primitives.StandaloneMaskedInput Mask=\"00000-000\" @bind-Value=\"PostalCode\" aria-label=\"邮政编码\" />"),
            new(typeof(Primitives.SearchAutocomplete<>), "从候选集合搜索并选择结果。", "Value 双向绑定查询文字；ValueSelector / TextSelector 提供值与显示名；ItemSelected 接收选择；MaximumResults 限制候选；FilterItems 可由宿主接管过滤。", "<label for=\"project-search\">选择项目</label>\n<Primitives.SearchAutocomplete TItem=\"Project\" Id=\"project-search\" Items=\"Projects\" @bind-Value=\"ProjectQuery\" ValueSelector=\"ValueOf\" TextSelector=\"NameOf\" ItemSelected=\"SelectProject\" />"),
            new(typeof(Primitives.ReferenceDropdown<>), "搜索并选择一个值类型引用。", "TValue 必须为 struct；Value 可以为 null；ValueChanged 配合 @bind-Value；AllowNone 控制清除；CreateRequested 可创建引用。", "<span id=\"project-label\">选择项目</span>\n<Primitives.ReferenceDropdown TValue=\"int\" Id=\"project\" LabelledBy=\"project-label\" Items=\"References\" @bind-Value=\"SelectedId\" />"),
            new(typeof(Primitives.DataPager), "控制分页并显示记录范围。", "TotalCount / CurrentPage / PageSize 提供状态；PageChanged 接收页码；IsLimited 标识受限结果。", "<Primitives.DataPager TotalCount=\"Projects.Count\" CurrentPage=\"Page\" PageSize=\"20\" PageChanged=\"ChangePage\" />"),
            new(typeof(Primitives.EditingGrid), "支持单元格编辑、列宽和键盘表格操作。", "Columns / Rows 使用 GridColumn / GridRow；CellChanged 接收编辑；Interactions 注入撤销、重做、保存、粘贴等行为；每行单元格数需匹配列数。", "<Primitives.EditingGrid Columns=\"GridColumns\" Rows=\"GridRows\" Interactions=\"GridCommands\" CellChanged=\"CellChanged\" Label=\"编辑项目\" />"),
            new(typeof(Primitives.NavigationGuard), "对未保存内容的离开操作请求确认。", "HasUnsavedChanges 应来自真实草稿状态；Message 提供页面内导航的确认文字。草稿使用 Field / TextBox 绑定，并在每次输入后标记修改；测试导航使用 Underline Button 链接。", "<Primitives.NavigationGuard HasUnsavedChanges=\"Dirty\" Message=\"存在未保存内容，是否离开？\" />"),
            new(typeof(Primitives.InteractionBoundary), "在受控区域暂停编辑交互。", "Locked 控制锁定；Title / Message 解释原因；由页面决定何时锁定。ChildContent 中的编辑器使用 Field / TextBox，命令使用标准 Button，锁定仍由边界统一处理。", "<Primitives.InteractionBoundary Locked=\"Saving\" Title=\"正在保存\"><Message><p>请等待完成。</p></Message><ChildContent><p>受控区域</p></ChildContent></Primitives.InteractionBoundary>")
        ]),
        new("导航与访问场景", "这些入口针对访问页品牌、外壳导航和状态说明的专门场景；按用途选择，不与通用控件互换。",
        [
            new(typeof(Primitives.AccessBrand), "显示入口页的标识与名称。", "ContextName 与 Description 可提供入口说明；TitleId 关联入口标题。", "<Primitives.AccessBrand LogoPath=\"logo.svg\" BrandName=\"应用\" TitleId=\"access-title\" />"),
            new(typeof(Primitives.ShellHeader), "组合品牌、服务菜单和顶部操作。", "Services / LeadingActions / EndActions 提供插槽；IdentityName / Href 提供身份链接。", "<Primitives.ShellHeader HomeHref=\"/\" HomeAriaLabel=\"返回主页\" LogoPath=\"logo.svg\" BrandName=\"应用\" />"),
            new(typeof(Primitives.ServiceMenu), "将目标链接放入顶部下拉菜单。", "Items 使用 ServiceLink；与 ActionMenu 的命令操作不同，项目直接导航。", "<Primitives.ServiceMenu Label=\"页面\" Items=\"ServiceLinks\" />"),
            new(typeof(Primitives.PrimaryNavigationItem), "显示一级导航链接及不可用状态。", "Match 使用 NavLinkMatch；Disabled / DisabledMessage 解释不可用；Id 保持稳定。常规导航从 Program 配置。", "<Primitives.PrimaryNavigationItem Id=\"home\" Label=\"主页\" Icon=\"home\" Href=\"/\" Match=\"NavLinkMatch.All\" />"),
            new(typeof(Primitives.NoticeTrigger), "用正文大小的圆形状态图标，在悬停或聚焦时显示说明。", "Severity 控制红色叉、橙色感叹号、绿色勾或白底蓝色感叹号；Subtle 使用信息图标。ChildContent 提供说明，不应代替必须立即显示的错误。", "<Primitives.NoticeTrigger Severity=\"NotificationSeverity.Warning\">此字段需要检查。</Primitives.NoticeTrigger>"),
        ]),
    ];

    public static int Count => Groups.Sum(group => group.Entries.Count);
}

public sealed record CatalogGroup(string Title, string Description, IReadOnlyList<ComponentEntry> Entries);

public sealed class ComponentEntry(Type componentType, string purpose, string variants, string example)
{
    public Type ComponentType { get; } = componentType;
    public string Name { get; } = DisplayName(componentType);
    public string Namespace { get; } = componentType.Namespace ?? string.Empty;
    public string Purpose { get; } = purpose;
    public Controls.ComponentUsageInfo Usage => Controls.ComponentUsageCatalog.For(ComponentType);
    public bool IsProductionEntry => Usage.Kind is Controls.ComponentUseKind.General or Controls.ComponentUseKind.Scenario;
    public string Variants { get; } = variants;
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
                { "Shape", "Columns", "Rows", "NarrowColumns", "MaxCellSize", "MaxCellHeight", "IconSupport", "Variant", "Centered" };
            parameters.AddRange(OwnParameters(typeof(Controls.UniformGrid)).Where(parameter => names.Contains(parameter.Name))
                .Select(parameter => parameter with
                {
                    Name = $"UniformGrid.{parameter.Name}",
                    Description = $"配置在外层 UniformGrid 上，不能写在 {type.Name} 上。{parameter.Description}"
                }));
        }
        if (type == typeof(Controls.UniformGridButton))
            parameters.AddRange(OwnParameters(typeof(Controls.FormActions)).Where(parameter => parameter.Name == "Columns")
                .Select(parameter => parameter with
                {
                    Name = "FormActions.Columns",
                    Description = "使用 FormActions 替代 UniformGrid 作为表单操作容器时，在 FormActions 上配置列数；不属于按钮参数。"
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
                property.IsDefined(typeof(EditorRequiredAttribute), true), ParameterMeaning.Describe(property), defaults.Describe(property)));
    }

    public bool Matches(string query) => string.IsNullOrWhiteSpace(query)
        || $"{Name} {Namespace} {Purpose} {Variants} {Parameters} {Usage.Kind} {Usage.Scenario} {Usage.Guidance}".Contains(query.Trim(), StringComparison.OrdinalIgnoreCase);

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

public sealed record ComponentParameter(string Name, string Type, bool Required, string Description, string DefaultValue);

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
