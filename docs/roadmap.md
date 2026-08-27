# WPF / WinUI 3 迁移路线图

> 文档状态：迁移前基线
> 基线日期：2026-08-29
> 当前实现：`src/Flourish` 中的 WPF 单体程序集
> 目标：在不破坏 WPF 版本的前提下，逐步提取平台无关 Core，并以 WinUI 3 原生能力优先的方式还原和扩展全部功能。

## 1. 文档用途

本文是持续维护的功能总账，不是一次性的迁移说明。实现、替换或放弃任何功能时，都应更新对应原子项的状态、决策和验收结果；新增功能也应先登记再实现。

当前基线约有 296 个手写 C# 文件、51 个 XAML、32 个公开视觉控件、47 个 Gallery 页面、93 个测试文件和 23 篇中文功能文章。现有文档没有覆盖全部控件，因此清单同时以源码、Gallery 和测试为依据。

状态：

- `[ ]`：未开始。
- `[~]`：进行中。
- `[x]`：已完成并通过自动化及手动验收。
- `[!]`：被外部依赖、技术限制或产品决策阻塞。
- `[-]`：经记录决策后不迁移。

UI 实现类型：

- **框架直用**：WinUI 3 / Windows App SDK 已提供完整语义，最多增加资源和样式，不复制其行为。
- **原生组合**：组合多个原生控件或 API，只实现产品特有的状态同步与编排。
- **薄适配**：用小型平台适配器隔离 WindowId、HWND、Win32、凭据库等平台能力。
- **自定义**：没有满足要求的原生等价物，且现有功能确属产品差异点。
- **待选型**：没有可靠的第一方等价物，必须先做技术验证和决策记录。

归属只有两类：

- **Core**：平台无关的契约、状态、领域规则、注册表、历史、持久化模型、纯算法与可测试编排。
- **UI**：所有 XAML、控件、窗口生命周期、视觉资源、输入、动画及操作系统集成。这里也包含文件选择器、托盘、凭据库等平台适配器。

一个现有模块可以被拆成多个原子项，例如“导航历史”属于 Core，“页面实例工厂”属于 UI。不得把混合模块整体强塞进任一层。

## 2. 已确定的架构方向

### 2.1 单一产品，多项目实现

保持一个 Flourish 产品和一套功能语义，但不继续维持单一程序集：

```text
Flourish.Core
├── Flourish.Wpf
│   └── Gallery.Wpf
└── Flourish.WinUI3
    └── Gallery.WinUI3
```

- `Flourish.Core` 初始目标框架使用 `net10.0`，从编译层面阻止 Windows UI 类型进入。
- `Flourish.Wpf` 使用 `net10.0-windows` 和 `UseWPF`。
- `Flourish.WinUI3` 使用与所选稳定版 Windows App SDK 相匹配的 Windows TFM 和 `UseWinUI`。
- NuGet 包名使用 `Arkheide.Flourish.Core`、`Arkheide.Flourish.Wpf`、`Arkheide.Flourish.WinUI3`。
- 命名空间继续使用 `ArkheideSystem.Flourish...`；类型名直接表达语义，不添加项目或品牌前缀。
- 当前 `Arkheide.Flourish` 可在过渡期继续发布为 WPF 兼容包或元包；移除时机另行记录。

不建议在同一程序集同时启用 WPF 与 WinUI。两套 XAML 编译链、`Application` / `Window` / `Page` 类型、资源字典和线程模型并不兼容，这会让所谓“共享”退化为条件编译和平台泄漏。

### 2.2 保持同一仓库

Core、WPF、WinUI 3、示例、文档和契约测试应留在同一仓库：

- 一次提交可同时修改共享契约及两端适配器。
- 可以用同一套契约测试验证行为一致性。
- Issue、版本和破坏性变更只需维护一份。
- 只有在维护团队、发布节奏或产品定位真正独立后，才重新评估拆仓。

### 2.3 WinUI 3 原生优先

迁移目标是保持产品能力和公共语义，不是逐像素复制 WPF 内部实现。优先级固定为：

1. 直接使用 WinUI 3 / Windows App SDK 的控件与平台行为。
2. 用原生控件组合出 Shell，并保留 Core 中的状态与策略。
3. 通过薄适配器处理 HWND、WindowId、通知区域等平台边界。
4. 仅为没有原生等价物或确属差异化能力的部分编写自定义控件。

特别约束：

- 标题栏保留系统最小化、最大化、关闭按钮、Snap Layout 和系统菜单，不手绘系统 caption buttons。
- 原生 `Button`、`CheckBox`、`ComboBox`、`PasswordBox`、`RadioButton`、`TextBox`、`ToolTip` 等只做样式和必要附加属性。
- 不把 WPF 控件模板逐行翻译成 WinUI XAML。
- WPF 与 WinUI 的 XAML 资源不共享；共享的是颜色、字号、间距、圆角等语义 Token 和行为契约。

## 3. Core 边界硬规则

Core 项目不得引用：

- `System.Windows`、`System.Windows.Forms`、`Microsoft.UI.Xaml`、`Windows.UI.Xaml`。
- `Application`、`Window`、`Page`、`FrameworkElement`、`DependencyObject`、`ResourceDictionary`。
- WPF `KeyGesture`、`Color`、`Rect`、`Size`、`WindowState`、`ResizeMode`。
- WindowId、HWND、Win32 句柄、Dispatcher、Composition 和 XAML 资源键。
- 以 `object`、条件编译或别名隐藏的平台对象。

现有 `Abstract` 目录不能直接整体移动到 Core。目前至少有 22 个公开契约文件直接引用 WPF 类型：

| 泄漏类别 | 现有位置 | Core 目标表达 |
| --- | --- | --- |
| 应用生命周期 | `IApplicationBuilder`、`IApplicationRuntime` | Core 保留 Host 配置；WPF/WinUI 各自提供应用启动门面 |
| 页面约束与工厂 | `INavigation*`、`NavigationRoute`、`NavigatedEventArgs` | Core 使用 route key、view key、缓存策略和纯数据事件；UI 注册页面类型及工厂 |
| 自定义视觉内容 | `ICustomContentBuilder`、`IShellRegionService` | UI 保存元素工厂；Core 只保存区域 ID、顺序和启用状态 |
| 快捷键 | `IShortcutService`、`ShortcutRegistrationInfo` | Core 使用 `ShortcutChord` 与平台无关修饰键；UI 做按键映射 |
| 对话框与 owner | `IMessageService` | Core 使用中立请求/结果；UI 负责 XamlRoot、WindowId 和呈现 |
| 页面字体覆盖 | `IFontBuilder`、`IFontService` | 以 route/view key 标识覆盖，不约束 WPF `Page` |
| 主题颜色 | `ThemeColors` | 使用平台无关 `ArgbColor`，UI 转换为各自颜色类型 |
| 窗口快照 | `WindowStateSnapshot`、`IWindowBuilder`、`IWindowService` | 使用中立 bounds、size、state、resize policy；UI 转换为平台 API |
| Toolbar 页面映射 | `IToolbarBuilder`、`IToolbarService` | 使用 route/view key，不使用 WPF 页面泛型约束 |
| Profile 内容页 | `ITitleBarBuilder`、`IProfileFlyoutService` | Core 保存 content key；UI 解析为页面或 DataTemplate |
| 导航 DI 扩展 | `Hosting/ServiceCollectionExtensions.cs` | Core 注册路由元数据；平台包提供强类型 Page 扩展 |

Core 完成的最低门槛：

- 单独构建不需要 Windows Desktop SDK。
- Core 源码中检索 `System.Windows`、`Microsoft.UI.Xaml`、`Windows.UI.Xaml` 的结果为空。
- 所有状态模型可在无 UI 线程的测试中创建和变更。
- WPF 与 WinUI 只依赖 Core，Core 不反向依赖任一 UI 项目。

## 4. 实施阶段门

| 阶段 | 目标 | 进入下一阶段的门槛 |
| --- | --- | --- |
| R0 基线冻结 | 登记功能、测试和视觉基线 | 本文覆盖现有 Builder、Service、Shell、控件、Gallery 和测试 |
| R1 契约中立化 | 定义平台无关值对象和接口 | 公共 Core API 不含 WPF 类型；兼容层方案已记录 |
| R2 提取 Core | 移动纯逻辑并拆分测试 | Core 独立构建；WPF 行为和现有测试无回归 |
| R3 WinUI 垂直切片 | App、Window、Host、主题、标题栏、路由和一个页面 | 尽早验证 Window/Dialog/Route 的契约边界 |
| R4 Shell 还原 | 导航、内容、工具栏、状态栏、浮层 | 关键应用流程在 WinUI Gallery 走通 |
| R5 控件还原 | 先原生样式，后组合和自定义控件 | 每个控件有 Gallery、状态和无障碍验收 |
| R6 平台能力 | Picker、凭据、托盘、窗口细节 | 打包/非打包、Win10/Win11 降级路径明确 |
| R7 风险项 | DataGrid、右侧可调导航、复杂动效 | 技术验证通过并形成决策记录 |
| R8 发布 | 文档、包、迁移指南、版本策略 | 两平台包可独立引用，自动化与手动验收通过 |

## 5. 架构、工程与公共契约

当前依据：`Flourish.slnx`、`src/Flourish/Flourish.csproj`、`src/Flourish/Abstract`、`src/Flourish/Hosting`。

| 状态 | ID | 原子模块 | 归属 | WinUI 3 实现 / 原生性 | 验收点 |
| --- | --- | --- | --- | --- | --- |
| [ ] | A01 | Core 项目与目标框架 | Core | 不适用 | 可独立构建且无 Windows Desktop SDK |
| [ ] | A02 | WPF 平台项目 | UI | WPF 保持现有实现 | 现有应用只需受控的包名/API 迁移 |
| [ ] | A03 | WinUI 3 平台项目 | UI | Windows App SDK 原生项目模板 | 支持目标架构并能创建空窗口 |
| [ ] | A04 | WPF Gallery 拆分 | UI | 保留现有 Gallery | 全部现有示例仍可运行 |
| [ ] | A05 | WinUI Gallery | UI | WinUI 原生应用 | 每个迁移功能都有可发现入口 |
| [ ] | A06 | Core 测试项目 | Core | 不适用 | 测试不需要 STA、窗口或 XAML |
| [ ] | A07 | WPF 契约/UI 测试 | UI | 保留 WPF 测试基础设施 | 原有契约保持通过 |
| [ ] | A08 | WinUI 契约/UI 测试 | UI | WinUI/XAML 编译与组件测试 | 覆盖公开控件、资源和平台适配器 |
| [ ] | A09 | 包依赖方向 | Core | 不适用 | 无 WPF↔WinUI、Core→UI 引用 |
| [ ] | A10 | 公共 API 兼容策略 | Core | 不适用 | 每个破坏性变更有 shim 或主版本说明 |
| [ ] | A11 | 语义命名规范 | Core | 不适用 | 新类型不带项目/品牌前缀 |
| [ ] | A12 | NuGet 包元数据 | UI | SDK 打包能力 | 三个包的依赖、TFM、README 和符号正确 |
| [ ] | A13 | Windows App SDK 基线 | UI | 实施前固定稳定版及最低版 | 记录 Windows、SDK、.NET、打包模式 |
| [ ] | A14 | 功能开关与能力检测 | Core | UI 注入平台能力快照 | 缺失 API 时可预测降级 |
| [ ] | A15 | UI 调度端口 | UI | WPF Dispatcher / WinUI DispatcherQueue | Core 不捕获 UI SynchronizationContext |
| [ ] | A16 | 日志与诊断 | Core | Logging 抽象，UI 记录平台细节 | 失败含模块 ID、平台和降级原因 |
| [ ] | A17 | 稳定 XAML URI 兼容 | UI | WPF 保持现有 XmlnsDefinition；WinUI 独立 URI 决策 | 拆包不静默破坏消费者 XAML |
| [ ] | A18 | 外部 Extension 依赖 | Core | 决定包引用或同仓协调 | Solution/CI 不依赖未声明兄弟仓库状态 |

