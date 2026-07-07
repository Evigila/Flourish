---
uid: AckSS.Flourish.Abstract
summary: Flourish 对外公开的应用组合、Shell 配置、导航和工具栏契约。
---

---
uid: AckSS.Flourish.Abstract.BreadcrumbShowOption
summary: 指定标题栏中的面包屑导航何时显示。
---

---
uid: AckSS.Flourish.Abstract.BreadcrumbShowOption.Always
summary: 只要标题栏可见，就始终显示面包屑导航。
---

---
uid: AckSS.Flourish.Abstract.BreadcrumbShowOption.Auto
summary: 由 Flourish 根据当前导航状态自动决定是否显示面包屑导航。
---

---
uid: AckSS.Flourish.Abstract.BreadcrumbShowOption.Hidden
summary: 隐藏面包屑导航。
---

---
uid: AckSS.Flourish.Abstract.FlourishBuilder
summary: 提供用于创建 Flourish 应用 builder 的工厂方法。
---

---
uid: AckSS.Flourish.Abstract.FlourishBuilder.CreateDefaultBuilder(System.String[])
summary: 创建使用标准 .NET Host 默认值配置的 Flourish builder。
syntax:
  parameters:
  - id: args
    description: 传递给应用的命令行参数。
  return:
    description: 用于配置并构建 Flourish 运行时的 IFlourishBuilder。
---

---
uid: AckSS.Flourish.Abstract.FlourishNavigatedEventArgs
summary: 为 Flourish 导航事件提供数据。
---

---
uid: AckSS.Flourish.Abstract.FlourishNavigatedEventArgs.#ctor(System.Type,System.Windows.Controls.Page,System.Object)
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
uid: AckSS.Flourish.Abstract.FlourishNavigatedEventArgs.SourcePageType
summary: 获取已经导航到的已注册页面类型。
---

---
uid: AckSS.Flourish.Abstract.FlourishNavigatedEventArgs.Page
summary: 获取显示在内容框架中的页面实例。
---

---
uid: AckSS.Flourish.Abstract.FlourishNavigatedEventArgs.Parameter
summary: 获取调用方提供的可选导航参数。
---

---
uid: AckSS.Flourish.Abstract.FlourishNavigationPanelTransition
summary: 指定导航面板打开或关闭时使用的动画行为。
---

---
uid: AckSS.Flourish.Abstract.FlourishNavigationPanelTransition.None
summary: 禁用导航面板过渡动画。
---

---
uid: AckSS.Flourish.Abstract.FlourishNavigationPanelTransition.Resize
summary: 通过调整布局列宽来为导航面板生成动画。
---

---
uid: AckSS.Flourish.Abstract.FlourishPageCacheMode
summary: 指定 Flourish 是否缓存导航创建的页面实例。
---

---
uid: AckSS.Flourish.Abstract.FlourishPageCacheMode.Enabled
summary: 页面创建后复用同一个页面实例。
---

---
uid: AckSS.Flourish.Abstract.FlourishPageCacheMode.Disabled
summary: 每次导航请求都创建新的页面实例。
---

---
uid: AckSS.Flourish.Abstract.FlourishPageTransition
summary: 指定页面进入内容框架时使用的动画行为。
---

---
uid: AckSS.Flourish.Abstract.FlourishPageTransition.None
summary: 禁用页面过渡动画。
---

---
uid: AckSS.Flourish.Abstract.FlourishPageTransition.Fade
summary: 将页面淡入显示。
---

---
uid: AckSS.Flourish.Abstract.FlourishPageTransition.EntranceFromBottom
summary: 让页面从底部进入视图。
---

---
uid: AckSS.Flourish.Abstract.FlourishServiceCollectionExtensions
summary: 提供 Flourish 应用使用的服务集合扩展方法。
---

