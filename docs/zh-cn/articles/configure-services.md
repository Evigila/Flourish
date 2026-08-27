---
title: 依赖注入
description: 注册应用服务、可导航页面和可替换的 Flourish 服务。
---

# 依赖注入

通过 Generic Host 的 `IServiceCollection` 和 `ConfigureServices` 注册应用服务、WPF 页面及可替换服务。

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

通过 `ConfigureServices` 注册命令依赖。Host 生命周期映射实现 `ICommandParser` 并用 `AddCommandParser<TParser>` 添加；短期或动态处理程序直接使用 `ICommandRegistry`。两种方式见[命令调度](commands.md)。

## 替换 Profile 服务

注册 `IProfileAuthService` 可以提供应用认证，同时保留内置 Profile 状态和记住登录行为。应用自行管理完整 Profile 流程时，请注册 `IProfileService`。只有应用没有注册这些接口时，Flourish 才会提供默认实现。

## 替换项目行为

应用自行管理项目对话框或文件生命周期时，注册一个单例 `IProjectBehavior`。只有应用没有注册该接口时，Flourish 才会提供默认的 `.txt` 占位文件行为。

```csharp
builder.ConfigureServices((_, services) =>
    services.AddSingleton<IProjectBehavior, WorkspaceProjectBehavior>());
```

多项目模式调用替换实现的 `CreateProjectAsync`、`SaveActiveProjectAsync`、`ActivateProjectAsync`、`DeleteProjectAsync` 和 `CanCloseAsync`；返回 `false` 取消操作。标题选择、右键删除、Ctrl+S 和关闭守卫均使用该服务。未启用时，应用自行管理单项目保存。

替换 `IProjectBehavior` 只会改变对话框和项目文件处理，不会替换项目目录。替换实现应通过 `IProjectService` 发布元数据与活动选择变更；Flourish 仍会将每次目录变更原子写入 `SetProjectCatalogFilePath` 选择的路径。完整生命周期契约参见[项目](projects.md)。

## 相关功能

- [导航](navigation.md)
- [动态工具栏](dynamic-toolbar.md)
- [用户资料（Profile）](configure-profile.md)
- [项目](projects.md)
- [后台任务](background-tasks.md)
- [命令调度](commands.md)
