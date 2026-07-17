---
title: 项目
description: 管理多项目 Flourish 应用在标题栏中显示的项目标识、活动选择与交互请求。
---

# 项目

项目功能为标题栏提供可变化的应用内视图标识，例如解决方案、工作区或文档集合。Flourish 只管理内存中的显示元数据与 UI 请求；项目数据的创建、打开、保存和删除仍由应用负责。

## 启用项目模式

启用标题栏和项目模式，再配置没有活动项目时显示的文本：

```csharp
builder
    .ConfigureShell(shell =>
        shell.UseTitleBar().UseMultiProject())
    .ConfigureTitleBar(titleBar =>
        titleBar
            .SetApplicationTitle("Foobar")
            .SetUnnamedProjectPlaceholder("未命名项目")
            .SetLogo(showProjectTitle: true));
```

调用 `UseMultiProject()` 时默认启用；省略该调用时则默认禁用。未启用项目模式时，标题按钮显示应用标题，菜单中也只显示该标题。启用项目模式后，标题按钮显示活动项目名称；没有活动项目时显示未命名项目占位文本。

## 项目元数据

通过依赖注入解析单例 `IProjectService`。每个 `FlourishProject` 都有稳定且区分大小写的 ID、显示名称和可选的本地存储路径。

```csharp
public sealed class WorkspaceCatalog(IProjectService projects)
{
    public void Register()
    {
        projects.AddProject(
            new FlourishProject(
                "reports",
                "报表",
                @"C:\Work\Reports"));

        projects.UpsertProject(
            new FlourishProject("samples", "示例"),
            activate: false);
    }
}
```

存储路径只是描述性元数据。Flourish 会去除首尾空白，但不要求路径实际存在，也不会读取、创建、移动或删除该位置。

## 运行时操作

`IProjectService.Current` 返回不可变的 `FlourishProjectSnapshot`，其中包含有序项目、活动项目、项目模式状态与版本号。

| 操作 | 行为 |
| --- | --- |
| `AddProject(project, activate)` | 添加唯一的项目元数据，并可将其设为活动项目。 |
| `UpsertProject(project, activate)` | 按 ID 添加或替换项目元数据。 |
| `SetProjectMetadata(id, name, storagePath)` | 修改现有项目的名称与可选路径。 |
| `SetActiveProject(id)` | 修改活动标识；传入 `null` 可清除选择。 |
| `RemoveProject(id)` | 只移除 Shell 元数据；移除活动项目时会清除选择。 |
| `TryGetProject(id, out project)` | 查询一个已注册项目。 |
| `SetMultiProjectEnabled(enabled)` | 在运行时修改标题栏的项目模式。 |

元数据、活动选择或项目模式发生变化后会触发 `Changed`。事件会说明变更类型、受影响的项目以及活动项目是否发生变化。

## 处理标题栏请求

在标题菜单中选择项目或“新建项目”不会直接执行应用业务。服务会改为触发请求事件。应用应先完成业务操作，再更新 Flourish 状态：

```csharp
public sealed class ProjectRequests : IDisposable
{
    private readonly IProjectService projects;

    public ProjectRequests(IProjectService projects)
    {
        this.projects = projects;
        projects.ProjectActivationRequested += OnActivationRequested;
        projects.NewProjectRequested += OnNewProjectRequested;
    }

    private void OnActivationRequested(
        object? sender,
        FlourishProjectActivationRequestedEventArgs args)
    {
        OpenWorkspace(args.Project);
        projects.SetActiveProject(args.Project.Id);
    }

    private void OnNewProjectRequested(
        object? sender,
        FlourishNewProjectRequestedEventArgs args)
    {
        var project = CreateWorkspace();
        projects.AddProject(project);
    }

    public void Dispose()
    {
        projects.ProjectActivationRequested -= OnActivationRequested;
        projects.NewProjectRequested -= OnNewProjectRequested;
    }
}
```

请求事件在处理标题栏交互的线程上触发。应用可以自行安排异步工作，并在需要时切换线程来更新 UI。

## 相关功能

- [标题栏](configure-title-bar.md)说明应用标识、Logo 详情与标题菜单行为。
- [运行时 API](runtime-apis.md)汇总完整的运行时服务。
- [依赖注入](configure-services.md)说明服务解析与应用注册。