## 6. Hosting、生命周期与依赖注入

当前依据：`Hosting/ApplicationBuilder.cs`、`DefaultApplicationBuilder.cs`、`HostedApplicationRuntime.cs`、`ApplicationCompositionRoot.cs`、`ServiceCollectionExtensions.cs`。

| 状态 | ID | 原子模块 | 归属 | WinUI 3 实现 / 原生性 | 验收点 |
| --- | --- | --- | --- | --- | --- |
| [ ] | H01 | 默认 Host 配置管线 | Core | `Microsoft.Extensions.Hosting` | appsettings、环境变量、命令行顺序一致 |
| [ ] | H02 | `ConfigureConfiguration` | Core | 不适用 | 两平台得到同一 IConfiguration |
| [ ] | H03 | `ConfigureServices` | Core | 不适用 | 用户服务注册与生命周期一致 |
| [ ] | H04 | 功能 Builder 草稿 | Core | 不适用 | Build 前可配置，Build 后拒绝修改 |
| [ ] | H05 | Builder 校验聚合 | Core | 不适用 | 报告前置条件、重复键和非法值 |
| [ ] | H06 | Core 服务注册 | Core | 不适用 | 注册表、状态服务、存储只注册一次 |
| [ ] | H07 | 平台服务注册 | UI | 各 UI 项目拥有 composition root | 同一 Core 契约解析到正确平台实现 |
| [ ] | H08 | WPF Application 启动 | UI | 保留 WPF Application/Startup | 原有 `Run<TApplication>` 不回归 |
| [ ] | H09 | WinUI Application 启动 | UI | 原生 `Microsoft.UI.Xaml.Application` | OnLaunched 完成 Host/主窗口连接 |
| [ ] | H10 | 主窗口创建与显示 | UI | 原生 `Window` / `AppWindow` | Host 就绪后只创建一次 |
| [ ] | H11 | Host Start/Stop 顺序 | Core | UI 触发生命周期 | HostedService 顺序明确且可重入保护 |
| [ ] | H12 | 启动失败回滚 | Core | UI 回报平台异常 | 不留下半初始化服务或窗口 |
| [ ] | H13 | Dispose 顺序 | Core | 不适用 | 注册、任务和 Host 按逆序释放 |
| [ ] | H14 | 页面 DI 注册 | UI | 平台 Page 工厂 + DI | transient/singleton/cache 语义明确 |
| [ ] | H15 | 命令解析器 DI 扩展 | Core | 不适用 | 多解析器按注册顺序启动和逆序释放 |
| [ ] | H16 | Options 按模块拆分 | Core | 平台 Options 由各 UI 包贡献 | 移除单个 ApplicationOptions 对所有模块的耦合 |

## 7. 配置、数据、本地化与偏好

当前依据：`Configuration`、`Localization`、`PreferenceLoader.cs`、`PreferencePersistenceService.cs`、`Assets/FlourishCulture.Json`。

| 状态 | ID | 原子模块 | 归属 | WinUI 3 实现 / 原生性 | 验收点 |
| --- | --- | --- | --- | --- | --- |
| [ ] | D01 | 应用数据路径解析 | Core | BCL 文件系统 API | 相对路径、绝对路径和空路径规则正确 |
| [ ] | D02 | appsettings 文件路径 | Core | 不适用 | 自定义与默认路径行为一致 |
| [ ] | D03 | 定向 JSON 配置源 | Core | 不适用 | Flourish 根/完整文件模式与优先级正确 |
| [ ] | D04 | 非对象根与重复根校验 | Core | 不适用 | 坏结构不产生部分配置 |
| [ ] | D05 | 外部文件监视/恢复 | Core | 不适用 | 外部修改后定向 reload |
| [ ] | D06 | User Secrets | Core | 不适用 | 仅在合适环境加载 |
| [ ] | D07 | 可写设置事务 | Core | 不适用 | Set/Remove/Merge/Append 一次原子写 |
| [ ] | D08 | 设置根边界 | Core | 不适用 | 只允许拥有的配置根 |
| [ ] | D09 | 设置并发 | Core | 不适用 | 串行更新，排队事务可取消 |
| [ ] | D10 | 禁止事务重入 | Core | 不适用 | 嵌套更新可预测失败 |
| [ ] | D11 | 无变化写入 | Core | 不适用 | 无变化不重写文件 |
| [ ] | D12 | 损坏 JSON 防护 | Core | 不适用 | 坏文件不被覆盖且有诊断 |
| [ ] | D13 | 配置重载通知 | Core | 不适用 | 写入后 IConfiguration 立即可见 |
| [ ] | D14 | 偏好键目录 | Core | 不适用 | 每个功能的键、类型、默认值登记 |
| [ ] | D15 | 偏好加载编排 | Core | 模块 `ISettingsContributor` | 中央服务不直接依赖全部 UI Service |
| [ ] | D16 | 偏好批量持久化 | Core | UI 提交平台状态 DTO | 快速变化合并为最新值 |
| [ ] | D17 | 偏好版本迁移 | Core | 不适用 | 旧键迁移可重复执行 |
| [ ] | D18 | Locale 初始选择 | Core | 不适用 | 配置、持久、系统、默认优先级明确 |
| [ ] | D19 | 内置文化资源 | Core | 不适用 | en-US/zh-CN 与键名回退完整 |
| [ ] | D20 | 外部文化文件注册 | Core | 不适用 | 注册、覆盖、Reload、Dispose 正确 |
| [ ] | D21 | 可用语言快照 | Core | 不适用 | 文件变化后版本和事件正确 |
| [ ] | D22 | 运行时语言切换 | Core | UI 订阅变化 | 当前文化格式化正确 |
| [ ] | D23 | UI 文本刷新 | UI | 静态资源原生；动态切换用绑定/适配 | 已打开 Shell 与页面同步刷新 |
| [ ] | D24 | RTL/FlowDirection | UI | 原生 `FlowDirection` | RTL 不改变业务方向语义 |
| [ ] | D25 | 缺失翻译诊断 | Core | 不适用 | 可定位来源并稳定回退 |
| [ ] | D26 | 文化资源打包 | UI | EmbeddedResource/PRI | packaged/unpackaged 均可加载 |
| [ ] | D27 | 平台应用数据目录 | UI | Windows App SDK / Win32 薄适配 | 两种打包模式路径一致可诊断 |

## 8. 命令、快捷键与注册生命周期

当前依据：`Commands`、`Command*`、`IShortcutService`、`IRegistration`。

| 状态 | ID | 原子模块 | 归属 | WinUI 3 实现 / 原生性 | 验收点 |
| --- | --- | --- | --- | --- | --- |
| [ ] | C01 | 命令键规则 | Core | 不适用 | 空白、重复和大小写规则固定 |
| [ ] | C02 | Reject/Replace/Append | Core | 不适用 | 三种重复策略均可测试 |
| [ ] | C03 | 优先级与注册顺序 | Core | 不适用 | 同优先级顺序稳定 |
| [ ] | C04 | 命令执行委托 | Core | 不适用 | 同步完成与 ValueTask 正确 |
| [ ] | C05 | CanExecute | Core | UI 只投影 Enabled | 失效通知到全部观察者 |
| [ ] | C06 | CommandContext | Core | 不适用 | 参数、来源、服务、取消完整 |
| [ ] | C07 | NotHandled 链 | Core | 不适用 | 处理器链短路规则正确 |
| [ ] | C08 | 调度结果 | Core | 不适用 | Disabled/Failed/Canceled/NotFound 可区分 |
| [ ] | C09 | 异常隔离 | Core | 不适用 | 处理器异常形成结果并记录 |
| [ ] | C10 | Parser HostedService | Core | 不适用 | 失败完整回滚，Registrar 及时失效 |
| [ ] | C11 | 注册租约 | Core | 不适用 | Dispose 幂等，旧租约不删替代注册 |
| [ ] | C12 | `ShortcutChord` 值对象 | Core | 不使用 KeyGesture/VirtualKey | key/modifiers 可序列化 |
| [ ] | C13 | 快捷键作用域 | Core | 不适用 | Page→Window→Application 优先级准确 |
| [ ] | C14 | 快捷键冲突策略 | Core | 不适用 | 精确 scope 与 fallback 可测试 |
| [ ] | C15 | 文本输入 opt-in | Core | UI 提供焦点上下文 | 默认不截获编辑快捷键 |
| [ ] | C16 | WPF 按键映射 | UI | WPF KeyGesture/InputBinding | 现有快捷键无回归 |
| [ ] | C17 | WinUI 按键映射 | UI | **框架直用** `KeyboardAccelerator` | 常见组合键不依赖全局 KeyDown |
| [ ] | C18 | 特殊按键路径 | UI | 键盘事件 + InputKeyboardSource | AltGr、纯修饰键、系统键正确 |
| [ ] | C19 | 快捷键可发现性 | UI | Accelerator tooltip/AccessKey | 菜单、按钮和 UIA 显示组合键 |
| [ ] | C20 | Command 与 UI 连接 | UI | ICommand/Click/Invoked 薄适配 | Toolbar、导航、按钮共享 command key |

## 9. 后台任务

当前依据：`BackgroundTasks`、`IBackgroundTaskService`、`BackgroundTask*`。

| 状态 | ID | 原子模块 | 归属 | WinUI 3 实现 / 原生性 | 验收点 |
| --- | --- | --- | --- | --- | --- |
| [ ] | B01 | 任务元数据 | Core | 不适用 | ID、名称、描述、可取消、显示策略稳定 |
| [ ] | B02 | 任务状态机 | Core | 不适用 | Queued/Running/Cancelling/终态转换合法 |
| [ ] | B03 | 有界并发 | Core | 不适用 | 上限和公平排队可测试 |
| [ ] | B04 | 等待队列 | Core | 不适用 | 取消排队项不占执行槽 |
| [ ] | B05 | CancellationToken 联动 | Core | 不适用 | 用户、Host、内部取消可区分 |
| [ ] | B06 | 进度报告 | Core | UI 订阅快照 | 已知/未知进度均有定义 |
| [ ] | B07 | 时间戳 | Core | 不适用 | 排队、开始、结束时间一致 |
| [ ] | B08 | 无返回值结果 | Core | 不适用 | 完成、取消、异常完整 |
| [ ] | B09 | 泛型返回值 | Core | 不适用 | 结果类型与异常传播正确 |
| [ ] | B10 | 活动任务快照 | Core | 不适用 | 不可变列表、版本和事件原子 |
| [ ] | B11 | 任务句柄 | Core | 不适用 | Cancel、Completion 与泛型结果一致 |
| [ ] | B12 | Host 停止排空 | Core | 不适用 | 活动任务取消并等待结束 |
| [ ] | B13 | 状态栏任务投影 | UI | ProgressRing/ProgressBar + Flyout | 排队计数、详情、逐项/全部取消正确 |
| [ ] | B14 | 状态 UI 刷新节流 | UI | DispatcherQueue + 差量更新 | 高频进度不重建全部视图 |
| [ ] | B15 | 关闭窗口任务守卫 | Core | UI 负责对话框 | 继续运行/中止退出决策完整 |

## 10. 项目与文件操作

当前依据：`Projects`、`IProjectBuilder`、`IProjectService`、`IProjectBehavior`、`ProjectSaveFileDialog.cs`。

