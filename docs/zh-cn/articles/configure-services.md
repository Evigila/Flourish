---
title: 依赖注入
description: 在 Flourish 使用的 .NET Generic Host 中注册应用服务、页面和扩展点。
---

# 依赖注入

Flourish 基于 .NET Generic Host 组织应用服务。通过 `ConfigureServices` 可以使用标准 `IServiceCollection` 注册应用对象、ViewModel、仓储、页面和 Flourish 扩展点，并通过 `HostBuilderContext` 读取当前环境与配置。

```csharp
builder.ConfigureServices((context, services) =>
{
    services.AddSingleton<App>();
    services.AddSingleton<ICommandParser, AppCommandParser>();

    services.AddNavigable<HomePage>(
        "首页",
        "\uE80F",
        navigationKey: NavigationRoutes.Home);
    services.AddNavigable<SettingsPage>(
        "设置",
        "\uE713",
        navigationKey: NavigationRoutes.Settings);
});
```

## 注册应用服务

ViewModel、仓储和应用服务按照普通 .NET 依赖注入规则注册。Shell 和导航页面从 Host 根服务提供器解析；只有应用显式创建并管理 `IServiceScope` 时，才应让相关对象依赖 scoped 服务。

Flourish 通过同一容器解析页面、命令解析器和 Profile 服务，因此这些类型可以在构造函数中声明应用服务依赖。自定义 Shell 内容则由应用提供工厂创建；工厂可以通过收到的 `IServiceProvider` 显式解析依赖。

## 注册可导航页面

`AddNavigable<TPage>` 会注册派生自 `System.Windows.Controls.Page` 的页面，并记录显示名称、图标、缓存模式和可选的导航键。页面在导航发生时由依赖注入容器解析。

页面注册只声明页面及其元数据，不决定它在导航栏中的位置。分组、固定项、初始页面和运行时导航参见[导航](navigation.md)。

## 注册 Flourish 扩展点

导航项、动态工具栏按钮、标题栏动作和状态栏命令可以把稳定的命令键交给 `ICommandParser`。解析器实现也在 `ConfigureServices` 中注册；完整路由方式参见[命令解析器](command-parser.md)。

Profile 认证可以通过注册 `IProfileAuthService` 进行替换；注册 `IProfileService` 则可以接管完整的 Profile 状态与持久化流程。只有应用没有预先注册这些接口时，Flourish 才会使用默认实现。

Shell 的展示和行为配置由对应功能负责，例如[导航](navigation.md)、[标题栏](configure-title-bar.md)和[窗口](configure-window.md)。

## 相关功能

- [导航](navigation.md) 将已注册页面放入可见导航模型。
- [动态工具栏](dynamic-toolbar.md) 将页面命令绑定到已注册页面类型。
- [用户资料（Profile）](configure-profile.md)说明 Profile 页面、认证和持久化。
- [命令解析器](command-parser.md)说明命令键路由。
