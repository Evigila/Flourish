---
uid: AcksheedSys.Flourish.Abstract
summary: Flourish 对外公开的应用组合、Shell 配置、导航和工具栏契约。
---

---
uid: AcksheedSys.Flourish.Abstract.BreadcrumbShowOption
summary: 指定标题栏中的面包屑导航何时显示。
---

---
uid: AcksheedSys.Flourish.Abstract.BreadcrumbShowOption.Always
summary: 只要标题栏可见，就始终显示面包屑导航。
---

---
uid: AcksheedSys.Flourish.Abstract.BreadcrumbShowOption.Auto
summary: 由 Flourish 根据当前导航状态自动决定是否显示面包屑导航。
---

---
uid: AcksheedSys.Flourish.Abstract.BreadcrumbShowOption.Hidden
summary: 隐藏面包屑导航。
---

---
uid: AcksheedSys.Flourish.Abstract.FlourishBuilder
summary: 提供用于创建 Flourish 应用 builder 的工厂方法。
---

---
uid: AcksheedSys.Flourish.Abstract.FlourishBuilder.CreateDefaultBuilder(System.String[])
summary: 创建使用标准 .NET Host 默认值配置的 Flourish builder。
syntax:
  parameters:
  - id: args
    description: 传递给应用的命令行参数。
  return:
    description: 用于配置并构建 Flourish 运行时的 IFlourishBuilder。
---

---
uid: AcksheedSys.Flourish.Abstract.FlourishNavigatedEventArgs
summary: 为 Flourish 导航事件提供数据。
---

---
uid: AcksheedSys.Flourish.Abstract.FlourishNavigatedEventArgs.#ctor(System.Type,System.Windows.Controls.Page,System.Object)
summary: 初始化导航事件数据。
syntax:
  parameters:
  - id: sourcePageType
    description: 已经导航到的已注册页面类型。
  - id: page
    description: 显示在内容框架中的页面实例。
  - id: parameter
    description: 调用方提供的可选导航参数。
---

---
uid: AcksheedSys.Flourish.Abstract.FlourishNavigatedEventArgs.SourcePageType
summary: 获取已经导航到的已注册页面类型。
---

---
uid: AcksheedSys.Flourish.Abstract.FlourishNavigatedEventArgs.Page
summary: 获取显示在内容框架中的页面实例。
---

---
uid: AcksheedSys.Flourish.Abstract.FlourishNavigatedEventArgs.Parameter
summary: 获取调用方提供的可选导航参数。
---

---
uid: AcksheedSys.Flourish.Abstract.FlourishNavigationPanelTransition
summary: 指定导航面板打开或关闭时使用的动画行为。
---

---
uid: AcksheedSys.Flourish.Abstract.FlourishNavigationPanelTransition.None
summary: 禁用导航面板过渡动画。
---

---
uid: AcksheedSys.Flourish.Abstract.FlourishNavigationPanelTransition.Resize
summary: 通过调整布局列宽来为导航面板生成动画。
---

---
uid: AcksheedSys.Flourish.Abstract.FlourishPageCacheMode
summary: 指定 Flourish 是否缓存导航创建的页面实例。
---

---
uid: AcksheedSys.Flourish.Abstract.FlourishPageCacheMode.Enabled
summary: 页面创建后复用同一个页面实例。
---

---
uid: AcksheedSys.Flourish.Abstract.FlourishPageCacheMode.Disabled
summary: 每次导航请求都创建新的页面实例。
---

---
uid: AcksheedSys.Flourish.Abstract.FlourishPageTransition
summary: 指定页面进入内容框架时使用的动画行为。
---

---
uid: AcksheedSys.Flourish.Abstract.FlourishPageTransition.None
summary: 禁用页面过渡动画。
---

---
uid: AcksheedSys.Flourish.Abstract.FlourishPageTransition.Fade
summary: 将页面淡入显示。
---

---
uid: AcksheedSys.Flourish.Abstract.FlourishPageTransition.EntranceFromBottom
summary: 让页面从底部进入视图。
---

---
uid: AcksheedSys.Flourish.Abstract.FlourishServiceCollectionExtensions
summary: 提供 Flourish 应用使用的服务集合扩展方法。
---

