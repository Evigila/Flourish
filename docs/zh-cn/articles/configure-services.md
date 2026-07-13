---
title: 依赖注入
description: 注册应用服务、可导航页面、命令解析器和可替换的 Flourish 服务。
---

# 依赖注入

Flourish 使用其 .NET Generic Host 中的 `IServiceCollection`。通过 `ConfigureServices` 注册应用服务、WPF 页面、命令解析器和可替换的 Flourish 服务。

## 注册服务

```csharp
builder.ConfigureServices((context, services) =>
{
    services.AddSingleton<App>();
    services.AddSingleton<ICommandParser, AppCommandParser>();

    services.AddNavigable<HomePage>("首页", "\uE80F");
    services.AddNavigable<ReportsPage>("报表", "\uE9D2");
});
```

回调会收到 `HostBuilderContext`，因此注册可以使用当前环境以及供应 Flourish 设置的同一份 Host 配置。ViewModel、仓储和其他应用服务使用标准 .NET 依赖注入方式。

`IBackgroundTaskService` 已完成注册，可以直接通过构造函数注入。参见[后台任务](background-tasks.md)。

## 注册可导航页面

`AddNavigable<TPage>` 注册 `System.Windows.Controls.Page` 及其导航元数据。Flourish 从类名生成区分大小写的导航键，并移除一个末尾 `Page` 后缀：`SettingsPage` 生成 `Settings`，`Page1` 仍生成 `Page1`。

没有配置显式导航分组或固定项时，导航栏会自动列出已注册页面。配置分组或固定项会替代该自动列表。[导航](navigation.md)说明可见位置和初始页面选择。

ViewModel 可以使用生成的键导航，例如 `navigation.Navigate("Settings")`，无需引用 WPF 页面类型。`Build()` 会拒绝重复的生成键。

## 注册命令解析器

分配给 Flourish UI 区域的命令通过 `ICommandDispatcher` 调度。在此注册 `ICommandParser` 实现，以提供一组固定的同步处理程序。[命令解析器](command-parser.md)说明调度顺序和运行时处理程序。

## 替换 Profile 服务

注册 `IProfileAuthService` 可以提供应用认证，同时保留内置 Profile 状态和记住登录行为。应用自行管理完整 Profile 流程时，请注册 `IProfileService`。只有应用没有注册这些接口时，Flourish 才会提供默认实现。

## 相关功能

- [导航](navigation.md)说明自动和显式导航项。
- [动态工具栏](dynamic-toolbar.md)将命令附加到已注册页面类型。
- [用户资料（Profile）](configure-profile.md)说明认证和 Profile 服务替换。
- [后台任务](background-tasks.md)说明异步工作、取消、进度和结果。
- [命令解析器](command-parser.md)说明命令键路由。
