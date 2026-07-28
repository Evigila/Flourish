---
title: 应用数据
description: 配置 Flourish 本地化、持久化设置路径与项目目录。
---

# 应用数据

`ConfigData` 用于配置 Flourish 内置界面的语言、自定义翻译文件与持久化设置路径。即使没有调用 `ConfigData` 或 `InitLocale`，Flourish 也会使用内置英文（`en-US`），因此内置界面始终具有可用文案。偏好与受保护的 Profile 凭据使用 .NET Generic Host 管理的配置；项目元数据使用可独立配置的目录文件。

## 选择内置语言

Flourish 内置 `en-US` 和 `zh-CN`。语言标识不区分大小写，并以规范的 BCP 47 形式返回。建议使用连字符；下划线输入也会被接受并转换为连字符。

```csharp
builder.ConfigData(data => data.InitLocale("en-US"));
```

省略 `ConfigData` 时，Flourish 默认使用 `en-US`。持久化默认启用，因此有效配置中的合法 `Flourish:Preferences:Locale` 会优先，后续 `SetLocale` 变更也会写回。代码配置的语言必须始终优先时，传入 `usePersistedPreference: false`。应用传入的标题、搜索占位文本、导航标签、自定义状态项标签、对话框消息和自定义选项文本不会自动翻译。

## 添加自定义语言

`InitLocaleFile(path)` 注册 UTF-8 JSON 文件。文件名提供语言标识，必须使用 `lang_<locale>.json` 格式；语言部分可以包含字母、数字、连字符和下划线，并且分隔符两侧都必须有非空子标识。文件名中的标识使用与 `InitLocale` 相同的规范化规则。

```csharp
builder.ConfigData(data =>
{
    data
        .InitLocale("en-US")
        .InitLocaleFile("Locales/lang_en-US.json");
});
```

Flourish 在 `Build()` 应用配置时读取已注册的语言文件。文件不存在时抛出 `FileNotFoundException`；文件名无效时抛出 `ArgumentException`；文件不可读、JSON 无效、对象为空、键重复或为空、值为空或非字符串时抛出 `InvalidDataException`。

语言文件是扁平 JSON 对象，可以只提供需要覆盖的键：

```json
{
  "TitleBar.Back": "上一页",
  "Tray.Show": "打开"
}
```

为同一语言多次调用 `InitLocaleFile` 时，Flourish 会按注册顺序合并文件，后添加的文件会覆盖先添加文件中的同名键。每次查找按以下优先级返回文本：

1. 选中语言的自定义值。
2. 选中语言的内置值。
3. 自定义 `en-US` 值。
4. 内置 `en-US` 值。
5. 键本身。

因此，`lang_fr-FR.json` 等自定义语言可以只定义部分界面文本，其余键会回退到英文。

## 翻译键

内置语言文件定义以下键。`{0}` 是格式化占位符，覆盖对应文本时应保留它。

| 键 | 英文（`en-US`） | 简体中文（`zh-CN`） |
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

## Host 配置

`FlourishBuilder.CreateDefaultBuilder(args)` 使用标准 Generic Host 配置管线。Flourish 从应用通过 `HostBuilderContext.Configuration` 和依赖注入获得的同一个 `IConfiguration` 中读取设置。

Flourish 的可写偏好配置源默认为 `appsettings.Flourish.json`。它会在 Host
基础 appsettings 配置源之前显式注册，并在首次写入偏好时创建。不要在每次构建或
部署时用种子文件覆盖它。

普通回退值应通过 Builder 参数配置。只有应用策略必须覆盖已持久化的用户偏好时，
才在应用的基础 `appsettings.json` 中设置对应值：

```json
{
  "Flourish": {
    "Preferences": {
      "Theme": "System"
    }
  }
}
```

应用自己的基础文件可以按常规方式复制到输出目录：

```xml
<ItemGroup>
  <None Update="appsettings.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
</ItemGroup>
```

配置键为 `Flourish:Preferences:Theme`。读取遵循完整的 Host 优先级：`appsettings.Flourish.json`、`appsettings.json`、`appsettings.{Environment}.json`、User Secrets、环境变量、命令行参数；越靠后的来源优先级越高。`Host.CreateDefaultBuilder` 只会自动加载基础文件和当前环境文件；`appsettings.User.json` 等其他名称必须由应用代码显式注册。

通过 `ConfigAppConfiguration` 添加该文件或其他 Microsoft 配置 provider：

```csharp
builder.ConfigAppConfiguration((_, configuration) =>
    configuration.AddJsonFile(
        "appsettings.User.json",
        optional: true,
        reloadOnChange: true));
```

这些回调在默认 Host 与 Flourish 配置源注册完成后执行，因此其新增来源具有更高优先级。请按照预期覆盖顺序注册来源。