---
uid: AcksheedSys.Flourish.Abstract.FlourishServiceCollectionExtensions.AddNavigable``1(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.String,System.String,System.Boolean,AcksheedSys.Flourish.Abstract.FlourishPageCacheMode)
summary: 将 WPF 页面注册为 Flourish 可导航页面。
syntax:
  typeParameters:
  - id: TPage
    description: 要注册的页面类型。
  parameters:
  - id: services
    description: 接收页面注册的服务集合。
  - id: displayName
    description: 在导航 UI 中显示的名称。
  - id: iconGlyph
    description: 在导航 UI 中显示的图标字形。
  - id: isInitial
    description: 指示该页面是否作为初始页面。
  - id: cacheMode
    description: 该页面使用的页面缓存模式。
  return:
    description: 用于链式注册的同一个服务集合。
---

---
uid: AcksheedSys.Flourish.Abstract.FlourishServiceCollectionExtensions.AddNavigable(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Type,System.String,System.String,System.Boolean,AcksheedSys.Flourish.Abstract.FlourishPageCacheMode)
summary: 将指定的 WPF 页面类型注册为 Flourish 可导航页面。
syntax:
  parameters:
  - id: services
    description: 接收页面注册的服务集合。
  - id: pageType
    description: 要注册的页面类型。
  - id: displayName
    description: 在导航 UI 中显示的名称。
  - id: iconGlyph
    description: 在导航 UI 中显示的图标字形。
  - id: isInitial
    description: 指示该页面是否作为初始页面。
  - id: cacheMode
    description: 该页面使用的页面缓存模式。
  return:
    description: 用于链式注册的同一个服务集合。
exceptions:
- type: System.ArgumentException
  description: 当 pageType 未派生自 System.Windows.Controls.Page 时引发。
---

---
uid: AcksheedSys.Flourish.Abstract.FlourishToolbarItem
summary: 描述显示在 Flourish Shell 中的工具栏项。
---

---
uid: AcksheedSys.Flourish.Abstract.FlourishToolbarItem.#ctor(System.String,System.String,System.String)
summary: 创建一个工具栏项描述。
syntax:
  parameters:
  - id: DisplayName
    description: 工具栏项显示文本。
  - id: IconGlyph
    description: 工具栏项显示的图标字形。
  - id: CommandKey
    description: 传递给 ICommandParser 的可选命令键。
---

---
uid: AcksheedSys.Flourish.Abstract.FlourishToolbarItem.DisplayName
summary: 获取工具栏项显示文本。
---

---
uid: AcksheedSys.Flourish.Abstract.FlourishToolbarItem.IconGlyph
summary: 获取工具栏项显示的图标字形。
---

---
uid: AcksheedSys.Flourish.Abstract.FlourishToolbarItem.CommandKey
summary: 获取传递给 ICommandParser 的可选命令键。
---

---
uid: AcksheedSys.Flourish.Abstract.ICommandParser
summary: 解析由 Flourish UI 表面触发的命令键，例如工具栏项命令。
---

---
uid: AcksheedSys.Flourish.Abstract.ICommandParser.TryParse(System.String)
summary: 尝试解析并处理命令键。
syntax:
  parameters:
  - id: commandKey
    description: 与请求操作关联的命令键。
  return:
    description: 如果命令已识别并处理，则为 true；否则为 false。
---

---
uid: AcksheedSys.Flourish.Abstract.IFlourish
summary: 表示已经构建完成的 Flourish 应用运行时。
---

---
uid: AcksheedSys.Flourish.Abstract.IFlourish.Services
summary: 获取由底层 .NET Host 创建的应用服务提供程序。
---

---
uid: AcksheedSys.Flourish.Abstract.IFlourish.GetRequiredService``1
summary: 从 Flourish 服务提供程序中获取必需服务。
syntax:
  typeParameters:
  - id: T
    description: 要解析的服务类型。
  return:
    description: 已解析的服务实例。
---

---
uid: AcksheedSys.Flourish.Abstract.IFlourish.Start
summary: 启动底层应用 Host。
---

