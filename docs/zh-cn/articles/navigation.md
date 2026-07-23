---
title: 导航
description: 注册并导航到 Flourish 页面。
---

# 导航

在[依赖注入](configure-services.md)配置中使用 `AddNavigable` 注册 WPF 页面，通过 [Shell 配置](shell-configuration.md)启用导航区域，再使用 `ConfigureNavigation` 把页面和命令项放入明确的位置。

## 注册页面

`AddNavigable` 会把 `Page` 类型注册到依赖注入，并记录导航使用的显示名称、图标字形和缓存模式。注册后页面可供导航使用；若要在面板中显示它，还需添加对应的 ViewItem。

```csharp
builder.ConfigureServices((_, services) =>
{
    services.AddNavigable<HomePage>(
        displayName: "首页",
        iconGlyph: "\uE80F",
        cacheMode: FlourishPageCacheMode.Enabled);

    services.AddNavigable<SettingsPage>(
        displayName: "设置",
        iconGlyph: "\uE713",
        cacheMode: FlourishPageCacheMode.Enabled);
});
```

页面类型必须派生自 `System.Windows.Controls.Page`。Flourish 从简单类名生成导航键，并移除一个末尾、区分大小写的 `Page` 后缀：`SettingsPage` 生成 `Settings`，`ReportPagePage` 生成 `ReportPage`，`Page1` 仍生成 `Page1`。显示名称不会影响 key。这里设置的显示名称和图标会被 `AddNavigableViewItem` 复用，因此 ViewItem 不会再次要求传入这些值。

标准 Shell 使用随主题变化的主色前景呈现导航图标，同时让标签保持中性色，从而在浅色与深色主题中提供一致的视觉强调。

```csharp
services.AddNavigable<ReportsPage>("报表", "\uE9D2");
services.AddNavigable<EditorPage>(
    "编辑",
    "\uE70F",
    cacheMode: FlourishPageCacheMode.Disabled);
```

需要在离开页面后保留局部状态时，使用 `FlourishPageCacheMode.Enabled`。希望离开后再次进入页面时重新创建实例，则使用 `Disabled`。

## 配置分组

使用 `ConfigureNavigation` 定义可见导航模型。`SetGroup` 创建可滚动的分组，`AddNavigableViewItem<TPage>` 将已注册页面放入该分组。

```csharp
builder.ConfigureShell(shell =>
{
    shell.UseNavigation();
})
.ConfigureNavigation(navigation =>
{
    navigation
        .SetDirection(NavigationPanelDirection.Left)
        .SetInitiallyOpen()
        .SetPanelWidth(openWidth: 260, closedWidth: 64, maxWidth: 480, minWidth: 180)
        .SetGroup("导航", groupId: 0, group =>
        {
            group.AddNavigableViewItem<HomePage>(isInitial: true);
            group.AddNavigableViewItem<ReportsPage>();
        });

    navigation.SetGroup("工具", groupId: 1, group =>
    {
        group.AddNavigableViewItem<EditorPage>();
    });
});
```

分组规则：

- `groupId` 决定显示顺序，数值越小越靠前。
- `groupId` 不允许重复，重复会在构建阶段报错。
- 0 号组可以省略 `displayName`。
- 非 0 号组必须提供 `displayName`。

```csharp
nav.SetGroup(groupId: 0, configureGroup: group =>
{
    group.AddNavigableViewItem<HomePage>(isInitial: true);
});

nav.SetGroup("管理", groupId: 10, group =>
{
    group.AddNavigableViewItem<SettingsPage>();
});
```

## 调整导航栏宽度

使用 `SetPanelWidth` 可以配置导航栏展开宽度、折叠宽度，以及拖拽调整时的宽度约束。

```csharp
nav.SetPanelWidth(openWidth: 260, closedWidth: 64, maxWidth: 480, minWidth: 180);
```

默认展开宽度为 `220`，折叠宽度为 `64`。将 `closedWidth` 设为 `0` 会完全隐藏折叠面板；否则其值不得小于 `64`。可调整范围默认为 `160` 到 `420`。用户调整大小时，会在该范围内更新展开宽度。

## 添加命令项

`AddNavigableItem` 添加的是按钮类型导航项。它不会跳转页面，而是通过 `ICommandDispatcher` 调度 `commandKey`。

