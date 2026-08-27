---
title: 运行时 API
description: 在应用构建后使用按功能划分的 Flourish Service。
---

# 运行时 API

Builder 配置启动状态，Service 修改已构建应用并报告实时状态：

- 功能 Builder 在 `Build()` 前记录初始草稿；
- 功能 Service 修改已构建应用并报告实时状态。

所有契约位于 `ArkheideSystem.Flourish.Abstract`，Service 均可通过依赖注入获得。

## 状态与事件

有状态的 UI Service 公开不可变 `Current` 快照和 `Changed` 事件；关联值应从同一快照读取。

```csharp
var themeState = theme.Current;
var materialState = material.Current;
var fontState = fonts.Current;

theme.Changed += (_, change) => ApplyTheme(change.Current);
```

`IThemeService.Current` 包含 `RequestedTheme`、`EffectiveTheme` 和 `IsDark`；`IMaterialEffectService.Current` 包含材质、支持与应用状态及深色模式；`IFontService.Current` 包含字体、字号和 `PageOverrides`。`FontChangedEventArgs` 用 `ChangeKind` 与 `AffectedPageType` 描述范围，新状态仍在 `args.Current`。

简单变化使用 `StateChangedEventArgs<TState>` 或 `StateTransitionEventArgs<TState>`；只有额外领域元数据需要专用事件参数。

集合 Service 同样使用 `Current`/`Changed`。`IBackgroundTaskService.Current` 是活动任务列表；`INotificationService.Current` 以 `NotificationState` 同时提供 `Notifications` 与 `Version`。`ICommandRegistry` 和 `IShortcutService` 公开活动注册，并用 `CollectionChangeKind` 表示 `Added`、`Updated` 或 `Removed`。

`ILocalizationService.Current` 包含 `Locale` 与 `AvailableLocales`；`SetLocale` 切换语言，`Changed` 监听语言或来源。`IProfileService.Current` 是包含资料、登录状态和名称顺序的 `ProfileState`。

## 配置与可写设置

注入 `Microsoft.Extensions.Configuration.IConfiguration` 可读取 appsettings、User Secrets、环境变量、命令行及 `ConfigureConfiguration` 添加的最终 Host 配置。

默认可写文件是 `appsettings.Flourish.json`；构建前可用 `IDataBuilder.SetAppSettingsFilePath` 更改。`ISettingsStore` 原子修改 `Flourish:` 值，成功后重新加载标准 `IConfiguration` 管线。

```csharp
public async ValueTask SaveEndpointAsync(
    IConfiguration configuration,
    ISettingsStore settings,
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
| 本地化与设置 | `ILocalizationService`、`ISettingsStore`、标准 `IConfiguration` |

`INavigationService.Current` 组合活动导航、路由、菜单、面板和缓存状态；该服务还负责路由、菜单事务、面板、历史与缓存操作。

`ITitleBarService` 管理标题、元素可见性、搜索状态和订阅；`SubscribeSearch` 返回 `IRegistration`。

## 注册生命周期

命令、快捷键、运行时路由、Shell 区域项、关闭守卫和搜索处理器均返回 `IRegistration`；在功能有效期间持有，不再需要时 `Dispose()`。

`CultureRegistration` 实现 `IRegistration`，并用 `Reload()` 刷新文化文件；释放即取消注册。`IStatusBarItemHandle` 继承 `IRegistration`，它和 `NotificationHandle` 还可更新实时展示。

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