---
uid: AcksheedSys.Flourish.Abstract.IFlourish.StopAsync(System.Threading.CancellationToken)
summary: 异步停止底层应用 Host。
syntax:
  parameters:
  - id: cancellationToken
    description: 用于取消停止操作的令牌。
  return:
    description: 当 Host 完成停止时结束的任务。
---

---
uid: AcksheedSys.Flourish.Abstract.IFlourish.Show(System.Windows.Application)
summary: 为指定的 WPF 应用显示 Flourish Shell。
syntax:
  parameters:
  - id: application
    description: 拥有 Flourish Shell 的 WPF 应用。
---

---
uid: AcksheedSys.Flourish.Abstract.IFlourishBuilder
summary: 在构建 Flourish 运行时之前配置服务、Shell 选项、工具栏项和状态栏项。
---

---
uid: AcksheedSys.Flourish.Abstract.IFlourishBuilder.ConfigureServices(System.Action{Microsoft.Extensions.Hosting.HostBuilderContext,Microsoft.Extensions.DependencyInjection.IServiceCollection})
summary: 向底层 .NET Host builder 添加服务注册。
syntax:
  parameters:
  - id: configureServices
    description: 接收 Host 上下文和服务集合的配置回调。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AcksheedSys.Flourish.Abstract.IFlourishBuilder.ConfigureShell(System.Action{Microsoft.Extensions.Hosting.HostBuilderContext,AcksheedSys.Flourish.Abstract.IFlourishShellBuilder})
summary: 配置 Flourish Shell。
syntax:
  parameters:
  - id: configureShell
    description: 接收 Host 上下文和 Shell builder 的配置回调。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AcksheedSys.Flourish.Abstract.IFlourishBuilder.ConfigureDynamicToolbar(System.Action{Microsoft.Extensions.Hosting.HostBuilderContext,AcksheedSys.Flourish.Abstract.IFlourishDynamicToolbarBuilder})
summary: 配置按页面变化的动态工具栏项。
syntax:
  parameters:
  - id: configureToolbar
    description: 接收 Host 上下文和动态工具栏 builder 的配置回调。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AcksheedSys.Flourish.Abstract.IFlourishBuilder.ConfigureStatus(System.Action{Microsoft.Extensions.Hosting.HostBuilderContext,AcksheedSys.Flourish.Abstract.IFlourishStatusBuilder})
summary: 配置 Shell 状态区域。
syntax:
  parameters:
  - id: configureStatus
    description: 接收 Host 上下文和状态栏 builder 的配置回调。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AcksheedSys.Flourish.Abstract.IFlourishBuilder.Build
summary: 构建 Flourish 运行时。
syntax:
  return:
    description: 可以启动和释放的 Flourish 运行时。
---

---
uid: AcksheedSys.Flourish.Abstract.IFlourishDynamicToolbarBuilder
summary: 配置会随当前页面变化的工具栏项。
---

---
uid: AcksheedSys.Flourish.Abstract.IFlourishDynamicToolbarBuilder.CreateToolbarItems(System.Type,AcksheedSys.Flourish.Abstract.FlourishToolbarItem[])
summary: 为指定页面类型创建工具栏项。
syntax:
  parameters:
  - id: pageType
    description: 与工具栏项关联的页面类型。
  - id: items
    description: 为该页面显示的工具栏项。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AcksheedSys.Flourish.Abstract.IFlourishDynamicToolbarBuilder.CreateToolbarItems(System.Type,System.Boolean,AcksheedSys.Flourish.Abstract.FlourishToolbarItem[])
summary: 为指定页面类型创建工具栏项，并控制是否显示工具栏图标。
syntax:
  parameters:
  - id: pageType
    description: 与工具栏项关联的页面类型。
  - id: icon
    description: 指示是否显示工具栏项图标。
  - id: items
    description: 为该页面显示的工具栏项。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AcksheedSys.Flourish.Abstract.IFlourishDynamicToolbarBuilder.CreateToolbarItems``1(AcksheedSys.Flourish.Abstract.FlourishToolbarItem[])
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
uid: AcksheedSys.Flourish.Abstract.IFlourishDynamicToolbarBuilder.CreateToolbarItems``1(System.Boolean,AcksheedSys.Flourish.Abstract.FlourishToolbarItem[])
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
uid: AcksheedSys.Flourish.Abstract.IFlourishMotionBuilder
summary: 配置 Flourish Shell 的动效和动画行为。
---

