---
uid: ArkheideSystem.Flourish.Abstract
summary: Flourish 的公共应用组合与运行时契约。
---

---
uid: ArkheideSystem.Flourish.Abstract.BreadcrumbShowOption
summary: 指定标题栏中的面包屑导航何时显示。
---

---
uid: ArkheideSystem.Flourish.Abstract.BreadcrumbShowOption.Always
summary: 只要标题栏可见，就始终显示面包屑导航。
---

---
uid: ArkheideSystem.Flourish.Abstract.BreadcrumbShowOption.Auto
summary: 可以后退或前进时显示面包屑导航，否则隐藏。
---

---
uid: ArkheideSystem.Flourish.Abstract.BreadcrumbShowOption.Hidden
summary: 隐藏面包屑导航。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishBuilder
summary: 提供用于创建 Flourish 应用 builder 的工厂方法。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishBuilder.CreateDefaultBuilder(System.String[])
summary: 创建使用标准 .NET Host 默认值配置的 Flourish builder。
syntax:
  parameters:
  - id: args
    description: 传递给应用的命令行参数。
  return:
    description: 用于配置并构建 Flourish 运行时的 IFlourishBuilder。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishNavigatedEventArgs
summary: 为 Flourish 导航事件提供数据。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishNavigatedEventArgs.#ctor(System.String,System.Type,System.Windows.Controls.Page,System.Object)
summary: 初始化导航事件数据。
syntax:
  parameters:
  - id: navigationKey
    description: 已经导航到的注册键。
  - id: sourcePageType
    description: 已经导航到的已注册页面类型。
  - id: page
    description: 显示在内容框架中的页面实例。
  - id: parameter
    description: 调用方提供的可选导航参数。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishNavigatedEventArgs.NavigationKey
summary: 获取已注册的导航键。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishNavigatedEventArgs.SourcePageType
summary: 获取已经导航到的已注册页面类型。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishNavigatedEventArgs.Page
summary: 获取显示在内容框架中的页面实例。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishNavigatedEventArgs.Parameter
summary: 获取调用方提供的可选导航参数。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishNavigationPanelTransition
summary: 指定导航面板打开或关闭时使用的动画行为。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishNavigationPanelTransition.None
summary: 禁用导航面板过渡动画。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishNavigationPanelTransition.Resize
summary: 通过视觉尺寸变化为导航面板生成动画，保持达到宽度限制的 Shell 居中内容的宽度与自然横向比例，并在结束时提交最终布局宽度。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishPageCacheMode
summary: 指定 Flourish 是否缓存导航创建的页面实例。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishPageCacheMode.Enabled
summary: 页面创建后复用同一个页面实例。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishPageCacheMode.Disabled
summary: 每次导航请求都创建新的页面实例。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishPageTransition
summary: 指定页面进入内容框架时使用的动画行为。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishPageTransition.None
summary: 禁用页面过渡动画。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishPageTransition.Fade
summary: 通过淡出过渡遮罩揭示保持静止的页面。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishPageTransition.EntranceFromBottom
summary: 从底部揭示保持静止的页面。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishServiceCollectionExtensions
summary: 提供 Flourish 应用使用的服务集合扩展方法。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishServiceCollectionExtensions.AddNavigable``1(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.String,System.String,ArkheideSystem.Flourish.Abstract.FlourishPageCacheMode)
summary: 注册 WPF 页面，根据页面类名生成默认导航键，并在构建时验证键的唯一性。
syntax:
  typeParameters:
  - id: TPage
    description: 要注册的页面类型。
  parameters:
  - id: services
    description: 接收页面注册的服务集合。
  - id: displayName
    description: 页面显示为导航项时使用的名称。
  - id: iconGlyph
    description: 页面显示为导航项时使用的图标字形。
  - id: cacheMode
    description: 该页面使用的页面缓存模式。
  return:
    description: 用于链式注册的同一个服务集合。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishToolbarItem
summary: 描述显示在 Flourish Shell 中的工具栏项。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishToolbarItem.#ctor(System.String,System.String,System.String)
summary: 创建一个工具栏项描述。
syntax:
  parameters:
  - id: displayName
    description: 工具栏项显示文本。
  - id: iconGlyph
    description: 工具栏项显示的图标字形。
  - id: commandKey
    description: 通过 ICommandDispatcher 调度的可选命令键。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishToolbarItem.DisplayName
summary: 获取工具栏项显示文本。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishToolbarItem.IconGlyph
summary: 获取工具栏项显示的图标字形。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishToolbarItem.CommandKey
summary: 获取通过 ICommandDispatcher 调度的可选命令键。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourish
summary: 表示已经构建完成的 Flourish 应用运行时。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourish.Services
summary: 获取由底层 .NET Host 创建的应用服务提供程序。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourish.GetRequiredService``1
summary: 从 Flourish 服务提供程序中获取必需服务。
syntax:
  typeParameters:
  - id: T
    description: 要解析的服务类型。
  return:
    description: 已解析的服务实例。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourish.Start
summary: 启动底层应用 Host。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourish.StopAsync(System.Threading.CancellationToken)
summary: 异步停止底层应用 Host。
syntax:
  parameters:
  - id: cancellationToken
    description: 用于取消停止操作的令牌。
  return:
    description: 当 Host 完成停止时结束的任务。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourish.Show(System.Windows.Application)
summary: 为指定的 WPF 应用显示 Flourish Shell。
syntax:
  parameters:
  - id: application
    description: 拥有 Flourish Shell 的 WPF 应用。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishBuilder
summary: 在构建运行时之前配置 Flourish 应用。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishBuilder.ConfigureData(System.Action{ArkheideSystem.Flourish.Abstract.IFlourishDataBuilder})
summary: 配置 Flourish 内置界面的语言与自定义翻译文件。
syntax:
  parameters:
  - id: configureData
    description: 接收应用数据 builder 的配置回调。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishDataBuilder
summary: 配置 Flourish 使用的本地化资源。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishDataBuilder.SetLocale(System.String)
summary: 选择 Flourish 内置界面文案使用的语言。
remarks: 即使省略 ConfigureData 和 SetLocale，Flourish 也会使用内置 EN 语言。内置语言标识为 CN 和 EN，标识不区分大小写。
syntax:
  parameters:
  - id: locale
    description: 语言标识；默认为 EN。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishDataBuilder.AddLocale(System.String)
