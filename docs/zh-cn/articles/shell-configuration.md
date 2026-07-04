---
title: Shell 配置
description: 配置 Flourish 应用 Shell。
---

# Shell 配置

Shell 配置通过 `IFlourishShellBuilder` 完成。它把窗口和 Shell 的高层配置集中在一个入口中：

- 标题栏行为
- 导航面板行为
- 窗口尺寸和位置
- 全局字体配置
- 材质效果选择
- 动态工具栏可见性
- 动效配置

```csharp
builder.ConfigureShell((_, shell) =>
{
    shell
        .UseTitlebar((_, titlebar) =>
        {
            titlebar
                .ShowLogo()
                .ShowTitle()
                .ShowSubTitle()
                .ShowSearch()
                .ShowBreadcrumb()
                .ShowNavToggle()
                .SetTitle("Gallery")
                .SetSubtitle("Flourish sample");
        })
        .UseNavigationPanel((_, nav) =>
        {
            nav.SetInitiallyOpen().SetTitle("Navigation");
        })
        .SetWindowProperty((_, window) =>
        {
            window.SetWindowSize().SetWindowMinSize().SetWindowPosition();
        })
        .UseMaterialEffect()
        .UseMotion()
        .UseDynamicToolbar();
});
```

建议始终通过 builder 接口配置 Shell 行为，而不是直接构造 Shell 服务。这样公开契约保持稳定，内部实现仍然可以继续演进。
