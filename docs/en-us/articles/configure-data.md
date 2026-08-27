---
title: Application data
description: Configure localization, persisted settings paths, and the project catalog.
---

# Application data

`ConfigureData` sets the interface locale, culture catalogs, and persistence paths. It defaults to built-in English (`en-US`).

## Select a built-in locale

Flourish includes `en-US` and `zh-CN`. Locale identifiers are case-insensitive, normalized to canonical BCP 47 form, and may use underscores instead of preferred hyphens.

```csharp
builder.ConfigureData(data => data.SetLocale("en-US"));
```

Persistence is enabled by default: a valid `Flourish:Preferences:Locale` value wins, and later `SetLocale` changes are saved. Pass `usePersistedPreference: false` to keep the configured locale authoritative. Flourish does not translate application-provided text automatically.

## Connect Essential Culture

To translate application text with `Arkheide.Essential.Culture`, install only `Arkheide.Extension.Culture`. It supplies Culture Core, the WPF adapter, and the Generator transitively.

```bash
dotnet add package Arkheide.Extension.Culture
```

Call the non-generic entry point:

```csharp
using ArkheideSystem.Extension.Culture;

var flourish = ApplicationBuilder
    .CreateDefaultBuilder(args)
    .UseEssentialCulture()
    .ConfigureData(data => data.SetLocale("en-US"))
    .Build();
```

`ILocalizationService` is the public Flourish culture endpoint. Locale changes synchronize Essential Culture before the first frame and at runtime, then refresh culture tokens stored in Shell state:

```csharp
localization.SetLocale("zh-CN");

string locale = localization.Current.Locale;
IReadOnlyList<string> available = localization.Current.AvailableLocales;
localization.Changed += OnLocalizationChanged;
```

The application owns `Culture.json`; XAML uses the supplied `Localize` extension. Resolve transient or parameterized text when created. User input, project names, and search text are never translated implicitly.

## Override the built-in culture catalog

Flourish embeds `FlourishCulture.Json`. A same-named file in the application output directory loads automatically and overrides matching locale-key cells without repeating the catalog.

Additional catalogs can be registered with `AddCultureFile(path)`. Every registered file must also be named `FlourishCulture.Json`; use separate directories when composing multiple catalogs.

```csharp
builder.ConfigureData(data =>
{
    data
        .SetLocale("en-US")
        .AddCultureFile("Locales/FlourishCulture.Json");
});
```

Catalogs load during `Build()`. Missing files throw `FileNotFoundException`; invalid names throw `ArgumentException`; unreadable or invalid content throws `InvalidDataException`.

The format matches Essential Culture: translation keys are top-level properties and locales are nested properties. A user catalog may contain only the cells it needs to override:

```json
{
  "TitleBar.Back": {
    "en-US": "Previous",
    "fr-FR": "Précédent"
  },
  "Tray.Show": {
    "fr-FR": "Ouvrir"
  }
}
```

Catalogs are merged in registration order. A later catalog replaces an earlier value only when both its locale and key match. For each lookup, Flourish uses this priority:

1. Custom value for the selected locale.
2. Built-in value for the selected locale.
3. Custom `en-US` value.
4. Built-in `en-US` value.
5. The key itself.

A catalog may override one cell or supply a partial locale; remaining values follow this fallback order.

## Translation keys

The built-in `FlourishCulture.Json` catalog defines the following keys. `{0}` is a format placeholder and must remain in custom values that use it.

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
| `Window.BackgroundTasksCloseTitle` | Stop background tasks? | 中止后台任务？ |
| `Window.BackgroundTasksClosePrompt` | Active background tasks: {0}. Closing the window will cancel them. Stop the tasks and exit? | 仍有 {0} 个后台任务正在进行。关闭窗口将取消这些任务。是否中止任务并退出？ |
| `Window.BackgroundTasksKeepRunning` | Keep running | 继续运行 |
| `Window.BackgroundTasksStopAndExit` | Stop tasks and exit | 中止任务并退出 |
| `Tray.Show` | Show | 显示 |
| `Tray.Exit` | Exit | 退出 |
| `Status.Connected` | Connected | 已连接 |
| `Status.Disconnected` | Disconnected | 未连接 |

## Host configuration

`ApplicationBuilder.CreateDefaultBuilder(args)` uses the standard Generic Host configuration pipeline. Flourish reads its settings from the same `IConfiguration` that applications receive through `HostBuilderContext.Configuration` and dependency injection.

The writable preference source defaults to `appsettings.Flourish.json`, loads before the Host's base appsettings sources, and is created on first write. It publishes only the top-level `Flourish` object, so do not overwrite it with a seed file during every deployment.

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

