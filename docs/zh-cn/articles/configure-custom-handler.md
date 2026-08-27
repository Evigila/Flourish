---
title: 自定义 Shell 内容
description: 将应用提供的 WPF 元素或命令插入 Flourish Shell 的预定义区域。
---

# 自定义 Shell 内容

使用 `ConfigureContent` 将应用提供的 WPF 元素或命令放入 Shell 的预定义区域。

## 添加自定义内容与命令

```csharp
builder
    .ConfigureTitleBar(titleBar => titleBar.SetEnabled().SetProfile())
    .ConfigureStatusBar(statusBar => statusBar.SetEnabled())
    .ConfigureContent(custom =>
    {
        custom
            .SetProfileContent(_ => new Button { Content = "Foo Bar" })
            .AddTitleBarAction("同步", "\uE895", "cmd_sync_run")
            .AddFooterCommand(
                ShellRegion.FooterEnd,
                "帮助",
                "\uE946",
                "cmd_help_open");
    });
```

## 所属区域前置条件

自定义内容不会自动启用 Shell 功能；仍须在对应 `ConfigureTitleBar`、`ConfigureNavigation`、`ConfigureToolbar` 或 `ConfigureStatusBar` 中调用 `SetEnabled()`。`SetProfileContent` 还要求[标题栏](configure-title-bar.md)调用 `SetProfile()`；Builder 与 Service 模型见 [Shell 配置](shell-configuration.md)。

`ShellRegion.TitleBarApplicationInfo` 渲染为 Logo 信息视图的 Body，使用前须通过 `SetLogo()` 配置 Logo。应用可显示动态详情，但项目新建、保存、激活、删除和关闭仍由 `IProjectBehavior` 协调；见[项目](projects.md)。

## 元素工厂

元素工厂接收 `IServiceProvider`，可按需从依赖注入容器解析应用服务；即使元素不依赖服务，也使用同一种工厂形式。工厂应在 Shell 请求内容时创建尚未拥有 WPF 父级的元素。

```csharp
builder.ConfigureContent(custom =>
{
    custom
        .AddRegionContent(
            ShellRegion.TitleBarEnd,
            services => new SyncStatusView(
                services.GetRequiredService<SyncService>()))
        .AddRegionContent(
            ShellRegion.TitleBarApplicationInfo,
            services => new ApplicationDetailsView(
                services.GetRequiredService<ApplicationDetailsService>()));
});
```

自定义元素的内容、绑定和可访问性语义由应用定义。

## 命令与回调

页脚命令辅助方法必须显式选择 `ShellRegion.FooterStart` 或 `ShellRegion.FooterEnd`。命令辅助方法通过 `ICommandDispatcher` 调度稳定的命令键；回调辅助方法直接执行提供的局部行为。显示文本可以本地化，命令键则保持不变。

## 相关功能

- [标题栏](configure-title-bar.md)
- [项目](projects.md)
- [动态工具栏](dynamic-toolbar.md)
- [状态栏](status-bar.md)
- [命令调度](commands.md)
- [依赖注入](configure-services.md)
