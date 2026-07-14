---
title: 依赖注入
description: 注册应用服务、可导航页面和可替换的 Flourish 服务。
---

# 依赖注入

Flourish 使用其 .NET Generic Host 中的 `IServiceCollection`。通过 `ConfigureServices` 注册应用服务、WPF 页面和可替换的 Flourish 服务。

## 注册服务

```csharp
builder.ConfigureServices((context, services) =>
{
    services.AddSingleton<App>();
    services.AddSingleton<ReportExporter>();
    services.AddCommandParser<ReportCommands>();

    services.AddNavigable<HomePage>("首页", "\uE80F");
    services.AddNavigable<ReportsPage>("报表", "\uE9D2");
});
```

回调会收到 `HostBuilderContext`，因此注册可以使用当前环境以及供应 Flourish 设置的同一份 Host 配置。ViewModel、仓储和其他应用服务使用标准 .NET 依赖注入方式。

`IBackgroundTaskService` 已完成注册，可以直接通过构造函数注入。参见[后台任务](background-tasks.md)。

## 注册可导航页面

`AddNavigable<TPage>` 注册 `System.Windows.Controls.Page` 及其导航元数据。Flourish 从类名生成区分大小写的导航键，并移除一个末尾 `Page` 后缀：`SettingsPage` 生成 `Settings`，`Page1` 仍生成 `Page1`。

注册页面不会创建可见导航项。使用 `ConfigureNavigation` 将每个页面放入分组或固定区域。[导航](navigation.md)说明可见位置和初始页面选择。

ViewModel 可以使用生成的键导航，例如 `navigation.Navigate("Settings")`，无需引用 WPF 页面类型。`Build()` 会拒绝重复的生成键。

## 提供命令依赖项

通过 `ConfigureServices` 注册命令处理程序依赖的应用服务。需要在整个 Host 生命周期内保持有效的命令映射应实现 `ICommandParser`，并通过 `AddCommandParser<TParser>` 添加；Flourish 会随 Host 注册和移除这些映射。生命周期较短或需要动态变化的处理程序仍直接使用 `ICommandRegistry`。[命令调度](commands.md)说明两种注册方式、可用性与结果。

## 替换 Profile 服务

注册 `IProfileAuthService` 可以提供应用认证，同时保留内置 Profile 状态和记住登录行为。应用自行管理完整 Profile 流程时，请注册 `IProfileService`。只有应用没有注册这些接口时，Flourish 才会提供默认实现。

## 相关功能

- [导航](navigation.md)说明显式导航分组与固定项。
- [动态工具栏](dynamic-toolbar.md)将命令附加到已注册页面类型。
- [用户资料（Profile）](configure-profile.md)说明认证和 Profile 服务替换。
- [后台任务](background-tasks.md)说明异步工作、取消、进度和结果。
- [命令调度](commands.md)说明命令键路由。
