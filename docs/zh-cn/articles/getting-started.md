---
title: 快速开始
description: 构建一个基础的 Flourish 应用。
---

# 快速开始

创建默认的 Flourish builder，注册应用服务和页面，配置 Shell，然后构建并启动运行时。

```csharp
using AcksheedSys.Flourish.Abstract;
using Microsoft.Extensions.DependencyInjection;

var flourish = FlourishBuilder
    .CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
    {
        services.AddSingleton<App>();
        services.AddNavigable<HomePage>("Home", "\uE80F", isInitial: true);
        services.AddNavigable<SettingsPage>("Settings", "\uE713");
    })
    .ConfigureShell((_, shell) =>
    {
        shell
            .UseTitlebar((_, titlebar) =>
            {
                titlebar
                    .ShowTitle()
                    .ShowSearch()
                    .ShowBreadcrumb()
                    .SetTitle("Gallery")
                    .SetSubtitle("Flourish sample");
            })
            .UseNavigationPanel((_, nav) =>
            {
                nav.SetInitiallyOpen().SetTitle("Navigation");
            })
            .UseDynamicToolbar()
            .UseMotion()
            .UseMaterialEffect()
            .SetGlobalFont("Microsoft YaHei");
    })
    .Build();

flourish.Start();
var app = flourish.GetRequiredService<App>();
app.Run();
```

仓库中的 `Gallery` 项目是当前公开 API 的可运行示例。新增场景文档时，优先从这个示例项目提炼代码片段。