| 状态 | ID | 原子模块 | 归属 | WinUI 3 实现 / 原生性 | 验收点 |
| --- | --- | --- | --- | --- | --- |
| [ ] | P01 | 单/多项目模式 | Core | 不适用 | 模式切换后的活动项目规则明确 |
| [ ] | P02 | 项目描述模型 | Core | 不适用 | ID、名称、路径、扩展元数据可序列化 |
| [ ] | P03 | 项目目录快照 | Core | 不适用 | 有序列表、活动 ID、版本原子 |
| [ ] | P04 | 项目 Add | Core | 不适用 | 重复 ID/路径策略可测试 |
| [ ] | P05 | 项目 Set | Core | 不适用 | 替换不破坏活动状态 |
| [ ] | P06 | 项目元数据修改 | Core | 不适用 | 名称/路径更新只发布一次 |
| [ ] | P07 | 活动项目切换 | Core | 不适用 | 请求、拒绝、成功事件顺序固定 |
| [ ] | P08 | 项目 Remove | Core | 不适用 | 移除活动项后的选择规则明确 |
| [ ] | P09 | 项目查询 | Core | 不适用 | ID 区分大小写和缺失结果稳定 |
| [ ] | P10 | Catalog 持久化 | Core | BCL 文件系统 API | 原子保存和损坏恢复正确 |
| [ ] | P11 | 失效路径清理 | Core | 不适用 | 启动清理并修复活动项 |
| [ ] | P12 | 持久化失败回滚 | Core | 不适用 | 内存状态与文件保持一致 |
| [ ] | P13 | 创建项目编排 | Core | Dialog/Picker 端口 | 可替换行为且支持取消 |
| [ ] | P14 | 保存项目编排 | Core | Dialog/Picker 端口 | 无活动项、另存、覆盖规则明确 |
| [ ] | P15 | 切换/删除/关闭编排 | Core | Dialog 端口 | 未保存状态和 veto 完整 |
| [ ] | P16 | WPF 保存文件适配器 | UI | 现有桌面对话框 | owner、筛选器和取消正确 |
| [ ] | P17 | WinUI 保存文件适配器 | UI | **框架直用** Windows App SDK FileSavePicker + WindowId | 路径、扩展、取消和异常正确 |
| [ ] | P18 | 文件/目录事务 | Core | BCL API | 失败不误删共享路径 |
| [ ] | P19 | 项目选择器状态 | Core | 不适用 | 标题栏只消费投影 |
| [ ] | P20 | WinUI 项目选择器 | UI | DropDownButton/MenuFlyout 或 ComboBox | 空项、单项、多项和 active 勾选正确 |
| [ ] | P21 | 未命名项目占位 | Core | UI 本地化显示 | 各表面文本一致 |

## 11. Profile、认证与凭据

当前依据：`Profile`、`IProfileService`、`IProfileAuthService`、`IProfileFlyoutService`、`Views/Page/ProfilePage.xaml`。

| 状态 | ID | 原子模块 | 归属 | WinUI 3 实现 / 原生性 | 验收点 |
| --- | --- | --- | --- | --- | --- |
| [ ] | U01 | Profile 用户模型 | Core | 不适用 | 姓名、图片和扩展数据可序列化 |
| [ ] | U02 | 登录状态机 | Core | 不适用 | SignedOut/SigningIn/SignedIn/Failed 合法 |
| [ ] | U03 | 名称顺序 | Core | 不适用 | FirstLast/LastFirst 正确 |
| [ ] | U04 | DisplayName | Core | 不适用 | 空字段、单字段和 Unicode 正确 |
| [ ] | U05 | Unicode 首字母 | Core | 不适用 | grapheme、CJK、空名称正确 |
| [ ] | U06 | 默认用户资料 | Core | 不适用 | 未登录/已登录快照不混淆 |
| [ ] | U07 | 认证请求/结果 | Core | 不适用 | 成功、拒绝、取消、错误可区分 |
| [ ] | U08 | 可替换认证服务 | Core | 不适用 | 自定义认证不依赖 XAML |
| [ ] | U09 | 默认简单认证 | Core | 不适用 | 姓名/密码校验与错误本地化 key 正确 |
| [ ] | U10 | 登录/登出编排 | Core | 不适用 | busy、失败回滚和事件顺序正确 |
| [ ] | U11 | 记住登录策略 | Core | `ISecretStore` 端口 | 开关变化立即更新状态 |
| [ ] | U12 | 凭据 Schema 版本 | Core | 不适用 | 失效版本可识别并清理 |
| [ ] | U13 | WPF 凭据实现 | UI | Windows DPAPI CurrentUser | 旧凭据可读取或有迁移说明 |
| [ ] | U14 | WinUI 凭据实现 | UI | Windows 凭据/DPAPI 薄适配 | 明文不落盘，删除幂等 |
| [ ] | U15 | 图片路径/回退策略 | Core | UI 负责解码 | 空、缺失、损坏得到回退 |
| [ ] | U16 | WinUI 图片解码 | UI | BitmapImage/ImageBrush 异步加载 | 最长边限幅、不放大、不锁文件 |
| [ ] | U17 | 透明图片与缓存 | UI | 平台 ImageSource 缓存 | 透明 PNG、复用、释放正确 |
| [ ] | U18 | Profile 浮层状态 | Core | 不适用 | Enable/Show/Hide/Toggle 一致 |
| [ ] | U19 | Profile content key | Core | 不保存 WPF Page Type | 默认和自定义内容可路由 |
| [ ] | U20 | 内容惰性创建/失败回滚 | UI | View factory + DI | 隐藏时不实例化，失败保持旧内容 |
| [ ] | U21 | WinUI Profile 表面 | UI | **原生组合** Flyout + PersonPicture | 焦点、关闭、触摸正确 |
| [ ] | U22 | WinUI Profile 页面 | UI | Page/UserControl + 原生输入控件 | 登录、图片、Password、Remember Login 完整 |
| [ ] | U23 | Profile 无障碍 | UI | PersonPicture/AutomationProperties | 姓名、状态和按钮可读 |

## 12. 消息、通知与临时状态

当前依据：`Messaging`、`IMessageService`、`INotificationService`、`MessageBoxWindow.xaml`、`NotificationHost.xaml`。

| 状态 | ID | 原子模块 | 归属 | WinUI 3 实现 / 原生性 | 验收点 |
| --- | --- | --- | --- | --- | --- |
| [ ] | M01 | `DialogRequest` | Core | 不使用 WPF MessageBox 枚举 | 标题、正文、严重级别和选择完整 |
| [ ] | M02 | `DialogChoice` | Core | 不适用 | ID、文本、default/cancel/primary 可表达 |
| [ ] | M03 | `DialogResult` | Core | 不使用 MessageBoxResult | 标准与自定义选项统一 |
| [ ] | M04 | 选项校验 | Core | 不适用 | 唯一 default/cancel/primary 规则正确 |
| [ ] | M05 | 异步消息端口 | Core | UI presenter | Core 不依赖 XamlRoot/Window |
| [ ] | M06 | WPF 同步兼容 API | UI | 仅 WPF 过渡期保留 | 不进入 Core/WinUI 主 API |
| [ ] | M07 | WPF 消息窗口 | UI | 保留自定义 WPF Window | 视觉、RTL、owner 无回归 |
| [ ] | M08 | WinUI 消息窗口 | UI | **框架直用** ContentDialog | XamlRoot、默认键、取消正确 |
| [ ] | M09 | 每窗口 Dialog 队列 | UI | 原生组合 | 同一 XamlRoot 不并发显示两个 Dialog |
| [ ] | M10 | 任意数量 actions | UI | 三按钮内原生；超出时收敛或自定义 footer | 产品决策和差异文档明确 |
| [ ] | M11 | owner/XamlRoot 解析 | UI | Window 注册表薄适配 | 多窗口显示到正确 owner |
| [ ] | M12 | 打开前取消 | Core | 不适用 | 与现有取消契约一致 |
| [ ] | M13 | 通知模型/严重级别 | Core | 不适用 | Info/Success/Warning/Error 稳定 |
| [ ] | M14 | 通知 Show/Upsert/Update | Core | 不适用 | 替换时重置超时 |
| [ ] | M15 | Dismiss/DismissAll | Core | 不适用 | 幂等且版本准确 |
| [ ] | M16 | 通知句柄 | Core | 不适用 | Update/Dispose 只影响自身 |
| [ ] | M17 | 自动消失计时 | Core | UI 仅呈现 | 取消/替换/关闭无竞态 |
| [ ] | M18 | 通知命令 | Core | command key | Action 通过统一 dispatcher |
| [ ] | M19 | WinUI 通知单项 | UI | **框架直用** InfoBar | severity、close、action、UIA 正确 |
| [ ] | M20 | NotificationHost | UI | InfoBar 集合 host | 最新 5 条、复用、live region 正确 |
| [ ] | M21 | 系统 AppNotification | UI | Windows App SDK 原生，可选扩展 | 与应用内通知分开建模 |
| [ ] | M22 | Status Overlay | UI | Flyout/Popup/ContentPresenter | 不混用 Dialog/ToolTip 语义 |

## 13. 导航、路由、历史与缓存

当前依据：`Navigation`、`INavigationBuilder`、`INavigationService`、`NavigationMenu*`、`NavigationPaneView.xaml`。

| 状态 | ID | 原子模块 | 归属 | WinUI 3 实现 / 原生性 | 验收点 |
| --- | --- | --- | --- | --- | --- |
| [ ] | N01 | route key 规则 | Core | 不适用 | 默认 key 去一个大小写敏感 Page 后缀 |
| [ ] | N02 | 中立路由定义 | Core | 不含 Page/factory | key、view key、cache、metadata 完整 |
| [ ] | N03 | route key/type 双索引元数据 | Core | 实际 Page 校验留 UI | 重复页面定义规则明确 |
| [ ] | N04 | 路由 Add/Set/Remove | Core | 不适用 | 租约与版本防止乱序删除 |
| [ ] | N05 | 初始路由 | Core | 不适用 | 显式、首项、空路由回退明确 |
| [ ] | N06 | 导航请求 | Core | UI 执行 view 切换 | 参数、历史策略、取消可表达 |
| [ ] | N07 | 当前导航快照 | Core | 不保存 Page 实例 | key、parameter、CanGoBack/Forward 原子 |
| [ ] | N08 | 有界 Back 栈 | Core | 不适用 | 超限逐出最旧项 |
| [ ] | N09 | Forward 栈 | Core | 不适用 | 新导航后按规则清空 |
| [ ] | N10 | 清理 Back/Forward/All | Core | 不适用 | 三个操作各自准确 |
| [ ] | N11 | 相同 key/parameter 去重 | Core | 不适用 | 无操作不发布事件 |
| [ ] | N12 | 路由删除清理历史 | Core | 不适用 | 不留下不可达项 |
| [ ] | N13 | 上次导航持久化 | Core | 不适用 | 路由缺失时安全回退 |
| [ ] | N14 | 缓存策略/快照 | Core | 仅 key/type 元数据 | Enabled/Disabled 和逐出事件正确 |
| [ ] | N15 | 创建失败不缓存 | Core | UI 报告创建结果 | 无半初始化缓存 |
| [ ] | N16 | WPF 页面缓存 | UI | WPF Page 实例 | 原有行为无回归 |
| [ ] | N17 | WinUI 页面缓存 | UI | Frame/Page 或 view cache | 页面生命周期和释放明确 |
| [ ] | N18 | WPF 页面工厂 | UI | DI + WPF Page | DI 优先、Activator fallback |
| [ ] | N19 | WinUI 页面工厂 | UI | DI + WinUI Page/UserControl | 构造注入和失败诊断正确 |
| [ ] | N20 | 导航宿主拒绝回滚 | Core | UI 返回结果 | 历史和当前项恢复 |
| [ ] | N21 | 并发导航串行化 | Core | 不适用 | 只提交最新合法状态 |
| [ ] | N22 | 导航完成事件 | Core | 不返回具体 Page | route/view/parameter 和时机正确 |
| [ ] | N23 | 菜单分组 | Core | 不适用 | Add/Remove/Title/Order 事务化 |
| [ ] | N24 | 页面项 | Core | 不适用 | route、icon、visible、enabled 完整 |
| [ ] | N25 | 命令项 | Core | 不适用 | command 与 page item 不混淆 |
| [ ] | N26 | 固定项 | Core | 不适用 | Footer 顺序稳定 |
| [ ] | N27 | 一层父子树 | Core | UI 投影层级 | move/expand/selection 正确 |
| [ ] | N28 | 菜单事务编辑器 | Core | 不适用 | 批量编辑只发布一次 |
| [ ] | N29 | 面板开关状态 | Core | 不适用 | Enable/Open/Close/Toggle 一致 |
| [ ] | N30 | 面板方向 | Core | 不适用 | Left/Right 是布局语义 |
| [ ] | N31 | 面板宽度约束 | Core | 不适用 | open/closed/min/max 校验完整 |
| [ ] | N32 | 面板偏好 | Core | 不适用 | 方向、开关、宽度独立 |
| [ ] | N33 | 标准左/顶导航 | UI | **框架直用** NavigationView + Frame | 自适应、选择和 Back 同步 |
| [ ] | N34 | Breadcrumb | UI | **框架直用** BreadcrumbBar | 路径、溢出、键盘正确 |
| [ ] | N35 | 右侧可调导航 | UI | **原生组合/风险** SplitView + ListView + GridSplitter | 不用 RTL；拖宽和折叠正确 |
| [ ] | N36 | 导航组/固定项 UI | UI | NavigationView/ListView/TreeView | 滚动组和底部固定区正确 |
| [ ] | N37 | 导航图标 | UI | FontIcon/SymbolIcon/IconSource | glyph 与未来 icon source 可扩展 |
| [ ] | N38 | Frame 导航适配 | UI | 原生 Frame.Navigate/GoBack | NavigationView 不承担历史规则 |
| [ ] | N39 | 页面过渡投影 | UI | 原生 transition/Composition | 遵守 Core 动效策略 |
| [ ] | N40 | 导航失败恢复 | UI | 失败结果回传 Core | View、selection、history 一致 |

