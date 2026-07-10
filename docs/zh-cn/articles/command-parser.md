---
title: 命令解析器
description: 处理 Flourish UI 区域触发的命令键。
---

# 命令解析器

`ICommandParser` 是 Flourish UI 区域触发命令键时的公开扩展点，最常见来源是动态工具栏项和按钮类型导航项。UI 项会保存一个命令键；用户触发该项时，Flourish 会询问已注册的解析器是否能处理该命令。

## 注册解析器

解析器实现放在[依赖注入](configure-services.md)配置中注册。

```csharp
builder.ConfigureServices((_, services) =>
{
    services.AddSingleton<ICommandParser, AppCommandParser>();
});
```

可以注册多个解析器。某个解析器处理成功时返回 `true`；如果它不认识该命令键，则返回 `false`。

## 实现 TryParse

```csharp
internal sealed class AppCommandParser(IMessageService messages) : ICommandParser
{
    public bool TryParse(string commandKey)
    {
        return commandKey switch
        {
            "home.open" => OpenHome(),
            "home.save" => SaveHome(),
            "reports.export" => ExportReports(),
            _ => false
        };
    }

    private bool OpenHome()
    {
        messages.Show(
            "从首页打开",
            "首页",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
        return true;
    }

    private static bool SaveHome()
    {
        return true;
    }

    private static bool ExportReports()
    {
        return true;
    }
}
```

`TryParse` 应保持快速、明确。不要用显示文本路由命令，应使用稳定的命令键。

> [!CAUTION]
> `TryParse` 在触发命令的 UI 线程上同步调用。耗时工作应交给应用服务或异步流程处理，不应阻塞解析器。

## 连接工具栏项

```csharp
toolbar.CreateToolbarItems<ReportsPage>(
    new FlourishToolbarItem("导出", "\uE898", "reports.export"));
```

构造函数第三个参数就是命令键。它是可选的，但需要执行动作的工具栏项应提供命令键。工具栏项通过[动态工具栏](dynamic-toolbar.md)配置注册。

## 连接导航命令项

按钮类型导航项使用同一条解析路径。可以在[导航](navigation.md)配置的分组内使用 `AddNavigableItem` 添加，也可以在底部固定区域通过 `AddFixedNavigableItem` 添加。

```csharp
builder.ConfigureNavigation(navigation =>
{
    navigation.SetGroup("命令", groupId: 1, group =>
    {
        group.AddNavigableItem("刷新", "reports.refresh", iconGlyph: "\uE72C");
    });

    navigation.AddFixedNavigableItem("关于", "app.about", iconGlyph: "\uE946");
});
```

如果命令项同时是父节点，点击它只会展开或折叠子项，不会执行命令键。

## 在解析器中使用服务

解析器由 DI 解析，因此可以依赖应用服务。Flourish 也会注册 `IMessageService`，用于显示符合 Flourish 样式的模态消息，并复用 WPF `MessageBox` 的按钮、图标和返回值枚举。它也支持自定义选项；参见 [消息服务](message-service.md)。通过[自定义 Shell 内容](configure-custom-handler.md)注册的标题栏和状态栏命令也使用同一条解析路径。

```csharp
internal sealed class ReportsCommandParser(ReportService reports) : ICommandParser
{
    public bool TryParse(string commandKey)
    {
        if (commandKey != "reports.export")
        {
            return false;
        }

        reports.Export();
        return true;
    }
}
```

按普通方式注册依赖：

```csharp
services.AddSingleton<ReportService>();
services.AddSingleton<ICommandParser, ReportsCommandParser>();
```

## 命令键约定

- 使用小写点分名称，例如 `reports.export`。
- 用功能或页面作为前缀。
- 即使显示文本本地化，命令键也应保持稳定。
- 对未知命令返回 `false`，不要直接抛异常。