summary: 添加可扩展或覆盖内置翻译的自定义语言文件。
remarks: |
  文件在 `Build()` 应用配置时读取，必须是使用 UTF-8 编码的非空扁平 JSON 对象，并命名为 `lang_<locale>.json`。键和值必须是非空字符串，键不能重复。语言部分可以包含字母、数字、连字符和下划线。

  同一语言的多个文件按注册顺序合并，后添加文件中的同名键优先。查找顺序为：选中语言的自定义值、选中语言的内置值、自定义 `EN`、内置 `EN`，最后返回键本身。

  文件不存在时抛出 `FileNotFoundException`；文件名无效时抛出 `ArgumentException`；文件不可读或 JSON 无效时抛出 `InvalidDataException`。

  ```json
  {
    "TitleBar.Back": "上一页",
    "Tray.Show": "打开"
  }
  ```

  | 键 | EN | CN |
  | --- | --- | --- |
  | `TitleBar.Back` | Back | 返回 |
  | `TitleBar.Forward` | Forward | 前进 |
  | `TitleBar.ToggleNavigation` | Toggle navigation | 切换导航 |
  | `TitleBar.Theme` | Theme | 主题 |
  | `TitleBar.ThemeSystem` | Theme: System ({0}) | 主题：跟随系统（{0}） |
  | `TitleBar.ThemeCurrent` | Theme: {0} | 主题：{0} |
  | `TitleBar.Profile` | Profile | 个人资料 |
  | `TitleBar.ApplicationInfo` | Application information | 应用信息 |
  | `TitleBar.ProjectMenu` | Projects | 项目 |
  | `TitleBar.NewProject` | New project | 新建项目 |
  | `Project.Delete` | Delete project | 删除项目 |
  | `Project.Save` | Save | 保存 |
  | `Project.DontSave` | Don't save | 不保存 |
  | `Project.SaveDialogTitle` | Save project | 保存项目 |
  | `Project.TextFileFilter` | Text project files (*.txt)\|*.txt | 文本项目文件 (*.txt)\|*.txt |
  | `Project.UnsavedTitle` | Save project | 保存项目 |
  | `Project.UnsavedPrompt` | "{0}" has not been saved. Save it before continuing? | “{0}”尚未保存。是否先保存再继续？ |
  | `Project.DeleteTitle` | Delete project | 删除项目 |
  | `Project.DeletePrompt` | Delete "{0}"? Its managed project file is also deleted when no other project uses it. | 是否删除“{0}”？没有其他项目使用同一路径时，也会删除其受管理的项目文件。 |
  | `TitleBar.Minimize` | Minimize | 最小化 |
  | `TitleBar.Maximize` | Maximize | 最大化 |
  | `TitleBar.Restore` | Restore | 还原 |
  | `TitleBar.Close` | Close | 关闭 |
  | `Theme.Dark` | Dark | 深色 |
  | `Theme.Light` | Light | 浅色 |
  | `Profile.DefaultName` | User | 用户 |
  | `Profile.SignIn` | Sign in | 登录 |
  | `Profile.SignOut` | Sign out | 退出登录 |
  | `Profile.FirstName` | First Name | 名 |
  | `Profile.LastName` | Last Name | 姓 |
  | `Profile.Image` | Profile image | 个人资料图片 |
  | `Profile.ChooseImage` | Choose profile image | 选择个人资料图片 |
  | `Profile.UploadImage` | Upload image | 上传图片 |
  | `Profile.Password` | Password | 密码 |
  | `Profile.Cancel` | Cancel | 取消 |
  | `Profile.RememberLogin` | Remember login | 记住登录状态 |
  | `Profile.SignedIn` | Signed in | 已登录 |
  | `Profile.SignedOut` | Signed out | 未登录 |
  | `Profile.ImageFiles` | Image files | 图片文件 |
  | `Profile.AllFiles` | All files | 所有文件 |
  | `Profile.ImageLoadFailed` | The selected image could not be loaded. | 无法加载所选图片。 |
  | `Profile.SignInFailed` | Sign in failed. | 登录失败。 |
  | `Profile.EnterName` | Enter a first or last name. | 请输入名字或姓氏。 |
  | `Profile.EnterPassword` | Enter a password. | 请输入密码。 |
  | `Profile.RememberLoginRequiresSignIn` | Remember login can only be changed while a profile is signed in. | 仅可在个人资料已登录时更改记住登录状态。 |
  | `BackgroundTask.Title` | Background tasks | 后台任务 |
  | `BackgroundTask.Running` | Running | 运行中 |
  | `BackgroundTask.Queued` | Waiting | 等待中 |
  | `BackgroundTask.Cancelling` | Cancelling | 正在取消 |
  | `BackgroundTask.Cancel` | Cancel | 取消 |
  | `BackgroundTask.WaitingCount` | {0} task(s) waiting | {0} 个任务等待中 |
  | `BackgroundTask.NoActiveTasks` | No active background tasks | 没有活动的后台任务 |
  | `SystemStatus.Title` | System status | 系统状态 |
  | `SystemStatus.Network` | Network | 网络 |
  | `SystemStatus.Power` | Power | 电源 |
  | `SystemStatus.AC` | AC power | 外接电源 |
  | `SystemStatus.Battery` | Battery | 电池供电 |
  | `SystemStatus.Unknown` | Unknown | 未知 |
  | `MessageBox.OK` | OK | 确定 |
  | `MessageBox.Cancel` | Cancel | 取消 |
  | `MessageBox.Yes` | Yes | 是 |
  | `MessageBox.No` | No | 否 |
  | `Window.CloseTitle` | Close | 关闭 |
  | `Window.ClosePrompt` | Are you sure you want to close this window? | 确定要关闭此窗口吗？ |
  | `Tray.Show` | Show | 显示 |
  | `Tray.Exit` | Exit | 退出 |
  | `Status.Connected` | Connected | 已连接 |
  | `Status.Disconnected` | Disconnected | 未连接 |
syntax:
  parameters:
  - id: path
    description: 自定义语言文件路径。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishBuilder.ConfigureServices(System.Action{Microsoft.Extensions.Hosting.HostBuilderContext,Microsoft.Extensions.DependencyInjection.IServiceCollection})
summary: 向底层 .NET Host builder 添加服务注册。
syntax:
  parameters:
  - id: configureServices
    description: 接收 Host 上下文和服务集合的配置回调。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishBuilder.ConfigureShell(System.Action{ArkheideSystem.Flourish.Abstract.IFlourishShellBuilder})
summary: 配置 Flourish Shell 功能与共享选项。
syntax:
  parameters:
  - id: configureShell
    description: 接收 Shell builder 的配置回调。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishBuilder.ConfigureTitleBar(System.Action{ArkheideSystem.Flourish.Abstract.IFlourishTitlebarBuilder})
summary: 配置标题栏内容和行为。
syntax:
  parameters:
  - id: configureTitleBar
    description: 接收标题栏 builder 的配置回调。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishBuilder.ConfigureNavigation(System.Action{ArkheideSystem.Flourish.Abstract.IFlourishNavigationBuilder})
summary: 配置导航栏展示和可见导航模型。
syntax:
  parameters:
  - id: configureNavigation
    description: 接收导航 builder 的配置回调。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishBuilder.ConfigureCustomHandler(System.Action{ArkheideSystem.Flourish.Abstract.IFlourishCustomHandlerBuilder})
summary: 配置预定义 Shell 区域中的自定义 WPF 内容。
syntax:
  parameters:
  - id: configureCustomHandler
    description: 接收自定义区域 builder 的配置回调。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishBuilder.ConfigureDynamicToolbar(System.Action{ArkheideSystem.Flourish.Abstract.IFlourishDynamicToolbarBuilder})
summary: 配置按页面变化的动态工具栏项。
syntax:
  parameters:
  - id: configureToolbar
    description: 接收动态工具栏 builder 的配置回调。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishBuilder.ConfigureMotion(System.Action{ArkheideSystem.Flourish.Abstract.IFlourishMotionBuilder})
summary: 配置 Flourish 动效行为。
syntax:
  parameters:
  - id: configureMotion
    description: 接收动效 builder 的配置回调。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishBuilder.ConfigureWindow(System.Action{ArkheideSystem.Flourish.Abstract.IFlourishWindowPropertyBuilder})
summary: 配置 Flourish Shell 窗口属性。
syntax:
  parameters:
  - id: configureWindow
    description: 接收窗口属性 builder 的配置回调。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishBuilder.ConfigureStatusBar(System.Action{ArkheideSystem.Flourish.Abstract.IFlourishStatusBarBuilder})
summary: 配置 Shell 状态栏。
syntax:
  parameters:
  - id: configureStatusBar
    description: 接收状态栏 builder 的配置回调。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishBuilder.Build
summary: 构建 Flourish 运行时。
syntax:
  return:
    description: 可以启动和释放的 Flourish 运行时。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishCustomHandlerBuilder
summary: 配置显示在预定义 Shell 区域中的自定义 WPF 内容。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishCustomHandlerBuilder.Add(ArkheideSystem.Flourish.Abstract.FlourishRegion,System.Func{System.IServiceProvider,System.Windows.FrameworkElement},System.Int32)
summary: 向 Shell 区域添加自定义内容。
syntax:
  parameters:
  - id: region
    description: 接收内容的 Shell 区域。
  - id: contentFactory
    description: 创建 Shell 时用于创建 WPF 元素的服务工厂。
  - id: order
    description: 内容在区域内的显示顺序；值越小越靠前。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishCustomHandlerBuilder.SetProfileContent(System.Func{System.IServiceProvider,System.Windows.FrameworkElement})
summary: 设置标题栏 Profile 区域的自定义 WPF 内容。
remarks: 在标题栏配置中调用 `SetProfile()` 启用 Profile 区域后，该内容才会显示。
syntax:
  parameters:
  - id: contentFactory
    description: 创建 Shell 时用于创建个人资料内容的服务工厂。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishCustomHandlerBuilder.AddTitlebarAction(System.String,System.String,System.String,System.Int32)
summary: 向标题栏末尾添加命令按钮。
syntax:
  parameters:
  - id: displayName
    description: 用于工具提示和后备标签的操作文本。
  - id: iconGlyph
    description: 操作按钮显示的图标字形。
  - id: commandKey
    description: 点击操作时通过 `ICommandDispatcher` 调度的可选命令键。
  - id: order
    description: 内容在标题栏末尾区域内的显示顺序；值越小越靠前。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishCustomHandlerBuilder.AddTitlebarActionHandler(System.String,System.String,System.Action{System.IServiceProvider},System.Int32)
