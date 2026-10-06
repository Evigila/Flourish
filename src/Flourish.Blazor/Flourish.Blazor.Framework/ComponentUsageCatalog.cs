using System.Collections.Frozen;
using Patterns = ArkheideSystem.Flourish.Blazor.Components.Patterns;
using Primitives = ArkheideSystem.Flourish.Blazor.Components.Primitives;

namespace ArkheideSystem.Flourish.Blazor.Components;

/// <summary>Distinguishes general controls, explicit production scenarios and construction helpers.</summary>
public enum ComponentUseKind
{
    /// <summary>A general-purpose production control or layout component.</summary>
    General,
    /// <summary>A production entry point for the explicitly described scenario.</summary>
    Scenario,
    /// <summary>A construction helper whose owner must provide the missing interaction and semantics.</summary>
    BuildingBlock
}

/// <summary>Source-owned usage guidance; this describes actual capabilities, not a second visual specification.</summary>
public sealed record ComponentUsageInfo(
    Type ComponentType,
    ComponentUseKind Kind,
    string Scenario,
    string Guidance,
    Type? PreferredEntry = null);

/// <summary>Explicit usage inventory of exported Framework production entries and construction helpers.</summary>
public static class ComponentUsageCatalog
{
    public static IReadOnlyDictionary<Type, ComponentUsageInfo> Entries { get; } = CreateEntries();

    /// <summary>Looks up the reviewed entry, treating closed generic components as their generic definition.</summary>
    public static ComponentUsageInfo For(Type componentType)
    {
        ArgumentNullException.ThrowIfNull(componentType);
        var definition = componentType.IsGenericType ? componentType.GetGenericTypeDefinition() : componentType;
        return Entries.TryGetValue(definition, out var usage)
            ? usage
            : throw new KeyNotFoundException($"No reviewed component usage entry exists for {componentType.FullName}.");
    }

