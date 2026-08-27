---
title: 动态工具栏
description: 配置按页面变化的工具栏项，并连接到命令调度。
---

# 动态工具栏

动态工具栏随当前页面切换，用于打开、保存、导入或刷新等页面命令。

使用 `ConfigureToolbar` 同时启用工具栏区域并注册页面对应的工具栏项。

## 启用工具栏区域

```csharp
builder.ConfigureToolbar(toolbar => toolbar.SetEnabled());
```

即使已经注册工具栏项，`SetEnabled(false)` 也会让该区域保持禁用。

> [!NOTE]
> 启用动态工具栏只会创建 Shell 区域。为页面注册匹配的工具栏项后，该区域才会显示按钮。

## 为页面注册工具栏项

使用 `IToolbarBuilder.Set<TPage>` 将工具栏项与 WPF 页面类型关联。

```csharp
builder.ConfigureToolbar(toolbar =>
{
    toolbar
        .SetEnabled()
        .Set<ReportsPage>(
        new ToolbarItem("刷新", "\uE72C", "cmd_reports_refresh"),
        new ToolbarItem("导出", "\uE898", "cmd_reports_export"));
});
```

## 控制图标显示

带 `iconOnly: false` 的重载可以创建纯文本工具栏项。

```csharp
toolbar.Set<EditorPage>(
    iconOnly: false,
    new ToolbarItem("预览", "\uE8A7", "cmd_editor_preview"));
```

## 工具栏项字段

`ToolbarItem` 包含三个值：

| 值 | 作用 |
| --- | --- |
| `DisplayName` | 显示在工具栏上的文字。 |
| `IconGlyph` | 启用图标显示时使用的图标字形。 |
| `CommandKey` | 可选的命令键，通过 `ICommandDispatcher` 调度。 |

命令键应使用稳定的 `cmd_` 前缀和下划线分段，例如 `cmd_reports_export` 或 `cmd_editor_preview`。显示文本本地化时，命令键仍保持不变。

## 处理命令

从构建完成的运行时解析 `ICommandRegistry`，并注册工具栏项使用的命令键：

```csharp
IRegistration exportCommand = commands.Register(
    "cmd_reports_export",
    async (_, token) =>
    {
        await exporter.ExportAsync(token);
        return CommandResult.Handled;
    });
```

[命令调度](commands.md)说明注册所有权、可用性、重复策略和结果。[自定义 Shell 内容](configure-custom-handler.md)也可以让标题栏和状态栏命令复用同一组命令键。