## 14. 外观、主题、字体与材质

当前依据：`Appearance`、`Themes`、`IAppearance*`、`IFont*`、`IThemeService`、`IMaterialEffectService`。

| 状态 | ID | 原子模块 | 归属 | WinUI 3 实现 / 原生性 | 验收点 |
| --- | --- | --- | --- | --- | --- |
| [ ] | T01 | 主题选择 | Core | 不适用 | System/Light/Dark 语义一致 |
| [ ] | T02 | 请求/有效主题 | Core | UI 回报系统有效值 | 两者不混淆 |
| [ ] | T03 | Toggle 算法 | Core | 不适用 | System 状态的切换规则明确 |
| [ ] | T04 | 主题偏好 | Core | 不适用 | runtime 先应用后持久化 |
| [ ] | T05 | `ArgbColor` | Core | 不使用 WPF Color | 解析、相等、序列化正确 |
| [ ] | T06 | Primary/Secondary/Accent | Core | UI 映射 ThemeResource | 只接受不透明色 |
| [ ] | T07 | 派生颜色算法 | Core | 不适用 | hover/pressed/surface/on-color 可读 |
| [ ] | T08 | 语义颜色角色 | Core | UI 映射资源 | Neutral/Danger/Warning/Selection 等全覆盖 |
| [ ] | T09 | 圆角 Token | Core | UI 映射 CornerRadius | 默认、统一覆盖、恢复正确 |
| [ ] | T10 | 外观原子更新 | Core | 不适用 | 颜色+圆角只发布一次 |
| [ ] | T11 | WinUI RequestedTheme | UI | **框架直用** | Window、Popup、新页面同步 |
| [ ] | T12 | ThemeDictionaries | UI | **框架直用** ResourceDictionary | Light/Dark/HighContrast 完整 |
| [ ] | T13 | 系统主题监听 | UI | 原生设置事件薄适配 | 不轮询且可释放 |
| [ ] | T14 | 高对比度 | UI | 系统 ThemeResource/UISettings | 自定义色不覆盖系统可读性 |
| [ ] | T15 | 字体族状态 | Core | 字符串/中立描述 | 正文与图标字体分离 |
| [ ] | T16 | 六级字号 | Core | 不适用 | Small/Standard/Icon/Large/ExtraLarge/Header |
| [ ] | T17 | 页面字体覆盖 | Core | route/view key | 空值跟随全局，移除恢复 |
| [ ] | T18 | 字体变更元数据 | Core | affected view key | 变化范围明确 |
| [ ] | T19 | WinUI 字体资源 | UI | FontFamily/Double ThemeResource | 已打开视图立即刷新 |
| [ ] | T20 | 图标字体 | UI | FontIcon；平台推荐图标优先 | 缺失 glyph 有回退 |
| [ ] | T21 | 材质请求 | Core | 不适用 | None/Mica/Acrylic/MicaAlt/Auto 完整 |
| [ ] | T22 | 材质有效状态 | Core | UI 回报支持/应用结果 | requested/effective/supported/applied 分离 |
| [ ] | T23 | WinUI Mica | UI | **框架直用** Window.SystemBackdrop + MicaBackdrop | Win11 与 fallback 正确 |
| [ ] | T24 | WinUI MicaAlt | UI | MicaKind.BaseAlt | 视觉与能力降级明确 |
| [ ] | T25 | Desktop Acrylic | UI | **框架直用** DesktopAcrylicBackdrop | 节能/失焦/不支持状态正确 |
| [ ] | T26 | 局部材质 | UI | SystemBackdropElement/FlyoutBase.SystemBackdrop 或 AcrylicBrush | 不混淆桌面与应用内 backdrop |
| [ ] | T27 | WPF 材质后端 | UI | 保留现有 DWM/Accent 实现 | 不搬入 WinUI 默认路径 |
| [ ] | T28 | 深色模式同步 | UI | 有效主题→backdrop | 切换无闪屏和旧控制器残留 |
| [ ] | T29 | 材质资源释放 | UI | Window Closed 解除控制器/事件 | 多窗口无泄漏 |
| [ ] | T30 | 降级矩阵 | UI | 能力检测而非版本字符串 | 每个目标 OS 有明确最终背景 |

## 15. 布局、滚动、动效与 ToolTip

当前依据：`Layout`、`Motion`、`ToolTips`、`Controls/HoverReveal*`、`RoundedClipCoordinator.cs`。

| 状态 | ID | 原子模块 | 归属 | WinUI 3 实现 / 原生性 | 验收点 |
| --- | --- | --- | --- | --- | --- |
| [ ] | L01 | 内容居中状态 | Core | 不适用 | enabled/width 校验和无操作行为正确 |
| [ ] | L02 | 内容居中算法 | Core | 纯几何 | 窄视口使用可用宽度 |
| [ ] | L03 | 内容居中 UI | UI | Grid MaxWidth/HorizontalAlignment | 只限制页面主体，滚动条仍在视口边缘 |
| [ ] | L04 | 平滑滚动策略 | Core | 不适用 | app 默认与控件本地覆盖明确 |
| [ ] | L05 | WinUI 基础滚动 | UI | **框架直用** ScrollView/ScrollViewer | 原生触摸、触控板、滚轮和惯性 |
| [ ] | L06 | 程序化滚动动画 | UI | ScrollOptions.AnimationMode=Auto | 只映射 smooth policy，不改物理滚轮手感 |
| [ ] | L07 | 嵌套滚动链 | UI | 原生 ScrollChainMode 优先 | 子视口边界后外层继续 |
| [ ] | L08 | 精密滚轮 | UI | 原生输入优先 | 不把精密滚轮离散化 |
| [ ] | L09 | 虚拟化滚动 | UI | ListView/ItemsRepeater | 外层无限测量不破坏虚拟化 |
| [ ] | L10 | 动效总开关 | Core | 不适用 | 所有自定义动效消费同一有效状态 |
| [ ] | L11 | 页面过渡类型/时长 | Core | 不适用 | Fade/Entrance/None 等语义固定 |
| [ ] | L12 | 面板过渡类型/时长 | Core | 不适用 | Resize/Overlay 等语义固定 |
| [ ] | L13 | Hover Reveal 策略 | Core | 不适用 | enabled/duration/persistence 正确 |
| [ ] | L14 | Reduce Motion 策略 | Core | UI 提供系统偏好 | 系统禁用动画时 effective=false |
| [ ] | L15 | WinUI 系统动画偏好 | UI | UISettings.AnimationsEnabled 薄适配 | 运行时变化立即生效 |
| [ ] | L16 | WinUI 页面过渡 | UI | Frame transition/Composition | 快速导航取消旧过渡 |
| [ ] | L17 | WinUI 面板过渡 | UI | VisualStateManager/Composition | 中断、反向和尺寸变化连续 |
| [ ] | L18 | WinUI Hover Reveal | UI | Pointer VisualState/implicit transition | 键盘焦点独立、Reduce Motion 生效 |
| [ ] | L19 | 圆角裁剪 | UI | Border.CornerRadius/原生 clip 优先 | 复杂动态情形才保留 helper |
| [ ] | L20 | ToolTip 开关 | Core | 不适用 | 只影响框架 opt-in 控件 |
| [ ] | L21 | ToolTip delay/margin 状态 | Core | 不适用 | 非法值拒绝、快照正确 |
| [ ] | L22 | WinUI ToolTip | UI | **框架直用** ToolTip/ToolTipService | pointer/focus/touch/UIA 原生 |
| [ ] | L23 | InitialShowDelay 差异 | UI | WinUI 无等价公共 API；默认记录平台差异 | 不为像素一致重写完整生命周期 |
| [ ] | L24 | 严格延迟兼容扩展 | UI | 仅硬需求时 opt-in custom overlay | 不影响第三方控件和原生 ToolTip |
| [ ] | L25 | 放置计算 | Core | 仅纯几何可共享 | 四边、角落、DPI 测试 |
| [ ] | L26 | WinUI 放置/夹紧 | UI | 原生 Placement 优先 | 不越窗口/显示区域 |
| [ ] | L27 | Accelerator ToolTip | UI | 原生自动呈现 | 自定义内容不丢组合键 |
| [ ] | L28 | 生命周期清理 | UI | Unloaded/Closed 解除动画、计时器、事件 | 页面缓存和关窗无泄漏 |

## 16. Shell 状态、标题栏、工具栏、状态栏与区域

当前依据：`Shell`、`TitleBar*`、`Toolbar*`、`StatusBar*`、`ShellRegion*`。