The configuration key is `Flourish:Preferences:Theme`. Reads follow Host precedence: `appsettings.Flourish.json`, `appsettings.json`, `appsettings.{Environment}.json`, User Secrets, application sources, environment variables, then command-line arguments. `Host.CreateDefaultBuilder` loads only the base and current-environment files; register other files explicitly.

Use `ConfigureConfiguration` to register that file:

```csharp
builder.ConfigureConfiguration((_, configuration) =>
    configuration.AddJsonFile(
        "appsettings.User.json",
        optional: true,
        reloadOnChange: true));
```

The callback receives the standard Microsoft `IConfigurationBuilder`, so use `AddJsonFile` or the extension supplied by any other installed configuration provider. Flourish inserts these application sources after appsettings and User Secrets but before environment variables and command-line arguments. Registration order is preserved, so a later application source overrides an earlier one without overriding environment or command-line policy.

`ISettingsStore` accepts only descendant `Flourish:` paths and preserves unrelated top-level values. It rewrites the complete JSON object, removing comments and possibly reformatting it, so prefer the dedicated file when another process also writes settings. The directory must be writable; existing content and any `Flourish` property must be JSON objects.

## User preferences

User-interface preferences are restored and updated by default. The normal calls are therefore sufficient:

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

The last builder call supplies each preference's fallback and persistence policy. Pass `usePersistedPreference: false` to keep that value authoritative and stop writes without deleting stored data. With persistence enabled, complete valid Host values win; otherwise the builder fallback remains, and compound settings restore atomically.

Persistence is available for locale; theme mode; window restore size, position, state, topmost behavior, and close-to-notification-area behavior; navigation side, open state, user-adjusted width, and last route; profile name order; motion categories; smooth scrolling; global font; centered-content layout; material effect; theme colors; and corner radius. Runtime changes are coalesced before an atomic appsettings update, and pending changes are flushed during Host shutdown. `Minimized` is never restored, normal restore bounds are retained while maximized, and an off-screen persisted position is moved far enough into the current virtual desktop to remain reachable.

Application capabilities and structure are not preferences. Flourish does not persist title bar, navigation, Profile, project, toolbar, or status-bar enablement; page types and routes; handlers and factories; branding; minimum or maximum window constraints; resize mode; taskbar visibility; culture-file registrations; or page-specific font overrides. A stored value therefore cannot re-enable a capability that application code has disabled.

Flourish reads preference values through the effective `IConfiguration`, preserving normal Host precedence. By default it writes application-root `appsettings.Flourish.json`. Select another JSON file and an independent project-catalog file in `ConfigureData`:

```csharp
builder.ConfigureData(data => data
    .SetAppSettingsFilePath("Data/appsettings.Flourish.json")
    .SetProjectCatalogFilePath("Data/projects.json"));
```

Relative paths resolve against `AppContext.BaseDirectory`; absolute paths are accepted, and parent directories are created on first write. A non-base settings file adds a section-limited provider before base appsettings while preserving the higher priority of User Secrets, environment variables, and command-line arguments. Selecting base `appsettings.json` keeps its full-document provider, but Flourish writes only `Flourish:` descendants; the settings and catalog paths must name different `.json` files.

Use `ISettingsStore.RemoveAsync` with a full `Flourish:` path to reset one stored preference. Passing `usePersistedPreference: false` only ignores and stops updating that value; it does not silently erase an existing user choice.

## Project catalog

`IProjectService` stores ordered project mappings whose local files exist, plus the active persisted project ID, in the file selected by `SetProjectCatalogFilePath`; the default is application-root `projects.json`. The catalog path is independent of `ISettingsStore.FilePath`, is not a Host configuration source, and does not participate in configuration precedence.

At startup Flourish removes entries whose files no longer exist; valid mutations write atomically to a writable directory. Replacing `IProjectBehavior` changes dialogs and file lifecycle, not catalog persistence. See [Projects](projects.md).

## User Secrets

Remembered Profile credentials use the application's User Secrets configuration. [Profile](configure-profile.md) explains the required `UserSecretsId`, credential protection, and behavior when the provider is unavailable.

## Related features

- [Title bar](configure-title-bar.md), [Window](configure-window.md), [Background tasks](background-tasks.md), [Status bar](status-bar.md), and [Message service](message-service.md) use localized built-in text.
- [Themes](configure-themes.md) persist the selected theme through Host configuration.
- [Profile](configure-profile.md) explains remembered credentials and User Secrets setup.
- [Projects](projects.md) explains the persistent project catalog and replaceable lifecycle behavior.
- [IApplicationBuilder](flourish-builder.md) explains when configuration callbacks are applied.
