---
title: Application data
description: Configure localization, application identity, and preference storage.
---

# Application data

`ConfigureData` controls Flourish built-in interface language, custom locale files, application identity, and preference storage. Localization is always available: when `ConfigureData` or `SetLocale` is omitted, Flourish uses the built-in English (`EN`) locale.

## Select a built-in locale

Flourish includes `CN` and `EN`. Locale identifiers are case-insensitive and normalized when the application is built.

```csharp
builder.ConfigureData(data => data.SetLocale("EN"));
```

Flourish uses `EN` when `ConfigureData` is omitted. Call `SetLocale(locale)` only when selecting another built-in or custom locale. Application-provided text such as titles, search placeholders, navigation labels, status text, dialog messages, and custom option text is not translated automatically.

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
| `Profile.ChangeImage` | Change profile image | 更换个人资料图片 |
| `Profile.UploadImage` | Upload image | 上传图片 |
| `Profile.ImageSelected` | Image selected | 已选择图片 |
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
| `Status.Power` | Power | 电源 |

## Configure application identity

Application identity scopes saved preferences such as the selected theme and remembered profile state.

```csharp
builder.ConfigureData(data =>
{
    data
        .SetAppCompany("Example Company")
        .SetAppName("Foobar");
});
```

`SetAppName` supplies the name used for preference storage. If it is omitted, Flourish uses the title configured through [Title bar](configure-title-bar.md).

`SetAppCompany` supplies the company directory segment in the default preference path. Changing either identity value changes that path; Flourish does not move preferences from the previous path.

The default directory requires a company name and either an application name or a non-empty title. Use an explicit directory when the application should not derive storage from those values.

## Choose a custom directory

Use `SetAppPreferenceDataPath` when preferences must be portable or stored with a particular workspace.

```csharp
builder.ConfigureData(data =>
{
    data.SetAppPreferenceDataPath(preferenceDirectory);
});
```

The application is responsible for choosing an accessible, persistent directory. Otherwise, keep the default location derived from the company and application names.

## Related features

- [Title bar](configure-title-bar.md), [Profile](configure-profile.md), [Window](configure-window.md), [Status bar](status-bar.md), and [Message service](message-service.md) use localized built-in text.
- [Themes](configure-themes.md) persist the selected theme for this application identity.
- [IFlourishBuilder](flourish-builder.md) explains when configuration callbacks are applied.
