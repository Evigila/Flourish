---
title: 命令调度
description: 为 Flourish UI 区域注册异步命令处理程序并调度命令键。
---

# 命令调度

Flourish UI 区域通过 `ICommandDispatcher` 发送稳定的命令键。使用 `ICommandRegistry` 注册处理程序；`Build()` 完成后，可以从 Flourish 服务提供程序获取这两个接口。

## 注册处理程序

`ICommandRegistry.Register` 将命令键与异步处理程序关联，并返回 `ICommandRegistration`。只要处理程序需要保持有效，就应持有该注册；释放注册会移除命令。

```csharp
ICommandRegistry commands = flourish.GetRequiredService<ICommandRegistry>();

ICommandRegistration exportCommand = commands.Register(
    "reports.export",
    async (context, cancellationToken) =>
    {
        await exporter.ExportAsync(context.Parameter, cancellationToken);
        return CommandResult.Handled;
    });
```

处理程序会收到 `CommandContext`，其中包含命令键、可选参数和来源 `CommandSource`。返回 `CommandResult.Handled`、`HandledWith(value)`、`NotHandled`、`Canceled` 或 `Failed(exception)` 来描述结果。

## 定义启动命令映射

命令解析器用于定义与 Host 生命周期一致的命令键和处理程序映射，而无需公开其注册租约。Flourish 在 Host 启动时调用每个解析器，并在 Host 停止时按相反顺序移除映射。

```csharp
internal sealed class ReportCommands(ReportService reports)
    : ICommandParser
{
    public void RegisterCommands(ICommandRegistrar commands)
    {
        commands.Register(
            "reports.refresh",
            async (_, token) =>
            {
                await reports.RefreshAsync(token);
                return CommandResult.Handled;
            });

        commands.Register(
            "reports.export",
            async (context, token) =>
            {
                await reports.ExportAsync(context.Parameter, token);
                return CommandResult.Handled;
            });
    }
}
```

在服务配置中注册解析器及其依赖项。应用无需主动解析该解析器，也无需实现 `IDisposable`：

```csharp
builder.ConfigureServices((_, services) =>
{
    services.AddSingleton<ReportService>();
    services.AddCommandParser<ReportCommands>();
});
```

`ICommandRegistrar.Register` 不返回租约，因为其生命周期由 Host 管理。解析器必须在 `RegisterCommands` 内同步定义映射，且不得保留 registrar。处理程序需要在 Host 持续运行期间动态添加或移除时，直接使用 `ICommandRegistry`。

## 控制可用性

命令可用性取决于应用状态时，向 `Register` 传入谓词。Flourish UI 可以通过 `ICommandDispatcher.CanExecute` 查询该谓词。

```csharp
var saveCommand = commands.Register(
    "editor.save",
    async (_, token) =>
    {
        await editor.SaveAsync(token);
        return CommandResult.Handled;
    },
    _ => editor.HasChanges);
```

谓词依赖的状态变化时，调用 `commands.NotifyCanExecuteChanged("editor.save")`。省略命令键可以通知监听器任意命令都可能发生变化。

## 重复命令键

默认重复策略为 `Reject`。新处理程序需要取代当前注册时，将 `CommandRegistrationOptions.DuplicatePolicy` 设为 `Replace`；多个处理程序需要依次参与调度时设为 `Append`。追加的处理程序先按优先级降序执行，再按注册顺序执行；处理程序返回 `NotHandled` 之外的结果后，调度立即停止。

启动解析器通常应保留 `Reject`。`Replace` 会立即停用已有处理程序，因此后续启动失败时可以移除替代项，但无法重建被替代的处理程序。

## 直接调度

应用代码需要复用 Flourish 控件的命令路径时，使用 `ICommandDispatcher`。

```csharp
CommandResult result = await dispatcher.ExecuteAsync(
    "reports.export",
    selectedReport,
    CommandSource.Application,
    cancellationToken);

if (result.Status == CommandExecutionStatus.Failed)
{
    logger.LogError(result.Exception, "导出失败");
}
```

调度器会把处理程序异常捕获到 `CommandResult`，并通过 `CommandExecutionStatus.Canceled` 报告取消。

## 连接 Flourish 区域

工具栏、导航、标题栏、状态栏、通知和快捷键 API 使用同一套命令键。例如：

```csharp
toolbar.CreateToolbarItems<ReportsPage>(
    new FlourishToolbarItem("导出", "\uE898", "reports.export"));
```

命令项无需了解由哪个服务处理命令键。这样既能独立本地化显示文本，也能在不重建 UI 模型的情况下变更注册。

## 命令键约定

- 使用小写点分名称，例如 `reports.export`。
- 用功能或页面作为前缀。
- 显示文本本地化时保持命令键不变。
- 所属功能被移除时释放对应注册。
