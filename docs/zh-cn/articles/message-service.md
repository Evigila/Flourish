---
title: 消息服务
description: 使用 Flourish 样式显示标准或自定义选项的模态消息。
---

# 消息服务

Flourish 会在应用服务提供程序中注册 `IMessageService`。当视图模型、命令处理程序或应用服务需要显示符合 Flourish 窗口风格的模态消息时，可以直接注入并使用它。

## 标准消息

标准重载复用 WPF `MessageBox` 的按钮、图标、选项和返回值枚举。

```csharp
if (messages.Show(
        "关闭当前工作区？",
        "关闭",
        MessageBoxButton.YesNo,
        MessageBoxImage.Question,
        MessageBoxResult.No) == MessageBoxResult.Yes)
{
    CloseWorkspace();
}
```

`MessageBoxButton.YesNo` 先显示否定操作，再显示肯定操作，因此肯定操作位于右侧。`MessageBoxButton.YesNoCancel` 依次显示取消、否定和肯定操作。标准按钮标签使用[应用数据](configure-data.md)中选择的语言。如果没有显式提供默认返回值，这两类标准按钮仍会以 `Yes` 作为默认返回值。

## 自定义选项

当返回值不是标准 `MessageBoxResult` 时，可以使用自定义选项重载。该方法会返回被选中的 `FlourishMessageOption`；如果对话框被关闭且没有配置取消选项，则返回 `null`。

```csharp
var selected = messages.Show(
    "导入目标中已经存在匹配文件。",
    "导入",
    [
        new FlourishMessageOption("skip", "跳过") { IsCancel = true },
        new FlourishMessageOption("replace", "替换")
        {
            IsDefault = true,
            IsPrimary = true,
        },
    ],
    MessageBoxImage.Question);

if (selected?.Id == "replace")
{
    ReplaceFiles();
}
```

选项会按照传入顺序从左到右显示，最后一个选项位于弹窗底部右侧。`IsDefault` 控制 Enter 键，`IsCancel` 控制 Esc 和标题栏关闭按钮，`IsPrimary` 使用强调按钮样式。每个自定义选项都必须提供唯一且非空的 `Id`，并提供非空的 `Text`。

消息文本、标题和 `FlourishMessageOption.Text` 均由应用提供，不会自动翻译。

## 所属窗口

标准重载和自定义重载都提供可指定 owner 的形式。当当前活动窗口不是期望的对话框所属窗口时，可以使用该重载。

```csharp
var selected = messages.Show(
    owner,
    "将更改应用到所有打开的项目？",
    "应用",
    [
        new FlourishMessageOption("current", "仅当前项") { IsCancel = true },
        new FlourishMessageOption("all", "所有项目")
        {
            IsDefault = true,
            IsPrimary = true,
        },
    ]);
```
