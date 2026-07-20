---
title: 用户资料（Profile）
description: 配置用户资料、登录状态、已记住的凭据和自定义认证。
---

# 用户资料（Profile）

Profile 从标题栏提供账户入口。调用 `SetProfile` 可显示入口并使用内置 Profile 页面。

```csharp
builder
    .ConfigureShell(shell => shell.UseTitleBar())
    .ConfigureTitleBar(titleBar =>
        titleBar.SetProfile(NameOrder.FirstLast));
```

调用不带参数的 `SetProfile()` 时使用 `NameOrder.FirstLast`。登录前，内置页面显示已本地化的 `Profile.DefaultName` 文本。

## 名称与首字母

内置登录表单分别收集名和姓。`NameOrder` 控制输入框顺序、`ProfileUser.DisplayName` 以及无图片时显示的首字母顺序。

| 值 | 显示名称 | 首字母 |
| --- | --- | --- |
| `NameOrder.FirstLast` | `Foo Bar` | `FB` |
| `NameOrder.LastFirst` | `Bar Foo` | `BF` |

名与姓至少填写一项。`ProfileUser.FirstName`、`LastName`、`NameOrder`、`DisplayName` 和 `Initials` 提供格式化后的结果。

内置页面的标签、状态文本、文件选择器筛选文本和验证消息使用[应用数据](configure-data.md)中选择的语言。应用提供的 Profile 页面自行管理文本。

## 交互行为

Profile 使用强 [Overlay](../controls/overlay.md)，因此不会随指针移动而关闭。再次使用 Profile 入口、点击 Profile 卡片外部或按 <kbd>Esc</kbd> 可关闭界面。打开 Windows 原生文件选择器不会关闭 Profile；选择或取消图片后会返回同一登录表单。

承载区域不提供滚动功能。如果自定义内容可能超过可用高度，请在自定义页面中加入 `ScrollViewer` 或其他滚动区域。

## Profile 图片

内置表单允许用户通过 Windows 原生文件选择器选择或更换图片。Flourish 保存所选文件的绝对路径，不会复制文件。如果之后无法加载该文件，Profile 会显示配置的首字母。

## 登录状态

认证完成后，登录表单会替换为记住登录和登出操作。`IProfileService.LoginState` 提供当前状态。

| 状态 | 含义 |
| --- | --- |
| `SignedOut` | 当前未登录。 |
| `SignedIn` | 仅在本次应用会话中保持登录。 |
| `SignedInRemembered` | 当前已登录，并标记为在下次启动时恢复。 |

未记住的登录在应用退出前保持有效。已记住的登录会在下次启动时重新认证，认证成功后才会生效。

## 已记住的凭据

默认 Profile 服务把普通登录保留在内存中。启用“记住登录”后，Flourish 会为当前 Windows 用户保护凭据，并通过应用的 User Secrets 配置进行存储。登出或禁用“记住登录”会删除已存储的凭据。

请为应用项目配置稳定的 User Secrets 标识：

```xml
<PropertyGroup>
  <UserSecretsId>Foobar.Desktop</UserSecretsId>
</PropertyGroup>
```

没有 User Secrets provider 时，普通登录仍然可用，但启用“记住登录”会抛出 `InvalidOperationException`。不要把 Profile 凭据放入 `appsettings.json`。

> [!WARNING]
> 默认 `IProfileAuthService` 只要求显示名称和密码非空。需要验证凭据时，请注册应用自己的认证服务。

## 替换认证服务

通过[依赖注入](configure-services.md)注册 `IProfileAuthService`，可以替换认证逻辑，同时保留内置 Profile 状态和记住登录行为。

```csharp
builder.ConfigureServices((_, services) =>
{
    services.AddSingleton<IProfileAuthService, FoobarProfileAuthService>();
});
```

如果应用自行管理认证、状态与持久化，请注册 `IProfileService`。

```csharp
services.AddSingleton<IProfileService, FoobarProfileService>();
```

只有应用没有注册这些接口时，Flourish 才会提供默认实现。

## 承载自定义页面

使用 `ConfigureProfile` 可以替换 Profile 界面承载的内容。自定义页面从依赖注入容器解析；标题栏仍需调用 `SetProfile` 才会显示入口。

```csharp
builder
    .ConfigureServices((_, services) =>
        services.AddTransient<FoobarProfilePage>())
    .ConfigureShell(shell => shell.UseTitleBar())
    .ConfigureTitleBar(titleBar =>
        titleBar.SetProfile(NameOrder.FirstLast))
    .ConfigureProfile(profile =>
        profile.SetProfilePage<FoobarProfilePage>());
```

省略 `ConfigureProfile` 时，`SetProfile` 使用内置页面。

## 相关功能

- [Shell 配置](shell-configuration.md)启用标题栏。
- [标题栏](configure-title-bar.md)显示 Profile 入口并选择名称顺序。
- [依赖注入](configure-services.md)注册自定义 Profile 服务和页面。