---
uid: AckSS.Flourish.Abstract.FlourishServiceCollectionExtensions.AddNavigable``1(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.String,System.String,AckSS.Flourish.Abstract.FlourishPageCacheMode)
summary: 将 WPF 页面注册为 Flourish 可导航页面，并记录页面显示元数据和缓存模式。
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
uid: AckSS.Flourish.Abstract.FlourishServiceCollectionExtensions.AddNavigable(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Type,System.String,System.String,AckSS.Flourish.Abstract.FlourishPageCacheMode)
summary: 将指定的 WPF 页面类型注册为 Flourish 可导航页面，并记录页面显示元数据和缓存模式。
syntax:
  parameters:
  - id: services
    description: 接收页面注册的服务集合。
  - id: pageType
    description: 要注册的页面类型。
  - id: displayName
    description: 页面显示为导航项时使用的名称。
  - id: iconGlyph
    description: 页面显示为导航项时使用的图标字形。
  - id: cacheMode
    description: 该页面使用的页面缓存模式。
  return:
    description: 用于链式注册的同一个服务集合。
exceptions:
- type: System.ArgumentException
  description: 当 pageType 未派生自 System.Windows.Controls.Page 时引发。
---

---
uid: AckSS.Flourish.Abstract.FlourishToolbarItem
summary: 描述显示在 Flourish Shell 中的工具栏项。
---

---
uid: AckSS.Flourish.Abstract.FlourishToolbarItem.#ctor(System.String,System.String,System.String)
summary: 创建一个工具栏项描述。
syntax:
  parameters:
  - id: displayName
    description: 工具栏项显示文本。
  - id: iconGlyph
    description: 工具栏项显示的图标字形。
  - id: commandKey
    description: 传递给 ICommandParser 的可选命令键。
---

---
uid: AckSS.Flourish.Abstract.FlourishToolbarItem.DisplayName
summary: 获取工具栏项显示文本。
---

---
uid: AckSS.Flourish.Abstract.FlourishToolbarItem.IconGlyph
summary: 获取工具栏项显示的图标字形。
---

---
uid: AckSS.Flourish.Abstract.FlourishToolbarItem.CommandKey
summary: 获取传递给 ICommandParser 的可选命令键。
---

---
uid: AckSS.Flourish.Abstract.ICommandParser
summary: 解析由 Flourish UI 表面触发的命令键，例如工具栏项命令和按钮类型导航项命令。
---

---
uid: AckSS.Flourish.Abstract.ICommandParser.TryParse(System.String)
summary: 尝试解析并处理命令键。
syntax:
  parameters:
  - id: commandKey
    description: 与请求操作关联的命令键。
  return:
    description: 如果命令已识别并处理，则为 true；否则为 false。
---

---
uid: AckSS.Flourish.Abstract.IFlourish
summary: 表示已经构建完成的 Flourish 应用运行时。
---

---
uid: AckSS.Flourish.Abstract.IFlourish.Services
summary: 获取由底层 .NET Host 创建的应用服务提供程序。
---

---
uid: AckSS.Flourish.Abstract.IFlourish.GetRequiredService``1
summary: 从 Flourish 服务提供程序中获取必需服务。
syntax:
  typeParameters:
  - id: T
    description: 要解析的服务类型。
  return:
    description: 已解析的服务实例。
---

---
uid: AckSS.Flourish.Abstract.IFlourish.Start
summary: 启动底层应用 Host。
---

---
uid: AckSS.Flourish.Abstract.IFlourish.StopAsync(System.Threading.CancellationToken)
summary: 异步停止底层应用 Host。
syntax:
  parameters:
  - id: cancellationToken
    description: 用于取消停止操作的令牌。
  return:
    description: 当 Host 完成停止时结束的任务。
---

---
uid: AckSS.Flourish.Abstract.IFlourish.Show(System.Windows.Application)
summary: 为指定的 WPF 应用显示 Flourish Shell。
syntax:
  parameters:
  - id: application
    description: 拥有 Flourish Shell 的 WPF 应用。
---

---
uid: AckSS.Flourish.Abstract.IFlourishBuilder
summary: 在构建 Flourish 运行时之前配置服务、Shell 选项、导航项、自定义区域、工具栏项和 Footer 状态项。
---

---
uid: AckSS.Flourish.Abstract.IFlourishBuilder.ConfigureServices(System.Action{Microsoft.Extensions.Hosting.HostBuilderContext,Microsoft.Extensions.DependencyInjection.IServiceCollection})
summary: 向底层 .NET Host builder 添加服务注册。
syntax:
  parameters:
  - id: configureServices
    description: 接收 Host 上下文和服务集合的配置回调。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AckSS.Flourish.Abstract.IFlourishBuilder.ConfigureShell(System.Action{AckSS.Flourish.Abstract.IFlourishShellBuilder})