```csharp
nav.SetGroup("命令", groupId: 2, group =>
{
    group.AddNavigableItem("刷新", "\uE72C", "reports.refresh");
    group.AddNavigableItem("导出", "\uE898", "reports.export");
});
```

命令项不会保持选中。命令触发后，导航栏会恢复当前页面的选中状态。处理程序的注册与实现方式参见[命令调度](commands.md)。

## 添加固定项

固定项显示在导航栏底部区域，不受上半部分滚动视角影响，适合放置设置、关于、用户资料或其他持久操作。

```csharp
builder.ConfigureNavigation(navigation =>
{
    navigation.SetGroup("导航", groupId: 0, group =>
    {
        group.AddNavigableViewItem<HomePage>(isInitial: true);
        group.AddNavigableViewItem<ReportsPage>();
    });

    navigation.AddFixedNavigableViewItem<SettingsPage>();
    navigation.AddFixedNavigableItem("帮助", "\uE946", "help.open");
});
```

固定页面项同样要求页面已经通过 `AddNavigable` 注册。固定命令项与分组内命令项使用相同的命令调度路径。

## 构建一层树

导航项默认是扁平结构。要构建一层父子关系，可以在 `AddNavigableViewItem` 和 `AddNavigableItem` 上设置 `parentId` 或 `childId`。

```csharp
nav.SetGroup("树", groupId: 3, group =>
{
    group.AddNavigableViewItem<TreeParentPage>(parentId: 1);
    group.AddNavigableItem("Button1", "\uE8B7", "tree.button1", childId: 1);
    group.AddNavigableItem("Button2", "\uE8B7", "tree.button2", childId: 1);

    group.AddNavigableItem("页面", "\uE8A5", null, parentId: 2);
    group.AddNavigableViewItem<Page1>(childId: 2);
    group.AddNavigableViewItem<Page2>(childId: 2);
});
```

树规则：

- `parentId` 和 `childId` 默认都是 0，表示该项不参与父子系统。
- `parentId` 和 `childId` 不能同时为非 0。
- 同一个分组或固定项范围内，`parentId` 必须唯一。
- 子项会跟随 `childId` 对应的 `parentId`。
- 导航树支持一层可见子级。

> [!CAUTION]
> 树 ID 只在当前分组或固定项区域内生效。同一范围内复用 `parentId`，或让 `childId` 指向不存在的父节点，都会在构建阶段失败。

页面项可以作为父节点。点击页面父节点时，会跳转到该页面并展开或折叠子项。命令项也可以作为父节点，但命令父节点只负责展开或折叠子项，不会执行 `commandKey`；不需要命令键时传入 `null`。

选中页面子项时，Flourish 会展开对应父节点，并标记父节点名称。导航栏折叠时不显示子项；在折叠状态下点击父节点会先展开导航栏。页面父节点随后导航到对应页面，命令父节点只展开或折叠子项。

## 校验规则

Flourish 会在构建阶段校验导航模型，让错误配置尽早失败。

```csharp
nav.SetGroup("One", groupId: 1, group =>
{
    group.AddNavigableViewItem<HomePage>();
});

nav.SetGroup("Two", groupId: 2, group =>
{
    // 这里会报错，因为 HomePage 已经显示在 group 1 中。
    group.AddNavigableViewItem<HomePage>();
});
```

常见校验错误包括：重复的生成导航键、重复的分组 ID、非 0 分组没有名称、同一页面被加入多个导航位置、ViewItem 页面未通过 `AddNavigable` 注册、同一范围内 `parentId` 重复，以及子项的 `childId` 找不到对应父节点。即使命名空间不同，两个简单类名相同的页面仍会生成相同 key；`Build()` 会拒绝它们，并报告重复 key 和两个页面的完整类型名。

## 从代码导航

运行时导航可以从依赖注入中获取 `INavigationService`，再传入自动生成、区分大小写的字符串 key。ViewModel 因此无需引用 WPF `Page` 类型。

```csharp
public sealed class HomeViewModel(INavigationService navigation)
{
    public void OpenSettings()
    {
        navigation.Navigate("Settings");
    }
}
```

如果 key 未注册，`Navigate` 会抛出 `InvalidOperationException`，消息包含实际传入的 key、生成规则，并提示检查拼写和大小写。重命名 Page 类也会改变生成 key，因此应在同一次修改中更新字符串导航调用。