| 状态 | ID | 原子模块 | 归属 | WinUI 3 实现 / 原生性 | 验收点 |
| --- | --- | --- | --- | --- | --- |
| [ ] | S01 | 标题栏启用状态 | Core | 不适用 | Builder/Runtime 状态一致 |
| [ ] | S02 | 应用标题/副标题 | Core | 不适用 | 空值、更新、本地化规则明确 |
| [ ] | S03 | 未命名项目文本 | Core | 不适用 | 项目和 TitleBar 使用同一值 |
| [ ] | S04 | Logo 路径/回退文本 | Core | UI 解码图片 | 损坏资源安全回退 |
| [ ] | S05 | Logo 信息字段开关 | Core | 不适用 | app title/subtitle/project 独立 |
| [ ] | S06 | 标题栏元素可见性 | Core | 不适用 | Search/Breadcrumb/Nav/Logo/Title/Theme/Profile 全覆盖 |
| [ ] | S07 | Breadcrumb 模式 | Core | UI 投影 Auto/Always/Never | 尺寸变化结果明确 |
| [ ] | S08 | Search placeholder | Core | 不适用 | runtime 更新立即投影 |
| [ ] | S09 | Search text | Core | 不适用 | 编程设置不触发用户查询 |
| [ ] | S10 | Search sequence | Core | 不适用 | 用户查询序号递增 |
| [ ] | S11 | Search 取消旧工作 | Core | CancellationToken | 新查询取消旧查询 |
| [ ] | S12 | Search 订阅租约 | Core | 不适用 | 无订阅不分配 CTS，Dispose 精确 |
| [ ] | S13 | Search focus request | Core | UI 消费一次性请求 | 不重复聚焦 |
| [ ] | S14 | 标题/项目投影 | Core | 不适用 | 项目切换后标题一致 |
| [ ] | S15 | WinUI TitleBar 控件 | UI | **框架直用** Windows App SDK 1.7+ `TitleBar` | Back/Pane/Icon/Title/Header/Content 可用 |
| [ ] | S16 | 标题栏接管 | UI | ExtendsContentIntoTitleBar/SetTitleBar | 拖动区与交互区准确 |
| [-] | S17 | 自绘 caption buttons | UI | **不迁移** | 使用系统按钮，保留 Snap/菜单/UIA |
| [ ] | S18 | AppWindowTitleBar | UI | **系统原生** caption buttons/颜色/insets | 系统最小/最大/关闭完整 |
| [ ] | S19 | Win10/能力降级 | UI | IsCustomizationSupported | 不支持时回到安全系统标题栏 |
| [ ] | S20 | inset/DPI/RTL | UI | LeftInset/RightInset | 内容不被 caption 遮挡 |
| [ ] | S21 | Back/Forward UI | UI | TitleBar BackRequested + 导航状态 | 可用性、历史、快捷键同步 |
| [ ] | S22 | PaneToggle UI | UI | TitleBar PaneToggleRequested | 与面板状态同步 |
| [ ] | S23 | 标题栏搜索 UI | UI | AutoSuggestBox 放入 Content/RightHeader | IME、清除、提交正确 |
| [ ] | S24 | Logo 信息表面 | UI | Button + Flyout | pack/URI/文件 Logo、焦点回归正确 |
| [ ] | S25 | 窗口图标同步 | UI | AppWindow SetIcon/TitleBar IconSource | 失败不影响标题栏 |
| [ ] | S26 | Theme 入口 | UI | Button/MenuFlyout | System/Light/Dark 状态和图标同步 |
| [ ] | S27 | Profile 入口 | UI | PersonPicture + Flyout | 登录状态和图片同步 |
| [ ] | S28 | Toolbar 总开关 | Core | 不适用 | 隐藏不丢注册项 |
| [ ] | S29 | 默认 Toolbar items | Core | 不适用 | ID/text/icon/command/visible/enabled 完整 |
| [ ] | S30 | route-specific Toolbar | Core | 使用 route/view key | 不用 WPF Page Type |
| [ ] | S31 | Toolbar 增删改排 | Core | 不适用 | 集合事件和版本准确 |
| [ ] | S32 | icon-only | Core | 不适用 | 默认/页面覆盖分离 |
| [ ] | S33 | WinUI Toolbar | UI | CommandBar/AppBarButton/AppBarElementContainer | overflow、键盘、UIA 正确 |
| [ ] | S34 | CanExecute 差量刷新 | UI | 原生控件状态更新 | 不全量重建 |
| [ ] | S35 | Toolbar 元素缓存 | UI | 缓存 descriptor/template，不缓存挂载 UIElement | 无 parent 冲突 |
| [ ] | S36 | StatusBar 总开关 | Core | 不适用 | 内置/自定义状态分别保留 |
| [ ] | S37 | Status items | Core | 不适用 | Add/Set/Text/Icon/Visible/Order/Remove |
| [ ] | S38 | 临时状态句柄 | Core | 不适用 | Update/Timeout/Dispose 竞态正确 |
| [ ] | S39 | LAN 状态端口 | Core | 平台 provider 回报 | Online/Offline/Unknown 语义明确 |
| [ ] | S40 | WinUI LAN 观察器 | UI | Windows 网络 API 薄适配 | 合并事件、线程切换、释放正确 |
| [ ] | S41 | Power 状态端口 | Core | 平台 provider 回报 | AC/Battery/Unknown/percentage 完整 |
| [ ] | S42 | WinUI Power 观察器 | UI | Windows 电源 API 薄适配 | 状态及时且无轮询泄漏 |
| [ ] | S43 | WinUI StatusBar | UI | 自定义布局 + InfoBadge/ProgressRing/Flyout | item/task/network/power 正确 |
| [ ] | S44 | Shell Region 元数据 | Core | id/region/order/enabled/content key | 不保存 UIElement factory |
| [ ] | S45 | 14 个 Shell Region | Core | 不适用 | TitleBar/Navigation/Content/Toolbar/Footer 全覆盖 |
| [ ] | S46 | Region Add/Set/Enable/Order/Remove | Core | 不适用 | 租约、替换和 RemoveAll 正确 |
| [ ] | S47 | WPF Region factory | UI | FrameworkElement factory | 兼容现有扩展 |
| [ ] | S48 | WinUI Region factory | UI | DataTemplate/view factory + DI | 单 parent、异常和释放正确 |
| [ ] | S49 | 快捷 TitleBar/Footer action | UI | command key 优先，callback 兼容 | CommandSource 与 UIA 正确 |

## 17. 窗口、关闭流程与通知区域

当前依据：`Windowing`、`IWindowBuilder`、`IWindowService`、`IWindowCloseService`、`ITrayService`、`ShellWindow.xaml`。

| 状态 | ID | 原子模块 | 归属 | WinUI 3 实现 / 原生性 | 验收点 |
| --- | --- | --- | --- | --- | --- |
| [ ] | W01 | `WindowSize` | Core | 不使用 WPF Size | finite/positive/serialization 正确 |
| [ ] | W02 | `WindowBounds` | Core | 不使用 WPF Rect | 坐标、尺寸、相等正确 |
| [ ] | W03 | `WindowDisplayState` | Core | 不使用 WPF WindowState | Normal/Minimized/Maximized 完整 |
| [ ] | W04 | `ResizePolicy` | Core | 不使用 WPF ResizeMode | 各 resize/min/max 组合明确 |
| [ ] | W05 | `StartupPlacement` | Core | 不使用 WindowStartupLocation | Center/Manual/Persisted 完整 |
| [ ] | W06 | 初始/最小/最大尺寸 | Core | UI 应用 | 非法组合在构建期失败 |
| [ ] | W07 | 窗口语义快照 | Core | UI 上报平台观察值 | bounds/state/topmost/taskbar/visible/active 原子 |
| [ ] | W08 | 窗口偏好 | Core | UI 提供坐标转换 | size/position/state/topmost/close 独立 |
| [ ] | W09 | 多显示器恢复 | UI | **框架直用** DisplayArea + AppWindow | 显示器移除后窗口仍可见 |
| [ ] | W10 | DIPs/物理像素转换 | UI | XAML scale/AppWindow 像素 | 100/150/200% 正确 |
| [ ] | W11 | WinUI Move/Resize | UI | AppWindow.Move/Resize/MoveAndResize | 位置与尺寸事件正确 |
| [ ] | W12 | WinUI min/max 尺寸 | UI | OverlappedPresenter preferred size；必要时最小 Win32 hook | 拖动/程序设置都受限 |
| [ ] | W13 | WinUI resize 能力 | UI | OverlappedPresenter | resizable/minimizable/maximizable 映射 |
| [ ] | W14 | Topmost | UI | OverlappedPresenter.IsAlwaysOnTop | 运行时切换与恢复正确 |
| [ ] | W15 | Taskbar/Alt-Tab | UI | AppWindow.IsShownInSwitchers/薄适配 | 行为和平台差异记录 |
| [ ] | W16 | Show/Hide/Activate | UI | Window/AppWindow 原生 API | 激活失败可诊断 |
| [ ] | W17 | Minimize/Maximize/Restore | UI | OverlappedPresenter | 状态事件一致 |
| [ ] | W18 | Window changed 观察 | UI | AppWindow.Changed + XAML 事件 | 去重并封送 UI 队列 |
| [ ] | W19 | 关闭行为 | Core | 不适用 | Prompt/Close/MinimizeToTray 完整 |
| [ ] | W20 | 关闭原因 | Core | 不适用 | TitleBar/Window/Tray/Application 可区分 |
| [ ] | W21 | 关闭守卫注册/顺序 | Core | 不适用 | order 后 ID，异步/veto/Dispose 正确 |
| [ ] | W22 | 项目关闭守卫 | Core | Dialog 端口 | 保存/放弃/取消完整 |
| [ ] | W23 | 后台任务关闭守卫 | Core | Dialog 端口 | keep running/stop and exit 完整 |
| [ ] | W24 | WinUI Closing 桥接 | UI | AppWindow.Closing + 异步协调 | 无重入、双关闭、失去取消机会 |
| [ ] | W25 | Tray 状态 | Core | 不适用 | enabled/visible/tooltip/version 一致 |
| [ ] | W26 | WinUI Tray | UI | **待选型** WinUIEx TrayIcon 或 Shell_NotifyIcon | 不引入 WinForms，Explorer 重启可恢复 |
| [ ] | W27 | Tray Tooltip 限制 | UI | 平台适配 | 长度、本地化和截断规则明确 |
| [ ] | W28 | Tray menu | UI | 平台菜单/第三方控件 | Restore/Exit 和命令状态正确 |
| [ ] | W29 | Close to Tray | UI | close service + tray adapter | Hide 不停 Host，Exit 走守卫 |
| [ ] | W30 | 应用退出 | UI | Application.Exit/窗口 + Host.StopAsync | 无线程/图标残留 |
| [ ] | W31 | 多窗口扩展边界 | UI | Window/AppWindow/XamlRoot 注册表 | 当前可单窗，但契约不锁死单 HWND |

## 18. Shell 视图与控制器

当前依据：`Views/Windows`、`Views/Page`、`Windowing/ShellFrameController.cs`。

| 状态 | ID | 原子模块 | 归属 | WinUI 3 实现 / 原生性 | 验收点 |
| --- | --- | --- | --- | --- | --- |
| [ ] | V01 | ShellWindow 骨架 | UI | Window + Grid + TitleBar + 内容层 | 尺寸变化区域不重叠 |
| [ ] | V02 | ShellContentHost | UI | ContentPresenter/Grid | 页面、导航、Overlay 层级正确 |
| [ ] | V03 | NavigationPaneView | UI | NavigationView 或 SplitView 方案 | 只消费 Core 快照 |
| [ ] | V04 | TitleBar view | UI | 原生 TitleBar 主体 | 不复制非客户区系统行为 |
| [ ] | V05 | ToolbarView | UI | CommandBar/ItemsRepeater | 页面切换即时更新 |
| [ ] | V06 | StatusBarView | UI | Grid/ItemsRepeater | 系统项/应用项顺序稳定 |
| [ ] | V07 | NotificationHost | UI | InfoBar 集合 | queue/upsert/timeout 正确 |
| [ ] | V08 | ApplicationInfoOverlay | UI | Flyout/ContentPresenter | Logo、标题、项目按状态显示 |
| [ ] | V09 | ProfileOverlay | UI | Flyout + Profile content | 自定义内容可替换 |
| [ ] | V10 | StatusOverlay | UI | Flyout | 锚点、边界、Esc、焦点循环/恢复正确 |
| [ ] | V11 | ProfilePage | UI | WinUI Page/UserControl | 输入、认证、图片、本地化完整 |
| [ ] | V12 | ShellNavigationController | UI | Core↔view adapter | 无领域规则复制到 code-behind |
| [ ] | V13 | ShellTitleBarController | UI | 原生 TitleBar event adapter | 订阅可释放，无状态双写 |
| [ ] | V14 | ShellToolbarController | UI | descriptor→control | 定向刷新，无挂载元素缓存 |
| [ ] | V15 | ShellStatusSurfaceController | UI | 状态聚合→view | 节流、复用、差量更新 |
| [ ] | V16 | ShellNotificationController | UI | queue→InfoBar | timeout/手动关闭无竞态 |
| [ ] | V17 | ShellProfileController | UI | state→PersonPicture/Flyout | 登录切换无旧 view |
| [ ] | V18 | ProjectSelectorController | UI | state→menu | active/check 同步 |
| [ ] | V19 | ShellRegionElementFactory | UI | content key→DataTemplate/factory | DI、异常、parent 安全 |
| [ ] | V20 | Frame/边框修复 | UI | 先验证 WinUI 原生 | 仅已复现平台缺陷保留补丁 |
| [ ] | V21 | 焦点恢复 | UI | FocusManager | 导航/Flyout/Dialog 后合理 |
| [ ] | V22 | UI 线程封送 | UI | DispatcherQueue | 后台事件不跨线程更新 XAML |
| [ ] | V23 | 浮层尺寸夹紧 | UI | Flyout placement/Popup helper | 锚点消失和窗口缩放正确 |
| [ ] | V24 | 浮层 light-dismiss/Esc | UI | 原生行为优先 | 模态性与焦点契约明确 |

