---
title: 导航
description: 注册并导航到 Flourish 页面。
---

# 导航

Flourish 将页面注册和可见导航模型拆分开。在服务配置中使用 `AddNavigable` 注册 WPF 页面，通过 `UseNavigation` 启用导航区域，再通过 `ConfigureNavigation` 配置导航栏展示参数和可见导航项。

## 注册页面

`AddNavigable` 会把 `Page` 类型注册到依赖注入，并记录页面导航项使用的元数据：显示名称、图标字形和缓存模式。它不会单独把页面显示到导航栏中。

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

页面类型必须派生自 `System.Windows.Controls.Page`。这里设置的显示名称和图标会被 `AddNavigableViewItem` 复用，因此 ViewItem 不会再次要求传入这些值。

```csharp
services.AddNavigable<GalleryPage>("图库", "\uE91B");
services.AddNavigable<EditorPage>("编辑", "\uE70F", cacheMode: FlourishPageCacheMode.Disabled);
```

需要在离开页面后保留局部状态时，使用 `FlourishPageCacheMode.Enabled`。希望每次导航都重新创建页面时，使用 `Disabled`。

## 配置分组

使用 `ConfigureNavigation` 构建可见的导航模型。`SetGroup` 创建可滚动的分组，`AddNavigableViewItem<TPage>` 将已注册页面放入该分组。

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
        .SetPanelWidth(openWidth: 260, closedWidth: 48, maxWidth: 480, minWidth: 180)
        .SetGroup("导航", groupId: 0, group =>
        {
            group.AddNavigableViewItem<HomePage>(isInitial: true);
            group.AddNavigableViewItem<GalleryPage>();
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
- 0 号组可以省略 `displayName`。当 0 号组没有名称时，Flourish 不会在导航栏顶部为标题预留空间。
- 非 0 号组必须提供 `displayName`。
- 分组之间会比普通导航项拥有更大的间距。

```csharp
nav.SetGroup(groupId: 0, group =>
{
    group.AddNavigableViewItem<HomePage>(isInitial: true);
});

nav.SetGroup("管理", groupId: 10, group =>
{
    group.AddNavigableViewItem<SettingsPage>();
});
```

如果启用了导航栏展示，但没有配置任何分组或固定项，Flourish 会回退到旧的扁平模式：把所有已注册页面组成一个列表。

## 调整导航栏宽度

使用 `SetPanelWidth` 可以配置导航栏展开宽度、折叠宽度，以及拖拽调整时的宽度约束。

```csharp
nav.SetPanelWidth(openWidth: 260, closedWidth: 48, maxWidth: 480, minWidth: 180);
```

默认展开宽度为 `220`，折叠宽度为 `48`，可调整范围为 `160` 到 `420`。用户可以通过悬浮时显示的 splitter 调整展开状态下的导航栏宽度；它使用预览模式，因此 Flourish 会在拖拽结束后再刷新布局。

## 添加命令项

`AddNavigableItem` 添加的是按钮类型导航项。它不会跳转页面，而是把 `commandKey` 发送给已注册的 `ICommandParser`。

```csharp
nav.SetGroup("按钮", groupId: 2, group =>
{
    group.AddNavigableItem("Hello", "demo.hello", iconGlyph: "\uE8F2");
    group.AddNavigableItem("World", "demo.world", iconGlyph: "\uE774");
});
```

命令项只保留 Hover 样式。命令触发后，Flourish 会恢复当前页面选中状态，并清除命令项焦点。

```csharp
internal sealed class AppCommandParser(IMessageService messages) : ICommandParser
{
    public bool TryParse(string commandKey)
    {
        return commandKey switch
        {
            "demo.hello" => Show("Hello"),
            "demo.world" => Show("World"),
            _ => false
        };
    }

    private bool Show(string text)
    {
        messages.Show(text, "导航", MessageBoxButton.OK, MessageBoxImage.Information);
        return true;
    }
}
```

## 添加固定项

固定项显示在导航栏底部区域，不受上半部分滚动视角影响，适合放置设置、关于、用户资料或其他持久操作。

```csharp
builder.ConfigureNavigation(navigation =>
{
    navigation.SetGroup("导航", groupId: 0, group =>
    {
        group.AddNavigableViewItem<HomePage>(isInitial: true);
        group.AddNavigableViewItem<GalleryPage>();
    });

    navigation.AddFixedNavigableViewItem<SettingsPage>();
    navigation.AddFixedNavigableItem("关于", "app.about", iconGlyph: "\uE946");
});
```

固定页面项同样要求页面已经通过 `AddNavigable` 注册。固定命令项与分组内命令项一样，通过 `ICommandParser` 执行行为。

## 构建一层树

导航项默认是扁平结构。要构建一层父子关系，可以在 `AddNavigableViewItem` 和 `AddNavigableItem` 上设置 `parentId` 或 `childId`。

```csharp
nav.SetGroup("树", groupId: 3, group =>
{
    group.AddNavigableViewItem<TreeParentPage>(parentId: 1);
    group.AddNavigableItem("Button1", "tree.button1", childId: 1, iconGlyph: "\uE8B7");
    group.AddNavigableItem("Button2", "tree.button2", childId: 1, iconGlyph: "\uE8B7");

    group.AddNavigableItem("页面", null, parentId: 2, iconGlyph: "\uE8A5");
    group.AddNavigableViewItem<Page1>(childId: 2);
    group.AddNavigableViewItem<Page2>(childId: 2);
});
```

树规则：

- `parentId` 和 `childId` 默认都是 0，表示该项不参与父子系统。
- `parentId` 和 `childId` 不能同时为非 0。
- 同一个分组或固定项范围内，`parentId` 必须唯一。
- 子项会跟随 `childId` 对应的 `parentId`。
- Flourish 当前只支持一层可见子级。

> [!CAUTION]
> 树 ID 只在当前分组或固定项区域内生效。同一范围内复用 `parentId`，或让 `childId` 指向不存在的父节点，都会在构建阶段失败。

页面项可以作为父节点。点击页面父节点时，会跳转到该页面并展开或折叠子项。命令项也可以作为父节点，但命令父节点只负责展开或折叠子项，不会执行 `commandKey`；因此推荐为命令父节点传入 `null`。

当选中页面子项时，Flourish 会自动展开父节点，并把父节点名称标记为激活样式。导航栏折叠时，会先隐藏所有子项，避免折叠后的图标位置偏移。折叠状态下点击父节点会先打开整个导航栏；页面父节点会立刻被选中并导航到对应页面，命令父节点仍然只负责显示或折叠子项。

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

常见校验错误包括：重复的分组 ID、非 0 分组没有名称、同一页面被加入多个导航位置、ViewItem 页面未通过 `AddNavigable` 注册、同一范围内 `parentId` 重复，以及子项的 `childId` 找不到对应父节点。

## 从代码导航

运行时导航可以从依赖注入中获取 `INavigationService`，再按页面类型跳转：

```csharp
public sealed class HomeViewModel(INavigationService navigation)
{
    public void OpenSettings()
    {
        navigation.Navigate<SettingsPage>();
    }
}
```