summary: 配置 Flourish Shell 的高层功能开关。
syntax:
  parameters:
  - id: configureShell
    description: 接收 Shell builder 的配置回调。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AckSS.Flourish.Abstract.IFlourishBuilder.ConfigureTitleBar(System.Action{AckSS.Flourish.Abstract.IFlourishTitlebarBuilder})
summary: 配置标题栏内容和行为。
syntax:
  parameters:
  - id: configureTitleBar
    description: 接收标题栏 builder 的配置回调。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AckSS.Flourish.Abstract.IFlourishBuilder.ConfigureNavigation(System.Action{AckSS.Flourish.Abstract.IFlourishNavigationBuilder})
summary: 配置导航栏展示和可见导航模型。
syntax:
  parameters:
  - id: configureNavigation
    description: 接收导航 builder 的配置回调。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AckSS.Flourish.Abstract.IFlourishBuilder.ConfigureCustomHandler(System.Action{AckSS.Flourish.Abstract.IFlourishCustomHandlerBuilder})
summary: 配置预定义 Shell 区域中的自定义 WPF 内容。
syntax:
  parameters:
  - id: configureCustomHandler
    description: 接收自定义区域 builder 的配置回调。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AckSS.Flourish.Abstract.IFlourishBuilder.ConfigureDynamicToolbar(System.Action{AckSS.Flourish.Abstract.IFlourishDynamicToolbarBuilder})
summary: 配置按页面变化的动态工具栏项。
syntax:
  parameters:
  - id: configureToolbar
    description: 接收动态工具栏 builder 的配置回调。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AckSS.Flourish.Abstract.IFlourishBuilder.ConfigureTips(System.Action{AckSS.Flourish.Abstract.IFlourishTipsBuilder})
summary: 配置 Flourish 提示浮层行为。
syntax:
  parameters:
  - id: configureTips
    description: 接收 Tips builder 的配置回调。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AckSS.Flourish.Abstract.IFlourishBuilder.ConfigureMotion(System.Action{AckSS.Flourish.Abstract.IFlourishMotionBuilder})
summary: 配置 Flourish 动效行为。
syntax:
  parameters:
  - id: configureMotion
    description: 接收动效 builder 的配置回调。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AckSS.Flourish.Abstract.IFlourishBuilder.ConfigureWindow(System.Action{AckSS.Flourish.Abstract.IFlourishWindowPropertyBuilder})
summary: 配置 Flourish Shell 窗口属性。
syntax:
  parameters:
  - id: configureWindow
    description: 接收窗口属性 builder 的配置回调。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AckSS.Flourish.Abstract.IFlourishBuilder.ConfigureFont(System.String,System.Double)
summary: 配置 Flourish Shell UI 使用的全局字体。
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
uid: AckSS.Flourish.Abstract.IFlourishBuilder.ConfigureMaterialEffect(AckSS.Flourish.Abstract.MaterialEffect)
summary: 配置材质特效启用时应用到 Shell 窗口的系统材质。
syntax:
  parameters:
  - id: effect
    description: 要应用的材质效果。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AckSS.Flourish.Abstract.IFlourishBuilder.ConfigureThemes(AckSS.Flourish.Abstract.FlourishTheme)
summary: 配置主题启用时使用的默认主题。
syntax:
  parameters:
  - id: defaultTheme
    description: 尚未保存用户偏好时使用的默认主题。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AckSS.Flourish.Abstract.IFlourishBuilder.ConfigureFooter(System.Action{AckSS.Flourish.Abstract.IFlourishFooterBuilder})
summary: 配置 Shell Footer 状态区域。
syntax:
  parameters:
  - id: configureFooter
    description: 接收 Footer builder 的配置回调。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AckSS.Flourish.Abstract.IFlourishBuilder.Build
summary: 构建 Flourish 运行时。
syntax:
  return:
    description: 可以启动和释放的 Flourish 运行时。
---

---
uid: AckSS.Flourish.Abstract.IFlourishDynamicToolbarBuilder
summary: 配置会随当前页面变化的工具栏项。
---

