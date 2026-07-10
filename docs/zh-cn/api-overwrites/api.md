---
uid: ArkheideSystem.Flourish.Abstract
summary: Flourish 对外公开的应用组合、Shell 配置、导航和工具栏契约。
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
summary: 由 Flourish 根据当前导航状态自动决定是否显示面包屑导航。
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
summary: 通过调整布局列宽来为导航面板生成动画。
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
summary: 将页面淡入显示。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishPageTransition.EntranceFromBottom
summary: 让页面从底部进入视图。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishServiceCollectionExtensions
summary: 提供 Flourish 应用使用的服务集合扩展方法。
---

---
uid: ArkheideSystem.Flourish.Abstract.FlourishServiceCollectionExtensions.AddNavigable``1(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.String,System.String,ArkheideSystem.Flourish.Abstract.FlourishPageCacheMode,System.String)
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
uid: ArkheideSystem.Flourish.Abstract.FlourishServiceCollectionExtensions.AddNavigable(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Type,System.String,System.String,ArkheideSystem.Flourish.Abstract.FlourishPageCacheMode,System.String)
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
    description: 传递给 ICommandParser 的可选命令键。
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
summary: 获取传递给 ICommandParser 的可选命令键。
---

---
uid: ArkheideSystem.Flourish.Abstract.ICommandParser
summary: 解析由 Flourish UI 表面触发的命令键，例如工具栏项命令和按钮类型导航项命令。
---

---
uid: ArkheideSystem.Flourish.Abstract.ICommandParser.TryParse(System.String)
summary: 尝试解析并处理命令键。
syntax:
  parameters:
  - id: commandKey
    description: 与请求操作关联的命令键。
  return:
    description: 如果命令已识别并处理，则为 true；否则为 false。
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
summary: 在构建 Flourish 运行时之前配置服务、Shell 选项、导航项、自定义区域、工具栏项和 Footer 状态项。
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
summary: 配置 Flourish Shell 的高层功能开关。
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
uid: ArkheideSystem.Flourish.Abstract.IFlourishBuilder.ConfigureTips(System.Action{ArkheideSystem.Flourish.Abstract.IFlourishTipsBuilder})
summary: 配置 Flourish 提示浮层行为。
syntax:
  parameters:
  - id: configureTips
    description: 接收 Tips builder 的配置回调。
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
uid: ArkheideSystem.Flourish.Abstract.IFlourishBuilder.ConfigureFont(System.String,System.Double)
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
uid: ArkheideSystem.Flourish.Abstract.IFlourishBuilder.ConfigureMaterialEffect(ArkheideSystem.Flourish.Abstract.MaterialEffect)
summary: 配置材质特效启用时应用到 Shell 窗口的系统材质。
syntax:
  parameters:
  - id: effect
    description: 要应用的材质效果。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishBuilder.ConfigureThemes(ArkheideSystem.Flourish.Abstract.FlourishTheme)
summary: 配置主题启用时使用的默认主题。
syntax:
  parameters:
  - id: defaultTheme
    description: 尚未保存用户偏好时使用的默认主题。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishBuilder.ConfigureFooter(System.Action{ArkheideSystem.Flourish.Abstract.IFlourishFooterBuilder})
summary: 配置 Shell Footer 状态区域。
syntax:
  parameters:
  - id: configureFooter
    description: 接收 Footer builder 的配置回调。
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
uid: ArkheideSystem.Flourish.Abstract.IFlourishDynamicToolbarBuilder
summary: 配置会随当前页面变化的工具栏项。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishDynamicToolbarBuilder.CreateToolbarItems(System.Type,ArkheideSystem.Flourish.Abstract.FlourishToolbarItem[])
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
uid: ArkheideSystem.Flourish.Abstract.IFlourishDynamicToolbarBuilder.CreateToolbarItems(System.Type,System.Boolean,ArkheideSystem.Flourish.Abstract.FlourishToolbarItem[])
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
    description: 导航栏折叠宽度。
  - id: maxWidth
    description: 导航栏最大展开宽度。
  - id: minWidth
    description: 导航栏最小展开宽度。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishNavigationBuilder.SetTitle(System.String)