summary: 向标题栏末尾添加回调按钮。
syntax:
  parameters:
  - id: displayName
    description: 用于工具提示和后备标签的操作文本。
  - id: iconGlyph
    description: 操作按钮显示的图标字形。
  - id: action
    description: 点击操作时调用的回调。
  - id: order
    description: 内容在标题栏末尾区域内的显示顺序；值越小越靠前。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishCustomHandlerBuilder.AddFooterCommand(ArkheideSystem.Flourish.Abstract.FlourishRegion,System.String,System.String,System.String,System.Int32)
summary: 向指定 Shell Footer 区域添加命令按钮。
syntax:
  parameters:
  - id: region
    description: Footer 区域；必须为 `FooterStart` 或 `FooterEnd`。
  - id: displayText
    description: 命令显示文本。
  - id: iconGlyph
    description: 显示在文本前的图标字形。
  - id: commandKey
    description: 点击按钮时通过 `ICommandDispatcher` 调度的可选命令键。
  - id: order
    description: 内容在 Footer 区域内的显示顺序；值越小越靠前。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishCustomHandlerBuilder.AddFooterCommandHandler(ArkheideSystem.Flourish.Abstract.FlourishRegion,System.String,System.String,System.Action{System.IServiceProvider},System.Int32)
summary: 向指定 Shell Footer 区域添加回调按钮。
syntax:
  parameters:
  - id: region
    description: Footer 区域；必须为 `FooterStart` 或 `FooterEnd`。
  - id: displayText
    description: 命令显示文本。
  - id: iconGlyph
    description: 显示在文本前的图标字形。
  - id: action
    description: 点击按钮时调用的回调。
  - id: order
    description: 内容在 Footer 区域内的显示顺序；值越小越靠前。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishDynamicToolbarBuilder
summary: 配置会随当前页面变化的工具栏项。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishDynamicToolbarBuilder.CreateToolbarItems``1(ArkheideSystem.Flourish.Abstract.FlourishToolbarItem[])
summary: 为指定泛型页面类型创建工具栏项。
syntax:
  typeParameters:
  - id: TPage
    description: 与工具栏项关联的页面类型。
  parameters:
  - id: items
    description: 为该页面显示的工具栏项。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishDynamicToolbarBuilder.CreateToolbarItems``1(System.Boolean,ArkheideSystem.Flourish.Abstract.FlourishToolbarItem[])
summary: 为指定泛型页面类型创建工具栏项，并控制是否显示工具栏图标。
syntax:
  typeParameters:
  - id: TPage
    description: 与工具栏项关联的页面类型。
  parameters:
  - id: icon
    description: 指示是否显示工具栏项图标。
  - id: items
    description: 为该页面显示的工具栏项。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishMotionBuilder
summary: 配置 Flourish Shell 的动效和动画行为。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishMotionBuilder.EnablePageTransition(ArkheideSystem.Flourish.Abstract.FlourishPageTransition,System.Nullable{System.TimeSpan})
summary: 启用页面进入内容框架时使用的过渡效果。
syntax:
  parameters:
  - id: transition
    description: 要使用的页面过渡效果。
  - id: duration
    description: 页面过渡使用的持续时间。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishMotionBuilder.EnableNavigationPanelTransition(ArkheideSystem.Flourish.Abstract.FlourishNavigationPanelTransition,System.Nullable{System.TimeSpan})
summary: 启用导航面板打开或关闭时使用的过渡效果。
syntax:
  parameters:
  - id: transition
    description: 要使用的导航面板过渡效果。
  - id: duration
    description: 导航面板过渡使用的持续时间。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishMotionBuilder.EnableHoverRevealAnimation(System.Nullable{System.TimeSpan})
summary: 启用悬停揭示动画。
syntax:
  parameters:
  - id: duration
    description: 悬停揭示动画使用的持续时间。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishMotionBuilder.RespectSystemReducedMotion(System.Boolean)
summary: 控制 Flourish 是否遵循操作系统的减少动态效果偏好。
syntax:
  parameters:
  - id: enabled
    description: 指示是否遵循系统减少动态效果偏好。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishNavigationBuilder
summary: 配置 Flourish 导航栏展示和可见导航模型。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishNavigationBuilder.SetDirection(ArkheideSystem.Flourish.Abstract.NavigationPanelDirection)
summary: 设置导航面板显示在 Shell 的哪一侧。
syntax:
  parameters:
  - id: direction
    description: 导航面板方向。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishNavigationBuilder.SetInitiallyOpen(System.Boolean)
summary: 设置 Shell 首次显示时导航面板是否打开。
syntax:
  parameters:
  - id: enabled
    description: 指示导航面板是否初始打开。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishNavigationBuilder.SetPanelWidth(System.Double,System.Double,System.Double,System.Double)
summary: 设置导航栏宽度和 splitter 调整范围。
syntax:
  parameters:
  - id: openWidth
    description: 导航栏展开宽度。
  - id: closedWidth
    description: 导航栏折叠宽度。设为 0 会完全隐藏折叠导航栏；其他值不得小于 64 DIP。
  - id: maxWidth
    description: 导航栏最大展开宽度。
  - id: minWidth
    description: 导航栏最小展开宽度。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishNavigationBuilder.SetGroup(System.String,System.Int32,System.Action{ArkheideSystem.Flourish.Abstract.IFlourishNavigationGroupBuilder})
summary: 添加并配置一个可滚动导航分组。
syntax:
  parameters:
  - id: displayName
    description: 分组标题。groupId 不为 0 时必须提供。
  - id: groupId
    description: 唯一分组 ID。数值越小显示越靠前，默认值为 0。
  - id: configureGroup
    description: 用于添加分组内导航项的配置回调。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishNavigationBuilder.AddFixedNavigableViewItem``1(System.Boolean,System.Int32,System.Int32)
summary: 在导航栏底部固定区域添加一个已注册页面导航项。
syntax:
  typeParameters:
  - id: TPage
    description: 要显示的已注册页面类型。
  parameters:
  - id: isInitial
    description: 指示该页面是否作为 Shell 打开后的初始页面。
  - id: parentId
    description: 可选父节点 ID。childId 不为 0 时必须为 0。
  - id: childId
    description: 可选父节点归属 ID。parentId 不为 0 时必须为 0。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishNavigationBuilder.AddFixedNavigableItem(System.String,System.String,System.String,System.Int32,System.Int32)
summary: 在导航栏底部固定区域添加一个按钮类型命令项。
syntax:
  parameters:
  - id: displayName
    description: 命令项显示文本。
  - id: iconGlyph
    description: 命令项显示的图标字形；传入 null 可省略图标。
  - id: commandKey
    description: 触发时通过 ICommandDispatcher 调度的命令键；仅作为父节点时传入 null。
  - id: parentId
    description: 可选父节点 ID。childId 不为 0 时必须为 0。
  - id: childId
    description: 可选父节点归属 ID。parentId 不为 0 时必须为 0。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishNavigationGroupBuilder
summary: 配置 Flourish 导航分组中显示的导航项。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishNavigationGroupBuilder.AddNavigableViewItem``1(System.Boolean,System.Int32,System.Int32)
summary: 将一个已注册 WPF 页面添加到当前导航分组。
syntax:
  typeParameters:
  - id: TPage
    description: 要显示的已注册页面类型。
  parameters:
  - id: isInitial
    description: 指示该页面是否作为 Shell 打开后的初始页面。
  - id: parentId
    description: 可选父节点 ID。childId 不为 0 时必须为 0。
  - id: childId
    description: 可选父节点归属 ID。parentId 不为 0 时必须为 0。
  return:
    description: 用于链式配置的当前分组 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishNavigationGroupBuilder.AddNavigableItem(System.String,System.String,System.String,System.Int32,System.Int32)
summary: 将一个按钮类型命令项添加到当前导航分组。
syntax:
  parameters:
  - id: displayName
    description: 命令项显示文本。
  - id: iconGlyph
    description: 命令项显示的图标字形；传入 null 可省略图标。
  - id: commandKey
    description: 触发时通过 ICommandDispatcher 调度的命令键；仅作为父节点时传入 null。
  - id: parentId
    description: 可选父节点 ID。childId 不为 0 时必须为 0。
  - id: childId
    description: 可选父节点归属 ID。parentId 不为 0 时必须为 0。
  return:
    description: 用于链式配置的当前分组 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishShellBuilder
