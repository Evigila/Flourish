---
title: ConfigureNavigation
description: 配置导航栏展示和可见导航项。
---

# ConfigureNavigation

`ConfigureNavigation` 配置导航栏展示参数和可见导航模型。导航栏只有在 [`ConfigureShell`](configure-shell.md) 启用 `UseNavigation()` 后才会显示。

```csharp
builder
    .ConfigureShell(shell => shell.UseNavigation())
    .ConfigureNavigation(navigation =>
    {
        navigation
            .SetDirection(NavigationPanelDirection.Left)
            .SetInitiallyOpen()
            .SetPanelWidth(openWidth: 260, closedWidth: 48, maxWidth: 480, minWidth: 180)
            .SetGroup("导航", groupId: 0, group =>
            {
                group.AddNavigableViewItem<HomePage>(isInitial: true);
                group.AddNavigableItem("刷新", "navigation.refresh", iconGlyph: "\uE72C");
            })
            .AddFixedNavigableViewItem<SettingsPage>();
    });
```

## 细节

`SetDirection`、`SetInitiallyOpen`、`SetPanelWidth` 和 `SetTitle` 配置导航栏展示。它们替代了旧的独立 NavigationPanel API。

`SetGroup` 创建可滚动分组。`groupId` 决定显示顺序，并且必须唯一。`0` 号分组可以省略名称，非零分组必须提供名称。

`AddNavigableViewItem<TPage>` 放置通过 [`ConfigureServices`](configure-services.md) 注册的页面。`AddNavigableItem` 创建命令项，并把 command key 发送给 `ICommandParser`。

固定项始终停留在底部区域，适合设置、关于、用户信息或常驻工具命令。

## 相关 API

- [`ConfigureServices`](configure-services.md) 使用 `AddNavigable` 注册页面类型。
- [`命令解析器`](command-parser.md) 处理导航命令项。
- [`导航`](navigation.md) 更深入说明分组、固定项和一级树。
