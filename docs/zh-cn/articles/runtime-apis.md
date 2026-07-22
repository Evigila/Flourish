---
title: 运行时 API
description: 在应用运行期间读取和修改 Flourish 配置、Shell 界面、项目、导航、命令、窗口、通知与后台任务。
---

# 运行时 API

Flourish 提供两层互补的配置方式：

- `IFlourishBuilder` 及其 `Configure...` 回调用于定义应用的初始对象图和启动状态。页面与服务注册、默认值选择以及启动前校验仍应放在 Builder 期完成。
- 运行时服务用于在 `Build()` 之后修改正在运行的应用。请通过依赖注入获取这些服务，通常应将其注入 Page、ViewModel 或应用服务的构造函数。

下列运行时服务均以单例注册。Builder 负责确定且可复现的启动默认值；运行时服务则适合用户偏好、插件、功能开关以及会在会话期间变化的状态。

## 状态、事件与生命周期

有状态的服务会公开不可变的 `Current` 快照，或 `ActiveTasks`、`Registrations` 等具名快照，并提供 `Changed`、`StateChanged` 或领域专用事件。收到事件后应读取新快照，不要长期保存可变 UI 对象。事件通常在发起修改的线程同步触发；配置重载、后台任务和通知过期可能在 WPF Dispatcher 之外触发，因此更新 UI 时需要按需切回 UI 线程。

注册和临时展示 API 使用可释放的租约。返回对象应只存活到对应功能不再需要为止，随后调用 `Dispose()`。这适用于 `ICommandRegistration`、`IShortcutRegistration`、`INavigationRouteRegistration`、`IShellRegionRegistration`、`IStatusBarItemHandle`、`FlourishNotificationHandle`、`IWindowCloseGuardRegistration` 以及标题栏搜索订阅。`FlourishLocaleRegistration` 不实现 `IDisposable`，需要通过 `IFlourishLocalization.Unregister` 显式移除。

## 配置与本地化

| 服务 | 运行时用途 |
| --- | --- |
| `IFlourishConfiguration` | 通过 `Current`、字符串索引器、`Get<T>` 或 `GetSection<T>` 读取 Host 的最终有效配置；可调用 `Reload()` 并监听 `Changed`。 |
| `IAppSettingsStore` | 原子执行 `SetAsync`、`RemoveAsync`、`MergeAsync`、`AppendAsync`，或在一次 `UpdateAsync` 事务中完成多项编辑。文件发生变化后会重载 Host 配置。 |
| `IFlourishLocalization` | 读取、格式化本地化键，运行时调用 `SetLocale`，以及注册、重载或注销 `lang_<locale>.json` 文件。 |

`IAppSettingsStore` 会写入基础 `appsettings.json`。`IProjectService` 会另外将项目目录持久化到 `IAppSettingsStore.FilePath` 相邻的 `projects.json`；普通运行时快照仅存在于内存中，除非对应服务明确说明会持久化。

```csharp
public async ValueTask SaveEndpointAsync(
    IAppSettingsStore settings,
    IFlourishLocalization localization,
    string endpoint,
    CancellationToken cancellationToken)
{
    await settings.UpdateAsync(editor =>
    {
        editor.Set("Api:BaseUrl", endpoint);
        editor.Merge("FeatureFlags", new { ReportsEnabled = true });
        editor.Append("Api:RecentEndpoints", endpoint);
    }, cancellationToken);

    localization.SetLocale("CN");
}
```

## 外观与 Shell 功能

| 服务 | 运行时用途 |
| --- | --- |
| `IShellFeatureService` | 通过 `SetEnabled` 启用或禁用 `TitleBar`、`Navigation`、`DynamicToolbar`、`StatusContent`、`ToolTips`、`Motion` 或 `Profile`。 |
| `IThemeService` | 使用 `SetTheme` 选择并持久化 `System`、`Light` 或 `Dark`，或通过 `ToggleTheme` 循环切换；可读取 `EffectiveTheme` 和 `IsDark`。 |
| `IFontService` | 通过 `SetFont` 原子修改全局字体及彼此独立、仅要求有限正数的 Small、Standard、Icon、Large、ExtraLarge、HeaderSize 字号；可独立修改图标字体，并通过 `PageOverrides`、`SetOverrideFont` 与 `ClearOverrideFont` 查看、设置和清除页面字体覆盖。 |
| `IToolTipService` | 在原生 WPF 与 Flourish 呈现之间切换 Flourish 自有 Tooltip，并通过 `Configure` 修改 Flourish 呈现的首次显示延迟和生成边距；原生与第三方控件不受其控制。 |
| `IMotionService` | 启用动画，修改页面/导航过渡及其时长，配置 Hover Reveal，并遵循 Windows 的减少动态效果设置。 |
| `IMaterialEffectService` | 检查并应用 `MaterialEffect`，或修改沉浸式深色模式。 |

