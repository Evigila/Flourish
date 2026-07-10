---
title: 动态工具栏
description: 配置按页面变化的工具栏项，并连接到命令解析器。
---

# 动态工具栏

动态工具栏是 Shell 中会随当前页面变化的命令区域，适合放置打开、保存、导入、刷新或页面专属操作。

配置分两步：

1. 在 [Shell 配置](shell-configuration.md)中启用工具栏区域。
2. 使用 `ConfigureDynamicToolbar` 注册页面对应的工具栏项。

## 启用工具栏区域

```csharp
builder.ConfigureShell(shell =>
{
    shell.UseDynamicToolbar();
});
```

即使已经注册工具栏项，`UseDynamicToolbar(false)` 也会让该区域保持禁用。

> [!NOTE]
> 启用动态工具栏只会创建 Shell 区域。为页面注册匹配的工具栏项后，该区域才会显示按钮。

## 为页面注册工具栏项

页面类型在编译期已知时，使用 `IFlourishDynamicToolbarBuilder.CreateToolbarItems<TPage>`。

```csharp
builder.ConfigureDynamicToolbar(toolbar =>
{
    toolbar.CreateToolbarItems<HomePage>(
        new FlourishToolbarItem("打开", "\uE8E5", "home.open"),
        new FlourishToolbarItem("保存", "\uE74E", "home.save"));
});
```

如果页面类型来自动态发现，可以使用 `Type` 重载。

```csharp
toolbar.CreateToolbarItems(
    typeof(ReportsPage),
    new FlourishToolbarItem("导出", "\uE898", "reports.export"));
```

## 控制图标显示

带 `icon: false` 的重载可以创建纯文本工具栏项。

```csharp
toolbar.CreateToolbarItems<EditorPage>(
    icon: false,
    new FlourishToolbarItem("预览", "\uE8A7", "editor.preview"));
```

## 工具栏项字段

`FlourishToolbarItem` 包含三个值：

| 值 | 作用 |
| --- | --- |
| `DisplayName` | 显示在工具栏上的文字。 |
| `IconGlyph` | 启用图标显示时使用的图标字形。 |
| `CommandKey` | 可选的命令键，会传递给 `ICommandParser`。 |

命令键应使用稳定、带命名空间的名称，例如 `reports.export` 或 `editor.preview`。显示文本本地化时，命令键仍保持不变。

## 处理命令

`CommandKey` 会发送给已注册的 `ICommandParser`。在[依赖注入](configure-services.md)配置中注册解析器：

```csharp
services.AddSingleton<ICommandParser, AppCommandParser>();
```

解析器如何识别命令以及多个解析器之间的处理顺序，请参阅[命令解析器](command-parser.md)。[自定义 Shell 内容](configure-custom-handler.md)也可以让标题栏和状态栏命令复用同一组命令键。