summary: 设置未显式配置分组或固定项时使用的导航面板标题。
syntax:
  parameters:
  - id: title
    description: 显示在自动列出的导航项上方的标题。
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
uid: ArkheideSystem.Flourish.Abstract.IFlourishNavigationBuilder.AddFixedNavigableViewItem(System.String,System.Boolean,System.Int32,System.Int32)
summary: 按导航键在导航栏底部固定区域添加已注册页面项。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishNavigationBuilder.AddFixedNavigableItem(System.String,System.String,System.Int32,System.Int32,System.String)
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
uid: ArkheideSystem.Flourish.Abstract.IFlourishNavigationGroupBuilder.AddNavigableViewItem(System.String,System.Boolean,System.Int32,System.Int32)
summary: 按导航键将已注册 WPF 页面添加到当前导航分组。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishNavigationGroupBuilder.AddNavigableItem(System.String,System.String,System.Int32,System.Int32,System.String)
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
uid: ArkheideSystem.Flourish.Abstract.IFlourishShellBuilder
summary: 配置高层 Flourish Shell 功能开关。
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
uid: ArkheideSystem.Flourish.Abstract.IFlourishShellBuilder.UseTips(System.Boolean)
summary: 启用或禁用 Flourish 提示浮层。
syntax:
  parameters:
  - id: enabled
    description: 指示是否启用提示浮层。
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
uid: ArkheideSystem.Flourish.Abstract.IFlourishShellBuilder.UseMaterialEffect(System.Boolean)
summary: 启用或禁用 Shell 材质特效。
syntax:
  parameters:
  - id: enabled
    description: 指示是否启用材质特效。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishShellBuilder.UseThemes(System.Boolean)
summary: 启用或禁用 Flourish 主题支持。
syntax:
  parameters:
  - id: enabled
    description: 指示是否启用主题支持。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishShellBuilder.UseFooter(System.Boolean)
summary: 启用或禁用 Shell Footer。
syntax:
  parameters:
  - id: enabled
    description: 指示是否启用 Footer。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishFooterBuilder
summary: 配置 Flourish Shell Footer 状态区域。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishFooterBuilder.SetStatusText(System.String)
summary: 设置主要 Footer 状态文本。
syntax:
  parameters:
  - id: text
    description: 显示在 Shell Footer 中的状态文本。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishFooterBuilder.AddStatusItem(System.String,System.String)
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
uid: ArkheideSystem.Flourish.Abstract.IFlourishFooterBuilder.ShowLANConnectionStatus
summary: 显示内置 LAN 连接状态项。
syntax:
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishFooterBuilder.ShowPowerStatus
summary: 显示内置电源状态项。
syntax:
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishTitlebarBuilder
summary: 配置 Flourish Shell 标题栏。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishTitlebarBuilder.ShowSearch(System.Boolean)
summary: 显示或隐藏搜索框。
syntax:
  parameters:
  - id: enabled
    description: 指示是否显示搜索框。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishTitlebarBuilder.ShowBreadcrumb(System.Boolean)
summary: 显示或隐藏面包屑导航。
syntax:
  parameters:
  - id: enabled
    description: 指示是否显示面包屑导航。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishTitlebarBuilder.ShowNavToggle(System.Boolean)
summary: 显示或隐藏导航面板切换按钮。
syntax:
  parameters:
  - id: enabled
    description: 指示是否显示导航面板切换按钮。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishTitlebarBuilder.ShowLogo(System.Boolean)
summary: 显示或隐藏 Logo 区域。
syntax:
  parameters:
  - id: enabled
    description: 指示是否显示 Logo。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishTitlebarBuilder.ShowTitle(System.Boolean)
summary: 显示或隐藏标题文本。
syntax:
  parameters:
  - id: enabled
    description: 指示是否显示标题。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishTitlebarBuilder.ShowSubTitle(System.Boolean)
summary: 显示或隐藏副标题文本。
syntax:
  parameters:
  - id: enabled
    description: 指示是否显示副标题。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishTitlebarBuilder.ShowProfile(System.Boolean)
summary: 显示或隐藏用户资料区域。
syntax:
  parameters:
  - id: enabled
    description: 指示是否显示用户资料区域。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishTitlebarBuilder.SetTrayExit(System.Boolean)
summary: 设置关闭标题栏时是否使用托盘退出流程。
syntax:
  parameters:
  - id: enabled
    description: 指示是否启用托盘退出行为。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishTitlebarBuilder.SetTitle(System.String)
summary: 设置标题文本。
syntax:
  parameters:
  - id: title
    description: 显示在标题栏中的标题。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishTitlebarBuilder.SetSubtitle(System.String)
summary: 设置副标题文本。
syntax:
  parameters:
  - id: subtitle
    description: 显示在标题旁边的副标题。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishTitlebarBuilder.SetLogo(System.String)
summary: 使用 WPF pack URI 设置 Logo 图像。
syntax:
  parameters:
  - id: packUri
    description: Logo 图像的 pack URI。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishTitlebarBuilder.SetSearchPlaceholder(System.String)