`ShellFeature.TitleBar` 用于在 Flourish 自定义标题栏与 Windows 原生标题栏之间切换。
禁用后会恢复原生标题栏，但不会改变请求的材质效果；重新启用后会恢复 Flourish 标题栏，
并将该材质请求重新应用到自定义窗口框架。

## 标题栏、项目与搜索

| 服务 | 运行时用途 |
| --- | --- |
| `ITitleBarService` | 修改应用标题/副标题、未命名项目占位文本、Logo 及其信息字段可见性、搜索占位符、面包屑模式和各个 `TitleBarElement`。 |
| `IProjectService` | 添加、更新、查询、激活和移除 `FlourishProject` 目录元数据；修改项目模式；观察不可变快照。每次目录变更都会原子写入 `projects.json`。 |
| `IProjectBehavior` | 以异步方式新建、保存、激活和删除项目，并决定是否允许关闭。应用可以替换默认对话框与 `.txt` 文件生命周期。 |
| `ITitleBarSearchService` | 控制搜索文本、可见性、占位符、清空和焦点；观察 `QueryChanged`；通过 `Subscribe` 按注册顺序添加异步处理器。 |

未启用项目模式时，标题选择器只显示并列出应用标题。启用项目模式后，选择器显示活动项目或未命名项目占位文本，并列出全部项目与“新建项目”。`StoragePath == null` 表示项目未持久化，占位文本只用于显示。

直接调用 `IProjectService` 会更新 Shell 状态与持久化目录，但不会显示生命周期对话框或访问项目文件。启用项目模式时，选择项目、新建项目、右键删除、使用 Ctrl+S 保存以及关闭都会路由到 `IProjectBehavior`；内置 Ctrl+S 注册使用低优先级。未启用项目模式时，这些 Shell 路由保持停用，由应用代码管理单项目保存行为。替换该行为会改变项目对话框与文件操作，但不会停用 `IProjectService` 的目录持久化。

激活操作只更新活动目录项与标题。应用仍负责加载或切换业务内容。参见[项目](projects.md)。

```csharp
public sealed class SearchModule(
    ITitleBarSearchService search,
    ISearchIndex searchIndex) : IDisposable
{
    private readonly IDisposable subscription = search.Subscribe(async (query, token) =>
    {
        await searchIndex.UpdateResultsAsync(query.Text, token);
    });

    public void Open()
    {
        search.SetVisible(true);
        search.SetPlaceholder("搜索报表");
        search.Focus();
    }

    public void Dispose() => subscription.Dispose();
}
```

## 导航、路由与页面缓存

| 服务 | 运行时用途 |
| --- | --- |
| `INavigationService` | 按区分大小写的路由键或页面类型导航，执行异步导航，读取当前路由/参数，并控制后退和前进历史。 |
| `INavigationPanelService` | 启用、移动、调整导航面板尺寸，以及打开、关闭或切换面板。 |
| `INavigationMenuService` | 通过一次 `Update` 事务原子修改分组、固定项、页面/命令项、顺序、文字、可见性、启用状态与树展开状态。 |
| `INavigationRouteRegistry` | `Register` 或 `Upsert` 路由，移除路由，并修改其 `FlourishPageCacheMode`。 |
| `IPageCacheService` | 按页面类型修改缓存模式，读取已缓存页面类型，逐页驱逐或清空全部缓存实例。 |

应先注册路由，再公开指向该路由的菜单项。释放路由租约之前，应先移除对应菜单项。

```csharp
public sealed class DiagnosticsModule : IDisposable
{
    private readonly INavigationRouteRegistration route;
    private readonly INavigationMenuService menu;

    public DiagnosticsModule(
        INavigationRouteRegistry routes,
        INavigationMenuService menu,
        INavigationService navigation)
    {
        this.menu = menu;
        route = routes.Register(new FlourishNavigationRoute(
            "runtime.diagnostics",
            typeof(DiagnosticsPage),
            FlourishPageCacheMode.Enabled));

        menu.Update(editor =>
        {
            editor.AddGroup("runtime", "运行时");
            editor.AddItem("runtime", FlourishNavigationMenuItem.Page(
                "runtime.diagnostics.item", "runtime.diagnostics", "诊断", "\uE9D2"));
        });

        navigation.Navigate("runtime.diagnostics");
    }

    public void Dispose()
    {
        menu.Update(editor =>
        {
            editor.RemoveItem("runtime.diagnostics.item");
            editor.RemoveGroup("runtime");
        });
        route.Dispose();
    }
}
```

## 工具栏、状态栏与 Shell 区域

| 服务 | 运行时用途 |
| --- | --- |
| `IToolbarService` | 启用工具栏；替换默认或页面专属定义；添加、覆盖、移动、显示、启用、移除或清空 `FlourishToolbarItem`；修改页面的 `IconOnly` 模式。 |
| `IStatusBarService` | 启用自定义内容和内置 LAN/电源指示；添加、更新、移动、显示、隐藏、移除或清空 `FlourishStatusItem`。`Show` 可创建定时项目并返回可释放句柄。 |
| `IShellRegionService` | 向 `FlourishRegion` 添加或覆盖 WPF 内容工厂，并启用、重排、移除或清空注册项。 |