    private static IReadOnlyDictionary<Type, ComponentUsageInfo> CreateEntries()
    {
        // Deliberately explicit: adding an exported component must also choose its scope and guidance.
        ComponentUsageInfo[] entries =
        [
            General<ActionMenu>("统一命令与记录操作菜单", "生产使用：Actions 使用 MenuAction；ChildContent 使用完整的标准 Button 或原生协议表单动作，两者互斥。统一菜单核心管理可用性、键盘、关闭与异步弹窗调用者；OpenOnHover 仅用于 Actions。服务导航仍使用 ServiceMenu。"),
            Scenario<ApplicationLayout>("业务应用路由布局", "生产使用：作为业务 Router 默认布局，消费 Program 配置并加载框架资源；嵌入示例时关闭 OwnsDocument，展示页使用展示类组合。"),
            Scenario<ApplicationShell>("业务应用外壳", "生产使用：顶部栏、两级导航、内容滚动和页内导航；不是全宽展示页面的横幅布局。通常由 ApplicationLayout 创建。"),
            General<Button>("操作与导航", "生产使用：Variant 只决定外观，Disabled / Busy 决定状态；Href 导航，OnClick 执行操作，纯图标按钮必须有可访问名称。"),
            General<Card>("内容卡片", "生产使用：组织普通内容；不代替页面限宽容器或展示类横幅。"),
            General<CopyText>("可选择的原值文本", "生产使用：编码后展示 Value，统一代码文本外观；不执行文本或复制副作用，完整复制动作使用 DisplayBoard。"),
            General<FormGroup>("成组的原生字段", "生产使用：原生 fieldset / legend 与 FormLayout；Disabled 整体禁用原生字段，宿主保留验证与提交。"),
            General<InlineActions>("紧凑标准动作行", "生产使用：标准 Button 或原生协议内实际控件的换行动作行；只管理统一间距和布局，不发明动作语义。"),
            Scenario<ImagePreview>("只读图片与说明", "生产使用：标准 figure / img / figcaption，Url 与 Alt 由宿主提供；Actions 使用标准操作，不负责上传、认证或图片数据处理。"),
            Scenario<AttributionFooter>("紧凑归属说明", "生产使用：以标准 footer / small 展示宿主提供的版权或归属文字，不包含营销品牌艺术字；营销页使用 PresentationFooter。"),
            Scenario<CheckBox>("绑定模型的布尔输入", "生产使用：使用 @bind-Value 提供 ValueExpression，并可接入 EditForm 验证；原生 POST 或无模型绑定使用 StandaloneCheckBox。"),
            Scenario<CodeBlock>("代码和文本展示", "生产使用：转义并展示文本，不执行代码；可与 DisplayBoard 组合，不是普通业务内容的默认容器。"),
            Scenario(typeof(DataTable<>), "统一记录浏览与业务表格", "生产使用：统一 TableColumn / TableSearchRequest；本地或远端搜索、分页、文化格式、Table / Cards、列显隐/顺序/宽度、作用域排序偏好、原生行操作、Registry / Pool / Worklist 和显式批量编辑。Progressive 仅渐进增强已授权且完整可见的 SSR 文本，不支持隐藏默认列、独立搜索/排序值或编辑。"),
            Scenario(typeof(DataSearch<>), "按列受控搜索", "生产使用：与 DataTable 共用 TableColumn / TableSearchRequest 和标准 Field、选择与文字输入；默认第一个可搜索列，IncludeAllColumns 显式启用空键代表全部列。宿主保留查询和结果；ChildContent 提供同行筛选项，不另建搜索或网格皮肤。"),
            Scenario(typeof(DateBox<>), "绑定模型的日期输入", "生产使用：InputDate 类型解析与 ValueExpression / EditForm 验证；原生日期 POST 可使用 StandaloneTextBox Type=date。"),
            General<Dialog>("通用模态与内容视图", "生产使用：IsOpen 受控打开或 ShowAsync 等待任意结果；CloseAsync 返回结果，关闭/取消/销毁为 null。Busy / CanClose 与焦点共用一个核心；Presentation 包含 Centered / BottomSheet。BrowserControlled 预先渲染，由标准浏览器模块独立控制；Views / View 提供键控内容，不包含确认或重连业务。"),
            General<Disclosure>("原生展开内容", "生产使用：details / summary 展开区；Title 提供触发名称，不用于修改业务布尔字段。"),
            Scenario<DisplayBoard>("控件演示和可复制内容", "生产使用：预览、点阵背景和复制反馈；不是营销 Hero、表单或业务页面的通用皮肤。"),
            BuildingBlock<DropdownSurface>("原生下拉内容表面", "组合基元：仅提供 details / summary 和面板，不包含选中值、列表键盘、外部点击关闭或菜单动作模型；选择用 SelectBox / ReferenceDropdown / MultiSelectBox，命令用 ActionMenu。"),
            General<EmptyState>("真实空结果", "生产使用：没有记录时展示原因与下一步操作；不是加载、失败或无权限状态。"),
            BuildingBlock<ExpansionIndicator>("装饰性展开标记", "组合基元：aria-hidden 的三角标记，没有交互；由 Button、SplitButton 或选择器提供名称、aria-expanded / aria-controls 和事件。"),
            General<Field>("字段标签与验证", "生产使用：关联 Label、Id、必填状态、验证错误与输入；不负责提交、授权或持久化。"),
            Scenario<ValidationMessages>("隐藏和跨字段验证消息", "生产使用：与 Field 共用唯一错误呈现，For 绑定 EditContext 字段、Error 提供显式错误。普通输入使用 Field.For，不重复渲染；隐藏令牌或跨字段错误可独立使用。业务验证、翻译、提交和授权仍由宿主负责。"),
            Scenario<NavigationChoices>("原生 GET 方式选择", "生产使用：Items采用NavigationChoiceItem，ActiveKey由当前路由确定。原生同站点GET链接和保留挂载面板由库生成，当前项使用aria-current；不是ARIA tabs，不接管认证/POST或授权。ChildContent提供业务表单，禁用不代替授权。"),
            Scenario<FilePicker>("交互式文件选择", "生产使用：使用 InputFileChangeEventArgs 获取选择；文件类型、大小、授权及服务端验证由业务负责，Accept 不是安全边界。"),
            Scenario<FormActions>("业务表单操作区", "生产使用：等宽操作区，适合保存、取消和批量业务命令；展示页和登录入口的小型按钮组使用 AccessActions 或普通 Button。"),
            General<FormLayout>("字段布局", "生产使用：字段列数、输入行为和无效字段聚焦；EditForm 或原生 GET / POST 边界仍由宿主负责。"),
            General<Icon>("图标呈现", "生产使用：共享自托管图标来源，颜色继承所属控件；装饰图标不提供独立操作语义。"),
            General<IdentityCard>("身份和记录摘要", "生产使用：通过语义化内容与 SideContent 显示摘要；不是产品展示 Hero 或独立编辑表单。"),
            Scenario(typeof(ListView<>), "静态只读表格展示", "生产使用：按输入顺序完整展示 TableColumn 数据，可有行标题和只读模板；无搜索、排序、分页、选择、行操作或 JavaScript，不适合交互式记录管理。"),
            General<LoadingState>("真实加载反馈", "生产使用：状态播报与加载指示；不要通过它掩盖失败、空数据或权限问题。"),
            General<Notice>("状态与错误通知", "生产使用：NotificationSeverity、Title、Subtle 和播报控制；必须立即可见的错误不能仅放入悬停说明。"),
            Scenario(typeof(NumberBox<>), "绑定模型的数值输入", "生产使用：类型解析、ValueExpression 和可选 EditForm 验证；原生 POST / 原始文本数值输入使用 StandaloneTextBox Type=number。"),
            General<PageBody>("业务页面正文宽度", "生产使用：受控正文宽度与边距；直接子区块遵循页面布局。全宽背景且内容限宽的展示区使用 PresentationBand / ContentContainer。"),
            Scenario<PageHeading>("业务页面和记录标题", "生产使用：可收缩标题、上级链接和操作区；艺术化标题使用 PresentationHero，不以业务大标题强制套用营销版式。"),
            General<ProgressBar>("任务进度", "生产使用：0–100 的确定进度或 null 的未知进度；状态文本说明真实任务阶段。"),
            General<ProgressRing>("紧凑任务进度", "生产使用：紧凑位置的确定或未知进度；不是按钮忙碌状态的替代业务逻辑。"),
            General<SearchBox>("查询文字", "生产使用：防抖与可取消查询回调；候选项选择使用 SearchAutocomplete，按列查询使用 DataSearch。"),
            General<Section>("业务内容章节", "生产使用：标准标题、章节内容和区级操作；全宽背景与艺术排版使用展示组件。"),
            Scenario<SectionNavigator>("业务正文页内导航", "生产使用：跟随指定 ContentId 中的标题或显式 Items；适用于业务内容滚动区，不是展示页顶部导航或营销目录。"),
            Scenario<BackToTop>("正文返回顶部", "生产使用：标准 Elevated 图标按钮与指定 ContentId 的滚动区域协作；保留静态片段链接、减少动态效果和目标焦点，不操作授权或业务导航。"),
            Scenario<LineChart>("只读多序列折线图", "生产使用：传入标签与数值序列，使用标准主题绘制；独立刻度显式启用并显示最大值，精确值有辅助技术表格。图表不计算业务指标或修改数据。"),
            General<MultiSelectBox>("通用多项选择与可选排序", "生产使用：业务选项、表格列和图表序列共享一个多选入口。稳定键、完整选中/顺序快照、上下限、固定与禁用项、搜索及异步创建由同一组件负责；排序明确启用，不持有业务持久化或显示状态。"),
            Scenario(typeof(SelectBox<>), "绑定模型的单值选择", "生产使用：SelectOption 和 ValueExpression / 可选 EditForm 验证；原生 POST 使用 StandaloneSelectBox，带引用搜索创建使用 ReferenceDropdown。"),
            General<SplitButton>("双动作入口", "生产使用：主动作与次动作独立；MenuContent 复用原生 details 下拉，静态 SSR 可用且次操作不提交表单。未设置插槽时保留宿主回调和受控展开。FullWidth 或 Width 调整总宽，次按钮始终 48px 方形；Busy/Disabled 阻止重复动作。"),
            Scenario<StandaloneCheckBox>("原生 POST 或无验证模型的布尔输入", "生产使用：Value / ValueChanged、原生属性及提交值；不自动注册 EditContext 验证，绑定模型验证使用 CheckBox。"),
            Scenario(typeof(StandaloneSelectBox<>), "原生 POST 或无验证模型的单值选择", "生产使用：选项、Value / ValueChanged 和原生属性；不自动注册 EditContext 验证，验证模型选择使用 SelectBox。"),
            Scenario<StandaloneTextBox>("原生 POST 或无验证模型的文字输入", "生产使用：原生 name / form / autocomplete 等属性与 Value / ValueChanged；服务器承担验证，EditContext 验证使用 TextBox。"),
            Scenario<TextBox>("绑定模型的文字输入", "生产使用：ValueExpression、类型输入和可选 EditForm 验证；原生 POST 或无验证模型场景使用 StandaloneTextBox。"),
            General<ToggleSection>("布尔状态控制的内容区", "生产使用：与 ToggleSwitch 共用布尔绑定，隐藏内容保持挂载；原生 disclosure 仅控制展开，不表示业务开关值。"),
            General<ToggleSwitch>("布尔开关", "生产使用：Value / ValueChanged、状态文字和受控区域；外观变化不应引入另一套开关状态 API。"),
            Scenario<UniformGrid>("等形状展示单元格", "生产使用：配合 UniformGridItem / UniformGridButton 的形状、尺寸上限、自动列数和外观；只由库管理网格单元的统一布局与交互。"),
            Scenario<UniformGridButton>("可操作的网格单元格", "生产使用：表单操作和多步向导的有限选项均使用此控件；纯选择步骤只保留 UniformGrid 和按钮文字，不加独立标题、说明或下拉框。长方形默认铺满行并按列均分；正方形仍限制等量宽高。继承 UniformGrid 布局和外观，复用 Button 行为；不是导航栏或登录表单普通按钮的替代。"),
            Scenario<UniformGridItem>("只读网格单元格", "生产使用：与 UniformGridButton 共享内容结构和样式，不具有 Href / OnClick；有操作需求必须使用真正按钮。"),

            Scenario<Patterns.ContentSurface>("自定义内容宿主", "生产使用：顶部栏与正文组合，DocumentFlow / Footer 支持文档流展示页；滚动业务区和全宽展示页按明确布局模式选择，不使用宿主 CSS 覆盖另一种模式。"),
            Scenario<Patterns.NavigationSurface>("自定义业务导航宿主", "生产使用：两级导航与单一业务滚动区；标准应用通常使用 ApplicationLayout，展示页面使用文档流内容宿主。"),

            Scenario<Primitives.AccessBrand>("访问入口品牌", "生产使用：登录、注册和访问入口的标识与说明；TitleId 必须在页面中唯一，不用于营销艺术标题。"),
            Scenario<Primitives.DataPager>("受控加载结果分页", "生产使用：宿主管理 CurrentPage / PageSize，PageChanged 请求页码；IsLimited 表示仅已加载结果，不承诺服务端总量或自行查询数据。"),
            Scenario<Primitives.EditingGrid>("电子表格编辑", "生产使用：GridColumn / GridRow / GridCell、键盘、剪贴板及主机编辑命令；不是只读列表或普通 DataTable 的外观变种。宿主负责校验、保存和授权。"),
            Scenario<Primitives.InteractionBoundary>("临时交互锁定", "生产使用：宿主提供真实锁定原因，inert 暂停编辑；不是身份验证或授权边界，不能代替服务器校验。"),
            Scenario<Primitives.MaskedInput>("绑定模型的掩码输入", "生产使用：InputBase、ValueExpression 和共享掩码格式化；原生 POST / 无验证模型场景使用 StandaloneMaskedInput，不是普通 TextBox 的纯外观变种。"),
            Scenario<Primitives.NavigationGuard>("未保存草稿离开确认", "生产使用：HasUnsavedChanges 来自真实草稿状态；交互导航和浏览器离开确认，不是安全、授权或防丢失持久化保证，纯 SSR 无交互保护。"),
            Scenario<Primitives.NoticeTrigger>("次要状态说明", "生产使用：焦点和悬停可读的状态图标说明；只用于补充信息，必须立即显示的错误或重要警告使用 Notice。"),
            Scenario<Primitives.PrimaryNavigationItem>("业务外壳一级导航", "生产使用：图标导航轨、当前状态和不可用说明；常规应用从 Program 配置，不用于营销顶部链接或普通动作按钮。"),
            Scenario(typeof(Primitives.ReferenceDropdown<>), "可搜索的单值引用", "生产使用：值类型标识、可空选择、未收录名称和可选创建；不是普通 SelectBox 的纯外观变种，也不使用多值集合 API。"),
            Scenario(typeof(Primitives.SearchAutocomplete<>), "搜索候选项选择", "生产使用：查询文字、候选键、键盘活动项和 ItemSelected；远程候选由宿主提供，不等价于 SearchBox 或引用标识选择。"),
            Scenario<Primitives.SecondaryNavigationItem>("业务外壳二级导航", "生产使用：路径匹配与当前页面链接；常规导航从 Program 配置，不是普通内容链接或营销 CTA。"),
            Scenario<Primitives.ServiceMenu>("顶部服务导航", "生产使用：ServiceLink 导航目标；不是执行命令的 ActionMenu，也不负责用户授权或会话切换。"),
            Scenario<Primitives.ShellHeader>("品牌与身份顶部栏", "生产使用：品牌、服务、身份和操作插槽；可作为明确页面宿主的顶部栏，不独立管理整个页面滚动和导航。"),
            Scenario<Primitives.StandaloneMaskedInput>("原生 POST 或无验证模型的掩码输入", "生产使用：Value / ValueChanged 与原生属性；不自动注册 EditContext 验证，模型验证使用 MaskedInput。"),

            General<ContentContainer>("中心限宽内容", "生产使用：独立控制内容限宽与水平居中，适用于业务和展示页；背景全宽由 PresentationBand 管理。"),
            Scenario<PresentationBand>("全宽展示横幅", "生产使用：默认最小高度 800px，长内容自然增长；全宽背景与内部 ContentContainer 分层。Dotted 默认关闭；Tone 使用标准颜色角色，推荐相邻横幅交替颜色。不通过消费端 CSS 覆盖宽度。"),
            Scenario<PresentationHero>("产品展示艺术标题", "生产使用：展示首页的艺术标题、副标题和标准操作；业务记录、管理标题使用 PageHeading，不将艺术字号应用于业务正文。"),
            Scenario<LogoDisplayer>("访问和展示页项目标识", "生产使用：上方 Logo、下方项目艺术字，默认读取 ConfigureProject 的名称/标志；项目名支持作用域本地化和实例覆盖。HeadingLevel 选择真实标题语义。不是业务壳顶部品牌或认证控件。"),
            Scenario<PresentationFooter>("展示网站品牌页脚", "生产使用：全宽页脚、中心内容与装饰水印；标题和水印统一跟随 ConfigureProject 的项目名及其本地化。Copyright 由宿主提供，不复制到业务导航或账户管理面板。"),
            Scenario<OfferStage>("产品报价展示", "生产使用：展示报价的响应式舞台与选中呈现；价格、许可能力及购买动作由宿主提供，不管理真实订单或许可授权。"),
            Scenario<OfferCard>("产品报价卡片", "生产使用：报价名称、说明和标准动作；与 OfferStage 组合，业务实体摘要使用 IdentityCard / Card。"),
            Scenario<AccessPanel>("访问入口内容区", "生产使用：已存在页面宿主中的居中访问面板，不生成第二个 main；配合 AccessFormSurface 与 AccessActions，认证仍由宿主负责。"),
            Scenario<AccessFormSurface>("访问入口表单内容", "生产使用：登录、注册和恢复入口的标准表单表面；只组织内容，不创建认证状态或改写原生 GET / POST 协议。"),
            Scenario<AccessActions>("入口和展示页小型动作组", "生产使用：排列标准 Button，不把登录或营销 CTA 强制渲染为大型表单网格单元格；业务等宽操作使用 FormActions。")
        ];
        return entries.ToFrozenDictionary(entry => entry.ComponentType);
    }

    private static ComponentUsageInfo General<TComponent>(string scenario, string guidance, Type? preferredEntry = null)
        => new(typeof(TComponent), ComponentUseKind.General, scenario, guidance, preferredEntry);

    private static ComponentUsageInfo Scenario<TComponent>(string scenario, string guidance)
        => Scenario(typeof(TComponent), scenario, guidance);

    private static ComponentUsageInfo Scenario(Type componentType, string scenario, string guidance)
        => new(componentType, ComponentUseKind.Scenario, scenario, guidance);

    private static ComponentUsageInfo BuildingBlock<TComponent>(string scenario, string guidance, Type? preferredEntry = null)
        => new(typeof(TComponent), ComponentUseKind.BuildingBlock, scenario, guidance, preferredEntry);
}
