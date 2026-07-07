---
title: 动态工具栏
description: 配置按页面变化的工具栏项，并连接到命令解析器。
---

# 动态工具栏

动态工具栏是 Shell 中会随当前页面变化的命令区域，适合放置打开、保存、导入、刷新或页面专属操作。

配置分两步：

1. 在 Shell 配置中启用工具栏区域。
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
> 启用动态工具栏只会创建 Shell 区域。页面需要通过 `ConfigureDynamicToolbar` 注册匹配的工具栏项后，才会显示按钮。

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
    typeof(GalleryPage),
    new FlourishToolbarItem("导入", "\uE898", "gallery.import"));
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
| `IconGlyph` | 可选的图标字形。 |
| `CommandKey` | 可选的命令键，会传递给 `ICommandParser`。 |

建议使用稳定、带命名空间的命令键，例如 `gallery.import` 或 `editor.preview`。它比显示文本更适合路由，也不会因为 UI 语言变化而改变。

## 处理命令

在 `ConfigureServices` 中注册一个或多个 `ICommandParser` 实现。

```csharp
services.AddSingleton<ICommandParser, AppCommandParser>();
```

```csharp
internal sealed class AppCommandParser : ICommandParser
{
    public bool TryParse(string commandKey)
    {
        return commandKey switch
        {
            "home.open" => OpenHome(),
            "home.save" => SaveHome(),
            "gallery.import" => ImportGallery(),
            _ => false
        };
    }
}
```

识别并处理成功时返回 `true`。返回 `false` 表示当前解析器不处理该命令，允许其他解析器继续尝试。

## 完整示例

```csharp
var flourish = FlourishBuilder
    .CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
    {
        services.AddSingleton<App>();
        services.AddSingleton<ICommandParser, AppCommandParser>();
        services.AddNavigable<HomePage>("首页", "\uE80F");
        services.AddNavigable<GalleryPage>("图库", "\uE91B");
    })
    .ConfigureShell(shell =>
    {
        shell
            .UseNavigation()
            .UseDynamicToolbar();
    })
    .ConfigureNavigation(navigation =>
    {
        navigation
            .SetInitiallyOpen()
            .SetGroup("导航", groupId: 0, group =>
            {
                group.AddNavigableViewItem<HomePage>(isInitial: true);
                group.AddNavigableViewItem<GalleryPage>();
            });
    })
    .ConfigureDynamicToolbar(toolbar =>
    {
        toolbar.CreateToolbarItems<HomePage>(
            new FlourishToolbarItem("打开", "\uE8E5", "home.open"),
            new FlourishToolbarItem("保存", "\uE74E", "home.save"));

        toolbar.CreateToolbarItems<GalleryPage>(
            new FlourishToolbarItem("打开", "\uE8E5", "gallery.open"),
            new FlourishToolbarItem("保存", "\uE74E", "gallery.save"),
            new FlourishToolbarItem("导入", "\uE898", "gallery.import"));
    })
    .Build();
```