Flourish 写入所选文件时会保留无关设置，但会重新序列化整个 JSON 对象，因此文档会被重新格式化，注释也会被移除。所选目录必须可写；已有文件必须是根节点为对象的有效 JSON。

## 用户偏好

界面用户偏好默认会恢复并更新，因此正常调用已经足够：

```csharp
builder
    .ConfigData(data =>
        data.InitLocale("en-US"))
    .ConfigWindow(window =>
        window
            .InitWindowSize(1280, 720)
            .InitManualWindowPosition(80, 60)
            .InitWindowState(WindowState.Normal))
    .ConfigNavigation(navigation =>
        navigation
            .InitInitiallyOpen()
            .InitPanelWidth(260, 64, 520, 180)
            .UseLastNavigation());
```

对于每个逻辑偏好，最后一次 Builder 调用同时决定回退值和持久化策略。代码必须始终采用本次调用值并停止写回运行时变更时，传入 `usePersistedPreference: false`；这不会删除旧的存储值。持久化启用时，完整且合法的有效 Host 配置值优先，缺失、不完整或无效的值则保留 Builder 回退值。窗口大小、位置、字体比例、配色和动效时长等复合设置会整组恢复，不会把部分保存字段与部分回退字段混合。

可持久化范围包括：语言；主题模式；窗口还原大小、位置、状态、置顶和关闭到通知区域行为；导航栏方向、开合状态、用户调整后的宽度与最后路由；Profile 姓名顺序；各类动效；平滑滚动；全局字体；居中内容布局；材质；主题配色；圆角。运行时变更会先合并再原子更新 appsettings，并在 Host 停止期间刷新待写入内容。Flourish 不会恢复最小化状态；窗口最大化时会保留正常还原边界；完全移出屏幕的持久化位置会被移回当前虚拟桌面的可触达范围。

应用能力和结构不属于用户偏好。Flourish 不会持久化标题栏、导航、Profile、项目、工具栏或状态栏的能力开关，也不会持久化页面类型与路由、处理程序与工厂、品牌信息、窗口最小/最大约束、ResizeMode、任务栏可见性、语言文件注册或页面专用字体覆盖。因此，保存的数据不能重新启用应用代码已经关闭的能力。

Flourish 通过最终有效的 `IConfiguration` 读取偏好，并保留 Host 的正常优先级。默认写入应用根目录中的 `appsettings.Flourish.json`。可在 `ConfigData` 中选择其他 JSON 文件以及独立的项目目录文件：

```csharp
builder.ConfigData(data => data
    .InitAppSettingsFilePath("Data/appsettings.Flourish.json")
    .InitProjectCatalogFilePath("Data/projects.json"));
```

相对路径以 `AppContext.BaseDirectory` 为基准，也可以传入绝对路径。所选设置文件与基础 `appsettings.json` 不同时，Flourish 会把它插入该基础配置源之前，并保留应用的全部 Host appsettings 配置源；User Secrets、环境变量与命令行仍保持正常的更高优先级。显式选择基础 `appsettings.json` 时，Flourish 会改为写入该共享文件。两个路径必须指向不同的 `.json` 文件，首次写入时会创建父目录。

需要重置某个已保存偏好时，使用 `IAppSettingsStore.RemoveAsync`。传入 `usePersistedPreference: false` 只表示忽略并停止更新该值，不会隐式删除已有用户选择。

## 项目目录

`IProjectService` 将有序项目元数据与活动项目 ID 存储在 `InitProjectCatalogFilePath` 选择的文件中，默认是应用根目录下的 `projects.json`。该路径独立于 `IAppSettingsStore.FilePath`，不是 Host 配置源，也不参与配置优先级。

项目服务启动时会加载该目录，并在每次目录变更时执行原子写入。注册替换的 `IProjectBehavior` 只会改变项目对话框与文件生命周期，不会禁用目录持久化。目录必须可写。未持久化项目与生命周期行为参见[项目](projects.md)。

## User Secrets

已记住的 Profile 凭据使用应用的 User Secrets 配置。[用户资料（Profile）](configure-profile.md)说明所需的 `UserSecretsId`、凭据保护以及 provider 不可用时的行为。

## 相关功能

- [标题栏](configure-title-bar.md)、[窗口](configure-window.md)、[后台任务](background-tasks.md)、[状态栏](status-bar.md)和[消息服务](message-service.md)使用已本地化的内置文案。
- [主题](configure-themes.md)通过 Host 配置持久化用户选择的主题。
- [用户资料（Profile）](configure-profile.md)说明已记住凭据与 User Secrets 配置。
- [项目](projects.md)说明持久化项目目录与可替换生命周期行为。
- [`IFlourishBuilder`](flourish-builder.md)说明配置回调的应用时机。