summary: 设置搜索框占位文本。
syntax:
  parameters:
  - id: placeholder
    description: 显示在搜索框中的占位文本。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishTitlebarBuilder.SetBreadcrumbBehavior(ArkheideSystem.Flourish.Abstract.BreadcrumbShowOption)
summary: 设置面包屑导航何时显示。
syntax:
  parameters:
  - id: behavior
    description: 面包屑显示行为。
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
summary: 按导航键导航到已注册页面。
---

---
uid: ArkheideSystem.Flourish.Abstract.INavigationService.Navigate(System.Type,System.Object,System.Boolean)
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
uid: ArkheideSystem.Flourish.Abstract.INavigationService.Navigate``1(System.Object,System.Boolean)
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
summary: 配置 Profile 的默认用户、承载页面和登录体验。
syntax:
  parameters:
  - id: configureProfile
    description: 接收 Profile builder 的配置回调。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishShellBuilder.UseProfile(System.Boolean)
summary: 启用或禁用 Shell 标题栏中的 Profile 入口与弹层。
syntax:
  parameters:
  - id: enabled
    description: 指示是否启用 Profile 功能。
  return:
    description: 用于链式配置的当前 builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishProfileBuilder
summary: 配置 Profile 的默认显示信息以及弹层中承载的页面。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishProfileBuilder.SetDefaultProfile(System.String,System.String)
summary: 设置未登录时显示的默认图片和组合名称；组合名称按当前名称顺序拆分。
syntax:
  parameters:
  - id: imagePath
    description: 可为空的本地图片路径或 pack URI。
  - id: userName
    description: 不可为空的默认显示名称；Flourish 会按已配置的名称顺序拆分该值。
  return:
    description: 用于链式配置的当前 Profile builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishProfileBuilder.SetNameOrder(ArkheideSystem.Flourish.Abstract.NameOrder)
summary: 设置 first name 与 last name 的显示顺序，并同步控制名称输入框和占位首字母的顺序。
syntax:
  parameters:
  - id: nameOrder
    description: 要应用的名称顺序。
  return:
    description: 用于链式配置的当前 Profile builder。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishProfileBuilder.SetProfilePage``1
summary: 设置由 DI 解析并承载在 Profile 弹层中的 WPF 页面。
---

---
uid: ArkheideSystem.Flourish.Abstract.IFlourishProfileBuilder.SetProfilePage(System.Type)
summary: 使用运行时类型设置 Profile 弹层承载的 WPF 页面。
syntax:
  parameters:
  - id: pageType
    description: 继承自 WPF Page 的页面类型。
  return:
    description: 用于链式配置的当前 Profile builder。
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
uid: ArkheideSystem.Flourish.Abstract.ProfileUser.#ctor(System.String,System.String)
summary: 使用组合显示名称和可选图片路径创建 Profile 用户，并按 FirstLast 拆分名称。
syntax:
  parameters:
  - id: userName
    description: 不可为空的显示名称。
  - id: imagePath
    description: 可为空的本地图片路径或 pack URI。
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
uid: ArkheideSystem.Flourish.Abstract.ProfileUser.UserName
summary: 获取与 DisplayName 相同的格式化 Profile 显示名称。
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
uid: ArkheideSystem.Flourish.Abstract.ProfileSignInRequest.#ctor(System.String,System.String,System.String)
summary: 使用组合显示名称创建 Profile 登录请求，并按 FirstLast 拆分名称。
syntax:
  parameters:
  - id: userName
    description: 用户显示名称。
  - id: password
    description: 用户提交的密码。
  - id: imagePath
    description: 可选的 Profile 图片路径。
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
uid: ArkheideSystem.Flourish.Abstract.ProfileSignInRequest.UserName
summary: 获取与 DisplayName 相同的格式化登录显示名称。
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
---

---
uid: ArkheideSystem.Flourish.Abstract.IProfileAuthService.SignOutAsync(ArkheideSystem.Flourish.Abstract.ProfileUser,System.Threading.CancellationToken)
summary: 执行认证提供程序所需的异步登出工作。
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
summary: 恢复已记住的登录，或清除上次未记住的登录信息。
---

---
uid: ArkheideSystem.Flourish.Abstract.IProfileService.SignInAsync(ArkheideSystem.Flourish.Abstract.ProfileSignInRequest,System.Threading.CancellationToken)
summary: 异步认证并激活当前 Profile。
---

---
uid: ArkheideSystem.Flourish.Abstract.IProfileService.SetRememberLoginAsync(System.Boolean,System.Threading.CancellationToken)
summary: 设置是否在下次启动时恢复当前登录。
---

---
uid: ArkheideSystem.Flourish.Abstract.IProfileService.SignOutAsync(System.Threading.CancellationToken)
summary: 异步登出并删除持久化的 Profile 凭据。
---
