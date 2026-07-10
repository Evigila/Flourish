---
title: ConfigureProfile
description: 配置紧凑的 Flourish Profile 界面、名称顺序、认证与加密持久化。
---

# ConfigureProfile

`ConfigureProfile` 用于配置从标题栏打开的紧凑 Profile 卡片。只有在 [`ConfigureShell`](configure-shell.md) 同时启用 `UseTitleBar()` 与 `UseProfile()` 时，它才会出现。

```csharp
builder
    .ConfigureShell(shell => shell.UseTitleBar().UseProfile())
    .ConfigureProfile(profile =>
        profile
            .SetNameOrder(NameOrder.FirstLast)
            .SetDefaultProfile(
                imagePath: null,
                userName: "Cristian Ronaldo"));
```

无参数调用 `SetDefaultProfile()` 时默认名称为 `User`。为保持源码兼容，参数名称仍为 `userName`；Flourish 会按照调用该方法时已经生效的名称顺序拆分它。因此同时使用两个方法时，应先调用 `SetNameOrder()`。

## 名称顺序与占位首字母

内置登录表单将名称拆分为 **First Name** 与 **Last Name** 两个输入框。`SetNameOrder()` 同时控制输入框的视觉顺序、`ProfileUser.DisplayName` 以及无图片时显示的首字母顺序：

| 值 | 显示名称 | 占位首字母 |
| --- | --- | --- |
| `NameOrder.FirstLast` | `Cristian Ronaldo` | `CR` |
| `NameOrder.LastFirst` | `Ronaldo Cristian` | `RC` |

First Name 与 Last Name 至少应填写一项。`ProfileUser.FirstName`、`LastName`、`NameOrder` 和 `DisplayName` 提供结构化结果；`ProfileUser.UserName` 继续作为 `DisplayName` 的兼容别名。

原有的 `ProfileUser(string userName, ...)` 与 `ProfileSignInRequest(string userName, ...)` 构造函数也继续可用，并以 `FirstLast` 解释组合名称。需要明确名称顺序的新代码应使用分别传入 first name 和 last name 的重载。只包含 `UserName` 的版本 1 凭据会按照当前配置的名称顺序读取，并在成功恢复记住的登录后升级为结构化格式。

## 紧凑的窗口内界面

Profile 卡片是由 Shell 管理的窗口内 overlay，而不是独立的 WPF `Popup`。卡片常规宽度为 304 像素，高度随内容自适应；当窗口可用空间不足时，宽度和最大高度都会随窗口缩小。Flourish 会将卡片居中放在 Profile 按钮正下方，并将它限制在 Shell 内部，各边至少保留 5 像素安全距离。

由于 overlay 不依赖窗口焦点，Windows 原生文件选择框获得焦点时 Profile 不会关闭；选择或取消图片后仍会返回同一登录表单。点击卡片外部或按 <kbd>Esc</kbd> 才会关闭它。承载内容的 `Frame` 禁用了横向和纵向滚动，因此自定义页面应适应这个紧凑、自适应的区域。

内置表单的 TextBox、PasswordBox、CheckBox 与操作按钮使用 Flourish 的共享样式，与 Shell 其他控件保持一致。

## 上传图片

内置表单只显示一个横向铺满的 **Upload image** 按钮。点击后会打开 Windows 原生文件选择框。成功选择有效图片后，同一个按钮会展示图片预览及 **Image selected**；再次点击可更换图片。没有选中图片时，按钮继续显示 **Upload image**。

Flourish 不会复制上传的图片。图片仍保留在 Windows 文件选择框返回的原始绝对路径；只有在成功登录后，这个路径才会写入 Profile 凭据。之后移动或删除原文件会导致图片加载失败，界面自动回退为按名称顺序生成的占位首字母。

## 登录状态

认证完成后，登录表单会替换为 **Remember login** 选择框和 **Sign out** 按钮。`IProfileService.LoginState` 维护三种状态：

| 状态 | 含义 |
| --- | --- |
| `SignedOut` | 当前未登录。 |
| `SignedIn` | 仅在本次应用会话中保持登录。 |
| `SignedInRemembered` | 下次启动时从加密存储恢复登录。 |

未记住的登录在当前会话内仍然有效；下次启动时 Flourish 会删除其持久化凭据，并以未登录状态启动。已记住的登录会先解密并再次经过认证服务，认证成功后才会恢复。

## 保存位置与调试

成功登录后，默认服务会序列化 schema 版本、first name、last name、密码、图片绝对路径和 remember 标记。完整载荷先使用 Windows DPAPI 的 `DataProtectionScope.CurrentUser` 加密，再以 Base64 形式写入 User Secrets 的 `Flourish.Profile.Credential` 键。

Windows 上的文件位置为：

```text
%APPDATA%\Microsoft\UserSecrets\<secretId>\secrets.json
```

Secret ID 以 `ArkheideSystem.Flourish.Profile.` 开头，后面拼接以下字符串 SHA-256 值的前 24 个大写十六进制字符：

```text
<companyName>|<appName>|<entryAssemblyName>
```

当前 Gallery 配置对应的精确值如下：

```text
secretId: ArkheideSystem.Flourish.Profile.7523BCEB80CE0A555E66754B
file: %APPDATA%\Microsoft\UserSecrets\ArkheideSystem.Flourish.Profile.7523BCEB80CE0A555E66754B\secrets.json
key: Flourish.Profile.Credential
image: 用户选中的原始文件；Flourish 不创建副本
```

User Secrets 本身不是加密保险库；DPAPI 才负责保护 Flourish 载荷并将它绑定到当前 Windows 用户。因此 `secrets.json` 中看到的是加密 Base64，而不是可直接阅读的用户字段。登出时会删除 Profile 键；如果文件中没有其他内容，也会删除 `secrets.json`。未记住的凭据同样会在下次启动时删除。

## 替换认证服务

内置 `IProfileAuthService` 有意保持简单：显示名称和密码均非空即可通过。应用可以在 `ConfigureServices` 中注册自定义实现，同时保留默认 Profile 状态管理与加密存储。

```csharp
builder.ConfigureServices((_, services) =>
{
    services.AddSingleton<IProfileAuthService, CompanyProfileAuthService>();
});
```

如果应用希望完整接管认证、状态与持久化，也可以直接替换 `IProfileService`。只有应用没有预先注册这些接口时，Flourish 才会注册默认实现。

```csharp
services.AddSingleton<IProfileService, CompanyProfileService>();
```

## 承载自定义页面

overlay 始终由 Shell 管理，自定义页面只替换其中的内容，并可通过 DI 解析构造函数依赖。

```csharp
builder
    .ConfigureServices((_, services) =>
        services.AddTransient<AccountProfilePage>())
    .ConfigureProfile(profile =>
        profile.SetProfilePage<AccountProfilePage>());
```

## 相关 API

- [`ConfigureShell`](configure-shell.md) 提供 `UseProfile` 总开关。
- [`ConfigureTitleBar`](configure-title-bar.md) 可通过 `ShowProfile(false)` 显式隐藏标题栏 Profile 入口。
- [`ConfigureServices`](configure-services.md) 用于注册自定义 Profile 服务和页面。
