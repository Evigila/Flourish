---
title: 后台任务
description: 使用元信息、取消、进度、结果和状态栏集成运行有并发上限的异步任务。
---

# 后台任务

Flourish 将 `IBackgroundTaskService` 注册为单例运行时服务，并随 Generic Host 一同启动。应用通过依赖注入解析该服务，使用 `AddTask` 提交工作；需要取消任务或读取最终结果时，应保留返回的 handle。

```csharp
public sealed class ExportViewModel(IBackgroundTaskService backgroundTasks)
{
    public FlourishBackgroundTaskHandle Export()
    {
        var metadata = new FlourishBackgroundTaskMetadata(
            name: "导出报表",
            description: "将当前报表写入磁盘。",
            iconGlyph: "\uE74E");

        return backgroundTasks.AddTask(metadata, async context =>
        {
            for (var step = 1; step <= 10; step++)
            {
                await Task.Delay(200, context.CancellationToken);
                context.ReportProgress(step / 10d);
            }
        });
    }
}
```

`AddTask` 会立即返回，其委托在 WPF UI 线程之外运行。任务委托不能直接访问 WPF 控件；确有需要时，应通过对应的 `Dispatcher` 切回 UI 线程。

## 元信息与状态栏集成

每次提交都必须提供 `FlourishBackgroundTaskMetadata`。`Name` 不可为空，`Description` 和 `IconGlyph` 可省略。Shell 会把这些信息用于提示、任务行、自动化名称和状态图标，因此应在提交任务前填写面向用户的元信息。

存在活动任务时，状态栏左侧会为每个正在运行或正在取消的任务显示一个图标。悬停图标可查看元信息、状态和已报告进度；点击会打开后台任务浮层。所有执行槽都被占用后，后续任务进入等待队列，队列图标会显示等待数量。队列提供取消等待中或运行中任务的操作。

即使应用省略 `UseTips()`，任务状态和队列详情仍然可用。任务和队列按钮支持指针与键盘操作。

即使应用没有配置 `UseStatusBar()`，活动任务也会临时显示状态栏。任务完成、失败或取消后会离开活动列表，其图标也会移除；需要最终结果或已完成任务记录时，应保存返回的 handle 并由应用自行记录。

## 并发与等待队列

`MaxConcurrency` 提供可同时运行的委托数量。超过该数量的任务按提交顺序等待，直到执行槽可用。

`ActiveTasks` 返回等待中、运行中和正在取消任务的不可变快照。活动集合、状态或进度变化时，`TasksChanged` 会发布一份新的不可变列表。该事件可能从非 UI 线程触发；事件处理器若要更新应用 UI，必须切换到 UI dispatcher。

`FlourishBackgroundTaskState` 描述完整生命周期：

| 状态 | 含义 |
| --- | --- |
| `Queued` | 正在等待执行槽。 |
| `Running` | 任务委托正在执行。 |
| `Cancelling` | 已请求取消，运行中的委托正在结束。 |
| `Succeeded` | 已成功完成。 |
| `Canceled` | 已以取消状态结束。 |
| `Failed` | 已失败并捕获异常。 |

`ActiveTasks` 只包含前三种状态；终止状态仍可从 handle 和结果读取。

## ValueTask 委托

两个提交重载分别接收：

- `Func<FlourishBackgroundTaskContext, ValueTask>`：没有返回值的工作。
- `Func<FlourishBackgroundTaskContext, ValueTask<TResult>>`：产生返回值的工作。

异步 lambda 可以直接匹配这些重载。异步 I/O 不需要额外包装在 `Task.Run` 中，应直接等待，并把 `context.CancellationToken` 传给支持取消的 API。

## 取消与 Host 停止

调用 `handle.Cancel()`、`CancelTask(handle.Id)` 或停止 Host 时，`FlourishBackgroundTaskContext.CancellationToken` 会收到取消请求。

- 取消等待任务会直接移出队列，不会调用任务委托。
- 取消运行任务会先进入 `Cancelling` 状态。取消是协作式的，委托应观察 token 并尽快结束。
- 重复取消或取消已终止任务会返回 `false`。
- Host 停止时不再接受新任务，并会取消活动任务、等待活动委托结束；忽略取消的委托可能延迟应用关闭。

与上下文 token 对应的 `OperationCanceledException` 会生成取消结果，其他异常会被捕获为失败结果。

## 进度

使用 `context.ReportProgress(value)` 报告 `0` 到 `1` 之间的有限数值。最新进度可从 `FlourishBackgroundTaskInfo.Progress` 读取；首次报告前为 `null`。超出范围的值会抛出 `ArgumentOutOfRangeException`。

## 返回值与异常

`handle.Completion` 始终正常完成并返回 `FlourishBackgroundTaskResult`。任务委托失败不会使 `Completion` fault；应检查 `Succeeded`、`Canceled`、`Exception` 和最终的 `Info` 快照。

泛型重载还会携带成功返回值：

```csharp
public async Task<int?> CountFilesAsync(IBackgroundTaskService backgroundTasks)
{
    var handle = backgroundTasks.AddTask<int>(
        new FlourishBackgroundTaskMetadata(
            "统计文件",
            "统计所选工作区中的文件数量。",
            "\uE8B7"),
        async context =>
        {
            var files = await fileCatalog.ListAsync(context.CancellationToken);
            context.ReportProgress(1);
            return files.Count;
        });

    FlourishBackgroundTaskResult<int> result = await handle.Completion;
    if (result.Succeeded)
    {
        return result.Value;
    }

    if (result.Exception is not null)
    {
        logger.LogError(result.Exception, "统计文件失败。");
    }

    return null;
}
```

任务取消或失败时，`Value` 为该类型的默认值。任务活动期间，`handle.Snapshot` 提供最新状态；完成后仍可通过它读取终止状态。

## 相关功能

- [状态栏](status-bar.md)说明运行图标、等待队列和系统状态浮层。
- [依赖注入](configure-services.md)说明如何解析应用服务和 `IBackgroundTaskService`。
- [应用数据](configure-data.md)列出内置任务界面的本地化键。
