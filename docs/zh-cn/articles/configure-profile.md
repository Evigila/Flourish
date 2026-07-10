---
title: ConfigureProfile
description: 配置 Flourish Profile 弹层、登录状态以及可替换的认证服务。
---

# ConfigureProfile

`ConfigureProfile` 用于配置标题栏下方的小型 Profile 页面。Profile 是一个固定尺寸、无横向或纵向滚动的容器，内部承载一个 WPF `Page`。只有在 [`ConfigureShell`](configure-shell.md) 同时启用 `UseTitleBar()` 与 `UseProfile()` 时，它才会出现。

```csharp
builder
    .ConfigureShell(shell => shell.UseTitleBar().UseProfile())
    .ConfigureProfile(profile =>
        profile.SetDefaultProfile(
            imagePath: null,
            userName: "Cristian Ronaldo"));
```

无参数调用 `SetDefaultProfile()` 时，用户名默认为 `User`。图片为空、无效或无法加载时，Flourish 会显示用户名首字母：`User` 显示 `U`，`Cristian Ronaldo` 显示 `CR`。`imagePath` 可使用本地文件路径或 pack URI。

## 默认页面

内置页面会展示图片和用户名。未登录时，点击 **Sign in** 后可填写用户名、选择图片并设置密码；选择图片使用 Windows 原生文件选择框。

认证完成后，登录表单会替换为 **Remember login** 选择框和 **Sign out** 按钮。`IProfileService.LoginState` 维护三种状态：

| 状态 | 含义 |
| --- | --- |
| `SignedOut` | 当前未登录。 |
| `SignedIn` | 仅在本次应用会话中保持登录。 |
| `SignedInRemembered` | 下次启动时从加密存储恢复登录。 |

未记住的登录在当前会话内仍然有效。下次启动时，Flourish 会优先检查持久化标记，删除这类凭据并以未登录状态启动。已记住的登录会先解密并再次经过认证服务，认证成功后才会恢复。

## 凭据存储

默认服务通过 .NET User Secrets 的存储位置写入一个 Profile 凭据值。写入前，Flourish 会序列化用户名、图片路径、密码和记住状态，再使用 Windows DPAPI 的 `DataProtectionScope.CurrentUser` 加密完整载荷。

User Secrets 本身并不是加密保险库；真正保护 Flourish 凭据并将其绑定到当前 Windows 用户的是 DPAPI。登出会删除对应的 Profile Secret。

## 替换认证服务

内置 `IProfileAuthService` 只进行简单认证：用户名和密码均非空即可通过。应用可在 `ConfigureServices` 中注册自定义实现，同时保留默认的 Profile 状态管理和加密存储。

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

弹层始终由 Shell 作为容器管理，自定义页面只替换其中的内容，并可通过 DI 解析构造函数依赖。

```csharp
builder
    .ConfigureServices((_, services) =>
        services.AddTransient<AccountProfilePage>())
    .ConfigureProfile(profile =>
        profile.SetProfilePage<AccountProfilePage>());
```

Profile `Frame` 已禁用横向与纵向滚动，因此自定义页面应适配这一紧凑区域。

## 相关 API

- [`ConfigureShell`](configure-shell.md) 提供 `UseProfile` 总开关。
- [`ConfigureTitleBar`](configure-title-bar.md) 可通过 `ShowProfile(false)` 显式隐藏标题栏 Profile 入口。
- [`ConfigureServices`](configure-services.md) 用于注册自定义 Profile 服务和页面。