---
uid: AckSS.Flourish.Abstract.IFlourishDynamicToolbarBuilder.CreateToolbarItems(System.Type,AckSS.Flourish.Abstract.FlourishToolbarItem[])
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
uid: AckSS.Flourish.Abstract.IFlourishDynamicToolbarBuilder.CreateToolbarItems(System.Type,System.Boolean,AckSS.Flourish.Abstract.FlourishToolbarItem[])
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
uid: AckSS.Flourish.Abstract.IFlourishDynamicToolbarBuilder.CreateToolbarItems``1(AckSS.Flourish.Abstract.FlourishToolbarItem[])
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
uid: AckSS.Flourish.Abstract.IFlourishDynamicToolbarBuilder.CreateToolbarItems``1(System.Boolean,AckSS.Flourish.Abstract.FlourishToolbarItem[])
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
uid: AckSS.Flourish.Abstract.IFlourishMotionBuilder
summary: 配置 Flourish Shell 的动效和动画行为。
---

---
uid: AckSS.Flourish.Abstract.IFlourishMotionBuilder.SetDuration
summary: 设置默认动效持续时间。
syntax:
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AckSS.Flourish.Abstract.IFlourishMotionBuilder.SetDuration(System.TimeSpan)
summary: 设置动效持续时间。
syntax:
  parameters:
  - id: duration
    description: Flourish 动画使用的持续时间。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AckSS.Flourish.Abstract.IFlourishMotionBuilder.SetPageTransition(AckSS.Flourish.Abstract.FlourishPageTransition)
summary: 设置页面进入内容框架时使用的过渡效果。
syntax:
  parameters:
  - id: transition
    description: 要使用的页面过渡效果。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AckSS.Flourish.Abstract.IFlourishMotionBuilder.SetNavigationPanelTransition(AckSS.Flourish.Abstract.FlourishNavigationPanelTransition)
summary: 设置导航面板打开或关闭时使用的过渡效果。
syntax:
  parameters:
  - id: transition
    description: 要使用的导航面板过渡效果。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AckSS.Flourish.Abstract.IFlourishMotionBuilder.SetHoverReveal(System.Boolean)
summary: 启用或禁用悬停揭示动画。
syntax:
  parameters:
  - id: enabled
    description: 指示是否启用悬停揭示动画。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AckSS.Flourish.Abstract.IFlourishMotionBuilder.RespectSystemReducedMotion(System.Boolean)
summary: 控制 Flourish 是否遵循操作系统的减少动态效果偏好。
syntax:
  parameters:
  - id: enabled
    description: 指示是否遵循系统减少动态效果偏好。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AckSS.Flourish.Abstract.IFlourishNavigationBuilder
summary: 配置 Flourish 导航栏展示和可见导航模型。
---

---
uid: AckSS.Flourish.Abstract.IFlourishNavigationBuilder.SetDirection(AckSS.Flourish.Abstract.NavigationPanelDirection)
summary: 设置导航面板显示在 Shell 的哪一侧。
syntax:
  parameters:
  - id: direction
    description: 导航面板方向。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AckSS.Flourish.Abstract.IFlourishNavigationBuilder.SetInitiallyOpen(System.Boolean)
summary: 设置 Shell 首次显示时导航面板是否打开。
syntax:
  parameters:
  - id: enabled
    description: 指示导航面板是否初始打开。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AckSS.Flourish.Abstract.IFlourishNavigationBuilder.SetPanelWidth(System.Double,System.Double,System.Double,System.Double)
summary: 设置导航栏宽度和 splitter 调整范围。
syntax:
  parameters:
  - id: openWidth
    description: 导航栏展开宽度。
  - id: closedWidth
    description: 导航栏折叠宽度。
  - id: maxWidth
    description: 导航栏最大展开宽度。
  - id: minWidth
    description: 导航栏最小展开宽度。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AckSS.Flourish.Abstract.IFlourishNavigationBuilder.SetTitle(System.String)