---
uid: AcksheedSys.Flourish.Abstract.IFlourishMotionBuilder.SetEnabled(System.Boolean)
summary: 启用或禁用 Flourish 动效。
syntax:
  parameters:
  - id: enabled
    description: 指示是否启用动效。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AcksheedSys.Flourish.Abstract.IFlourishMotionBuilder.SetDuration
summary: 设置默认动效持续时间。
syntax:
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AcksheedSys.Flourish.Abstract.IFlourishMotionBuilder.SetDuration(System.TimeSpan)
summary: 设置动效持续时间。
syntax:
  parameters:
  - id: duration
    description: Flourish 动画使用的持续时间。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AcksheedSys.Flourish.Abstract.IFlourishMotionBuilder.SetPageTransition(AcksheedSys.Flourish.Abstract.FlourishPageTransition)
summary: 设置页面进入内容框架时使用的过渡效果。
syntax:
  parameters:
  - id: transition
    description: 要使用的页面过渡效果。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AcksheedSys.Flourish.Abstract.IFlourishMotionBuilder.SetNavigationPanelTransition(AcksheedSys.Flourish.Abstract.FlourishNavigationPanelTransition)
summary: 设置导航面板打开或关闭时使用的过渡效果。
syntax:
  parameters:
  - id: transition
    description: 要使用的导航面板过渡效果。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AcksheedSys.Flourish.Abstract.IFlourishMotionBuilder.SetHoverReveal(System.Boolean)
summary: 启用或禁用悬停揭示动画。
syntax:
  parameters:
  - id: enabled
    description: 指示是否启用悬停揭示动画。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AcksheedSys.Flourish.Abstract.IFlourishMotionBuilder.RespectSystemReducedMotion(System.Boolean)
summary: 控制 Flourish 是否遵循操作系统的减少动态效果偏好。
syntax:
  parameters:
  - id: enabled
    description: 指示是否遵循系统减少动态效果偏好。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AcksheedSys.Flourish.Abstract.IFlourishNavigationPanelBuilder
summary: 配置 Flourish 导航面板。
---

---
uid: AcksheedSys.Flourish.Abstract.IFlourishNavigationPanelBuilder.SetEnabled(System.Boolean)
summary: 启用或禁用导航面板。
syntax:
  parameters:
  - id: enabled
    description: 指示是否启用导航面板。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AcksheedSys.Flourish.Abstract.IFlourishNavigationPanelBuilder.SetDirection(AcksheedSys.Flourish.Abstract.NavigationPanelDirection)
summary: 设置导航面板显示在 Shell 的哪一侧。
syntax:
  parameters:
  - id: direction
    description: 导航面板方向。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AcksheedSys.Flourish.Abstract.IFlourishNavigationPanelBuilder.SetInitiallyOpen(System.Boolean)
summary: 设置 Shell 首次显示时导航面板是否打开。
syntax:
  parameters:
  - id: enabled
    description: 指示导航面板是否初始打开。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AcksheedSys.Flourish.Abstract.IFlourishNavigationPanelBuilder.SetTitle(System.String)
summary: 设置导航面板标题。
syntax:
  parameters:
  - id: title
    description: 显示在导航项上方的标题。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AcksheedSys.Flourish.Abstract.IFlourishShellBuilder
summary: 配置高层 Flourish Shell。
---

---
uid: AcksheedSys.Flourish.Abstract.IFlourishShellBuilder.UseTitlebar(System.Action{Microsoft.Extensions.Hosting.HostBuilderContext,AcksheedSys.Flourish.Abstract.IFlourishTitlebarBuilder})
summary: 启用并配置 Shell 标题栏。
syntax:
  parameters:
  - id: configureTitlebar
    description: 接收 Host 上下文和标题栏 builder 的配置回调。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AcksheedSys.Flourish.Abstract.IFlourishShellBuilder.UseNavigationPanel(System.Action{Microsoft.Extensions.Hosting.HostBuilderContext,AcksheedSys.Flourish.Abstract.IFlourishNavigationPanelBuilder})
