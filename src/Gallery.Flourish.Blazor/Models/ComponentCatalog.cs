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
            new(typeof(Controls.Button), "在页面标题、工具区或弹窗中触发操作，也可提供导航链接。", "Filled、Outlined、Danger、Quiet、Underline、Elevated 六种外观；Disabled / Busy 是状态属性。Icon 可选；Text 与 ChildContent 都为空时自动只显示图标，需提供 aria-label。Href 提供导航目标。Primary / Secondary 保留兼容。", "<Button Variant=\"ButtonVariant.Outlined\" Icon=\"save\" Text=\"保存\" OnClick=\"SaveAsync\" Busy=\"Saving\" />"),
            new(typeof(Controls.ExpansionIndicator), "显示展开状态的装饰性三角标记，放在控制区域展开的按钮中。", "与 SelectBox 的原生三角标记保持一致；Expanded 控制展开状态，Design 使用 140ms 旋转动画并支持减少动态效果。按钮负责点击、名称和 aria-expanded / aria-controls；标记本身不可交互。SplitButton、导航和菜单已复用。", "<Button Variant=\"ButtonVariant.Quiet\" OnClick=\"() => Expanded = !Expanded\" aria-expanded=\"@(Expanded ? \"true\" : \"false\")\" aria-controls=\"details\">详情 <ExpansionIndicator Expanded=\"Expanded\" /></Button>\n<div id=\"details\" hidden=\"@(!Expanded)\">详情内容</div>"),
            new(typeof(Controls.SplitButton), "把主动作和次动作呈现为有小间隙的两个按钮；适用于主导航与独立展开操作。", "Href 指定主链接；OnClick / OnSecondaryClick 各自处理动作。Selected 同步两部分的选中外观；Expanded 描述次按钮控制区域的展开状态。", "<SplitButton Text=\"分类\" Href=\"/category\" Expanded=\"Expanded\" OnSecondaryClick=\"() => Expanded = !Expanded\" />"),
            new(typeof(Controls.DisplayBoard), "为控件预览、演示场景和可复制内容提供统一展示区域。", "Dotted 默认 true；Centered 默认 true，控制普通子内容的位置。CodeBlock 始终从左上开始；代码背景板可禁用点阵。CopyText 提供完整复制内容；右上角 Elevated 图标按钮复制成功后暂时显示勾号。", "<DisplayBoard><Button Icon=\"add\" Text=\"创建\" /></DisplayBoard>\n<DisplayBoard Dotted=\"false\" CopyText=\"@Source\" CopyLabel=\"复制代码\"><CodeBlock Text=\"@Source\" Language=\"razor\" /></DisplayBoard>"),
            new(typeof(Controls.UniformGridButton), "为等宽网格提供可点击的单元格，与普通网格项共享外观。", "配合 UniformGrid 或 FormActions 使用。Variant 支持 Filled、Outlined、Danger、Elevated；省略时继承网格外观，独立使用时为 Elevated。表单操作区建议主要操作显式设为 Filled，其余操作设为 Outlined。Title 使用 H1 的 34px，Text 与 ChildContent 使用正文 17px。Icon 可选，有图标默认自动启用上下等分布局，IconSupport=false 保留连续布局；Href、Type、Busy、Disabled 与 OnClick 复用 Button 行为。", "<UniformGrid Shape=\"UniformGridShape.Square\"><UniformGridButton Title=\"创建\" Text=\"演示项目\" Icon=\"add\" Variant=\"UniformGridVariant.Filled\" OnClick=\"Create\" /></UniformGrid>"),
            new(typeof(Controls.ActionMenu), "将多项操作放入可关闭的菜单。", "Actions 使用 MenuAction；每项可 Disabled 或 Destructive；TriggerContent 自定义触发器；OpenOnHover 提供移入打开、移出关闭的弱菜单。", "<ActionMenu Label=\"更多操作\" Actions=\"Actions\" />"),
            new(typeof(Controls.TextBox), "绑定文字并参与表单验证。", "Type 支持原生输入类型；Multiline 与 Rows 切换多行；MaxLength 限制输入长度。", "<Field Label=\"名称\" Id=\"name\"><TextBox Id=\"name\" @bind-Value=\"Draft.Name\" MaxLength=\"120\" /></Field>"),
            new(typeof(Controls.NumberBox<>), "绑定数值并处理解析错误。", "TValue 支持 int、long、short、decimal、double、float 及其可空类型；Min / Max / Step 控制原生输入范围。", "<Field Label=\"金额\" Id=\"amount\"><NumberBox TValue=\"decimal\" Id=\"amount\" @bind-Value=\"Draft.Amount\" Min=\"0\" Step=\"0.01\" /></Field>"),
            new(typeof(Controls.SelectBox<>), "绑定一个选项，包括枚举值。", "Options 使用 SelectOption<TValue>；也可用 ChildContent 提供 option；不可用项由选项的 Disabled 控制。支持可定制原生选择器的浏览器中，下拉面板始终右对齐，并使用共享的三角旋转效果。", "<Field Label=\"状态\" Id=\"state\"><SelectBox TValue=\"RecordState\" Id=\"state\" Options=\"States\" @bind-Value=\"Draft.State\" /></Field>"),
            new(typeof(Controls.CheckBox), "绑定布尔字段并参与表单验证。", "Label 或 ChildContent 提供说明；Disabled 禁用交互。Design 中选中框边长等于单行输入框高度，默认为 48px。", "<CheckBox Label=\"启用\" @bind-Value=\"Draft.Active\" />"),
            new(typeof(Controls.ToggleSwitch), "切换一个布尔状态。", "OnLabel / OffLabel 提供状态文字；Controls 指向受控区域；可独立于 EditForm 使用。", "<ToggleSwitch Label=\"显示详情\" @bind-Value=\"ShowDetails\" Controls=\"details\" />"),
            new(typeof(Controls.ToggleSection), "用开关显示或隐藏一段内容。", "Value 绑定展开状态；隐藏内容仍保留其状态；Disabled 禁用切换。", "<ToggleSection Label=\"高级选项\" @bind-Value=\"Expanded\"><p>选项内容</p></ToggleSection>"),
            new(typeof(Controls.SearchBox), "接收搜索文字并发送查询事件。", "ValueChanged 立即反映输入；防抖后 SearchRequested 接收文字与 CancellationToken，适合可取消的远程查询；SearchChanged 在查询未取消时通知；DebounceMilliseconds 控制延迟。", "<SearchBox Label=\"搜索项目\" @bind-Value=\"Query\" SearchChanged=\"FilterItems\" />"),
            new(typeof(Controls.Field), "关联标签、输入、必填状态与验证错误。", "For 连接表单验证；Error 传入外部错误；FullWidth 占用整行。Id 应与输入相同。", "<Field Label=\"名称\" Id=\"name\" For=\"@(() => Draft.Name)\"><TextBox Id=\"name\" @bind-Value=\"Draft.Name\" /></Field>"),
            new(typeof(Controls.FormLayout), "排列字段并将焦点移到无效输入。", "Columns 指定列数；FocusInvalidFields 可关闭自动聚焦。需要外层 EditForm 负责提交与验证。", "<FormLayout Columns=\"2\"><Field Label=\"名称\" Id=\"name\"><TextBox Id=\"name\" @bind-Value=\"Draft.Name\" /></Field></FormLayout>"),
            new(typeof(Controls.FormActions), "统一表单操作区域的列数与尺寸。", "Columns 指定操作列数；子项使用 UniformGridButton；窄窗口通过主题样式适配。建议主要操作显式使用 Variant=Filled，其余操作使用 Variant=Outlined；按操作语义配置，不依赖子项顺序。", "<FormActions Columns=\"2\"><UniformGridButton Title=\"保存\" Variant=\"UniformGridVariant.Filled\" Type=\"submit\" /><UniformGridButton Title=\"取消\" Variant=\"UniformGridVariant.Outlined\" OnClick=\"Cancel\" /></FormActions>"),
            new(typeof(Controls.UniformGrid), "将多个单元格排列成等宽网格，适合入口目录与信息概览。", "Shape 独立控制单元格形状：默认 Rectangle，Square 为正方形。Variant 控制 Filled、Outlined、Danger 或默认 Elevated 外观，Outline 为 Outlined 别名。单元格保持连贯布局，容器透明，不为未占用位置绘制背景或阴影。MaxCellSize 默认 280px，仅限制正方形边长；MaxCellHeight 默认 260px，仅限制长方形高度。长方形以 2:1 比例按高度上限限制宽度。容器宽度等于实际列宽与分隔间距总和；可用宽度只决定自动列数，窄于一个格子时缩小格子，避免横向滚动。Columns / Rows 可显式指定，NarrowColumns 仅在设置后覆盖窄窗口列数。IconSupport 可配置上下等分布局，null 时有图标自动启用，false 保留连续布局；单格可覆盖该设置。单元格使用 UniformGridItem 或 UniformGridButton，可各自覆盖 Variant；容器自身不会变成方形。Filled=true 保留兼容并选择 Filled 外观。", "<UniformGrid Shape=\"UniformGridShape.Square\" Variant=\"UniformGridVariant.Elevated\" MaxCellSize=\"280\"><UniformGridItem Title=\"项目\" Icon=\"folder\" Text=\"演示概览\" /><UniformGridButton Title=\"创建\" Icon=\"add\" OnClick=\"Create\" /></UniformGrid>"),
            new(typeof(Controls.UniformGridItem), "在等宽网格中展示没有点击行为的信息。", "与 UniformGridButton 共用 Filled、Outlined、Danger、Elevated 外观。Variant 省略时继承网格外观，独立使用时为 Elevated。Title 使用 H1 的 34px，Text 与 ChildContent 使用正文 17px；Icon 可选。没有 Href 或 OnClick。", "<UniformGrid Shape=\"UniformGridShape.Square\"><UniformGridItem Title=\"项目\" Icon=\"folder\" Text=\"12 个演示项目\" Variant=\"UniformGridVariant.Outlined\" /></UniformGrid>"),
            new(typeof(Controls.SectionNavigator), "在正文右侧显示页面板块入口，不占用正文宽度。", "默认跟随 ApplicationShell 中当前页面的 H2 标题。独立使用时指定 ContentId；Items 可显式提供标题与目标 ID。悬浮或聚焦显示标题，当前板块的圆点放大并显示主题色外圈。", "<SectionNavigator ContentId=\"page-content\" Label=\"页面板块\" />"),
            new(typeof(Controls.DataTable<>), "显示带搜索、排序、分页和行操作的数据。", "View 在 Table / Cards 间切换；Columns 使用 TableColumn<TItem>；Actions 使用 RowAction<TItem>。每页默认 10 项，可选择 20 / 50 / 100；PageSizeChanged 支持 @bind-PageSize。Loading、Error、EmptyMessage 显示真实状态。", "<DataTable TItem=\"Project\" Items=\"Projects\" Columns=\"Columns\" Actions=\"RowActions\" @bind-PageSize=\"PageSize\" />"),
            new(typeof(Controls.Dialog), "打开模态对话框并管理焦点与关闭。", "Presentation 支持居中或底部面板；Busy 禁用关闭；CanClose 可异步确认；Actions 提供操作区。", "<Dialog Title=\"确认操作\" @bind-IsOpen=\"Open\"><ChildContent><p>操作说明</p></ChildContent><Actions><Button OnClick=\"Confirm\">确认</Button></Actions></Dialog>"),
            new(typeof(Controls.BottomSheet), "以底部面板呈现模态内容。", "与 Dialog 共用关闭、忙碌和内容接口；@bind-IsOpen 由页面管理状态。", "<BottomSheet Title=\"编辑项目\" @bind-IsOpen=\"Open\"><ChildContent><p>编辑内容</p></ChildContent></BottomSheet>"),
            new(typeof(Controls.Disclosure), "提供原生 details 展开区域。", "InitiallyOpen 指定初始展开；Title 是可访问触发器文字。", "<Disclosure Title=\"调用说明\" InitiallyOpen=\"false\"><p>说明内容</p></Disclosure>"),
            new(typeof(Controls.Notice), "显示操作结果或需要注意的状态。", "Severity 使用 NotificationSeverity；Announce 控制播报；Subtle 提供轻量外观；Title 可增加标题。", "<Notice Severity=\"NotificationSeverity.Success\">保存完成。</Notice>"),
            new(typeof(Controls.EmptyState), "显示没有数据时的真实状态与可选操作。", "Title 描述当前空结果；Actions 提供下一步操作。", "<EmptyState Title=\"没有项目\"><Actions><Button OnClick=\"Create\">创建项目</Button></Actions></EmptyState>"),
            new(typeof(Controls.LoadingState), "显示内容加载状态。", "Message 提供播报内容；ChildContent 可替换文字。", "<LoadingState Message=\"正在加载项目…\" />"),
            new(typeof(Controls.ProgressBar), "显示完整任务的进度条。", "Value 使用 0–100；null 表示未知进度；StatusText 说明当前阶段。", "<ProgressBar Label=\"导入进度\" Value=\"Progress\" StatusText=\"Stage\" />"),
            new(typeof(Controls.ProgressRing), "在紧凑位置显示进度环。", "Value 使用 0–100 或 null；Stopped 暂停旋转；StatusText 为阶段提示。", "<ProgressRing Label=\"任务进度\" Value=\"Progress\" StatusText=\"Stage\" Stopped=\"Paused\" />"),
            new(typeof(Controls.PageBody), "为页面正文提供统一宽度与边距。", "Fluid 省略时继承 Program 的布局配置；true 使用流动宽度，false 使用居中最大宽度；FullWidth 使用完整正文区域。", "<PageBody><PageHeading Title=\"项目\" /><Section Title=\"列表\"><p>页面内容</p></Section></PageBody>"),
            new(typeof(Controls.PageHeading), "显示可收缩页面标题与操作。", "Design 中展开标题为 50px、坍缩标题为 38px；38px 仅用于坍缩大标题。Compact 指定紧凑状态；ParentHref / ParentLabel 显示真正的上级链接；Actions 放置操作。", "<PageHeading Title=\"项目\"><Actions><Button OnClick=\"Create\">创建</Button></Actions></PageHeading>"),
            new(typeof(Controls.Section), "组织有标题的页面内容区。", "Title 使用真实内容标题；Actions 提供区级操作；正文通过 ChildContent 提供。", "<Section Title=\"联系方式\"><p>区块内容</p></Section>"),
            new(typeof(Controls.Card), "承载适合卡片组织的内容。", "Title 可选；内容由应用提供。", "<Card Title=\"当前项目\"><p>项目内容</p></Card>"),
            new(typeof(Controls.IdentityCard), "显示身份主值及关联信息。", "Columns 控制分栏；SideContent 提供侧栏；内容使用 dl / dt / dd 等语义结构。", "<IdentityCard Title=\"项目负责人\"><dl><dt>名称</dt><dd class=\"f-identity-primary\">林晓</dd></dl></IdentityCard>"),
            new(typeof(Controls.Icon), "显示自托管 Material Symbols Outlined 图标。", "Name 接受官方名称及旧名称别名；Class 可调整宿主组合。颜色继承父控件，纯图标操作需要可访问名称。", "<Icon Name=\"home\" />"),
            new(typeof(Controls.CodeBlock), "将代码以文本方式显示。", "Text 自动转义；Language 标识语言，不执行代码。", "<CodeBlock Text=\"ExampleCode\" Language=\"razor\" />")
        ]),
        new("页面组合", "Patterns 命名空间提供较低层的页面组合。通常优先使用 ApplicationLayout；以下组件适用于自定义宿主。",
        [
            new(typeof(Patterns.ContentSurface), "提供固定顶部栏与单一正文滚动区。", "TitleBar / ChildContent 提供区域；DocumentKey 在页面切换时识别正文；ContentId 与 SkipLabel 提供跳过链接。", "<Patterns.ContentSurface DocumentKey=\"@Navigation.Uri\"><TitleBar><p>顶部栏</p></TitleBar><ChildContent><p>正文</p></ChildContent></Patterns.ContentSurface>"),
            new(typeof(Patterns.NavigationSurface), "组合顶部栏、一级与二级导航和正文。", "ShowSecondary 控制二级栏；区域由命名片段提供；DocumentKey 识别页面。一级导航使用 PrimaryNavigationItem，插槽中的页面操作使用标准 Button；Underline 适合轻量操作或链接。", "<Patterns.NavigationSurface ShowSecondary=\"false\"><TitleBar><p>顶部栏</p></TitleBar><PrimaryNavigation><Primitives.PrimaryNavigationItem Id=\"home\" Label=\"主页\" Icon=\"home\" Href=\"/\" /></PrimaryNavigation><ChildContent><p>正文</p></ChildContent></Patterns.NavigationSurface>"),
            new(typeof(Patterns.RecordListPage), "组合列表标题、列表区和反馈区域。", "Actions 提供标题操作；Status 放置真实反馈；SectionTitle 标识列表区。", "<Patterns.RecordListPage Title=\"项目\" SectionTitle=\"项目列表\"><ChildContent><DataTable TItem=\"Project\" Items=\"Projects\" Columns=\"Columns\" /></ChildContent></Patterns.RecordListPage>")
        ]),
        new("高级输入与数据原语", "Primitives 命名空间的较低层 API，用于常用控件不能满足的组合。数据模型与回调由应用定义。",
        [
            new(typeof(Primitives.MaskedInput), "将输入掩码接入 EditForm。", "继承 InputBase<string?>；Mask 指定格式，InputMode 指定原生键盘类型。", "<Primitives.MaskedInput Mask=\"00000-000\" @bind-Value=\"Draft.PostalCode\" aria-label=\"邮政编码\" />"),
            new(typeof(Primitives.StandaloneMaskedInput), "在表单上下文之外绑定掩码输入。", "Value / ValueChanged 提供双向绑定；原生属性通过 AdditionalAttributes 传递。", "<Primitives.StandaloneMaskedInput Mask=\"00000-000\" @bind-Value=\"PostalCode\" aria-label=\"邮政编码\" />"),
            new(typeof(Primitives.SearchAutocomplete<>), "从候选集合搜索并选择结果。", "Value 双向绑定查询文字；ValueSelector / TextSelector 提供值与显示名；ItemSelected 接收选择；MaximumResults 限制候选；FilterItems 可由宿主接管过滤。", "<label for=\"project-search\">选择项目</label>\n<Primitives.SearchAutocomplete TItem=\"Project\" Id=\"project-search\" Items=\"Projects\" @bind-Value=\"ProjectQuery\" ValueSelector=\"ValueOf\" TextSelector=\"NameOf\" ItemSelected=\"SelectProject\" />"),
            new(typeof(Primitives.MultiSelectDropdown<,>), "搜索并选择多项，可选创建新项。", "SelectedValues 保存选择集合；SelectionChanged 返回本次切换的单个 TValue，由宿主增删集合；MaximumSelections 限制数量；CreateRequested 接收创建请求；LabelledBy 指向已有标签。", "<span id=\"projects-label\">选择项目</span>\n<Primitives.MultiSelectDropdown TItem=\"Project\" TValue=\"int\" Id=\"projects\" LabelledBy=\"projects-label\" Items=\"Projects\" SelectedValues=\"SelectedIds\" ValueSelector=\"IdOf\" TextSelector=\"NameOf\" SelectionChanged=\"ToggleProject\" />"),
            new(typeof(Primitives.ReferenceDropdown<>), "搜索并选择一个值类型引用。", "TValue 必须为 struct；SelectedId 可以为 null；AllowNone 控制清除；CreateRequested 可创建引用。", "<span id=\"project-label\">选择项目</span>\n<Primitives.ReferenceDropdown TValue=\"int\" Id=\"project\" LabelledBy=\"project-label\" Items=\"References\" SelectedId=\"SelectedId\" SelectionChanged=\"SelectReference\" />"),
            new(typeof(Primitives.DataTable<>), "提供可配置列、偏好和批量编辑的数据视图。", "Columns 使用 DataColumn<TItem>；Purpose 选择 Registry / Pool / Worklist；CellTemplate 定制单元格；BulkEditCell 与批量回调支持编辑。", "<Primitives.DataTable TItem=\"Project\" Label=\"项目列表\" Items=\"Projects\" Columns=\"DataColumns\" PreferenceKey=\"projects\" />"),
            new(typeof(Primitives.DataSearch<>), "按选择的列发送搜索请求。", "Columns 仅展示 CanSearch 列；Changed 接收 DataSearchRequest；Value 与 ColumnKey 由页面保存。", "<Primitives.DataSearch TItem=\"Project\" Columns=\"DataColumns\" Value=\"Query\" ColumnKey=\"SearchColumn\" Changed=\"SearchChanged\" />"),
            new(typeof(Primitives.DataPager), "控制分页并显示记录范围。", "TotalCount / CurrentPage / PageSize 提供状态；PageChanged 接收页码；IsLimited 标识受限结果。", "<Primitives.DataPager TotalCount=\"Projects.Count\" CurrentPage=\"Page\" PageSize=\"20\" PageChanged=\"ChangePage\" />"),
            new(typeof(Primitives.EditingGrid), "支持单元格编辑、列宽和键盘表格操作。", "Columns / Rows 使用 GridColumn / GridRow；CellChanged 接收编辑；Interactions 注入撤销、重做、保存、粘贴等行为；每行单元格数需匹配列数。", "<Primitives.EditingGrid Columns=\"GridColumns\" Rows=\"GridRows\" Interactions=\"GridCommands\" CellChanged=\"CellChanged\" Label=\"编辑项目\" />"),
            new(typeof(Primitives.NavigationGuard), "对未保存内容的离开操作请求确认。", "HasUnsavedChanges 应来自真实草稿状态；Message 提供页面内导航的确认文字。草稿使用 Field / TextBox 绑定，并在每次输入后标记修改；测试导航使用 Underline Button 链接。", "<Primitives.NavigationGuard HasUnsavedChanges=\"Dirty\" Message=\"存在未保存内容，是否离开？\" />"),
            new(typeof(Primitives.InteractionBoundary), "在受控区域暂停编辑交互。", "Locked 控制锁定；Title / Message 解释原因；由页面决定何时锁定。ChildContent 中的编辑器使用 Field / TextBox，命令使用标准 Button，锁定仍由边界统一处理。", "<Primitives.InteractionBoundary Locked=\"Saving\" Title=\"正在保存\"><Message><p>请等待完成。</p></Message><ChildContent><p>受控区域</p></ChildContent></Primitives.InteractionBoundary>")
        ]),
        new("组合与表现原语", "这些组件保留较低层的内容插槽，常用控件已封装其中部分行为。示例使用 Primitives 别名，以区分同名控件。",
        [
            new(typeof(Primitives.AccessBrand), "显示入口页的标识与名称。", "ContextName 与 Description 可提供入口说明；TitleId 关联入口标题。", "<Primitives.AccessBrand LogoPath=\"logo.svg\" BrandName=\"应用\" TitleId=\"access-title\" />"),
            new(typeof(Primitives.AccessSurface), "承载居中的访问入口内容。", "Emphasized 切换强调表面；页面使用 FormLayout / Field / TextBox / Button 组合字段与提交，原生 form 保留必填与邮箱验证。", "<Primitives.AccessSurface><p>入口内容</p></Primitives.AccessSurface>"),
            new(typeof(Primitives.AppIcon), "提供 Material Symbols 图标与尺寸角色。", "Name 使用官方名称或兼容别名；Size 使用 standard、primary、tool、search、information 等尺寸角色。", "<Primitives.AppIcon Name=\"home\" Size=\"tool\" />"),
            new(typeof(Primitives.Glyph), "显示继承当前颜色的 Material 字体图标。", "Name 使用官方名称或兼容别名；父控件负责状态与可访问名称。", "<Primitives.Glyph Name=\"person\" />"),
            new(typeof(Primitives.ShellHeader), "组合品牌、服务菜单和顶部操作。", "Services / LeadingActions / EndActions 提供插槽；IdentityName / Href 提供身份链接。", "<Primitives.ShellHeader HomeHref=\"/\" HomeAriaLabel=\"返回主页\" LogoPath=\"logo.svg\" BrandName=\"应用\" />"),
            new(typeof(Primitives.ServiceMenu), "将目标链接放入顶部下拉菜单。", "Items 使用 ServiceLink；与 ActionMenu 的命令操作不同，项目直接导航。", "<Primitives.ServiceMenu Label=\"页面\" Items=\"ServiceLinks\" />"),
            new(typeof(Primitives.PrimaryNavigationItem), "显示一级导航链接及不可用状态。", "Match 使用 NavLinkMatch；Disabled / DisabledMessage 解释不可用；Id 保持稳定。常规导航从 Program 配置。", "<Primitives.PrimaryNavigationItem Id=\"home\" Label=\"主页\" Icon=\"home\" Href=\"/\" Match=\"NavLinkMatch.All\" />"),
            new(typeof(Primitives.PageHeading), "显示低层页面标题和操作区。", "Actions 提供标题操作；自动收缩由宿主行为与主题配合。", "<Primitives.PageHeading Title=\"项目\"><Actions><Button OnClick=\"Create\">创建</Button></Actions></Primitives.PageHeading>"),
            new(typeof(Primitives.RecordPageHeading), "显示记录标题及真实上级链接。", "ParentLabel / ParentHref 必需；Compact 控制紧凑标题；Actions 提供操作。", "<Primitives.RecordPageHeading Title=\"项目名称\" ParentLabel=\"全部项目\" ParentHref=\"/projects\" />"),
            new(typeof(Primitives.PageContent), "组合正文与可选页内目录。", "Contents 提供目录中的 li / a；正文从 ChildContent 提供。", "<Primitives.PageContent><Contents><li><a href=\"#details\">详情</a></li></Contents><ChildContent><section id=\"details\"><h2>详情</h2></section></ChildContent></Primitives.PageContent>"),
            new(typeof(Primitives.PageContents), "显示可聚焦的页内目录导航。", "子内容提供指向实际区块的 li / a。", "<Primitives.PageContents><li><a href=\"#details\">详情</a></li></Primitives.PageContents>"),
            new(typeof(Primitives.FormSurface), "承载表单内容与可选目录。", "ShowContents 控制目录显示；Contents 提供页内链接；外层 EditForm 仍由页面负责。", "<Primitives.FormSurface ShowContents=\"false\"><ChildContent><p>表单内容</p></ChildContent></Primitives.FormSurface>"),
            new(typeof(Primitives.FormFields), "排列较低层表单字段。", "Class 与原生属性允许宿主扩展；不会代替 EditForm 的提交与验证。", "<Primitives.FormFields><label>名称<input @bind=\"Draft.Name\" /></label></Primitives.FormFields>"),
            new(typeof(Primitives.FormActionBar), "排列表单操作。", "Columns 指定列数；子内容使用 UniformGridButton。建议主要操作显式使用 Variant=Filled，其余操作使用 Variant=Outlined；按操作语义配置，不依赖子项顺序。", "<Primitives.FormActionBar Columns=\"2\"><UniformGridButton Title=\"保存\" Variant=\"UniformGridVariant.Filled\" Type=\"submit\" /><UniformGridButton Title=\"取消\" Variant=\"UniformGridVariant.Outlined\" OnClick=\"Cancel\" /></Primitives.FormActionBar>"),
            new(typeof(Primitives.FilledIdentityCard), "用数据项显示身份主值与侧栏信息。", "Facts / SideFacts 使用 IdentityFact；IsPrimary 标识主值；Columns 控制分栏。", "<Primitives.FilledIdentityCard Facts=\"IdentityFacts\" SideFacts=\"SideFacts\" Columns=\"true\" />"),
            new(typeof(Primitives.BottomSheet), "提供较低层底部模态面板。", "IsBusy 阻止关闭；OnClose 通知宿主更新 IsOpen；ChildContent 提供可滚动正文，Actions 将操作按钮靠右排列。", "<Primitives.BottomSheet Id=\"edit-sheet\" Title=\"编辑\" IsOpen=\"Open\" OnClose=\"Close\"><ChildContent><p>编辑内容</p></ChildContent><Actions><Button OnClick=\"Close\">关闭</Button></Actions></Primitives.BottomSheet>"),
            new(typeof(Primitives.RowActionMenu), "提供行操作菜单触发器与关闭行为。", "ChildContent 由应用提供操作按钮；Label 提供触发器可访问名称。", "<Primitives.RowActionMenu Label=\"项目操作\"><button type=\"button\" @onclick=\"Edit\">编辑</button></Primitives.RowActionMenu>"),
            new(typeof(Primitives.DisclosureSection), "提供带标签的原生展开区。", "InitiallyOpen 指定初始状态；Label 提供 summary 文本。", "<Primitives.DisclosureSection Label=\"详情\"><p>详情内容</p></Primitives.DisclosureSection>"),
            new(typeof(Primitives.StatusNotice), "按状态语义显示和播报通知。", "Severity 使用 NoticeSeverity；Role 可覆盖默认语义；Announce 控制播报。", "<Primitives.StatusNotice Severity=\"Primitives.NoticeSeverity.Success\">保存完成。</Primitives.StatusNotice>"),
            new(typeof(Primitives.NoticeTrigger), "用正文大小的圆形状态图标，在悬停或聚焦时显示说明。", "Severity 控制红色叉、橙色感叹号、绿色勾或白底蓝色感叹号；Subtle 使用信息图标。ChildContent 提供说明，不应代替必须立即显示的错误。", "<Primitives.NoticeTrigger Severity=\"Primitives.NoticeSeverity.Warning\">此字段需要检查。</Primitives.NoticeTrigger>"),
            new(typeof(Primitives.PageLoading), "标识页面内容加载区域。", "ChildContent 提供真实加载文字；AdditionalAttributes 可提供辅助属性。", "<Primitives.PageLoading>正在加载项目…</Primitives.PageLoading>"),
            new(typeof(Primitives.ToggleSwitch), "提供较低层布尔开关。", "Value / ValueChanged 绑定状态；Controls 关联区域；Label 提供文字。", "<Primitives.ToggleSwitch Label=\"显示详情\" @bind-Value=\"ShowDetails\" />"),
            new(typeof(Primitives.ToggleSection), "保留内容状态的低层切换区域。", "Value 绑定显示状态；Disabled 禁用切换；Label 为触发器文字。", "<Primitives.ToggleSection Label=\"高级选项\" @bind-Value=\"Expanded\"><p>选项内容</p></Primitives.ToggleSection>"),
            new(typeof(Primitives.UniformGrid), "提供等宽子项的低层网格。", "Columns / NarrowColumns 提供常规与窄窗口列数；Filled 使用填充外观。入口目录通常使用 Components.UniformGrid 获得自动换行、形状和外观配置。", "<Primitives.UniformGrid Columns=\"3\" NarrowColumns=\"1\"><p>第一项</p><p>第二项</p></Primitives.UniformGrid>")
        ]),
        new("内部组合基元", "以下公开类型用于库内组合，应用通常不需要直接声明；使用上层控件获得完整交互和可访问语义。",
        [
            new(typeof(Primitives.SelectionDropdownSurface), "为下拉选项提供滚动与布局表面。", "不提供选择、触发器、键盘和关闭逻辑。使用 MultiSelectDropdown 或 ReferenceDropdown。", "<!-- 使用 Primitives.MultiSelectDropdown 或 Primitives.ReferenceDropdown；此表面由控件内部生成。 -->"),
            new(typeof(Primitives.ToggleIndicator), "绘制开关的纯视觉滑块。", "没有绑定或交互参数，且 aria-hidden；使用 ToggleSwitch 或 ToggleSection。", "<!-- 使用 ToggleSwitch 或 ToggleSection；指示器由控件内部生成。 -->")
        ])
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
                { "Shape", "Columns", "Rows", "NarrowColumns", "MaxCellSize", "MaxCellHeight", "IconSupport", "Variant", "Filled" };
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
        || $"{Name} {Namespace} {Purpose} {Variants} {Parameters}".Contains(query.Trim(), StringComparison.OrdinalIgnoreCase);

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
            && parameter.DeclaringType.GetGenericTypeDefinition() == typeof(Controls.DataTable<>))
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