summary: 设置旧版未分组导航界面使用的导航面板标题。
syntax:
  parameters:
  - id: title
    description: 显示在旧版导航项上方的标题。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AckSS.Flourish.Abstract.IFlourishNavigationBuilder.SetGroup(System.String,System.Int32,System.Action{AckSS.Flourish.Abstract.IFlourishNavigationGroupBuilder})
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
uid: AckSS.Flourish.Abstract.IFlourishNavigationBuilder.AddFixedNavigableViewItem``1(System.Boolean,System.Int32,System.Int32)
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
uid: AckSS.Flourish.Abstract.IFlourishNavigationBuilder.AddFixedNavigableItem(System.String,System.String,System.Int32,System.Int32,System.String)
summary: 在导航栏底部固定区域添加一个按钮类型命令项。
syntax:
  parameters:
  - id: displayName
    description: 命令项显示文本。
  - id: commandKey
    description: 触发时传递给 ICommandParser 的可选命令键。
  - id: parentId
    description: 可选父节点 ID。childId 不为 0 时必须为 0。
  - id: childId
    description: 可选父节点归属 ID。parentId 不为 0 时必须为 0。
  - id: iconGlyph
    description: 命令项显示的可选图标字形。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AckSS.Flourish.Abstract.IFlourishNavigationGroupBuilder
summary: 配置 Flourish 导航分组中显示的导航项。
---

---
uid: AckSS.Flourish.Abstract.IFlourishNavigationGroupBuilder.AddNavigableViewItem``1(System.Boolean,System.Int32,System.Int32)
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
uid: AckSS.Flourish.Abstract.IFlourishNavigationGroupBuilder.AddNavigableItem(System.String,System.String,System.Int32,System.Int32,System.String)
summary: 将一个按钮类型命令项添加到当前导航分组。
syntax:
  parameters:
  - id: displayName
    description: 命令项显示文本。
  - id: commandKey
    description: 触发时传递给 ICommandParser 的可选命令键。
  - id: parentId
    description: 可选父节点 ID。childId 不为 0 时必须为 0。
  - id: childId
    description: 可选父节点归属 ID。parentId 不为 0 时必须为 0。
  - id: iconGlyph
    description: 命令项显示的可选图标字形。
  return:
    description: 用于链式配置的当前分组 builder。
---

---
uid: AckSS.Flourish.Abstract.IFlourishShellBuilder
summary: 配置高层 Flourish Shell 功能开关。
---

---
uid: AckSS.Flourish.Abstract.IFlourishShellBuilder.UseTitleBar(System.Boolean)
summary: 启用或禁用 Shell 标题栏。
syntax:
  parameters:
  - id: enabled
    description: 指示是否启用标题栏。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AckSS.Flourish.Abstract.IFlourishShellBuilder.UseNavigation(System.Boolean)
summary: 启用或禁用 Shell 导航栏。
syntax:
  parameters:
  - id: enabled
    description: 指示是否启用导航栏。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AckSS.Flourish.Abstract.IFlourishShellBuilder.UseDynamicToolbar(System.Boolean)
summary: 启用或禁用动态工具栏区域。
syntax:
  parameters:
  - id: enabled
    description: 指示是否启用动态工具栏。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AckSS.Flourish.Abstract.IFlourishShellBuilder.UseTips(System.Boolean)
summary: 启用或禁用 Flourish 提示浮层。
syntax:
  parameters:
  - id: enabled
    description: 指示是否启用提示浮层。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AckSS.Flourish.Abstract.IFlourishShellBuilder.UseMotion(System.Boolean)
summary: 启用或禁用 Flourish 动效。
syntax:
  parameters:
  - id: enabled
    description: 指示是否启用动效。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AckSS.Flourish.Abstract.IFlourishShellBuilder.UseMaterialEffect(System.Boolean)
summary: 启用或禁用 Shell 材质特效。
syntax:
  parameters:
  - id: enabled
    description: 指示是否启用材质特效。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AckSS.Flourish.Abstract.IFlourishShellBuilder.UseThemes(System.Boolean)
summary: 启用或禁用 Flourish 主题支持。
syntax:
  parameters:
  - id: enabled
    description: 指示是否启用主题支持。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AckSS.Flourish.Abstract.IFlourishShellBuilder.UseFooter(System.Boolean)
summary: 启用或禁用 Shell Footer。
syntax:
  parameters:
  - id: enabled
    description: 指示是否启用 Footer。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AckSS.Flourish.Abstract.IFlourishFooterBuilder
summary: 配置 Flourish Shell Footer 状态区域。
---

---
uid: AckSS.Flourish.Abstract.IFlourishFooterBuilder.SetStatusText(System.String)
summary: 设置主要 Footer 状态文本。
syntax:
  parameters:
  - id: text
    description: 显示在 Shell Footer 中的状态文本。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AckSS.Flourish.Abstract.IFlourishFooterBuilder.AddStatusItem(System.String,System.String)