summary: 配置 Flourish Shell 功能与共享选项。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishThemeColors
summary: 定义用于生成 Flourish 主题资源的应用颜色。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishThemeColors.#ctor(System.Windows.Media.Color,System.Windows.Media.Color,System.Windows.Media.Color)
summary: 使用三种不透明颜色创建主题颜色配置。
syntax:
  parameters:
  - id: primary
    description: 主要操作颜色。
  - id: secondary
    description: 辅助颜色。
  - id: accent
    description: 强调内容使用的颜色。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishThemeColors.Primary
summary: 获取主要操作颜色。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishThemeColors.Secondary
summary: 获取辅助颜色。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishThemeColors.Accent
summary: 获取强调内容使用的颜色。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishShellBuilder.UseTitleBar(System.Boolean)
summary: 启用或禁用 Shell 标题栏。
syntax:
  parameters:
  - id: enabled
    description: 指示是否启用标题栏。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishShellBuilder.UseMultiProject(System.Boolean)
summary: 启用或禁用标题栏的多项目显示语义；调用时默认启用，项目目录本身始终可用。
syntax:
  parameters:
  - id: enabled
    description: 是否让标题选择器显示活动项目，并在下拉列表中列出全部项目与“新建项目”。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishShellBuilder.UseNavigation(System.Boolean)
summary: 启用或禁用 Shell 导航栏。
syntax:
  parameters:
  - id: enabled
    description: 指示是否启用导航栏。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishShellBuilder.UseCenterContent(System.Boolean,System.Double)
summary: 配置导航页面内容及对齐的 Shell 内容区域是否在宽视口和导航栏过渡期间按最大宽度居中显示。
syntax:
  parameters:
  - id: enabled
    description: 指示是否启用导航页面内容的最大宽度限制与居中布局。
  - id: contentWidth
    description: 导航页面内容的最大宽度，以设备无关像素为单位。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishShellBuilder.UseDynamicToolbar(System.Boolean)
summary: 启用或禁用动态工具栏区域。
syntax:
  parameters:
  - id: enabled
    description: 指示是否启用动态工具栏。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishShellBuilder.UseTips(System.Int32)
summary: 设置初始显示延迟并启用 Flourish 工具提示；提示间距使用内置默认值。
syntax:
  parameters:
  - id: delay
    description: 工具提示的初始显示延迟（毫秒）。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishShellBuilder.UseMotion(System.Boolean)
summary: 启用或禁用 Flourish 动效。
syntax:
  parameters:
  - id: enabled
    description: 指示是否启用动效。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishShellBuilder.UseMaterialEffect(ArkheideSystem.Flourish.Abstract.MaterialEffect)
summary: 设置 Shell 窗口的系统材质效果；`None` 禁用材质，其他值启用对应效果。
syntax:
  parameters:
  - id: effect
    description: 应用于 Shell 窗口的材质效果。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishShellBuilder.UseThemeColors(ArkheideSystem.Flourish.Abstract.FlourishThemeColors)
summary: 设置 Flourish 主题资源使用的主要、辅助和强调颜色。
syntax:
  parameters:
  - id: colors
    description: 应用的主题颜色配置；三种颜色必须完全不透明。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishShellBuilder.UseCornerRadius(System.Double)
summary: 设置 Flourish 控件与 Shell 区域共用的圆角半径。
syntax:
  parameters:
  - id: radius
    description: 以设备无关像素为单位的有限非负半径；`0` 表示直角。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishShellBuilder.UseGlobalFont(System.String,System.Double,System.Double,System.Double,System.Double,System.Double,System.Double)
summary: 设置 Flourish Shell UI 使用的全局字体与 Small、Standard、Icon、Large、ExtraLarge 和 HeaderSize 六种字号。
syntax:
  parameters:
  - id: fontFamily
    description: 字体系列名称。
  - id: smallFontSize
    description: Small 字号。
  - id: standardFontSize
    description: 标准字号。
  - id: iconFontSize
    description: Icon 字号。
  - id: largeFontSize
    description: Large 字号。
  - id: extraLargeFontSize
    description: ExtraLarge 字号。
  - id: headerSizeFontSize
    description: HeaderSize 字号。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishShellBuilder.UseStatusBar(System.Boolean)
summary: 启用或禁用 Shell 状态栏。
syntax:
  parameters:
  - id: enabled
    description: 指示是否启用状态栏。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishStatusBarBuilder
summary: 配置 Flourish Shell 状态栏。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishStatusBarBuilder.AddStatusItem(System.String,System.String)
summary: 添加包含显示文本和图标字形的状态栏项目。
syntax:
  parameters:
  - id: displayText
    description: 状态项显示文本。
  - id: iconGlyph
    description: 显示在文本前的图标字形。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishStatusBarBuilder.ShowLANConnectionStatus
summary: 在合并的系统状态浮层中启用网络详情；浮层打开时读取当前网络可用性。
syntax:
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishStatusBarBuilder.ShowPowerStatus
summary: 在合并的系统状态浮层中启用电源来源和可用电池百分比。
syntax:
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishTitlebarBuilder
summary: 配置 Flourish Shell 标题栏；调用成员时会同时显示对应元素，未配置的元素保持隐藏。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishTitlebarBuilder.SetSearch(System.String,System.Action{System.IServiceProvider,System.String})
summary: 设置占位文本和可访问应用服务的文本变化回调，并显示搜索框。
syntax:
  parameters:
  - id: placeholder
    description: 搜索框中显示的占位文本。
  - id: handler
    description: 搜索文本变化时接收应用服务提供程序和新文本的回调。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishTitlebarBuilder.SetBreadcrumbButton(ArkheideSystem.Flourish.Abstract.BreadcrumbShowOption)
summary: 设置面包屑显示行为，并启用面包屑按钮。
syntax:
  parameters:
  - id: option
    description: 面包屑的显示行为。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishTitlebarBuilder.SetNavToggle
summary: 显示导航面板切换按钮。
syntax:
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishTitlebarBuilder.SetLogo(System.String,System.Boolean,System.Boolean,System.Boolean)
summary: 设置 Logo 按钮及其信息视图中显示的应用与项目标识；省略路径时使用 Flourish 内置图标。
syntax:
  parameters:
  - id: logoPath
    description: Logo 图像的相对 URI、绝对 URI 或 WPF pack URI；省略时使用内置图标。
  - id: showApplicationTitle
    description: 是否在 Logo 信息视图中显示应用标题；默认为 true。
  - id: showApplicationSubTitle
    description: 是否在 Logo 信息视图中显示应用副标题；默认为 true。
  - id: showProjectTitle
    description: 是否在 Logo 信息视图中显示当前项目标题；默认为 false。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishTitlebarBuilder.SetApplicationTitle(System.String)
summary: 设置应用标题并显示标题选择器；未启用多项目时，选择器只显示该标题。
syntax:
  parameters:
  - id: title
    description: 应用的非空显示标题。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishTitlebarBuilder.SetApplicationSubTitle(System.String)
summary: 设置在 Logo 信息视图中显示的应用副标题。
syntax:
  parameters:
  - id: subTitle
    description: 应用的非空辅助标题。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishTitlebarBuilder.SetUnnamedProjectPlaceholder(System.String)
summary: 设置未持久化项目或缺少活动选择时使用的显示占位文本。
syntax:
  parameters:
  - id: placeholder
    description: 非空占位文本；默认为 Unnamed project。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishTitlebarBuilder.SetProfile(ArkheideSystem.Flourish.Abstract.NameOrder)
summary: 使用内置默认资料设置名称顺序，并显示 Profile 入口。
syntax:
  parameters:
  - id: nameOrder
    description: Profile 名称和首字母的显示顺序。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishTitlebarBuilder.SetThemeToggle(ArkheideSystem.Flourish.Abstract.FlourishTheme)
summary: 设置尚无已保存偏好时使用的主题，并显示主题切换按钮。
syntax:
  parameters:
  - id: mode
    description: 尚无已保存偏好时使用的主题模式。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishWindowPropertyBuilder
summary: 配置 Flourish Shell 窗口。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishWindowPropertyBuilder.SetWindowSize(System.Double,System.Double)
summary: 设置 Shell 窗口初始尺寸。
syntax:
  parameters:
  - id: width
    description: 初始窗口宽度。
  - id: height
    description: 初始窗口高度。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishWindowPropertyBuilder.SetWindowMinSize(System.Double,System.Double)
summary: 设置 Shell 窗口最小尺寸。
syntax:
  parameters:
  - id: minWidth
    description: 最小窗口宽度。
  - id: minHeight
    description: 最小窗口高度。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishWindowPropertyBuilder.SetWindowMaxSize(System.Double,System.Double)
