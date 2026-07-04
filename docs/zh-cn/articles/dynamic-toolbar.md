---
title: 动态工具栏
description: 配置与页面关联的工具栏项。
---

# 动态工具栏

动态工具栏通过 `IFlourishDynamicToolbarBuilder` 配置。工具栏项可以按页面类型注册，Shell 会在导航切换时替换对应命令。

```csharp
builder.ConfigureDynamicToolbar((_, toolbar) =>
{
    toolbar.CreateToolbarItems<HomePage>(
        new FlourishToolbarItem("Open", "\uE8E5", "home.open"),
        new FlourishToolbarItem("Save", "\uE74E", "home.save"));

    toolbar.CreateToolbarItems<GalleryPage>(
        new FlourishToolbarItem("Open", "\uE8E5", "gallery.open"),
        new FlourishToolbarItem("Save", "\uE74E", "gallery.save"),
        new FlourishToolbarItem("Import", "\uE898", "gallery.import"));
});
```

工具栏命令键由注册到依赖注入中的 `ICommandParser` 实现处理。