summary: 添加包含显示文本和图标字形的 Footer 状态项。
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
uid: AckSS.Flourish.Abstract.IFlourishFooterBuilder.ShowLANConnectionStatus
summary: 显示内置 LAN 连接状态项。
syntax:
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AckSS.Flourish.Abstract.IFlourishFooterBuilder.ShowPowerStatus
summary: 显示内置电源状态项。
syntax:
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AckSS.Flourish.Abstract.IFlourishTitlebarBuilder
summary: 配置 Flourish Shell 标题栏。
---

---
uid: AckSS.Flourish.Abstract.IFlourishTitlebarBuilder.ShowSearch(System.Boolean)
summary: 显示或隐藏搜索框。
syntax:
  parameters:
  - id: enabled
    description: 指示是否显示搜索框。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AckSS.Flourish.Abstract.IFlourishTitlebarBuilder.ShowBreadcrumb(System.Boolean)
summary: 显示或隐藏面包屑导航。
syntax:
  parameters:
  - id: enabled
    description: 指示是否显示面包屑导航。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AckSS.Flourish.Abstract.IFlourishTitlebarBuilder.ShowNavToggle(System.Boolean)
summary: 显示或隐藏导航面板切换按钮。
syntax:
  parameters:
  - id: enabled
    description: 指示是否显示导航面板切换按钮。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AckSS.Flourish.Abstract.IFlourishTitlebarBuilder.ShowLogo(System.Boolean)
summary: 显示或隐藏 Logo 区域。
syntax:
  parameters:
  - id: enabled
    description: 指示是否显示 Logo。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AckSS.Flourish.Abstract.IFlourishTitlebarBuilder.ShowTitle(System.Boolean)
summary: 显示或隐藏标题文本。
syntax:
  parameters:
  - id: enabled
    description: 指示是否显示标题。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AckSS.Flourish.Abstract.IFlourishTitlebarBuilder.ShowSubTitle(System.Boolean)
summary: 显示或隐藏副标题文本。
syntax:
  parameters:
  - id: enabled
    description: 指示是否显示副标题。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AckSS.Flourish.Abstract.IFlourishTitlebarBuilder.ShowProfile(System.Boolean)
summary: 显示或隐藏用户资料区域。
syntax:
  parameters:
  - id: enabled
    description: 指示是否显示用户资料区域。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AckSS.Flourish.Abstract.IFlourishTitlebarBuilder.SetTrayExit(System.Boolean)
summary: 设置关闭标题栏时是否使用托盘退出流程。
syntax:
  parameters:
  - id: enabled
    description: 指示是否启用托盘退出行为。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AckSS.Flourish.Abstract.IFlourishTitlebarBuilder.SetTitle(System.String)
summary: 设置标题文本。
syntax:
  parameters:
  - id: title
    description: 显示在标题栏中的标题。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AckSS.Flourish.Abstract.IFlourishTitlebarBuilder.SetSubtitle(System.String)
summary: 设置副标题文本。
syntax:
  parameters:
  - id: subtitle
    description: 显示在标题旁边的副标题。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AckSS.Flourish.Abstract.IFlourishTitlebarBuilder.SetLogo(System.String)
summary: 使用 WPF pack URI 设置 Logo 图像。
syntax:
  parameters:
  - id: packUri
    description: Logo 图像的 pack URI。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AckSS.Flourish.Abstract.IFlourishTitlebarBuilder.SetSearchPlaceholder(System.String)
summary: 设置搜索框占位文本。
syntax:
  parameters:
  - id: placeholder
    description: 显示在搜索框中的占位文本。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AckSS.Flourish.Abstract.IFlourishTitlebarBuilder.SetBreadcrumbBehavior(AckSS.Flourish.Abstract.BreadcrumbShowOption)
summary: 设置面包屑导航何时显示。
syntax:
  parameters:
  - id: behavior
    description: 面包屑显示行为。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AckSS.Flourish.Abstract.IFlourishWindowPropertyBuilder
summary: 配置 Flourish Shell 窗口。
---

