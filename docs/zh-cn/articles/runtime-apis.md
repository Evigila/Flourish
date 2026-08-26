---
title: 运行时 API
description: 在应用构建后使用聚焦的 Flourish Service。
---

# 运行时 API

启动 Builder 与运行时 Service 描述相同领域的不同时期：

- 功能 Builder 在 `Build()` 前记录初始草稿；
- 功能 Service 修改已构建应用并报告实时状态。

所有契约统一使用 `ArkheideSystem.Flourish.Abstract`，所有 Service 都可通过依赖注入获得。

## 状态与事件

具有状态的 UI Service 公开不可变 `Current` 快照与 `Changed` 事件。一次读取同一快照中的关联值，才能保证它们属于同一个原子状态。

```csharp
var themeState = theme.Current;
var materialState = material.Current;
var fontState = fonts.Current;

theme.Changed += (_, change) => ApplyTheme(change.Current);
```

`IThemeService.Current` 包含 `RequestedTheme`、`EffectiveTheme` 和 `IsDark`。`IMaterialEffectService.Current` 包含请求/有效材质、支持状态、应用状态与深色模式。`IFontService.Current` 包含两种字体、全部字号层级和 `PageOverrides`。字体事件保留专用 `FlourishFontChangedEventArgs`，因为 `ChangeKind` 与 `AffectedPageType` 描述了变化范围；变化后的字体状态仍通过 `args.Current` 获取。

简单状态变化使用 `FlourishStateChangedEventArgs<TState>` 或 `FlourishStateTransitionEventArgs<TState>`；只有携带额外领域变化元数据时才保留专用事件参数。

集合型 Service 也使用统一的 `Current`/`Changed` 形态。`IBackgroundTaskService.Current` 是活动任务列表，通用变化事件通过 `args.Current` 发布同一份缓存列表。`INotificationService.Current` 是 `FlourishNotificationState`，以一个原子快照同时提供 `Notifications` 与 `Version`。`ICommandRegistry` 和 `IShortcutService` 暴露活动注册，其专用事件参数通过 `Current` 提供变化后的快照，并使用 `FlourishRuntimeChangeKind` 表示 `Added`、`Updated` 或 `Removed`。

`IFlourishLocalization.Current` 包含 `Locale` 与 `AvailableLocales`；调用 `SetLocale` 切换语言，并通过 `Changed` 监听语言或来源更新。`IProfileService.Current` 是包含用户资料、登录状态与名称顺序的 `FlourishProfileState`，其 `Changed` 事件提供替换后的快照。

## 配置与可写设置

注入标准 `Microsoft.Extensions.Configuration.IConfiguration` 即可读取最终 Host 配置，其中包括 appsettings、User Secrets、环境变量、命令行以及通过 `ConfigureConfiguration` 添加的来源。

`appsettings.Flourish.json` 仍是默认可写设置文件。可在构建前使用 `IDataBuilder.SetAppSettingsFilePath` 选择其他路径。注入 `IFlourishSettingsStore` 可原子修改 `Flourish:` 下的值；成功写入后会重新加载同一套标准 `IConfiguration` 管线。

```csharp
public async ValueTask SaveEndpointAsync(
    IConfiguration configuration,
    IFlourishSettingsStore settings,
    string endpoint,
    CancellationToken cancellationToken)
{
    string? previous = configuration["Flourish:Extensions:Api:BaseUrl"];
    await settings.SetAsync(
        "Flourish:Extensions:Api:BaseUrl",
        endpoint,
        cancellationToken);
}
```

## 运行时 Service 对照

| 领域 | 运行时 Service |
| --- | --- |
| 主题与外观 | `IThemeService`、`IAppearanceService`、`IMaterialEffectService`、`IFontService` |
| 布局与交互 | `IContentLayoutService`、`IScrollService`、`IToolTipService`、`IMotionService` |
| 标题栏与资料 | `ITitleBarService`、`IProfileFlyoutService`、`IProfileService` |
| 导航 | `INavigationService` |
| 工具栏、状态、区域 | `IToolbarService`、`IStatusBarService`、`IShellRegionService` |
| 窗口与托盘 | `IWindowService`、`IWindowCloseService`、`ITrayService` |
| 项目 | `IProjectService`、`IProjectBehavior` |
| 命令 | `ICommandRegistry`、`ICommandDispatcher`、`IShortcutService` |
| 消息与工作 | `IMessageService`、`INotificationService`、`IBackgroundTaskService` |
| 本地化与设置 | `IFlourishLocalization`、`IFlourishSettingsStore`、标准 `IConfiguration` |

`INavigationService` 是完整导航门面。它的 `Current` 快照组合活动导航、路由、菜单、面板和缓存状态，并负责路由注册、菜单事务、面板修改、导航历史与页面缓存操作。

`ITitleBarService` 同样统一负责标题内容、元素可见性、搜索状态和搜索订阅。通过 `SubscribeSearch` 注册搜索处理器会返回 `IRegistration`。

## 注册生命周期

命令处理器、快捷键、运行时路由、Shell 区域项、关闭守卫和标题栏搜索处理器统一返回 `IRegistration`。功能有效期间持有租约，不再需要时调用 `Dispose()`。

`FlourishCultureRegistration` 同样实现 `IRegistration`，并额外提供 `Reload()` 刷新文化文件；释放时会自动取消注册。`IStatusBarItemHandle` 继承 `IRegistration`；它与 `FlourishNotificationHandle` 仍保留专用 API，因为两者除了释放外还可以更新实时展示。

```csharp
public sealed class RefreshBindings : IDisposable
{
    private readonly IRegistration command;
    private readonly IRegistration shortcut;

    public RefreshBindings(ICommandRegistry commands, IShortcutService shortcuts)
    {
        command = commands.Register("refresh", (_, _) =>
            ValueTask.FromResult(CommandResult.Handled));
        shortcut = shortcuts.Register(new KeyGesture(Key.F5), "refresh");
    }

    public void Dispose()
    {
        shortcut.Dispose();
        command.Dispose();
    }
}
```
