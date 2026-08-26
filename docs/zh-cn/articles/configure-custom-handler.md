---
title: 自定义 Shell 内容
description: 将应用提供的 WPF 元素或命令插入 Flourish Shell 的预定义区域。
---

# 自定义 Shell 内容

Flourish Shell 为标题栏、Logo 信息视图、导航栏、动态工具栏、内容区和状态栏提供预定义扩展区域。使用 `ConfigureContent` 可以将应用提供的 WPF 元素或命令放入这些区域。

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
                FlourishRegion.FooterEnd,
                "帮助",
                "\uE946",
                "cmd_help_open");
    });
```

## 所属区域前置条件

自定义内容不会自动启用所属 Shell 功能。请在 `ConfigureTitleBar`、`ConfigureNavigation`、`ConfigureToolbar` 或 `ConfigureStatusBar` 中调用 `SetEnabled()`，启用该区域所属的功能。`SetProfileContent` 还需要在[标题栏](configure-title-bar.md)中调用 `SetProfile()`。[Shell 配置](shell-configuration.md)说明按功能划分的 Builder 与 Service 模型。

`FlourishRegion.TitleBarApplicationInfo` 区域会渲染为 Logo 信息视图的 Body。添加此内容前应通过 `SetLogo()` 配置 Logo。Body 完全由应用定义，可以显示动态详情，但不会参与项目新建、保存、激活、删除或关闭处理。这些 Shell 入口由 `IProjectBehavior` 协调；参见[项目](projects.md)。

## 元素工厂

元素工厂接收 `IServiceProvider`，可按需从依赖注入容器解析应用服务；即使元素不依赖服务，也使用同一种工厂形式。工厂应在 Shell 请求内容时创建尚未拥有 WPF 父级的元素。

```csharp
builder.ConfigureContent(custom =>
{
    custom
        .AddRegionContent(
            FlourishRegion.TitleBarEnd,
            services => new SyncStatusView(
                services.GetRequiredService<SyncService>()))
        .AddRegionContent(
            FlourishRegion.TitleBarApplicationInfo,
            services => new ApplicationDetailsView(
                services.GetRequiredService<ApplicationDetailsService>()));
});
```

自定义元素的内容、绑定和可访问性语义由应用定义。

## 命令与回调

页脚命令辅助方法必须显式选择 `FlourishRegion.FooterStart` 或 `FlourishRegion.FooterEnd`。命令辅助方法通过 `ICommandDispatcher` 调度稳定的命令键；回调辅助方法直接执行提供的局部行为。显示文本可以本地化，命令键则保持不变。

## 相关功能

- [标题栏](configure-title-bar.md)控制内置标题栏内容和行为。
- [项目](projects.md)说明持久化目录与可替换生命周期行为。
- [动态工具栏](dynamic-toolbar.md) 配置随当前页面变化的工具栏命令。
- [状态栏](status-bar.md) 配置后台任务指示器、自定义状态项和系统状态入口。
- [命令调度](commands.md)处理命令键。
- [依赖注入](configure-services.md)为自定义内容和命令处理程序提供应用服务。