summary: 启用并配置 Shell 导航面板。
syntax:
  parameters:
  - id: configureNavigationPanel
    description: 接收 Host 上下文和导航面板 builder 的配置回调。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AcksheedSys.Flourish.Abstract.IFlourishShellBuilder.SetWindowProperty(System.Action{Microsoft.Extensions.Hosting.HostBuilderContext,AcksheedSys.Flourish.Abstract.IFlourishWindowPropertyBuilder})
summary: 配置 Shell 窗口属性。
syntax:
  parameters:
  - id: configureWindow
    description: 接收 Host 上下文和窗口属性 builder 的配置回调。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AcksheedSys.Flourish.Abstract.IFlourishShellBuilder.SetGlobalFont(System.String,System.Double)
summary: 设置 Flourish Shell UI 使用的全局字体。
syntax:
  parameters:
  - id: fontFamily
    description: 字体系列名称。
  - id: fontSize
    description: 基础字号。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AcksheedSys.Flourish.Abstract.IFlourishShellBuilder.UseMaterialEffect(AcksheedSys.Flourish.Abstract.MaterialEffect)
summary: 为 Shell 窗口应用系统材质效果。
syntax:
  parameters:
  - id: effect
    description: 要应用的材质效果。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AcksheedSys.Flourish.Abstract.IFlourishShellBuilder.UseDynamicToolbar(System.Boolean)
summary: 启用或禁用动态工具栏区域。
syntax:
  parameters:
  - id: enabled
    description: 指示是否启用动态工具栏。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AcksheedSys.Flourish.Abstract.IFlourishShellBuilder.UseMotion(System.Boolean)
summary: 使用默认动效设置启用或禁用 Flourish 动效。
syntax:
  parameters:
  - id: enabled
    description: 指示是否启用动效。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AcksheedSys.Flourish.Abstract.IFlourishShellBuilder.UseMotion(System.Action{Microsoft.Extensions.Hosting.HostBuilderContext,AcksheedSys.Flourish.Abstract.IFlourishMotionBuilder})
summary: 启用并配置 Flourish 动效。
syntax:
  parameters:
  - id: configureMotion
    description: 接收 Host 上下文和动效 builder 的配置回调。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AcksheedSys.Flourish.Abstract.IFlourishStatusBuilder
summary: 配置 Flourish Shell 状态区域。
---

---
uid: AcksheedSys.Flourish.Abstract.IFlourishStatusBuilder.SetStatusText(System.String)
summary: 设置主要状态文本。
syntax:
  parameters:
  - id: text
    description: 显示在 Shell 状态区域中的状态文本。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AcksheedSys.Flourish.Abstract.IFlourishStatusBuilder.AddStatusItem(System.String,System.String)
summary: 添加包含显示文本和图标字形的状态项。
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
uid: AcksheedSys.Flourish.Abstract.IFlourishStatusBuilder.ShowLANConnectionStatus
summary: 显示内置 LAN 连接状态项。
syntax:
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AcksheedSys.Flourish.Abstract.IFlourishStatusBuilder.ShowPowerStatus
summary: 显示内置电源状态项。
syntax:
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AcksheedSys.Flourish.Abstract.IFlourishTitlebarBuilder
summary: 配置 Flourish Shell 标题栏。
---

---
uid: AcksheedSys.Flourish.Abstract.IFlourishTitlebarBuilder.ShowSearch(System.Boolean)
summary: 显示或隐藏搜索框。
syntax:
  parameters:
  - id: enabled
    description: 指示是否显示搜索框。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AcksheedSys.Flourish.Abstract.IFlourishTitlebarBuilder.ShowBreadcrumb(System.Boolean)
summary: 显示或隐藏面包屑导航。
syntax:
  parameters:
  - id: enabled
    description: 指示是否显示面包屑导航。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AcksheedSys.Flourish.Abstract.IFlourishTitlebarBuilder.ShowNavToggle(System.Boolean)