summary: 设置 Shell 窗口最大尺寸。
syntax:
  parameters:
  - id: maxWidth
    description: 最大窗口宽度。
  - id: maxHeight
    description: 最大窗口高度。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishWindowPropertyBuilder.SetWindowPosition(System.Windows.WindowStartupLocation)
summary: 设置 Shell 窗口启动位置。
syntax:
  parameters:
  - id: startupLocation
    description: Shell 窗口使用的 WPF 启动位置。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishWindowPropertyBuilder.SetManualWindowPosition(System.Double,System.Double)
summary: 设置 Shell 窗口的手动位置。
syntax:
  parameters:
  - id: left
    description: Shell 窗口的左侧坐标。
  - id: top
    description: Shell 窗口的顶部坐标。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishWindowPropertyBuilder.SetWindowState(System.Windows.WindowState)
summary: 设置 Shell 窗口初始状态。
syntax:
  parameters:
  - id: windowState
    description: 初始 WPF 窗口状态。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishWindowPropertyBuilder.SetWindowResizeMode(System.Windows.ResizeMode)
summary: 设置 Shell 窗口调整大小模式。
syntax:
  parameters:
  - id: resizeMode
    description: Shell 窗口使用的 WPF 调整大小模式。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishWindowPropertyBuilder.UseTopmost(System.Boolean)
summary: 设置 Shell 窗口是否保持在其他窗口上方。
syntax:
  parameters:
  - id: enabled
    description: 指示是否启用置顶行为。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishWindowPropertyBuilder.ShowInTaskbar(System.Boolean)
summary: 设置 Shell 窗口是否显示在 Windows 任务栏中。
syntax:
  parameters:
  - id: enabled
    description: 指示窗口是否显示在任务栏中。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishWindowPropertyBuilder.SetTrayExit(System.Boolean)
summary: 设置标题栏关闭按钮是否将 Shell 窗口隐藏到 Windows 通知区域。
remarks: 启用后，关闭按钮不会显示退出确认；可以通过通知区域菜单恢复窗口或退出应用。禁用后，关闭按钮使用常规退出确认流程。
syntax:
  parameters:
  - id: enabled
    description: 指示是否启用关闭到通知区域的行为。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.INavigationService
summary: 为已注册的 Flourish 页面提供运行时导航服务。
---

---
uid: ArkheideSystem.Flourish.Abstract.INavigationService.Navigated
summary: Flourish 导航到已注册页面后触发。
---

---
uid: ArkheideSystem.Flourish.Abstract.INavigationService.CanGoBack
summary: 获取是否可以执行后退导航。
---

---
uid: ArkheideSystem.Flourish.Abstract.INavigationService.CanGoForward
summary: 获取是否可以执行前进导航。
---

---
uid: ArkheideSystem.Flourish.Abstract.INavigationService.CurrentSourcePageType
summary: 获取当前显示在内容框架中的已注册源页面类型。
---

---
uid: ArkheideSystem.Flourish.Abstract.INavigationService.CurrentNavigationKey
summary: 获取当前导航键。
---

---
uid: ArkheideSystem.Flourish.Abstract.INavigationService.Navigate(System.String,System.Object,System.Boolean)
summary: 使用区分大小写的已注册导航键执行导航。
syntax:
  parameters:
  - id: navigationKey
    description: 区分大小写的已注册导航键。
  - id: parameter
    description: 传递给目标页面的可选参数。
  - id: addToBackStack
    description: 指示是否将当前页面加入后退栈。
  return:
    description: 如果导航成功，则为 true；否则为 false。
---

---
uid: ArkheideSystem.Flourish.Abstract.INavigationService.GoBack
summary: 导航到后退栈中的上一页。
syntax:
  return:
    description: 如果后退导航成功，则为 true；否则为 false。
---

---
uid: ArkheideSystem.Flourish.Abstract.INavigationService.GoForward
summary: 导航到前进栈中的下一页。
syntax:
  return:
    description: 如果前进导航成功，则为 true；否则为 false。
---

---
uid: ArkheideSystem.Flourish.Abstract.INavigationService.ClearBackStack
summary: 清空导航后退栈。
---

---
uid: ArkheideSystem.Flourish.Abstract.MaterialEffect
summary: 指定应用到 Flourish Shell 窗口的系统材质效果。
---

---
uid: ArkheideSystem.Flourish.Abstract.MaterialEffect.None
summary: 不应用系统材质效果。
---

---
uid: ArkheideSystem.Flourish.Abstract.MaterialEffect.Mica
summary: 在支持时应用 Windows Mica 材质效果。
---

---
uid: ArkheideSystem.Flourish.Abstract.NavigationPanelDirection
summary: 指定导航面板显示在 Shell 的哪一侧。
---

---
uid: ArkheideSystem.Flourish.Abstract.NavigationPanelDirection.Left
summary: 在 Shell 左侧显示导航面板。
---

---
uid: ArkheideSystem.Flourish.Abstract.NavigationPanelDirection.Right
summary: 在 Shell 右侧显示导航面板。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishBuilder.ConfigureProfile(System.Action{ArkheideSystem.Flourish.Abstract.IFlourishProfileBuilder})
summary: 配置 Profile 弹层承载的页面。
syntax:
  parameters:
  - id: configureProfile
    description: 接收 Profile builder 的配置回调。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishProfileBuilder
summary: 配置由 SetProfile 启用的 Profile 弹层所承载的页面。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishProfileBuilder.SetProfilePage``1
summary: 设置由 DI 解析并承载在 Profile 弹层中的 WPF 页面。
syntax:
  typeParameters:
  - id: TPage
    description: 由依赖注入解析的 WPF 页面类型。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.NameOrder
summary: 指定 Profile first name 与 last name 的显示顺序。
---

---
uid: ArkheideSystem.Flourish.Abstract.NameOrder.FirstLast
summary: 先显示 first name，再显示 last name；占位首字母采用相同顺序。
---

---
uid: ArkheideSystem.Flourish.Abstract.NameOrder.LastFirst
summary: 先显示 last name，再显示 first name；占位首字母采用相同顺序。
---

---
uid: ArkheideSystem.Flourish.Abstract.ProfileLoginState
summary: 表示 Profile 当前的登录状态。
---

---
uid: ArkheideSystem.Flourish.Abstract.ProfileLoginState.SignedOut
summary: 当前没有用户登录。
---

---
uid: ArkheideSystem.Flourish.Abstract.ProfileLoginState.SignedIn
summary: 用户已登录，但仅在当前应用会话中保持。
---

---
uid: ArkheideSystem.Flourish.Abstract.ProfileLoginState.SignedInRemembered
summary: 用户已登录，并将在下次启动时恢复登录。
---

---
uid: ArkheideSystem.Flourish.Abstract.ProfileUser
summary: 表示 Profile 中显示的用户信息。
---

---
uid: ArkheideSystem.Flourish.Abstract.ProfileUser.#ctor(System.String,System.String,ArkheideSystem.Flourish.Abstract.NameOrder,System.String)
summary: 使用独立的 first name、last name、名称顺序和可选图片路径创建 Profile 用户。
syntax:
  parameters:
  - id: firstName
    description: 用户的 first name；可为空，但不能与 lastName 同时为空。
  - id: lastName
    description: 用户的 last name；可为空，但不能与 firstName 同时为空。
  - id: nameOrder
    description: 显示名称和占位首字母所使用的顺序。
  - id: imagePath
    description: 可为空的本地图片路径或 pack URI。
---

---
uid: ArkheideSystem.Flourish.Abstract.ProfileUser.FirstName
summary: 获取用户的 first name。
---

---
uid: ArkheideSystem.Flourish.Abstract.ProfileUser.LastName
summary: 获取用户的 last name。
---

---
uid: ArkheideSystem.Flourish.Abstract.ProfileUser.NameOrder
summary: 获取显示名称和占位首字母所使用的名称顺序。
---

---
uid: ArkheideSystem.Flourish.Abstract.ProfileUser.DisplayName
summary: 获取按照 NameOrder 格式化后的 Profile 显示名称。
---

---
uid: ArkheideSystem.Flourish.Abstract.ProfileUser.ImagePath
summary: 获取可选的 Profile 图片路径。
---

---
uid: ArkheideSystem.Flourish.Abstract.ProfileUser.Initials
summary: 获取图片不可用时按照 NameOrder 显示的名称首字母。
---

