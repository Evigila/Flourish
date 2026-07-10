---
title: 用户资料（Profile）
description: 在 Flourish Shell 中提供用户资料、登录状态和可替换的认证体验。
---

# 用户资料（Profile）

Profile 是从标题栏打开的紧凑用户资料界面，可用于登录、显示用户名称与头像，以及恢复已记住的登录状态。通过 [Shell 配置](shell-configuration.md)同时启用 `UseTitleBar()` 与 `UseProfile()`，再使用 `ConfigureProfile` 配置默认资料和 Profile 页面。

```csharp
builder
    .ConfigureShell(shell => shell.UseTitleBar().UseProfile())
    .ConfigureProfile(profile =>
        profile
            .SetNameOrder(NameOrder.FirstLast)
            .SetDefaultProfile(
                imagePath: null,
                userName: "Foo Bar"));
```

无参数调用 `SetDefaultProfile()` 时，默认名称为 `User`。组合名称会按照调用时生效的名称顺序进行拆分，因此同时配置名称顺序和默认资料时，应先调用 `SetNameOrder()`。

## 名称顺序与占位首字母

内置登录表单将名称拆分为 **First Name** 与 **Last Name** 两个输入框。`SetNameOrder()` 同时控制输入框的视觉顺序、`ProfileUser.DisplayName` 以及无图片时显示的首字母顺序：

| 值 | 显示名称 | 占位首字母 |
| --- | --- | --- |
| `NameOrder.FirstLast` | `Foo Bar` | `FB` |
| `NameOrder.LastFirst` | `Bar Foo` | `BF` |

First Name 与 Last Name 至少应填写一项。`ProfileUser.FirstName`、`LastName`、`NameOrder` 和 `DisplayName` 提供结构化的用户名称结果。需要明确名称组成时，应分别传入 first name 和 last name。

## Profile 界面

Profile 以 Shell 内的覆盖层承载，并显示在标题栏 Profile 按钮下方。它会根据 Shell 的可用空间调整大小，并保持在窗口范围内。自定义 Profile 页面也应适应这一紧凑、自适应的内容区域。

Profile 不依赖窗口焦点，因此打开 Windows 原生文件选择框不会将其关闭。再次使用 Profile 入口、点击卡片外部或按 <kbd>Esc</kbd> 可关闭它；完成或取消图片选择后，用户会返回同一登录表单。

## 选择头像

内置表单允许用户通过 Windows 原生文件选择框选择或更换头像。选择有效图片后，表单会显示预览；没有可用图片时，Profile 会按照名称顺序显示占位首字母。

Flourish 不会复制所选图片，而是保存文件选择框返回的绝对路径。移动或删除原文件后，图片将无法加载，Profile 会自动回退到占位首字母。

## 登录状态

认证完成后，用户可以选择是否记住登录，也可以从 Profile 中登出。`IProfileService.LoginState` 提供三种状态：

| 状态 | 含义 |
| --- | --- |
| `SignedOut` | 当前未登录。 |
| `SignedIn` | 仅在本次应用会话中保持登录。 |
| `SignedInRemembered` | 当前已登录，并标记为在下次启动时尝试恢复。 |

未记住的登录仅在当前会话内有效。已记住的登录会在下次启动时重新经过认证服务，只有认证成功后才会恢复。

## 凭据保护

默认 Profile 服务会持久化需要恢复的凭据，并使用 Windows DPAPI 的 `DataProtectionScope.CurrentUser` 加密，因此只能由同一 Windows 用户解密。

登出或清理未记住的登录时，默认服务会删除对应凭据。应用应通过 `IProfileService` 管理 Profile 状态，不应依赖默认存储的文件路径或序列化格式。

## 替换认证服务

默认 `IProfileAuthService` 只检查显示名称和密码是否非空，不提供应用身份认证。需要验证真实身份时，应在 `ConfigureServices` 中注册自定义实现；这样可以接入应用自己的认证规则，同时保留默认 Profile 状态管理与凭据保护。

```csharp
builder.ConfigureServices((_, services) =>
{
    services.AddSingleton<IProfileAuthService, FoobarProfileAuthService>();
});
```

如果应用需要完整接管认证、状态与持久化，也可以直接替换 `IProfileService`。只有应用没有预先注册这些接口时，Flourish 才会注册默认实现。

```csharp
services.AddSingleton<IProfileService, FoobarProfileService>();
```

## 承载自定义页面

Shell 负责管理 Profile 覆盖层。`SetProfilePage<TPage>()` 可以替换其中的内容，自定义页面的构造函数依赖会通过依赖注入解析。

```csharp
builder
    .ConfigureServices((_, services) =>
        services.AddTransient<FoobarProfilePage>())
    .ConfigureProfile(profile =>
        profile.SetProfilePage<FoobarProfilePage>());
```

## 相关功能

- [Shell 配置](shell-configuration.md)提供 `UseProfile` 总开关。
- [标题栏](configure-title-bar.md)控制标题栏中的 Profile 入口。
- [依赖注入](configure-services.md)用于注册自定义 Profile 服务和页面。