工具栏和导航中的命令项通过 `ICommandDispatcher` 调度。

## 命令与快捷键

`ICommandRegistry.Register` 可添加异步处理器，并指定可选的可执行谓词、重复策略和优先级。`ICommandDispatcher.CanExecute` 用于查询命令当前是否可用；`ExecuteAsync` 用于调度命令，并返回捕获了执行结果的 `CommandResult`。`IShortcutService.Register` 可将 WPF `KeyGesture` 映射到命令，并配置应用、窗口或页面作用域及冲突策略。

默认情况下，文本输入控件获得键盘焦点时不会派发快捷键，以保留正常输入、剪贴板、编辑、AltGr 与 IME 行为。只有确实需要在编辑文字时保持生效的快捷键，才应将 `ShortcutRegistrationOptions.AllowWhenTextInputFocused` 设置为 `true`。

命令处理程序通过 `ICommandRegistry` 注册，并通过 `ICommandDispatcher` 调用。应在所属功能的生命周期内持有 `ICommandRegistration`，并通过释放注册来移除处理程序。命令映射与整个 Host 生命周期一致时，实现 `ICommandParser` 即可由 Flourish 自动管理其租约。

```csharp
public sealed class RefreshBindings : IDisposable
{
    private readonly ICommandRegistration command;
    private readonly IShortcutRegistration shortcut;

    public RefreshBindings(
        ICommandRegistry commands,
        IShortcutService shortcuts,
        IDataRefresher refresher)
    {
        command = commands.Register("data.refresh", async (_, token) =>
        {
            await refresher.RefreshAsync(token);
            return CommandResult.Handled;
        });

        shortcut = shortcuts.Register(
            new KeyGesture(Key.F5, ModifierKeys.Control),
            "data.refresh");
    }

    public void Dispose()
    {
        shortcut.Dispose();
        command.Dispose();
    }
}
```

当外部状态改变了可执行谓词的结果时，调用 `NotifyCanExecuteChanged(commandKey)`。

## 窗口、托盘、关闭、Profile、消息与通知

| 服务 | 运行时用途 |
| --- | --- |
| `IWindowService` | 修改边界、尺寸约束、缩放模式、置顶/任务栏状态；居中、显示、隐藏、激活、最小化、最大化或还原 Shell。 |
| `ITrayService` | 启用通知区域行为，修改 Tooltip，最小化到托盘、还原或请求退出。 |
| `IWindowCloseService` | 选择 `Prompt`、`Close` 或 `MinimizeToTray`；注册有序异步关闭守卫；检查或发起关闭请求。 |
| `IProfileFlyoutService` | 启用、显示、隐藏或切换 Profile Flyout，并替换其中的 WPF `Page` 内容。 |
| `IProfileService` | 读取 Profile/登录状态，初始化已记住的登录，登录，修改“记住登录”状态或退出登录。 |
| `IMessageService` | 同步或通过 `ShowAsync` 显示标准/自定义选项模态消息；异步取消只在对话框打开前有效。 |
| `INotificationService` | `Show` 或 `Upsert` 非模态通知，读取活动通知，关闭单项/全部通知，并可在激活通知时派发命令。 |

关闭守卫和通知都会返回可释放租约。通知句柄还可在关闭前更新通知内容。
`IProfileAuthService` 仍是启动期注册的认证提供程序；`IProfileService` 是运行时调用它的门面。

## 后台任务

`IBackgroundTaskService.AddTask` 可在运行时将异步工作加入有界队列。任务通过 `FlourishBackgroundTaskContext` 获得协作式取消和进度上报能力；返回句柄提供 `Cancel`、`Snapshot`，以及通过结果对象捕获成功、取消或失败的 `Completion` Task。

```csharp
public FlourishBackgroundTaskHandle StartExport(
    IBackgroundTaskService tasks,
    IReportExporter exporter)
{
    return tasks.AddTask(
        new FlourishBackgroundTaskMetadata("导出报告", "正在写入文件", "\uE74E"),
        async context =>
        {
            for (var step = 1; step <= 10; step++)
            {
                await exporter.WritePartAsync(step, context.CancellationToken);
                context.ReportProgress(step / 10d);
            }
        });
}
```

任务委托不会在 WPF UI 线程运行。可监听 `TasksChanged` 更新 Shell 或应用 UI，并将控件更新切回 UI 线程。

## 相关指南

- [IFlourishBuilder](flourish-builder.md)与[依赖注入](configure-services.md)
- [应用数据](configure-data.md)、[项目](projects.md)、[导航](navigation.md)与[命令调度](commands.md)
- [动态工具栏](dynamic-toolbar.md)、[状态栏](status-bar.md)与[后台任务](background-tasks.md)
- [窗口](configure-window.md)、[Profile](configure-profile.md)与[消息服务](message-service.md)