summary: 显示或隐藏导航面板切换按钮。
syntax:
  parameters:
  - id: enabled
    description: 指示是否显示导航面板切换按钮。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AcksheedSys.Flourish.Abstract.IFlourishTitlebarBuilder.ShowLogo(System.Boolean)
summary: 显示或隐藏 Logo 区域。
syntax:
  parameters:
  - id: enabled
    description: 指示是否显示 Logo。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AcksheedSys.Flourish.Abstract.IFlourishTitlebarBuilder.ShowTitle(System.Boolean)
summary: 显示或隐藏标题文本。
syntax:
  parameters:
  - id: enabled
    description: 指示是否显示标题。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AcksheedSys.Flourish.Abstract.IFlourishTitlebarBuilder.ShowSubTitle(System.Boolean)
summary: 显示或隐藏副标题文本。
syntax:
  parameters:
  - id: enabled
    description: 指示是否显示副标题。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AcksheedSys.Flourish.Abstract.IFlourishTitlebarBuilder.ShowProfile(System.Boolean)
summary: 显示或隐藏用户资料区域。
syntax:
  parameters:
  - id: enabled
    description: 指示是否显示用户资料区域。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AcksheedSys.Flourish.Abstract.IFlourishTitlebarBuilder.SetTrayExit(System.Boolean)
summary: 设置关闭标题栏时是否使用托盘退出流程。
syntax:
  parameters:
  - id: enabled
    description: 指示是否启用托盘退出行为。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AcksheedSys.Flourish.Abstract.IFlourishTitlebarBuilder.SetTitle(System.String)
summary: 设置标题文本。
syntax:
  parameters:
  - id: title
    description: 显示在标题栏中的标题。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AcksheedSys.Flourish.Abstract.IFlourishTitlebarBuilder.SetSubtitle(System.String)
summary: 设置副标题文本。
syntax:
  parameters:
  - id: subtitle
    description: 显示在标题旁边的副标题。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AcksheedSys.Flourish.Abstract.IFlourishTitlebarBuilder.SetLogo(System.String)
summary: 使用 WPF pack URI 设置 Logo 图像。
syntax:
  parameters:
  - id: packUri
    description: Logo 图像的 pack URI。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AcksheedSys.Flourish.Abstract.IFlourishTitlebarBuilder.SetSearchPlaceholder(System.String)
summary: 设置搜索框占位文本。
syntax:
  parameters:
  - id: placeholder
    description: 显示在搜索框中的占位文本。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AcksheedSys.Flourish.Abstract.IFlourishTitlebarBuilder.SetBreadcrumbBehavior(AcksheedSys.Flourish.Abstract.BreadcrumbShowOption)
summary: 设置面包屑导航何时显示。
syntax:
  parameters:
  - id: behavior
    description: 面包屑显示行为。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AcksheedSys.Flourish.Abstract.IFlourishWindowPropertyBuilder
summary: 配置 Flourish Shell 窗口。
---

---
uid: AcksheedSys.Flourish.Abstract.IFlourishWindowPropertyBuilder.SetWindowSize(System.Double,System.Double)
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
uid: AcksheedSys.Flourish.Abstract.IFlourishWindowPropertyBuilder.SetWindowMinSize(System.Double,System.Double)
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
uid: AcksheedSys.Flourish.Abstract.IFlourishWindowPropertyBuilder.SetWindowMaxSize(System.Double,System.Double)
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
uid: AcksheedSys.Flourish.Abstract.IFlourishWindowPropertyBuilder.SetWindowPosition(System.Windows.WindowStartupLocation)
summary: 设置 Shell 窗口启动位置。
syntax:
  parameters:
  - id: startupLocation
    description: Shell 窗口使用的 WPF 启动位置。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AcksheedSys.Flourish.Abstract.IFlourishWindowPropertyBuilder.SetManualWindowPosition(System.Double,System.Double)
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
uid: AcksheedSys.Flourish.Abstract.IFlourishWindowPropertyBuilder.SetWindowState(System.Windows.WindowState)
summary: 设置 Shell 窗口初始状态。
syntax:
  parameters:
  - id: windowState
    description: 初始 WPF 窗口状态。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AcksheedSys.Flourish.Abstract.IFlourishWindowPropertyBuilder.SetWindowResizeMode(System.Windows.ResizeMode)