## 19. 控件库迁移

当前依据：`Controls`、`Themes/Controls.xaml`、`Themes/Generic.xaml`、`docs/zh-cn/controls`。所有本节原子项归属 UI。

### 19.1 原生控件优先项

| 状态 | ID | 控件/行为 | WinUI 3 实现 / 原生性 | 必须保留的能力与验收点 |
| --- | --- | --- | --- | --- |
| [ ] | K01 | Button 基础语义 | **框架直用** 原生 Button | pointer/keyboard/focus/disabled/default/UIA 完整 |
| [ ] | K02 | Button variants | Style/lightweight styling/必要模板 | Elevated/Filled/Tonal/Outlined/Text/Danger/Standard |
| [ ] | K03 | Button icon | FontIcon/IconSource + Content | IconSize、icon-only、文本布局正确 |
| [ ] | K04 | CardButton | 原生 Button 自定义模板 | Card 整面点击、Title/IconPosition/MaxLines |
| [-] | K05 | WindowCaptionButton | **无需迁移为系统功能** | WinUI 使用系统 caption；若保留示例只能是普通视觉按钮 |
| [ ] | K06 | CheckBox | **框架直用** CheckBox + 样式 | Horizontal/Vertical、Icon、二态/三态 |
| [ ] | K07 | ComboBox | **框架直用** ComboBox + 样式 | 原生选择、Popup、输入、键盘 |
| [ ] | K08 | ComboBoxItem | **框架直用** ComboBoxItem 样式 | 对齐、contained navigation、容器回收 |
| [ ] | K09 | ListBox/ListView | 优先 ListView/ListBox + Style | Standard/Borderless、Compact、虚拟化、选择 |
| [ ] | K10 | List item metadata | 原生 container + 附加属性/数据模板 | IsItemVisible/IsGroupHeader/IsCommandItem |
| [ ] | K11 | RadioButton | **框架直用** RadioButton + 样式 | 分组、键盘、选择语义 |
| [ ] | K12 | PasswordBox | **框架直用** PasswordBox + 模板 | Password/SecurePassword、char、MaxLength、事件 |
| [ ] | K13 | PasswordBox 操作 | 原生能力 + 必要薄门面 | Clear/SelectAll/FocusEditor，不暴露可绑定明文 |
| [ ] | K14 | TextBox | **框架直用** TextBox + 样式 | IME、撤销、选择、焦点、验证 |
| [ ] | K15 | TextBlock | **框架直用** TextBlock + role 样式映射 | wrapping/trimming/max lines/UIA |
| [ ] | K16 | 12 个 TextRole | ThemeResource/附加属性 | 字号、行高、底部间距、字重完整 |
| [ ] | K17 | TextLayoutMode | 原生 line-height/wrapping 属性投影 | Flow/ControlLineBox 语义明确 |
| [ ] | K18 | SearchBox | **框架直用** AutoSuggestBox | placeholder、glyph、suggest/submit/clear/IME |
| [ ] | K19 | ScrollBar | **框架直用** ScrollBar + 样式 | 横纵几何、圆角 Thumb、pressed |
| [ ] | K20 | ScrollViewer | 优先 ScrollView/ScrollViewer | 原生惯性、chain、模板缺失和 Unload 安全 |
| [ ] | K21 | ToolTip | **框架直用** ToolTip/ToolTipService + 样式 | focus/hover/touch/accelerator/placement |
| [ ] | K22 | Label | 控件 Header 或 TextBlock + LabeledBy | 与输入控件的 UIA 标签关系正确 |

### 19.2 原生组合控件

| 状态 | ID | 控件/行为 | WinUI 3 实现 / 原生性 | 必须保留的能力与验收点 |
| --- | --- | --- | --- | --- |
| [ ] | K23 | Card | Border/ContentControl 原生组合 | Elevated/Standard/Tonal/Filled、Dock、单 glyph |
| [ ] | K24 | Card elevation | ThemeShadow/translation 或推荐阴影 | 只有 Elevated 使用，Light/Dark 正确 |
| [ ] | K25 | ActionCard 布局 | Button 语义 + Card 视觉 | Horizontal/Vertical、Icon/Title/Content |
| [ ] | K26 | ActionCard 内容约束 | 自定义 ContentControl/模板校验 | 只允许一个 Body，空区域折叠 |
| [ ] | K27 | Chunk | 自定义 ContentControl 模板 | Title/Content/Body/Margin/Spacing |
| [ ] | K28 | Chunk 逻辑树 | 平台内容模型 | 隐式内容进入 Body，所有权正确 |
| [ ] | K29 | HeaderChunk | Presenter 特化/组合 | HeaderSize、三种模式、默认右侧展示 |
| [ ] | K30 | Document | ItemsControl/ItemsRepeater + 内容校验 | 只接受 Paragraph，首段/后续间距正确 |
| [ ] | K31 | Paragraph | RichTextBlock Paragraph 或 TextBlock 薄封装 | Large/Regular、换行、首行缩进 |
| [ ] | K32 | Document UIA | AutomationProperties 转发 | 文档名、内容和值可读取 |
| [ ] | K33 | CodeSpace 外壳 | **原生组合** Expander + ScrollView + Text + Button | 展开、绑定状态、最大高度 |
| [ ] | K34 | CodeSpace 文本 | TextBlock/RichTextBlock | 精确保留空白、等宽、长行策略 |
| [ ] | K35 | CodeSpace Clipboard | Windows Clipboard 薄适配 | Copy、确认态、失败和 UIA 正确 |
| [ ] | K36 | OutputCard 数据 | 自定义复合控件 + observable buffer | WriteLine/Clear，Output 立即可读 |
| [ ] | K37 | OutputCard UI 合并 | ItemsRepeater/ListView 差量刷新 | 高频输出不逐条重排 |
| [ ] | K38 | OutputCard 滚动 | ScrollView + parent chain | 自动滚底、最大高度、边界交接 |
| [ ] | K39 | Overlay 视觉 | Flyout wrapper/自定义表面 | Temporary/Strong、内容布局和主题 |
| [ ] | K40 | Temporary Overlay | Flyout TransientWithDismissOnPointerMoveAway 或薄协调 | 指针离开锚点+内容后宽限关闭 |
| [ ] | K41 | Strong Overlay | Standard Flyout/light-dismiss/显式关闭 | 打开状态由宿主管理 |
| [ ] | K42 | PageBody 集合约束 | 自定义 ScrollView 页面根 | 一个首位 HeaderChunk + 后续 Chunk |
| [ ] | K43 | PageBody 内容 API | Children 集合，拒绝直接 Content | 居中包装不破坏集合 |
| [ ] | K44 | PageBody 滚动/居中 | ScrollView + content max width | 滚动条在视口边缘 |
| [ ] | K45 | Presenter 内容区域 | 自定义 Control/Panel | Title/Content/Body/Presentation 空区折叠 |
| [ ] | K46 | Presenter Split | Grid/自定义 Panel + VisualState | Left/Right、比例、min height、spacing |
| [ ] | K47 | Presenter TopDown | 自定义模板/Panel | 多内容测量、排列、对齐 |
| [ ] | K48 | Presenter Overlay | Grid/ZIndex | 命中测试、对齐、圆角裁剪 |
| [ ] | K49 | Presenter 多元素 | 自定义 Panel/ItemsRepeater | 元素增删和 parent 生命周期正确 |

### 19.3 自定义控件与风险项

| 状态 | ID | 控件/行为 | WinUI 3 实现 / 原生性 | 必须保留的能力与验收点 |
| --- | --- | --- | --- | --- |
| [ ] | K50 | BunchedListBox 选择基础 | 原生 ListView 优先 | 原生 selection、keyboard、UIA、virtualization |
| [ ] | K51 | 共享 hover layer | **自定义** 父层 indicator | item 不重复绘制状态层 |
| [ ] | K52 | 共享 pressed layer | **自定义** 父层 indicator | pointer capture/leave 正确 |
| [ ] | K53 | 共享 selected layer | **自定义** Composition/VisualState | 选择切换连续移动 |
| [ ] | K54 | Indicator 重定向 | 自定义状态机/纯算法可 Core 测试 | 动画中再次选择从当前几何接续 |
| [ ] | K55 | Indicator 滚动恢复 | 自定义容器坐标同步 | 虚拟化/滚动后重新定位 |
| [ ] | K56 | Indicator 方向/RTL | Composition/XAML geometry | horizontal/vertical/RTL/不等高 item 正确 |
| [ ] | K57 | Reduce Motion | 静态提交最终几何 | 不残留动画 clock |
| [ ] | K58 | BunchedListBoxItem | 仅模板不足时自定义 container | SelectionItem UIA 保留 |
| [ ] | K59 | HoverReveal 标准控件 | VisualState/implicit transition | 不为标准 pointer state 建空壳派生类 |
| [ ] | K60 | HoverReveal 差异效果 | 必要时 Composition | danger、duration、Unload 清理 |
| [ ] | K61 | RoundedClipCoordinator | 优先删除常规路径 | 只有复杂动态裁剪保留窄 helper |
| [ ] | K62 | GridSplitter | **待选型** CommunityToolkit GridSplitter 或自定义 Thumb | 行/列、live resize、键盘、min/max |
| [!] | K63 | DataGrid 技术选型 | **待选型/高风险** WinUI 3 无第一方 DataGrid | 决策记录通过前不进入完整实现 |
| [ ] | K64 | DataGrid 列模型 | 候选库或共享 descriptor | Auto/manual、Text/Check/Combo/Template |
| [ ] | K65 | DataGrid 选择 | 候选实现原生能力优先 | 行/单元格、多选和 UIA |
| [ ] | K66 | DataGrid 编辑事务 | 候选实现 | begin/commit/cancel/validation |
| [ ] | K67 | DataGrid 排序/列宽 | 候选实现 | keyboard/pointer/状态持久化 |
| [ ] | K68 | DataGrid 行详情/冻结 | 产品需求裁剪后决定 | 不为未使用能力过度实现 |
| [ ] | K69 | DataGrid 虚拟化 | 候选实现性能验证 | 大数据、滚动、回收不残留 |
| [ ] | K70 | DataGrid 滚轮交接 | 最小自定义 chain | 表格到边界后页面继续 |
| [ ] | K71 | DataGrid AOT/trimming | 候选库验证 | 发布构建不依赖未声明反射 |
| [ ] | K72 | DataGrid 主题/本地化/UIA | 候选库适配 | Light/Dark/HC、RTL、Narrator |

DataGrid 决策规则：

