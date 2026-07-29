---
title: Application data
description: Configure localization, persisted settings paths, and the project catalog.
---

# Application data

`ConfigData` controls Flourish built-in interface language, custom locale files, and persisted settings paths. Localization is always available: when `ConfigData` or `InitLocale` is omitted, Flourish uses the built-in English (`en-US`) locale. Preferences and protected profile credentials use the configuration owned by the .NET Generic Host. Project metadata uses an independently configurable catalog.

## Select a built-in locale

Flourish includes `en-US` and `zh-CN`. Locale identifiers are case-insensitive and returned in canonical BCP 47 form. Hyphens are preferred; underscores are accepted and normalized to hyphens.

```csharp
builder.ConfigData(data => data.InitLocale("en-US"));
```

Flourish uses `en-US` when `ConfigData` is omitted. Persistence is enabled by default, so a valid effective `Flourish:Preferences:Locale` value takes precedence and later `SetLocale` changes are written back. Pass `usePersistedPreference: false` when the configured locale must always win. Application-provided text such as titles, search placeholders, navigation labels, custom status-item labels, dialog messages, and custom option text is not translated automatically.

## Add a custom locale

`AddLocaleFile(path)` registers a UTF-8 JSON file. The file name supplies the locale identifier and must follow `lang_<locale>.json`; the locale segment may contain letters, digits, hyphens, and underscores. Each separator must have a non-empty subtag on both sides. File-name identifiers use the same canonicalization as `InitLocale`.

```csharp
builder.ConfigData(data =>
{
    data
        .InitLocale("en-US")
        .AddLocaleFile("Locales/lang_en-US.json");
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

Calling `AddLocaleFile` more than once for the same locale merges the files in registration order. A later file replaces earlier values for the same key. For each lookup, Flourish uses this priority:

1. Custom value for the selected locale.
2. Built-in value for the selected locale.
3. Custom `en-US` value.
4. Built-in `en-US` value.
5. The key itself.

This lookup also allows a custom locale such as `lang_fr-FR.json` to define only part of the interface while the remaining keys fall back to English.

## Translation keys

The built-in locale files define the following keys. `{0}` is a format placeholder and must remain in custom values that use it.

| Key | English (`en-US`) | Simplified Chinese (`zh-CN`) |
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

## Host configuration

`FlourishBuilder.CreateDefaultBuilder(args)` uses the standard Generic Host configuration pipeline. Flourish reads its settings from the same `IConfiguration` that applications receive through `HostBuilderContext.Configuration` and dependency injection.

The writable Flourish preference source defaults to
`appsettings.Flourish.json`. It is registered explicitly before the Host's base
appsettings sources and is created on the first preference write. This dedicated
source publishes only the structural top-level `Flourish` object; another
top-level property in that file does not enter Host configuration through the
Flourish provider. Do not copy a seed file over it during every build or
deployment.

Use Builder parameters for normal fallback values. Place a value in the
application's base `appsettings.json` only when application policy must override
the persisted user preference:

```json
{
  "Flourish": {
    "Preferences": {
      "Theme": "System"
    }
  }
}
```

The application can copy its own base file to the output in the normal way:

```xml
<ItemGroup>
  <None Update="appsettings.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
</ItemGroup>
```

The configuration key is `Flourish:Preferences:Theme`. Reads follow the complete Host precedence: `appsettings.Flourish.json`, `appsettings.json`, `appsettings.{Environment}.json`, User Secrets, application-registered sources, environment variables, and command-line arguments. Later sources override earlier sources. `Host.CreateDefaultBuilder` automatically loads only the base and current-environment appsettings files; another name such as `appsettings.User.json` must be registered by application code.

Use `ConfigConfiguration` to register that file:

```csharp
builder.ConfigConfiguration((_, configuration) =>
    configuration.UseConfigurationFile(
        "appsettings.User.json",
        optional: true,
        reloadOnChange: true));
