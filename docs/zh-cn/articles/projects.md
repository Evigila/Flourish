---
title: 项目
description: 管理项目标识、目录持久化、标题栏选择与可替换的项目生命周期行为。
---

# 项目

项目功能在标题栏表示解决方案、工作区或文档集合。`IProjectService` 管理目录与活动选择；可替换的 `IProjectBehavior` 管理新建、保存、激活、删除和关闭流程。

这些 API 提供 Shell 状态和默认占位文件流程；激活项目只更新元数据与标题，不切换业务内容。

## 启用项目模式

启用标题栏和项目模式，再配置未持久化项目的显示文本：

```csharp
builder
    .ConfigureProjects(projects => projects.SetMultiProjectEnabled())
    .ConfigureTitleBar(titleBar =>
        titleBar
            .SetEnabled()
            .SetApplicationTitle("Foobar")
            .SetUnnamedProjectPlaceholder("未命名项目")
            .SetLogo(showProjectTitle: true));
```

`IProjectBuilder.SetMultiProjectEnabled()` 默认启用项目模式；省略 `ConfigureProjects` 则禁用。禁用时标题选择器只显示应用标题，不提供项目保存或关闭语义；启用后显示活动项目、未命名占位文本、全部项目和“新建项目”。

## 项目元数据与持久化

通过依赖注入解析单例 `IProjectService`。每个 `ProjectDescriptor` 都有稳定且区分大小写的 ID、显示名称和可选的本地存储路径。

```csharp
public sealed class WorkspaceCatalog(IProjectService projects)
{
    public void Register()
    {
        projects.AddProject(
            new ProjectDescriptor(
                "reports",
                "报表",
                @"C:\Work\Reports.txt"));

        projects.SetProject(
            new ProjectDescriptor("samples", "示例"),
            activate: false);
    }
}
```

`StoragePath == null` 表示尚未持久化；不要比较项目名与占位文本。名称可重复，占位文本可修改或本地化。

Flourish 从 `SetProjectCatalogFilePath` 指定的文件加载目录与活动 ID，默认是应用根目录的 `projects.json`，并独立于可写设置文件。目录变更会原子写入；失败时回滚内存且不发布事件。

目录持久化由 `IProjectService` 管理，替换 `IProjectBehavior` 不影响它。目录只保存元数据，不读写项目指向的内容。

持久化目录中没有项目时，Flourish 会创建并激活一个未持久化项目，并使用配置的占位文本显示它。

## 运行时目录操作

`IProjectService.Current` 返回不可变的 `ProjectCatalogSnapshot`，其中包含有序项目、活动项目、项目模式状态与版本号。

| 操作 | 行为 |
| --- | --- |
| `AddProject(project, activate)` | 添加唯一的项目元数据，并可将其设为活动项目。 |
| `SetProject(project, activate)` | 按 ID 添加或替换项目元数据。 |
| `SetProjectMetadata(id, name, storagePath)` | 修改现有项目的名称与可选路径。 |
| `SetActiveProject(id)` | 只修改活动 Shell 标识；传入 `null` 可清除选择。 |
| `RemoveProject(id)` | 只移除目录项；移除活动项目时会清除选择。 |
| `GetProject(id)` | 返回一个已注册项目；ID 未注册时返回 `null`。 |
| `SetMultiProjectEnabled(enabled)` | 在运行时修改标题栏的项目模式。 |

元数据、活动选择或项目模式发生变化后会触发 `Changed`。事件会说明变更类型、受影响的项目以及活动项目是否发生变化。直接调用 `SetActiveProject` 和 `RemoveProject` 不会运行生命周期对话框，也不会操作项目文件；用户操作需要这些行为时，应使用 `IProjectBehavior`。

## 默认项目行为

应用未提供 `IProjectBehavior` 时，Flourish 会注册默认实现。该实现管理 `.txt` 占位文件与 Shell 元数据，不管理应用文档数据。

| 用户操作 | 默认行为 |
| --- | --- |
| “新建项目” | 先处理尚未持久化的活动项目，再打开保存对话框；所选文件不存在时创建 `.txt` 占位文件，随后添加元数据并将其激活。取消任一步骤都会停止创建。 |
| Ctrl+S | 在项目模式下保存活动项目元数据。未持久化项目会打开保存对话框，随后采用文件名称与路径；已存在的所选文件只建立映射而不会修改内容，再次保存已持久化项目则不执行文件操作。未启用项目模式时，Flourish 不处理 Ctrl+S，由应用定义自身保存语义。内置命令和快捷键使用低优先级，因此应用注册可以优先处理。 |
| 选择其他项目 | 如果活动项目尚未持久化，只提供“保存”或“取消”。仅在保存成功后才继续激活。 |
| 关闭应用 | 在项目模式下，未持久化的活动项目会在实际关闭前提供“保存”“不保存”和“取消”。“保存”必须成功完成；“不保存”会在不创建项目文件的情况下继续关闭；“取消”会阻止关闭。未启用项目模式时不会显示项目保存提示。 |
| 右键单击项目 | 请求确认并移除目录项；没有其他项目引用同一路径时，同时删除受管理的 `.txt` 文件。目录写入失败时会恢复已隔离的文件并保持目录不变。 |

默认行为只把扩展名为 `.txt` 的路径视为受管理文件，不会删除其他文件类型。直接调用 `IProjectService.RemoveProject` 只会移除元数据，不会显示确认。

占位文件不包含应用数据。需要序列化文档、打开存储或协调领域工作区的应用应替换生命周期行为。

## 替换项目行为

通过 `ConfigureServices` 注册一个单例 `IProjectBehavior`。只有应用没有注册该接口时，Flourish 才会提供默认实现。

```csharp
builder.ConfigureServices((_, services) =>
    services.AddSingleton<IProjectBehavior, WorkspaceProjectBehavior>());
```

启用项目模式时，Shell 会将生命周期入口路由到五个异步方法：

| 方法 | Shell 入口 |
| --- | --- |
| `CreateProjectAsync` | 下拉框中的“新建项目”。 |
| `SaveActiveProjectAsync` | 内置 Ctrl+S 命令。 |
| `ActivateProjectAsync` | 选择其他项目。 |
| `DeleteProjectAsync` | 右键删除。 |
| `CanCloseAsync` | 项目关闭守卫。 |

操作继续时返回 `true`，取消或失败时返回 `false`。替换实现负责对话框和文件生命周期，并通过 `IProjectService` 发布变更；Flourish 仍原子写入 `projects.json`。

## 相关功能

- [标题栏](configure-title-bar.md)
- [运行时 API](runtime-apis.md)
- [依赖注入](configure-services.md)
- [应用数据](configure-data.md)