---
uid: ArkheideSystem.Flourish.Abstract.ProfileSignInRequest
summary: 包含登录时提交的结构化名称、名称顺序、密码和可选图片路径。
---

---
uid: ArkheideSystem.Flourish.Abstract.ProfileSignInRequest.#ctor(System.String,System.String,System.String,ArkheideSystem.Flourish.Abstract.NameOrder,System.String)
summary: 使用独立的 first name、last name、密码、名称顺序和可选图片路径创建 Profile 登录请求。
syntax:
  parameters:
  - id: firstName
    description: 用户提交的 first name。
  - id: lastName
    description: 用户提交的 last name。
  - id: password
    description: 用户提交的密码。
  - id: nameOrder
    description: 显示名称所使用的顺序。
  - id: imagePath
    description: 可选的 Profile 图片路径。
---

---
uid: ArkheideSystem.Flourish.Abstract.ProfileSignInRequest.FirstName
summary: 获取提交的 first name。
---

---
uid: ArkheideSystem.Flourish.Abstract.ProfileSignInRequest.LastName
summary: 获取提交的 last name。
---

---
uid: ArkheideSystem.Flourish.Abstract.ProfileSignInRequest.NameOrder
summary: 获取提交的名称顺序。
---

---
uid: ArkheideSystem.Flourish.Abstract.ProfileSignInRequest.DisplayName
summary: 获取按照 NameOrder 格式化后的提交显示名称。
---

---
uid: ArkheideSystem.Flourish.Abstract.ProfileSignInRequest.Password
summary: 获取提交的密码。
---

---
uid: ArkheideSystem.Flourish.Abstract.ProfileSignInRequest.ImagePath
summary: 获取可选的提交图片路径。
---

---
uid: ArkheideSystem.Flourish.Abstract.ProfileAuthenticationResult
summary: 表示一次 Profile 认证的结果。
---

---
uid: ArkheideSystem.Flourish.Abstract.ProfileAuthenticationResult.Succeeded
summary: 获取认证是否成功。
---

---
uid: ArkheideSystem.Flourish.Abstract.ProfileAuthenticationResult.ErrorMessage
summary: 获取可选的认证失败消息。
---

---
uid: ArkheideSystem.Flourish.Abstract.ProfileAuthenticationResult.Success
summary: 创建成功的认证结果。
---

---
uid: ArkheideSystem.Flourish.Abstract.ProfileAuthenticationResult.Failure(System.String)
summary: 创建包含错误消息的失败认证结果。
syntax:
  parameters:
  - id: errorMessage
    description: 面向用户的认证失败消息。
  return:
    description: 失败的认证结果。
---

---
uid: ArkheideSystem.Flourish.Abstract.ProfileChangedEventArgs
summary: 提供 Profile 用户或登录状态发生变化时的数据。
---

---
uid: ArkheideSystem.Flourish.Abstract.ProfileChangedEventArgs.Profile
summary: 获取当前 Profile 用户信息。
---

---
uid: ArkheideSystem.Flourish.Abstract.ProfileChangedEventArgs.LoginState
summary: 获取当前登录状态。
---

---
uid: ArkheideSystem.Flourish.Abstract.IProfileAuthService
summary: 定义可由应用替换的 Profile 认证逻辑。
---

---
uid: ArkheideSystem.Flourish.Abstract.IProfileAuthService.AuthenticateAsync(ArkheideSystem.Flourish.Abstract.ProfileSignInRequest,System.Threading.CancellationToken)
summary: 异步认证给定的 Profile 登录请求。
syntax:
  parameters:
  - id: request
    description: Profile 登录请求。
  - id: cancellationToken
    description: 请求取消操作的令牌。
  return:
    description: 包含认证结果的任务。
---

---
uid: ArkheideSystem.Flourish.Abstract.IProfileAuthService.SignOutAsync(ArkheideSystem.Flourish.Abstract.ProfileUser,System.Threading.CancellationToken)
summary: 执行认证提供程序所需的异步登出工作。
syntax:
  parameters:
  - id: profile
    description: 正在登出的 Profile 用户。
  - id: cancellationToken
    description: 请求取消操作的令牌。
  return:
    description: 登出工作完成时结束的任务。
---

---
uid: ArkheideSystem.Flourish.Abstract.IProfileService
summary: 维护当前 Profile 用户、登录状态和持久化流程。
---

---
uid: ArkheideSystem.Flourish.Abstract.IProfileService.CurrentProfile
summary: 获取 Shell 当前显示的 Profile 用户。
---

---
uid: ArkheideSystem.Flourish.Abstract.IProfileService.LoginState
summary: 获取当前 Profile 登录状态。
---

---
uid: ArkheideSystem.Flourish.Abstract.IProfileService.ProfileChanged
summary: 当 Profile 用户或登录状态变化时发生。
---

---
uid: ArkheideSystem.Flourish.Abstract.IProfileService.InitializeAsync(System.Threading.CancellationToken)
summary: 初始化 Profile 状态，并在存在已存储凭据时恢复已记住的登录。
syntax:
  parameters:
  - id: cancellationToken
    description: 请求取消操作的令牌。
  return:
    description: 初始化完成时结束的任务。
---

---
uid: ArkheideSystem.Flourish.Abstract.IProfileService.SignInAsync(ArkheideSystem.Flourish.Abstract.ProfileSignInRequest,System.Threading.CancellationToken)
summary: 异步认证并激活当前 Profile。
syntax:
  parameters:
  - id: request
    description: Profile 登录请求。
  - id: cancellationToken
    description: 请求取消操作的令牌。
  return:
    description: 包含认证结果的任务。
---

---
uid: ArkheideSystem.Flourish.Abstract.IProfileService.SetRememberLoginAsync(System.Boolean,System.Threading.CancellationToken)
summary: 设置是否在下次启动时恢复当前登录。
syntax:
  parameters:
  - id: rememberLogin
    description: 指示是否在下次启动时恢复当前登录。
  - id: cancellationToken
    description: 请求取消操作的令牌。
  return:
    description: 设置应用完成时结束的任务。
---

---
uid: ArkheideSystem.Flourish.Abstract.IProfileService.SignOutAsync(System.Threading.CancellationToken)
summary: 异步登出并删除持久化的 Profile 凭据。
syntax:
  parameters:
  - id: cancellationToken
    description: 请求取消操作的令牌。
  return:
    description: 登出完成时结束的任务。
---

---
uid: ArkheideSystem.Flourish.Abstract.IBackgroundTaskService
summary: 提交并执行具有并发上限的异步后台任务。
---

---
uid: ArkheideSystem.Flourish.Abstract.IBackgroundTaskService.MaxConcurrency
summary: 获取可同时运行的最大任务数量。
---

---
uid: ArkheideSystem.Flourish.Abstract.IBackgroundTaskService.ActiveTasks
summary: 获取所有等待中、运行中和正在取消任务的不可变快照。
---

---
uid: ArkheideSystem.Flourish.Abstract.IBackgroundTaskService.TasksChanged
summary: 当活动任务集合、任务状态或任务进度发生变化时触发。
---

---
uid: ArkheideSystem.Flourish.Abstract.IBackgroundTaskService.AddTask(ArkheideSystem.Flourish.Abstract.FlourishBackgroundTaskMetadata,System.Func{ArkheideSystem.Flourish.Abstract.FlourishBackgroundTaskContext,System.Threading.Tasks.ValueTask})
summary: 提交一个没有返回值的异步后台任务。
syntax:
  parameters:
  - id: metadata
    description: Shell 显示任务时使用的元信息。
  - id: task
    description: 接收取消与进度上下文的异步任务委托。
  return:
    description: 用于取消任务并等待捕获结果的 handle。
---

---
uid: ArkheideSystem.Flourish.Abstract.IBackgroundTaskService.AddTask``1(ArkheideSystem.Flourish.Abstract.FlourishBackgroundTaskMetadata,System.Func{ArkheideSystem.Flourish.Abstract.FlourishBackgroundTaskContext,System.Threading.Tasks.ValueTask{``0}})
summary: 提交一个产生返回值的异步后台任务。
syntax:
  parameters:
  - id: metadata
    description: Shell 显示任务时使用的元信息。
  - id: task
    description: 接收取消与进度上下文并产生值的异步任务委托。
  return:
    description: 用于取消任务并等待捕获结果与返回值的泛型 handle。
---