1. 先冻结实际需求：编辑、排序、列调整、冻结列、行详情、虚拟化、复制和 UI Automation。
2. 简单只读表优先验证 `ListView` / `ItemsRepeater + Grid`。
3. 完整表格优先验证社区项目 WinUI.TableView，并审查许可证、维护状态和 Windows App SDK 兼容范围。
4. 旧 Windows Community Toolkit DataGrid 已归档，Toolkit 8.0+ 尚未提供 WinUI 3 DataGrid，不能作为长期默认方案。
5. Toolkit Labs 的实验控件不能直接成为稳定包基线。
6. 全量自研 DataGrid 视为独立高风险项目，不与普通控件迁移捆绑。

## 20. 资源、视觉状态、输入与无障碍

当前依据：`Themes/Colors`、`Controls.xaml`、`Generic.xaml`、`Layout.xaml`、`Typography.xaml` 和全部控件模板。

| 状态 | ID | 原子模块 | 归属 | WinUI 3 实现 / 原生性 | 验收点 |
| --- | --- | --- | --- | --- | --- |
| [ ] | R01 | 语义 Token 目录 | Core | 纯数据/文档，不共享 XAML | 颜色、字号、间距、圆角、时长稳定 |
| [ ] | R02 | WPF ResourceDictionary | UI | 保留 WPF XAML | 资源键兼容 |
| [ ] | R03 | WinUI ResourceDictionary | UI | WinUI 原生 XAML | 不复制 WPF Trigger/Setter 限制 |
| [ ] | R04 | 资源总入口 | UI | WinUI Generic.xaml/MergedDictionaries | 单一入口，重复挂载幂等 |
| [ ] | R05 | 字典图循环防护 | UI | 加载器测试 | 嵌套引用不会死循环 |
| [ ] | R06 | Light palette | UI | ThemeDictionary | 现有 58 个语义键有映射 |
| [ ] | R07 | Dark palette | UI | ThemeDictionary | 键名/类型与 Light 一致 |
| [ ] | R08 | HighContrast palette | UI | 系统 ThemeResource | 达到新增无障碍目标 |
| [ ] | R09 | Layout tokens | UI | Thickness/Double/CornerRadius | 现有 62 个间距/几何 Token 有映射 |
| [ ] | R10 | Typography tokens | UI | FontFamily/Size/Weight/line height | 现有 29 个排版 Token 有映射 |
| [ ] | R11 | 控件尺寸/圆角 | UI | lightweight styling/ThemeResource | Control/Surface/Overlay/Dialog 完整 |
| [ ] | R12 | VisualState 命名 | UI | Common/Focus/Check/Selection states | 与原生控件状态一致 |
| [ ] | R13 | 动态资源传播 | UI | ThemeResource + state applicator | 无需重建控件即可更新 |
| [ ] | R14 | 鼠标输入 | UI | Pointer 事件 | hover/press/capture/leave 正确 |
| [ ] | R15 | 触摸与笔 | UI | 原生 pointer/input | 不用 Mouse 事件替代 |
| [ ] | R16 | 键盘导航 | UI | Tab/方向键/Accelerator | 全功能无鼠标可完成 |
| [ ] | R17 | AltGr/IME | UI | 原生文本输入 | 快捷键不破坏输入 |
| [ ] | R18 | 焦点视觉 | UI | 原生 focus visual 优先 | HC/自定义模板可见 |
| [ ] | R19 | AutomationProperties | UI | 原生 UIA | name/help/labeled-by 完整 |
| [ ] | R20 | 自定义 AutomationPeer | UI | 仅无原生 peer 时自定义 | Invoke/Selection/ExpandCollapse 正确 |
| [ ] | R21 | Notification live region | UI | AutomationLiveSetting.Polite | 新通知可被读屏识别 |
| [ ] | R22 | 文本缩放 | UI | 系统文本缩放 | 200% 不裁关键文本 |
| [ ] | R23 | DPI/显示缩放 | UI | XAML scale + AppWindow conversion | 100/150/200% 正确 |
| [ ] | R24 | RTL | UI | FlowDirection + 语义 icon | 导航/返回/insets 正确 |
| [ ] | R25 | 消费者资源覆盖 | UI | MergedDictionaries/ThemeResource | 无需重模板即可换 Token |
| [ ] | R26 | 独立控件资源 | UI | 可手动加载 ThemeResources | 不启 Shell 也能使用控件 |

## 21. 扩展点与兼容面

| 状态 | ID | 原子模块 | 归属 | WinUI 3 实现 / 原生性 | 验收点 |
| --- | --- | --- | --- | --- | --- |
| [ ] | E01 | DI command parser 扩展 | Core | 不适用 | 同类型去重并按注册顺序启动 |
| [ ] | E02 | route metadata 扩展 | Core | 不适用 | 默认 key 与显式 key 规则一致 |
| [ ] | E03 | WPF Page 注册扩展 | UI | 强类型 WPF Page overload | 兼容现有调用 |
| [ ] | E04 | WinUI Page 注册扩展 | UI | 强类型 WinUI Page overload | 编译期类型约束和 DI 生命周期正确 |
| [ ] | E05 | Essential Culture bridge | Core | 独立扩展包/adapter | 首帧前及运行时同步 locale |
| [ ] | E06 | 自定义 Shell Region | Core | metadata registry | Builder/Runtime 使用同一 ID/order 语义 |
| [ ] | E07 | WPF Region content | UI | FrameworkElement factory | 兼容现有扩展 |
| [ ] | E08 | WinUI Region content | UI | UIElement/DataTemplate factory | 单 parent 和 UI thread 正确 |
| [ ] | E09 | TitleBar action 扩展 | UI | AppBarButton/Button 投影 | command key 优先，callback 有迁移策略 |
| [ ] | E10 | Footer command 扩展 | UI | 原生 Button/CommandBar element | CommandSource 和 UIA 正确 |
| [ ] | E11 | 可替换 Project behavior | Core | Dialog/Picker/FileSystem ports | UI 不拥有业务事务 |
| [ ] | E12 | 可替换 Profile auth | Core | 不适用 | Profile UI 不知道认证实现 |
| [ ] | E13 | 可替换 Secret store | Core | 契约在 Core，Windows 实现在 UI/平台包 | 测试可使用内存实现 |
| [ ] | E14 | 可替换 Dialog presenter | Core | 契约在 Core，UI 实现 | 测试无需显示窗口 |
| [ ] | E15 | 可替换 File picker | Core | 契约在 Core，UI 实现 | 取消/异常可测试 |
| [ ] | E16 | 多窗口 owner scope | UI | Window/XamlRoot registry | Dialog/Flyout/Picker 不依赖全局 MainWindow |
| [ ] | E17 | 稳定 XAML namespace | UI | 两平台分别声明映射 | 类型迁移有 obsolete 周期 |
| [ ] | E18 | API XML documentation | Core | 平台扩展各自注释 | Core 注释不出现 WPF 类型 |

## 22. Gallery、文档、测试与发布

当前依据：`src/Gallery/Views`、`docs`、`tests/Flourish.Test`、`.github/workflows/docs.yml`。

### 22.1 现有 Gallery 覆盖映射

下表确保 47 个现有页面没有因文档缺失而漏项。每组中的 WinUI 页面均应保留“初始配置、运行时修改、边界状态”三个层次。

| 状态 | 页面组 | 现有 Gallery 页面 | 对应模块 |
| --- | --- | --- | --- |
| [ ] | 应用入口 | Home、About、ControlLibrary | A、H、Q |
| [ ] | 配置/运行时 | Configuration、Appearance、MotionConfiguration、Commands、BackgroundTasks | D、T、L、C、B |
| [ ] | Shell 配置 | CustomHandlerConfiguration、DynamicToolbarConfiguration、StatusBarConfiguration、ToolTipsConfiguration | E、S、L |
| [ ] | Shell 运行时 | NavigationRuntime、RuntimeRoute、TitleBarRuntime、ToolbarStatus、WindowRuntime | N、S、W、V |
| [ ] | 项目/Profile | ProjectRuntime、ProfileConfiguration | P、U |
| [ ] | Card 类 | Card、ActionCard、CardButton、OutputCard | K23-K26、K36-K38 |
| [ ] | 内容结构 | Chunk、HeaderChunk、PageBody、Presenter、Document、CodeSpace | K27-K35、K42-K49 |
| [ ] | 选择控件 | CheckBox、RadioButton、ComboBox、ListBox、BunchedListBox、DataGrid | K06-K10、K50-K58、K63-K72 |
| [ ] | 文本/输入 | Label、TextBlock、TextBox、PasswordBox、SearchBox | K12-K18、K22 |
| [ ] | 基础按钮 | Button、WindowCaptionButton | K01-K05；caption 示例必须注明非系统实现 |
| [ ] | 布局/滚动/提示 | GridSplitter、ScrollBar、ScrollViewer、Overlay、ToolTip | K19-K21、K39-K41、K62 |

### 22.2 交付清单

| 状态 | ID | 原子模块 | 归属 | 验收点 |
| --- | --- | --- | --- | --- |
| [ ] | Q01 | Core Gallery 场景 | UI | 两平台页面消费同一 Core 服务 |
| [ ] | Q02 | Shell Gallery 场景 | UI | TitleBar/Navigation/Toolbar/Status/Window 全覆盖 |
| [ ] | Q03 | 原生控件状态页 | UI | normal/hover/pressed/focus/disabled 覆盖 |
| [ ] | Q04 | 自定义控件边界页 | UI | 空/长/大量数据、主题、响应式覆盖 |
| [ ] | Q05 | 平台能力页 | UI | AppWindow/backdrop/picker/tray 降级可见 |
| [ ] | Q06 | Core 单元测试迁移 | Core | Core 测试不启用 WPF |
| [ ] | Q07 | 跨平台契约测试 | Core | WPF/WinUI adapter 通过同一用例 |
| [ ] | Q08 | WPF 回归 | UI | 当前测试全部恢复，非预期失败为零 |
| [ ] | Q09 | WinUI XAML 架构测试 | UI | 默认样式、资源、模板可解析 |
| [ ] | Q10 | WinUI adapter 测试 | UI | Window/Frame/XamlRoot/DispatcherQueue 可控 |
| [ ] | Q11 | 导航并发/回滚测试 | Core | 乱序版本、失败回滚、取消全覆盖 |
| [ ] | Q12 | 注册租约测试 | Core | 旧租约不删除替代注册 |
| [ ] | Q13 | 持久化并发/损坏测试 | Core | 取消、重入、坏 JSON、外部修改覆盖 |
| [ ] | Q14 | 视觉基线 | UI | 关键尺寸/主题/状态有人工对照 |
| [ ] | Q15 | 无障碍验收 | UI | Narrator/Accessibility Insights 无关键错误 |
| [ ] | Q16 | WPF 文档 | UI | 标注平台和正确包名 |
| [ ] | Q17 | WinUI 文档 | UI | 原生映射、限制、示例完整 |
| [ ] | Q18 | Core 文档 | Core | 不出现平台类型 |
| [ ] | Q19 | WPF→分层迁移指南 | UI | 包、命名空间、API 替换表完整 |
| [ ] | Q20 | WPF↔WinUI 差异表 | UI | 每项有原因、替代和降级 |
| [ ] | Q21 | DocFX 多项目输入 | UI | Core/WPF/WinUI API 不互相污染 |
| [ ] | Q22 | 页面创建指南更新 | UI | PageBody/Chunk 层级和平台差异正确 |
| [ ] | Q23 | CI Core build/test | Core | 每次提交自动执行 |
| [ ] | Q24 | CI WPF build/test | UI | AnyCPU/x86/x64/ARM64 支持矩阵明确 |
| [ ] | Q25 | CI WinUI build/test | UI | 受支持架构全部构建 |
| [ ] | Q26 | CI package validation | UI | 空白消费项目可安装三个包 |
| [ ] | Q27 | packaged 验证 | UI | 安装、更新、资源、通知、picker 正确 |
| [ ] | Q28 | unpackaged 验证 | UI | bootstrap/deployment、资源、平台 API 正确 |
| [ ] | Q29 | NuGet 依赖检查 | UI | Core 无 UI 传递依赖 |
| [ ] | Q30 | API/程序集兼容 | UI | AssemblyName、pack URI、Xmlns、obsolete 周期记录 |
| [ ] | Q31 | 版本/发布说明 | Core | 破坏变化、差异、已知限制公开 |
| [ ] | Q32 | Roadmap 回写 | Core | 每个相关 PR 同步状态和证据 |

