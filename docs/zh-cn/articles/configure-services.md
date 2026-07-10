---
title: ConfigureServices
description: 注册 WPF 应用服务、页面和命令解析器。
---

# ConfigureServices

`ConfigureServices` 向底层 .NET Generic Host 注册应用服务。它是唯一接收 `HostBuilderContext` 的 `Configure...` 回调，因为服务注册经常需要读取环境或配置。

```csharp
builder.ConfigureServices((context, services) =>
{
    services.AddSingleton<App>();
    services.AddSingleton<ICommandParser, AppCommandParser>();

    services.AddNavigable<HomePage>("Home", "\uE80F", navigationKey: NavigationRoutes.Home);
    services.AddNavigable<SettingsPage>("Settings", "\uE713", navigationKey: NavigationRoutes.Settings);
});
```

## 细节

导航栏中显示的页面通过 `AddNavigable` 注册。页面必须继承 `System.Windows.Controls.Page`，Flourish 会在导航时从依赖注入容器解析页面。ViewModel 运行时导航应使用 `navigationKey`，这样不需要引用页面类。

命令解析器实现 `ICommandParser`，也在这里注册。导航项、动态工具栏按钮、标题栏动作和 Footer 命令都可以把 command key 交给同一条解析链。

ViewModel、仓储和应用服务也属于 `ConfigureServices`。Shell 细节配置应放在 [`ConfigureNavigation`](configure-navigation.md)、[`ConfigureTitleBar`](configure-title-bar.md)、[`ConfigureWindow`](configure-window.md) 等专用 API 中。

Profile 认证同样可以在这里替换。注册 `IProfileAuthService` 可保留内置状态与加密持久化；注册 `IProfileService` 则可接管完整 Profile 流程。只有这些接口尚未注册时，Flourish 才会使用默认实现。

## 相关 API

- [`ConfigureNavigation`](configure-navigation.md) 将已注册页面放入可见导航模型。
- [`ConfigureDynamicToolbar`](configure-dynamic-toolbar.md) 将页面命令绑定到已注册页面类型。
- [`ConfigureProfile`](configure-profile.md) 说明 Profile 页面、认证和持久化。
- [`命令解析器`](command-parser.md) 说明 command key 路由。