---
uid: AckSS.Flourish.Abstract.IFlourishWindowPropertyBuilder.SetWindowSize(System.Double,System.Double)
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
uid: AckSS.Flourish.Abstract.IFlourishWindowPropertyBuilder.SetWindowMinSize(System.Double,System.Double)
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
uid: AckSS.Flourish.Abstract.IFlourishWindowPropertyBuilder.SetWindowMaxSize(System.Double,System.Double)
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
uid: AckSS.Flourish.Abstract.IFlourishWindowPropertyBuilder.SetWindowPosition(System.Windows.WindowStartupLocation)
summary: 设置 Shell 窗口启动位置。
syntax:
  parameters:
  - id: startupLocation
    description: Shell 窗口使用的 WPF 启动位置。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AckSS.Flourish.Abstract.IFlourishWindowPropertyBuilder.SetManualWindowPosition(System.Double,System.Double)
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
uid: AckSS.Flourish.Abstract.IFlourishWindowPropertyBuilder.SetWindowState(System.Windows.WindowState)
summary: 设置 Shell 窗口初始状态。
syntax:
  parameters:
  - id: windowState
    description: 初始 WPF 窗口状态。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AckSS.Flourish.Abstract.IFlourishWindowPropertyBuilder.SetWindowResizeMode(System.Windows.ResizeMode)
summary: 设置 Shell 窗口调整大小模式。
syntax:
  parameters:
  - id: resizeMode
    description: Shell 窗口使用的 WPF 调整大小模式。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AckSS.Flourish.Abstract.IFlourishWindowPropertyBuilder.UseTopmost(System.Boolean)
summary: 设置 Shell 窗口是否保持在其他窗口上方。
syntax:
  parameters:
  - id: enabled
    description: 指示是否启用置顶行为。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AckSS.Flourish.Abstract.IFlourishWindowPropertyBuilder.ShowInTaskbar(System.Boolean)
summary: 设置 Shell 窗口是否显示在 Windows 任务栏中。
syntax:
  parameters:
  - id: enabled
    description: 指示窗口是否显示在任务栏中。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: AckSS.Flourish.Abstract.INavigationService
summary: 为已注册的 Flourish 页面提供运行时导航服务。
---

---
uid: AckSS.Flourish.Abstract.INavigationService.Navigated
summary: Flourish 导航到已注册页面后触发。
---

---
uid: AckSS.Flourish.Abstract.INavigationService.CanGoBack
summary: 获取是否可以执行后退导航。
---

---
uid: AckSS.Flourish.Abstract.INavigationService.CanGoForward
summary: 获取是否可以执行前进导航。
---

---
uid: AckSS.Flourish.Abstract.INavigationService.CurrentSourcePageType
summary: 获取当前显示在内容框架中的已注册源页面类型。
---

---
uid: AckSS.Flourish.Abstract.INavigationService.Navigate(System.Type,System.Object,System.Boolean)
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
uid: AckSS.Flourish.Abstract.INavigationService.Navigate``1(System.Object,System.Boolean)
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
uid: AckSS.Flourish.Abstract.INavigationService.GoBack
summary: 导航到后退栈中的上一页。
syntax:
  return:
    description: 如果后退导航成功，则为 true；否则为 false。
---

---
uid: AckSS.Flourish.Abstract.INavigationService.GoForward
summary: 导航到前进栈中的下一页。
syntax:
  return:
    description: 如果前进导航成功，则为 true；否则为 false。
---

---
uid: AckSS.Flourish.Abstract.INavigationService.ClearBackStack
summary: 清空导航后退栈。
---

---
uid: AckSS.Flourish.Abstract.MaterialEffect
summary: 指定应用到 Flourish Shell 窗口的系统材质效果。
---

---
uid: AckSS.Flourish.Abstract.MaterialEffect.None
summary: 不应用系统材质效果。
---

---
uid: AckSS.Flourish.Abstract.MaterialEffect.Mica
summary: 在支持时应用 Windows Mica 材质效果。
---

---
uid: AckSS.Flourish.Abstract.NavigationPanelDirection
summary: 指定导航面板显示在 Shell 的哪一侧。
---

---
uid: AckSS.Flourish.Abstract.NavigationPanelDirection.Left
summary: 在 Shell 左侧显示导航面板。
---

---
uid: AckSS.Flourish.Abstract.NavigationPanelDirection.Right
summary: 在 Shell 右侧显示导航面板。
---
