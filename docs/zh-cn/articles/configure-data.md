---
title: 应用数据
description: 配置 Flourish 本地化、持久化设置路径与项目目录。
---

# 应用数据

`ConfigureData` 配置内置界面语言、翻译文件和持久化路径。未调用 `ConfigureData` 或 `SetLocale` 时使用内置英文（`en-US`）；偏好和 Profile 凭据使用 Generic Host 配置，项目元数据使用独立目录文件。

## 选择内置语言

Flourish 内置 `en-US` 和 `zh-CN`。语言标识不区分大小写，返回规范 BCP 47 形式；下划线输入会转换为连字符。

```csharp
builder.ConfigureData(data => data.SetLocale("en-US"));
```

省略 `ConfigureData` 时默认使用 `en-US`。持久化默认启用：合法的 `Flourish:Preferences:Locale` 优先，`SetLocale` 变更会写回；传入 `usePersistedPreference: false` 可让代码值始终优先。Flourish 不会自动翻译应用传入的标题、占位文本、标签、消息或选项。

## 接入 Essential Culture

使用 `Arkheide.Essential.Culture` 翻译应用文案时，直接安装其 WPF 包。可选的 `Flourish.Extensions.Culture.WPF` 项目现已归入本仓库；改名后的包尚未发布。

```bash
dotnet add package Arkheide.Essential.Culture.Wpf
```

在组合应用前设置 Essential Culture 的初始语言：

```csharp
using ArkheideSystem.Essential.Culture;

Localizer.Current.SetCulture("en-US");

var flourish = ApplicationBuilder
    .CreateDefaultBuilder(args)
    .ConfigureData(data => data.SetLocale("en-US"))
    .Build();
```

`ILocalizationService` 仍是 Flourish 的文化访问点。在独立扩展发布前，应用需要在语言选择变化时显式同步 Essential Culture：

```csharp
localization.SetLocale("zh-CN");
Localizer.Current.SetCulture("zh-CN");

string locale = localization.Current.Locale;
IReadOnlyList<string> available = localization.Current.AvailableLocales;
localization.Changed += OnLocalizationChanged;
```

应用维护自己的 `Culture.json`；XAML 使用 Essential.Culture 的 `Localize` 与 `KeyBinding`。Shell 中以普通字符串保存的文案应由应用在创建时解析。用户输入、项目名称和搜索内容不会自动翻译。

## 覆盖内置文化目录

Flourish 内嵌 `FlourishCulture.Json`。输出目录中的同名文件会自动按“语言与键”覆盖内置单元，无需重复全部语言或键。

还可以通过 `AddCultureFile(path)` 注册其他目录。所有目录文件仍必须命名为 `FlourishCulture.Json`；组合多个目录时将它们放在不同文件夹中。

```csharp
builder.ConfigureData(data =>
{
    data
        .SetLocale("en-US")
        .AddCultureFile("Locales/FlourishCulture.Json");
});
```

Flourish 在 `Build()` 时读取目录。文件不存在抛出 `FileNotFoundException`，文件名无效抛出 `ArgumentException`；文件不可读、JSON 无效、对象为空、键重复或为空、规范化后语言重复，以及译文空白或非字符串时抛出 `InvalidDataException`。

格式与 Essential Culture 一致：翻译键位于最外层，语言是内层属性。用户目录只需要提供待覆盖的单元：

```json
{
  "TitleBar.Back": {
    "zh-CN": "上一页",
    "fr-FR": "Précédent"
  },
  "Tray.Show": {
    "fr-FR": "Ouvrir"
  }
}
```

目录按注册顺序合并；只有语言和键都相同时，后注册的目录才会覆盖先前值。每次查找按以下优先级返回文本：

1. 选中语言的自定义值。
2. 选中语言的内置值。
3. 自定义 `en-US` 值。
4. 内置 `en-US` 值。
5. 键本身。

因此可以只覆盖单个英文键，或只补充新语言的部分键，其余内容继续回退到英文。

## 翻译键

内置 `FlourishCulture.Json` 目录定义以下键。`{0}` 是格式化占位符，覆盖对应文本时应保留它。

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
| `Window.BackgroundTasksCloseTitle` | Stop background tasks? | 中止后台任务？ |
| `Window.BackgroundTasksClosePrompt` | Active background tasks: {0}. Closing the window will cancel them. Stop the tasks and exit? | 仍有 {0} 个后台任务正在进行。关闭窗口将取消这些任务。是否中止任务并退出？ |
| `Window.BackgroundTasksKeepRunning` | Keep running | 继续运行 |
| `Window.BackgroundTasksStopAndExit` | Stop tasks and exit | 中止任务并退出 |
| `Tray.Show` | Show | 显示 |
| `Tray.Exit` | Exit | 退出 |
| `Status.Connected` | Connected | 已连接 |
| `Status.Disconnected` | Disconnected | 未连接 |

## Host 配置

`ApplicationBuilder.CreateDefaultBuilder(args)` 使用标准 Generic Host 配置管线；Flourish 从同一个 `IConfiguration` 读取设置。