summary: 设置 Shell 窗口调整大小模式。
syntax:
  parameters:
  - id: resizeMode
    description: Shell 窗口使用的 WPF 调整大小模式。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AcksheedSys.Flourish.Abstract.IFlourishWindowPropertyBuilder.UseTopmost(System.Boolean)
summary: 设置 Shell 窗口是否保持在其他窗口上方。
syntax:
  parameters:
  - id: enabled
    description: 指示是否启用置顶行为。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AcksheedSys.Flourish.Abstract.IFlourishWindowPropertyBuilder.ShowInTaskbar(System.Boolean)
summary: 设置 Shell 窗口是否显示在 Windows 任务栏中。
syntax:
  parameters:
  - id: enabled
    description: 指示窗口是否显示在任务栏中。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AcksheedSys.Flourish.Abstract.INavigationService
summary: 为已注册的 Flourish 页面提供运行时导航服务。
---

---
uid: AcksheedSys.Flourish.Abstract.INavigationService.Navigated
summary: Flourish 导航到已注册页面后触发。
---

---
uid: AcksheedSys.Flourish.Abstract.INavigationService.CanGoBack
summary: 获取是否可以执行后退导航。
---

---
uid: AcksheedSys.Flourish.Abstract.INavigationService.CanGoForward
summary: 获取是否可以执行前进导航。
---

---
uid: AcksheedSys.Flourish.Abstract.INavigationService.CurrentSourcePageType
summary: 获取当前显示在内容框架中的已注册源页面类型。
---

---
uid: AcksheedSys.Flourish.Abstract.INavigationService.Navigate(System.Type,System.Object,System.Boolean)
summary: 导航到已注册的页面类型。
syntax:
  parameters:
  - id: sourcePageType
    description: 要导航到的已注册页面类型。
  - id: parameter
    description: 传递给目标页面的可选参数。
  - id: addToBackStack
    description: 指示是否将当前页面加入后退栈。
  return:
    description: 如果导航成功，则为 true；否则为 false。
---

---
uid: AcksheedSys.Flourish.Abstract.INavigationService.Navigate``1(System.Object,System.Boolean)
summary: 导航到已注册的泛型页面类型。
syntax:
  typeParameters:
  - id: TPage
    description: 要导航到的已注册页面类型。
  parameters:
  - id: parameter
    description: 传递给目标页面的可选参数。
  - id: addToBackStack
    description: 指示是否将当前页面加入后退栈。
  return:
    description: 如果导航成功，则为 true；否则为 false。
---

---
uid: AcksheedSys.Flourish.Abstract.INavigationService.GoBack
summary: 导航到后退栈中的上一页。
syntax:
  return:
    description: 如果后退导航成功，则为 true；否则为 false。
---

---
uid: AcksheedSys.Flourish.Abstract.INavigationService.GoForward
summary: 导航到前进栈中的下一页。
syntax:
  return:
    description: 如果前进导航成功，则为 true；否则为 false。
---

---
uid: AcksheedSys.Flourish.Abstract.INavigationService.ClearBackStack
summary: 清空导航后退栈。
---

---
uid: AcksheedSys.Flourish.Abstract.MaterialEffect
summary: 指定应用到 Flourish Shell 窗口的系统材质效果。
---

---
uid: AcksheedSys.Flourish.Abstract.MaterialEffect.None
summary: 不应用系统材质效果。
---

---
uid: AcksheedSys.Flourish.Abstract.MaterialEffect.Mica
summary: 在支持时应用 Windows Mica 材质效果。
---

---
uid: AcksheedSys.Flourish.Abstract.NavigationPanelDirection
summary: 指定导航面板显示在 Shell 的哪一侧。
---

---
uid: AcksheedSys.Flourish.Abstract.NavigationPanelDirection.Left
summary: 在 Shell 左侧显示导航面板。
---

---
uid: AcksheedSys.Flourish.Abstract.NavigationPanelDirection.Right
summary: 在 Shell 右侧显示导航面板。
---