## 23. WinUI 3 原生能力决策摘要

| 领域 | 结论 | 不应做的事 |
| --- | --- | --- |
| 标题栏 | Windows App SDK 1.7+ `TitleBar`、`SetTitleBar`、`AppWindow.TitleBar`；系统负责 caption buttons | 不移植自绘 `WindowCaptionButton`，不牺牲 Snap Layout/系统菜单 |
| 窗口 | `Window.AppWindow`、`AppWindow`、`OverlappedPresenter`、`DisplayArea` | 不把 WPF Window 属性机械映射为不存在的属性 |
| 导航 | 标准模式 `NavigationView + Frame + BreadcrumbBar`；右侧可调模式原生组合 | 不用 RTL 冒充右侧导航 |
| 主题 | `RequestedTheme`、`ThemeDictionaries`、`ThemeResource` | 不硬编码 Light/Dark 颜色 |
| 材质 | `Window.SystemBackdrop`、`MicaBackdrop`、`DesktopAcrylicBackdrop` | 不迁移 WPF DWM/Accent hack 作为默认路径 |
| 对话框 | 异步 `ContentDialog` + 正确 `XamlRoot` + per-window queue | 不在 WinUI 新增同步阻塞 Dialog |
| 搜索 | `AutoSuggestBox` | 不重写 IME、suggest popup 和 clear button |
| 快捷键 | `KeyboardAccelerator`；Core 保存中立 chord | 不用全局 KeyDown 覆盖常见快捷键 |
| ToolTip | `ToolTipService`；延迟差异默认接受 | 不为视觉一致手写 timer + popup 生命周期 |
| 滚动 | `ScrollView` / `ScrollViewer` 原生输入、惯性和 chain | 不移植 WPF render-transform 平滑滚轮 |
| Picker | Windows App SDK picker + WindowId | 新基线不默认走旧 InitializeWithWindow |
| 应用内通知 | 单项 `InfoBar` + 自定义队列 host | 不与系统 AppNotification 混为一体 |
| 托盘 | 无第一方控件，WinUIEx 或隔离 `Shell_NotifyIcon` | 不只为托盘继续依赖 WinForms |
| GridSplitter | CommunityToolkit 或窄自定义 splitter | 不让第三方类型泄漏到公共 API |
| DataGrid | 无第一方等价物，先需求裁剪和选型 | 不直接全量手搓，不以归档 Toolkit 控件为长期方案 |

官方依据：

- [Windows App SDK 下载与稳定版](https://learn.microsoft.com/windows/apps/windows-app-sdk/downloads)
- [TitleBar 控件](https://learn.microsoft.com/windows/apps/develop/ui/controls/title-bar)
- [标题栏自定义](https://learn.microsoft.com/windows/apps/develop/title-bar?tabs=winui3)
- [Windowing overview](https://learn.microsoft.com/windows/apps/develop/ui/windowing-overview)
- [管理 AppWindow](https://learn.microsoft.com/windows/apps/develop/ui/manage-app-windows)
- [OverlappedPresenter](https://learn.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.windowing.overlappedpresenter)
- [NavigationView](https://learn.microsoft.com/windows/apps/develop/ui/controls/navigationview)
- [BreadcrumbBar](https://learn.microsoft.com/windows/apps/develop/ui/controls/breadcrumbbar)
- [SplitView](https://learn.microsoft.com/windows/apps/develop/ui/controls/split-view)
- [Navigation history](https://learn.microsoft.com/windows/apps/design/basics/navigation-history-and-backwards-navigation)
- [主题资源](https://learn.microsoft.com/windows/apps/develop/platform/xaml/xaml-theme-resources)
- [主题与系统强调色](https://learn.microsoft.com/windows/apps/develop/ui/theming)
- [材质与 SystemBackdrop](https://learn.microsoft.com/windows/apps/develop/ui/materials)
- [ContentDialog](https://learn.microsoft.com/windows/apps/develop/ui/controls/dialogs-and-flyouts/dialogs)
- [ToolTip](https://learn.microsoft.com/windows/apps/develop/ui/controls/tooltips)
- [Scroll controls](https://learn.microsoft.com/windows/apps/develop/ui/controls/scroll-controls)
- [键盘加速键](https://learn.microsoft.com/windows/apps/develop/input/keyboard-accelerators)
- [Access keys](https://learn.microsoft.com/windows/apps/develop/input/access-keys)
- [页面过渡](https://learn.microsoft.com/windows/apps/develop/motion/page-transitions)
- [Implicit transitions](https://learn.microsoft.com/windows/apps/develop/motion/implicit-transitions)
- [InfoBar](https://learn.microsoft.com/windows/apps/develop/ui/controls/infobar)
- [App notifications](https://learn.microsoft.com/windows/apps/develop/notifications/app-notifications/)
- [Windows App SDK Picker](https://learn.microsoft.com/windows/apps/develop/files/using-file-folder-pickers)
- [CommunityToolkit GridSplitter](https://learn.microsoft.com/dotnet/communitytoolkit/windows/sizers/gridsplitter)
- [Win32 notification area](https://learn.microsoft.com/windows/win32/shell/notification-area)
- [WinUIEx TrayIcon](https://github.com/dotMorten/WinUIEx/blob/main/docs/concepts/TrayIcon.md)
- [WPF→Windows App SDK 支持差异](https://learn.microsoft.com/windows/apps/windows-app-sdk/migrate-to-windows-app-sdk/what-is-supported)
- [已归档 Toolkit DataGrid](https://learn.microsoft.com/dotnet/communitytoolkit/archive/windows/datagrid)
- [WinUI.TableView](https://github.com/w-ahmad/WinUI.TableView)
- [PersonPicture](https://learn.microsoft.com/windows/apps/develop/ui/controls/person-picture)
- [Expander](https://learn.microsoft.com/windows/apps/develop/ui/controls/expander)
- [TeachingTip](https://learn.microsoft.com/windows/apps/develop/ui/controls/dialogs-and-flyouts/teaching-tip)
- [Windows 应用无障碍](https://learn.microsoft.com/windows/apps/develop/accessibility)

## 24. 每个原子模块的完成定义

只有同时满足下列条件，原子模块才能标记为 `[x]`：

1. Core/UI 归属符合边界，没有通过 `object`、条件编译或别名隐藏平台泄漏。
2. 已先验证 WinUI 原生能力；使用自定义实现时，决策记录解释原生能力为何不足。
3. Builder 初始配置、运行时 Service、不可变 `Current` 快照和 `Changed` 事件语义一致。
4. 资源、事件、计时器、平台句柄和注册租约均有明确释放路径。
5. 自动化测试覆盖纯逻辑、非法输入、取消、异常、重入、旧租约和竞态。
6. Gallery 提供最小示例、完整示例和运行时修改示例。
7. 手动测试覆盖对应主题、输入、DPI、无障碍和平台降级场景。
8. 中英文文档、API 注释、差异说明和本路线图同步。

## 25. 手动测试清单

实现阶段不使用 Computer Use 代替验收。每个里程碑至少人工执行并记录：

### 应用与窗口

- [ ] Windows 10 支持版本和 Windows 11 当前支持版本各运行一次。
- [ ] x64、ARM64；若继续支持则补充 x86。
- [ ] packaged/unpackaged 启动、资源、Picker 和退出。
- [ ] 单显示器、多显示器、主副屏切换及显示器断开后恢复。
- [ ] 100%、150%、200% DPI 下尺寸、标题栏 inset 和窗口持久化。
- [ ] 最小化、最大化、恢复、Topmost、任务栏显示和关闭守卫。
- [ ] 标题栏拖动、双击、右键系统菜单、Snap Layout、caption buttons。
- [ ] 关闭到托盘、托盘恢复、明确退出和 Explorer 重启后恢复。

### 主题、布局与输入

- [ ] Light、Dark、System、High Contrast 及运行时切换。
- [ ] Mica、MicaAlt、Desktop Acrylic 和实色降级。
- [ ] Reduce Motion 开关前后所有自定义动画一致响应。
- [ ] 鼠标、触控板、触摸、笔和 keyboard-only。
- [ ] Tab/Shift+Tab、方向键、Enter、Space、Esc、常用 Accelerator。
- [ ] 200% 文本缩放和长本地化文本。
- [ ] 简中、英文、缺失翻译回退和 RTL 试验语言。
- [ ] IME、AltGr 和文本编辑快捷键不被全局命令截获。

### Shell 与服务

- [ ] 首次启动、偏好恢复、损坏设置和旧版本设置迁移。
- [ ] 导航分组、命令项、固定项、Back/Forward、历史、缓存和失败回滚。
- [ ] 左侧导航、右侧可调导航、折叠/展开和快速反向动画。
- [ ] 标题栏搜索、Breadcrumb、项目选择、主题、Profile、Logo 信息。
- [ ] 页面 Toolbar、状态项增删、LAN/Power 变化和后台任务详情。
- [ ] 多通知、自动消失、手动关闭和关闭窗口竞态。
- [ ] 标准/自定义 Dialog、取消、默认按钮和多窗口 owner。
- [ ] 后台任务排队、并发、进度、取消、异常及关闭处理。
- [ ] 项目创建、保存、另存、切换、删除和无权限路径。
- [ ] Profile 登录/注销、记住凭据、名称顺序、图片损坏和自定义内容。

### 控件与无障碍

- [ ] 每个控件的 normal、pointer over、pressed、focused、disabled、selected/checked。
- [ ] 自定义模板在 Light/Dark/High Contrast 下均可辨识。
- [ ] BunchedListBox 连续选择、滚动、虚拟化、触摸、RTL 和 Reduce Motion。
- [ ] Presenter 三种模式、多元素、窗口缩放和裁剪。
- [ ] ScrollView 嵌套边界、虚拟化和 OutputCard 自动滚底。
- [ ] DataGrid 选型后检查选择、编辑、排序、列宽、虚拟化、滚轮和 UIA。
- [ ] Narrator/Accessibility Insights 检查 name、role、value、label 和焦点顺序。

## 26. 首批实施切片

在创建 Core 前先完成 R0 基线冻结；随后按以下顺序推进，避免一次移动整个 `Abstract`：

1. **基础值对象**：`IRegistration`、状态事件、集合变化、`ArgbColor`、`WindowSize`、`WindowBounds`、窗口状态和 `ShortcutChord`。
2. **纯逻辑服务**：命令、后台任务、本地化状态、设置事务、项目目录、Profile 状态、通知队列。
3. **导航逻辑**：路由、菜单事务、历史、缓存元数据；页面类型和实例仍留 WPF UI。
4. **Shell 状态**：标题栏、Toolbar、StatusBar、Region metadata、关闭守卫。
5. **外观策略**：主题、字体、材质、动效、滚动和 ToolTip 的 requested/effective 状态。
6. **WPF 回接**：让现有 WPF 项目消费 Core，恢复全部测试和 Gallery。
7. **WinUI 垂直切片**：Application、Window、Host、资源、DispatcherQueue、原生 TitleBar、一个 route 和一个 Page。
8. **WinUI Shell 与控件**：按“框架直用→原生组合→自定义→待选型”的顺序实现。

第一批不得包含 XAML 控件、`Application`、`Window`、`Page`、`FrameworkElement`、`KeyGesture`、标题栏或托盘 Win32 代码。这样 Core 边界能在最早阶段由编译器和测试验证。