可写偏好源默认为 `appsettings.Flourish.json`，位于 Host 基础 appsettings 源之前，并在首次写入时创建。它只发布顶级 `Flourish` 对象；其他顶级属性不进入 Host 配置。不要在构建或部署时用种子文件覆盖它。

普通回退值使用 Builder 参数；只有应用策略必须覆盖已保存偏好时，才在基础 `appsettings.json` 中设置：

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

配置键为 `Flourish:Preferences:Theme`。读取遵循完整的 Host 优先级：`appsettings.Flourish.json`、`appsettings.json`、`appsettings.{Environment}.json`、User Secrets、应用注册的配置源、环境变量、命令行参数；越靠后的来源优先级越高。`Host.CreateDefaultBuilder` 只会自动加载基础文件和当前环境文件；`appsettings.User.json` 等其他名称必须由应用代码显式注册。

通过 `ConfigureConfiguration` 注册该文件：

```csharp
builder.ConfigureConfiguration((_, configuration) =>
    configuration.AddJsonFile(
        "appsettings.User.json",
        optional: true,
        reloadOnChange: true));
```

回调接收标准 `IConfigurationBuilder`，可使用 `AddJsonFile` 或其他 provider。应用源位于 appsettings 与 User Secrets 之后、环境变量与命令行之前；后注册源覆盖先注册源，但不能覆盖环境变量或命令行。

`ISettingsStore` 只接受 `Flourish:` 后代路径，不能修改其他顶级节。它保留无关节，但会重新序列化整个 JSON，可能改变格式并移除注释；其他进程也管理配置时应使用独立文件。目录必须可写，已有文件必须是有效 JSON 对象，已有 `Flourish` 属性也必须是对象。

## 用户偏好

界面偏好默认会恢复并更新：

```csharp
builder
    .ConfigureData(data =>
        data.SetLocale("en-US"))
    .ConfigureWindow(window =>
        window
            .SetSize(1280, 720)
            .SetManualPosition(80, 60)
            .SetState(WindowState.Normal))
    .ConfigureNavigation(navigation =>
        navigation
            .SetInitiallyOpen()
            .SetPanelWidth(260, 64, 520, 180)
            .SetLastNavigationPersistence());
```

每个偏好的最后一次 Builder 调用决定回退值和持久化策略。`usePersistedPreference: false` 让代码值优先并停止写回，但不删除旧值。启用持久化时，完整合法的 Host 值优先；缺失、不完整或无效时使用 Builder 回退，复合设置按整组恢复。

可持久化项包括语言、主题、窗口还原状态、导航状态、Profile 姓名顺序、动效、滚动、全局字体、内容布局、材质、配色和圆角。运行时变更会合并后原子写入，并在 Host 停止时刷新。最小化状态不恢复；最大化会保留正常还原边界；完全离屏的位置会移回可触达范围。

应用能力和结构不会持久化，包括功能开关、页面类型与路由、处理程序与工厂、品牌信息、窗口约束、`ResizeMode`、任务栏可见性、文化文件和页面字体覆盖。保存数据不能重新启用代码已关闭的能力。

Flourish 通过最终有效的 `IConfiguration` 读取偏好，并保留 Host 的正常优先级。默认写入应用根目录中的 `appsettings.Flourish.json`。可在 `ConfigureData` 中选择其他 JSON 文件以及独立的项目目录文件：

```csharp
builder.ConfigureData(data => data
    .SetAppSettingsFilePath("Data/appsettings.Flourish.json")
    .SetProjectCatalogFilePath("Data/projects.json"));
```

相对路径基于 `AppContext.BaseDirectory`，也可使用绝对路径。自定义设置文件只发布 `Flourish` 节并位于基础 `appsettings.json` 之前；User Secrets、环境变量和命令行仍有更高优先级。选择基础 `appsettings.json` 时保留完整 Host provider 行为，但 Flourish 只能写入 `Flourish` 后代路径。设置与项目目录路径必须是不同的 `.json` 文件，首次写入会创建父目录。

使用 `ISettingsStore.RemoveAsync` 和完整 `Flourish:` 路径重置偏好；`usePersistedPreference: false` 只会忽略并停止更新，不会删除旧值。

## 项目目录

`IProjectService` 将有序项目元数据与活动项目 ID 存储在 `SetProjectCatalogFilePath` 选择的文件中，默认是应用根目录下的 `projects.json`。该路径独立于 `ISettingsStore.FilePath`，不是 Host 配置源，也不参与配置优先级。

项目服务启动时加载目录，并在每次变更时原子写入。替换 `IProjectBehavior` 不会禁用目录持久化；目录必须可写。未持久化项目与生命周期见[项目](projects.md)。

## User Secrets

已记住的 Profile 凭据使用应用的 User Secrets 配置。[用户资料（Profile）](configure-profile.md)说明所需的 `UserSecretsId`、凭据保护以及 provider 不可用时的行为。

## 相关功能

- [标题栏](configure-title-bar.md)、[窗口](configure-window.md)、[后台任务](background-tasks.md)、[状态栏](status-bar.md)和[消息服务](message-service.md)
- [主题](configure-themes.md)
- [用户资料（Profile）](configure-profile.md)
- [项目](projects.md)
- [`IApplicationBuilder`](flourish-builder.md)