```

`UseConfigurationFile` registers a JSON source. `AddConfigurationSource` accepts a standard Microsoft `IConfigurationSource` when another provider type is required. Flourish does not expose the Host's mutable `IConfigurationBuilder`; it inserts registered sources after appsettings and User Secrets but before environment variables and command-line arguments. Registration order is preserved, so a later application source overrides an earlier one without overriding environment or command-line policy.

`IFlourishSettingsStore` accepts only descendant paths that start with `Flourish:`.
It cannot create, replace, or remove another top-level section. Flourish
preserves the values of unrelated sections already present in the selected file,
but serializes the complete JSON object again. This can reformat the document
and removes comments, so the dedicated default file is preferable when another
process also manages application settings. The selected directory must be
writable, and an existing file must contain valid JSON with an object at its
root. Its `Flourish` property, when present, must be an object.

## User preferences

User-interface preferences are restored and updated by default. The normal calls are therefore sufficient:

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

For each logical preference, the last builder call supplies both the fallback and the persistence policy. Pass `usePersistedPreference: false` when code must always use that call's value and stop writing runtime changes. This does not delete an existing stored value. With persistence enabled, a complete valid effective Host configuration value takes precedence; a missing, incomplete, or invalid value leaves the builder fallback intact. Compound settings such as size, position, font scale, colors, and motion timings are restored atomically instead of mixing saved and fallback fields.

Persistence is available for locale; theme mode; window restore size, position, state, topmost behavior, and close-to-notification-area behavior; navigation side, open state, user-adjusted width, and last route; profile name order; motion categories; smooth scrolling; global font; centered-content layout; material effect; theme colors; and corner radius. Runtime changes are coalesced before an atomic appsettings update, and pending changes are flushed during Host shutdown. `Minimized` is never restored, normal restore bounds are retained while maximized, and an off-screen persisted position is moved far enough into the current virtual desktop to remain reachable.

Application capabilities and structure are not preferences. Flourish does not persist title bar, navigation, Profile, project, toolbar, or status-bar enablement; page types and routes; handlers and factories; branding; minimum or maximum window constraints; resize mode; taskbar visibility; locale-file registrations; or page-specific font overrides. A stored value therefore cannot re-enable a capability that application code has disabled.

Flourish reads preference values through the effective `IConfiguration`, preserving normal Host precedence. By default it writes application-root `appsettings.Flourish.json`. Select another JSON file and an independent project-catalog file in `ConfigData`:

```csharp
builder.ConfigData(data => data
    .InitAppSettingsFilePath("Data/appsettings.Flourish.json")
    .InitProjectCatalogFilePath("Data/projects.json"));
```

Relative paths are resolved against `AppContext.BaseDirectory`; absolute paths are accepted. When the selected settings file differs from the base `appsettings.json`, Flourish adds a section-limited provider before that base source and leaves all Host appsettings sources available to the application. User Secrets, environment variables, and command-line providers keep their normal higher priority. Selecting the base `appsettings.json` explicitly keeps its normal full-document Host provider behavior, while Flourish writes only descendant paths under its `Flourish` section. Both paths must identify different `.json` files, and parent directories are created on the first write.

Use `IFlourishSettingsStore.RemoveAsync` with a full `Flourish:` path to reset one stored preference. Passing `usePersistedPreference: false` only ignores and stops updating that value; it does not silently erase an existing user choice.

## Project catalog

`IProjectService` stores ordered project mappings whose local files exist, plus the active persisted project ID, in the file selected by `InitProjectCatalogFilePath`; the default is application-root `projects.json`. The catalog path is independent of `IFlourishSettingsStore.FilePath`, is not a Host configuration source, and does not participate in configuration precedence.

Flourish loads this catalog when the project service starts, removes entries whose mapped files no longer exist, and writes valid catalog mutations atomically. Registering a replacement `IProjectBehavior` changes project dialogs and file lifecycle only; it does not disable catalog persistence. The directory must be writable. See [Projects](projects.md) for process-local unpersisted projects and lifecycle behavior.

## User Secrets

Remembered Profile credentials use the application's User Secrets configuration. [Profile](configure-profile.md) explains the required `UserSecretsId`, credential protection, and behavior when the provider is unavailable.

## Related features

- [Title bar](configure-title-bar.md), [Window](configure-window.md), [Background tasks](background-tasks.md), [Status bar](status-bar.md), and [Message service](message-service.md) use localized built-in text.
- [Themes](configure-themes.md) persist the selected theme through Host configuration.
- [Profile](configure-profile.md) explains remembered credentials and User Secrets setup.
- [Projects](projects.md) explains the persistent project catalog and replaceable lifecycle behavior.
- [IFlourishBuilder](flourish-builder.md) explains when configuration callbacks are applied.