---
uid: ArkheideSystem.Flourish.Abstract.IBackgroundTaskService.CancelTask(System.Guid)
summary: 使用任务标识符请求协作式取消。
syntax:
  parameters:
  - id: taskId
    description: 要取消的活动任务标识符。
  return:
    description: 本次调用改变任务状态时为 `true`；否则为 `false`。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishBackgroundTaskContext
summary: 向后台任务提供协作式取消和进度报告能力。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishBackgroundTaskContext.CancellationToken
summary: 获取任务取消或应用 Host 停止时收到取消请求的 token。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishBackgroundTaskContext.ReportProgress(System.Double)
summary: 报告从 0 到 1 的有限进度值。
syntax:
  parameters:
  - id: progress
    description: 已完成比例，取值范围包含 0 和 1。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishBackgroundTaskMetadata
summary: 描述提交前的后台任务及其 Shell 展示信息。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishBackgroundTaskMetadata.#ctor(System.String,System.String,System.String)
summary: 使用名称、可选描述和可选图标字形创建任务元信息。
syntax:
  parameters:
  - id: name
    description: 向用户显示的非空任务名称。
  - id: description
    description: 可选任务描述。
  - id: iconGlyph
    description: 可选任务图标字形。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishBackgroundTaskMetadata.Name
