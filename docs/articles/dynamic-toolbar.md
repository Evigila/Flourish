---
title: Dynamic toolbar
description: Configure page-specific toolbar items.
---

# Dynamic toolbar

Dynamic toolbar items are configured with `IFlourishDynamicToolbarBuilder`. Items can be registered per page type, allowing the shell to swap commands as navigation changes.

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

Handle toolbar command keys through an `ICommandParser` implementation registered in dependency injection.
