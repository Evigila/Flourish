---
title: Application data
description: Configure localization and use the Generic Host configuration shared by Flourish and the application.
---

# Application data

`ConfigureData` controls Flourish built-in interface language and custom locale files. Localization is always available: when `ConfigureData` or `SetLocale` is omitted, Flourish uses the built-in English (`EN`) locale. Preferences and protected profile credentials use the configuration owned by the .NET Generic Host.

## Select a built-in locale

Flourish includes `CN` and `EN`. Locale identifiers are case-insensitive and normalized when the application is built.

```csharp
builder.ConfigureData(data => data.SetLocale("EN"));
```

Flourish uses `EN` when `ConfigureData` is omitted. Call `SetLocale(locale)` only when selecting another built-in or custom locale. Application-provided text such as titles, search placeholders, navigation labels, custom status-item labels, dialog messages, and custom option text is not translated automatically.

## Add a custom locale

`AddLocale(path)` registers a UTF-8 JSON file. The file name supplies the locale identifier and must follow `lang_<locale>.json`; the locale segment may contain letters, digits, hyphens, and underscores.

```csharp
builder.ConfigureData(data =>
{
    data
        .SetLocale("EN")
        .AddLocale("Locales/lang_EN.json");
});
```

Flourish reads registered locale files while `Build()` applies configuration. A missing file throws `FileNotFoundException`. An invalid file name throws `ArgumentException`. Unreadable files, malformed JSON, an empty object, duplicate or empty keys, and empty or non-string values throw `InvalidDataException`.

Locale files are flat JSON objects. They may contain only the keys they need to override:

```json
{
  "TitleBar.Back": "Previous",
  "Tray.Show": "Open"
}
```

Calling `AddLocale` more than once for the same locale merges the files in registration order. A later file replaces earlier values for the same key. For each lookup, Flourish uses this priority:

1. Custom value for the selected locale.
2. Built-in value for the selected locale.
3. Custom `EN` value.
4. Built-in `EN` value.
5. The key itself.

This lookup also allows a custom locale such as `lang_FR.json` to define only part of the interface while the remaining keys fall back to English.

## Translation keys

The built-in locale files define the following keys. `{0}` is a format placeholder and must remain in custom values that use it.

| Key | English (`EN`) | Simplified Chinese (`CN`) |
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

## Host configuration

`FlourishBuilder.CreateDefaultBuilder(args)` uses the standard Generic Host configuration pipeline. Flourish reads its settings from the same `IConfiguration` that applications receive through `HostBuilderContext.Configuration` and dependency injection.

Place the initial theme in the base `appsettings.json` file:

```json
{
  "Flourish": {
    "Preferences": {
      "Theme": "System"
    }
  }
}
```

Copy the file to the application output in a desktop project:

```xml
<ItemGroup>
  <None Update="appsettings.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
</ItemGroup>
```

The configuration key is `Flourish:Preferences:Theme`. Reads follow the complete Host precedence, so environment-specific appsettings files, User Secrets, environment variables, and command-line arguments can override the base file. A runtime theme change writes only the base `appsettings.json` in the Host content root; a higher-priority source can therefore override that value again on the next launch.

Flourish preserves unrelated settings when it writes the base file, but serializes the complete JSON object again. This reformats the document and removes comments. The content root must be writable, and an existing file must contain valid JSON with an object at its root.

## User Secrets

Remembered Profile credentials use the application's User Secrets configuration. [Profile](configure-profile.md) explains the required `UserSecretsId`, credential protection, and behavior when the provider is unavailable.

## Related features

- [Title bar](configure-title-bar.md), [Window](configure-window.md), [Background tasks](background-tasks.md), [Status bar](status-bar.md), and [Message service](message-service.md) use localized built-in text.
- [Themes](configure-themes.md) persist the selected theme through Host configuration.
- [Profile](configure-profile.md) explains remembered credentials and User Secrets setup.
- [IFlourishBuilder](flourish-builder.md) explains when configuration callbacks are applied.
