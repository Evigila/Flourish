---
title: Getting started
description: Build a basic Flourish application.
---

# Getting started

Create a default Flourish builder, register application services and pages, configure the shell, then build and start the runtime.

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

The `Gallery` project in this repository is the living sample for the current public API. Use it as the first reference when adding new scenarios to the guide.