summary: 获取向用户显示的任务名称。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishBackgroundTaskMetadata.Description
summary: 获取可选任务描述。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishBackgroundTaskMetadata.IconGlyph
summary: 获取可选任务图标字形。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishBackgroundTaskHandle
summary: 控制并观察没有返回值的已提交后台任务。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishBackgroundTaskHandle.Id
summary: 获取唯一任务标识符。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishBackgroundTaskHandle.Completion
summary: 获取始终正常完成并携带捕获结果的任务。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishBackgroundTaskHandle.Snapshot
summary: 获取任务的最新不可变快照。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishBackgroundTaskHandle.Cancel
summary: 请求协作式取消当前任务。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishBackgroundTaskHandle`1
summary: 控制并观察产生返回值的已提交后台任务。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishBackgroundTaskHandle`1.Id
summary: 获取唯一任务标识符。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishBackgroundTaskHandle`1.Completion
summary: 获取始终正常完成并携带捕获结果与返回值的任务。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishBackgroundTaskHandle`1.Snapshot
summary: 获取任务的最新不可变快照。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishBackgroundTaskHandle`1.Cancel
summary: 请求协作式取消当前任务。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishBackgroundTaskInfo
summary: 提供后台任务在某一时刻的不可变快照。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishBackgroundTaskInfo.Id
summary: 获取唯一任务标识符。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishBackgroundTaskInfo.Metadata
summary: 获取提交任务时提供的元信息。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishBackgroundTaskInfo.State
summary: 获取当前任务生命周期状态。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishBackgroundTaskInfo.Progress
summary: 获取从 0 到 1 的最新进度；尚未报告进度时为 `null`。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishBackgroundTaskInfo.QueuedAt
summary: 获取任务进入队列的 UTC 时间。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishBackgroundTaskInfo.StartedAt
summary: 获取任务开始执行的 UTC 时间。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishBackgroundTaskInfo.CompletedAt
summary: 获取任务到达终止状态的 UTC 时间。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishBackgroundTaskInfo.Exception
summary: 获取失败任务捕获的异常。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishBackgroundTaskState
summary: 描述 Flourish 后台任务的生命周期状态。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishBackgroundTaskState.Queued
summary: 任务正在等待可用执行槽。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishBackgroundTaskState.Running
summary: 任务委托正在运行。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishBackgroundTaskState.Cancelling
summary: 已请求取消，运行中的委托正在结束。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishBackgroundTaskState.Succeeded
summary: 任务已成功完成。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishBackgroundTaskState.Canceled
summary: 任务已取消。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishBackgroundTaskState.Failed
summary: 任务因异常失败。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishBackgroundTaskResult
summary: 表示没有返回值的后台任务终止结果。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishBackgroundTaskResult.Info
summary: 获取最终任务快照。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishBackgroundTaskResult.Succeeded
summary: 获取任务是否成功完成。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishBackgroundTaskResult.Canceled
summary: 获取任务是否已取消。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishBackgroundTaskResult.Exception
summary: 获取失败任务捕获的异常。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishBackgroundTaskResult`1
summary: 表示后台任务终止结果及其返回值。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishBackgroundTaskResult`1.Info
summary: 获取最终任务快照。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishBackgroundTaskResult`1.Value
summary: 获取任务成功时的返回值；取消或失败时为默认值。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishBackgroundTaskResult`1.Succeeded
summary: 获取任务是否成功完成。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishBackgroundTaskResult`1.Canceled
summary: 获取任务是否已取消。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishBackgroundTaskResult`1.Exception
summary: 获取失败任务捕获的异常。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishBackgroundTasksChangedEventArgs
summary: 提供后台任务变化后的当前活动任务列表。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishBackgroundTasksChangedEventArgs.Tasks
summary: 获取按提交顺序排列的等待中、运行中和正在取消任务。
---

---
uid: ArkheideSystem.Flourish.Abstract.IProjectBehavior
summary: 协调 Shell 使用的项目新建、保存、激活、删除与关闭生命周期；应用可以通过依赖注入替换默认行为。
---

---
uid: ArkheideSystem.Flourish.Abstract.IProjectBehavior.CreateProjectAsync(System.Threading.CancellationToken)
summary: 创建并激活一个项目。
syntax:
  parameters:
  - id: cancellationToken
    description: 用于取消操作的令牌。
  return:
    description: 成功创建项目时为 true；操作被取消时为 false。
---

---
uid: ArkheideSystem.Flourish.Abstract.IProjectBehavior.SaveActiveProjectAsync(System.Threading.CancellationToken)
summary: 在活动项目需要框架管理的持久化时保存它。
syntax:
  parameters:
  - id: cancellationToken
    description: 用于取消操作的令牌。
  return:
    description: 保存完成或无需框架管理的保存时为 true；操作被取消或没有活动项目时为 false。
---

---
uid: ArkheideSystem.Flourish.Abstract.IProjectBehavior.ActivateProjectAsync(System.String,System.Threading.CancellationToken)
summary: 处理尚未保存的活动项目后，激活指定项目。
syntax:
  parameters:
  - id: projectId
    description: 要激活的、区分大小写的项目 ID。
  - id: cancellationToken
    description: 用于取消操作的令牌。
  return:
    description: 成功激活项目时为 true；操作被取消或项目已不存在时为 false。
---

---
uid: ArkheideSystem.Flourish.Abstract.IProjectBehavior.DeleteProjectAsync(System.String,System.Threading.CancellationToken)
summary: 确认后删除指定项目。
syntax:
  parameters:
  - id: projectId
    description: 要删除的、区分大小写的项目 ID。
  - id: cancellationToken
    description: 用于取消操作的令牌。
  return:
    description: 成功删除项目时为 true；操作被取消或项目已不存在时为 false。
---

---
uid: ArkheideSystem.Flourish.Abstract.IProjectBehavior.CanCloseAsync(System.Threading.CancellationToken)
summary: 确定活动项目是否允许 Shell 关闭。
syntax:
  parameters:
  - id: cancellationToken
    description: 用于取消操作的令牌。
  return:
    description: 可以继续关闭时为 true；用户取消必要的保存时为 false。
---

---
uid: ArkheideSystem.Flourish.Abstract.IProjectService
summary: 管理 Flourish Shell 显示的项目目录，并将有序元数据与活动项目 ID 持久化到 appsettings 相邻的 projects.json。
---

---
uid: ArkheideSystem.Flourish.Abstract.IProjectService.Changed
summary: 项目元数据、活动选择或多项目模式变化后发生。
---

---
uid: ArkheideSystem.Flourish.Abstract.IProjectService.NewProjectRequested
summary: 标题选择器请求创建项目时发生；事件本身不创建业务数据。
---

---
uid: ArkheideSystem.Flourish.Abstract.IProjectService.ProjectActivationRequested
summary: 标题选择器请求激活项目时发生；事件本身不切换业务状态。
---

---
uid: ArkheideSystem.Flourish.Abstract.IProjectService.Current
summary: 获取当前项目显示状态的只读快照。
---

---
uid: ArkheideSystem.Flourish.Abstract.IProjectService.AddProject(ArkheideSystem.Flourish.Abstract.FlourishProject,System.Boolean)
summary: 添加项目标识，并可选将其设为活动项目。
syntax:
  parameters:
  - id: project
    description: 要添加的项目显示元数据。
  - id: activate
    description: 是否将新项目设为活动项目。
---

---
uid: ArkheideSystem.Flourish.Abstract.IProjectService.UpsertProject(ArkheideSystem.Flourish.Abstract.FlourishProject,System.Boolean)
summary: 添加项目，或按区分大小写的 ID 替换已有项目元数据。
syntax:
  parameters:
  - id: project
    description: 要添加或替换的项目显示元数据。
  - id: activate
    description: 是否将该项目设为活动项目。
---

---
uid: ArkheideSystem.Flourish.Abstract.IProjectService.SetProjectMetadata(System.String,System.String,System.String)
summary: 修改已注册项目的显示名称与可选本地存储路径。
syntax:
  parameters:
  - id: projectId
    description: 区分大小写的项目 ID。
  - id: name
    description: 非空项目显示名称。
  - id: storagePath
    description: 项目表示的可选本地存储路径；为 null 时表示项目尚未持久化。IProjectService 本身不会读写该位置。
---

---
uid: ArkheideSystem.Flourish.Abstract.IProjectService.SetActiveProject(System.String)
summary: 更改活动项目；传入 null 或空白文本可清除选择。
syntax:
  parameters:
  - id: projectId
    description: 区分大小写的项目 ID，或 null。
---

---
uid: ArkheideSystem.Flourish.Abstract.IProjectService.RemoveProject(System.String)
summary: 移除 Shell 中的项目元数据；移除活动项目时会清除选择。
syntax:
  parameters:
  - id: projectId
    description: 区分大小写的项目 ID。
  return:
    description: 成功移除项目时为 true。
---

---
uid: ArkheideSystem.Flourish.Abstract.IProjectService.TryGetProject(System.String,ArkheideSystem.Flourish.Abstract.FlourishProject@)
summary: 按区分大小写的 ID 查询项目元数据。
syntax:
  parameters:
  - id: projectId
    description: 要查询的项目 ID。
  - id: project
    description: 匹配的项目；未注册时为 null。
  return:
    description: 存在匹配项目时为 true。
---

---
uid: ArkheideSystem.Flourish.Abstract.IProjectService.SetMultiProjectEnabled(System.Boolean)
summary: 在运行时启用或禁用标题栏的项目感知显示。
syntax:
  parameters:
  - id: enabled
    description: 标题选择器是否使用活动项目标识。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishProject
summary: 描述 Flourish Shell 所表示的一个应用项目。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishProject.#ctor(System.String,System.String,System.String)
summary: 创建项目显示元数据。
syntax:
  parameters:
  - id: id
    description: 稳定且区分大小写的项目 ID。
  - id: name
    description: 项目显示名称。
  - id: storagePath
    description: 项目表示的可选本地存储路径；为 null 时表示项目尚未持久化。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishProject.Id
summary: 获取稳定且区分大小写的项目 ID。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishProject.Name
summary: 获取项目显示名称。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishProject.StoragePath
summary: 获取项目表示的本地存储路径；项目尚未持久化时为 null。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishProjectSnapshot
summary: 表示当前项目标识、活动选择与模式的只读快照。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishProjectSnapshot.#ctor(System.Collections.Generic.IReadOnlyList{ArkheideSystem.Flourish.Abstract.FlourishProject},ArkheideSystem.Flourish.Abstract.FlourishProject,System.Boolean,System.Int64)
summary: 创建项目显示状态快照。
syntax:
  parameters:
  - id: Projects
    description: 按插入顺序排列的已注册项目。
  - id: ActiveProject
    description: 活动项目，或 null。
  - id: IsMultiProjectEnabled
    description: 是否启用项目感知标题显示。
  - id: Version
    description: 项目状态版本号。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishProjectSnapshot.Projects
summary: 获取按插入顺序排列的已注册项目。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishProjectSnapshot.ActiveProject
summary: 获取活动项目；尚未选择时为 null。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishProjectSnapshot.IsMultiProjectEnabled
summary: 获取标题栏是否使用项目感知的显示语义。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishProjectSnapshot.Version
summary: 获取单调递增的项目状态版本号。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishProjectsChangedEventArgs
summary: 提供项目显示状态变化后的数据。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishProjectsChangedEventArgs.#ctor(ArkheideSystem.Flourish.Abstract.FlourishProjectSnapshot,ArkheideSystem.Flourish.Abstract.FlourishRuntimeChangeKind,System.String,System.Boolean)
summary: 创建项目状态变更事件数据。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishProjectsChangedEventArgs.Current
summary: 获取变更后的项目状态。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishProjectsChangedEventArgs.ChangeKind
summary: 获取引发事件的变更种类。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishProjectsChangedEventArgs.ProjectId
summary: 获取受影响的项目 ID；不适用时为 null。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishProjectsChangedEventArgs.ActiveProjectChanged
summary: 获取活动项目标识或其显示元数据是否变化。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishNewProjectRequestedEventArgs
summary: 标题栏请求创建项目时提供项目状态。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishNewProjectRequestedEventArgs.#ctor(ArkheideSystem.Flourish.Abstract.FlourishProjectSnapshot)
summary: 创建新建项目请求事件数据。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishNewProjectRequestedEventArgs.Current
summary: 获取发出请求时的项目状态。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishProjectActivationRequestedEventArgs
summary: 提供标题栏请求激活的项目与当前状态。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishProjectActivationRequestedEventArgs.#ctor(ArkheideSystem.Flourish.Abstract.FlourishProject,ArkheideSystem.Flourish.Abstract.FlourishProjectSnapshot)
summary: 创建项目激活请求事件数据。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishProjectActivationRequestedEventArgs.Project
summary: 获取用户选择的项目。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishProjectActivationRequestedEventArgs.Current
summary: 获取发出请求时的项目状态。
---

---
uid: ArkheideSystem.Flourish.Abstract.ITitleBarService
summary: 在应用运行期间更改 Flourish 标题栏内容与可见性。
---

---
uid: ArkheideSystem.Flourish.Abstract.ITitleBarService.SetApplicationTitle(System.String)
summary: 在运行时设置应用标题并显示标题选择器。
syntax:
  parameters:
  - id: title
    description: 非空应用标题。
---

---
uid: ArkheideSystem.Flourish.Abstract.ITitleBarService.SetApplicationSubTitle(System.String)
summary: 设置 Logo 信息视图中的应用副标题；传入 null 可清除。
---

---
uid: ArkheideSystem.Flourish.Abstract.ITitleBarService.SetApplicationIdentity(System.String,System.String)
summary: 在运行时原子更新应用标题与副标题。
syntax:
  parameters:
  - id: title
    description: 非空应用标题。
  - id: subTitle
    description: 应用副标题，或 null。
---

---
uid: ArkheideSystem.Flourish.Abstract.ITitleBarService.SetUnnamedProjectPlaceholder(System.String)
summary: 设置未持久化项目或缺少活动选择时显示的标题占位文本。
---

---
uid: ArkheideSystem.Flourish.Abstract.ITitleBarService.SetLogo(System.String,System.String,System.Boolean,System.Boolean,System.Boolean)
summary: 在运行时更改 Logo 来源与信息视图字段，并显示 Logo 按钮。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishTitleBarState
summary: 表示当前 Flourish 标题栏状态的只读快照。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishTitleBarState.ApplicationTitle
summary: 获取应用标题。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishTitleBarState.ApplicationSubTitle
summary: 获取 Logo 信息视图使用的应用副标题。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishTitleBarState.UnnamedProjectPlaceholder
summary: 获取未持久化项目或缺少活动选择时显示的标题占位文本。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishTitleBarState.ShowApplicationTitle
summary: 获取 Logo 信息视图是否显示应用标题。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishTitleBarState.ShowApplicationSubTitle
summary: 获取 Logo 信息视图是否显示应用副标题。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishTitleBarState.ShowProjectTitle
summary: 获取 Logo 信息视图是否显示活动项目标题。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishRegion.TitlebarApplicationInfo
summary: Logo 信息视图中位于标识元数据下方的应用自定义 Body 区域。
---

---
uid: ArkheideSystem.Flourish.Abstract.TitleBarElement.Title
summary: 应用或活动项目标题选择器。
---

---
uid: ArkheideSystem.Flourish.Abstract.TitleBarElement.Subtitle
summary: Logo 信息视图中的应用副标题；不再直接显示于标题栏。
---

---
uid: ArkheideSystem.Flourish.Themes.FlourishThemeResources
summary: 加载 Flourish 控件与主题资源。
---
