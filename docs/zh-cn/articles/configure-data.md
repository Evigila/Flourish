---
title: 应用数据
description: 配置 Flourish 的本地化，并使用 Flourish 与应用共享的 Generic Host 配置。
---

# 应用数据

`ConfigureData` 用于配置 Flourish 内置界面的语言和自定义翻译文件。即使没有调用 `ConfigureData` 或 `SetLocale`，Flourish 也会使用内置英文（`EN`），因此内置界面始终具有可用文案。偏好与受保护的 Profile 凭据使用 .NET Generic Host 管理的配置。

## 选择内置语言

Flourish 内置 `CN` 和 `EN`。语言标识不区分大小写，并会在构建应用时归一化。

```csharp
builder.ConfigureData(data => data.SetLocale("EN"));
```

省略 `ConfigureData` 时，Flourish 默认使用 `EN`。只有在选择其他内置或自定义语言时才需要调用 `SetLocale(locale)`。应用传入的标题、搜索占位文本、导航标签、自定义状态项标签、对话框消息和自定义选项文本不会自动翻译。

## 添加自定义语言

`AddLocale(path)` 注册 UTF-8 JSON 文件。文件名提供语言标识，必须使用 `lang_<locale>.json` 格式；语言部分可以包含字母、数字、连字符和下划线。

```csharp
builder.ConfigureData(data =>
{
    data
        .SetLocale("EN")
        .AddLocale("Locales/lang_EN.json");
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

为同一语言多次调用 `AddLocale` 时，Flourish 会按注册顺序合并文件，后添加的文件会覆盖先添加文件中的同名键。每次查找按以下优先级返回文本：

1. 选中语言的自定义值。
2. 选中语言的内置值。
3. 自定义 `EN` 值。
4. 内置 `EN` 值。
5. 键本身。

因此，`lang_FR.json` 等自定义语言可以只定义部分界面文本，其余键会回退到英文。

## 翻译键

内置语言文件定义以下键。`{0}` 是格式化占位符，覆盖对应文本时应保留它。

| 键 | 英文（`EN`） | 简体中文（`CN`） |
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

在基础 `appsettings.json` 中配置初始主题：

```json
{
  "Flourish": {
    "Preferences": {
      "Theme": "System"
    }
  }
}
```

桌面应用项目可以这样把文件复制到输出目录：

```xml
<ItemGroup>
  <None Update="appsettings.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
</ItemGroup>
```

配置键为 `Flourish:Preferences:Theme`。读取遵循 Host 的完整优先级，因此环境专用 appsettings、User Secrets、环境变量和命令行参数都可以覆盖基础文件。用户在运行时更改主题时，Flourish 只写 Host 内容根目录中的基础 `appsettings.json`；更高优先级的来源仍可能在下次启动时再次覆盖该值。

Flourish 写入基础文件时会保留无关设置，但会重新序列化整个 JSON 对象，因此文档会被重新格式化，注释也会被移除。内容根目录必须可写；已有文件必须是根节点为对象的有效 JSON。

## User Secrets

已记住的 Profile 凭据使用应用的 User Secrets 配置。[用户资料（Profile）](configure-profile.md)说明所需的 `UserSecretsId`、凭据保护以及 provider 不可用时的行为。

## 相关功能

- [标题栏](configure-title-bar.md)、[窗口](configure-window.md)、[后台任务](background-tasks.md)、[状态栏](status-bar.md)和[消息服务](message-service.md)使用已本地化的内置文案。
- [主题](configure-themes.md)通过 Host 配置持久化用户选择的主题。
- [用户资料（Profile）](configure-profile.md)说明已记住凭据与 User Secrets 配置。
- [`IFlourishBuilder`](flourish-builder.md)说明配置回调的应用时机。
