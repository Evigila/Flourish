---
title: 项目
description: 管理项目标识、目录持久化、标题栏选择与可替换的项目生命周期行为。
---

# 项目

项目功能为标题栏提供可变化的应用内视图标识，例如解决方案、工作区或文档集合。`IProjectService` 管理有序项目目录与活动选择；`IProjectBehavior` 管理面向用户的新建、保存、激活、删除和关闭流程，应用可以替换该行为。

这些 API 提供 Shell 状态和默认的占位文件流程。激活项目只会更新活动元数据与标题，不会加载、卸载或切换应用的业务内容。

## 启用项目模式

启用标题栏和项目模式，再配置未持久化项目的显示文本：

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

调用 `UseMultiProject()` 时默认启用；省略该调用时则默认禁用。未启用项目模式时，标题选择器只显示并列出应用标题，Flourish 不公开项目标题、项目保存或项目关闭语义。启用项目模式后，选择器显示活动项目名称；活动项目未持久化或没有活动选择时显示未命名项目占位文本，下拉框中包含全部项目以及“新建项目”。

## 项目元数据与持久化

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
                @"C:\Work\Reports.txt"));

        projects.UpsertProject(
            new FlourishProject("samples", "示例"),
            activate: false);
    }
}
```

`StoragePath == null` 表示项目尚未持久化。未命名项目占位文本只用于显示；不要通过比较项目名称与占位文本来判断持久化状态。项目名称不要求唯一，占位文本也可以修改或本地化。

Flourish 从 `projects.json` 加载有序目录与活动项目 ID。该文件位于 `IAppSettingsStore.FilePath` 所在目录，通常与基础 `appsettings.json` 相邻。每次通过 `IProjectService` 修改目录时，Flourish 都会原子重写该文件。写入失败时会回滚内存变更，并且不会发布变更事件。

目录持久化属于 `IProjectService`；即使应用替换 `IProjectBehavior`，该行为也会继续生效。目录只保存元数据；`IProjectService` 不会读取或写入项目所表示的路径。

持久化目录中没有项目时，Flourish 会创建并激活一个未持久化项目，并使用配置的占位文本显示它。

## 运行时目录操作

`IProjectService.Current` 返回不可变的 `FlourishProjectSnapshot`，其中包含有序项目、活动项目、项目模式状态与版本号。

| 操作 | 行为 |
| --- | --- |
| `AddProject(project, activate)` | 添加唯一的项目元数据，并可将其设为活动项目。 |
| `UpsertProject(project, activate)` | 按 ID 添加或替换项目元数据。 |
| `SetProjectMetadata(id, name, storagePath)` | 修改现有项目的名称与可选路径。 |
| `SetActiveProject(id)` | 只修改活动 Shell 标识；传入 `null` 可清除选择。 |
| `RemoveProject(id)` | 只移除目录项；移除活动项目时会清除选择。 |
| `TryGetProject(id, out project)` | 查询一个已注册项目。 |
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

请求的操作可以继续时，每个方法返回 `true`；操作被取消或无法完成时返回 `false`。替换实现负责自身的对话框和项目文件生命周期，并应使用 `IProjectService` 发布元数据与活动选择变更；这些目录变更仍由 Flourish 原子写入 `projects.json`。

## 相关功能

- [标题栏](configure-title-bar.md)说明应用标识、Logo 详情与项目下拉框行为。
- [运行时 API](runtime-apis.md)汇总完整的运行时服务。
- [依赖注入](configure-services.md)说明如何替换 `IProjectBehavior`。
- [应用数据](configure-data.md)说明项目目录所使用的共享 appsettings 位置。
