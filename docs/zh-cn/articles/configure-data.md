---
title: 应用数据
description: 配置 Flourish 的本地化、应用标识和偏好存储。
---

# 应用数据

`ConfigureData` 用于配置 Flourish 内置界面的语言、自定义翻译文件、应用标识和偏好存储。即使没有调用 `ConfigureData` 或 `SetLocale`，Flourish 也会使用内置英文（`EN`），因此内置界面始终具有可用文案。

## 选择内置语言

Flourish 内置 `CN` 和 `EN`。语言标识不区分大小写，并会在构建应用时归一化。

```csharp
builder.ConfigureData(data => data.SetLocale("EN"));
```

省略 `ConfigureData` 时，Flourish 默认使用 `EN`。只有在选择其他内置或自定义语言时才需要调用 `SetLocale(locale)`。应用传入的标题、搜索占位文本、导航标签、状态文本、对话框消息和自定义选项文本不会自动翻译。

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

## 配置应用标识

应用标识用于隔离已保存的主题偏好和已记住的 Profile 状态等数据。

```csharp
builder.ConfigureData(data =>
{
    data
        .SetAppCompany("Example Company")
        .SetAppName("Foobar");
});
```

`SetAppName` 提供偏好存储使用的应用名称。没有显式配置应用名称时，Flourish 会使用[标题栏](configure-title-bar.md)中设置的标题。

`SetAppCompany` 提供默认偏好路径中的公司目录片段。更改应用名称或公司名称会改变默认存储位置，原位置中的偏好不会自动迁移。

默认目录需要公司名称，以及应用名称或非空标题。无需按这些值派生存储位置时，可以改用显式目录。

## 指定偏好目录

`SetAppPreferenceDataPath` 覆盖默认存储目录。默认路径按应用标识隔离数据；便携式存储或工作区级存储可以通过自定义目录实现。

```csharp
builder.ConfigureData(data =>
    data.SetAppPreferenceDataPath(preferencePath));
```

应用应确保自定义目录可写，并在整个运行期间保持可用。

## 相关功能

- [标题栏](configure-title-bar.md)、[用户资料（Profile）](configure-profile.md)、[窗口](configure-window.md)、[状态栏](status-bar.md)和[消息服务](message-service.md)使用已本地化的内置文案。
- [主题](configure-themes.md)会把用户选择的主题写入偏好存储。
- [`IFlourishBuilder`](flourish-builder.md) 说明配置回调的应用时机。
